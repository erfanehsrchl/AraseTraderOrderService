using Domain.Enums;

namespace Api.ViewModels.Orders;

public class AddOrderOutVm
{
    public Guid TrackingId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Message { get; set; }
}
