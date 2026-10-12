using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Gcs;

public class UpdateGcsTemporaryCommand : CommandBase, IRequest<RequestResult<GcsResponse>>
{
    public List<UpdateGcsTemporaryRequest> Request { get; set; }
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
}