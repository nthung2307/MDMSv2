using Application.Commands.Customers;
using Application.Commands.TagName;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.TagName;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.DataTransferObjects.Responses.TagName;
using AutoMapper;
using Domain.Entities;
using Nobisoft.Core.Constants;

namespace Infrastructure.MappingProfiles;

public class TagNameProfile : Profile
{
    public TagNameProfile()
    {
        CreateMap<CreateTagNameRequest, CreateTagNameCommands>();
        CreateMap<CreateListTagNameRequest, CreateTagNameCommands>();
        CreateMap<UpdateListTagNameRequest, UpdateTagNameCommand>();
        CreateMap<UpdateTagNameRequest, UpdateTagNameCommand>();
        CreateMap<TagNameEntity, TagNameDto>();
    }
}