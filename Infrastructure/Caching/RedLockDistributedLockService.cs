using Application.Interfaces;
using RedLockNet.SERedis;

namespace Infrastructure.Caching;

/// <summary>
/// Implements distributed locking with RedLock.net and Redis for concurrency-sensitive wallet processing.
/// </summary>
public class RedLockDistributedLockService : IDistributedLockService
{
    private static readonly TimeSpan Wait = TimeSpan.Zero;
    private static readonly TimeSpan Retry = TimeSpan.Zero;
    private readonly RedLockFactory _redLockFactory;

    public RedLockDistributedLockService(RedLockFactory redLockFactory)
    {
        _redLockFactory = redLockFactory;
    }

    /// <summary>
    /// Attempts to acquire a Redis-backed lock and returns null when another worker already owns the resource.
    /// </summary>
    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        var redLock = await _redLockFactory.CreateLockAsync(resource, expiry, Wait, Retry, cancellationToken);
        if (redLock.IsAcquired)
        {
            return new RedLockDistributedLockHandle(redLock);
        }

        await redLock.DisposeAsync();
        return null;
    }
}
