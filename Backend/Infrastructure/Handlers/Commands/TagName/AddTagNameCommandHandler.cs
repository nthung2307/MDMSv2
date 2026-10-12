using Application.Commands.TagName;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.TagName;
using Application.Handlers.Commands.TagName;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.TagName;

public class AddTagNameCommandHandler : ICreateTagNameCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;

    public AddTagNameCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, ITagNameReadWriteRepository tagNameReadWriteRepository, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
    }

    public async Task<RequestResult<TagNameResponse>> Handle(CreateTagNameCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var existedMeasurementPoint = await _measurementPointsReadOnlyRepository.IsExistMeasurementPointAsync(request.MeasurementPointId, cancellationToken);
            if (!existedMeasurementPoint)
            {
                return RequestResult<TagNameResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.MeasurementPointId),
                        Error = "Không tìm thấy điểm đo."
                    }
                });
            }

            var existingTagNameEntities = await _tagNameReadWriteRepository.GetByMeasurementPointIdAndTypeAsync(request.MeasurementPointId, cancellationToken);

            var existingTagNameDict = existingTagNameEntities.ToDictionary(t => t.Description, t => t);

            foreach (var item in request.TagNames)
            {
                if (existingTagNameDict.TryGetValue(item.Description, out var existingEntity))
                {
                    existingEntity.TagName = item.TagName;
                    existingEntity.Unit = item.Unit;
                    existingEntity.CustomName = item.CustomName;
                    existingEntity.ModifiedBy = currentId;
                    existingEntity.ModifiedName = userName;
                    existingEntity.ModifiedTime = DateTimeOffset.UtcNow;

                    await _tagNameReadWriteRepository.UpdateAsync(existingEntity, cancellationToken);
                }
                else
                {
                    await _tagNameReadWriteRepository.AddAsync(new TagNameEntity()
                    {
                        Id = await _snowflakeIdService.GenerateId(cancellationToken),
                        MeasurementPointId = request.MeasurementPointId,
                        TagName = item.TagName,
                        Description = item.Description,
                        Unit = item.Unit,
                        Type = item.Type,
                        CustomName = item.CustomName,
                        WarningValue = false,
                        Status = EntityStatus.Active,
                        CreatedBy = currentId,
                        CreatedName = userName,
                        CreatedTime = DateTimeOffset.UtcNow,
                        ModifiedBy = currentId,
                        ModifiedName = userName,
                        ModifiedTime = DateTimeOffset.UtcNow
                    }, cancellationToken);
                }
            }
            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<TagNameResponse>.Succeed(new TagNameResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}