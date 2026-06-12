using Domain.Enums;

namespace Application.DTOs.Orders;

public class AddOrderInDto
{
    public long CustomerId { get; set; }

    public OrderSide Side { get; set; }

    public decimal Amount { get; set; }
}
