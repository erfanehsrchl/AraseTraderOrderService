using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Models;

public class ExternalAuthTokenResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("tokenType")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expiresInSeconds")]
    public int ExpiresInSeconds { get; set; }

    [JsonPropertyName("expiresAtUtc")]
    public DateTime ExpiresAtUtc { get; set; }

    public CachedExternalAccessToken ToCachedAccessToken()
    {
        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            throw new InvalidOperationException("Token response did not contain an access token.");
        }

        var expiresInSeconds = ExpiresInSeconds > 0 ? ExpiresInSeconds : 3600;
        var expiresAtUtc = ExpiresAtUtc == default
            ? DateTime.UtcNow.AddSeconds(expiresInSeconds)
            : ExpiresAtUtc.ToUniversalTime();

        return new CachedExternalAccessToken
        {
            AccessToken = AccessToken,
            TokenType = string.IsNullOrWhiteSpace(TokenType) ? "Bearer" : TokenType,
            ExpiresInSeconds = expiresInSeconds,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
