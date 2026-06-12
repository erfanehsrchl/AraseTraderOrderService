namespace Infrastructure.ExternalServices.Models;

public class AraseAuthToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(AccessToken) && ExpiresAt > DateTime.UtcNow.AddMinutes(1);
}
