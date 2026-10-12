using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class ArchiveDataMeasurementPointMonitoringReadWriteRepository : RestfulRepository<ArchiveDataMeasurementPointMonitoringEntity, AppReadWriteDbContext>, IArchiveDataMeasurementPointMonitoringReadWriteRepository
    {
        private readonly DbSet<ArchiveDataMeasurementPointMonitoringEntity> _archiveDataMeasurementPointMonitoringEntities;
        public ArchiveDataMeasurementPointMonitoringReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _archiveDataMeasurementPointMonitoringEntities = applicationDbContext.Set<ArchiveDataMeasurementPointMonitoringEntity>();
        }
    }
}
