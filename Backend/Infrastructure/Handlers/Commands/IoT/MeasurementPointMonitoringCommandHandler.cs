using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.IoT;
using Application.Handlers.Commands.IoT;
using Application.Services;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointMonitoringCommandHandler : IMeasurementPointMonitoringCommandHandlers
{
    private readonly IMeterTelemetryIngestionService _ingestionService;
    private readonly ILoggerService _loggerService;

    public MeasurementPointMonitoringCommandHandler(
        IMeterTelemetryIngestionService ingestionService,
        ILoggerService loggerService)
    {
        _ingestionService = ingestionService;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(
        MeasurementPointMonitoringCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _ingestionService.IngestAsync(
                request.Request,
                request.MeterSerial,
                TelemetrySourceKind.LegacyMonitoring,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(MeasurementPointMonitoringCommandHandler));
            throw;
        }
    }
}
