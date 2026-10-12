using Application.DataTransferObjects.Responses.SystemConfig;
using Application.Queries.SystemConfig;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.SystemConfig;

public class SystemConfigQueryHandler : IRequestHandler<GetListConfigQuery, List<SystemConfigDto>>
{
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;

    public SystemConfigQueryHandler(ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository)
    {
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
    }

    public async Task<List<SystemConfigDto>> Handle(GetListConfigQuery request, CancellationToken cancellationToken)
    {
        return await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
    }
}