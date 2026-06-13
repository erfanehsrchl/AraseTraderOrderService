using System.ServiceModel;
using Contracts.Grpc.Models;

namespace Contracts.Grpc.Order;

/// <summary>
/// Defines the code-first gRPC contract for querying orders across service boundaries.
/// </summary>
[ServiceContract(Name = OrderGrpcConstants.ServiceName)]
public interface IOrderGrpcService
{
    /// <summary>
    /// Retrieves an order by the gateway-generated tracking identifier.
    /// </summary>
    [OperationContract(Name = OrderGrpcConstants.GetOrderByTrackingId)]
    Task<GetOrderByTrackingIdGrpcResponse> GetOrderByTrackingIdAsync(
        GetOrderByTrackingIdGrpcRequest request,
        CancellationToken cancellationToken);
}
