using Application.DTOs.Orders;

namespace Application.Interfaces;

/// <summary>
/// Defines the order application use cases shared by HTTP, gRPC, and Event-Driven Architecture entry points.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Registers a gateway-created order as pending so wallet processing can be completed asynchronously.
    /// </summary>
    Task<AddOrderOutDto> AddOrderAsync(AddOrderInDto input, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an order by its tracking identifier for diagnostics and synchronous service integrations.
    /// </summary>
    Task<GetOrderByTrackingIdOutDto> GetOrderByTrackingIdAsync(Guid trackingId, CancellationToken cancellationToken);
}
