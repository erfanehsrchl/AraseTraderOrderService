using Infrastructure.ExternalServices.Models;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Abstraction for obtaining external API access tokens without leaking cache or HTTP details to synchronization services.
/// </summary>
public interface IAraseAuthTokenClient
{
    /// <summary>
    /// Gets an access token that can be used to call the external Arase Trader API.
    /// </summary>
    Task<CachedExternalAccessToken> GetTokenAsync(CancellationToken cancellationToken);
}
