using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPoints;

public class GetMeasurementPointDetailQuery : QueryBase, IRequest<MeasurementPointDto>
{
    public long Id { get; set; }
}