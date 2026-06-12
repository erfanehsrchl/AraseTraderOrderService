using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.TrackingId).IsRequired();
        builder.Property(order => order.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Side).IsRequired();
        builder.Property(order => order.Status).IsRequired();
        builder.Property(order => order.FailureReason).HasMaxLength(1000);
        builder.Property(order => order.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(order => order.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(order => order.TrackingId).IsUnique();

        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.WalletTransaction)
            .WithOne(transaction => transaction.Order)
            .HasForeignKey<WalletTransaction>(transaction => transaction.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
