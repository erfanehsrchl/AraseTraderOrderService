namespace Application.Interfaces;

public interface IDistributedLockService
{
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken);
}
