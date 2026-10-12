using Application.DataTransferObjects.Responses.SystemConfig;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.SystemConfig;

public class GetListConfigQuery: QueryBase, IRequest<List<SystemConfigDto>>
{
    
}