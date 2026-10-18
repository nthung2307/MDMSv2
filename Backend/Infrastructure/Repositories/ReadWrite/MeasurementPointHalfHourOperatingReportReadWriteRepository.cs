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

namespace Infrastructure.Repositories.ReadWrite;

public class MeasurementPointHalfHourOperatingReportReadWriteRepository :
    RestfulRepository<MeasurementPointHalfHourOperatingReportEntity, AppReadWriteDbContext>,
    IMeasurementPointHalfHourOperatingReportReadWriteRepository
{
    private readonly DbSet<MeasurementPointHalfHourOperatingReportEntity> _dbSet;
    private readonly DbSet<MeasurementPointMonitoringHourlyEntity> _hourlySet;
    private readonly DbSet<MeasurementPointMonitoringEntity> _monitoringSet;
    private readonly DbSet<MeasurementPointsEntity> _pointSet;

    public MeasurementPointHalfHourOperatingReportReadWriteRepository(
        AppReadWriteDbContext context,
        ISnowflakeIdService snowflakeIdService) : base(context, snowflakeIdService)
    {
        _dbSet = context.Set<MeasurementPointHalfHourOperatingReportEntity>();
        _hourlySet = context.Set<MeasurementPointMonitoringHourlyEntity>();
        _monitoringSet = context.Set<MeasurementPointMonitoringEntity>();
        _pointSet = context.Set<MeasurementPointsEntity>();
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

    public Task<byte[]> GenerateExcel(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        return GenerateCsv(request, cancellationToken);
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
