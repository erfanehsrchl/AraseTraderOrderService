namespace Application.Interfaces;

/// <summary>
/// Represents an acquired distributed lock that releases its underlying resource when disposed.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    bool IsAcquired { get; }
}
