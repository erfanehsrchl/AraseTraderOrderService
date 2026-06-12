using Application.Interfaces;

namespace Infrastructure.Jobs;

public class CustomerSyncJob
{
    private readonly ICustomerSyncService _customerSyncService;

    public CustomerSyncJob(ICustomerSyncService customerSyncService)
    {
        _customerSyncService = customerSyncService;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        return _customerSyncService.SyncAsync(cancellationToken);
    }
}
