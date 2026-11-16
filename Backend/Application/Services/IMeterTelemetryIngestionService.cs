using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;

namespace Application.Services;

public enum TelemetrySourceKind
{
    Unified = 0,
    LegacyMonitoring = 1,
    LegacyAlert = 2,
    LegacyDateTime = 3
}

/// <summary>
/// Unified telemetry ingestion pipeline shared by HTTP (/api/v1/iot/telemetry + legacy Kepware routes)
/// and future transports (MQTT), handling numeric telemetry, composite/legacy DateTime, and Status/Alerts.
/// </summary>
public interface IMeterTelemetryIngestionService
{
    Task<RequestResult<MeasurementPointMonitoringResponse>> IngestAsync(
        IoTDataRequest? request,
        string? meterSerial,
        TelemetrySourceKind sourceKind = TelemetrySourceKind.Unified,
        CancellationToken cancellationToken = default);
}

