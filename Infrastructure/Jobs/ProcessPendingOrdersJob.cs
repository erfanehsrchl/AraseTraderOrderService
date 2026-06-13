using Application.DTOs.Jobs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire job that processes pending orders in batches and protects wallet updates with distributed locking.
/// </summary>
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

    /// <summary>
    /// Processes a bounded batch of pending orders and returns operational counts for diagnostics and monitoring.
    /// </summary>
    public async Task<ProcessPendingOrdersOutDto> RunAsync(CancellationToken cancellationToken)
    {
        var pendingOrderIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Status == OrderStatus.Pending)
            .OrderBy(order => order.CreatedAt)
            .Select(order => order.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var result = new ProcessPendingOrdersOutDto();

        if (pendingOrderIds.Count == 0)
        {
            result.Message = "No pending orders found.";
            return result;
        }

        foreach (var orderId in pendingOrderIds)
        {
            var processResult = await ProcessOrderAsync(orderId, cancellationToken);
            ApplyProcessResult(result, processResult);
        }

        result.Message = "Pending orders processing completed.";
        return result;
    }

    private static void ApplyProcessResult(
        ProcessPendingOrdersOutDto result,
        OrderProcessResult processResult)
    {
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

    private async Task<OrderProcessResult> ProcessOrderAsync(
    long orderId,
    CancellationToken cancellationToken)
    {
        var walletId = await GetPendingOrderWalletIdAsync(orderId, cancellationToken);

        if (walletId is null)
            return OrderProcessResult.Skipped;

        var lockResource = $"wallet:{walletId}";
        var lockExpiry = TimeSpan.FromSeconds(_options.WalletLockExpirySeconds);

        await using var distributedLock = await _distributedLockService.TryAcquireAsync(
            lockResource,
            lockExpiry,
            cancellationToken);

        if (distributedLock is null || !distributedLock.IsAcquired)
            return OrderProcessResult.Skipped;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null || order.Status != OrderStatus.Pending)
                return OrderProcessResult.Skipped;

            var wallet = await _dbContext.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == order.CustomerId, cancellationToken);

            if (wallet is null)
                return OrderProcessResult.Skipped;

            var now = DateTime.UtcNow;

            var alreadyProcessed = await _dbContext.WalletTransactions
                .AnyAsync(x => x.OrderId == order.Id, cancellationToken);

            if (alreadyProcessed)
            {
                MarkOrderAsProcessed(order, now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OrderProcessResult.Skipped;
            }

            if (order.Side == OrderSide.Buy && wallet.Balance < order.Amount)
            {
                MarkOrderAsFailed(order, "Insufficient wallet balance.", now);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OrderProcessResult.Failed;
            }

            ApplyWalletTransaction(order, wallet, now);

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

    private async Task<long?> GetPendingOrderWalletIdAsync(
         long orderId,
         CancellationToken cancellationToken)
    {
        return await (
            from order in _dbContext.Orders.AsNoTracking()
            join wallet in _dbContext.Wallets.AsNoTracking()
                on order.CustomerId equals wallet.CustomerId
            where order.Id == orderId &&
                  order.Status == OrderStatus.Pending
            select (long?)wallet.Id
        ).FirstOrDefaultAsync(cancellationToken);
    }

    private static void MarkOrderAsProcessed(Order order, DateTime now)
    {
        order.Status = OrderStatus.Processed;
        order.UpdatedAt = now;
        order.FailureReason = null;
    }

    private static void MarkOrderAsFailed(Order order, string reason, DateTime now)
    {
        order.Status = OrderStatus.Failed;
        order.FailureReason = reason;
        order.UpdatedAt = now;
    }

    private void ApplyWalletTransaction(Order order, Wallet wallet, DateTime now)
    {
        var signedAmount = order.Side == OrderSide.Buy
            ? -order.Amount
            : order.Amount;

        var balanceBefore = wallet.Balance;
        var balanceAfter = balanceBefore + signedAmount;

        wallet.Balance = balanceAfter;
        wallet.UpdatedAt = now;

        MarkOrderAsProcessed(order, now);

        _dbContext.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            OrderId = order.Id,
            Amount = signedAmount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Reason = order.Side == OrderSide.Buy
                ? WalletTransactionReason.OrderBuy
                : WalletTransactionReason.OrderSell,
            CreatedAt = now
        });
    }
}
