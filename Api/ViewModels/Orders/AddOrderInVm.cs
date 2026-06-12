using Domain.Enums;

namespace Api.ViewModels.Orders;

public class AddOrderInVm
{
    public long CustomerId { get; set; }
    public OrderSide Side { get; set; }
    public decimal Amount { get; set; }
}
