using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Queries.PeriodicEnergyStatement;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.PeriodicEnergyStatement;

public class CheckStatusSendMailQueryHandler :
    IRequestHandler<CheckStatusSendMailQuery, bool>
{
    private readonly IPeriodicEnergyStatementReadOnlyRepository _periodicEnergyStatementReadOnlyRepository;
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;

    public CheckStatusSendMailQueryHandler(IPeriodicEnergyStatementReadOnlyRepository periodicEnergyStatementReadOnlyRepository, IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository)
    {
        _periodicEnergyStatementReadOnlyRepository = periodicEnergyStatementReadOnlyRepository;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
    }
    public async Task<bool> Handle(CheckStatusSendMailQuery request, CancellationToken cancellationToken)
    {
        return await _gcsCutOffCycleReadOnlyRepository.IsSendingMail(request.Request, cancellationToken);
    }
}