using System.ServiceModel;
using Contracts.Grpc.Models;

namespace Contracts.Grpc.Wallet;

/// <summary>
/// Defines the code-first gRPC contract for wallet management queries consumed by external services.
/// </summary>
[ServiceContract(Name = WalletGrpcConstants.ServiceName)]
public interface IWalletGrpcService
{
    /// <summary>
    /// Retrieves the wallet owned by a customer without exposing OrderService persistence models.
    /// </summary>
    [OperationContract(Name = WalletGrpcConstants.GetWalletByCustomerId)]
    Task<GetWalletByCustomerIdGrpcResponse> GetWalletByCustomerIdAsync(
        GetWalletByCustomerIdGrpcRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves wallet ledger entries for balance audit and customer support workflows.
    /// </summary>
    [OperationContract(Name = WalletGrpcConstants.GetWalletTransactionsByWalletId)]
    Task<GetWalletTransactionsByWalletIdGrpcResponse> GetWalletTransactionsByWalletIdAsync(
        GetWalletTransactionsByWalletIdGrpcRequest request,
        CancellationToken cancellationToken);
}
