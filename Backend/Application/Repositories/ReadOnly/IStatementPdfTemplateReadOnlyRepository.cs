using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IStatementPdfTemplateReadOnlyRepository : IRepository<StatementPdfTemplateEntity>
{
    Task<List<StatementPdfTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<StatementPdfTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<StatementPdfTemplateEntity?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<StatementPdfTemplateEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
