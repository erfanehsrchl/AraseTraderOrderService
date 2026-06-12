using Application.DTOs.Customers;

namespace Application.Interfaces;

public interface ICustomerSyncService
{
    Task<CustomerSyncOutDto> SyncAsync(CancellationToken cancellationToken = default);
}
