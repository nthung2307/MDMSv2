using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Domain.Entities;

namespace Application.Services;

public interface IStatementPdfGenerator
{
    Task<byte[]> GeneratePdfAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePreviewPdfAsync(PreviewStatementPdfTemplateRequest templateRequest, CancellationToken cancellationToken = default);
}
