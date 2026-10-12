using System.Net.Mail;
using System.Security.Claims;
using Application.Commands.Auth;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Nobisoft.Core.Mailer.Interfaces;
using Nobisoft.Core.Mailer.Templates;

namespace Infrastructure.Handlers.Commands.Auth;

public class ForgotPasswordCommandHandler : IForgotPasswordCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;
    private readonly IJwtService _jwtService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMailingService _mailingService;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, IUserTokenReadWriteRepository userTokenReadWriteRepository, IJwtService jwtService, ISnowflakeIdService snowflakeIdService, IMailingService mailingService, ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository, IConfiguration configuration)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
        _jwtService = jwtService;
        _snowflakeIdService = snowflakeIdService;
        _mailingService = mailingService;
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
            await _mailingService.SendEmailAsync(new Email()
            {
                To = new List<MailAddress> { new MailAddress(user.Email) } ,
                Subject = "Yêu cầu đặt lại mật khẩu",
                Body = $@"<html>
                    <body>
                        <p>Kính gửi {user.UserName},</p>
                        <p></p>
                        <p>Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu cho tài khoản của bạn trên hệ thống MDMS. Nếu bạn đã yêu cầu thay đổi mật khẩu, vui lòng nhấp vào liên kết dưới đây để hoàn tất quá trình thiết lập lại:</p>
                        <p><b>{_configuration["FrontEndUrl"]}/reset-password?token={tokenResponse.Token}&email={user.Email}</b></p>
                        <p></p>
                        <p>Liên kết này sẽ hết hạn sau 10 phút, vì vậy hãy đảm bảo bạn thực hiện thao tác trước thời gian này.</p>
                        <p>Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.</p>
                        <p></p>
                        <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ.</p>
                        <p>Trân trọng,</p>
                    </body>
                </html>",
            }, cancellationToken);

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
