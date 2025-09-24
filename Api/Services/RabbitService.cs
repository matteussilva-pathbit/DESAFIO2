using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace DESAFIO2.Api.Services;

public class RabbitSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Queue    { get; set; } = "cadastro.em.analise.email";
}

public sealed class RabbitService : IDisposable
{
    private readonly IConnection _conn;
    private readonly IChannel _ch;
    private readonly string _queue;

    public RabbitService(IOptions<RabbitSettings> opt)
    {
        var cfg = opt.Value;
        _queue = cfg.Queue;

        var factory = new ConnectionFactory
        {
            HostName = cfg.HostName,
            UserName = cfg.UserName,
            Password = cfg.Password
        };

        // v7: métodos assíncronos. No ctor, bloqueamos de forma segura só 1x.
        _conn = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _ch   = _conn.CreateChannelAsync().GetAwaiter().GetResult();

        // v7: declarações também são async
        _ch.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false, arguments: null)
           .GetAwaiter().GetResult();
    }

    public async Task PublishJsonAsync(string json, CancellationToken ct = default)
    {
        // v7: cria props com 'new BasicProperties()' e publica com BasicPublishAsync
        var props = new BasicProperties();
        var body  = Encoding.UTF8.GetBytes(json);

        await _ch.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queue,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct
        );
    }

    public void Dispose()
    {
        _ch?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _conn?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
