namespace Orders.Persistence.Kafka;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = default!;
    public string TopicOrdersConfirmed { get; init; } = "orders.confirmed";
    public string TopicOrdersPaid { get; init; } = "orders.paid";
    public string TopicOrdersCancelled { get; init; } = "orders.cancelled";
}