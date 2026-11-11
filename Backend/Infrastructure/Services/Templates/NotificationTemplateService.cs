using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Templates;

public class NotificationTemplateService : INotificationTemplateService
{
    private static readonly Regex VariableRegex = new(@"\{\{\s*([\w\.\-]+)(?::([^}]+))?\s*\}\}", RegexOptions.Compiled);
    private readonly INotificationTemplateReadOnlyRepository _repository;
    private readonly IStatementPdfTemplateReadOnlyRepository? _statementPdfTemplateRepository;
    private readonly ISystemConfigReadOnlyRepository? _systemConfigRepository;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        INotificationTemplateReadOnlyRepository repository,
        ILogger<NotificationTemplateService> logger,
        IStatementPdfTemplateReadOnlyRepository? statementPdfTemplateRepository = null,
        ISystemConfigReadOnlyRepository? systemConfigRepository = null)
    {
        _repository = repository;
        _logger = logger;
        _statementPdfTemplateRepository = statementPdfTemplateRepository;
        _systemConfigRepository = systemConfigRepository;
    }

    public string Render(string template, IDictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        if (context == null || context.Count == 0) return template;

        // Build case-insensitive lookup
        var lookup = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in context)
        {
            lookup[kv.Key] = kv.Value;
        }

        return VariableRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value.Trim();
            var format = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

            if (lookup.TryGetValue(key, out var val) && val != null)
            {
                if (!string.IsNullOrEmpty(format) && val is IFormattable formattable)
                {
                    return formattable.ToString(format, CultureInfo.InvariantCulture);
                }
                return val.ToString() ?? string.Empty;
            }

            // If not found in context, return empty or match
            return string.Empty;
        });
    }

    public static bool IsGarbledEncoding(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return text.Contains('├') || text.Contains('ß') || text.Contains('─') || text.Contains('║') || text.Contains('¥') || text.Contains('┬') || text.Contains('┐') || text.Contains('└');
    }

    public async Task<(string Subject, string BodyHtml)> RenderNotificationAsync(string code, IDictionary<string, object?> context, CancellationToken cancellationToken = default)
    {
        if (_statementPdfTemplateRepository != null)
        {
            var activeStmtTpl = await _statementPdfTemplateRepository.GetDefaultAsync(cancellationToken);
            if (activeStmtTpl != null)
            {
                var layout = StatementPdfGenerator.ParseLayoutConfig(activeStmtTpl);
                var compName = !string.IsNullOrWhiteSpace(activeStmtTpl.CompanyName) && !IsGarbledEncoding(activeStmtTpl.CompanyName)
                    ? activeStmtTpl.CompanyName
                    : "CÔNG TY TNHH VSIP HẢI PHÒNG";
                var compAddr = !string.IsNullOrWhiteSpace(activeStmtTpl.CompanyAddress) && !IsGarbledEncoding(activeStmtTpl.CompanyAddress)
                    ? activeStmtTpl.CompanyAddress
                    : "Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng";
                var compPhone = !string.IsNullOrWhiteSpace(activeStmtTpl.ContactPhone) && !IsGarbledEncoding(activeStmtTpl.ContactPhone)
                    ? activeStmtTpl.ContactPhone
                    : "(0225) 3959 888";

                if (!context.ContainsKey("CompanyName") || string.IsNullOrWhiteSpace(context["CompanyName"]?.ToString()) || IsGarbledEncoding(context["CompanyName"]?.ToString()))
                {
                    context["CompanyName"] = compName;
                }
                if (!context.ContainsKey("CompanyAddress") || string.IsNullOrWhiteSpace(context["CompanyAddress"]?.ToString()) || IsGarbledEncoding(context["CompanyAddress"]?.ToString()))
                {
                    context["CompanyAddress"] = compAddr;
                }
                if (!context.ContainsKey("ContactPhone") || string.IsNullOrWhiteSpace(context["ContactPhone"]?.ToString()) || IsGarbledEncoding(context["ContactPhone"]?.ToString()))
                {
                    context["ContactPhone"] = compPhone;
                }
                if (!context.ContainsKey("TaxCode") || string.IsNullOrWhiteSpace(context["TaxCode"]?.ToString()))
                {
                    context["TaxCode"] = layout.TaxCode ?? string.Empty;
                }
            }
        }

        if (_systemConfigRepository != null)
        {
            try
            {
                var sysConfigs = await _systemConfigRepository.GetListSystemConfigAsync(cancellationToken);
                var careFullname = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCareFullname)?.Value;
                if (string.IsNullOrWhiteSpace(careFullname))
                    careFullname = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationFullname)?.Value;

                var careEmail = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCareEmail)?.Value;
                if (string.IsNullOrWhiteSpace(careEmail))
                    careEmail = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationEmail)?.Value;

                var carePhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCarePhone)?.Value;
                if (string.IsNullOrWhiteSpace(carePhone))
                    carePhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationPhone)?.Value;

                if (!string.IsNullOrWhiteSpace(careFullname) && (!context.ContainsKey("CareFullname") || string.IsNullOrWhiteSpace(context["CareFullname"]?.ToString())))
                    context["CareFullname"] = careFullname;

                if (!string.IsNullOrWhiteSpace(careEmail))
                {
                    if (!context.ContainsKey("CareEmail") || string.IsNullOrWhiteSpace(context["CareEmail"]?.ToString()))
                        context["CareEmail"] = careEmail;
                    if (!context.ContainsKey("SystemEmail") || string.IsNullOrWhiteSpace(context["SystemEmail"]?.ToString()))
                        context["SystemEmail"] = careEmail;
                }

                if (!string.IsNullOrWhiteSpace(carePhone))
                {
                    if (!context.ContainsKey("CarePhone") || string.IsNullOrWhiteSpace(context["CarePhone"]?.ToString()))
                        context["CarePhone"] = carePhone;
                    if (!context.ContainsKey("SupportPhone") || string.IsNullOrWhiteSpace(context["SupportPhone"]?.ToString()))
                        context["SupportPhone"] = carePhone;
                }

                // Nạp thông tin công ty / đơn vị cung cấp điện tập trung từ SystemConfig
                var companyName = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyName)?.Value;
                if (!string.IsNullOrWhiteSpace(companyName) && !IsGarbledEncoding(companyName))
                    context["CompanyName"] = companyName;

                var companyAddress = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyAddress)?.Value;
                if (!string.IsNullOrWhiteSpace(companyAddress) && !IsGarbledEncoding(companyAddress))
                    context["CompanyAddress"] = companyAddress.Replace("\r\n", ", ").Replace("\n", ", ");

                var companyPhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyPhone)?.Value;
                if (!string.IsNullOrWhiteSpace(companyPhone) && !IsGarbledEncoding(companyPhone))
                    context["ContactPhone"] = companyPhone;

                var companyTaxCode = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyTaxCode)?.Value;
                if (!string.IsNullOrWhiteSpace(companyTaxCode))
                    context["TaxCode"] = companyTaxCode;

                // Nạp đường dẫn công khai của hệ thống (Cloudflare Tunnel, Domain hoặc WAN IP)
                var publicBaseUrl = sysConfigs.Find(x => x.Key == Constants.SystemConfig.PublicBaseUrl)?.Value;
                if (!string.IsNullOrWhiteSpace(publicBaseUrl))
                {
                    var cleanUrl = publicBaseUrl.Trim().TrimEnd('/');
                    context["PublicBaseUrl"] = cleanUrl;
                    context["PortalUrl"] = cleanUrl;
                    if (!context.ContainsKey("LoginUrl") || string.IsNullOrWhiteSpace(context["LoginUrl"]?.ToString()) || context["LoginUrl"]?.ToString()?.Contains("127.0.0.1") == true)
                    {
                        context["LoginUrl"] = $"{cleanUrl}/login";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể nạp thông tin liên hệ trong RenderNotificationAsync: {Message}", ex.Message);
            }
        }

        var template = await _repository.GetDefaultByCodeAsync(code, cancellationToken);
        if (template != null)
        {
            if (IsGarbledEncoding(template.BodyHtmlTemplate) || IsGarbledEncoding(template.SubjectTemplate))
            {
                _logger.LogWarning("Phát hiện mẫu thông báo '{Code}' trong CSDL bị lỗi font mã hóa ký tự. Tự động chuyển về mẫu chuẩn mặc định.", code);
                return GetFallbackDefaultTemplate(code, context);
            }
            var cleanedHtml = CleanLegacyFields(template.BodyHtmlTemplate);
            var subject = Render(template.SubjectTemplate, context);
            var body = Render(cleanedHtml, context);
            return (subject, body);
        }

        // Fallback to built-in default templates
        return GetFallbackDefaultTemplate(code, context);
    }

    public async Task<PreviewNotificationTemplateResponse> PreviewAsync(PreviewNotificationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var sampleContext = GetSampleVariablesForCode(request.Code);
        if (_statementPdfTemplateRepository != null)
        {
            var activeStmtTpl = await _statementPdfTemplateRepository.GetDefaultAsync(cancellationToken);
            if (activeStmtTpl != null)
            {
                var layout = StatementPdfGenerator.ParseLayoutConfig(activeStmtTpl);
                if (!string.IsNullOrWhiteSpace(activeStmtTpl.CompanyName))
                    sampleContext["CompanyName"] = activeStmtTpl.CompanyName;
                if (!string.IsNullOrWhiteSpace(activeStmtTpl.CompanyAddress))
                    sampleContext["CompanyAddress"] = activeStmtTpl.CompanyAddress;
                if (!string.IsNullOrWhiteSpace(activeStmtTpl.ContactPhone))
                    sampleContext["ContactPhone"] = activeStmtTpl.ContactPhone;
                if (!string.IsNullOrWhiteSpace(layout.TaxCode))
                    sampleContext["TaxCode"] = layout.TaxCode;
            }
        }

        if (_systemConfigRepository != null)
        {
            try
            {
                var sysConfigs = await _systemConfigRepository.GetListSystemConfigAsync(cancellationToken);
                var careFullname = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCareFullname)?.Value;
                if (string.IsNullOrWhiteSpace(careFullname))
                    careFullname = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationFullname)?.Value;

                var careEmail = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCareEmail)?.Value;
                if (string.IsNullOrWhiteSpace(careEmail))
                    careEmail = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationEmail)?.Value;

                var carePhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CustomerCarePhone)?.Value;
                if (string.IsNullOrWhiteSpace(carePhone))
                    carePhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.SystemAdministrationPhone)?.Value;

                if (!string.IsNullOrWhiteSpace(careFullname)) sampleContext["CareFullname"] = careFullname;
                if (!string.IsNullOrWhiteSpace(careEmail)) { sampleContext["CareEmail"] = careEmail; sampleContext["SystemEmail"] = careEmail; }
                if (!string.IsNullOrWhiteSpace(carePhone)) { sampleContext["CarePhone"] = carePhone; sampleContext["SupportPhone"] = carePhone; }

                // Nạp thông tin công ty / đơn vị cung cấp điện tập trung từ SystemConfig
                var companyName = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyName)?.Value;
                if (!string.IsNullOrWhiteSpace(companyName))
                    sampleContext["CompanyName"] = companyName;

                var companyAddress = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyAddress)?.Value;
                if (!string.IsNullOrWhiteSpace(companyAddress))
                    sampleContext["CompanyAddress"] = companyAddress.Replace("\r\n", ", ").Replace("\n", ", ");

                var companyPhone = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyPhone)?.Value;
                if (!string.IsNullOrWhiteSpace(companyPhone))
                    sampleContext["ContactPhone"] = companyPhone;

                var companyTaxCode = sysConfigs.Find(x => x.Key == Constants.SystemConfig.CompanyTaxCode)?.Value;
                if (!string.IsNullOrWhiteSpace(companyTaxCode))
                    sampleContext["TaxCode"] = companyTaxCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể nạp thông tin liên hệ trong PreviewAsync: {Message}", ex.Message);
            }
        }

        if (request.CustomVariables != null)
        {
            foreach (var kv in request.CustomVariables)
            {
                sampleContext[kv.Key] = kv.Value;
            }
        }

        var cleanedBodyHtml = CleanLegacyFields(request.BodyHtmlTemplate);
        var renderedSubject = Render(request.SubjectTemplate, sampleContext);
        var renderedBody = Render(cleanedBodyHtml, sampleContext);

        return new PreviewNotificationTemplateResponse
        {
            Subject = renderedSubject,
            BodyHtml = renderedBody
        };
    }

    public static string CleanLegacyFields(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var cleaned = html;

        // 1. Loại bỏ các trường cũ (Mã khách hàng, Mã điểm đo, HSN) -> Chỉ hiển thị Tên điểm đo
        if (cleaned.Contains("Mã khách hàng", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Contains("CustomerCode", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Contains("Mã điểm đo", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Contains("Hệ số nhân", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Contains("HSN", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Contains("Số công tơ", StringComparison.OrdinalIgnoreCase))
        {
            const string cleanInfoTable = @"<table width='100%' cellpadding='10' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;'>
                                <tr>
                                    <td width='22%' style='color:#64748b;font-weight:500;'>Tên điểm đo:</td>
                                    <td style='font-weight:700;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                            </table>";

            cleaned = Regex.Replace(
                cleaned,
                @"<table[^>]*>[\s\S]*?(?:Mã khách hàng|CustomerCode|Mã điểm đo)[\s\S]*?</table>",
                cleanInfoTable,
                RegexOptions.IgnoreCase);
        }

        // 2. Hiệu chỉnh tệp đính kèm theo đúng yêu cầu
        cleaned = Regex.Replace(
            cleaned,
            @"(?:📎\s*(?:<strong>)?Tệp đính kèm:?(?:</strong>)?\s*)?Chi tiết phiếu báo chỉ số điện năng định dạng PDF[^<]*<strong>[^<]*</strong>[^<]*",
            "📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>",
            RegexOptions.IgnoreCase);

        // 3. Hiệu chỉnh Hotline & Email trên 2 hàng riêng biệt
        cleaned = Regex.Replace(
            cleaned,
            @"(📞\s*Hotline CSKH:[^<]*)\s*\|\s*(✉️\s*Email tiếp nhận:[^<]*)",
            "$1<br>$2",
            RegexOptions.IgnoreCase);

        // 4. Căn lề trên (vertical-align: top) cho cột Pmax và CosPhi
        cleaned = Regex.Replace(cleaned, @"style=['""]vertical-align:\s*middle;?['""]", "style='vertical-align:top;'", RegexOptions.IgnoreCase);

        // 5. Loại bỏ dòng "Đơn vị liên kết" trong mẫu email cấp tài khoản
        cleaned = Regex.Replace(cleaned, @"<tr>\s*<td[^>]*>[^<]*Đơn vị liên kết:?</td>[\s\S]*?</tr>", "", RegexOptions.IgnoreCase);

        // 6. Đổi nút "Đăng Nhập Vào Hệ Thống" thành "Đăng Nhập"
        cleaned = Regex.Replace(cleaned, @"Đăng\s+Nhập\s+Vào\s+Hệ\s+Thống", "Đăng Nhập", RegexOptions.IgnoreCase);

        // 7. Đồng bộ tiêu đề hệ thống dạng (MDMS)
        cleaned = Regex.Replace(cleaned, @"HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM - MDMS", "HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)", RegexOptions.IgnoreCase);

        // 8. Chuẩn hóa tiêu đề Header cho mẫu tài khoản & mật khẩu
        cleaned = Regex.Replace(cleaned, @"THÔNG TIN CẤP TÀI KHOẢN NGƯỜI DÙNG", "THÔNG TIN TÀI KHOẢN", RegexOptions.IgnoreCase);

        // 9. Bỏ đoạn "(từ ngày {{FromDate}} đến ngày {{ToDate}})" trong lời mở đầu để khớp 100% hình ảnh
        cleaned = Regex.Replace(cleaned, @"\s*\(từ ngày \{\{FromDate\}\} đến ngày \{\{ToDate\}\}\)", "", RegexOptions.IgnoreCase);

        return cleaned;
    }

    public static Dictionary<string, object?> GetSampleVariablesForCode(string? code)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            // Khách hàng & Điểm đo
            ["CustomerName"] = "CÔNG TY TNHH KYOCERA VIỆT NAM",
            ["PointCode"] = "101.402.001",
            ["PointName"] = "Điểm đo Trạm Biến Áp TBA 1",
            ["CustomerCode"] = "KH_KYOCERA_01",
            ["MeterSerial"] = "23098172",
            ["Multiplier"] = "120",
            ["CompanyName"] = "CÔNG TY TNHH VSIP HẢI PHÒNG",
            ["Address"] = "Lô đất số IN1-4, KCN VSIP Hải Phòng, Xã An Lư, Huyện Thủy Nguyên, TP. Hải Phòng",
            ["SerialNumber"] = "PB-202610-001",

            // Thời gian & Chu kỳ
            ["CycleName"] = "Kỳ 1 Tháng 10/2026",
            ["FromDate"] = "01/10/2026",
            ["ToDate"] = "31/10/2026",
            ["Month"] = "10",
            ["Year"] = "2026",
            ["Period"] = "Kỳ 1 Tháng 10/2026",

            // Chỉ số công tơ
            ["PreviousBt"] = "1,245,670",
            ["PreviousCd"] = "345,120",
            ["PreviousTd"] = "560,890",
            ["PreviousSg"] = "2,151,680",
            ["Bt"] = "1,295,430",
            ["Cd"] = "362,450",
            ["Td"] = "581,200",
            ["Sg"] = "2,239,080",

            // Sản lượng & Thông số vận hành
            ["OutputBt"] = "49,760",
            ["OutputCd"] = "17,330",
            ["OutputTd"] = "20,310",
            ["OutputSg"] = "87,400",
            ["OutputVc"] = "12,450",
            ["ConsumtionBt"] = "49,760",
            ["ConsumtionCd"] = "17,330",
            ["ConsumtionTd"] = "20,310",
            ["ConsumtionSg"] = "87,400",
            ["ConsumtionTotal"] = "87,400",
            ["ConsumtionVc"] = "12,450",
            ["Pmax"] = "1,450.85 kW",
            ["PmaxTime"] = "15/10/2026 14:30:00",
            ["CosPhi"] = "0.98",

            // Tài khoản & Xác thực
            ["FullName"] = "Nguyễn Văn An",
            ["Username"] = "an.nguyen",
            ["Password"] = "Vsip@2026#",
            ["Email"] = "an.nguyen@vsip.com.vn",
            ["LoginUrl"] = "https://amr.nobisoft.vn/login",
            ["ResetLink"] = "https://amr.nobisoft.vn/reset-password?token=sample_demo_token&email=admin@kyocera.com.vn",
            ["Token"] = "984210",
            ["CareFullname"] = "Phòng Quản Lý Vận Hành Cơ Điện VSIP",
            ["CareEmail"] = "noreply.mdms@vsip.com.vn",
            ["CarePhone"] = "0225 3959868",
            ["SystemEmail"] = "noreply.mdms@vsip.com.vn",
            ["SupportPhone"] = "0225 3959868",
            ["SystemName"] = "Hệ thống thu thập dữ liệu đo đếm - MDMS",
            ["CurrentDate"] = DateTime.Now.ToString("dd/MM/yyyy")
        };

        return dict;
    }

    private (string Subject, string BodyHtml) GetFallbackDefaultTemplate(string code, IDictionary<string, object?> context)
    {
        code = code.ToUpperInvariant();
        if (code == Constants.NotificationTemplateCode.StatementNotification)
        {
            var subjectTpl = "THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG";
            var bodyTpl = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên có thể thay đổi tên đơn vị tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN KHÁCH HÀNG & ĐIỂM ĐO ĐIỆN             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:24px 30px 10px 30px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p>
                            <p style='margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;'>
                                Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.
                            </p>
                            <table width='100%' cellpadding='10' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;'>
                                <tr>
                                    <td width='22%' style='color:#64748b;font-weight:500;'>Tên điểm đo:</td>
                                    <td style='font-weight:700;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 3] BẢNG BỘ CHỈ SỐ ĐO ĐẾM & SẢN LƯỢNG 3 BIỂU GIÁ    -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse;border-color:#e2e8f0;font-size:13px;'>
                                <tr style='background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;'>
                                    <td width='32%'>Biểu giá</td>
                                    <td width='22%'>Chỉ số cũ</td>
                                    <td width='22%'>Chỉ số mới</td>
                                    <td width='24%'>Sản lượng (kWh)</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Bình thường (BT)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousBt}}</td>
                                    <td align='right' style='color:#475569;'>{{Bt}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionBt}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Cao điểm (CD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousCd}}</td>
                                    <td align='right' style='color:#475569;'>{{Cd}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionCd}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Thấp điểm (TD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousTd}}</td>
                                    <td align='right' style='color:#475569;'>{{Td}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionTd}}</td>
                                </tr>
                                <tr style='background-color:#fef2f2;font-weight:700;'>
                                    <td style='padding-left:12px;color:#991b1b;'>Tổng điện năng hữu công (SG)</td>
                                    <td align='right' style='color:#991b1b;'>{{PreviousSg}}</td>
                                    <td align='right' style='color:#991b1b;'>{{Sg}}</td>
                                    <td align='right' style='color:#b22222;font-size:14px;'>{{ConsumtionTotal}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;color:#64748b;'>Tổng điện năng phản kháng (VC)</td>
                                    <td align='right' style='color:#64748b;'>{{PreviousVc}}</td>
                                    <td align='right' style='color:#64748b;'>{{Vc}}</td>
                                    <td align='right' style='font-weight:600;color:#64748b;'>{{ConsumtionVc}}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 4] THÔNG SỐ VẬN HÀNH PHỤ TẢI (PMAX & COS PHI)     -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='0' style='background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;'>
                                <tr>
                                    <td width='55%' style='vertical-align:top;'>
                                        • Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>
                                        • Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong>
                                    </td>
                                    <td width='45%' style='vertical-align:top;'>
                                        • Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 5] THÔNG BÁO TỆP ĐÍNH KÈM PHIẾU BÁO (PDF)          -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <div style='background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;'>
                                📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] THÔNG TIN LIÊN HỆ ĐỐI SOÁT & CHĂM SÓC KHÁCH HÀNG -->
                    <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây           -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 24px 30px;'>
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}<br>
                                📞 Điện thoại: {{ContactPhone}}<br>
                                ✉️ Email: {{CareEmail}}
                            </div>
                            <p style='margin:16px 0 0 0;font-size:13.5px;color:#334155;'>Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 7] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS) - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return (Render(subjectTpl, context), Render(bodyTpl, context));
        }

        if (code == Constants.NotificationTemplateCode.UserCredential)
        {
            var subjectTpl = "[MDMS] Thông tin tài khoản - {{FullName}}";
            var bodyTpl = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Tin Cấp Tài Khoản</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên sửa tên công ty / hệ thống tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>THÔNG TIN TÀI KHOẢN</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN TÀI KHOẢN ĐĂNG NHẬP                   -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:32px 32px 20px 32px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi Quý người dùng: <strong>{{FullName}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;'>
                                Tài khoản truy cập Hệ thống Quản trị Dữ liệu Đo đếm Điện tử (MDMS) của Quý vị đã được cấp thành công. Thông tin tài khoản đăng nhập chi tiết như sau:
                            </p>
                            <table width='100%' cellpadding='12' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:24px;font-size:14px;'>
                                <tr>
                                    <td width='35%' style='color:#64748b;font-weight:500;'>Tên đăng nhập:</td>
                                    <td width='65%' style='color:#0f172a;font-weight:700;font-family:monospace;font-size:15px;'>{{Username}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;font-weight:500;'>Mật khẩu khởi tạo:</td>
                                    <td style='color:#b22222;font-weight:700;font-family:monospace;font-size:15px;'>{{Password}}</td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 3] ĐƯỜNG DẪN TRUY CẬP HỆ THỐNG (NÚT BẤM CTA)  -->
                            <!-- ==================================================== -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{LoginUrl}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                                            Đăng Nhập
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 4] KHUYẾN CÁO AN TOÀN & ĐỔI MẬT KHẨU LẦN ĐẦU   -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;'>
                                <p style='margin:0;font-size:13px;color:#92400e;line-height:1.5;'>
                                    ⚠️ <strong>Khuyến cáo an toàn:</strong> Vì lý do bảo mật thông tin, Quý vị vui lòng thực hiện đổi mật khẩu ngay trong lần đăng nhập đầu tiên và tuyệt đối không chia sẻ mật khẩu này cho bất kỳ ai.
                                </p>
                            </div>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 5] THÔNG TIN HỖ TRỢ KỸ THUẬT                  -->
                            <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây      -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>
                                📞 Hotline: {{SupportPhone}}<br>
                                ✉️ Email: {{SystemEmail}}
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return (Render(subjectTpl, context), Render(bodyTpl, context));
        }

        if (code == Constants.NotificationTemplateCode.ForgotPassword)
        {
            var subjectTpl = "[MDMS] Yêu cầu đặt lại mật khẩu";
            var bodyTpl = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Yêu Cầu Đặt Lại Mật Khẩu</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:580px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên sửa tên công ty / hệ thống tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>YÊU CẦU ĐẶT LẠI MẬT KHẨU</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN YÊU CẦU ĐẶT LẠI MẬT KHẨU             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:32px 32px 20px 32px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi <strong>{{Username}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;'>
                                Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu đăng nhập cho tài khoản liên kết với địa chỉ email này trên hệ thống MDMS. Để hoàn tất quy trình, Quý vị vui lòng nhấp vào nút thao tác bên dưới:
                            </p>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 3] LIÊN KẾT ĐẶT LẠI MẬT KHẨU (NÚT BẤM CTA)     -->
                            <!-- ==================================================== -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{ResetLink}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                                            Thiết Lập Lại Mật Khẩu
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 4] CẢNH BÁO THỜI HẠN VÀ KHUYẾN CÁO AN TOÀN    -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;'>
                                <p style='margin:0;font-size:12.5px;color:#92400e;line-height:1.5;'>
                                    ⏱️ <strong>Lưu ý bảo mật:</strong> Liên kết đặt lại mật khẩu có hiệu lực trong vòng <strong>10 phút</strong> kể từ khi phát hành. Nếu Quý vị không gửi yêu cầu này, vui lòng bỏ qua thư hoặc thông báo ngay cho quản trị viên.
                                </p>
                            </div>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 5] THÔNG TIN HỖ TRỢ KỸ THUẬT                  -->
                            <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây      -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>
                                📞 Hotline: {{SupportPhone}}<br>
                                ✉️ Email: {{SystemEmail}}
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return (Render(subjectTpl, context), Render(bodyTpl, context));
        }

        // Default generic
        return ($"Thông báo từ hệ thống MDMS", Render("<p>Kính gửi Quý khách,</p><p>Hệ thống MDMS gửi thông báo đến Quý khách.</p>", context));
    }

    public static List<NotificationTemplateEntity> GetPrebuiltDefaultTemplates()
    {
        var dummyService = new NotificationTemplateService(null!, null!);
        var dummyContext = new Dictionary<string, object?>();

        var list = new List<NotificationTemplateEntity>();

        // 1. STATEMENT_NOTIFICATION (Phiếu chỉ số) - Default
        var (stmtSubj, stmtBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.StatementNotification, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.StatementNotification,
            Name = "Phiếu chỉ số",
            SubjectTemplate = stmtSubj,
            BodyHtmlTemplate = stmtBody,
            Description = "Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF",
            IsDefault = true,
            IsActive = true
        });

        // 2. USER_CREDENTIAL (Cấp mới tài khoản & mật khẩu) - Default
        var (userSubj, userBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.UserCredential, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.UserCredential,
            Name = "Cấp mới tài khoản & mật khẩu",
            SubjectTemplate = userSubj,
            BodyHtmlTemplate = userBody,
            Description = "Mẫu email gửi thông tin tên đăng nhập và mật khẩu khi tài khoản được cấp mới",
            IsDefault = true,
            IsActive = true
        });

        // 3. FORGOT_PASSWORD (Đặt lại mật khẩu) - Default
        var (forgotSubj, forgotBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.ForgotPassword, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.ForgotPassword,
            Name = "Đặt lại mật khẩu",
            SubjectTemplate = forgotSubj,
            BodyHtmlTemplate = forgotBody,
            Description = "Mẫu email gửi link yêu cầu thiết lập lại mật khẩu người dùng",
            IsDefault = true,
            IsActive = true
        });

        return list;
    }
}
