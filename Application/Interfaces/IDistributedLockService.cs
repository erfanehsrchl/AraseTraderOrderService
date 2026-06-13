namespace Application.Interfaces;

/// <summary>
/// Provides an application-level abstraction for distributed locking around shared resources such as wallets.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire a distributed lock without blocking so concurrent wallet processing can be safely skipped and retried later.
    /// </summary>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken);
}
