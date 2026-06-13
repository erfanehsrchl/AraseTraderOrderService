namespace Infrastructure.ExternalServices.Models;

public class AraseExternalApiOptions
{
    public const string SectionName = "AraseExternalApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
