// EmailWorker/Services/EmailConsumerService.cs
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
        var factory = new ConnectionFactory
        {
            HostName = _rabbit.HostName,
            UserName = _rabbit.UserName,
            Password = _rabbit.Password
        };

        // *** IMPORTANTE: use o parâmetro nomeado 'cancellationToken' ***
        _conn = await factory.CreateConnectionAsync(cancellationToken: stoppingToken);
        _ch   = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);

        await _ch.QueueDeclareAsync(
            queue: _rabbit.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_ch);

        // *** IMPORTANTE: o handler Recebe DOIS parâmetros: (_, ea) ***
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg  = JsonSerializer.Deserialize<EmailFilaMsg>(json)!;

                await EnviarEmailAsync(msg.Nome, msg.Email, stoppingToken);

                await _ch!.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha ao processar mensagem");
                await _ch!.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await _ch.BasicConsumeAsync(
            queue: _rabbit.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _log.LogInformation("EmailWorker iniciado. Aguardando mensagens…");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_ch is not null)  await _ch.DisposeAsync();
        if (_conn is not null) await _conn.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task EnviarEmailAsync(string nome, string email, CancellationToken ct)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SENDGRID_API_KEY não configurada.");

        var client  = new SendGridClient(apiKey);
        var from    = new EmailAddress(_email.FromAddress, _email.FromName);
        var to      = new EmailAddress(email, nome);
        var subject = "Seu cadastro está em análise";
        var plain   = $"Olá {nome},\n\nO seu cadastro está em análise e em breve você receberá um e-mail com novas atualizações sobre seu cadastro.\n\nAtenciosamente,\nEquipe PATHBIT";

        var msg  = MailHelper.CreateSingleEmail(from, to, subject, plain, null);
        var resp = await client.SendEmailAsync(msg, ct);
        _log.LogInformation("Email enviado para {Email} - Status: {StatusCode}", email, resp.StatusCode);
    }
}
