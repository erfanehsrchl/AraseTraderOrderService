namespace Infrastructure.ExternalServices.Models;

public class ExternalAuthTokenRequest
{
    public ExternalAuthTokenRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
}
