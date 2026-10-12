using Application.Commands.Gcs;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Handlers.Commands.Gcs;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Gcs;

public class UpdateGcsCommandHandler : IUpdateGcsCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;

    public UpdateGcsCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, IGcsReadWriteRepository gcsReadWriteRepository, IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _gcsReadWriteRepository = gcsReadWriteRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
    }

    public async Task<RequestResult<GcsResponse>> Handle(UpdateGcsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var gcs = await _gcsReadWriteRepository.GetGcsByIdAsync(command.Request.Id, cancellationToken);
            if (gcs == null)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.Id),
                        Error = "Không tìm thấy sổ ghi chốt."
                    }
                });
            }

            if (command.Request.Status == EntityStatus.Deactivate)
            {
                if (gcs.MeasurementPoints.Count > 0 && gcs.MeasurementPoints.Any(mp => mp.Status == EntityStatus.Active))
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(gcs.MeasurementPoints),
                            Error = "Đang có điểm đo hoạt động."
                        }
                    });
                }
            }


            var isExistGcsCode = await _gcsReadWriteRepository.IsExistGcsCodeAsync(command.Request.Id, command.Request.Code, cancellationToken);
            if (isExistGcsCode)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.Id),
                        Error = "Mã sổ đã tồn tại. Vui lòng thử lại"
                    }
                });
            }

            if (command.Request.DayPeriod.Any(x => (x <= 0 || x > 28) && x != 999))
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.DayPeriod),
                        Error = "Ngày ghi chỉ số không được lớn hơn 28"
                    }
                });
            }

            if (command.Request.DayPeriod.Count != command.Request.PeriodNumber)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Request.Id),
                        Error = "Vui lòng nhập ngày chốt đúng với số ngày chốt."
                    }
                });
            }

            for (int i = 0; i < command.Request.DayPeriod.Count - 1; i++)
            {
                var has28 = command.Request.DayPeriod.Contains(28);
                var has999 = command.Request.DayPeriod.Contains(999);

                if (has28 && has999)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.DayPeriod),
                            Error = "Không thể thêm kì chốt cuối tháng khi đã có kì chốt ngày 28."
                        }
                    });
                }

                // If 999 is present, it must be the last element
                if (command.Request.DayPeriod[i] == 999)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.DayPeriod),
                            Error = "Ngày cuối tháng chỉ được đặt ở vị trí cuối cùng."
                        }
                    });
                }

                // Ensure days are strictly increasing (except for the last 0)
                if (command.Request.DayPeriod[i] >= command.Request.DayPeriod[i + 1] && command.Request.DayPeriod[i + 1] != 0)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.DayPeriod),
                            Error = "Ngày của kì sau phải lớn hơn kì trước."
                        }
                    });
                }
            }

            var gcsCutOffEntity = await _gcsCutOffCycleReadWriteRepository.GetMaxGcsCutOffCycleAsync(command.Request.Id, cancellationToken);
            if (gcs.PeriodNumber != command.Request.PeriodNumber || gcs.DayPeriod != command.Request.DayPeriod)
            {
                if (gcsCutOffEntity != null && gcsCutOffEntity.PeriodNumber != gcs.PeriodNumber)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Request.DayPeriod),
                            Error = "Hoàn tất các kì chốt của tháng trước khi thay đổi kì chốt"
                        }
                    });
                }
            }

            if (command.Request.PeriodNumber != gcs.PeriodNumber)
                gcs.IsNewChange = true;

            gcs.Code = command.Request.Code;
            gcs.Name = command.Request.Name;
            gcs.PeriodNumber = command.Request.PeriodNumber;
            gcs.Description = command.Request.Description;
            gcs.Status = command.Request.Status;
            gcs.DayPeriod = command.Request.DayPeriod;
            gcs.ModifiedName = userName;
            gcs.ModifiedTime = DateTimeOffset.Now;
            gcs.ModifiedBy = currentId;
            await _gcsReadWriteRepository.UpdateAsync(gcs, cancellationToken);
            await _gcsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}