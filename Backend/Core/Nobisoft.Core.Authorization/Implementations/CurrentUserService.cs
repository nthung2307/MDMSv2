using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Exceptions;
using Nobisoft.Core.Extensions;
using Nobisoft.Core.HttpContextAccessor.Interfaces;

namespace Nobisoft.Core.Authorization.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessorService httpContextAccessorService, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessorService = httpContextAccessorService;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public long UserId
    {
        get
        {
            var jwt = GetJwtToken();
            if (jwt == null || jwt.IsMissing())
            {
                throw new UnauthorizedJwtException(nameof(CurrentUserService), nameof(UserId), "You are not logged in yet", _httpContextAccessorService.Tracker());
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            var idClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Id)?.Value;

            if (long.TryParse(idClaim, out var userId) && userId != 0)
            {
                return userId;
            }

            throw new UnauthorizedJwtException(nameof(CurrentUserService), nameof(UserId), "You are not logged in yet", _httpContextAccessorService.Tracker());
        }
    }


    public string UserName
    {
        get
        {
            var jwt = GetJwtToken();
            if (jwt == null || jwt.IsMissing())
            {
                throw new UnauthorizedJwtException(nameof(CurrentUserService), nameof(UserId), "You are not logged in yet", _httpContextAccessorService.Tracker());
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            var userNameClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.UserName)?.Value;

            if (!string.IsNullOrEmpty(userNameClaim))
            {
                return userNameClaim;
            }

            throw new UnauthorizedJwtException(nameof(CurrentUserService), nameof(UserId), "You are not logged in yet", _httpContextAccessorService.Tracker());
        }
    }

    public string? GetJwtToken()
    {
        var result = _httpContextAccessorService.HttpContextAccessor().HttpContext?.Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
        return result?.Value.ToString()?.Replace("Bearer ", "");
    }
}