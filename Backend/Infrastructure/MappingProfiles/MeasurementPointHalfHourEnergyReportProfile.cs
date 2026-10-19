using System.Globalization;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.Report;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class MeasurementPointHalfHourEnergyReportProfile : Profile
{
    public MeasurementPointHalfHourEnergyReportProfile()
    {
        CreateMap<MeasurementPointEnergyReportDto, MeasurementPointHalfHourEnergyReportEntity>();
        CreateMap<MeasurementPointHalfHourEnergyReportEntity, FinalizedEnergyReportResponse>()
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPTongKenhGiao)))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan,o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan)))
            .ForMember(d => d.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangTacDungAPTongKenhNhan)))
            .ForMember(d => d.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, o => o.MapFrom(s => FormatValue(s.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan)));
    }
    
    private static string? FormatValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        bool isNotTopical = value.EndsWith("*") || value.Contains("Not Topical");
        string cleanValue = value.Replace("*", "").Replace("(Not Topical)", "").Trim();

        if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double valueNumber))
        {
            var viCulture = CultureInfo.GetCultureInfo("vi-VN");
            string formatted = valueNumber % 1 == 0
                ? valueNumber.ToString("N0", viCulture)
                : valueNumber.ToString("N2", viCulture);
            return isNotTopical ? $"{formatted}*" : formatted;
        }
        return isNotTopical ? $"{cleanValue}*" : cleanValue;
    }
}