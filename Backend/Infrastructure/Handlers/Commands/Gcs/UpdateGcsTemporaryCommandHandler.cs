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
                foreach (var item in gcsTemporary)
                {
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
                    }
                    else
                    {
                        item.Consumption = (currentReadingValue - previousReadingValue) * item.MultiplierFactor;
                    }

                    item.ModifiedBy = currentId;
                    item.ModifiedTime = DateTime.Now;
                    item.ModifiedName = userName;
                    await _gcsTemporaryReadWriteRepository.UpdateAsync(item, cancellationToken);
                }

                await _gcsTemporaryReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}