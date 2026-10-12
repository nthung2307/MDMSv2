using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointMonitoringProfile : Profile
{
    public MeasurementPointMonitoringProfile()
    {
        CreateMap<MeasurementPointMonitoringEntity, MeasurementPointMonitoringRawDto>();
        CreateMap<MeasurementPointsEventEntity, MeasurementPointEventDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.MeasurementPoint.Customer.FullName))
            .ForMember(dest => dest.MeasurementPointCode, opt => opt.MapFrom(src => src.MeasurementPoint.Code));

        CreateMap<MeasurementPointsAlertEntity, MeasurementPointAlertDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.MeasurementPoint.Customer.FullName))
            .ForMember(dest => dest.MeasurementPointCode, opt => opt.MapFrom(src => src.MeasurementPoint.Code));

        CreateMap<MeasurementPointMonitoringEntity, MeasurementPointMonitoringHourlyRawDto>();
        CreateMap<MeasurementPointMonitoringByDateTimeEntity, MeasurementPointMonitoringByDateTimeRawDto>();
        CreateMap<MeasurementPointMonitoringByStatusEntity, MeasurementPointMonitoringByDateTimeRawDto>();
        CreateMap<MeasurementPointEnergyReportDto, MeasurementPointHalfHourEnergyReportEntity>();
    }
}