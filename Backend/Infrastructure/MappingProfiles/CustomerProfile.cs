using Application.Commands.Customers;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using AutoMapper;
using Domain.Entities;
using Nobisoft.Core.Constants;

namespace Infrastructure.MappingProfiles;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<CreateCustomerRequest, CreateCustomerCommands>();
        CreateMap<UpdateCustomerRequest, UpdateCustomerCommand>();
        CreateMap<CustomerEntity, CustomerDto>()
            .ForMember(dest => dest.MeasurePoints, opt =>
                opt.MapFrom(src => src.MeasurePoints.Where(x => x.Status != EntityStatus.Deleted && !x.Deleted).Select(uc => new MeasurementPointDto()
                {
                    Id = uc.Id,
                    Code = uc.Code,
                    Type = uc.Type,
                    Address = uc.Address,
                    Description = uc.Description,
                    CustomerId = uc.CustomerId,
                    Status = uc.Status,
                    GcsId = uc.GcsId,
                    Email = uc.Email,
                    Email2 = uc.Email2,
                    Email3 = uc.Email3,
                    Gcs = new GcsEntity()
                    {
                        Id = uc.Gcs.Id,
                        Name = uc.Gcs.Name,
                        Code = uc.Gcs.Code
                    }
                }))
            )
            .ForMember(dest => dest.UserCustomer, opt =>
                opt.MapFrom(src => src.UserCustomer.Where(x => x.User.Status != EntityStatus.Deleted && !x.User.Deleted).Select(uc => new UserCustomerEntity()
                {
                    Id = uc.Id,
                    CustomerId = uc.CustomerId,
                    UserId = uc.UserId,
                    User = uc.User
                }))
            );
    }
}