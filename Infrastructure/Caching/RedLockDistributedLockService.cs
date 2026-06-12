using Application.Interfaces;
using RedLockNet.SERedis;

namespace Infrastructure.Caching;

public class RedLockDistributedLockService : IDistributedLockService
{
    private static readonly TimeSpan Wait = TimeSpan.Zero;
    private static readonly TimeSpan Retry = TimeSpan.Zero;
    private readonly RedLockFactory _redLockFactory;

    public RedLockDistributedLockService(RedLockFactory redLockFactory)
    {
        _redLockFactory = redLockFactory;
    }

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
