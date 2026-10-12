using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class GcsCutOffCycleReadWriteRepository : RestfulRepository<GcsCutoffCycleEntity, AppReadWriteDbContext>, IGcsCutOffCycleReadWriteRepository
    {
        private readonly DbSet<GcsCutoffCycleEntity> _gcsCutoffCycleEntities;

        public GcsCutOffCycleReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _gcsCutoffCycleEntities = applicationDbContext.Set<GcsCutoffCycleEntity>();
        }

        public async Task<GcsCutoffCycleEntity?> GetGcsCutOffCycleEntityAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            return await _gcsCutoffCycleEntities
                .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<GcsCutoffCycleEntity?> GetMaxGcsCutOffCycleAsync(long gcsId, CancellationToken cancellationToken)
        {
            return await _gcsCutoffCycleEntities.AsNoTracking()
                .Where(x => x.GcsId == gcsId)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ThenByDescending(x => x.PeriodNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
