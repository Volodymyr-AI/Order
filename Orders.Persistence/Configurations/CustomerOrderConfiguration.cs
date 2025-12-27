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

        b.OwnsMany<OrderItem>("_items", items =>
        {
            items.ToTable("customer_order_items");

            items.WithOwner().HasForeignKey("order_id");

            items.Property<int>("id").ValueGeneratedOnAdd();
            items.HasKey("id");

            items.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            items.Property(x => x.NameSnapshot).HasColumnName("name_snapshot").HasMaxLength(256).IsRequired();
            items.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();

            items.OwnsOne(x => x.UnitPriceSnapshot, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("unit_price_amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                m.Property(p => p.Currency)
                    .HasColumnName("unit_price_currency")
                    .HasConversion(currencyConv)
                    .HasMaxLength(3)
                    .IsRequired();
            });

            items.HasIndex("order_id");
            items.HasIndex(x => x.ProductId);
        });
        b.HasIndex(x => x.CustomerId);
        b.HasIndex(x => x.StoreId);
    }
}