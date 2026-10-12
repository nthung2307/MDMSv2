using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization.Commons;
using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Authorization.Implementations;

public class JwtService : IJwtService
{
    private readonly JwtConfiguration _jwtOptions;

    public JwtService(IOptions<JwtConfiguration> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<CreateJwtResponse> GenerateJwtAsync(long accountId, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.SymmetricSecurityKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = claimsIdentity,
                Expires = DateTime.Now.AddSeconds(_jwtOptions.Expires),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            await Task.CompletedTask;
            return new CreateJwtResponse()
            {
                Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor)),
                RefreshToken = GenerateRefreshJwtToken(accountId, cancellationToken)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private string GenerateRefreshJwtToken(long accountId, CancellationToken cancellationToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.SymmetricSecurityKey);
            var claims = new List<Claim>();

            // claims.Add(new Claim(JwtClaimTypes.IdentityProvider, Constants.LoginProviders.Self));
            claims.Add(new Claim(JwtClaimTypes.Id, accountId.ToString()));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtOptions.RefreshTokenExpires),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}