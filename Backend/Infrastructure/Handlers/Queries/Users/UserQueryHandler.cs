using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Users;
using Application.Queries.Users;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.Users;

public class UserQueryHandler :
    IRequestHandler<GetListUserQuery, CursorPaginationResponse<UserDto>>,
    IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;

    public UserQueryHandler(IUserReadOnlyRepository userReadOnlyRepository)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<UserDto>> Handle(GetListUserQuery request, CancellationToken cancellationToken)
    {
        return await _userReadOnlyRepository.GetListAccountAsync(request.Request, cancellationToken);
    }

    public async Task<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        return await _userReadOnlyRepository.GetUserDetailAsync(query.Id, cancellationToken);
    }
}