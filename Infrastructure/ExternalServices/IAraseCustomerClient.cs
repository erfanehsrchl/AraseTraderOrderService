using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Abstraction for retrieving external customer data used by the Customer Synchronization use case.
/// </summary>
public interface IAraseCustomerClient
{
    /// <summary>
    /// Gets the current customer snapshot from the external Arase Trader API.
    /// </summary>
    Task<IReadOnlyCollection<AraseCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
}
