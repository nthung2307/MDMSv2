using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class UserCustomerReadWriteRepository : RestfulRepository<UserCustomerEntity, AppReadWriteDbContext>, IUserCustomerReadWriteRepository
    {
        private readonly DbSet<UserCustomerEntity> _userCustomerEntities;

        public UserCustomerReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _userCustomerEntities = applicationDbContext.Set<UserCustomerEntity>();
        }

        public async Task<List<UserCustomerEntity>> GetUserCustomerByCustomerIdAsync(long id, CancellationToken cancellationToken)
        {
            return await _userCustomerEntities.AsNoTracking().Where(x => x.CustomerId == id).ToListAsync(cancellationToken);
        }

        public async Task<List<UserCustomerEntity>> GetUserCustomerByUserIdAsync(long id, CancellationToken cancellationToken)
        {
            return await _userCustomerEntities.AsNoTracking().Where(x => x.UserId == id).ToListAsync(cancellationToken);
        }
    }
}
