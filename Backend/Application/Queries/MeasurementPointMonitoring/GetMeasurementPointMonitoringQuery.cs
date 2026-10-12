using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPointMonitoring;

public class GetMeasurementPointMonitoringQuery : QueryBase, IRequest<MeasurementPointMonitoringDto?>
{
    public string TagName { get; set; }
}