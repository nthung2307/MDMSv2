using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IStatementPdfTemplateReadWriteRepository : IRepository<StatementPdfTemplateEntity>
{
    Task<StatementPdfTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<StatementPdfTemplateEntity?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task SetDefaultAsync(long id, CancellationToken cancellationToken = default);
}
