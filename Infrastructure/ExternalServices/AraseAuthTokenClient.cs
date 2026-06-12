using System.Net.Http.Json;
using Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices;

public class AraseAuthTokenClient : IAraseAuthTokenClient
{
    private const string TokenCacheKey = "AraseExternalApi:AuthToken";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly AraseExternalApiOptions _options;

    public AraseAuthTokenClient(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<AraseExternalApiOptions> options)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _options = options.Value;
    }

    public async Task<AraseAuthToken> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(TokenCacheKey, out AraseAuthToken? cachedToken) &&
            cachedToken is not null &&
            cachedToken.IsValid)
        {
            var refreshedToken = await RefreshTokenAsync(cachedToken, cancellationToken);
            if (refreshedToken is not null)
            {
                CacheToken(refreshedToken);
                return refreshedToken;
            }
        }

        var token = await RequestTokenAsync(cancellationToken);
        CacheToken(token);
        return token;
    }

    private async Task<AraseAuthToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/token",
            new AuthTokenRequest(_options.Username, _options.Password),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>(cancellationToken);
        return token?.ToAuthToken() ?? throw new InvalidOperationException("Token response was empty.");
    }

    private async Task<AraseAuthToken?> RefreshTokenAsync(
        AraseAuthToken cachedToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshTokenRequest(cachedToken.RefreshToken, cachedToken.AccessToken),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>(cancellationToken);
            return token?.ToAuthToken();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private void CacheToken(AraseAuthToken token)
    {
        var expiresIn = token.ExpiresAt - DateTime.UtcNow;
        if (expiresIn <= TimeSpan.Zero)
        {
            expiresIn = TimeSpan.FromMinutes(5);
        }

        _memoryCache.Set(TokenCacheKey, token, expiresIn);
    }
}
