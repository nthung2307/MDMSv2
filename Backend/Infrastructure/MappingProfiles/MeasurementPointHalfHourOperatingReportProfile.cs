using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.Report;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointHalfHourOperatingReportProfile : Profile
{
    public MeasurementPointHalfHourOperatingReportProfile()
    {
        CreateMap<MeasurementPointOperatingReportDto, MeasurementPointHalfHourOperatingReportEntity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.MeasurementPoint, o => o.Ignore());
        CreateMap<MeasurementPointHalfHourOperatingReportEntity, OperatingParameterReportResponse>()
            .ForMember(d => d.DienApPhaAUnit, o => o.Ignore())
            .ForMember(d => d.DienApPhaBUnit, o => o.Ignore())
            .ForMember(d => d.DienApPhaCUnit, o => o.Ignore())
            .ForMember(d => d.DongDienPhaAUnit, o => o.Ignore())
            .ForMember(d => d.DongDienPhaBUnit, o => o.Ignore())
            .ForMember(d => d.DongDienPhaCUnit, o => o.Ignore())
            .ForMember(d => d.CongSuatTacDungTong3PhaUnit, o => o.Ignore())
            .ForMember(d => d.CongSuatPhanKhangTong3PhaUnit, o => o.Ignore())
            .ForMember(d => d.TanSoHeThongUnit, o => o.Ignore());
    }
}
