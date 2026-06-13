using Application.Interfaces;
using Contracts.Grpc.Models;
using Contracts.Grpc.Wallet;
using Grpc.Core;
using Mapster;

namespace Api.GrpcServices;

/// <summary>
/// Implements the wallet gRPC boundary without exposing EF Core entities or persistence details.
/// </summary>
public class WalletGrpcService : IWalletGrpcService
{
    private readonly IWalletService _walletService;

    public WalletGrpcService(
        IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Retrieves a customer's wallet for cross-service wallet management scenarios.
    /// </summary>
    public async Task<GetWalletByCustomerIdGrpcResponse> GetWalletByCustomerIdAsync(
        GetWalletByCustomerIdGrpcRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _walletService.GetWalletByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            return wallet.Adapt<GetWalletByCustomerIdGrpcResponse>();
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }

    /// <summary>
    /// Retrieves wallet transactions for synchronous gRPC clients that need wallet history.
    /// </summary>
    public async Task<GetWalletTransactionsByWalletIdGrpcResponse> GetWalletTransactionsByWalletIdAsync(
        GetWalletTransactionsByWalletIdGrpcRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _walletService.GetWalletTransactionsByWalletIdAsync(
                request.WalletId,
                cancellationToken);

            return result.Adapt<GetWalletTransactionsByWalletIdGrpcResponse>();
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }
}
