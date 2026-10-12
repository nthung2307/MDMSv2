using Domain.Entities;
using Newtonsoft.Json;

namespace Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

public class MeasurementPointMonitoringHourlyRawDto
{
    public long Id { get; set; }
    public long MeasurementPointsId { get; set; }
    public long TagNameId { get; set; }
    public TagNameEntity TagName { get; set; }
    public string TagNameDescription { get; set; }
    public string Value { get; set; }
    public long? GroupId { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}
