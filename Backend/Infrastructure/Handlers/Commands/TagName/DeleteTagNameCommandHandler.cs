using Application.Commands.TagName;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.TagName;
using Application.Handlers.Commands.TagName;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.TagName;

public class DeleteTagNameCommandHandler : IDeleteTagNameCommandHandler
{
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;


    public DeleteTagNameCommandHandler(ICurrentUserService currentUserService, ITagNameReadWriteRepository tagNameReadWriteRepository, ILoggerService loggerService)
    {
        _currentUserService = currentUserService;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<TagNameResponse>> Handle(DeleteTagNameCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _tagNameReadWriteRepository.GetTagNameIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<TagNameResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Tag name không tồn tại."
                    }
                });
            }

            profile.Status = EntityStatus.Deleted;
            profile.Deleted = true;
            profile.DeletedBy = currentId;
            profile.DeletedTime = DateTimeOffset.Now;
            profile.DeletedName = userName;
            
            await _tagNameReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<TagNameResponse>.Succeed(new TagNameResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}