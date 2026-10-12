using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class GcsMonthlyDetailReadWriteRepository : RestfulRepository<GcsMonthlyDetailEntity, AppReadWriteDbContext>, IGcsMonthlyDetailReadWriteRepository
    {
        private readonly DbSet<GcsMonthlyDetailEntity> _gcsMonthlyDetailEntities;

        public GcsMonthlyDetailReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _gcsMonthlyDetailEntities = applicationDbContext.Set<GcsMonthlyDetailEntity>();
        }
    }
}
