using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Auth;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class ForgotPasswordCommandHandler : IForgotPasswordCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;
    private readonly IJwtService _jwtService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        ILoggerService loggerService,
        IUserReadWriteRepository userReadWriteRepository,
        IUserTokenReadWriteRepository userTokenReadWriteRepository,
        IJwtService jwtService,
        ISnowflakeIdService snowflakeIdService,
        IEmailService emailService,
        ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository,
        IConfiguration configuration)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
        _jwtService = jwtService;
        _snowflakeIdService = snowflakeIdService;
        _emailService = emailService;
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
        _configuration = configuration;
    }

    public async Task<RequestResult<ForgotPasswordResponse>> Handle(ForgotPasswordCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userReadWriteRepository.GetUserByEmailAsync(request.Request.Email, cancellationToken);
            if (user == null)
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Email),
                        Error = "Email không tồn tại trên hệ thống"
                    }
                });

            if (user.IsLocked && user.TimeLocked >= DateTimeOffset.Now)
            {
                var systemConfig = await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Email),
                        Error = $"Tài khoản của bạn đã bị khóa. Vui lòng liên hệ {systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationFullname).Value} qua số điện thoại {systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationPhone).Value}, {systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationEmail).Value}"
                    }
                });
            }

            var claimsIdentity = BuildClaimsIdentity(user);
            var tokenResponse = await _jwtService.GenerateJwtAsync(user.Id, claimsIdentity, cancellationToken);
            var userToken = new UserTokenEntity()
            {
                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                UserId = user.Id,
                AccessToken = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiredTime = DateTimeOffset.Now.AddMinutes(10),
                Type = UserTokenType.ForgotPassword
            };

            try
            {
                await _emailService.SendEmailForgotPassword(user.Email, "Yêu cầu đặt lại mật khẩu - Hệ thống MDMS", tokenResponse.Token, user.UserName);
            }
            catch (Exception mailEx)
            {
                _loggerService.LogError(mailEx, "Gửi email đặt lại mật khẩu thất bại qua Email Provider");
                return RequestResult<ForgotPasswordResponse>.Fail(new[]
                {
                    new ErrorItem
                    {
                        FieldName = "Email",
                        Error = $"Không thể gửi email đặt lại mật khẩu: {mailEx.Message}. Vui lòng kiểm tra lại cấu hình email hệ thống."
                    }
                });
            }

            await _userTokenReadWriteRepository.AddAsync(userToken, cancellationToken);
            await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);

            return RequestResult<ForgotPasswordResponse>.Succeed(new ForgotPasswordResponse());
        } catch (Exception e)
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

        var claimsIdentity = new ClaimsIdentity(claims);
        return claimsIdentity;
    }
}
