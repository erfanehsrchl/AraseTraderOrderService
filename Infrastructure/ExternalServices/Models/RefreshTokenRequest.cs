namespace Infrastructure.ExternalServices.Models;

public class RefreshTokenRequest
{
    public RefreshTokenRequest(string? refreshToken, string accessToken)
    {
        RefreshToken = refreshToken;
        AccessToken = accessToken;
    }

    public string? RefreshToken { get; }
    public string AccessToken { get; }
}
