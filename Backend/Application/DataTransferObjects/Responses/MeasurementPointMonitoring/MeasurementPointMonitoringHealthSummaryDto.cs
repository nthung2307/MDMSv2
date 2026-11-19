using System;

namespace Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

/// <summary>
/// Summary statistics for the connection health widget on the Summary Parameters page.
/// </summary>
public class MeasurementPointMonitoringHealthSummaryDto
{
    public int TotalPoints { get; set; }
    public int GoodPoints { get; set; }
    public int ModbusLostPoints { get; set; }
    public int RestStalePoints { get; set; }
    public int BadQualityPoints { get; set; }
    public int RestInactivityTimeoutMinutes { get; set; }
    public DateTimeOffset EvaluatedAt { get; set; }
}

