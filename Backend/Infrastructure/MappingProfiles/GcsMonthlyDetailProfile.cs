using Application.DataTransferObjects.Responses.GcsMonthlyDetail;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class GcsMonthlyDetailProfile : Profile
{
    public GcsMonthlyDetailProfile()
    {
        CreateMap<GcsMonthlyDetailEntity, GcsMonthlyDetailDto>();
    }
}