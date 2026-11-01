using System.Threading;
using System.Threading.Tasks;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class SystemEmailConfigReadWriteRepository : RestfulRepository<SystemEmailConfigEntity, AppReadWriteDbContext>,
    ISystemEmailConfigReadWriteRepository
{
    private readonly DbSet<SystemEmailConfigEntity> _entities;
    private readonly AppReadWriteDbContext _context;

    public SystemEmailConfigReadWriteRepository(AppReadWriteDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<SystemEmailConfigEntity>();
        _context = applicationDbContext;
    }

    public async Task<SystemEmailConfigEntity?> GetActiveConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _entities.FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureTableCreatedAsync(cancellationToken);
            return await _entities.FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
        }
    }

    public async Task EnsureTableCreatedAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS `SystemEmailConfigs` (
  `Id` bigint NOT NULL,
  `ProviderType` varchar(50) NOT NULL,
  `SenderEmail` varchar(255) NOT NULL,
  `SenderDisplayName` varchar(255) DEFAULT NULL,
  `TenantId` varchar(100) DEFAULT NULL,
  `ClientId` varchar(100) DEFAULT NULL,
  `EncryptedClientSecret` varchar(1000) DEFAULT NULL,
  `SmtpHost` varchar(255) DEFAULT NULL,
  `SmtpPort` int DEFAULT NULL,
  `SecurityMode` varchar(50) DEFAULT NULL,
  `RequireAuth` tinyint(1) NOT NULL DEFAULT '1',
  `SmtpUsername` varchar(255) DEFAULT NULL,
  `EncryptedSmtpPassword` varchar(1000) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
