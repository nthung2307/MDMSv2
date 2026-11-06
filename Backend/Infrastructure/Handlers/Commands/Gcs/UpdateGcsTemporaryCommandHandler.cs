using Application.Commands.Gcs;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Handlers.Commands.Gcs;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Gcs;

public class UpdateGcsTemporaryCommandHandler : IUpdateGcsTemporaryCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsTemporaryReadWriteRepository _gcsTemporaryReadWriteRepository;
    private readonly IGcsReadOnlyRepository _gcsReadOnlyRepository;

    public UpdateGcsTemporaryCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, IGcsTemporaryReadWriteRepository gcsTemporaryReadWriteRepository, IGcsReadOnlyRepository gcsReadOnlyRepository)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _gcsTemporaryReadWriteRepository = gcsTemporaryReadWriteRepository;
        _gcsReadOnlyRepository = gcsReadOnlyRepository;
    }

    public async Task<RequestResult<GcsResponse>> Handle(UpdateGcsTemporaryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var existGcs = await _gcsReadOnlyRepository.IsExistGcsAsync(command.GcsId, cancellationToken);
            if (existGcs == null)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = LocalizationString.Common.UnknownFieldName,
                        Error = "Không tìm thấy sổ ghi chốt."
                    }
                });
            }

            if (command.Request.Count > 0)
            {
                var gcsTemporary = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryToUpdateAsync(command.Request, command.GcsId, command.Month, command.Year, command.PeriodNumber, cancellationToken);
                var affectedPointIds = new HashSet<long>();

                foreach (var item in gcsTemporary)
                {
                    affectedPointIds.Add(item.MeasurementPointId);
                    var requestItem = command.Request.FirstOrDefault(x => x.Id == item.Id);
                    var newCurrentReading = requestItem?.NewCurrentReading;
                    var newPreviousReading = requestItem?.NewPreviousReading;

                    var isPMax = item.MeterReadingSet == "Pmax";
                    if (newCurrentReading.HasValue)
                    {
                        item.NewCurrentReading = newCurrentReading;
                    }

                    if (newPreviousReading.HasValue)
                    {
                        item.NewPreviousReading = newPreviousReading;
                    }

                    var currentReadingValue = item.NewCurrentReading ?? item.CurrentReading ?? 0;
                    var previousReadingValue = item.NewPreviousReading ?? item.PreviousReading ?? 0;

                    if (isPMax)
                    {
                        item.Consumption = null;
                        item.IsSuspicious = false;
                        item.SuspiciousReason = null;
                    }
                    else
                    {
                        item.Consumption = (currentReadingValue - previousReadingValue) * item.MultiplierFactor;
                        if (item.Consumption < 0)
                        {
                            item.IsSuspicious = true;
                            item.SuspiciousReason = "Sản lượng âm (Chỉ số mới nhỏ hơn chỉ số cũ)";
                        }
                        else
                        {
                            item.IsSuspicious = false;
                            item.SuspiciousReason = null;
                        }
                    }

                    item.Quanlity = true;
                    if (requestItem?.IsChange.HasValue == true)
                    {
                        item.IsChange = requestItem.IsChange.Value;
                    }
                    else
                    {
                        bool isDifferentFromMeter = (newCurrentReading.HasValue && item.CurrentReading.HasValue && Math.Abs(newCurrentReading.Value - item.CurrentReading.Value) > 0.0001)
                                                 || (newPreviousReading.HasValue && item.PreviousReading.HasValue && Math.Abs(newPreviousReading.Value - item.PreviousReading.Value) > 0.0001);
                        item.IsChange = isDifferentFromMeter;
                    }

                    if (item.IsChange)
                    {
                        item.ModifiedBy = currentId;
                        item.ModifiedTime = DateTime.Now;
                        item.ModifiedName = userName;
                    }
                    else
                    {
                        item.ModifiedBy = null;
                        item.ModifiedTime = null;
                        item.ModifiedName = null;
                    }
                    await _gcsTemporaryReadWriteRepository.UpdateAsync(item, cancellationToken);
                }

                await _gcsTemporaryReadWriteRepository.SaveChangesAsync(cancellationToken);

                // Tính toán lại Sản lượng và hệ số CosPhi (PF) trung bình cho các điểm đo vừa được cập nhật
                if (affectedPointIds.Count > 0)
                {
                    var allCycleRecords = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryWithGcsAsync(command.GcsId, command.Month, command.Year, command.PeriodNumber, cancellationToken);
                    var recordsByPoint = allCycleRecords.Where(x => affectedPointIds.Contains(x.MeasurementPointId)).GroupBy(x => x.MeasurementPointId);
                    foreach (var pointGroup in recordsByPoint)
                    {
                        Infrastructure.Repositories.ReadWrite.GcsTemporaryReadWriteRepository.RecalculatePointConsumptionAndCosPhi(pointGroup);
                        foreach (var rec in pointGroup)
                        {
                            await _gcsTemporaryReadWriteRepository.UpdateAsync(rec, cancellationToken);
                        }
                    }
                    await _gcsTemporaryReadWriteRepository.SaveChangesAsync(cancellationToken);
                }
            }

            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}