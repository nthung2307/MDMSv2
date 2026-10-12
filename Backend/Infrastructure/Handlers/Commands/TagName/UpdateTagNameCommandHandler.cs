using Application.Commands.TagName;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.TagName;
using Application.Handlers.Commands.TagName;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.TagName;

public class UpdateTagNameCommandHandler : IUpdateTagNameCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;

    public UpdateTagNameCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, ITagNameReadWriteRepository tagNameReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
    }

    public async Task<RequestResult<TagNameResponse>> Handle(UpdateTagNameCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            foreach (var item in command.TagNames)
            {
                var profile = await _tagNameReadWriteRepository.GetTagNameIdAsync(item.Id, cancellationToken);
                if (profile == null)
                {
                    return RequestResult<TagNameResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(item.Id),
                            Error = "Không tìm thấy tag name."
                        }
                    });
                }
                profile.TagName = item.TagName;
                profile.Description = item.Description;
                profile.Unit = item.Unit;
                profile.Type = command.Type;
                profile.CustomName = item.CustomName;
                profile.ModifiedName = userName;
                profile.ModifiedTime = DateTimeOffset.Now;
                profile.ModifiedBy = currentId;
                await _tagNameReadWriteRepository.UpdateAsync(profile, cancellationToken);
            }

            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<TagNameResponse>.Succeed(new TagNameResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}