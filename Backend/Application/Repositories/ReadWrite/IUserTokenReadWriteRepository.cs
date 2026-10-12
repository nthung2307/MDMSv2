using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IUserTokenReadWriteRepository : IRepository<UserTokenEntity>
{
    Task<UserTokenEntity?> GetUserTokenAsync(long userId, string refreshToken, CancellationToken cancellationToken);
    Task<List<UserTokenEntity>> GetAllUserTokenAsync(long userId, CancellationToken cancellationToken);
    Task<UserTokenEntity?> GetUserTokenToForgotPasswordAsync(long userId, string token, CancellationToken cancellationToken);
    Task<bool> CheckTokenForgotPasswordAsync(string token, CancellationToken cancellationToken);
}