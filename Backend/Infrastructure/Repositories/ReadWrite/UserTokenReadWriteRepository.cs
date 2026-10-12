using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class UserTokenReadWriteRepository : RestfulRepository<UserTokenEntity, AppReadWriteDbContext>, IUserTokenReadWriteRepository
{
    private readonly DbSet<UserTokenEntity> _userTokenEntities;

    public UserTokenReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _userTokenEntities = applicationDbContext.Set<UserTokenEntity>();
    }

    public async Task<UserTokenEntity?> GetUserTokenAsync(long userId, string refreshToken, CancellationToken cancellationToken)
    {
        return await _userTokenEntities.Where(x => x.UserId == userId && x.RefreshToken == refreshToken).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserTokenEntity>> GetAllUserTokenAsync(long userId, CancellationToken cancellationToken)
    {
        return await _userTokenEntities.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<UserTokenEntity?> GetUserTokenToForgotPasswordAsync(long userId, string token, CancellationToken cancellationToken)
    {
        return await _userTokenEntities.Where(x => x.UserId == userId && x.AccessToken == token && x.Type == UserTokenType.ForgotPassword && x.ExpiredTime >= DateTimeOffset.Now).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckTokenForgotPasswordAsync(string token, CancellationToken cancellationToken)
    {
        return await _userTokenEntities.Where(x => x.AccessToken == token && x.Type == UserTokenType.ForgotPassword && x.ExpiredTime >= DateTimeOffset.Now).AnyAsync(cancellationToken);
    }
}