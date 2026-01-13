using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Core.BaseModels;
using Orders.Persistence.Converters;

namespace Orders.Persistence.Configurations;

public sealed class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
{
    public void Configure(EntityTypeBuilder<CustomerOrder> b)
    {
        b.ToTable("customer_orders");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.CustomerId).IsRequired();
        b.Property(x => x.StoreId).IsRequired();

        b.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
        
        b.Property(x=>x.CreatedAt).IsRequired();
        b.Property(x => x.ConfirmedAt);
        b.Property(x => x.PaidAt);
        b.Property(x => x.CancelledAt);

        //Total: Money -> amount + currency_code
        var currencyConv = new CurrencyCodeConverter();

        b.Property<Currency?>("_currency")
            .HasColumnName("currency_code")
            .HasConversion(currencyConv)
            .HasMaxLength(3);

        b.OwnsOne<Money>(x => x.Total, m =>
        {
            m.Property(p => p.Amount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2)
                .IsRequired();
            m.Property(p => p.Currency)
                .HasColumnName("total_currency")
                .HasConversion(currencyConv)
                .HasMaxLength(3)
                .IsRequired();
        });    
        
        b.Ignore("DomainEvents");
        b.Ignore("Outbox");

        b.OwnsMany(o => o.Items, items =>
        {
            items.ToTable("order_items");
            items.WithOwner().HasForeignKey("order_id");

            items.Property<Guid>("order_id");
            items.HasKey("order_id", nameof(OrderItem.ProductId));

            items.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
            items.Property(i => i.NameSnapshot).HasColumnName("name_snapshot").IsRequired();
            items.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();

            items.OwnsOne(i => i.UnitPriceSnapshot, m =>
            {
                m.Property(x => x.Amount).HasColumnName("unit_price_amount").IsRequired().HasPrecision(18, 2);
                m.Property(x => x.Currency)
                    .HasColumnName("unit_price_currency_code")
                    .HasConversion(v => v.Code, v => Currency.FromCode(v))
                    .IsRequired();
            });
        });
        
        b.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        b.HasIndex(x => x.CustomerId);
        b.HasIndex(x => x.StoreId);
    }
}