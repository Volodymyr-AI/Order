using Microsoft.Extensions.Logging;

namespace Order.Core.Outbox;

public sealed class LoggingOutboxPublisher : IOutboxPublisher
{
    private readonly ILogger<LoggingOutboxPublisher> _log;

    public LoggingOutboxPublisher(ILogger<LoggingOutboxPublisher> log)
    {
        _log = log;
    }

    public Task PublishAsync(Guid messageId, string type, string payloadJson, string correlationId, CancellationToken ct)
    {
        _log.LogInformation("OUTBOX PUBLISH id={Id} type={Type} payload={Payload} correlationId={CorrelationId}", messageId, type, payloadJson, correlationId);
        return Task.CompletedTask;
    }
}