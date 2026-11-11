using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadWrite;
using Application.Services;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Domain.Entities;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class DeleteMeasurementPointsCommandHandler : IDeleteMeasurementPointsCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;

    public DeleteMeasurementPointsCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, ITagRoutingCacheService tagRoutingCacheService)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _tagRoutingCacheService = tagRoutingCacheService;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(DeleteMeasurementPointsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var measurementPoint = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(command.Id, cancellationToken);
            if (measurementPoint == null)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy điểm đo."
                    }
                });
            }

            if (measurementPoint.Status == EntityStatus.Active)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động."
                    }
                });
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!string.IsNullOrEmpty(measurementPoint.Code) && !measurementPoint.Code.Contains("_deleted_"))
            {
                measurementPoint.Code = $"{measurementPoint.Code}_deleted_{timestamp}";
            }
            measurementPoint.Deleted = true;
            measurementPoint.Status = EntityStatus.Deleted;
            measurementPoint.DeletedBy = currentId;
            measurementPoint.DeletedTime = DateTimeOffset.Now;
            measurementPoint.DeletedName = userName;
            await _measurementPointsReadWriteRepository.UpdateAsync(measurementPoint, cancellationToken);

            // Save all changes
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            _tagRoutingCacheService.Invalidate();
            return RequestResult<MeasurementPointResponse>.Succeed(new MeasurementPointResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}