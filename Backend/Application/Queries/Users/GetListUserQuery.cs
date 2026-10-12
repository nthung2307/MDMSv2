using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Users;

public class GetListUserQuery : QueryBase, IRequest<CursorPaginationResponse<UserDto>>
{
    public GetListUserRequest Request { get; set; }
}