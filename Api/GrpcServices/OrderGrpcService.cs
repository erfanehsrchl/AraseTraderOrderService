using Application.Interfaces;
using Contracts.Grpc.Models;
using Contracts.Grpc.Order;
using Grpc.Core;
using Mapster;

namespace Api.GrpcServices;

/// <summary>
/// Implements the order gRPC boundary by delegating read requests to the application order service.
/// </summary>
public class OrderGrpcService : IOrderGrpcService
{
    private readonly IOrderService _orderService;

    public OrderGrpcService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Retrieves an order by tracking identifier for synchronous consumers such as Gateway or support tooling.
    /// </summary>
    public async Task<GetOrderByTrackingIdGrpcResponse> GetOrderByTrackingIdAsync(
        GetOrderByTrackingIdGrpcRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.GetOrderByTrackingIdAsync(
                request.TrackingId,
                cancellationToken);

            return order.Adapt<GetOrderByTrackingIdGrpcResponse>();
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }
}
