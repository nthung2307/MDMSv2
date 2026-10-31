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
                opt.MapFrom(src => src.Gcs != null ? new GcsEntity()
                {
                    Id = src.Gcs.Id,
                    Code = src.Gcs.Code,
                    Name = src.Gcs.Name,
                    PeriodNumber = src.Gcs.PeriodNumber
                } : null))
            .ForMember(dest => dest.Customer, opt =>
                opt.MapFrom(src => src.Customer != null ? new CustomerEntity()
                {
                    Id = src.Customer.Id,
                    CustomerCode = src.Customer.CustomerCode,
                    FullName = src.Customer.FullName,
                    Address = src.Customer.Address,
                    PhoneNumber = src.Customer.PhoneNumber,
                    Email = src.Customer.Email,
                    Status = src.Customer.Status
                } : null));
    }
}