using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class ImportMeasurementPointsCommandHandler : IImportMeasurementPointsCommandHandler
{
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;

    public ImportMeasurementPointsCommandHandler(
        IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository,
        ICurrentUserService currentUserService,
        ILoggerService loggerService)
    {
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _currentUserService = currentUserService;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<ImportMeasurementPointResponse>> Handle(ImportMeasurementPointsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserName = _currentUserService.UserName ?? "System";

            var result = await _measurementPointsReadWriteRepository.ImportMeasurementPointsFromCsvAsync(
                request.FileStream,
                currentUserId,
                currentUserName,
                cancellationToken);

            return RequestResult<ImportMeasurementPointResponse>.Succeed(result);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(ImportMeasurementPointsCommandHandler));
            return RequestResult<ImportMeasurementPointResponse>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "file",
                    Error = $"Có lỗi xảy ra khi nhập tệp CSV điểm đo: {ex.Message}"
                }
            });
        }
    }
}
