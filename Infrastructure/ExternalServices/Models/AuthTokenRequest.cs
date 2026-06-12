namespace Infrastructure.ExternalServices.Models;

public class AuthTokenRequest
{
    public AuthTokenRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
}
