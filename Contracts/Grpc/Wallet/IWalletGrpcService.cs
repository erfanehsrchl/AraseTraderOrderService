using System.ServiceModel;
using Contracts.Grpc.Models;

namespace Contracts.Grpc.Wallet;

[ServiceContract(Name = WalletGrpcConstants.ServiceName)]
public interface IWalletGrpcService
{
    [OperationContract(Name = WalletGrpcConstants.GetWalletByCustomerId)]
    Task<GetWalletByCustomerIdGrpcResponse> GetWalletByCustomerIdAsync(
        GetWalletByCustomerIdGrpcRequest request,
        CancellationToken cancellationToken);

    [OperationContract(Name = WalletGrpcConstants.GetWalletTransactionsByWalletId)]
    Task<GetWalletTransactionsByWalletIdGrpcResponse> GetWalletTransactionsByWalletIdAsync(
        GetWalletTransactionsByWalletIdGrpcRequest request,
        CancellationToken cancellationToken);
}
