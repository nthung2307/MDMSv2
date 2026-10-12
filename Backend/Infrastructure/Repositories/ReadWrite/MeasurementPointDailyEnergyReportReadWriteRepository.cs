using System.Globalization;
using System.Text;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
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

public class MeasurementPointDailyEnergyReportReadWriteRepository : RestfulRepository<MeasurementPointDailyEnergyReportEntity, AppReadWriteDbContext>, IMeasurementPointDailyEnergyReportReadWriteRepository
{
    private readonly DbSet<MeasurementPointDailyEnergyReportEntity> _measurementPointDailyEnergyReport;

    public MeasurementPointDailyEnergyReportReadWriteRepository(AppReadWriteDbContext context, ISnowflakeIdService snowflakeIdService) : base(context, snowflakeIdService)
    {
        _measurementPointDailyEnergyReport = context.Set<MeasurementPointDailyEnergyReportEntity>();
    }

    public async Task<byte[]> GeneratePdf(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        var startLocal = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var endLocal = startLocal.AddMonths(1);
        var startTime = startLocal.UtcDateTime;
        var endTime = endLocal.UtcDateTime;
        var query = _measurementPointDailyEnergyReport.AsNoTracking()
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= startTime && m.CreatedTime < endTime);

        var reports = await query
            .OrderBy(x => x.CreatedTime)
            .ToListAsync(cancellationToken);

        var backgroundImageData = "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";

        byte[] backgroundData = Convert.FromBase64String(backgroundImageData);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Set A4 Landscape
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                // Header with titles
                page.Header().Row(row =>
                {
                    row.ConstantItem(50).Width(50).Image(backgroundData, ImageScaling.FitWidth);
                    // Title section
                    row.RelativeItem().PaddingLeft(5)
                        .Text("- Hệ thống quản lý dữ liệu điểm đo")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(12)); 

                    row.RelativeItem().AlignRight()
                        .Text("Báo cáo chỉ số điện hàng ngày")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(12)); 
                });

                // Main title
                page.Content().PaddingTop(14).Column(column =>
                {
                    column.Item().AlignCenter().PaddingBottom(14)
                        .Text("Báo cáo chỉ số điện hàng ngày")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(16).Bold());

                    // Table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120); // Thời gian
                            
                            // Kênh giao
                            columns.RelativeColumn(1.2f); // Tổng
                            columns.RelativeColumn(1.2f); // Biểu 1
                            columns.RelativeColumn(1.2f); // Biểu 2
                            columns.RelativeColumn(1.2f); // Biểu 3
                            columns.RelativeColumn(1f);   // VC

                            // Kênh nhận
                            columns.RelativeColumn(1.2f); // Tổng
                            columns.RelativeColumn(1.2f); // Biểu 1
                            columns.RelativeColumn(1.2f); // Biểu 2
                            columns.RelativeColumn(1.2f); // Biểu 3
                            columns.RelativeColumn(1f);   // VC
                        });
                        
                        table.Header(header =>
                        {
                            header.Cell().RowSpan(2).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Thời gian").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().ColumnSpan(5).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Kênh giao (kWh)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().ColumnSpan(5).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Kênh nhận (kWh)").FontFamily(Fonts.Arial).FontSize(11).Bold();

                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Tổng").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 1 (BT)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 2 (CD)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 3 (TD)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("VC (kVArh)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Tổng").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 1 (BT)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 2 (CD)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("Biểu 3 (TD)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignMiddle().AlignCenter().Text("VC (kVArh)").FontFamily(Fonts.Arial).FontSize(11).Bold();
                        });
                        
                        for (var i = 0; i < reports.Count; i++)
                        {
                            var report = reports[i];
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).ShowEntire().Text(report.CreatedTime?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? string.Empty).FontFamily(Fonts.Arial).FontSize(11);
                            
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPTongKenhGiao)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao)).FontFamily(Fonts.Arial).FontSize(11);
                            
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPTongKenhNhan)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan)).FontFamily(Fonts.Arial).FontSize(11);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).AlignRight().ShowEntire().Text(FormatValue(report.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan)).FontFamily(Fonts.Arial).FontSize(11);
                        }
                    });
                });

                // Footer with page numbers
                page.Footer().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span($"Thời gian: {DateTime.Now.AddHours(7):HH:mm:ss, dd/MM/yyyy}")
                            .FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                    });

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Trang ").FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.CurrentPageNumber().FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.Span(" / ").FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.TotalPages().FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                    });
                });
            });
        });
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateCsv(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        var startLocal = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var endLocal = startLocal.AddMonths(1);
        var startTime = startLocal.UtcDateTime;
        var endTime = endLocal.UtcDateTime;
        var query = await _measurementPointDailyEnergyReport.AsNoTracking()
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= startTime && m.CreatedTime < endTime)
            .OrderBy(x => x.CreatedTime)
            .ToListAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        #region Main header
        csvWriter.WriteField("Thời gian");
        
        csvWriter.WriteField("");
        csvWriter.WriteField("");
        csvWriter.WriteField("Kênh giao (kWh)");
        csvWriter.WriteField("");
        csvWriter.WriteField("");
        
        csvWriter.WriteField("");
        csvWriter.WriteField("");
        csvWriter.WriteField("Kênh nhận (kWh)");
        csvWriter.WriteField("");
        csvWriter.WriteField("");
        #endregion
        
        await csvWriter.NextRecordAsync();
        
        #region Sub header
        csvWriter.WriteField("");
        
        csvWriter.WriteField("Tổng");
        csvWriter.WriteField("Biểu 1 (BT)");
        csvWriter.WriteField("Biểu 2 (CD)");
        csvWriter.WriteField("Biểu 3 (TD)");
        csvWriter.WriteField("VC (kVArh)");
        
        csvWriter.WriteField("Tổng");
        csvWriter.WriteField("Biểu 1 (BT)");
        csvWriter.WriteField("Biểu 2 (CD)");
        csvWriter.WriteField("Biểu 3 (TD)");
        csvWriter.WriteField("VC (kVArh)");
        #endregion
        
        await csvWriter.NextRecordAsync();
        
        for (var i = 0; i < query.Count; i++)
        {
            var report = query[i];
            csvWriter.WriteField(report.CreatedTime?.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? string.Empty);
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPTongKenhGiao));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPTongKenhNhan));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan));
            csvWriter.WriteField(FormatValue(report.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan));
            await csvWriter.NextRecordAsync();
        }
        await streamWriter.FlushAsync(cancellationToken);
        var csvBytes = memoryStream.ToArray();

        return csvBytes;
    }

    private string FormatValue(string? value)
    {
        if (value == null) return "N/A";
        var viCulture = CultureInfo.GetCultureInfo("vi-VN");
        double valueNumber = double.Parse(value, CultureInfo.InvariantCulture);
        if (valueNumber % 1 == 0)
        {
            return valueNumber.ToString("N0", viCulture);
        }
        return valueNumber.ToString("N2", viCulture);
    }
}