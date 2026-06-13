namespace Infrastructure.ExternalServices.Models;

/// <summary>
/// Provides external Arase Trader API settings used by authentication and customer synchronization clients.
/// </summary>
public class AraseExternalApiOptions
{
    public const string SectionName = "AraseExternalApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
