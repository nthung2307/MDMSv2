namespace Nobisoft.Core.ValueObjects;

public class CreateJwtResponse
{
    public CreateJwtResponse(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }

    public CreateJwtResponse()
    {
        
    }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}