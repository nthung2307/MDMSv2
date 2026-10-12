using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class ChangeInformationAccountCommandHandler : IChangeInformationAccountCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;

    public ChangeInformationAccountCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, ICurrentUserService currentUserService, IUserReadOnlyRepository userReadOnlyRepository)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _currentUserService = currentUserService;
        _userReadOnlyRepository = userReadOnlyRepository;
    }

    public async Task<RequestResult<ChangeInformationAccountResponse>> Handle(ChangeInformationAccountCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            // Find the user with status is not inactive
            var user = await _userReadWriteRepository.GetUserByUserIdAsync(currentId, cancellationToken);
            if (user == null)
                return RequestResult<ChangeInformationAccountResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(currentId),
                        Error = $"Tài khoản không tìm thấy, không hoạt động hoặc đã bị xóa"
                    }
                });

            if (request.Request.Email != null)
            {
                var checkDuplicateEmail = await _userReadOnlyRepository.IsDuplicateEmail(request.Request.Email, currentId, cancellationToken);
                if (checkDuplicateEmail)
                {
                    return RequestResult<ChangeInformationAccountResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(request.Request.Email),
                            Error = "Email đã tồn tại."
                        }
                    });
                }
            }

            user.FullName = request.Request.FullName;
            user.PhoneNumber = request.Request.PhoneNumber;
            user.Email = request.Request.Email;
            user.Description = request.Request.Description;
            user.ModifiedBy = currentId;
            user.ModifiedName = user.UserName;
            user.ModifiedTime = DateTimeOffset.Now;
            await _userReadWriteRepository.UpdateAsync(user, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);

            return RequestResult<ChangeInformationAccountResponse>.Succeed(new ChangeInformationAccountResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}