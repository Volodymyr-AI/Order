using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Order.Core.Outbox;

namespace Orders.Persistence.Kafka;

public sealed class KafkaOutboxPublisher : IOutboxPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _opt;
    private readonly ILogger<KafkaOutboxPublisher> _log;

    public KafkaOutboxPublisher(IOptions<KafkaOptions> options, ILogger<KafkaOutboxPublisher> log)
    {
        _opt = options.Value;
        _log = log;

        var config = new ProducerConfig
        {
            BootstrapServers = _opt.BootstrapServers,

            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5,
            RetryBackoffMs = 200,
            LingerMs = 5,
        };
        
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(
        Guid messageId,
        string type,
        string payloadJson,
        string correlationId,
        CancellationToken ct)
    {
        var topic = ResolveTopic(type);
        var message = new Message<string, string>
        {
            Key = messageId.ToString(),
            Value = payloadJson,
            Headers = new Headers
            {
                new Header("event_type", System.Text.Encoding.UTF8.GetBytes(type)),
                new Header("correlationId", Encoding.UTF8.GetBytes(correlationId))
            }
        };
        
        ct.ThrowIfCancellationRequested();

        try
        {
            var result = await _producer.ProduceAsync(topic, message).ConfigureAwait(false);
            _log.LogInformation("Kafka published topic={Topic} partition={Partition} offset={Offset} type={Type} correlationId={CorrelationId}",
                result.Topic, result.Partition, result.Offset, type, correlationId);
        }
        catch (ProduceException<string, string> ex)
        {
            _log.LogWarning(ex, "Kafka publish failed type={Type} correlationId={CorrelationId}", 
                type, correlationId);
            throw;
        }
    }

    private string ResolveTopic(string type)
    {
        if (type.Contains("CustomerOrderConfirmed", StringComparison.OrdinalIgnoreCase))
            return _opt.TopicOrdersConfirmed;

        if (type.Contains("CustomerOrderPaid", StringComparison.OrdinalIgnoreCase))
            return _opt.TopicOrdersPaid;

        if (type.Contains("Cancelled", StringComparison.OrdinalIgnoreCase))
            return _opt.TopicOrdersCancelled;
        
        return "orders.events";
    }
    
    public void Dispose() => _producer.Dispose();
}