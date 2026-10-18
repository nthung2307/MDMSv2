using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.Report;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointHalfHourOperatingReportProfile : Profile
{
    public MeasurementPointHalfHourOperatingReportProfile()
    {
        CreateMap<MeasurementPointOperatingReportDto, MeasurementPointHalfHourOperatingReportEntity>();
        CreateMap<MeasurementPointHalfHourOperatingReportEntity, OperatingParameterReportResponse>();
    }
}
