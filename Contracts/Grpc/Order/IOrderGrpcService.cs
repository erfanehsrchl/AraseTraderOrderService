using System.ServiceModel;
using Contracts.Grpc.Models;

namespace Contracts.Grpc.Order;

[ServiceContract(Name = OrderGrpcConstants.ServiceName)]
public interface IOrderGrpcService
{
    [OperationContract(Name = OrderGrpcConstants.GetOrderByTrackingId)]
    Task<GetOrderByTrackingIdGrpcResponse> GetOrderByTrackingIdAsync(
        GetOrderByTrackingIdGrpcRequest request,
        CancellationToken cancellationToken);
}
