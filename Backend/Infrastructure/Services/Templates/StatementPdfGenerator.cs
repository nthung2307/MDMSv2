using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services.Templates;

public class StatementPdfGenerator : IStatementPdfGenerator
{
    public static bool IsGarbledEncoding(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return text.Contains('├') || text.Contains('ß') || text.Contains('─') || text.Contains('║') || text.Contains('¥') || text.Contains('┬') || text.Contains('┐') || text.Contains('└');
    }

    public const string DefaultLogoBase64 = "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";

    public static byte[] GetTopRightDecorBytes()
    {
        int width = 662;
        int height = 464;
        using var bmp = new System.Drawing.Bitmap(width, height);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);

            // Path 1: Green curved stripe (#9FC153)
            using (var path1 = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path1.StartFigure();
                path1.AddLine(190f, 0f, 661f, 0f);
                path1.AddLine(661f, 0f, 661f, 464f);
                path1.AddBezier(661f, 464f, 560f, 328.666f, 353.2f, 52.2f, 334f, 29f);
                path1.AddBezier(334f, 29f, 314.8f, 5.8f, 230f, 0f, 190f, 0f);
                path1.CloseFigure();

                using var brush1 = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#9FC153"));
                g.FillPath(brush1, path1);
            }

            // Path 2: Cyan/Blue stripe (#0093ED)
            using (var path2 = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path2.StartFigure();
                path2.AddLine(472f, 0f, 660f, 0f);
                path2.AddLine(660f, 0f, 660f, 256f);
                path2.CloseFigure();

                using var brush2 = new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml("#0093ED"));
                g.FillPath(brush2, path2);
            }
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }
    private readonly IStatementPdfTemplateReadOnlyRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatementPdfGenerator> _logger;

    public StatementPdfGenerator(
        IStatementPdfTemplateReadOnlyRepository repository,
        IConfiguration configuration,
        ILogger<StatementPdfGenerator> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default)
    {
        if (template == null)
        {
            if (statement.StatementPdfTemplateId.HasValue && statement.StatementPdfTemplateId.Value > 0)
            {
                template = await _repository.GetByIdAsync(statement.StatementPdfTemplateId.Value, cancellationToken);
            }

            if (template == null)
            {
                template = await _repository.GetDefaultAsync(cancellationToken);
            }
        }

        template ??= GetFallbackDefaultTemplate();

        // Sử dụng thống nhất 100% QuestPDF Engine với bố cục co giãn cột động (đồng nhất tuyệt đối với TemplateNotice trên Web)
        var stream = new MemoryStream();
        BuildQuestPdfDocument(statement, template).GeneratePdf(stream);
        return stream.ToArray();
    }

    public Task<byte[]> GeneratePreviewPdfAsync(PreviewStatementPdfTemplateRequest templateRequest, CancellationToken cancellationToken = default)
    {
        var template = new StatementPdfTemplateEntity
        {
            HeaderTitle = templateRequest.HeaderTitle,
            SubHeaderTitle = templateRequest.SubHeaderTitle,
            CompanyName = templateRequest.CompanyName,
            CompanyAddress = templateRequest.CompanyAddress,
            ContactPhone = templateRequest.ContactPhone,
            LogoBase64 = templateRequest.LogoBase64,
            PaperSize = templateRequest.PaperSize,
            EngineType = "QUEST_PDF",
            ExcelTemplateBase64 = templateRequest.ExcelTemplateBase64,
            ExcelTemplateFileName = templateRequest.ExcelTemplateFileName,
            DisplayColumnsJson = templateRequest.DisplayColumnsJson,
            FooterNotes = templateRequest.FooterNotes,
            RequireDualSignatures = templateRequest.RequireDualSignatures
        };

        var mockStatement = CreateMockStatement();

        var stream = new MemoryStream();
        BuildQuestPdfDocument(mockStatement, template).GeneratePdf(stream);
        return Task.FromResult(stream.ToArray());
    }

    public byte[] GetStandardExcelTemplate(string paperSize = "A5_LANDSCAPE")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("MeterReading");

        var isLandscape = string.Equals(paperSize, "A5_LANDSCAPE", StringComparison.OrdinalIgnoreCase);

        // Page setup
        ws.PrinterSettings.Orientation = isLandscape ? eOrientation.Landscape : eOrientation.Portrait;
        ws.PrinterSettings.PaperSize = isLandscape ? ePaperSize.A5 : ePaperSize.A4;
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 1;
        ws.PrinterSettings.LeftMargin = 0.3M;
        ws.PrinterSettings.RightMargin = 0.3M;
        ws.PrinterSettings.TopMargin = 0.3M;
        ws.PrinterSettings.BottomMargin = 0.3M;

        ws.Cells.Style.Font.Name = "Arial";
        ws.Cells.Style.Font.Size = 9;

        // 1. Company Header
                // Embed VSIP Logo (Top-Left) and Decorative Stripes (Top-Right)
        try
        {
            var logoBytes = Convert.FromBase64String(DefaultLogoBase64);
            var logoPic = ws.Drawings.AddPicture("VsipLogo", new MemoryStream(logoBytes));
            logoPic.SetPosition(0, 4, 0, 8);
            logoPic.SetSize(120, 44);
        }
        catch { }

        try
        {
            var decorBytes = GetTopRightDecorBytes();
            var decorPic = ws.Drawings.AddPicture("TopRightDecor", new MemoryStream(decorBytes));
            decorPic.SetPosition(0, 0, 7, 10);
            decorPic.SetSize(110, 77);
        }
        catch { }

        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "{{CompanyName}}";
        ws.Cells["A1"].Style.Font.Size = 13;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A2:H2"].Merge = true;
        ws.Cells["A2"].Value = "{{CompanyAddress}}";
        ws.Cells["A2"].Style.Font.Size = 8;
        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A3:H3"].Merge = true;
        ws.Cells["A3"].Value = "{{ContactPhone}}";
        ws.Cells["A3"].Style.Font.Size = 8;
        ws.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // 2. Titles
        ws.Cells["A5:H5"].Merge = true;
        ws.Cells["A5"].Value = "{{HeaderTitle}}";
        ws.Cells["A5"].Style.Font.Size = 12;
        ws.Cells["A5"].Style.Font.Bold = true;
        ws.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A6:H6"].Merge = true;
        ws.Cells["A6"].Value = "{{SubHeaderTitle}}";
        ws.Cells["A6"].Style.Font.Size = 11;
        ws.Cells["A6"].Style.Font.Bold = true;
        ws.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A7:H7"].Merge = true;
        ws.Cells["A7"].Value = "(M&E Copy – Liên cho Phòng Cơ Điện)";
        ws.Cells["A7"].Style.Font.Size = 8.5f;
        ws.Cells["A7"].Style.Font.Italic = true;
        ws.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // 3. Customer & Meter Metadata
        ws.Cells["A9:H9"].Merge = true;
        ws.Cells["A9"].Value = "Tenant Name (Tên khách hàng): {{CustomerName}}";
        ws.Cells["A9"].Style.Font.Bold = true;

        ws.Cells["A10:H10"].Merge = true;
        ws.Cells["A10"].Value = "Meter ID (Mã điểm đo): {{PointCode}}";
        ws.Cells["A10"].Style.Font.Bold = true;

        ws.Cells["A11:B11"].Merge = true;
        ws.Cells["A11"].Value = "Reading Time (Giờ ghi): {{ReadingTime}}";

        ws.Cells["C11:E11"].Merge = true;
        ws.Cells["C11"].Value = "Reading Date (Ngày ghi): {{ReadingDate}}";

        ws.Cells["F11:H11"].Merge = true;
        ws.Cells["F11"].Value = "For month (Tháng): {{ForMonth}}";

        // 4. Data Table Header (Horizontal Grid 8 Columns - Matches TemplateNotice exactly)
        int tableStartRow = 13;
        ws.Cells[tableStartRow, 1].Value = "Description\n(Mô tả)";
        ws.Cells[tableStartRow, 2].Value = "Normal hours\nkWh meter 1\n(Giờ bình thường)";
        ws.Cells[tableStartRow, 3].Value = "Peak hours\nkWh meter 2\n(Giờ cao điểm)";
        ws.Cells[tableStartRow, 4].Value = "Off-peak hours\nkWh meter 3\n(Giờ thấp điểm)";
        ws.Cells[tableStartRow, 5].Value = "Total active power\n(Tổng công suất tác dụng)\nAP";
        ws.Cells[tableStartRow, 6].Value = "Total reactive power\n(Tổng công suất phản kháng)\nAQ";
        ws.Cells[tableStartRow, 7].Value = "Cos φ TB\n(≥0.9)";
        ws.Cells[tableStartRow, 8].Value = "Remarks\n(Ghi chú)";

        using (var headerRange = ws.Cells[tableStartRow, 1, tableStartRow, 8])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headerRange.Style.WrapText = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 244, 248));
            headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        ws.Row(tableStartRow).Height = 32;

        // Data Rows (4 rows matrix)
        var rowsData = new (string Label, string Col2, string Col3, string Col4, string Col5, string Col6, string Col7, string Col8, bool IsBold)[]
        {
            ("Previous meter reading\n(Chỉ số cũ)\nReading date: {{FromDate}}\n(Ngày ghi)", "{{PreviousBt}}", "{{PreviousCd}}", "{{PreviousTd}}", "{{PreviousSg}}", "{{PreviousVc}}", "", "", false),
            ("Current meter reading\n(Chỉ số mới)", "{{Bt}}", "{{Cd}}", "{{Td}}", "{{Sg}}", "{{Vc}}", "", "", false),
            ("Multiplier\n(hệ số)", "{{Multiplier}}", "{{Multiplier}}", "{{Multiplier}}", "{{Multiplier}}", "{{Multiplier}}", "", "", false),
            ("Actual consumption\n(Số tiêu thụ thực tế)", "{{ConsumtionBt}}", "{{ConsumtionCd}}", "{{ConsumtionTd}}", "{{ConsumtionSg}}", "{{ConsumtionVc}}", "{{CosPhi}}", "", true)
        };

        int currentRow = tableStartRow + 1;
        foreach (var r in rowsData)
        {
            ws.Cells[currentRow, 1].Value = r.Label;
            ws.Cells[currentRow, 2].Value = r.Col2;
            ws.Cells[currentRow, 3].Value = r.Col3;
            ws.Cells[currentRow, 4].Value = r.Col4;
            ws.Cells[currentRow, 5].Value = r.Col5;
            ws.Cells[currentRow, 6].Value = r.Col6;
            ws.Cells[currentRow, 7].Value = r.Col7;
            ws.Cells[currentRow, 8].Value = r.Col8;

            ws.Cells[currentRow, 1].Style.WrapText = true;
            if (r.IsBold)
            {
                ws.Cells[currentRow, 1, currentRow, 8].Style.Font.Bold = true;
                ws.Cells[currentRow, 1, currentRow, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentRow, 1, currentRow, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(248, 249, 250));
            }

            ws.Cells[currentRow, 2, currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[currentRow, 2, currentRow, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int c = 1; c <= 8; c++)
            {
                ws.Cells[currentRow, c].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[currentRow, c].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
            ws.Row(currentRow).Height = 28;
            currentRow++;
        }

        // 5. Pmax note and confirmation
        currentRow++;
        ws.Cells[currentRow, 1, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 1].Value = "* Công suất cực đại Pmax: {{Pmax}} kW, ghi nhận hồi: {{PmaxTime}}";
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        currentRow++;

        ws.Cells[currentRow, 1, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 1].Value = "The above meter reading has been jointly read by the following Parties:\n(Các chỉ số trên đã được các Bên dưới đây xác nhận)";
        ws.Cells[currentRow, 1].Style.Font.Size = 8;
        ws.Cells[currentRow, 1].Style.WrapText = true;
        currentRow += 2;

        // 6. Dual Signatures
        ws.Cells[currentRow, 1, currentRow, 4].Merge = true;
        ws.Cells[currentRow, 1].Value = "Party A (VSIP Hai Phong Co., Ltd.)\nBên A (Công ty TNHH VSIP Hải Phòng)";
        ws.Cells[currentRow, 1].Style.Font.Bold = true;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[currentRow, 1].Style.WrapText = true;

        ws.Cells[currentRow, 5, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 5].Value = "Party B (Tenant)\nBên B (Khách hàng)";
        ws.Cells[currentRow, 5].Style.Font.Bold = true;
        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[currentRow, 5].Style.WrapText = true;

        currentRow++;
        ws.Cells[currentRow, 1, currentRow, 4].Merge = true;
        ws.Cells[currentRow, 1].Value = "\n\nName and Signature:\n(Ký và ghi rõ họ tên)";
        ws.Cells[currentRow, 1].Style.Font.Size = 8;
        ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[currentRow, 1].Style.WrapText = true;

        ws.Cells[currentRow, 5, currentRow, 8].Merge = true;
        ws.Cells[currentRow, 5].Value = "\n\nName and Signature:\n(Ký và ghi rõ họ tên)";
        ws.Cells[currentRow, 5].Style.Font.Size = 8;
        ws.Cells[currentRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[currentRow, 5].Style.WrapText = true;

        // Column widths
        ws.Column(1).Width = 28;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 14;
        ws.Column(5).Width = 16;
        ws.Column(6).Width = 16;
        ws.Column(7).Width = 12;
        ws.Column(8).Width = 12;

        return package.GetAsByteArray();
    }

    public Task<byte[]> GenerateExcelStatementAsync(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity? template = null, CancellationToken cancellationToken = default)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        template ??= GetFallbackDefaultTemplate();

        ExcelPackage package;
        if (!string.IsNullOrWhiteSpace(template.ExcelTemplateBase64))
        {
            try
            {
                var cleanBase64 = template.ExcelTemplateBase64.Contains(",")
                    ? template.ExcelTemplateBase64.Substring(template.ExcelTemplateBase64.IndexOf(",") + 1)
                    : template.ExcelTemplateBase64;
                var bytes = Convert.FromBase64String(cleanBase64.Trim());
                package = new ExcelPackage(new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể đọc file template Excel tải lên từ Base64. Sử dụng mẫu Excel chuẩn: {Message}", ex.Message);
                package = new ExcelPackage(new MemoryStream(GetStandardExcelTemplate(template.PaperSize)));
            }
        }
        else
        {
            package = new ExcelPackage(new MemoryStream(GetStandardExcelTemplate(template.PaperSize)));
        }

        using (package)
        {
                        var ws = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("MeterReading");

            // Ensure VSIP Logo and Top-Right Decorative Emblem are embedded in the worksheet
            if (ws.Drawings["VsipLogo"] == null)
            {
                try
                {
                    var logoBase64 = !string.IsNullOrWhiteSpace(template.LogoBase64) ? template.LogoBase64 : DefaultLogoBase64;
                    if (logoBase64.Contains(",")) logoBase64 = logoBase64.Substring(logoBase64.IndexOf(",") + 1);
                    var logoBytes = Convert.FromBase64String(logoBase64.Trim());
                    var logoPic = ws.Drawings.AddPicture("VsipLogo", new MemoryStream(logoBytes));
                    logoPic.SetPosition(0, 4, 0, 8);
                    logoPic.SetSize(120, 44);
                }
                catch { }
            }

            if (ws.Drawings["TopRightDecor"] == null)
            {
                try
                {
                    var decorBytes = GetTopRightDecorBytes();
                    var decorPic = ws.Drawings.AddPicture("TopRightDecor", new MemoryStream(decorBytes));
                    decorPic.SetPosition(0, 0, 7, 10);
                    decorPic.SetSize(110, 77);
                }
                catch { }
            }

            var customerName = statement.MeasurementPoint?.Customer?.FullName ?? "---";
            var customerCode = statement.MeasurementPoint?.Customer?.CustomerCode ?? "---";
            var address = statement.MeasurementPoint?.Address ?? statement.MeasurementPoint?.Customer?.Address ?? "---";
            var pointName = statement.MeasurementPoint?.Description ?? statement.MeasurementPoint?.Code ?? "---";
            var pointCode = statement.MeasurementPoint?.Code ?? "---";
            var serialNumber = !string.IsNullOrWhiteSpace(statement.SerialNumber) ? statement.SerialNumber : "---";
            var mult = statement.Multiplier > 0 ? statement.Multiplier : (statement.MeasurementPoint?.MultiplierFactor ?? 1);

            var fromDateStr = statement.PreviousDay.HasValue && statement.PreviousMonth.HasValue && statement.PreviousYear.HasValue && statement.PreviousDay.Value > 0
                ? $"{statement.PreviousDay.Value:00}/{statement.PreviousMonth.Value:00}/{statement.PreviousYear.Value}"
                : ".........";
            var toDateStr = $"{statement.Day:00}/{statement.Month:00}/{statement.Year}";
            var periodStr = $"{fromDateStr} - {toDateStr}";

            var diffBt = statement.Bt - statement.PreviousBt;
            var diffCd = statement.Cd - statement.PreviousCd;
            var diffTd = statement.Td - statement.PreviousTd;
            var diffSg = statement.Sg - statement.PreviousSg;
            var diffVc = statement.Vc - statement.PreviousVc;

            var pmaxStr = statement.Pmax > 0 ? statement.Pmax.ToString("N2") : "-";
            var billingLocal = statement.BillingTimestamp.HasValue
                ? statement.BillingTimestamp.Value.ToOffset(TimeSpan.FromHours(7))
                : (DateTimeOffset?)null;
            var readingTimeStr = billingLocal.HasValue ? billingLocal.Value.ToString("HH:mm:ss") : "00:00:00";
            var readingDateStr = billingLocal.HasValue ? billingLocal.Value.ToString("dd/MM/yyyy") : toDateStr;
            var forMonthStr = $"{statement.Month:00}/{statement.Year}";
            var pmaxTimeStr = statement.PmaxTime.HasValue
                ? statement.PmaxTime.Value.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm:ss")
                : "---";
            var billingTimeStr = billingLocal.HasValue ? billingLocal.Value.ToString("dd/MM/yyyy HH:mm:ss") : toDateStr;

            // Cos phi calculation (ưu tiên giá trị đã lưu, hoặc tự động tính từ AP và AQ)
            var cosPhiStr = "-";
            if (statement.CosPhi.HasValue && statement.CosPhi.Value > 0m)
            {
                cosPhiStr = statement.CosPhi.Value.ToString("F2");
            }
            else if (statement.ConsumtionSg > 0m && statement.ConsumtionVc >= 0m)
            {
                var p = (double)statement.ConsumtionSg;
                var q = (double)statement.ConsumtionVc;
                var s = Math.Sqrt((p * p) + (q * q));
                if (s > 0)
                {
                    cosPhiStr = (p / s).ToString("F2");
                }
            }

            var compNameExcel = !string.IsNullOrWhiteSpace(template.CompanyName) && !IsGarbledEncoding(template.CompanyName) ? template.CompanyName : "CÔNG TY TNHH VSIP HẢI PHÒNG";
            var compAddrExcel = !string.IsNullOrWhiteSpace(template.CompanyAddress) && !IsGarbledEncoding(template.CompanyAddress) ? template.CompanyAddress : "Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng";
            var compPhoneExcel = !string.IsNullOrWhiteSpace(template.ContactPhone) && !IsGarbledEncoding(template.ContactPhone) ? template.ContactPhone : "(0225) 3959 888";
            var headerTitleExcel = !string.IsNullOrWhiteSpace(template.HeaderTitle) && !IsGarbledEncoding(template.HeaderTitle) ? template.HeaderTitle : "PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG";
            var subHeaderTitleExcel = !string.IsNullOrWhiteSpace(template.SubHeaderTitle) && !IsGarbledEncoding(template.SubHeaderTitle) ? template.SubHeaderTitle : "DÀNH CHO KHÁCH HÀNG SỬ DỤNG ĐIỆN";
            var footerNotesExcel = !string.IsNullOrWhiteSpace(template.FooterNotes) && !IsGarbledEncoding(template.FooterNotes)
                ? template.FooterNotes
                : "Ghi chú: Phiếu báo chỉ số điện năng này là cơ sở để phát hành hóa đơn tiền điện hàng tháng. Mọi thắc mắc xin vui lòng liên hệ Phòng Cơ Điện trong vòng 24 giờ kể từ khi nhận được phiếu báo.";

            var replacements = new Dictionary<string, object>
            {
                { "{{CompanyName}}", compNameExcel },
                { "{{CompanyAddress}}", compAddrExcel },
                { "{{ContactPhone}}", compPhoneExcel },
                { "{{HeaderTitle}}", headerTitleExcel },
                { "{{SubHeaderTitle}}", subHeaderTitleExcel },
                { "{{CustomerName}}", customerName },
                { "{{CustomerCode}}", customerCode },
                { "{{Address}}", address },
                { "{{PointName}}", pointName },
                { "{{PointCode}}", pointCode },
                { "{{MeterSerial}}", serialNumber },
                { "{{Period}}", periodStr },
                { "{{FromDate}}", fromDateStr },
                { "{{ToDate}}", toDateStr },
                { "{{ReadingTime}}", readingTimeStr },
                { "{{ReadingDate}}", readingDateStr },
                { "{{ForMonth}}", forMonthStr },
                { "{{BillingTimestamp}}", billingTimeStr },
                { "{{Multiplier}}", mult },
                { "{{PreviousBt}}", statement.PreviousBt },
                { "{{Bt}}", statement.Bt },
                { "{{DiffBt}}", diffBt },
                { "{{ConsumtionBt}}", statement.ConsumtionBt },
                { "{{PreviousCd}}", statement.PreviousCd },
                { "{{Cd}}", statement.Cd },
                { "{{DiffCd}}", diffCd },
                { "{{ConsumtionCd}}", statement.ConsumtionCd },
                { "{{PreviousTd}}", statement.PreviousTd },
                { "{{Td}}", statement.Td },
                { "{{DiffTd}}", diffTd },
                { "{{ConsumtionTd}}", statement.ConsumtionTd },
                { "{{PreviousSg}}", statement.PreviousSg },
                { "{{Sg}}", statement.Sg },
                { "{{DiffSg}}", diffSg },
                { "{{ConsumtionSg}}", statement.ConsumtionSg },
                { "{{PreviousVc}}", statement.PreviousVc },
                { "{{Vc}}", statement.Vc },
                { "{{DiffVc}}", diffVc },
                { "{{ConsumtionVc}}", statement.ConsumtionVc },
                { "{{Pmax}}", pmaxStr },
                { "{{PmaxTime}}", pmaxTimeStr },
                { "{{CosPhi}}", cosPhiStr },
                { "{{FooterNotes}}", footerNotesExcel }
            };

            // Scan all cells and perform substitution
            var start = ws.Dimension?.Start;
            var end = ws.Dimension?.End;
            if (start != null && end != null)
            {
                for (int row = start.Row; row <= end.Row; row++)
                {
                    for (int col = start.Column; col <= end.Column; col++)
                    {
                        var cell = ws.Cells[row, col];
                        if (cell.Value == null) continue;

                        var text = cell.Value.ToString() ?? string.Empty;
                        if (!text.Contains("{{")) continue;

                        // Check if the entire cell is an exact token
                        var trimmedText = text.Trim();
                        if (replacements.TryGetValue(trimmedText, out var exactValue))
                        {
                            if (exactValue is decimal decVal)
                            {
                                cell.Value = (double)decVal;
                                cell.Style.Numberformat.Format = "#,##0.00";
                            }
                            else if (exactValue is double dVal)
                            {
                                cell.Value = dVal;
                                cell.Style.Numberformat.Format = "#,##0.00";
                            }
                            else if (exactValue is int iVal)
                            {
                                cell.Value = iVal;
                                cell.Style.Numberformat.Format = "#,##0";
                            }
                            else
                            {
                                cell.Value = exactValue.ToString();
                            }
                            continue;
                        }

                        // Otherwise string replace
                        foreach (var kvp in replacements)
                        {
                            if (text.Contains(kvp.Key))
                            {
                                text = text.Replace(kvp.Key, kvp.Value is decimal dec ? dec.ToString("N2") : (kvp.Value is double d ? d.ToString("N2") : kvp.Value.ToString()));
                            }
                        }
                        cell.Value = text;
                    }
                }
            }

            return Task.FromResult(package.GetAsByteArray());
        }
    }

    public async Task<byte[]> ConvertExcelToPdfViaLibreOfficeAsync(byte[] excelBytes, CancellationToken cancellationToken = default)
    {
        var sofficePath = FindLibreOfficeExecutable();
        if (string.IsNullOrWhiteSpace(sofficePath) || !File.Exists(sofficePath))
        {
            throw new FileNotFoundException("Không tìm thấy trình chuyển đổi LibreOffice (soffice.exe). Vui lòng cấu hình 'LibreOfficePath' trong appsettings.json hoặc cài đặt LibreOffice trên máy chủ.");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "MDMS_ExcelPdf_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var excelFile = Path.Combine(tempDir, "statement.xlsx");
        var expectedPdfFile = Path.Combine(tempDir, "statement.pdf");

        try
        {
            await File.WriteAllBytesAsync(excelFile, excelBytes, cancellationToken);

            var psi = new ProcessStartInfo
            {
                FileName = sofficePath,
                Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{excelFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            var processWaitTask = process.WaitForExitAsync(cancellationToken);

            var completedTask = await Task.WhenAny(processWaitTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                try { process.Kill(true); } catch { }
                throw new TimeoutException("Quá trình chuyển đổi Excel sang PDF qua LibreOffice vượt quá thời gian chờ (30 giây).");
            }

            if (!File.Exists(expectedPdfFile))
            {
                var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new InvalidOperationException($"Không tìm thấy file PDF sau khi chuyển đổi. Mã lỗi: {process.ExitCode}. Output: {stdout}. Error: {stderr}");
            }

            return await File.ReadAllBytesAsync(expectedPdfFile, cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể dọn dẹp thư mục tạm {TempDir}: {Message}", tempDir, ex.Message);
            }
        }
    }

    private string? FindLibreOfficeExecutable()
    {
        var configured = _configuration["LibreOfficePath"];
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var commonPaths = new[]
        {
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
            "/usr/bin/soffice",
            "/usr/local/bin/soffice"
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    public class StatementLayoutConfig
    {
        public List<string> Columns { get; set; } = new() { "BT", "CD", "TD", "SG", "VC", "COSPHI", "REMARKS", "PMAX", "MULTIPLIER" };
        public string TaxCode { get; set; } = string.Empty;
        public bool ShowTaxCode { get; set; } = false;
        public string DocumentCode { get; set; } = "M&E-1/01 PR04";
        public string CopyLabel { get; set; } = "(M&E Copy – Liên cho Phòng Cơ Điện)";
        public string PartyAName { get; set; } = "Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)";
        public string PartyBName { get; set; } = "Party B (Tenant)\nBên B (Khách hàng)";
        public string HeaderLeftMode { get; set; } = "LOGO"; // LOGO | NONE
        public string HeaderCenterMode { get; set; } = "LOGO"; // LOGO | NONE
        public string HeaderRightMode { get; set; } = "DECOR"; // DECOR | LOGO | NONE
        public string HeaderLeftText { get; set; } = string.Empty;
        public string HeaderCenterText { get; set; } = string.Empty;
        public string HeaderRightText { get; set; } = string.Empty;
        public string FooterLeftMode { get; set; } = "LOGO"; // LOGO | NONE
        public string FooterCenterMode { get; set; } = "LOGO"; // LOGO | NONE
        public string FooterRightMode { get; set; } = "LOGO"; // LOGO | NONE
        public string FooterLeftText { get; set; } = string.Empty;
        public string FooterCenterText { get; set; } = string.Empty;
        public string FooterRightText { get; set; } = string.Empty;
    }

    public class StatementLogosConfig
    {
        public string? HeaderLeftLogo { get; set; }
        public string? HeaderCenterLogo { get; set; }
        public string? HeaderRightLogo { get; set; }
        public string? FooterLeftLogo { get; set; }
        public string? FooterCenterLogo { get; set; }
        public string? FooterRightLogo { get; set; }
    }

    public static StatementLayoutConfig ParseLayoutConfig(StatementPdfTemplateEntity? template)
    {
        var config = new StatementLayoutConfig();
        if (template == null || string.IsNullOrWhiteSpace(template.DisplayColumnsJson))
        {
            return config;
        }

        var raw = template.DisplayColumnsJson.Trim();
        try
        {
            if (raw.StartsWith("["))
            {
                var cols = JsonSerializer.Deserialize<List<string>>(raw);
                if (cols != null && cols.Count > 0)
                {
                    var set = new HashSet<string>(cols, StringComparer.OrdinalIgnoreCase);
                    // Mặc định bổ sung REMARKS và MULTIPLIER cho mảng cũ để đồng nhất 100% với mẫu chuẩn
                    if (!set.Contains("REMARKS")) cols.Add("REMARKS");
                    if (!set.Contains("MULTIPLIER")) cols.Add("MULTIPLIER");
                    config.Columns = cols;
                }
            }
            else if (raw.StartsWith("{"))
            {
                var parsed = JsonSerializer.Deserialize<StatementLayoutConfig>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (parsed != null)
                {
                    if (parsed.Columns == null || parsed.Columns.Count == 0)
                    {
                        parsed.Columns = new List<string> { "BT", "CD", "TD", "SG", "VC", "COSPHI", "REMARKS", "PMAX", "MULTIPLIER" };
                    }
                    return parsed;
                }
            }
        }
        catch
        {
            // Giữ cấu hình mặc định nếu JSON không hợp lệ
        }

        return config;
    }

    public static StatementLogosConfig ParseLogosConfig(StatementPdfTemplateEntity? template)
    {
        var logos = new StatementLogosConfig
        {
            HeaderLeftLogo = template?.LogoBase64
        };

        if (template != null && !string.IsNullOrWhiteSpace(template.ExcelTemplateBase64))
        {
            var raw = template.ExcelTemplateBase64.Trim();
            if (raw.StartsWith("{"))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<StatementLogosConfig>(raw, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (parsed != null)
                    {
                        if (string.IsNullOrWhiteSpace(parsed.HeaderLeftLogo))
                        {
                            parsed.HeaderLeftLogo = template.LogoBase64;
                        }
                        return parsed;
                    }
                }
                catch
                {
                    // Bỏ qua nếu không phải JSON
                }
            }
        }

        return logos;
    }

    private static byte[] DecodeBase64Image(string? base64Str, string? fallbackBase64 = null)
    {
        var target = !string.IsNullOrWhiteSpace(base64Str) ? base64Str : fallbackBase64;
        if (string.IsNullOrWhiteSpace(target)) return Array.Empty<byte>();
        try
        {
            if (target.Contains(","))
            {
                target = target.Substring(target.IndexOf(",") + 1);
            }
            return Convert.FromBase64String(target.Trim());
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(fallbackBase64) && target != fallbackBase64)
            {
                try { return Convert.FromBase64String(fallbackBase64); } catch { }
            }
            return Array.Empty<byte>();
        }
    }

    private static IDocument BuildQuestPdfDocument(PeriodicEnergyStatementEntity statement, StatementPdfTemplateEntity template)
    {
        var isLandscape = string.Equals(template.PaperSize, "A5_LANDSCAPE", StringComparison.OrdinalIgnoreCase);
        var pageSize = isLandscape ? PageSizes.A5.Landscape() : PageSizes.A4;

        var layout = ParseLayoutConfig(template);
        var logos = ParseLogosConfig(template);
        var enabledCols = new HashSet<string>(layout.Columns ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

        byte[] headerLeftLogoBytes = string.Equals(layout.HeaderLeftMode, "NONE", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<byte>()
            : DecodeBase64Image(logos.HeaderLeftLogo, DefaultLogoBase64);

        byte[] headerCenterLogoBytes = string.Equals(layout.HeaderCenterMode, "NONE", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<byte>()
            : DecodeBase64Image(logos.HeaderCenterLogo);

        byte[] headerRightBytes = Array.Empty<byte>();
        bool isRightLogo = string.Equals(layout.HeaderRightMode, "LOGO", StringComparison.OrdinalIgnoreCase);
        bool showRightDecor = !isRightLogo && !string.Equals(layout.HeaderRightMode, "NONE", StringComparison.OrdinalIgnoreCase);

        if (isRightLogo)
        {
            headerRightBytes = DecodeBase64Image(logos.HeaderRightLogo);
        }
        else if (showRightDecor)
        {
            try { headerRightBytes = GetTopRightDecorBytes(); } catch { headerRightBytes = Array.Empty<byte>(); }
        }

        byte[] footerLeftLogoBytes = string.Equals(layout.FooterLeftMode, "NONE", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<byte>()
            : DecodeBase64Image(logos.FooterLeftLogo);
        byte[] footerCenterLogoBytes = string.Equals(layout.FooterCenterMode, "NONE", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<byte>()
            : DecodeBase64Image(logos.FooterCenterLogo);
        byte[] footerRightLogoBytes = string.Equals(layout.FooterRightMode, "NONE", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<byte>()
            : DecodeBase64Image(logos.FooterRightLogo);

        var customerName = statement.MeasurementPoint?.Customer?.FullName ?? "---";
        var pointCode = statement.MeasurementPoint?.Code ?? "---";
        var serialNumber = !string.IsNullOrWhiteSpace(statement.SerialNumber) ? statement.SerialNumber : "---";
        var mult = statement.Multiplier > 0 ? statement.Multiplier : (statement.MeasurementPoint?.MultiplierFactor ?? 1);

        var fromDateStr = statement.PreviousDay.HasValue && statement.PreviousMonth.HasValue && statement.PreviousYear.HasValue && statement.PreviousDay.Value > 0
            ? $"{statement.PreviousDay.Value:00}/{statement.PreviousMonth.Value:00}/{statement.PreviousYear.Value}"
            : ".........";
        var toDateStr = $"{statement.Day:00}/{statement.Month:00}/{statement.Year}";

        var billingLocal = statement.BillingTimestamp.HasValue
            ? statement.BillingTimestamp.Value.ToOffset(TimeSpan.FromHours(7))
            : (DateTimeOffset?)null;
        var readingTimeStr = billingLocal.HasValue ? billingLocal.Value.ToString("HH:mm:ss") : "00:00:00";
        var readingDateStr = billingLocal.HasValue ? billingLocal.Value.ToString("dd/MM/yyyy") : toDateStr;
        var forMonthStr = $"{statement.Month:00}/{statement.Year}";

        var pmaxTimeLocalStr = statement.PmaxTime.HasValue
            ? statement.PmaxTime.Value.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm:ss")
            : "---";

        decimal? effectiveCosPhi = statement.CosPhi;
        if (!effectiveCosPhi.HasValue && statement.ConsumtionSg > 0m && statement.ConsumtionVc >= 0m)
        {
            var pD = (double)statement.ConsumtionSg;
            var qD = (double)statement.ConsumtionVc;
            var sApparent = Math.Sqrt((pD * pD) + (qD * qD));
            if (sApparent > 0)
            {
                effectiveCosPhi = Math.Round((decimal)(pD / sApparent), 2, MidpointRounding.AwayFromZero);
            }
        }
        var cosPhiStr = effectiveCosPhi.HasValue && effectiveCosPhi.Value > 0m ? effectiveCosPhi.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) : "0.00";

        static string FormatVal(decimal v)
        {
            if (Math.Abs(v % 1m) < 0.0001m)
                return v.ToString("#,##0", System.Globalization.CultureInfo.InvariantCulture);
            return v.ToString("#,##0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Xây dựng danh sách cột động theo cấu hình Bật/Tắt của người dùng (tỉ lệ chuẩn khớp 100% với TemplateNotice trên Web)
        var allMetricCols = new List<(string Code, string Header, float Weight, string PrevVal, string CurrVal, string MultVal, string ConsVal)>
        {
            ("BT", "Normal hours\nkWh meter 1\n(Giờ bình thường)", 1.30f, FormatVal(statement.PreviousBt), FormatVal(statement.Bt), mult.ToString(), FormatVal(statement.ConsumtionBt)),
            ("CD", "Peak hours\nkWh meter 2\n(Giờ cao điểm)", 1.30f, FormatVal(statement.PreviousCd), FormatVal(statement.Cd), mult.ToString(), FormatVal(statement.ConsumtionCd)),
            ("TD", "Off-peak hours\nkWh meter 3\n(Giờ thấp điểm)", 1.30f, FormatVal(statement.PreviousTd), FormatVal(statement.Td), mult.ToString(), FormatVal(statement.ConsumtionTd)),
            ("SG", "Total active power\n(Tổng công suất tác dụng)\nAP", 1.50f, FormatVal(statement.PreviousSg), FormatVal(statement.Sg), mult.ToString(), FormatVal(statement.ConsumtionSg)),
            ("VC", "Total reactive power\n(Tổng công suất phản kháng)\nAQ", 1.50f, FormatVal(statement.PreviousVc), FormatVal(statement.Vc), mult.ToString(), FormatVal(statement.ConsumtionVc)),
            ("COSPHI", "Cos φ TB\n(≥0.9)", 0.70f, "", "", "", cosPhiStr == "0.00" || cosPhiStr == "0" || cosPhiStr == "-" ? "" : cosPhiStr),
            ("REMARKS", "Remarks\n(Ghi chú)", 0.70f, "", "", "", "")
        };

        var activeMetricCols = allMetricCols.Where(c => enabledCols.Contains(c.Code)).ToList();
        if (activeMetricCols.Count == 0)
        {
            activeMetricCols = allMetricCols.Where(c => c.Code == "BT" || c.Code == "SG").ToList();
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.MarginHorizontal(18);
                page.MarginTop(12);
                page.MarginBottom(10);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(7.5f));

                if (showRightDecor && headerRightBytes.Length > 0)
                {
                    page.Background().AlignTop().AlignRight().Width(140).Image(headerRightBytes).FitArea();
                }

                page.Content().Column(col =>
                {
                    // 1. Header: Left Logo | Center (CompanyName, Address, Phone, TaxCode) | Right (Custom Logo if any)
                    col.Item().Row(headerRow =>
                    {
                        if (headerLeftLogoBytes.Length > 0)
                        {
                            headerRow.ConstantColumn(95).AlignLeft().AlignMiddle().MaxHeight(38).Image(headerLeftLogoBytes).FitArea();
                        }
                        else
                        {
                            headerRow.ConstantColumn(95);
                        }

                        headerRow.RelativeColumn().PaddingHorizontal(6).Column(center =>
                        {
                            if (headerCenterLogoBytes.Length > 0)
                            {
                                center.Item().PaddingBottom(2).AlignCenter().MaxHeight(24).Image(headerCenterLogoBytes).FitArea();
                            }

                            var compName = !string.IsNullOrWhiteSpace(template.CompanyName) && !IsGarbledEncoding(template.CompanyName) ? template.CompanyName : "VSIP HAI PHONG CO., LTD";
                            var compAddr = !string.IsNullOrWhiteSpace(template.CompanyAddress) && !IsGarbledEncoding(template.CompanyAddress) ? template.CompanyAddress : "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam";
                            var compPhone = !string.IsNullOrWhiteSpace(template.ContactPhone) && !IsGarbledEncoding(template.ContactPhone) ? template.ContactPhone : "Tel: 84-225-3959868   Fax: 84-225-3959886";

                            center.Item().Text(compName).FontSize(11.5f).AlignCenter().Bold();
                            if (!string.IsNullOrWhiteSpace(compAddr))
                            {
                                var addrNormalized = compAddr;
                                if (!addrNormalized.Contains('\n') && (addrNormalized.Contains("Hoa Binh Ward") || addrNormalized.Contains("Xã An Lư")))
                                {
                                    addrNormalized = Regex.Replace(addrNormalized, @"(,\s*)(Hoa Binh Ward|Xã An Lư)", "\n$2", RegexOptions.IgnoreCase);
                                }
                                var addrLines = addrNormalized.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var line in addrLines)
                                {
                                    center.Item().Text(line.Trim()).FontSize(8f).AlignCenter();
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(compPhone))
                            {
                                center.Item().Text(compPhone).FontSize(8f).AlignCenter();
                            }
                            if (layout.ShowTaxCode && !string.IsNullOrWhiteSpace(layout.TaxCode))
                            {
                                center.Item().Text($"Tax Code (MST): {layout.TaxCode}").FontSize(8f).Bold().AlignCenter();
                            }
                        });

                        if (isRightLogo && headerRightBytes.Length > 0)
                        {
                            headerRow.ConstantColumn(95).AlignRight().AlignTop().MaxHeight(38).Image(headerRightBytes).FitArea();
                        }
                        else
                        {
                            headerRow.ConstantColumn(95);
                        }
                    });

                    // 2. Title Section & Document Code / Serial Number (Đồng bộ tuyệt đối 100% với TemplateNotice trên Web)
                    col.Item().PaddingTop(2).PaddingBottom(2).Row(titleRow =>
                    {
                        // Cột trống bên trái đối xứng với cột Serial bên phải để tiêu đề nằm chính giữa 100%
                        titleRow.ConstantColumn(140);

                        titleRow.RelativeColumn().Column(titleCol =>
                        {
                            var mainTitle = !string.IsNullOrWhiteSpace(template.HeaderTitle) && !IsGarbledEncoding(template.HeaderTitle) ? template.HeaderTitle : "MONTHLY POWER METER READING";
                            var subTitle = !string.IsNullOrWhiteSpace(template.SubHeaderTitle) && !IsGarbledEncoding(template.SubHeaderTitle) ? template.SubHeaderTitle : "PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG";
                            var copyLbl = !string.IsNullOrWhiteSpace(layout.CopyLabel) ? layout.CopyLabel : "(M&E Copy – Liên cho Phòng Cơ Điện)";

                            titleCol.Item().Text(mainTitle).FontSize(11.5f).Bold().AlignCenter();
                            if (!string.IsNullOrWhiteSpace(subTitle))
                            {
                                titleCol.Item().Text(subTitle).FontSize(10.5f).Bold().AlignCenter();
                            }
                            if (!string.IsNullOrWhiteSpace(copyLbl))
                            {
                                titleCol.Item().Text(copyLbl).FontSize(8.5f).AlignCenter();
                            }
                        });

                        // Cột bên phải: Mã biểu mẫu và Serial Number căn đều trái với nhau (text-start), format chuẩn theo TemplateNotice
                        var trackingSerial = !string.IsNullOrWhiteSpace(serialNumber)
                            ? string.Join("\u00A0", serialNumber.Trim().ToCharArray())
                            : "---";

                        titleRow.ConstantColumn(140).AlignLeft().AlignBottom().PaddingBottom(2).Column(serialCol =>
                        {
                            var docCode = !string.IsNullOrWhiteSpace(layout.DocumentCode) ? layout.DocumentCode : "M&E-1/01 PR04";
                            serialCol.Item().Text(docCode).FontSize(8.5f).FontColor("#000000");
                            serialCol.Item().Text(text =>
                            {
                                text.Span("Serial No (số):\u00A0").FontSize(8.5f).FontColor("#000000");
                                text.Span(trackingSerial).FontSize(11.5f).Bold().FontColor("#EF4444");
                            });
                        });
                    });

                    // 3. Customer & Measurement Point Info (với gạch chân chuẩn như mẫu hiển thị)
                    col.Item().PaddingBottom(3).Column(metaCol =>
                    {
                        metaCol.Item().PaddingBottom(2).Row(r =>
                        {
                            r.ConstantColumn(115).Text("Tenant Name (Tên khách hàng):").FontSize(8.5f);
                            r.RelativeColumn().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(1).PaddingLeft(4).Text(customerName).FontSize(8.5f).Bold();
                        });

                        metaCol.Item().PaddingBottom(2).Row(r =>
                        {
                            r.ConstantColumn(115).Text("Meter ID (Mã điểm đo):").FontSize(8.5f);
                            r.RelativeColumn().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(1).PaddingLeft(4).Text(pointCode).FontSize(8.5f).Bold();
                        });

                        metaCol.Item().Row(r =>
                        {
                            r.RelativeColumn().Row(sub =>
                            {
                                sub.AutoItem().Text("Reading Time (Giờ ghi): ").FontSize(8.5f);
                                sub.RelativeColumn().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(1).PaddingLeft(4).Text(readingTimeStr).FontSize(8.5f).Bold();
                            });
                            r.ConstantColumn(14);
                            r.RelativeColumn().Row(sub =>
                            {
                                sub.AutoItem().Text("Reading Date (Ngày ghi): ").FontSize(8.5f);
                                sub.RelativeColumn().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(1).PaddingLeft(4).Text(readingDateStr).FontSize(8.5f).Bold();
                            });
                            r.ConstantColumn(14);
                            r.RelativeColumn().Row(sub =>
                            {
                                sub.AutoItem().Text("For month (Tháng): ").FontSize(8.5f);
                                sub.RelativeColumn().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(1).PaddingLeft(4).Text(forMonthStr).FontSize(8.5f).Bold();
                            });
                        });
                    });

                    // 4. Dynamic Horizontal Grid Matrix Table (Tự động co giãn các cột theo cấu hình Bật/Tắt)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.70f); // Description
                            foreach (var mc in activeMetricCols)
                            {
                                columns.RelativeColumn(mc.Weight);
                            }
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text("Description\n(Mô tả)").Bold().FontSize(7.2f);
                            foreach (var mc in activeMetricCols)
                            {
                                header.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(mc.Header).Bold().FontSize(7.2f);
                            }
                        });

                        // Row 1: Previous meter reading (Chỉ số cũ)
                        table.Cell().Element(CellStyle).AlignLeft().AlignMiddle().PaddingLeft(3).Text($"Previous meter reading\n(Chỉ số cũ)\nReading date: {fromDateStr}\n(Ngày ghi)").FontSize(7.2f);
                        foreach (var mc in activeMetricCols)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(mc.PrevVal).FontSize(7.5f);
                        }

                        // Row 2: Current meter reading (Chỉ số mới)
                        table.Cell().Element(CellStyle).AlignLeft().AlignMiddle().PaddingLeft(3).Text("Current meter reading\n(Chỉ số mới)").FontSize(7.2f);
                        foreach (var mc in activeMetricCols)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(mc.CurrVal).FontSize(7.5f);
                        }

                        // Row 3: Multiplier (Hệ số) - Có thể bật/tắt
                        if (enabledCols.Contains("MULTIPLIER"))
                        {
                            table.Cell().Element(CellStyle).AlignLeft().AlignMiddle().PaddingLeft(3).Text("Multiplier\n(hệ số)").FontSize(7.2f);
                            foreach (var mc in activeMetricCols)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(mc.MultVal).FontSize(7.5f);
                            }
                        }

                        // Row 4: Actual consumption (Số tiêu thụ thực tế)
                        table.Cell().Element(CellStyle).AlignLeft().AlignMiddle().PaddingLeft(3).Text("Actual consumption\n(Số tiêu thụ thực tế)").FontSize(7.2f);
                        foreach (var mc in activeMetricCols)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().AlignMiddle().Text(mc.ConsVal).FontSize(7.5f);
                        }
                    });

                    // 5. Pmax Line Detail (LocalTime GMT+7) & Confirmation Notice
                    col.Item().PaddingTop(2.5f).Column(footerCol =>
                    {
                        if (enabledCols.Contains("PMAX"))
                        {
                            var pmaxValStr = statement.Pmax > 0 ? statement.Pmax.ToString("N2", System.Globalization.CultureInfo.InvariantCulture) : "0.00";
                            footerCol.Item().PaddingLeft(2).Text($"* Công suất cực đại Pmax: {pmaxValStr} kW, ghi nhận hồi: {pmaxTimeLocalStr}").Bold().FontSize(8f);
                        }

                        footerCol.Item().PaddingTop(1.5f).PaddingLeft(2).Text("The above meter reading has been jointly read by the following Parties:\n(Các chỉ số trên đã được các Bên dưới đây xác nhận)").FontSize(7.5f);
                    });

                    // 6. Signatures Boxes (Party A & Party B)
                    var partyALines = (!string.IsNullOrWhiteSpace(layout.PartyAName)
                        ? layout.PartyAName
                        : "Party A (VSIP Hai Phong Co., Ltd)\nBên A (Công ty TNHH VSIP Hải Phòng)").Split('\n');
                    var partyBLines = (!string.IsNullOrWhiteSpace(layout.PartyBName)
                        ? layout.PartyBName
                        : "Party B (Tenant)\nBên B (Khách hàng)").Split('\n');

                    col.Item().PaddingTop(3.5f).Row(sigRow =>
                    {
                        if (template.RequireDualSignatures)
                        {
                            sigRow.RelativeColumn(0.47f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                for (int i = 0; i < partyALines.Length; i++)
                                {
                                    var t = partyA.Item().Text(partyALines[i].Trim()).FontSize(8f);
                                    if (i == 0) t.Bold();
                                }
                                partyA.Item().PaddingTop(12).Text("Name and Signature\n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });

                            sigRow.ConstantColumn(36);

                            sigRow.RelativeColumn(0.47f).Element(PartyBoxStyle).Column(partyB =>
                            {
                                for (int i = 0; i < partyBLines.Length; i++)
                                {
                                    var t = partyB.Item().Text(partyBLines[i].Trim()).FontSize(8f);
                                    if (i == 0) t.Bold();
                                }
                                partyB.Item().PaddingTop(12).Text("Name and Signature\n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                        else
                        {
                            sigRow.RelativeColumn(0.53f);
                            sigRow.RelativeColumn(0.47f).Element(PartyBoxStyle).Column(partyA =>
                            {
                                for (int i = 0; i < partyALines.Length; i++)
                                {
                                    var t = partyA.Item().Text(partyALines[i].Trim()).FontSize(8f);
                                    if (i == 0) t.Bold();
                                }
                                partyA.Item().PaddingTop(12).Text("Name and Signature\n(Ký và ghi rõ họ tên)").FontSize(7.5f);
                            });
                        }
                    });

                    // 7. Custom Footer (Left / Center / Right Logos & Text)
                    bool showFooterLeft = !string.Equals(layout.FooterLeftMode, "NONE", StringComparison.OrdinalIgnoreCase);
                    bool showFooterCenter = !string.Equals(layout.FooterCenterMode, "NONE", StringComparison.OrdinalIgnoreCase);
                    bool showFooterRight = !string.Equals(layout.FooterRightMode, "NONE", StringComparison.OrdinalIgnoreCase);

                    bool hasFooter = (showFooterLeft && (footerLeftLogoBytes.Length > 0 || !string.IsNullOrWhiteSpace(layout.FooterLeftText)))
                        || (showFooterCenter && (footerCenterLogoBytes.Length > 0 || !string.IsNullOrWhiteSpace(layout.FooterCenterText)))
                        || (showFooterRight && (footerRightLogoBytes.Length > 0 || !string.IsNullOrWhiteSpace(layout.FooterRightText)));

                    if (hasFooter)
                    {
                        col.Item().PaddingTop(4).Row(fRow =>
                        {
                            fRow.RelativeColumn().AlignLeft().Column(fLeft =>
                            {
                                if (showFooterLeft && footerLeftLogoBytes.Length > 0)
                                    fLeft.Item().MaxHeight(20).Image(footerLeftLogoBytes).FitArea();
                                if (showFooterLeft && !string.IsNullOrWhiteSpace(layout.FooterLeftText))
                                    fLeft.Item().Text(layout.FooterLeftText).FontSize(6.5f).FontColor("#475569");
                            });

                            fRow.RelativeColumn().AlignCenter().Column(fCenter =>
                            {
                                if (showFooterCenter && footerCenterLogoBytes.Length > 0)
                                    fCenter.Item().AlignCenter().MaxHeight(20).Image(footerCenterLogoBytes).FitArea();
                                if (showFooterCenter && !string.IsNullOrWhiteSpace(layout.FooterCenterText))
                                    fCenter.Item().AlignCenter().Text(layout.FooterCenterText).FontSize(6.5f).FontColor("#475569");
                            });

                            fRow.RelativeColumn().AlignRight().Column(fRight =>
                            {
                                if (showFooterRight && footerRightLogoBytes.Length > 0)
                                    fRight.Item().AlignRight().MaxHeight(20).Image(footerRightLogoBytes).FitArea();
                                if (showFooterRight && !string.IsNullOrWhiteSpace(layout.FooterRightText))
                                    fRight.Item().AlignRight().Text(layout.FooterRightText).FontSize(6.5f).FontColor("#475569");
                            });
                        });
                    }
                });
            });
        });
    }

    private static HashSet<string> ParseColumns(string? json)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BT", "CD", "TD", "SG", "VC", "PMAX", "COSPHI" };
        if (string.IsNullOrWhiteSpace(json)) return set;

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json);
            if (parsed != null && parsed.Count > 0)
            {
                return new HashSet<string>(parsed, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Fallback to full set
        }
        return set;
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Border(0.75f).BorderColor(Colors.Black).PaddingVertical(2.5f).PaddingHorizontal(2);
    }

    private static IContainer PartyBoxStyle(IContainer container)
    {
        return container.Border(0.75f).BorderColor(Colors.Black).PaddingVertical(3).PaddingHorizontal(5);
    }

    private static StatementPdfTemplateEntity GetFallbackDefaultTemplate()
    {
        return new StatementPdfTemplateEntity
        {
            Code = "DEFAULT_A5_VSIP",
            Name = "Phiếu Báo Điện Năng Chuẩn VSIP (A5 Ngang)",
            HeaderTitle = "MONTHLY POWER METER READING",
            SubHeaderTitle = "PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG",
            CompanyName = "VSIP HAI PHONG CO., LTD",
            CompanyAddress = "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam",
            ContactPhone = "Tel: 84-225-3959868   Fax: 84-225-3959886",
            PaperSize = "A5_LANDSCAPE",
            EngineType = "QUEST_PDF",
            DisplayColumnsJson = "{\"columns\":[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"COSPHI\",\"REMARKS\",\"PMAX\",\"MULTIPLIER\"],\"documentCode\":\"M&E-1/01 PR04\",\"copyLabel\":\"(M&E Copy – Liên cho Phòng Cơ Điện)\",\"partyAName\":\"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)\",\"partyBName\":\"Party B (Tenant)\\nBên B (Khách hàng)\",\"headerLeftMode\":\"LOGO\",\"headerCenterMode\":\"LOGO\",\"headerRightMode\":\"DECOR\",\"footerLeftMode\":\"LOGO\",\"footerCenterMode\":\"LOGO\",\"footerRightMode\":\"LOGO\"}",
            FooterNotes = "Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.",
            RequireDualSignatures = true,
            IsDefault = true,
            IsActive = true
        };
    }

    private static PeriodicEnergyStatementEntity CreateMockStatement()
    {
        return new PeriodicEnergyStatementEntity
        {
            SerialNumber = "00000162",
            Multiplier = 1,
            MeasurementPoint = new MeasurementPointsEntity
            {
                Code = "KYOCERA 1",
                Address = "KCN VSIP Hải Phòng",
                MultiplierFactor = 1,
                Customer = new CustomerEntity
                {
                    FullName = "CÔNG TY TNHH CÔNG NGHỆ MÁY VĂN PHÒNG KYOCERA VIỆT NAM",
                    CustomerCode = "KYOCERA",
                    Address = "KCN VSIP Hải Phòng"
                }
            },
            Day = 31,
            Month = 10,
            Year = 2026,
            PreviousDay = 1,
            PreviousMonth = 10,
            PreviousYear = 2026,
            BillingTimestamp = new DateTimeOffset(2026, 10, 31, 0, 0, 0, TimeSpan.FromHours(7)),
            PreviousBt = 0,
            Bt = 67,
            ConsumtionBt = 67,
            PreviousCd = 0,
            Cd = 0,
            ConsumtionCd = 0,
            PreviousTd = 0,
            Td = 0,
            ConsumtionTd = 0,
            PreviousSg = 0,
            Sg = 67,
            ConsumtionSg = 67,
            PreviousVc = 0,
            Vc = 28,
            ConsumtionVc = 28,
            Pmax = 66,
            PmaxTime = new DateTimeOffset(2026, 11, 6, 3, 16, 0, TimeSpan.FromHours(7)),
            CosPhi = 0.92m
        };
    }
}
