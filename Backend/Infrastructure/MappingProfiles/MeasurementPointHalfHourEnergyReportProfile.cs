using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.Report;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointHalfHourEnergyReportProfile : Profile
{
    public MeasurementPointHalfHourEnergyReportProfile()
    {
        CreateMap<MeasurementPointEnergyReportDto, MeasurementPointHalfHourEnergyReportEntity>();
        CreateMap<MeasurementPointHalfHourEnergyReportEntity, FinalizedEnergyReportResponse>()
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit, o => o.MapFrom(_ => "kVArh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhNhanUnit, o => o.MapFrom(_ => "kWh"))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, o => o.MapFrom(s => MeteringValueHelper.FormatEnergyReportValue(s.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, s.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, null)))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit, o => o.MapFrom(_ => "kVArh"));
    }
}
