namespace Domain.Entities;

/// <summary>
/// Represents the customer's financial balance used by order processing and protected by distributed locking.
/// </summary>
public class Wallet
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Customer Customer { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
