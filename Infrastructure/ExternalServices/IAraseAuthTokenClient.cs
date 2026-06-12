using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

public interface IAraseAuthTokenClient
{
    Task<CachedExternalAccessToken> GetTokenAsync(CancellationToken cancellationToken);
}
