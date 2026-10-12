using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class UpdateMeasurementPointsCommandHandler : IUpdateMeasurementPointsCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;
    private readonly IGcsReadOnlyRepository _gcsReadOnlyRepository;

    public UpdateMeasurementPointsCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository, IGcsReadOnlyRepository gcsReadOnlyRepository)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
        _gcsReadOnlyRepository = gcsReadOnlyRepository;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(UpdateMeasurementPointsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var measurementPoint = await _measurementPointsReadWriteRepository.GetMeasurementPointForUpdateByIdAsync(command.Request.Id, cancellationToken);
            if (measurementPoint == null)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.Id),
                        Error = "Không tìm thấy điểm đo."
                    }
                });
            }

            if (command.Request.Status == EntityStatus.Active)
            {
                if (measurementPoint.Gcs.Status != EntityStatus.Active || measurementPoint.Customer.Status != EntityStatus.Active)
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.Id),
                            Error = "Khách hàng phải đang hoạt động và sổ cài đặt phải đang mở."
                        }
                    });
                }
            }

            var isExistCustomerCode = await _measurementPointsReadOnlyRepository.IsExistMeasurementPointNameAsync(command.Request.Code, command.Request.Id, cancellationToken);
            if (isExistCustomerCode)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.Code),
                        Error = "Mã điểm đo đã tồn tại trong hệ thống. Vui lòng thử lại!"
                    }
                });
            }

            var gcs = await _gcsReadOnlyRepository.GetGcsAsync(measurementPoint.GcsId ?? 0, cancellationToken);
            if (gcs == null)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.GcsId),
                        Error = "Không tìm thấy sổ ghi chốt"
                    }
                });
            }

            var gcsCutOffEntity = await _gcsCutOffCycleReadWriteRepository.GetMaxGcsCutOffCycleAsync(measurementPoint.GcsId ?? 0, cancellationToken);
            if (measurementPoint.GcsId != command.Request.GcsId)
            {
                if (gcsCutOffEntity != null && gcsCutOffEntity.PeriodNumber != gcs.PeriodNumber)
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.GcsId),
                            Error = "Hoàn tất các kì chốt của tháng trước khi chuyển sổ"
                        }
                    });
                }
                var gcsNewCutOffEntity = await _gcsCutOffCycleReadWriteRepository.GetMaxGcsCutOffCycleAsync(command.Request.GcsId ?? 0, cancellationToken);
                var gcsNew = await _gcsReadOnlyRepository.GetGcsAsync(command.Request.GcsId ?? 0, cancellationToken);
                if (gcsNew == null)
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.GcsId),
                            Error = "Không tìm thấy sổ ghi chốt mới"
                        }
                    });
                }
                if (gcsNewCutOffEntity != null && gcsNewCutOffEntity.PeriodNumber != gcsNew.PeriodNumber)
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.GcsId),
                            Error = "Hoàn tất các kì chốt của tháng trước khi chuyển sổ"
                        }
                    });
                }

                var gcsYear = gcsCutOffEntity?.Year ?? gcs.CreatedTime.Value.Year;
                var gcsNewYear = gcsNewCutOffEntity?.Year ?? gcsNew.CreatedTime.Value.Year;
                var gcsMonth = gcsCutOffEntity?.Month ?? gcs.CreatedTime.Value.AddMonths(-1).Month;
                var gcsNewMonth = gcsNewCutOffEntity?.Month ?? gcsNew.CreatedTime.Value.AddMonths(-1).Month;
                if (gcsYear == gcsNewYear)
                {
                    if (gcsMonth != gcsNewMonth)
                    {
                        return RequestResult<MeasurementPointResponse>.Fail(new[]
                        {
                            new ErrorItem()
                            {
                                FieldName = nameof(command.Request.GcsId),
                                Error = "Dữ liệu đã chốt của điểm đo và sổ cài đặt chưa đồng bộ"
                            }
                        });
                    }
                }
                else
                {
                    return RequestResult<MeasurementPointResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.GcsId),
                            Error = "Dữ liệu đã chốt của điểm đo và sổ cài đặt chưa đồng bộ"
                        }
                    });
                }
            }

            measurementPoint.OldGcsId = measurementPoint.GcsId;
            measurementPoint.IsNewChange = true;
            measurementPoint.Code = command.Request.Code;
            measurementPoint.Type = command.Request.Type;
            measurementPoint.Address = command.Request.Address;
            measurementPoint.Description = command.Request.Description;
            measurementPoint.CustomerId = command.Request.CustomerId;
            measurementPoint.GcsId = command.Request.GcsId;
            measurementPoint.MultiplierFactor = command.Request.MultiplierFactor;
            measurementPoint.Status = command.Request.Status;
            measurementPoint.ModifiedName = userName;
            measurementPoint.ModifiedTime = DateTimeOffset.Now;
            measurementPoint.ModifiedBy = currentId;
            await _measurementPointsReadWriteRepository.UpdateAsync(measurementPoint, cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<MeasurementPointResponse>.Succeed(new MeasurementPointResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}