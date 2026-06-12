using Application.DTOs.Orders;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<AddOrderOutDto> AddOrderAsync(AddOrderInDto input, CancellationToken cancellationToken);
}
