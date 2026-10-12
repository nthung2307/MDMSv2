using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Queries.PeriodicEnergyStatement;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.PeriodicEnergyStatement;

public class PeriodicEnergyStatementQueryHandler :
    IRequestHandler<GetListPeriodicEnergyStatementQuery, CursorPaginationResponse<PeriodicEnergyStatementDto>>
{
    private readonly IPeriodicEnergyStatementReadOnlyRepository _periodicEnergyStatementReadOnlyRepository;

    public PeriodicEnergyStatementQueryHandler(IPeriodicEnergyStatementReadOnlyRepository periodicEnergyStatementReadOnlyRepository)
    {
        _periodicEnergyStatementReadOnlyRepository = periodicEnergyStatementReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<PeriodicEnergyStatementDto>> Handle(GetListPeriodicEnergyStatementQuery request, CancellationToken cancellationToken)
    {
        return await _periodicEnergyStatementReadOnlyRepository.GetListPeriodicEnergyStatementAsync(request.Request, cancellationToken);
    }
}