using Application.Commands.SystemConfig;
using Application.DataTransferObjects.Requests.SystemConfig;
using Application.DataTransferObjects.Responses.SystemConfig;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class SystemConfigProfile : Profile
{
    public SystemConfigProfile()
    {
        CreateMap<CreateUpdateSystemConfigRequest, CreateUpdateSystemConfigCommands>();
        CreateMap<SystemConfigEntity, SystemConfigDto>();
    }
}