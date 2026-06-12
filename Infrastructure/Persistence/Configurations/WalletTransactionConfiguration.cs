using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(transaction => transaction.BalanceBefore).HasPrecision(18, 2).IsRequired();
        builder.Property(transaction => transaction.BalanceAfter).HasPrecision(18, 2).IsRequired();
        builder.Property(transaction => transaction.Reason).IsRequired();
        builder.Property(transaction => transaction.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(transaction => transaction.OrderId).IsUnique();

        builder.HasOne(transaction => transaction.Wallet)
            .WithMany(wallet => wallet.Transactions)
            .HasForeignKey(transaction => transaction.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transaction => transaction.Order)
            .WithOne(order => order.WalletTransaction)
            .HasForeignKey<WalletTransaction>(transaction => transaction.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
