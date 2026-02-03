using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Core.BaseModels;

namespace Orders.Persistence.Configurations;

public sealed class IdempotencyRecordConfig : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> b)
    {
        b.ToTable("idempotency_keys");
        b.HasKey(x => x.Id);

        b.Property(x => x.Scope).HasMaxLength(128).IsRequired();
        b.Property(x => x.IdentityType).HasMaxLength(16).IsRequired();
        b.Property(x => x.IdentityId).HasMaxLength(128).IsRequired();
        b.Property(x => x.Key).HasMaxLength(128).IsRequired();
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();

        b.HasIndex(x => new { x.Scope, x.IdentityType, x.IdentityId, x.Key })
            .IsUnique();
    }
}