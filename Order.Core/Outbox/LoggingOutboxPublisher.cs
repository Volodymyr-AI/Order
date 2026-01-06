using Microsoft.Extensions.Logging;

namespace Order.Core.Outbox;

public sealed class LoggingOutboxPublisher : IOutboxPublisher
{
    private readonly ILogger<LoggingOutboxPublisher> _log;

    public LoggingOutboxPublisher(ILogger<LoggingOutboxPublisher> log)
    {
        _log = log;
    }

    public Task PublishAsync(string type, string payloadJson, CancellationToken ct)
    {
        _log.LogInformation("OUTBOX PUBLISH type={Type} payload={Payload}", type, payloadJson);
        return Task.CompletedTask;
    }
}