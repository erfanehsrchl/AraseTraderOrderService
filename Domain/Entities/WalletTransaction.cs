using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents an immutable wallet ledger entry created by order processing or other wallet balance operations.
/// </summary>
public class WalletTransaction
{
    public long Id { get; set; }
    public long WalletId { get; set; }
    public long? OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public WalletTransactionReason Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public Order? Order { get; set; }
}
