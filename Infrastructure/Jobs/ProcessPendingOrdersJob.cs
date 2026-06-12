using Application.DTOs.Jobs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Jobs;

public class ProcessPendingOrdersJob
{
    private const int BatchSize = 100;
    private readonly AppDbContext _dbContext;
    private readonly IDistributedLockService _distributedLockService;
    private readonly HangfireOptions _options;

    public ProcessPendingOrdersJob(
        AppDbContext dbContext,
        IDistributedLockService distributedLockService,
        IOptions<HangfireOptions> options)
    {
        _dbContext = dbContext;
        _distributedLockService = distributedLockService;
        _options = options.Value;
    }

    public async Task<ProcessPendingOrdersOutDto> RunAsync(CancellationToken cancellationToken)
    {
        var pendingOrderIds = await _dbContext.Orders
            .Where(order => order.Status == OrderStatus.Pending)
            .OrderBy(order => order.CreatedAt)
            .Select(order => order.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var result = new ProcessPendingOrdersOutDto();

        foreach (var orderId in pendingOrderIds)
        {
            var processResult = await ProcessOrderAsync(orderId, cancellationToken);
            switch (processResult)
            {
                case OrderProcessResult.Processed:
                    result.ProcessedCount++;
                    break;
                case OrderProcessResult.Failed:
                    result.FailedCount++;
                    break;
                case OrderProcessResult.Skipped:
                    result.SkippedCount++;
                    break;
            }
        }

        result.Message = "Pending orders processing completed.";
        return result;
    }

    private async Task<OrderProcessResult> ProcessOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var orderWallet = await (
                    from orderEntity in _dbContext.Orders.AsNoTracking()
                    join customerWallet in _dbContext.Wallets.AsNoTracking()
                        on orderEntity.CustomerId equals customerWallet.CustomerId
                    where orderEntity.Id == orderId
                    select new
                    {
                        orderEntity.Id,
                        orderEntity.Status,
                        WalletId = customerWallet.Id
                    }
                ).FirstOrDefaultAsync(cancellationToken);

        if (orderWallet is null ||
            orderWallet.Status != OrderStatus.Pending ||
            orderWallet.WalletId == 0)
        {
            return OrderProcessResult.Skipped;
        }

        var lockResource = $"wallet:{orderWallet.WalletId}";
        var lockExpiry = TimeSpan.FromSeconds(_options.WalletLockExpirySeconds);

        await using var distributedLock = await _distributedLockService.TryAcquireAsync(
            lockResource,
            lockExpiry,
            cancellationToken);

        if (distributedLock is null || !distributedLock.IsAcquired)
        {
            return OrderProcessResult.Skipped;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(currentOrder => currentOrder.Id == orderId, cancellationToken);

            if (order is null || order.Status != OrderStatus.Pending)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OrderProcessResult.Skipped;
            }

            var wallet = await _dbContext.Wallets
                .FirstOrDefaultAsync(currentWallet => currentWallet.CustomerId == order.CustomerId, cancellationToken);

            if (wallet is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OrderProcessResult.Skipped;
            }

            var now = DateTime.UtcNow;
            var existingTransaction = await _dbContext.WalletTransactions
                .AnyAsync(walletTransaction => walletTransaction.OrderId == order.Id, cancellationToken);

            if (existingTransaction)
            {
                order.Status = OrderStatus.Processed;
                order.UpdatedAt = now;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OrderProcessResult.Skipped;
            }

            if (order.Side == OrderSide.Buy && wallet.Balance < order.Amount)
            {
                order.Status = OrderStatus.Failed;
                order.FailureReason = "Insufficient wallet balance.";
                order.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OrderProcessResult.Failed;
            }

            var signedAmount = order.Side == OrderSide.Buy ? -order.Amount : order.Amount;
            var balanceBefore = wallet.Balance;
            var balanceAfter = balanceBefore + signedAmount;

            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = now;
            order.Status = OrderStatus.Processed;
            order.UpdatedAt = now;
            order.FailureReason = null;

            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                OrderId = order.Id,
                Amount = signedAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Reason = order.Side == OrderSide.Buy ? WalletTransactionReason.OrderBuy : WalletTransactionReason.OrderSell,
                CreatedAt = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return OrderProcessResult.Processed;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
