using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Users;

public class GetUserQuery : QueryBase, IRequest<UserDto>
{
    public long Id { get; set; }
}