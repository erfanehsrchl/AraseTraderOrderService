namespace Infrastructure.ExternalServices.Models;

public class ExternalAuthRefreshRequest
{
    public ExternalAuthRefreshRequest(string token)
    {
        Token = token;
    }

    public string Token { get; }
}
