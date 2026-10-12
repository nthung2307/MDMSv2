using System.Security.Claims;
using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.DataTransferObjects.Responses.Users;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class RefreshTokenCommandHandler : IRefreshTokenCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;

    public RefreshTokenCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, IJwtService jwtService, IUserTokenReadWriteRepository userTokenReadWriteRepository, ICurrentUserService currentUserService)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _jwtService = jwtService;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RequestResult<SignInResponse>> Handle(RefreshTokenCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            // Find the user with status is not inactive
            var user = await _userReadWriteRepository.GetUserByUserIdAsync(userId, cancellationToken);
            if (user == null)
                return RequestResult<SignInResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(userId),
                        Error = "Tài khoản không tìm thấy, không hoạt động hoặc đã bị xóa"
                    }
                });

            var getUserToken = await _userTokenReadWriteRepository.GetUserTokenAsync(userId, request.RefreshToken, cancellationToken);
            if (getUserToken == null)
                return RequestResult<SignInResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.RefreshToken),
                        Error = "Refresh token không hợp lệ"
                    }
                });

            var claimsIdentity = BuildClaimsIdentity(user);
            var tokenResponse = await _jwtService.GenerateJwtAsync(user.Id, claimsIdentity, cancellationToken);
            getUserToken.AccessToken = tokenResponse.Token;
            await _userTokenReadWriteRepository.UpdateAsync(getUserToken, cancellationToken);
            await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<SignInResponse>.Succeed(new SignInResponse()
            {
                AccessToken = tokenResponse.Token,
                RefreshToken = getUserToken.RefreshToken,
                Role = user.RoleName,
                UserName = user.UserName
            });
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }

    private static ClaimsIdentity BuildClaimsIdentity(UserEntity user)
    {
        var claims = new List<Claim>();

        claims.Add(new Claim(JwtClaimTypes.Id, user.Id.ToString()));
        claims.Add(new Claim(JwtClaimTypes.UserName, user.UserName));
        claims.Add(new Claim(JwtClaimTypes.Role, user.RoleName));

        var claimsIdentity = new ClaimsIdentity(claims);
        return claimsIdentity;
    }
}