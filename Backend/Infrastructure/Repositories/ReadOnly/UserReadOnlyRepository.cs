using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Users;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class UserReadOnlyRepository : RestfulRepository<UserEntity, AppReadOnlyDbContext>, IUserReadOnlyRepository
{
    private readonly DbSet<UserEntity> _userEntities;
    private readonly IMapper _mapper;

    public UserReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _userEntities = applicationDbContext.Set<UserEntity>();
    }

    public async Task<List<string>> GetPermissionsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var result = new List<string>();
        var userProfile = await _userEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
        if (userProfile == null)
            return result;

        return result.Distinct().ToList();
    }

    public async Task<CursorPaginationResponse<UserDto>> GetListAccountAsync(GetListUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userEntities
            .AsNoTracking()
            .Where(x => x.RoleName != DefaultRoles.SuperAdmin && !x.Deleted)
            .PaginateWithAndAsync<UserEntity, UserDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<UserDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<UserDto> GetUserDetailAsync(long id, CancellationToken cancellationToken)
    {
        var user = await _userEntities.AsNoTracking().Where(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> IsExistUsersAsync(List<long> ids, CancellationToken cancellationToken)
    {
        var existingUserCount = await _userEntities
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .CountAsync(cancellationToken);

        return existingUserCount == ids.Count;
    }

    public async Task<bool> IsDuplicateEmail(string email, long? id, CancellationToken cancellationToken)
    {
        if(id != null)
            return await _userEntities.AsNoTracking().AnyAsync(x => x.Email == email && x.Id != id, cancellationToken);
        return await _userEntities.AsNoTracking().AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> IsDuplicateUserName(string userName, long? id, CancellationToken cancellationToken)
    {
        if(id != null)
            return await _userEntities.AsNoTracking().AnyAsync(x => x.UserName == userName && x.Id != id, cancellationToken);
        return await _userEntities.AsNoTracking().AnyAsync(x => x.UserName == userName, cancellationToken);
    }
}