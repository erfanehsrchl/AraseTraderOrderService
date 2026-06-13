using Application.DTOs.Wallets;

namespace Application.Interfaces;

/// <summary>
/// Defines wallet query use cases consumed by diagnostic HTTP endpoints and gRPC service boundaries.
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Retrieves the wallet owned by a customer without exposing persistence concerns to the transport layer.
    /// </summary>
    Task<GetWalletByCustomerIdOutDto> GetWalletByCustomerIdAsync(
        long customerId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves wallet transaction history for wallet management and cross-service read scenarios.
    /// </summary>
    Task<GetWalletTransactionsByWalletIdOutDto> GetWalletTransactionsByWalletIdAsync(
        long walletId,
        CancellationToken cancellationToken);
}
