using Application.DTOs.Wallets;
using Application.Interfaces;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Implements wallet read use cases for HTTP diagnostics and gRPC clients without exposing EF Core entities.
/// </summary>
public class WalletService : IWalletService
{
    private readonly AppDbContext _dbContext;

    public WalletService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the wallet associated with a customer for wallet management and cross-service verification.
    /// </summary>
    public async Task<GetWalletByCustomerIdOutDto> GetWalletByCustomerIdAsync(
        long customerId,
        CancellationToken cancellationToken)
    {
        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .Where(currentWallet => currentWallet.CustomerId == customerId)
            .FirstOrDefaultAsync(cancellationToken);

        return wallet?.Adapt<GetWalletByCustomerIdOutDto>()
            ?? throw new InvalidOperationException("Wallet was not found.");
    }

    /// <summary>
    /// Returns wallet transactions in descending creation order for balance audit and support workflows.
    /// </summary>
    public async Task<GetWalletTransactionsByWalletIdOutDto> GetWalletTransactionsByWalletIdAsync(
        long walletId,
        CancellationToken cancellationToken)
    {
        var walletExists = await _dbContext.Wallets
            .AnyAsync(wallet => wallet.Id == walletId, cancellationToken);

        if (!walletExists)
        {
            throw new InvalidOperationException("Wallet was not found.");
        }

        var transactions = await _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(transaction => transaction.WalletId == walletId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);

        return new GetWalletTransactionsByWalletIdOutDto
        {
            Transactions = transactions.Adapt<List<WalletTransactionOutDto>>()
        };
    }
}
