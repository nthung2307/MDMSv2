using System.Globalization;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Services;
using Domain.Entities;
using Nobisoft.Core.Constants;

namespace Infrastructure.Helpers;

/// <summary>
/// Shared projection helper mapping MeasurementPointCurrentSnapshotEntity + point metadata
/// to summary, detail, and IoT DTOs while maintaining exact API field compatibility.
/// </summary>
internal static class SnapshotDtoMapper
{
    public static ListMeasurementPointMonitoringDto ToListDto(
        MeasurementPointsEntity point,
        MeasurementPointCurrentSnapshotEntity? snap,
        PointRoutingInfo? routingInfo = null)
    {
        return new ListMeasurementPointMonitoringDto
        {
            Id = point.Id,
            MeasurementPointsId = point.Id,
            MeasurementPointCode = point.Code,
            MeasurementPointDescription = point.Description,
            CustomerCode = point.Customer?.CustomerCode,
            CustomerName = point.Customer?.FullName,

            DongDienPhaA = ToDouble(snap?.DongDienPhaA),
            DongDienPhaAUnit = snap?.DongDienPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DongDienPhaA),
            DongDienPhaAQuanlity = snap?.DongDienPhaAQuality,

            DienApPhaA = ToDouble(snap?.DienApPhaA),
            DienApPhaAUnit = snap?.DienApPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DienApPhaA),
            DienApPhaAQuanlity = snap?.DienApPhaAQuality,

