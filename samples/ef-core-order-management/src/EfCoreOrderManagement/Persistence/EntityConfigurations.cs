using EfCoreOrderManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreOrderManagement.Persistence;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Email).HasMaxLength(256).IsRequired();
        builder.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(customer => customer.Email).IsUnique();
        builder.Navigation(customer => customer.Orders).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.CreatedAtUtc).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(order => order.Version).IsConcurrencyToken();
        builder.HasIndex(order => new { order.Status, order.CreatedAtUtc });

        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(order => order.ShippingAddress, owned =>
        {
            owned.Property(address => address.Line1).HasColumnName("shipping_line1").HasMaxLength(200).IsRequired();
            owned.Property(address => address.City).HasColumnName("shipping_city").HasMaxLength(100).IsRequired();
            owned.Property(address => address.CountryCode).HasColumnName("shipping_country_code").HasMaxLength(2).IsRequired();
        });
        builder.Navigation(order => order.ShippingAddress).IsRequired();
        builder.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductCode).HasMaxLength(64).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.Quantity).IsRequired();
        builder.HasIndex(item => new { item.OrderId, item.ProductCode }).IsUnique();

        builder.HasOne<Order>()
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
