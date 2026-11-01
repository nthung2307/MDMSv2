using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface INotificationTemplateReadWriteRepository : IRepository<NotificationTemplateEntity>
{
    Task<NotificationTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<NotificationTemplateEntity?> GetDefaultByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(long id, string code, CancellationToken cancellationToken = default);
}
