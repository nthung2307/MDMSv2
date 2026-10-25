using System.Globalization;
using System.Text;
using Application.DataTransferObjects.Requests.Report;
using Application.Repositories.ReadWrite;
using CsvHelper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace Infrastructure.Repositories.ReadWrite;

public class MeasurementPointHalfHourOperatingReportReadWriteRepository :
    RestfulRepository<MeasurementPointHalfHourOperatingReportEntity, AppReadWriteDbContext>,
    IMeasurementPointHalfHourOperatingReportReadWriteRepository
{
    private readonly AppReadWriteDbContext _context;
    private readonly DbSet<MeasurementPointHalfHourOperatingReportEntity> _dbSet;
    private readonly DbSet<MeasurementPointMonitoringHourlyEntity> _hourlySet;
    private readonly DbSet<MeasurementPointMonitoringEntity> _monitoringSet;
    private readonly DbSet<MeasurementPointsEntity> _pointSet;

    public MeasurementPointHalfHourOperatingReportReadWriteRepository(
        AppReadWriteDbContext context,
        ISnowflakeIdService snowflakeIdService) : base(context, snowflakeIdService)
    {
        _context = context;
        _dbSet = context.Set<MeasurementPointHalfHourOperatingReportEntity>();
        _hourlySet = context.Set<MeasurementPointMonitoringHourlyEntity>();
        _monitoringSet = context.Set<MeasurementPointMonitoringEntity>();
        _pointSet = context.Set<MeasurementPointsEntity>();
    }

    private async Task<string> GetConfiguredVoltageUnitAsync(long measurementPointId, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await _context.Set<TagNameEntity>().AsNoTracking()
                .Where(t => t.MeasurementPointId == measurementPointId &&
                            (t.Description == ListDataMeasurementPoint.DienApPhaA || t.Description.Contains("Điện áp") || t.Description.Contains("dien ap")) &&
                            !t.Deleted && !string.IsNullOrEmpty(t.Unit))
                .Select(t => t.Unit)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(unit)) return unit;
        }
        catch { }
        return "kV";
    }

    private async Task<List<MeasurementPointHalfHourOperatingReportEntity>> GetReportsForExportAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reports = await _dbSet.AsNoTracking()
                .Include(m => m.MeasurementPoint)
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime)
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            if (reports.Any())
            {
                return reports;
            }
        }
        catch
        {
            // Bỏ qua lỗi table chưa có
        }

        // Fallback từ MeasurementPointMonitoringHourly
        var point = await _pointSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.MeasurementPointId, cancellationToken);
        var cultureInfo = CultureInfo.InvariantCulture;

        try
        {
            var hourlyData = await _hourlySet.AsNoTracking()
                .Where(h => h.MeasurementPointId == request.MeasurementPointId &&
                            h.CreatedTime >= request.StartTime && h.CreatedTime <= request.EndTime)
                .OrderBy(h => h.CreatedTime)
                .ToListAsync(cancellationToken);

            if (hourlyData.Any())
            {
                return hourlyData.Select(h =>
                {
                    double? p = double.TryParse(h.TrungBinhCongSuatTacDungTong3Pha, NumberStyles.Any, cultureInfo, out var pVal) ? pVal : null;
                    double? q = double.TryParse(h.TrungBinhCongSuatPhanKhangTong3Pha, NumberStyles.Any, cultureInfo, out var qVal) ? qVal : null;
                    double? pf = null;
                    if (p.HasValue && q.HasValue)
                    {
                        var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                        if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
                    }

                    return new MeasurementPointHalfHourOperatingReportEntity
                    {
                        Id = h.Id,
                        MeasurementPointId = h.MeasurementPointId,
                        MeasurementPoint = point,
                        CreatedTime = h.CreatedTime,
                        DienApPhaA = double.TryParse(h.TrungBinhDienApPhaA, NumberStyles.Any, cultureInfo, out var ua) ? ua : null,
                        DienApPhaAUnit = h.TrungBinhDienApPhaAUnit,
                        DienApPhaB = double.TryParse(h.TrungBinhDienApPhaB, NumberStyles.Any, cultureInfo, out var ub) ? ub : null,
                        DienApPhaBUnit = h.TrungBinhDienApPhaBUnit,
                        DienApPhaC = double.TryParse(h.TrungBinhDienApPhaC, NumberStyles.Any, cultureInfo, out var uc) ? uc : null,
                        DienApPhaCUnit = h.TrungBinhDienApPhaCUnit,
                        DongDienPhaA = double.TryParse(h.TrungBinhDongDienPhaA, NumberStyles.Any, cultureInfo, out var ia) ? ia : null,
                        DongDienPhaAUnit = h.TrungBinhDongDienPhaAUnit,
                        DongDienPhaB = double.TryParse(h.TrungBinhDongDienPhaB, NumberStyles.Any, cultureInfo, out var ib) ? ib : null,
                        DongDienPhaBUnit = h.TrungBinhDongDienPhaBUnit,
                        DongDienPhaC = double.TryParse(h.TrungBinhDongDienPhaC, NumberStyles.Any, cultureInfo, out var ic) ? ic : null,
                        DongDienPhaCUnit = h.TrungBinhDongDienPhaCUnit,
                        CongSuatTacDungTong3Pha = p,
                        CongSuatTacDungTong3PhaUnit = h.CongSuatTacDungTong3PhaUnit,
                        CongSuatPhanKhangTong3Pha = q,
                        CongSuatPhanKhangTong3PhaUnit = h.TrungBinhCongSuatPhanKhangTong3PhaUnit,
                        HeSoCongSuatPF = pf,
                        TanSoHeThong = 50.0,
                        TanSoHeThongUnit = "Hz"
                    };
                }).ToList();
            }
        }
        catch
        {
            // Bỏ qua lỗi
        }

        // Fallback từ raw telemetry MeasurementPointMonitoring
        try
        {
            var rawList = await _monitoringSet.AsNoTracking()
                .Where(x => x.MeasurementPointsId == request.MeasurementPointId &&
                            x.CreatedTime >= request.StartTime && x.CreatedTime <= request.EndTime &&
                            x.Value != null && x.Value != "")
                .Include(x => x.TagName)
                .OrderBy(x => x.CreatedTime)
                .ToListAsync(cancellationToken);

            if (rawList.Any())
            {
                var grouped = rawList
                    .GroupBy(x =>
                    {
                        var t = x.CreatedTime!.Value;
                        var min = t.Minute >= 30 ? 30 : 0;
                        return new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, min, 0, t.Offset);
                    })
                    .OrderBy(g => g.Key);

                var list = new List<MeasurementPointHalfHourOperatingReportEntity>();
                foreach (var g in grouped)
                {
                    double? GetTagAvg(string tagDesc)
                    {
                        var vals = g.Where(x => x.TagNameDescription == tagDesc && double.TryParse(x.Value, NumberStyles.Any, cultureInfo, out _))
                            .Select(x => double.Parse(x.Value!, cultureInfo))
                            .ToList();
                        return vals.Any() ? Math.Round(vals.Average(), 2) : null;
                    }

                    string? GetTagUnit(string tagDesc, string defaultUnit)
                    {
                        return g.FirstOrDefault(x => x.TagNameDescription == tagDesc)?.TagName?.Unit ?? defaultUnit;
                    }

                    var p = GetTagAvg(ListDataMeasurementPoint.CongSuatTacDungTong3Pha);
                    var q = GetTagAvg(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha);
                    var pf = GetTagAvg(ListDataMeasurementPoint.HeSoCongSuatPFTrungBinh3Pha);
                    if (!pf.HasValue && p.HasValue && q.HasValue)
                    {
                        var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                        if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
                    }

                    list.Add(new MeasurementPointHalfHourOperatingReportEntity
                    {
                        Id = g.First().Id,
                        MeasurementPointId = request.MeasurementPointId,
                        MeasurementPoint = point,
                        CreatedTime = g.Key,
                        DienApPhaA = GetTagAvg(ListDataMeasurementPoint.DienApPhaA),
                        DienApPhaAUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaA, "V"),
                        DienApPhaB = GetTagAvg(ListDataMeasurementPoint.DienApPhaB),
                        DienApPhaBUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaB, "V"),
                        DienApPhaC = GetTagAvg(ListDataMeasurementPoint.DienApPhaC),
                        DienApPhaCUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaC, "V"),
                        DongDienPhaA = GetTagAvg(ListDataMeasurementPoint.DongDienPhaA),
                        DongDienPhaAUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaA, "A"),
                        DongDienPhaB = GetTagAvg(ListDataMeasurementPoint.DongDienPhaB),
                        DongDienPhaBUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaB, "A"),
                        DongDienPhaC = GetTagAvg(ListDataMeasurementPoint.DongDienPhaC),
                        DongDienPhaCUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaC, "A"),
                        CongSuatTacDungTong3Pha = p,
                        CongSuatTacDungTong3PhaUnit = GetTagUnit(ListDataMeasurementPoint.CongSuatTacDungTong3Pha, "kW"),
                        CongSuatPhanKhangTong3Pha = q,
                        CongSuatPhanKhangTong3PhaUnit = GetTagUnit(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha, "kVAr"),
                        HeSoCongSuatPF = pf,
                        TanSoHeThong = GetTagAvg(ListDataMeasurementPoint.TanSoHeThong) ?? 50.0,
                        TanSoHeThongUnit = GetTagUnit(ListDataMeasurementPoint.TanSoHeThong, "Hz")
                    });
                }
                return list;
            }
        }
        catch
        {
            // Bỏ qua lỗi
        }

        return new List<MeasurementPointHalfHourOperatingReportEntity>();
    }

    public async Task<byte[]> GenerateCsv(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        var reports = await GetReportsForExportAsync(request, cancellationToken);

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(true));
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        // Header
        csvWriter.WriteField("Thời gian");
        csvWriter.WriteField("Điện áp Pha A (V)");
        csvWriter.WriteField("Điện áp Pha B (V)");
        csvWriter.WriteField("Điện áp Pha C (V)");
        csvWriter.WriteField("Dòng điện Pha A (A)");
        csvWriter.WriteField("Dòng điện Pha B (A)");
        csvWriter.WriteField("Dòng điện Pha C (A)");
        csvWriter.WriteField("P Tổng (kW)");
        csvWriter.WriteField("Q Tổng (kVAr)");
        csvWriter.WriteField("Hệ số công suất PF");
        csvWriter.WriteField("Tần số (Hz)");
        await csvWriter.NextRecordAsync();

        var viCulture = CultureInfo.GetCultureInfo("vi-VN");
        foreach (var r in reports)
        {
            csvWriter.WriteField(r.CreatedTime?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? string.Empty);
            csvWriter.WriteField(r.DienApPhaA?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.DienApPhaB?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.DienApPhaC?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.DongDienPhaA?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.DongDienPhaB?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.DongDienPhaC?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.CongSuatTacDungTong3Pha?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.CongSuatPhanKhangTong3Pha?.ToString("N2", viCulture) ?? "N/A");
            csvWriter.WriteField(r.HeSoCongSuatPF?.ToString("N3", viCulture) ?? "N/A");
            csvWriter.WriteField(r.TanSoHeThong?.ToString("N2", viCulture) ?? "N/A");
            await csvWriter.NextRecordAsync();
        }

        await streamWriter.FlushAsync(cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateExcel(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var reports = await GetReportsForExportAsync(request, cancellationToken);
        var point = reports.FirstOrDefault()?.MeasurementPoint ?? await _pointSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.MeasurementPointId, cancellationToken);
        var pointCode = point?.Code ?? request.MeasurementPointId.ToString();

        using var package = new ExcelPackage();

        // 1. SHEET BÁO CÁO DỮ LIỆU
        var wsData = package.Workbook.Worksheets.Add("Báo cáo");
        wsData.View.ShowGridLines = true;

        // Tiêu đề thương hiệu
        wsData.Cells["A1:L1"].Merge = true;
        wsData.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
        wsData.Cells["A1"].Style.Font.Italic = true;
        wsData.Cells["A1"].Style.Font.Size = 10;
        wsData.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(85, 85, 85));
        wsData.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        wsData.Row(1).Height = 20;

        // Tiêu đề báo cáo
        wsData.Cells["A2:L2"].Merge = true;
        wsData.Cells["A2"].Value = "BÁO CÁO THÔNG SỐ VẬN HÀNH 30 PHÚT";
        wsData.Cells["A2"].Style.Font.Bold = true;
        wsData.Cells["A2"].Style.Font.Size = 14;
        wsData.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
        wsData.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        wsData.Row(2).Height = 26;

        // Thông tin phụ
        wsData.Cells["A3:L3"].Merge = true;
        wsData.Cells["A3"].Value = $"Điểm đo: {pointCode} | Thời gian: {request.StartTime.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm} - {request.EndTime.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm}";
        wsData.Cells["A3"].Style.Font.Bold = true;
        wsData.Cells["A3"].Style.Font.Size = 10;
        wsData.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        wsData.Row(3).Height = 20;

        var voltageUnit = reports.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DienApPhaAUnit))?.DienApPhaAUnit;
        if (string.IsNullOrWhiteSpace(voltageUnit) || voltageUnit == "V")
        {
            var configured = await GetConfiguredVoltageUnitAsync(request.MeasurementPointId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(configured))
            {
                voltageUnit = configured;
            }
        }
        if (string.IsNullOrWhiteSpace(voltageUnit))
        {
            voltageUnit = "kV";
        }

        // Header bảng (Dòng 5)
        var headers = new string[] {
            "STT", "Thời gian", $"Ua ({voltageUnit})", $"Ub ({voltageUnit})", $"Uc ({voltageUnit})",
            "Ia (A)", "Ib (A)", "Ic (A)", "Pt (kW)", "Qt (kVAr)", "PF", "Tần số (Hz)"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = wsData.Cells[5, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 121)); // Navy EVN
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
        }
        wsData.Row(5).Height = 26;

        int rowIdx = 6;
        int stt = 1;
        foreach (var r in reports)
        {
            wsData.Cells[rowIdx, 1].Value = stt++;
            wsData.Cells[rowIdx, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsData.Cells[rowIdx, 1].Style.Numberformat.Format = "0";

            var timeStr = r.CreatedTime?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm") ?? "";
            wsData.Cells[rowIdx, 2].Value = timeStr;
            wsData.Cells[rowIdx, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            void SetVal(int col, double? val, string fmt = "#,##0.00")
            {
                var c = wsData.Cells[rowIdx, col];
                if (val.HasValue)
                {
                    c.Value = val.Value;
                    c.Style.Numberformat.Format = fmt;
                    c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    c.Value = "-";
                    c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            SetVal(3, r.DienApPhaA);
            SetVal(4, r.DienApPhaB);
            SetVal(5, r.DienApPhaC);
            SetVal(6, r.DongDienPhaA);
            SetVal(7, r.DongDienPhaB);
            SetVal(8, r.DongDienPhaC);
            SetVal(9, r.CongSuatTacDungTong3Pha);
            SetVal(10, r.CongSuatPhanKhangTong3Pha);
            SetVal(11, r.HeSoCongSuatPF, "#,##0.000");
            SetVal(12, r.TanSoHeThong);

            for (int col = 1; col <= 12; col++)
            {
                wsData.Cells[rowIdx, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(220, 224, 230));
                if (rowIdx % 2 == 1)
                {
                    wsData.Cells[rowIdx, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    wsData.Cells[rowIdx, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(249, 250, 251));
                }
            }
            wsData.Row(rowIdx).Height = 20;
            rowIdx++;
        }

        int dataEndRow = rowIdx - 1;
        int dataStartRow = 6;

        // AutoFit cột
        wsData.Column(1).Width = 8;
        wsData.Column(2).Width = 18;
        for (int c = 3; c <= 12; c++) wsData.Column(c).Width = 14;

        if (dataEndRow >= dataStartRow)
        {
            // 2. SHEET BIỂU ĐỒ ĐIỆN ÁP (Native Line Chart)
            var wsU = package.Workbook.Worksheets.Add("BĐ Điện áp");
            wsU.View.ShowGridLines = true;
            wsU.Cells["A1:N1"].Merge = true;
            wsU.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsU.Cells["A1"].Style.Font.Italic = true;
            wsU.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsU.Cells["A2:N2"].Merge = true;
            wsU.Cells["A2"].Value = $"BIỂU ĐỒ: ĐIỆN ÁP ({voltageUnit})";
            wsU.Cells["A2"].Style.Font.Bold = true;
            wsU.Cells["A2"].Style.Font.Size = 13;
            wsU.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsU.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartU = wsU.Drawings.AddChart("chartU", eChartType.Line) as ExcelLineChart;
            if (chartU != null)
            {
                chartU.SetPosition(3, 0, 1, 0);
                chartU.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];
                var serA = chartU.Series.Add(wsData.Cells[dataStartRow, 3, dataEndRow, 3], xSeries) as ExcelLineChartSerie;
                if (serA != null) { serA.Header = $"Ua ({voltageUnit})"; serA.Smooth = true; }
                var serB = chartU.Series.Add(wsData.Cells[dataStartRow, 4, dataEndRow, 4], xSeries) as ExcelLineChartSerie;
                if (serB != null) { serB.Header = $"Ub ({voltageUnit})"; serB.Smooth = true; }
                var serC = chartU.Series.Add(wsData.Cells[dataStartRow, 5, dataEndRow, 5], xSeries) as ExcelLineChartSerie;
                if (serC != null) { serC.Header = $"Uc ({voltageUnit})"; serC.Smooth = true; }
            }

            // 3. SHEET BIỂU ĐỒ DÒNG ĐIỆN (Native Line Chart)
            var wsI = package.Workbook.Worksheets.Add("BĐ Dòng điện");
            wsI.View.ShowGridLines = true;
            wsI.Cells["A1:N1"].Merge = true;
            wsI.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsI.Cells["A1"].Style.Font.Italic = true;
            wsI.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsI.Cells["A2:N2"].Merge = true;
            wsI.Cells["A2"].Value = "BIỂU ĐỒ: DÒNG ĐIỆN (A)";
            wsI.Cells["A2"].Style.Font.Bold = true;
            wsI.Cells["A2"].Style.Font.Size = 13;
            wsI.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsI.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartI = wsI.Drawings.AddChart("chartI", eChartType.Line) as ExcelLineChart;
            if (chartI != null)
            {
                chartI.SetPosition(3, 0, 1, 0);
                chartI.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];
                var serIa = chartI.Series.Add(wsData.Cells[dataStartRow, 6, dataEndRow, 6], xSeries) as ExcelLineChartSerie;
                if (serIa != null) { serIa.Header = "Ia (A)"; serIa.Smooth = true; }
                var serIb = chartI.Series.Add(wsData.Cells[dataStartRow, 7, dataEndRow, 7], xSeries) as ExcelLineChartSerie;
                if (serIb != null) { serIb.Header = "Ib (A)"; serIb.Smooth = true; }
                var serIc = chartI.Series.Add(wsData.Cells[dataStartRow, 8, dataEndRow, 8], xSeries) as ExcelLineChartSerie;
                if (serIc != null) { serIc.Header = "Ic (A)"; serIc.Smooth = true; }
            }

            // 4. SHEET BIỂU ĐỒ CÔNG SUẤT (Native Line Chart)
            var wsP = package.Workbook.Worksheets.Add("BĐ Công suất");
            wsP.View.ShowGridLines = true;
            wsP.Cells["A1:N1"].Merge = true;
            wsP.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsP.Cells["A1"].Style.Font.Italic = true;
            wsP.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsP.Cells["A2:N2"].Merge = true;
            wsP.Cells["A2"].Value = "BIỂU ĐỒ: CÔNG SUẤT Pt (kW) & Qt (kVAr)";
            wsP.Cells["A2"].Style.Font.Bold = true;
            wsP.Cells["A2"].Style.Font.Size = 13;
            wsP.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsP.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartP = wsP.Drawings.AddChart("chartP", eChartType.Line) as ExcelLineChart;
            if (chartP != null)
            {
                chartP.SetPosition(3, 0, 1, 0);
                chartP.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];
                var serP = chartP.Series.Add(wsData.Cells[dataStartRow, 9, dataEndRow, 9], xSeries) as ExcelLineChartSerie;
                if (serP != null) { serP.Header = "Pt (kW)"; serP.Smooth = true; }
                var serQ = chartP.Series.Add(wsData.Cells[dataStartRow, 10, dataEndRow, 10], xSeries) as ExcelLineChartSerie;
                if (serQ != null) { serQ.Header = "Qt (kVAr)"; serQ.Smooth = true; }
            }

            // 5. SHEET BIỂU ĐỒ HỆ SỐ PF (Native Line Chart)
            var wsPF = package.Workbook.Worksheets.Add("BĐ Hệ số PF");
            wsPF.View.ShowGridLines = true;
            wsPF.Cells["A1:N1"].Merge = true;
            wsPF.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsPF.Cells["A1"].Style.Font.Italic = true;
            wsPF.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsPF.Cells["A2:N2"].Merge = true;
            wsPF.Cells["A2"].Value = "BIỂU ĐỒ: HỆ SỐ PF";
            wsPF.Cells["A2"].Style.Font.Bold = true;
            wsPF.Cells["A2"].Style.Font.Size = 13;
            wsPF.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsPF.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartPF = wsPF.Drawings.AddChart("chartPF", eChartType.Line) as ExcelLineChart;
            if (chartPF != null)
            {
                chartPF.SetPosition(3, 0, 1, 0);
                chartPF.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];
                var serPF = chartPF.Series.Add(wsData.Cells[dataStartRow, 11, dataEndRow, 11], xSeries) as ExcelLineChartSerie;
                if (serPF != null) { serPF.Header = "Hệ số PF"; serPF.Smooth = true; }
            }

            // 6. SHEET BIỂU ĐỒ TẦN SỐ (Native Line Chart)
            var wsF = package.Workbook.Worksheets.Add("BĐ Tần số");
            wsF.View.ShowGridLines = true;
            wsF.Cells["A1:N1"].Merge = true;
            wsF.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsF.Cells["A1"].Style.Font.Italic = true;
            wsF.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsF.Cells["A2:N2"].Merge = true;
            wsF.Cells["A2"].Value = "BIỂU ĐỒ: TẦN SỐ (Hz)";
            wsF.Cells["A2"].Style.Font.Bold = true;
            wsF.Cells["A2"].Style.Font.Size = 13;
            wsF.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsF.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartF = wsF.Drawings.AddChart("chartF", eChartType.Line) as ExcelLineChart;
            if (chartF != null)
            {
                chartF.SetPosition(3, 0, 1, 0);
                chartF.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];
                var serF = chartF.Series.Add(wsData.Cells[dataStartRow, 12, dataEndRow, 12], xSeries) as ExcelLineChartSerie;
                if (serF != null) { serF.Header = "Tần số (Hz)"; serF.Smooth = true; }
            }
        }

        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> GeneratePdf(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        var reports = await GetReportsForExportAsync(request, cancellationToken);

        var pointCode = reports.FirstOrDefault()?.MeasurementPoint?.Code ?? request.MeasurementPointId.ToString();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Column(col =>
                {
                    col.Item().Text("HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM MDMS")
                        .FontFamily(Fonts.Arial).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"BÁO CÁO THÔNG SỐ VẬN HÀNH 30 PHÚT - ĐIỂM ĐO: {pointCode}")
                        .FontFamily(Fonts.Arial).FontSize(12).SemiBold();
                    col.Item().Text($"Khoảng thời gian: {request.StartTime.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm} - {request.EndTime.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm}")
                        .FontFamily(Fonts.Arial).FontSize(9).Italic();
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Thời gian
                        columns.RelativeColumn(1.2f); // Ua
                        columns.RelativeColumn(1.2f); // Ub
                        columns.RelativeColumn(1.2f); // Uc
                        columns.RelativeColumn(1.2f); // Ia
                        columns.RelativeColumn(1.2f); // Ib
                        columns.RelativeColumn(1.2f); // Ic
                        columns.RelativeColumn(1.5f); // P
                        columns.RelativeColumn(1.5f); // Q
                        columns.RelativeColumn(1.2f); // PF
                        columns.RelativeColumn(1.2f); // F
                    });

                    table.Header(header =>
                    {
                        static IContainer CellStyle(IContainer c) => c.Background(Colors.Grey.Lighten2).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3).AlignCenter();

                        header.Cell().Element(CellStyle).Text("Thời gian").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Ua (V)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Ub (V)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Uc (V)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Ia (A)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Ib (A)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Ic (A)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("P (kW)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("Q (kVAr)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("PF").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Element(CellStyle).Text("F (Hz)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                    });

                    var viCulture = CultureInfo.GetCultureInfo("vi-VN");
                    foreach (var r in reports)
                    {
                        static IContainer DataCellStyle(IContainer c) => c.Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(2).AlignRight();

                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(2).AlignCenter()
                            .Text(r.CreatedTime?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm") ?? "").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DienApPhaA?.ToString("N1", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DienApPhaB?.ToString("N1", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DienApPhaC?.ToString("N1", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DongDienPhaA?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DongDienPhaB?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.DongDienPhaC?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.CongSuatTacDungTong3Pha?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.CongSuatPhanKhangTong3Pha?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.HeSoCongSuatPF?.ToString("N3", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Element(DataCellStyle).Text(r.TanSoHeThong?.ToString("N2", viCulture) ?? "-").FontFamily(Fonts.Arial).FontSize(7.5f);
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}").FontFamily(Fonts.Arial).FontSize(8);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Trang ").FontFamily(Fonts.Arial).FontSize(8);
                        text.CurrentPageNumber().FontFamily(Fonts.Arial).FontSize(8);
                        text.Span(" / ").FontFamily(Fonts.Arial).FontSize(8);
                        text.TotalPages().FontFamily(Fonts.Arial).FontSize(8);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
