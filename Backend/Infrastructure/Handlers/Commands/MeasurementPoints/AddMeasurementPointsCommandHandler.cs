using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class AddMeasurementPointsCommandHandler : ICreateMeasurementPointsCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;

    public AddMeasurementPointsCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, ITagRoutingCacheService tagRoutingCacheService)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _tagRoutingCacheService = tagRoutingCacheService;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(CreateMeasurementPointsCommands request, CancellationToken cancellationToken)
    {
        try
        {
            if(request.Request.CustomerId == 0)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.CustomerId),
                        Error = "Thiếu thông tin người dùng."
                    }
                });
            }
            if(request.Request.GcsId == 0)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.CustomerId),
                        Error = "Thiếu thông tin điểm đo."
                    }
                });
            }

            var isExistCustomerCode = await _measurementPointsReadOnlyRepository.IsExistMeasurementPointNameAsync(request.Request.Code, 0, cancellationToken);
            if (isExistCustomerCode)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Code),
                        Error = "Mã điểm đo đã tồn tại trong hệ thống. Vui lòng thử lại!"
                    }
                });
            }

            // Giải phóng mã điểm đo từ các bản ghi đã xóa mềm trước đó nếu còn trùng
            var legacyDeletedPoints = await _measurementPointsReadWriteRepository.GetDeletedPointsByCodeAsync(request.Request.Code, cancellationToken);
            if (legacyDeletedPoints != null && legacyDeletedPoints.Count > 0)
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                foreach (var del in legacyDeletedPoints)
                {
                    del.Code = $"{del.Code}_deleted_{timestamp}_{del.Id}";
                    await _measurementPointsReadWriteRepository.UpdateAsync(del, cancellationToken);
                }
                await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var measurementPointId = await _snowflakeIdService.GenerateId(cancellationToken);

            var response = new MeasurementPointsEntity()
            {
                Id = measurementPointId,
                Code = request.Request.Code,
                Type = request.Request.Type,
                Address = request.Request.Address,
                Description = request.Request.Description,
                CustomerId = request.Request.CustomerId,
                GcsId = request.Request.GcsId,
                Status = request.Request.Status,
                MultiplierFactor = request.Request.MultiplierFactor,
                CreatedBy = currentId,
                CreatedName = userName,
                CreatedTime = DateTimeOffset.UtcNow,
                ModifiedBy = currentId,
                ModifiedName = userName,
                ModifiedTime = DateTimeOffset.UtcNow
            };

            await _measurementPointsReadWriteRepository.AddAsync(response, cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            _tagRoutingCacheService.Invalidate();
            return RequestResult<MeasurementPointResponse>.Succeed(new MeasurementPointResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}