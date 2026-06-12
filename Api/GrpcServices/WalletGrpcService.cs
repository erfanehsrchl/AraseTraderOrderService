using Application.Interfaces;
using Contracts.Grpc.Models;
using Contracts.Grpc.Wallet;
using Grpc.Core;
using Mapster;

namespace Api.GrpcServices;

public class WalletGrpcService : IWalletGrpcService
{
    private readonly IWalletService _walletService;

    public WalletGrpcService(
        IWalletService walletService)
    {
        _walletService = walletService;
    }

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
