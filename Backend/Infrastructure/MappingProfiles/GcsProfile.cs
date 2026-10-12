using Application.Commands.Gcs;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.IoT;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class GcsProfile : Profile
{
    public GcsProfile()
    {
        CreateMap<CreateGcsRequest, CreateGcsCommand>();
        CreateMap<UpdateGcsRequest, UpdateGcsCommand>();
        CreateMap<GcsEntity, GcsDto>();
        CreateMap<GcsTemporaryEntity, GetDataLatchResponse>();
    }
}