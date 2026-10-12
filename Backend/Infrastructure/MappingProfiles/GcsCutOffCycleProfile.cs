using Application.Commands.Gcs;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.IoT;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class GcsCutOffCycleProfile : Profile
{
    public GcsCutOffCycleProfile()
    {
        CreateMap<GcsCutoffCycleEntity, GcsCutOffCycleResponse>();
    }
}