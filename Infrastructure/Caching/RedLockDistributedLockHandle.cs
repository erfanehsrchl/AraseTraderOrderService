using Application.Interfaces;
using RedLockNet;

namespace Infrastructure.Caching;

public class RedLockDistributedLockHandle : IDistributedLockHandle
{
    private readonly IRedLock _redLock;

    public RedLockDistributedLockHandle(IRedLock redLock)
    {
        _redLock = redLock;
    }

    public bool IsAcquired => _redLock.IsAcquired;

    public async ValueTask DisposeAsync()
    {
        await _redLock.DisposeAsync();
    }
}
