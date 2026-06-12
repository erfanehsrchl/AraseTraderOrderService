using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public Guid TrackingId { get; set; }
    public long CustomerId { get; set; }
    public OrderSide Side { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Customer Customer { get; set; } = null!;
    public WalletTransaction? WalletTransaction { get; set; }
}
