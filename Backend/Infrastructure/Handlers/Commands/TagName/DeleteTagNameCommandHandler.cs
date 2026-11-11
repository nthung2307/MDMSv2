using Application.Commands.TagName;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.TagName;
using Application.Handlers.Commands.TagName;
using Application.Repositories.ReadWrite;
using Application.Services;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.TagName;

public class DeleteTagNameCommandHandler : IDeleteTagNameCommandHandler
{
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;

    public DeleteTagNameCommandHandler(ICurrentUserService currentUserService, ITagNameReadWriteRepository tagNameReadWriteRepository, ILoggerService loggerService, ITagRoutingCacheService tagRoutingCacheService)
    {
        _currentUserService = currentUserService;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _loggerService = loggerService;
        _tagRoutingCacheService = tagRoutingCacheService;
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

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!string.IsNullOrEmpty(profile.TagName) && !profile.TagName.Contains("_deleted_"))
            {
                profile.TagName = $"{profile.TagName}_deleted_{timestamp}";
            }
            profile.Status = EntityStatus.Deleted;
            profile.Deleted = true;
            profile.DeletedBy = currentId;
            profile.DeletedTime = DateTimeOffset.Now;
            profile.DeletedName = userName;
            
            await _tagNameReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            _tagRoutingCacheService.Invalidate();
            return RequestResult<TagNameResponse>.Succeed(new TagNameResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}