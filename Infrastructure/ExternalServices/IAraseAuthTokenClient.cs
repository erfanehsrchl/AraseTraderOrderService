using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

public interface IAraseAuthTokenClient
{
    Task<AraseAuthToken> GetTokenAsync(CancellationToken cancellationToken);
}
