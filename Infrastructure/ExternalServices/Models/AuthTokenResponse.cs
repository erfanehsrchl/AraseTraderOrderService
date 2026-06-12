using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Models;

public class AuthTokenResponse
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("expiresIn")]
    public int? ExpiresIn { get; set; }

    public AraseAuthToken ToAuthToken()
    {
        var accessToken = AccessToken ?? Token;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Token response did not contain an access token.");
        }

        return new AraseAuthToken
        {
            AccessToken = accessToken,
            RefreshToken = RefreshToken,
            ExpiresAt = ExpiresAt ?? DateTime.UtcNow.AddSeconds(ExpiresIn ?? 3600)
        };
    }
}
