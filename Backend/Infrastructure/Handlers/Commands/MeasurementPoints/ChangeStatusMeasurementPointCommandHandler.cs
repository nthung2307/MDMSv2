using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class ChangeStatusMeasurementPointCommandHandler : IChangeStatusMeasurementPointsCommandHandler
{
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;

    public ChangeStatusMeasurementPointCommandHandler(
        ICurrentUserService currentUserService,
        IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository,
        IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository,
        ILoggerService loggerService,
        ITagRoutingCacheService tagRoutingCacheService)
    {
        _currentUserService = currentUserService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _loggerService = loggerService;
        _tagRoutingCacheService = tagRoutingCacheService;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(ChangeStatusMeasurementPointCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(command.Id, cancellationToken);
            if (profile == null)
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

            var targetStatus = profile.Status == EntityStatus.Active ? EntityStatus.Deactivate : EntityStatus.Active;
            if (targetStatus == EntityStatus.Active && !string.IsNullOrWhiteSpace(profile.MeterSerial))
            {
                var conflictingCode = await _measurementPointsReadOnlyRepository.GetActivePointCodeByMeterSerialAsync(
                    profile.MeterSerial,
                    profile.Id,
                    cancellationToken);

                if (!string.IsNullOrEmpty(conflictingCode))
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(profile.MeterSerial),
                            Error = $"Không thể kích hoạt điểm đo vì Số chế tạo '{profile.MeterSerial.Trim()}' đang được sử dụng bởi điểm đo đang hoạt động '{conflictingCode}'."
                        }
                    });
                }
            }

            profile.Status = targetStatus;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;
            await _measurementPointsReadWriteRepository.UpdateAsync(profile, cancellationToken);
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