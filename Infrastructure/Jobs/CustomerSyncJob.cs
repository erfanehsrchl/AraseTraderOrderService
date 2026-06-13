using Application.Interfaces;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire job adapter that triggers Customer Synchronization without containing synchronization business logic.
/// </summary>
public class CustomerSyncJob
{
    private readonly ICustomerSyncService _customerSyncService;

    public CustomerSyncJob(ICustomerSyncService customerSyncService)
    {
        _customerSyncService = customerSyncService;
    }

    /// <summary>
    /// Runs the customer synchronization application use case from the scheduler.
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken)
    {
        return _customerSyncService.SyncAsync(cancellationToken);
    }
}
