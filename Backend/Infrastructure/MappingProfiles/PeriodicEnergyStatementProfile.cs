using Application.Commands.Customers;
using Application.Commands.TagName;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.TagName;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.TagName;
using AutoMapper;
using Domain.Entities;
using Nobisoft.Core.Constants;

namespace Infrastructure.MappingProfiles;

public class PeriodicEnergyStatementProfile : Profile
{
    public PeriodicEnergyStatementProfile()
    {
        CreateMap<PeriodicEnergyStatementEntity, PeriodicEnergyStatementDto>();
    }
}