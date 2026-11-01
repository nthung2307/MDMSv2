using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface INotificationTemplateReadOnlyRepository : IRepository<NotificationTemplateEntity>
{
    Task<List<NotificationTemplateDto>> GetAllTemplatesAsync(string? code = null, CancellationToken cancellationToken = default);
    Task<NotificationTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<NotificationTemplateEntity?> GetDefaultByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<NotificationTemplateEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