            CongSuatTacDungTong3Pha = ToDouble(snap?.CongSuatTacDungTong3Pha),
            CongSuatTacDungTong3PhaUnit = snap?.CongSuatTacDungTong3PhaUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatTacDungTong3Pha),
            CongSuatTacDungTong3PhaQuanlity = snap?.CongSuatTacDungTong3PhaQuality,

            CongSuatPhanKhangTong3Pha = ToDouble(snap?.CongSuatPhanKhangTong3Pha),
            CongSuatPhanKhangTong3PhaUnit = snap?.CongSuatPhanKhangTong3PhaUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha),
            CongSuatPhanKhangTong3PhaQuanlity = snap?.CongSuatPhanKhangTong3PhaQuality,

            HeSoCongSuatPFTrungBinh3Pha = ToDouble(snap?.HeSoCongSuatPFTrungBinh3Pha),
            HeSoCongSuatPFTrungBinh3PhaQuanlity = snap?.HeSoCongSuatPFTrungBinh3PhaQuality,

            TanSoHeThong = ToDouble(snap?.TanSoHeThong),
            TanSoHeThongUnit = snap?.TanSoHeThongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.TanSoHeThong),
            TanSoHeThongQuanlity = snap?.TanSoHeThongQuality,

            MatKetNoi = snap?.MatKetNoi,
            MatKetNoiQuanlity = snap?.MatKetNoiQuality,
            GroupId = snap?.Id ?? point.Id
        };
    }

    public static MeasurementPointMonitoringDto ToDetailDto(
        MeasurementPointsEntity point,
        MeasurementPointCurrentSnapshotEntity? snap,
        PointRoutingInfo? routingInfo = null)
    {
        return new MeasurementPointMonitoringDto
        {
            Id = snap?.Id ?? point.Id,
            MeasurementPointsId = point.Id,
            MeasurementPointCode = point.Code,
            MeasurementPointDescription = point.Description,
            TagNameDescription = point.Description ?? string.Empty,
            CustomerCode = point.Customer?.CustomerCode,
            CustomerName = point.Customer?.FullName,
            CreatedTime = snap?.LastTelemetryAt ?? snap?.UpdatedAt,

            DongDienPhaA = ToDouble(snap?.DongDienPhaA),
            DongDienPhaAUnit = snap?.DongDienPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DongDienPhaA),
            DongDienPhaAQuanlity = snap?.DongDienPhaAQuality,
            DongDienPhaACreatedTime = snap?.DongDienPhaAUpdatedAt,

            DongDienPhaB = ToDouble(snap?.DongDienPhaB),
            DongDienPhaBUnit = snap?.DongDienPhaBUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DongDienPhaB),
            DongDienPhaBQuanlity = snap?.DongDienPhaBQuality,
            DongDienPhaBCreatedTime = snap?.DongDienPhaBUpdatedAt,

            DongDienPhaC = ToDouble(snap?.DongDienPhaC),
            DongDienPhaCUnit = snap?.DongDienPhaCUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DongDienPhaC),
            DongDienPhaCQuanlity = snap?.DongDienPhaCQuality,
            DongDienPhaCCreatedTime = snap?.DongDienPhaCUpdatedAt,

            DienApPhaA = ToDouble(snap?.DienApPhaA),
            DienApPhaAUnit = snap?.DienApPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DienApPhaA),
            DienApPhaAQuanlity = snap?.DienApPhaAQuality,
            DienApPhaACreatedTime = snap?.DienApPhaAUpdatedAt,

            DienApPhaB = ToDouble(snap?.DienApPhaB),
            DienApPhaBUnit = snap?.DienApPhaBUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DienApPhaB),
            DienApPhaBQuanlity = snap?.DienApPhaBQuality,
            DienApPhaBCreatedTime = snap?.DienApPhaBUpdatedAt,

            DienApPhaC = ToDouble(snap?.DienApPhaC),
            DienApPhaCUnit = snap?.DienApPhaCUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.DienApPhaC),
            DienApPhaCQuanlity = snap?.DienApPhaCQuality,
            DienApPhaCCreatedTime = snap?.DienApPhaCUpdatedAt,

            CongSuatTacDungPhaA = ToDouble(snap?.CongSuatTacDungPhaA),
            CongSuatTacDungPhaAUnit = snap?.CongSuatTacDungPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatTacDungPhaA),
            CongSuatTacDungPhaAQuanlity = snap?.CongSuatTacDungPhaAQuality,
            CongSuatTacDungPhaACreatedTime = snap?.CongSuatTacDungPhaAUpdatedAt,

            CongSuatTacDungPhaB = ToDouble(snap?.CongSuatTacDungPhaB),
            CongSuatTacDungPhaBUnit = snap?.CongSuatTacDungPhaBUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatTacDungPhaB),
            CongSuatTacDungPhaBQuanlity = snap?.CongSuatTacDungPhaBQuality,
            CongSuatTacDungPhaBCreatedTime = snap?.CongSuatTacDungPhaBUpdatedAt,

            CongSuatTacDungPhaC = ToDouble(snap?.CongSuatTacDungPhaC),
            CongSuatTacDungPhaCUnit = snap?.CongSuatTacDungPhaCUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatTacDungPhaC),
            CongSuatTacDungPhaCQuanlity = snap?.CongSuatTacDungPhaCQuality,
            CongSuatTacDungPhaCCreatedTime = snap?.CongSuatTacDungPhaCUpdatedAt,

            CongSuatTacDungTong3Pha = ToDouble(snap?.CongSuatTacDungTong3Pha),
            CongSuatTacDungTong3PhaUnit = snap?.CongSuatTacDungTong3PhaUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatTacDungTong3Pha),
            CongSuatTacDungTong3PhaQuanlity = snap?.CongSuatTacDungTong3PhaQuality,
            CongSuatTacDungTong3PhaCreatedTime = snap?.CongSuatTacDungTong3PhaUpdatedAt,

            CongSuatPhanKhangPhaA = ToDouble(snap?.CongSuatPhanKhangPhaA),
            CongSuatPhanKhangPhaAUnit = snap?.CongSuatPhanKhangPhaAUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatPhanKhangPhaA),
            CongSuatPhanKhangPhaAQuanlity = snap?.CongSuatPhanKhangPhaAQuality,
            CongSuatPhanKhangPhaACreatedTime = snap?.CongSuatPhanKhangPhaAUpdatedAt,

            CongSuatPhanKhangPhaB = ToDouble(snap?.CongSuatPhanKhangPhaB),
            CongSuatPhanKhangPhaBUnit = snap?.CongSuatPhanKhangPhaBUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatPhanKhangPhaB),
            CongSuatPhanKhangPhaBQuanlity = snap?.CongSuatPhanKhangPhaBQuality,
            CongSuatPhanKhangPhaBCreatedTime = snap?.CongSuatPhanKhangPhaBUpdatedAt,

            CongSuatPhanKhangPhaC = ToDouble(snap?.CongSuatPhanKhangPhaC),
            CongSuatPhanKhangPhaCUnit = snap?.CongSuatPhanKhangPhaCUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatPhanKhangPhaC),
            CongSuatPhanKhangPhaCQuanlity = snap?.CongSuatPhanKhangPhaCQuality,
            CongSuatPhanKhangPhaCCreatedTime = snap?.CongSuatPhanKhangPhaCUpdatedAt,

            CongSuatPhanKhangTong3Pha = ToDouble(snap?.CongSuatPhanKhangTong3Pha),
            CongSuatPhanKhangTong3PhaUnit = snap?.CongSuatPhanKhangTong3PhaUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha),
            CongSuatPhanKhangTong3PhaQuanlity = snap?.CongSuatPhanKhangTong3PhaQuality,
            CongSuatPhanKhangTong3PhaCreatedTime = snap?.CongSuatPhanKhangTong3PhaUpdatedAt,

            HeSoCongSuatPFPhaA = ToDouble(snap?.HeSoCongSuatPFPhaA),
            HeSoCongSuatPFPhaAQuanlity = snap?.HeSoCongSuatPFPhaAQuality,
            HeSoCongSuatPFPhaACreatedTime = snap?.HeSoCongSuatPFPhaAUpdatedAt,

            HeSoCongSuatPFPhaB = ToDouble(snap?.HeSoCongSuatPFPhaB),
            HeSoCongSuatPFPhaBQuanlity = snap?.HeSoCongSuatPFPhaBQuality,
            HeSoCongSuatPFPhaBCreatedTime = snap?.HeSoCongSuatPFPhaBUpdatedAt,

            HeSoCongSuatPFPhaC = ToDouble(snap?.HeSoCongSuatPFPhaC),
            HeSoCongSuatPFPhaCQuanlity = snap?.HeSoCongSuatPFPhaCQuality,
            HeSoCongSuatPFPhaCCreatedTime = snap?.HeSoCongSuatPFPhaCUpdatedAt,

            HeSoCongSuatPFTrungBinh3Pha = ToDouble(snap?.HeSoCongSuatPFTrungBinh3Pha),
            HeSoCongSuatPFTrungBinh3PhaQuanlity = snap?.HeSoCongSuatPFTrungBinh3PhaQuality,
            HeSoCongSuatPFTrungBinh3PhaCreatedTime = snap?.HeSoCongSuatPFTrungBinh3PhaUpdatedAt,

            TanSoHeThong = ToDouble(snap?.TanSoHeThong),
            TanSoHeThongUnit = snap?.TanSoHeThongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.TanSoHeThong),
            TanSoHeThongQuanlity = snap?.TanSoHeThongQuality,
            TanSoHeThongCreatedTime = snap?.TanSoHeThongUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioBT = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBT),
            ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBT),
            ChiSoTichLuyDienNangTacDungAPBieuGioBTQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioCD = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCD),
            ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCD),
            ChiSoTichLuyDienNangTacDungAPBieuGioCDQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioCDCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioTD = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTD),
            ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTD),
            ChiSoTichLuyDienNangTacDungAPBieuGioTDQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioTDCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPTong = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPTong),
            ChiSoTichLuyDienNangTacDungAPTongUnit = snap?.ChiSoTichLuyDienNangTacDungAPTongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong),
            ChiSoTichLuyDienNangTacDungAPTongQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPTongQuality,
            ChiSoTichLuyDienNangTacDungAPTongCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPTongUpdatedAt,

            ChiSoTichLuyDienNangPhanKhangAQTong = ToDouble(snap?.ChiSoTichLuyDienNangPhanKhangAQTong),
            ChiSoTichLuyDienNangPhanKhangAQTongUnit = snap?.ChiSoTichLuyDienNangPhanKhangAQTongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTong),
            ChiSoTichLuyDienNangPhanKhangAQTongQuanlity = snap?.ChiSoTichLuyDienNangPhanKhangAQTongQuality,
            ChiSoTichLuyDienNangPhanKhangAQTongCreatedTime = snap?.ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt,

            ChiSoChotDienNangAPGioBT = ToDouble(snap?.ChiSoChotDienNangAPGioBT),
            ChiSoChotDienNangAPGioBTUnit = snap?.ChiSoChotDienNangAPGioBTUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioBT),
            ChiSoChotDienNangAPGioBTQuanlity = snap?.ChiSoChotDienNangAPGioBTQuality,
            ChiSoChotDienNangAPGioBTCreatedTime = snap?.ChiSoChotDienNangAPGioBTUpdatedAt,

            ChiSoChotDienNangAPGioCD = ToDouble(snap?.ChiSoChotDienNangAPGioCD),
            ChiSoChotDienNangAPGioCDUnit = snap?.ChiSoChotDienNangAPGioCDUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioCD),
            ChiSoChotDienNangAPGioCDQuanlity = snap?.ChiSoChotDienNangAPGioCDQuality,
            ChiSoChotDienNangAPGioCDCreatedTime = snap?.ChiSoChotDienNangAPGioCDUpdatedAt,

            ChiSoChotDienNangAPGioTD = ToDouble(snap?.ChiSoChotDienNangAPGioTD),
            ChiSoChotDienNangAPGioTDUnit = snap?.ChiSoChotDienNangAPGioTDUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioTD),
            ChiSoChotDienNangAPGioTDQuanlity = snap?.ChiSoChotDienNangAPGioTDQuality,
            ChiSoChotDienNangAPGioTDCreatedTime = snap?.ChiSoChotDienNangAPGioTDUpdatedAt,

            ChiSoChotDienNangAPTong = ToDouble(snap?.ChiSoChotDienNangAPTong),
            ChiSoChotDienNangAPTongUnit = snap?.ChiSoChotDienNangAPTongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPTong),
            ChiSoChotDienNangAPTongQuanlity = snap?.ChiSoChotDienNangAPTongQuality,
            ChiSoChotDienNangAPTongCreatedTime = snap?.ChiSoChotDienNangAPTongUpdatedAt,

            ChiSoChotDienNangAQTong = ToDouble(snap?.ChiSoChotDienNangAQTong),
            ChiSoChotDienNangAQTongUnit = snap?.ChiSoChotDienNangAQTongUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAQTong),
            ChiSoChotDienNangAQTongQuanlity = snap?.ChiSoChotDienNangAQTongQuality,
            ChiSoChotDienNangAQTongCreatedTime = snap?.ChiSoChotDienNangAQTongUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan),
            ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality,
            ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt,

            ChiSoTichLuyDienNangTacDungAPTongChieuNhan = ToDouble(snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhan),
            ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit = snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTongChieuNhan),
            ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuanlity = snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality,
            ChiSoTichLuyDienNangTacDungAPTongChieuNhanCreatedTime = snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt,

            ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan = ToDouble(snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan),
            ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit = snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan),
            ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuanlity = snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality,
            ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanCreatedTime = snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt,

            ChiSoChotDienNangAPGioBTChieuNhan = ToDouble(snap?.ChiSoChotDienNangAPGioBTChieuNhan),
            ChiSoChotDienNangAPGioBTChieuNhanUnit = snap?.ChiSoChotDienNangAPGioBTChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioBTChieuNhan),
            ChiSoChotDienNangAPGioBTChieuNhanQuanlity = snap?.ChiSoChotDienNangAPGioBTChieuNhanQuality,
            ChiSoChotDienNangAPGioBTChieuNhanCreatedTime = snap?.ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt,

            ChiSoChotDienNangAPGioCDChieuNhan = ToDouble(snap?.ChiSoChotDienNangAPGioCDChieuNhan),
            ChiSoChotDienNangAPGioCDChieuNhanUnit = snap?.ChiSoChotDienNangAPGioCDChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioCDChieuNhan),
            ChiSoChotDienNangAPGioCDChieuNhanQuanlity = snap?.ChiSoChotDienNangAPGioCDChieuNhanQuality,
            ChiSoChotDienNangAPGioCDChieuNhanCreatedTime = snap?.ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt,

            ChiSoChotDienNangAPGioTDChieuNhan = ToDouble(snap?.ChiSoChotDienNangAPGioTDChieuNhan),
            ChiSoChotDienNangAPGioTDChieuNhanUnit = snap?.ChiSoChotDienNangAPGioTDChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPGioTDChieuNhan),
            ChiSoChotDienNangAPGioTDChieuNhanQuanlity = snap?.ChiSoChotDienNangAPGioTDChieuNhanQuality,
            ChiSoChotDienNangAPGioTDChieuNhanCreatedTime = snap?.ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt,

            ChiSoChotDienNangAPTongChieuNhan = ToDouble(snap?.ChiSoChotDienNangAPTongChieuNhan),
            ChiSoChotDienNangAPTongChieuNhanUnit = snap?.ChiSoChotDienNangAPTongChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAPTongChieuNhan),
            ChiSoChotDienNangAPTongChieuNhanQuanlity = snap?.ChiSoChotDienNangAPTongChieuNhanQuality,
            ChiSoChotDienNangAPTongChieuNhanCreatedTime = snap?.ChiSoChotDienNangAPTongChieuNhanUpdatedAt,

            ChiSoChotDienNangAQTongChieuNhan = ToDouble(snap?.ChiSoChotDienNangAQTongChieuNhan),
            ChiSoChotDienNangAQTongChieuNhanUnit = snap?.ChiSoChotDienNangAQTongChieuNhanUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.ChiSoChotDienNangAQTongChieuNhan),
            ChiSoChotDienNangAQTongChieuNhanQuanlity = snap?.ChiSoChotDienNangAQTongChieuNhanQuality,
            ChiSoChotDienNangAQTongChieuNhanCreatedTime = snap?.ChiSoChotDienNangAQTongChieuNhanUpdatedAt,

            PmaxKyHienTai = ToDouble(snap?.PmaxKyHienTai),
            PmaxKyHienTaiUnit = snap?.PmaxKyHienTaiUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.PmaxKyHienTai),
            PmaxKyHienTaiQuanlity = snap?.PmaxKyHienTaiQuality,
            PmaxKyHienTaiCreatedTime = snap?.PmaxKyHienTaiUpdatedAt,

            PmaxKyTruoc = ToDouble(snap?.PmaxKyTruoc),
            PmaxKyTruocUnit = snap?.PmaxKyTruocUnit ?? GetTagUnit(routingInfo, ListDataMeasurementPoint.PmaxKyTruoc),
            PmaxKyTruocQuanlity = snap?.PmaxKyTruocQuality,
            PmaxKyTruocCreatedTime = snap?.PmaxKyTruocUpdatedAt,

            MatKetNoi = snap?.MatKetNoi,
            MatKetNoiQuanlity = snap?.MatKetNoiQuality,
            MatKetNoiCreatedTime = snap?.MatKetNoiUpdatedAt,

            CanhBao01 = snap?.CanhBao01,
            CanhBao01CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao01, ListDataMeasurementPoint.CanhBao01CustomName),
            CanhBao01Quanlity = snap?.CanhBao01Quality,
            CanhBao01CreatedTime = snap?.CanhBao01UpdatedAt,

            CanhBao02 = snap?.CanhBao02,
            CanhBao02CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao02, ListDataMeasurementPoint.CanhBao02CustomName),
            CanhBao02Quanlity = snap?.CanhBao02Quality,
            CanhBao02CreatedTime = snap?.CanhBao02UpdatedAt,

            CanhBao03 = snap?.CanhBao03,
            CanhBao03CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao03, ListDataMeasurementPoint.CanhBao03CustomName),
            CanhBao03Quanlity = snap?.CanhBao03Quality,
            CanhBao03CreatedTime = snap?.CanhBao03UpdatedAt,

            CanhBao04 = snap?.CanhBao04,
            CanhBao04CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao04, ListDataMeasurementPoint.CanhBao04CustomName),
            CanhBao04Quanlity = snap?.CanhBao04Quality,
            CanhBao04CreatedTime = snap?.CanhBao04UpdatedAt,

            CanhBao05 = snap?.CanhBao05,
            CanhBao05CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao05, ListDataMeasurementPoint.CanhBao05CustomName),
            CanhBao05Quanlity = snap?.CanhBao05Quality,
            CanhBao05CreatedTime = snap?.CanhBao05UpdatedAt,

            CanhBao06 = snap?.CanhBao06,
            CanhBao06CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao06, ListDataMeasurementPoint.CanhBao06CustomName),
            CanhBao06Quanlity = snap?.CanhBao06Quality,
            CanhBao06CreatedTime = snap?.CanhBao06UpdatedAt,

            CanhBao07 = snap?.CanhBao07,
            CanhBao07CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao07, ListDataMeasurementPoint.CanhBao07CustomName),
            CanhBao07Quanlity = snap?.CanhBao07Quality,
            CanhBao07CreatedTime = snap?.CanhBao07UpdatedAt,

            CanhBao08 = snap?.CanhBao08,
            CanhBao08CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao08, ListDataMeasurementPoint.CanhBao08CustomName),
            CanhBao08Quanlity = snap?.CanhBao08Quality,
            CanhBao08CreatedTime = snap?.CanhBao08UpdatedAt,

            CanhBao09 = snap?.CanhBao09,
            CanhBao09CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao09, ListDataMeasurementPoint.CanhBao09CustomName),
            CanhBao09Quanlity = snap?.CanhBao09Quality,
            CanhBao09CreatedTime = snap?.CanhBao09UpdatedAt,

            CanhBao10 = snap?.CanhBao10,
            CanhBao10CustomName = GetCustomName(routingInfo, ListDataMeasurementPoint.CanhBao10, ListDataMeasurementPoint.CanhBao10CustomName),
            CanhBao10Quanlity = snap?.CanhBao10Quality,
            CanhBao10CreatedTime = snap?.CanhBao10UpdatedAt,

            CurrentDate = ToIntStr(snap?.MeterDay),
            CurrentDateQuanlity = snap?.MeterDayQuality,
            CurrentMonth = ToIntStr(snap?.MeterMonth),
            CurrentMonthQuanlity = snap?.MeterMonthQuality,
            CurrentYear = ToIntStr(snap?.MeterYear),
            CurrentYearQuanlity = snap?.MeterYearQuality,
            CurrentTime = ToIntStr(snap?.MeterHour),
            CurrentTimeQuanlity = snap?.MeterHourQuality,
            CurrentMinute = ToIntStr(snap?.MeterMinute),
            CurrentMinuteQuanlity = snap?.MeterMinuteQuality,
            CurrentSecond = ToIntStr(snap?.MeterSecond),
            CurrentSecondQuanlity = snap?.MeterSecondQuality,

            LatestClosingDate = ToIntStr(snap?.ClosingDay),
            LatestClosingDateQuanlity = snap?.ClosingDayQuality,
            LatestClosingMonth = ToIntStr(snap?.ClosingMonth),
            LatestClosingMonthQuanlity = snap?.ClosingMonthQuality,
            LatestClosingYear = ToIntStr(snap?.ClosingYear),
            LatestClosingYearQuanlity = snap?.ClosingYearQuality,
            LatestClosingTime = ToIntStr(snap?.ClosingHour),
            LatestClosingTimeQuanlity = snap?.ClosingHourQuality,
            LatestClosingMinute = ToIntStr(snap?.ClosingMinute),
            LatestClosingMinuteQuanlity = snap?.ClosingMinuteQuality,
            LatestClosingSecond = ToIntStr(snap?.ClosingSecond),
            LatestClosingSecondQuanlity = snap?.ClosingSecondQuality,

            PmaxCurrentPeriodYear = ToIntStr(snap?.PmaxCurrentYear),
            PmaxCurrentPeriodYearQuanlity = snap?.PmaxCurrentYearQuality,
            PmaxCurrentPeriodMonth = ToIntStr(snap?.PmaxCurrentMonth),
            PmaxCurrentPeriodMonthQuanlity = snap?.PmaxCurrentMonthQuality,
            PmaxCurrentPeriodDate = ToIntStr(snap?.PmaxCurrentDay),
            PmaxCurrentPeriodDateQuanlity = snap?.PmaxCurrentDayQuality,
            PmaxCurrentPeriodTime = ToIntStr(snap?.PmaxCurrentHour),
            PmaxCurrentPeriodTimeQuanlity = snap?.PmaxCurrentHourQuality,
            PmaxCurrentPeriodMinute = ToIntStr(snap?.PmaxCurrentMinute),
            PmaxCurrentPeriodMinuteQuanlity = snap?.PmaxCurrentMinuteQuality,
            PmaxCurrentPeriodSecond = ToIntStr(snap?.PmaxCurrentSecond),
            PmaxCurrentPeriodSecondQuanlity = snap?.PmaxCurrentSecondQuality,

            PmaxPreviousPeriodYear = ToIntStr(snap?.PmaxPrevYear),
            PmaxPreviousPeriodYearQuanlity = snap?.PmaxPrevYearQuality,
            PmaxPreviousPeriodMonth = ToIntStr(snap?.PmaxPrevMonth),
            PmaxPreviousPeriodMonthQuanlity = snap?.PmaxPrevMonthQuality,
            PmaxPreviousPeriodDate = ToIntStr(snap?.PmaxPrevDay),
            PmaxPreviousPeriodDateQuanlity = snap?.PmaxPrevDayQuality,
            PmaxPreviousPeriodTime = ToIntStr(snap?.PmaxPrevHour),
            PmaxPreviousPeriodTimeQuanlity = snap?.PmaxPrevHourQuality,
            PmaxPreviousPeriodMinute = ToIntStr(snap?.PmaxPrevMinute),
            PmaxPreviousPeriodMinuteQuanlity = snap?.PmaxPrevMinuteQuality,
            PmaxPreviousPeriodSecond = ToIntStr(snap?.PmaxPrevSecond),
            PmaxPreviousPeriodSecondQuanlity = snap?.PmaxPrevSecondQuality,

            GroupId = snap?.Id ?? point.Id
        };
    }

    public static MeasurementPointMonitoringByIoTDto ToIoTDto(
        MeasurementPointsEntity point,
        MeasurementPointCurrentSnapshotEntity? snap,
        PointRoutingInfo? routingInfo = null)
    {
        var d = ToDetailDto(point, snap, routingInfo);
        return new MeasurementPointMonitoringByIoTDto
        {
            Id = d.Id,
            MeasurementPointsId = d.MeasurementPointsId,
            MeasurementPointCode = d.MeasurementPointCode,
            MeasurementPointDescription = d.MeasurementPointDescription,
            TagNameDescription = d.TagNameDescription,
            CustomerCode = d.CustomerCode,
            CustomerName = d.CustomerName,
            CreatedTime = d.CreatedTime,

            DongDienPhaA = d.DongDienPhaA,
            DongDienPhaAUnit = d.DongDienPhaAUnit,
            DongDienPhaAQuanlity = d.DongDienPhaAQuanlity,
            DongDienPhaACreatedTime = d.DongDienPhaACreatedTime,
            DongDienPhaB = d.DongDienPhaB,
            DongDienPhaBUnit = d.DongDienPhaBUnit,
            DongDienPhaBQuanlity = d.DongDienPhaBQuanlity,
            DongDienPhaBCreatedTime = d.DongDienPhaBCreatedTime,
            DongDienPhaC = d.DongDienPhaC,
            DongDienPhaCUnit = d.DongDienPhaCUnit,
            DongDienPhaCQuanlity = d.DongDienPhaCQuanlity,
            DongDienPhaCCreatedTime = d.DongDienPhaCCreatedTime,

            DienApPhaA = d.DienApPhaA,
            DienApPhaAUnit = d.DienApPhaAUnit,
            DienApPhaAQuanlity = d.DienApPhaAQuanlity,
            DienApPhaACreatedTime = d.DienApPhaACreatedTime,
            DienApPhaB = d.DienApPhaB,
            DienApPhaBUnit = d.DienApPhaBUnit,
            DienApPhaBQuanlity = d.DienApPhaBQuanlity,
            DienApPhaBCreatedTime = d.DienApPhaBCreatedTime,
            DienApPhaC = d.DienApPhaC,
            DienApPhaCUnit = d.DienApPhaCUnit,
            DienApPhaCQuanlity = d.DienApPhaCQuanlity,
            DienApPhaCCreatedTime = d.DienApPhaCCreatedTime,

            CongSuatTacDungPhaA = d.CongSuatTacDungPhaA,
            CongSuatTacDungPhaAUnit = d.CongSuatTacDungPhaAUnit,
            CongSuatTacDungPhaAQuanlity = d.CongSuatTacDungPhaAQuanlity,
            CongSuatTacDungPhaACreatedTime = d.CongSuatTacDungPhaACreatedTime,
            CongSuatTacDungPhaB = d.CongSuatTacDungPhaB,
            CongSuatTacDungPhaBUnit = d.CongSuatTacDungPhaBUnit,
            CongSuatTacDungPhaBQuanlity = d.CongSuatTacDungPhaBQuanlity,
            CongSuatTacDungPhaBCreatedTime = d.CongSuatTacDungPhaBCreatedTime,
            CongSuatTacDungPhaC = d.CongSuatTacDungPhaC,
            CongSuatTacDungPhaCUnit = d.CongSuatTacDungPhaCUnit,
            CongSuatTacDungPhaCQuanlity = d.CongSuatTacDungPhaCQuanlity,
            CongSuatTacDungPhaCCreatedTime = d.CongSuatTacDungPhaCCreatedTime,
            CongSuatTacDungTong3Pha = d.CongSuatTacDungTong3Pha,
            CongSuatTacDungTong3PhaUnit = d.CongSuatTacDungTong3PhaUnit,
            CongSuatTacDungTong3PhaQuanlity = d.CongSuatTacDungTong3PhaQuanlity,
            CongSuatTacDungTong3PhaCreatedTime = d.CongSuatTacDungTong3PhaCreatedTime,

            CongSuatPhanKhangPhaA = d.CongSuatPhanKhangPhaA,
            CongSuatPhanKhangPhaAUnit = d.CongSuatPhanKhangPhaAUnit,
            CongSuatPhanKhangPhaAQuanlity = d.CongSuatPhanKhangPhaAQuanlity,
            CongSuatPhanKhangPhaACreatedTime = d.CongSuatPhanKhangPhaACreatedTime,
            CongSuatPhanKhangPhaB = d.CongSuatPhanKhangPhaB,
            CongSuatPhanKhangPhaBUnit = d.CongSuatPhanKhangPhaBUnit,
            CongSuatPhanKhangPhaBQuanlity = d.CongSuatPhanKhangPhaBQuanlity,
            CongSuatPhanKhangPhaBCreatedTime = d.CongSuatPhanKhangPhaBCreatedTime,
            CongSuatPhanKhangPhaC = d.CongSuatPhanKhangPhaC,
            CongSuatPhanKhangPhaCUnit = d.CongSuatPhanKhangPhaCUnit,
            CongSuatPhanKhangPhaCQuanlity = d.CongSuatPhanKhangPhaCQuanlity,
            CongSuatPhanKhangPhaCCreatedTime = d.CongSuatPhanKhangPhaCCreatedTime,
            CongSuatPhanKhangTong3Pha = d.CongSuatPhanKhangTong3Pha,
            CongSuatPhanKhangTong3PhaUnit = d.CongSuatPhanKhangTong3PhaUnit,
            CongSuatPhanKhangTong3PhaQuanlity = d.CongSuatPhanKhangTong3PhaQuanlity,
            CongSuatPhanKhangTong3PhaCreatedTime = d.CongSuatPhanKhangTong3PhaCreatedTime,

            HeSoCongSuatPFPhaA = d.HeSoCongSuatPFPhaA,
            HeSoCongSuatPFPhaAQuanlity = d.HeSoCongSuatPFPhaAQuanlity,
            HeSoCongSuatPFPhaACreatedTime = d.HeSoCongSuatPFPhaACreatedTime,
            HeSoCongSuatPFPhaB = d.HeSoCongSuatPFPhaB,
            HeSoCongSuatPFPhaBQuanlity = d.HeSoCongSuatPFPhaBQuanlity,
            HeSoCongSuatPFPhaBCreatedTime = d.HeSoCongSuatPFPhaBCreatedTime,
            HeSoCongSuatPFPhaC = d.HeSoCongSuatPFPhaC,
            HeSoCongSuatPFPhaCQuanlity = d.HeSoCongSuatPFPhaCQuanlity,
            HeSoCongSuatPFPhaCCreatedTime = d.HeSoCongSuatPFPhaCCreatedTime,
            HeSoCongSuatPFTrungBinh3Pha = d.HeSoCongSuatPFTrungBinh3Pha,
            HeSoCongSuatPFTrungBinh3PhaQuanlity = d.HeSoCongSuatPFTrungBinh3PhaQuanlity,
            HeSoCongSuatPFTrungBinh3PhaCreatedTime = d.HeSoCongSuatPFTrungBinh3PhaCreatedTime,

            TanSoHeThong = d.TanSoHeThong,
            TanSoHeThongUnit = d.TanSoHeThongUnit,
            TanSoHeThongQuanlity = d.TanSoHeThongQuanlity,
            TanSoHeThongCreatedTime = d.TanSoHeThongCreatedTime,

            ChiSoTichLuyDienNangTacDungAPBieuGioBT = d.ChiSoTichLuyDienNangTacDungAPBieuGioBT,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit = d.ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTQuanlity = d.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuanlity,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTCreatedTime = d.ChiSoTichLuyDienNangTacDungAPBieuGioBTCreatedTime,
            ChiSoTichLuyDienNangTacDungAPBieuGioCD = d.ChiSoTichLuyDienNangTacDungAPBieuGioCD,
            ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit = d.ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit,
            ChiSoTichLuyDienNangTacDungAPBieuGioCDQuanlity = d.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuanlity,
            ChiSoTichLuyDienNangTacDungAPBieuGioCDCreatedTime = d.ChiSoTichLuyDienNangTacDungAPBieuGioCDCreatedTime,
            ChiSoTichLuyDienNangTacDungAPBieuGioTD = d.ChiSoTichLuyDienNangTacDungAPBieuGioTD,
            ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit = d.ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit,
            ChiSoTichLuyDienNangTacDungAPBieuGioTDQuanlity = d.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuanlity,
            ChiSoTichLuyDienNangTacDungAPBieuGioTDCreatedTime = d.ChiSoTichLuyDienNangTacDungAPBieuGioTDCreatedTime,
            ChiSoTichLuyDienNangTacDungAPTong = d.ChiSoTichLuyDienNangTacDungAPTong,
            ChiSoTichLuyDienNangTacDungAPTongUnit = d.ChiSoTichLuyDienNangTacDungAPTongUnit,
            ChiSoTichLuyDienNangTacDungAPTongQuanlity = d.ChiSoTichLuyDienNangTacDungAPTongQuanlity,
            ChiSoTichLuyDienNangTacDungAPTongCreatedTime = d.ChiSoTichLuyDienNangTacDungAPTongCreatedTime,
            ChiSoTichLuyDienNangPhanKhangAQTong = d.ChiSoTichLuyDienNangPhanKhangAQTong,
            ChiSoTichLuyDienNangPhanKhangAQTongUnit = d.ChiSoTichLuyDienNangPhanKhangAQTongUnit,
            ChiSoTichLuyDienNangPhanKhangAQTongQuanlity = d.ChiSoTichLuyDienNangPhanKhangAQTongQuanlity,
            ChiSoTichLuyDienNangPhanKhangAQTongCreatedTime = d.ChiSoTichLuyDienNangPhanKhangAQTongCreatedTime,

            ChiSoChotDienNangAPGioBT = d.ChiSoChotDienNangAPGioBT,
            ChiSoChotDienNangAPGioBTUnit = d.ChiSoChotDienNangAPGioBTUnit,
            ChiSoChotDienNangAPGioBTQuanlity = d.ChiSoChotDienNangAPGioBTQuanlity,
            ChiSoChotDienNangAPGioBTCreatedTime = d.ChiSoChotDienNangAPGioBTCreatedTime,
            ChiSoChotDienNangAPGioCD = d.ChiSoChotDienNangAPGioCD,
            ChiSoChotDienNangAPGioCDUnit = d.ChiSoChotDienNangAPGioCDUnit,
            ChiSoChotDienNangAPGioCDQuanlity = d.ChiSoChotDienNangAPGioCDQuanlity,
            ChiSoChotDienNangAPGioCDCreatedTime = d.ChiSoChotDienNangAPGioCDCreatedTime,
            ChiSoChotDienNangAPGioTD = d.ChiSoChotDienNangAPGioTD,
            ChiSoChotDienNangAPGioTDUnit = d.ChiSoChotDienNangAPGioTDUnit,
            ChiSoChotDienNangAPGioTDQuanlity = d.ChiSoChotDienNangAPGioTDQuanlity,
            ChiSoChotDienNangAPGioTDCreatedTime = d.ChiSoChotDienNangAPGioTDCreatedTime,
            ChiSoChotDienNangAPTong = d.ChiSoChotDienNangAPTong,
            ChiSoChotDienNangAPTongUnit = d.ChiSoChotDienNangAPTongUnit,
            ChiSoChotDienNangAPTongQuanlity = d.ChiSoChotDienNangAPTongQuanlity,
            ChiSoChotDienNangAPTongCreatedTime = d.ChiSoChotDienNangAPTongCreatedTime,
            ChiSoChotDienNangAQTong = d.ChiSoChotDienNangAQTong,
            ChiSoChotDienNangAQTongUnit = d.ChiSoChotDienNangAQTongUnit,
            ChiSoChotDienNangAQTongQuanlity = d.ChiSoChotDienNangAQTongQuanlity,
            ChiSoChotDienNangAQTongCreatedTime = d.ChiSoChotDienNangAQTongCreatedTime,
            GroupId = d.GroupId
        };
    }

    private static double? ToDouble(decimal? value) => value.HasValue ? (double)value.Value : null;

    private static string? ToIntStr(short? value) => value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : null;

    private static string? GetTagUnit(PointRoutingInfo? routingInfo, string description)
    {
        if (routingInfo == null) return null;
        return routingInfo.TagsByDescription.TryGetValue(description, out var tag) ? tag.Unit : null;
    }

    private static string? GetCustomName(PointRoutingInfo? routingInfo, string warningDescription, string fallbackDesc)
    {
        if (routingInfo == null) return null;
        if (routingInfo.TagsByDescription.TryGetValue(warningDescription, out var warningTag) &&
            !string.IsNullOrWhiteSpace(warningTag.CustomName))
        {
            return warningTag.CustomName;
        }
        return routingInfo.TagsByDescription.TryGetValue(fallbackDesc, out var customTag)
            ? customTag.CustomName
            : null;
    }
}

