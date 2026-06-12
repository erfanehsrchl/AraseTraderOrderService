namespace Application.Interfaces;

public interface IDistributedLockHandle : IAsyncDisposable
{
    bool IsAcquired { get; }
}
