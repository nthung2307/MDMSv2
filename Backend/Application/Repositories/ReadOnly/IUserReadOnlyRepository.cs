using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IUserReadOnlyRepository : IRepository<UserEntity>
{
    Task<List<string>> GetPermissionsByUserNameAsync(string userName,CancellationToken cancellationToken = default);
    Task<CursorPaginationResponse<UserDto>> GetListAccountAsync(GetListUserRequest request, CancellationToken cancellationToken);
    Task<UserDto> GetUserDetailAsync(long id, CancellationToken cancellationToken);
    Task<bool> IsExistUsersAsync(List<long> ids, CancellationToken cancellationToken);
    Task<bool> IsDuplicateEmail(string email, long? id, CancellationToken cancellationToken);
    Task<bool> IsDuplicateUserName(string userName, long? id, CancellationToken cancellationToken);
}