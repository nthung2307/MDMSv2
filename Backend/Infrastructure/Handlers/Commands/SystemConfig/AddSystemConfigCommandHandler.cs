using Application.Commands.SystemConfig;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.SystemConfig;
using Application.Handlers.Commands.SystemConfig;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.SystemConfig;

public class AddUpdateSystemConfigCommandHandler : ICreateUpdateSystemConfigCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ISystemConfigReadWriteRepository _systemConfigReadWriteRepository;
    private readonly ILoggerService _loggerService;

    public AddUpdateSystemConfigCommandHandler(
        ISnowflakeIdService snowflakeIdService,
        ISystemConfigReadWriteRepository systemConfigReadWriteRepository,
        ILoggerService loggerService)
    {
        _snowflakeIdService = snowflakeIdService;
        _systemConfigReadWriteRepository = systemConfigReadWriteRepository;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<CreateUpdateSystemConfigResponse>> Handle(CreateUpdateSystemConfigCommands request,
        CancellationToken cancellationToken)
    {
        var configs = request.Configs.ToDictionary(dto => dto.Key, dto => dto.Value);

        try
        {
            foreach (var config in configs)
            {
                var existingConfig =
                    await _systemConfigReadWriteRepository.GetConfigByKeyAsync(config.Key, cancellationToken);
                if (existingConfig != null)
                {
                    existingConfig.Value = config.Value;
                }
                else
                {
                    var userConfigId = await _snowflakeIdService.GenerateId(cancellationToken);
                    var response = new SystemConfigEntity
                    {
                        Id = userConfigId,
                        Key = config.Key,
                        Value = config.Value
                    };
                    await _systemConfigReadWriteRepository.AddAsync(response, cancellationToken);
                }
            }

            await _systemConfigReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<CreateUpdateSystemConfigResponse>.Succeed(new CreateUpdateSystemConfigResponse());
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}