using Application.DTOs.Customers;

namespace Application.Interfaces;

/// <summary>
/// Defines the Customer Synchronization use case that imports external customers and ensures wallet ownership.
/// </summary>
public interface ICustomerSyncService
{
    /// <summary>
    /// Synchronizes customers from the external provider and creates missing wallets for newly discovered customers.
    /// </summary>
    Task<CustomerSyncOutDto> SyncAsync(CancellationToken cancellationToken);
}
