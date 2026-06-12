namespace Infrastructure.ExternalServices.Models;

public class CachedExternalAccessToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(AccessToken) &&
        ExpiresAtUtc > DateTime.UtcNow.AddMinutes(1);
}
