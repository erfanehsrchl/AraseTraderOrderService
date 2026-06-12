using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Balance).HasPrecision(18, 2).IsRequired();
        builder.Property(wallet => wallet.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(wallet => wallet.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(wallet => wallet.CustomerId).IsUnique();

        builder.HasOne(wallet => wallet.Customer)
            .WithOne(customer => customer.Wallet)
            .HasForeignKey<Wallet>(wallet => wallet.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wallet => wallet.Transactions)
            .WithOne(transaction => transaction.Wallet)
            .HasForeignKey(transaction => transaction.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
