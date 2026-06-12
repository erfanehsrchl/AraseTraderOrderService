using System.ServiceModel;
using Contracts.Grpc.Models;

namespace Contracts.Grpc;

[ServiceContract]
public interface IWalletGrpcService
{
    [OperationContract]
    Task<GetWalletByCustomerIdGrpcResponse> GetWalletByCustomerIdAsync(
        GetWalletByCustomerIdGrpcRequest request,
        CancellationToken cancellationToken);

    [OperationContract]
    Task<GetWalletTransactionsByWalletIdGrpcResponse> GetWalletTransactionsByWalletIdAsync(
        GetWalletTransactionsByWalletIdGrpcRequest request,
        CancellationToken cancellationToken);
}
