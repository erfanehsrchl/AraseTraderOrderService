using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices;

public class AraseAuthTokenClient : IAraseAuthTokenClient
{
    private const string TokenCacheKey = "arase:external-api:access-token";
    private static readonly TimeSpan MaxTokenCacheTtl = TimeSpan.FromMinutes(55);
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private readonly AraseExternalApiOptions _options;

    public AraseAuthTokenClient(
        HttpClient httpClient,
        IDistributedCache cache,
        IOptions<AraseExternalApiOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<AraseAuthToken> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var cachedToken = await GetCachedTokenAsync(cancellationToken);
        if (cachedToken is not null && cachedToken.IsValid)
        {
            var refreshedToken = await RefreshTokenAsync(cachedToken, cancellationToken);
            if (refreshedToken is not null)
            {
                await CacheTokenAsync(refreshedToken, cancellationToken);
                return refreshedToken;
            }
        }

        var token = await RequestTokenAsync(cancellationToken);
        await CacheTokenAsync(token, cancellationToken);
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

    private async Task<AraseAuthToken?> GetCachedTokenAsync(CancellationToken cancellationToken)
    {
        var cachedToken = await _cache.GetStringAsync(TokenCacheKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cachedToken))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AraseAuthToken>(cachedToken);
        }
        catch (JsonException)
        {
            await _cache.RemoveAsync(TokenCacheKey, cancellationToken);
            return null;
        }
    }

    private async Task CacheTokenAsync(AraseAuthToken token, CancellationToken cancellationToken)
    {
        var ttl = GetCacheTtl(token);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _cache.SetStringAsync(
            TokenCacheKey,
            JsonSerializer.Serialize(token),
            cacheOptions,
            cancellationToken);
    }

    private static TimeSpan GetCacheTtl(AraseAuthToken token)
    {
        var tokenLifetime = token.ExpiresAt - DateTime.UtcNow;
        if (tokenLifetime <= TimeSpan.FromMinutes(1))
        {
            return TimeSpan.FromMinutes(5);
        }

        var ttl = tokenLifetime - TimeSpan.FromMinutes(1);
        return ttl < MaxTokenCacheTtl ? ttl : MaxTokenCacheTtl;
    }
}
