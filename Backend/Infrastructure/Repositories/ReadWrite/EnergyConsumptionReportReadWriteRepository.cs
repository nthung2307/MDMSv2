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
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

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

    private static string FormatVal(double? val, CultureInfo culture, string format = "N3", bool isSuspected = false)
    {
        if (!val.HasValue) return "-";
        var str = val.Value.ToString(format, culture);
        return isSuspected ? $"{str}*" : str;
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
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungTongKenhGiao, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungBTKenhGiao, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungCDKenhGiao, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungTDKenhGiao, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongPhanKhangTongKenhGiao, viCulture, "N3", item.IsSuspected));

            csvWriter.WriteField(FormatVal(item.SanLuongTacDungTongKenhNhan, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungBTKenhNhan, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungCDKenhNhan, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongTacDungTDKenhNhan, viCulture, "N3", item.IsSuspected));
            csvWriter.WriteField(FormatVal(item.SanLuongPhanKhangTongKenhNhan, viCulture, "N3", item.IsSuspected));

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
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungTongKenhGiao, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungBTKenhGiao, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungCDKenhGiao, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungTDKenhGiao, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongPhanKhangTongKenhGiao, viCulture, "N3"));

            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungTongKenhNhan, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungBTKenhNhan, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungCDKenhNhan, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongTacDungTDKenhNhan, viCulture, "N3"));
            csvWriter.WriteField(FormatVal(data.Total.SanLuongPhanKhangTongKenhNhan, viCulture, "N3"));

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

    public async Task<byte[]> GenerateExcel(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var data = await _readOnlyRepo.GetEnergyConsumptionReportAsync(request, cancellationToken);
        var point = await _pointSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.MeasurementPointId, cancellationToken);
        var pointCode = point?.Code ?? request.MeasurementPointId.ToString();

        var title = request.CycleType?.ToUpperInvariant() switch
        {
            "MONTHLY" => $"BÁO CÁO SẢN LƯỢNG ĐIỆN HÀNG THÁNG ({request.Month:D2}/{request.Year})",
            "YEARLY" => $"BÁO CÁO SẢN LƯỢNG ĐIỆN HÀNG NĂM ({request.Year})",
            _ => $"BÁO CÁO SẢN LƯỢNG ĐIỆN CHU KỲ 30 PHÚT ({request.Date?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")})"
        };

        using var package = new ExcelPackage();

        // 1. SHEET BÁO CÁO
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

        // Tiêu đề chính
        wsData.Cells["A2:L2"].Merge = true;
        wsData.Cells["A2"].Value = title.ToUpper();
        wsData.Cells["A2"].Style.Font.Bold = true;
        wsData.Cells["A2"].Style.Font.Size = 14;
        wsData.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
        wsData.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        wsData.Row(2).Height = 26;

        // Thông tin phụ
        wsData.Cells["A3:L3"].Merge = true;
        wsData.Cells["A3"].Value = $"Điểm đo: {pointCode} | Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        wsData.Cells["A3"].Style.Font.Bold = true;
        wsData.Cells["A3"].Style.Font.Size = 10;
        wsData.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        wsData.Row(3).Height = 20;

        // Dòng 5 & 6: Header 2 tầng (Kênh giao & Kênh nhận)
        wsData.Cells["A5:A6"].Merge = true;
        wsData.Cells["A5"].Value = "STT";
        wsData.Cells["B5:B6"].Merge = true;
        wsData.Cells["B5"].Value = "Thời gian";

        wsData.Cells["C5:G5"].Merge = true;
        wsData.Cells["C5"].Value = "KÊNH GIAO (kWh)";
        wsData.Cells["C6"].Value = "Tổng";
        wsData.Cells["D6"].Value = "Biểu 1 (BT)";
        wsData.Cells["E6"].Value = "Biểu 2 (CD)";
        wsData.Cells["F6"].Value = "Biểu 3 (TD)";
        wsData.Cells["G6"].Value = "VC (kVArh)";

        wsData.Cells["H5:L5"].Merge = true;
        wsData.Cells["H5"].Value = "KÊNH NHẬN (kWh)";
        wsData.Cells["H6"].Value = "Tổng";
        wsData.Cells["I6"].Value = "Biểu 1 (BT)";
        wsData.Cells["J6"].Value = "Biểu 2 (CD)";
        wsData.Cells["K6"].Value = "Biểu 3 (TD)";
        wsData.Cells["L6"].Value = "VC (kVArh)";

        for (int r = 5; r <= 6; r++)
        {
            wsData.Row(r).Height = 24;
            for (int c = 1; c <= 12; c++)
            {
                var cell = wsData.Cells[r, c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 121));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
            }
        }

        int rowIdx = 7;
        int stt = 1;
        foreach (var it in data.Items)
        {
            wsData.Cells[rowIdx, 1].Value = stt++;
            wsData.Cells[rowIdx, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsData.Cells[rowIdx, 1].Style.Numberformat.Format = "0";

            wsData.Cells[rowIdx, 2].Value = it.TimeLabel;
            wsData.Cells[rowIdx, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            void SetVal(int col, double? val)
            {
                var c = wsData.Cells[rowIdx, col];
                if (val.HasValue)
                {
                    c.Value = Math.Round(val.Value);
                    c.Style.Numberformat.Format = "#,##0";
                    c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    c.Value = "-";
                    c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            SetVal(3, it.SanLuongTacDungTongKenhGiao);
            SetVal(4, it.SanLuongTacDungBTKenhGiao);
            SetVal(5, it.SanLuongTacDungCDKenhGiao);
            SetVal(6, it.SanLuongTacDungTDKenhGiao);
            SetVal(7, it.SanLuongPhanKhangTongKenhGiao);
            SetVal(8, it.SanLuongTacDungTongKenhNhan);
            SetVal(9, it.SanLuongTacDungBTKenhNhan);
            SetVal(10, it.SanLuongTacDungCDKenhNhan);
            SetVal(11, it.SanLuongTacDungTDKenhNhan);
            SetVal(12, it.SanLuongPhanKhangTongKenhNhan);

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
        int dataStartRow = 7;

        // DÒNG TỔNG KẾT ĐƠN SẮC: TỔNG, MIN, MAX
        if (dataEndRow >= dataStartRow)
        {
            void AddSummaryRow(string label, string formulaFunc, System.Drawing.Color bgColor, System.Drawing.Color fontColor, bool isDoubleBottom = false)
            {
                wsData.Row(rowIdx).Height = 22;
                wsData.Cells[rowIdx, 1].Value = label;
                wsData.Cells[rowIdx, 1].Style.Font.Bold = true;
                wsData.Cells[rowIdx, 1].Style.Font.Color.SetColor(fontColor);
                wsData.Cells[rowIdx, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                wsData.Cells[rowIdx, 2].Value = ""; // Cột thời gian để trống

                for (int col = 3; col <= 12; col++)
                {
                    string colLetter = GetColumnLetter(col);
                    var c = wsData.Cells[rowIdx, col];
                    c.Formula = $"{formulaFunc}({colLetter}{dataStartRow}:{colLetter}{dataEndRow})";
                    c.Style.Numberformat.Format = "#,##0";
                    c.Style.Font.Bold = true;
                    c.Style.Font.Color.SetColor(fontColor);
                    c.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                for (int col = 1; col <= 12; col++)
                {
                    var c = wsData.Cells[rowIdx, col];
                    c.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    c.Style.Fill.BackgroundColor.SetColor(bgColor);
                    c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    c.Style.Border.Top.Color.SetColor(System.Drawing.Color.FromArgb(180, 180, 180));
                    c.Style.Border.Bottom.Style = isDoubleBottom ? ExcelBorderStyle.Double : ExcelBorderStyle.Thin;
                    c.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    c.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    c.Style.Border.Left.Color.SetColor(System.Drawing.Color.FromArgb(180, 180, 180));
                    c.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    c.Style.Border.Right.Color.SetColor(System.Drawing.Color.FromArgb(180, 180, 180));
                }
                rowIdx++;
            }

            AddSummaryRow("Tổng", "SUM", System.Drawing.Color.FromArgb(242, 244, 247), System.Drawing.Color.Black);
            AddSummaryRow("Min", "MIN", System.Drawing.Color.White, System.Drawing.Color.FromArgb(50, 50, 50));
            AddSummaryRow("Max", "MAX", System.Drawing.Color.White, System.Drawing.Color.FromArgb(50, 50, 50), true);
        }

        // AutoFit cột
        wsData.Column(1).Width = 8;
        wsData.Column(2).Width = 18;
        for (int c = 3; c <= 12; c++) wsData.Column(c).Width = 15;

        // 2. SHEET BIỂU ĐỒ KÊNH GIAO (Native Stacked Column Chart)
        if (dataEndRow >= dataStartRow)
        {
            var wsGiao = package.Workbook.Worksheets.Add("BĐ Kênh giao");
            wsGiao.View.ShowGridLines = true;
            wsGiao.Cells["A1:N1"].Merge = true;
            wsGiao.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsGiao.Cells["A1"].Style.Font.Italic = true;
            wsGiao.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsGiao.Cells["A2:N2"].Merge = true;
            wsGiao.Cells["A2"].Value = $"BIỂU ĐỒ SẢN LƯỢNG KÊNH GIAO (kWh) - {pointCode}";
            wsGiao.Cells["A2"].Style.Font.Bold = true;
            wsGiao.Cells["A2"].Style.Font.Size = 13;
            wsGiao.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsGiao.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartGiao = wsGiao.Drawings.AddChart("chartGiao", eChartType.ColumnStacked) as ExcelBarChart;
            if (chartGiao != null)
            {
                chartGiao.Title.Text = "Đồ thị sản lượng theo biểu Kênh giao (kWh)";
                chartGiao.SetPosition(3, 0, 1, 0);
                chartGiao.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];

                var serB1 = chartGiao.Series.Add(wsData.Cells[dataStartRow, 4, dataEndRow, 4], xSeries);
                serB1.Header = "Biểu 1 (BT)";

                var serB2 = chartGiao.Series.Add(wsData.Cells[dataStartRow, 5, dataEndRow, 5], xSeries);
                serB2.Header = "Biểu 2 (CD)";

                var serB3 = chartGiao.Series.Add(wsData.Cells[dataStartRow, 6, dataEndRow, 6], xSeries);
                serB3.Header = "Biểu 3 (TD)";
            }

            // 3. SHEET BIỂU ĐỒ KÊNH NHẬN (Native Stacked Column Chart)
            var wsNhan = package.Workbook.Worksheets.Add("BĐ Kênh nhận");
            wsNhan.View.ShowGridLines = true;
            wsNhan.Cells["A1:N1"].Merge = true;
            wsNhan.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            wsNhan.Cells["A1"].Style.Font.Italic = true;
            wsNhan.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            wsNhan.Cells["A2:N2"].Merge = true;
            wsNhan.Cells["A2"].Value = $"BIỂU ĐỒ SẢN LƯỢNG KÊNH NHẬN (kWh) - {pointCode}";
            wsNhan.Cells["A2"].Style.Font.Bold = true;
            wsNhan.Cells["A2"].Style.Font.Size = 13;
            wsNhan.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(15, 76, 129));
            wsNhan.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var chartNhan = wsNhan.Drawings.AddChart("chartNhan", eChartType.ColumnStacked) as ExcelBarChart;
            if (chartNhan != null)
            {
                chartNhan.Title.Text = "Đồ thị sản lượng theo biểu Kênh nhận (kWh)";
                chartNhan.SetPosition(3, 0, 1, 0);
                chartNhan.SetSize(1150, 550);
                var xSeries = wsData.Cells[dataStartRow, 2, dataEndRow, 2];

                var serB1 = chartNhan.Series.Add(wsData.Cells[dataStartRow, 9, dataEndRow, 9], xSeries);
                serB1.Header = "Biểu 1 (BT)";

                var serB2 = chartNhan.Series.Add(wsData.Cells[dataStartRow, 10, dataEndRow, 10], xSeries);
                serB2.Header = "Biểu 2 (CD)";

                var serB3 = chartNhan.Series.Add(wsData.Cells[dataStartRow, 11, dataEndRow, 11], xSeries);
                serB3.Header = "Biểu 3 (TD)";
            }
        }

        return await package.GetAsByteArrayAsync();
    }

    private static string GetColumnLetter(int columnNumber)
    {
        string columnName = "";
        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            columnNumber = (columnNumber - modulo) / 26;
        }
        return columnName;
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
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungTongKenhGiao, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungBTKenhGiao, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungCDKenhGiao, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungTDKenhGiao, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongPhanKhangTongKenhGiao, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungTongKenhNhan, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungBTKenhNhan, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungCDKenhNhan, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongTacDungTDKenhNhan, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(FormatVal(item.SanLuongPhanKhangTongKenhNhan, viCulture, "N2", item.IsSuspected)).FontFamily(Fonts.Arial).FontSize(7.5f);
                    }

                    if (data.Total != null)
                    {
                        static IContainer TotalCellStyle(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black).Padding(2).AlignRight();

                        table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(data.Total.TimeLabel).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungTongKenhGiao, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungBTKenhGiao, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungCDKenhGiao, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungTDKenhGiao, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongPhanKhangTongKenhGiao, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();

                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungTongKenhNhan, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungBTKenhNhan, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungCDKenhNhan, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongTacDungTDKenhNhan, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
                        table.Cell().Element(TotalCellStyle).Text(FormatVal(data.Total.SanLuongPhanKhangTongKenhNhan, viCulture, "N2")).FontFamily(Fonts.Arial).FontSize(8).Bold();
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
