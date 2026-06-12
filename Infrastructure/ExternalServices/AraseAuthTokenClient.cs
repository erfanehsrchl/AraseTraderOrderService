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

    public async Task<CachedExternalAccessToken> GetTokenAsync(CancellationToken cancellationToken)
    {
        var cachedToken = await GetCachedTokenAsync(cancellationToken);
        if (cachedToken is not null && cachedToken.IsValid)
        {
            var refreshedToken = await RefreshAccessTokenAsync(cachedToken, cancellationToken);
            if (refreshedToken is not null)
            {
                await CacheTokenAsync(refreshedToken, cancellationToken);
                return refreshedToken;
            }

            await _cache.RemoveAsync(TokenCacheKey, cancellationToken);
        }

        var token = await RequestTokenAsync(cancellationToken);
        await CacheTokenAsync(token, cancellationToken);
        return token;
    }

    private async Task<CachedExternalAccessToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/token",
            new ExternalAuthTokenRequest(_options.Username, _options.Password),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<ExternalAuthTokenResponse>(cancellationToken);
        return token?.ToCachedAccessToken() ?? throw new InvalidOperationException("Token response was empty.");
    }

    private async Task<CachedExternalAccessToken?> RefreshAccessTokenAsync(
        CachedExternalAccessToken cachedToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/refresh",
                new ExternalAuthRefreshRequest(cachedToken.AccessToken),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<ExternalAuthTokenResponse>(cancellationToken);
            return token?.ToCachedAccessToken();
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

    private async Task<CachedExternalAccessToken?> GetCachedTokenAsync(CancellationToken cancellationToken)
    {
        var cachedToken = await _cache.GetStringAsync(TokenCacheKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cachedToken))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CachedExternalAccessToken>(cachedToken);
        }
        catch (JsonException)
        {
            await _cache.RemoveAsync(TokenCacheKey, cancellationToken);
            return null;
        }
    }

    private async Task CacheTokenAsync(CachedExternalAccessToken token, CancellationToken cancellationToken)
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

    private static TimeSpan GetCacheTtl(CachedExternalAccessToken token)
    {
        var tokenLifetime = token.ExpiresAtUtc - DateTime.UtcNow;
        if (tokenLifetime <= TimeSpan.FromMinutes(1))
        {
            return TimeSpan.FromSeconds(1);
        }

        var ttl = tokenLifetime - TimeSpan.FromMinutes(1);
        return ttl < MaxTokenCacheTtl ? ttl : MaxTokenCacheTtl;
    }
}
