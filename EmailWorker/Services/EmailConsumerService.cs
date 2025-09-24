using System.Text;
using System.Text.Json;
using DESAFIO2.Domain;
using EmailWorker.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailWorker.Services;

public class EmailConsumerService : BackgroundService
{
    private readonly RabbitSettings _rabbit;
    private readonly EmailSettings _email;
    private readonly ILogger<EmailConsumerService> _log;

    private IConnection? _conn;
    private IChannel? _ch;

    public EmailConsumerService(
        IOptions<RabbitSettings> rabbit,
        IOptions<EmailSettings> email,
        ILogger<EmailConsumerService> log)
    {
        _rabbit = rabbit.Value;
        _email  = email.Value;
        _log    = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 0) Logs de configuração
        _log.LogInformation("Iniciando EmailWorker. Queue={Queue} Host={Host} Uri={Uri} DryRun={DryRun}",
            _rabbit.Queue, _rabbit.HostName, _rabbit.Uri, _email.DryRun);

        // 1) Conectar ao RabbitMQ com retry/backoff
        var factory = new ConnectionFactory();
        if (!string.IsNullOrWhiteSpace(_rabbit.Uri))
        {
            factory.Uri = new Uri(_rabbit.Uri);
        }
        else
        {
            factory.HostName = _rabbit.HostName;
            factory.UserName = _rabbit.UserName;
            factory.Password = _rabbit.Password;
        }

        var attempt = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                attempt++;
                _conn = await factory.CreateConnectionAsync(cancellationToken: stoppingToken);
                _ch   = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);

                await _ch.QueueDeclareAsync(
                    queue: _rabbit.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                _log.LogInformation("Conectado ao RabbitMQ e fila declarada '{Queue}'.", _rabbit.Queue);
                break; // sucesso
            }
            catch (Exception ex)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempt)));
                _log.LogError(ex, "Falha ao conectar no RabbitMQ (tentativa {Attempt}). Tentando novamente em {Delay}s...",
                    attempt, delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }

        if (_ch is null)
        {
            _log.LogCritical("Não foi possível estabelecer canal com o RabbitMQ. Encerrando worker.");
            return;
        }

        // 2) Consumidor assíncrono
        var consumer = new AsyncEventingBasicConsumer(_ch);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg  = JsonSerializer.Deserialize<EmailFilaMsg>(json);
                if (msg is null) throw new InvalidOperationException("Mensagem inválida (JSON nulo).");

                // 3) Envio de e-mail
                await EnviarEmailAsync(msg.Nome, msg.Email, stoppingToken);

                await _ch.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Erro ao processar mensagem. Nack (requeue=false).");
                await _ch!.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false, // evita loop infinito se a mensagem estiver envenenada
                    cancellationToken: stoppingToken);
            }
        };

        await _ch.BasicConsumeAsync(
            queue: _rabbit.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _log.LogInformation("EmailWorker pronto. Aguardando mensagens…");

        // 4) Mantém o worker vivo até o cancelamento
        try
        {
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
        catch (OperationCanceledException) { /* esperado no shutdown */ }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Parando EmailWorker…");
        if (_ch is not null)  await _ch.DisposeAsync();
        if (_conn is not null) await _conn.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task EnviarEmailAsync(string nome, string email, CancellationToken ct)
    {
        if (_email.DryRun)
        {
            _log.LogWarning("DryRun=TRUE → simulando envio de e-mail para {Email} (não chamando SendGrid).", email);
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SENDGRID_API_KEY não configurada. Defina a variável de ambiente ou ative Email.DryRun=true.");

        var client  = new SendGridClient(apiKey);
        var from    = new EmailAddress(_email.FromAddress, _email.FromName);
        var to      = new EmailAddress(email, nome);
        var subject = "Seu cadastro está em análise";
        var plain   = $"Olá {nome},\n\nO seu cadastro está em análise e em breve você receberá um e-mail com novas atualizações sobre seu cadastro.\n\nAtenciosamente,\nEquipe PATHBIT";

        var msg   = MailHelper.CreateSingleEmail(from, to, subject, plain, null);
        var resp  = await client.SendEmailAsync(msg, ct);
        var body  = await resp.Body.ReadAsStringAsync();

        _log.LogInformation("SendGrid Status: {Status} Body: {Body}", resp.StatusCode, body);
    }
}
