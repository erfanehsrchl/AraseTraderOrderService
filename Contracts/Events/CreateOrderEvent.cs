using Contracts.Enums;

namespace Contracts.Events;

public class CreateOrderEvent
{
    public Guid TrackingId { get; set; }
    public long CustomerId { get; set; }
    public OrderSideContract Side { get; set; }
    public decimal Amount { get; set; }
    public DateTime OccurredAt { get; set; }
}
