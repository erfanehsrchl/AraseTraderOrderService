using Domain.Enums;

namespace Application.Messaging.Orders;

public class CreateOrderEvent
{
    public Guid TrackingId { get; set; }
    public long CustomerId { get; set; }
    public OrderSide Side { get; set; }
    public decimal Amount { get; set; }
}
