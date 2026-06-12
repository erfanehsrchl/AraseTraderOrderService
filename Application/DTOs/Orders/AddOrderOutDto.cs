using Domain.Enums;

namespace Application.DTOs.Orders;

public class AddOrderOutDto
{
    public Guid TrackingId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Message { get; set; }
}
