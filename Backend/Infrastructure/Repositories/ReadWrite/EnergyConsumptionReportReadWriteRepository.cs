using System.Globalization;
using System.Text;
using Application.DataTransferObjects.Requests.Report;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using CsvHelper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Repositories.ReadWrite;

public class EnergyConsumptionReportReadWriteRepository :
    RestfulRepository<MeasurementPointDailyEnergyReportEntity, AppReadWriteDbContext>,
    IEnergyConsumptionReportReadWriteRepository
{
    private readonly IEnergyConsumptionReportReadOnlyRepository _readOnlyRepo;
    private readonly DbSet<MeasurementPointsEntity> _pointSet;

    public EnergyConsumptionReportReadWriteRepository(
        AppReadWriteDbContext context,
        ISnowflakeIdService snowflakeIdService,
        IEnergyConsumptionReportReadOnlyRepository readOnlyRepo) : base(context, snowflakeIdService)
    {
        _readOnlyRepo = readOnlyRepo;
        _pointSet = context.Set<MeasurementPointsEntity>();
    }

    public async Task<byte[]> GenerateCsv(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _readOnlyRepo.GetEnergyConsumptionReportAsync(request, cancellationToken);

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(true));
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        var viCulture = CultureInfo.GetCultureInfo("vi-VN");

        // Main Header
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
        if (request.CompareWithPrevious)
        {
            csvWriter.WriteField("So sánh kỳ trước");
            csvWriter.WriteField("");
            csvWriter.WriteField("");
            csvWriter.WriteField("");
        }
        await csvWriter.NextRecordAsync();

        // Sub Header
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
        if (request.CompareWithPrevious)
        {
            csvWriter.WriteField("Giao kỳ trước");
            csvWriter.WriteField("Tăng/Giảm Giao (%)");
            csvWriter.WriteField("Nhận kỳ trước");
            csvWriter.WriteField("Tăng/Giảm Nhận (%)");
        }
        await csvWriter.NextRecordAsync();

        // Data Rows
        foreach (var item in data.Items)
        {
            csvWriter.WriteField(item.TimeLabel);
            csvWriter.WriteField(item.SanLuongTacDungTongKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungBTKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungCDKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungTDKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongPhanKhangTongKenhGiao.ToString("N3", viCulture));

            csvWriter.WriteField(item.SanLuongTacDungTongKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungBTKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungCDKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongTacDungTDKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(item.SanLuongPhanKhangTongKenhNhan.ToString("N3", viCulture));

            if (request.CompareWithPrevious)
            {
                csvWriter.WriteField(item.PreviousSanLuongTacDungTongKenhGiao?.ToString("N3", viCulture) ?? "-");
                csvWriter.WriteField(item.DeltaPercentageKenhGiao.HasValue ? $"{item.DeltaPercentageKenhGiao.Value:+0.0;-0.0;0.0}%" : "-");
                csvWriter.WriteField(item.PreviousSanLuongTacDungTongKenhNhan?.ToString("N3", viCulture) ?? "-");
                csvWriter.WriteField(item.DeltaPercentageKenhNhan.HasValue ? $"{item.DeltaPercentageKenhNhan.Value:+0.0;-0.0;0.0}%" : "-");
            }
            await csvWriter.NextRecordAsync();
        }

        // Total Row
        if (data.Total != null)
        {
            csvWriter.WriteField(data.Total.TimeLabel);
            csvWriter.WriteField(data.Total.SanLuongTacDungTongKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungBTKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungCDKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungTDKenhGiao.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongPhanKhangTongKenhGiao.ToString("N3", viCulture));

            csvWriter.WriteField(data.Total.SanLuongTacDungTongKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungBTKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungCDKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongTacDungTDKenhNhan.ToString("N3", viCulture));
            csvWriter.WriteField(data.Total.SanLuongPhanKhangTongKenhNhan.ToString("N3", viCulture));

            if (request.CompareWithPrevious)
            {
                csvWriter.WriteField(data.Total.PreviousSanLuongTacDungTongKenhGiao?.ToString("N3", viCulture) ?? "-");
                csvWriter.WriteField(data.Total.DeltaPercentageKenhGiao.HasValue ? $"{data.Total.DeltaPercentageKenhGiao.Value:+0.0;-0.0;0.0}%" : "-");
                csvWriter.WriteField(data.Total.PreviousSanLuongTacDungTongKenhNhan?.ToString("N3", viCulture) ?? "-");
                csvWriter.WriteField(data.Total.DeltaPercentageKenhNhan.HasValue ? $"{data.Total.DeltaPercentageKenhNhan.Value:+0.0;-0.0;0.0}%" : "-");
            }
            await csvWriter.NextRecordAsync();
        }

        await streamWriter.FlushAsync(cancellationToken);
        return memoryStream.ToArray();
    }

    public Task<byte[]> GenerateExcel(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        return GenerateCsv(request, cancellationToken);
    }

    public async Task<byte[]> GeneratePdf(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        var data = await _readOnlyRepo.GetEnergyConsumptionReportAsync(request, cancellationToken);
        var point = await _pointSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.MeasurementPointId, cancellationToken);
        var pointCode = point?.Code ?? request.MeasurementPointId.ToString();

        var title = request.CycleType?.ToUpperInvariant() switch
        {
            "MONTHLY" => $"BÁO CÁO SẢN LƯỢNG ĐIỆN HÀNG THÁNG ({request.Month:D2}/{request.Year})",
            "YEARLY" => $"BÁO CÁO SẢN LƯỢNG ĐIỆN HÀNG NĂM ({request.Year})",
            _ => $"BÁO CÁO SẢN LƯỢNG ĐIỆN CHU KỲ 30 PHÚT ({request.Date?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")})"
        };

        var viCulture = CultureInfo.GetCultureInfo("vi-VN");

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
                    col.Item().Text($"{title} - ĐIỂM ĐO: {pointCode} (Hệ số nhân: {point?.MultiplierFactor ?? 1.0})")
                        .FontFamily(Fonts.Arial).FontSize(12).SemiBold();
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(90);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().RowSpan(2).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(3).AlignCenter().AlignMiddle().Text("Thời gian").FontFamily(Fonts.Arial).FontSize(9).Bold();
                        header.Cell().ColumnSpan(5).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(3).AlignCenter().AlignMiddle().Text("Sản lượng Giao (kWh)").FontFamily(Fonts.Arial).FontSize(9).Bold();
                        header.Cell().ColumnSpan(5).Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(3).AlignCenter().AlignMiddle().Text("Sản lượng Nhận (kWh)").FontFamily(Fonts.Arial).FontSize(9).Bold();

                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Tổng").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 1 (BT)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 2 (CD)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 3 (TD)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("VC (kVArh)").FontFamily(Fonts.Arial).FontSize(8).Bold();

                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Tổng").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 1 (BT)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 2 (CD)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("Biểu 3 (TD)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                        header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).Padding(2).AlignCenter().Text("VC (kVArh)").FontFamily(Fonts.Arial).FontSize(8).Bold();
                    });

                    foreach (var item in data.Items)
                    {
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(item.TimeLabel).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungTongKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungBTKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungCDKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungTDKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongPhanKhangTongKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungTongKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungBTKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungCDKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongTacDungTDKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(item.SanLuongPhanKhangTongKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(7.5f);
                    }

                    if (data.Total != null)
                    {
                        static IContainer TotalCellStyle(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black).Padding(2).AlignRight();

                        table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(data.Total.TimeLabel).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungTongKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungBTKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungCDKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungTDKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongPhanKhangTongKenhGiao.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();

                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungTongKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungBTKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungCDKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongTacDungTDKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(data.Total.SanLuongPhanKhangTongKenhNhan.ToString("N2", viCulture)).FontFamily(Fonts.Arial).FontSize(8).Bold();
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Thời gian xuất: {DateTime.Now:HH:mm:ss dd/MM/yyyy}").FontFamily(Fonts.Arial).FontSize(8);
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
