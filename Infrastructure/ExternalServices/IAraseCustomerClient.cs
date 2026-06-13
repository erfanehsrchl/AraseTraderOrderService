using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

public interface IAraseCustomerClient
{
    Task<IReadOnlyCollection<AraseCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
}
