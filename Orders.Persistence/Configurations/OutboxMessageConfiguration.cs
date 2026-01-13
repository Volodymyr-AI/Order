using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Core.Outbox;

namespace Orders.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");

        b.HasKey(x => x.Id);
        b.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(512);
        b.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .IsRequired();
        b.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();
        b.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");
        b.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .IsRequired();
        b.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2000);

        b.HasIndex(x => x.ProcessedAt).HasDatabaseName("ix_outbox_processed_at");
    }
}