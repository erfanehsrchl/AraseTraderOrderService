using Application.DTOs.Wallets;

namespace Application.Interfaces;

public interface IWalletService
{
    Task<GetWalletByCustomerIdOutDto> GetWalletByCustomerIdAsync(
        long customerId,
        CancellationToken cancellationToken);

    Task<GetWalletTransactionsByWalletIdOutDto> GetWalletTransactionsByWalletIdAsync(
        long walletId,
        CancellationToken cancellationToken);
}
