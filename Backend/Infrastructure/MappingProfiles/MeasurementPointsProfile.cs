using Application.Commands.MeasurementPoints;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointsProfile : Profile
{
    public MeasurementPointsProfile()
    {
        CreateMap<CreateMeasurementPointsRequest, CreateMeasurementPointsCommands>();
        CreateMap<UpdateMeasurementPointsRequest, UpdateMeasurementPointsCommand>();
        CreateMap<MeasurementPointsEntity, ListMeasurementPointDto>();
        CreateMap<MeasurementPointsEntity, MeasurementPointDto>()
            .ForMember(dest => dest.Gcs, opt =>
                opt.MapFrom(src => new GcsEntity()
                {
                    Id = src.Id,
                    Code = src.Gcs.Code,
                    Name = src.Gcs.Name,
                    PeriodNumber = src.Gcs.PeriodNumber
                })
            )
            .ForMember(dest => dest.Customer, opt =>
                opt.MapFrom(src => new CustomerEntity()
                {
                    Id = src.Id,
                    CustomerCode = src.Customer.CustomerCode,
                    FullName = src.Customer.FullName,
                    Address = src.Customer.Address,
                    PhoneNumber = src.Customer.PhoneNumber
                })
            );
    }
}