using System.Net.Mail;
using Application.Commands.Users;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Users;
using Application.Handlers.Commands.Users;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Nobisoft.Core.Mailer.Interfaces;
using Nobisoft.Core.Mailer.Templates;

namespace Infrastructure.Handlers.Commands.Users;

public class AddUserCommandHandler : ICreateUserCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IEmailService _emailService;
    private readonly IMailingService _mailingService;
    private readonly IConfiguration _configuration;

    public AddUserCommandHandler(ISnowflakeIdService snowflakeIdService, IPasswordGeneratorService passwordGeneratorService, IUserReadWriteRepository userReadWriteRepository, ILoggerService loggerService, ICurrentUserService currentUserService, IUserReadOnlyRepository userReadOnlyRepository, IEmailService emailService, IMailingService mailingService, IConfiguration configuration)
    {
        _snowflakeIdService = snowflakeIdService;
        _passwordGeneratorService = passwordGeneratorService;
        _userReadWriteRepository = userReadWriteRepository;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _emailService = emailService;
        _mailingService = mailingService;
        _configuration = configuration;
    }

    public async Task<RequestResult<CreateUserResponse>> Handle(CreateUserCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;
            var password = _passwordGeneratorService.GenerateRandomPassword();

            var checkDuplicateEmail = await _userReadOnlyRepository.IsDuplicateEmail(request.Email, null, cancellationToken);
            if (checkDuplicateEmail)
            {
                return RequestResult<CreateUserResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Email),
                        Error = "Email đã tồn tại trong hệ thống. Vui lòng thử lại!."
                    }
                });
            }

            var checkDuplicateUserName = await _userReadOnlyRepository.IsDuplicateUserName(request.UserName, null, cancellationToken);
            if (checkDuplicateUserName)
            {
                return RequestResult<CreateUserResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.UserName),
                        Error = "Tên đăng nhập đã tồn tại trong hệ thống. Vui lòng thử lại!."
                    }
                });
            }

            var userId = await _snowflakeIdService.GenerateId(cancellationToken);
            var response = new UserEntity()
            {
                Id = userId,
                UserName = request.UserName,
                Password = _passwordGeneratorService.HashPassword(password),
                Description = request.Description,
                CustomerCode = request.CustomerCode,
                RoleName = request.RoleName,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Status = request.Status,
                CreatedBy = currentId,
                CreatedName = userName,
                CreatedTime = DateTimeOffset.UtcNow,
                ModifiedBy = currentId,
                ModifiedName = userName,
                ModifiedTime = DateTimeOffset.UtcNow
            };
            await _mailingService.SendEmailAsync(new Email()
            {
                To = new List<MailAddress> { new MailAddress(request.Email) } ,
                Subject = "Thông tin Đăng nhập Tài khoản Hệ thống AMR",
                Body = $@"<html>
                    <body>
                        <p>Kính gửi {request.FullName},</p>
                        <p></p>
                        <p>Chúng tôi xin thông báo rằng tài khoản của bạn trong hệ thống AMR đã được tạo thành công. Dưới đây là thông tin đăng nhập của bạn</p>
                        <p><strong>Tên đăng nhập: {request.UserName}</strong></p>
                        <p><strong>Mật khẩu: {password}</strong></p>
                        <p>Liên kết đăng nhập: {_configuration["FrontEndUrl"]}/login</p>
                        <p></p>
                        <p>Nếu bạn gặp bất kỳ vấn đề gì trong quá trình đăng nhập, vui lòng liên hệ với bộ phận hỗ trợ để được giáp đáp thắc mắc.</p>
                        <p></p>
                        <p>Trân trọng.</p>
                    </body>
                </html>"
            }, cancellationToken);
            await _userReadWriteRepository.AddAsync(response, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<CreateUserResponse>.Succeed(new CreateUserResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}
