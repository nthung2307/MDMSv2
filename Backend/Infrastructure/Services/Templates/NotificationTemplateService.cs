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
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Templates;

public class NotificationTemplateService : INotificationTemplateService
{
    private static readonly Regex VariableRegex = new(@"\{\{\s*([\w\.\-]+)(?::([^}]+))?\s*\}\}", RegexOptions.Compiled);
    private readonly INotificationTemplateReadOnlyRepository _repository;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        INotificationTemplateReadOnlyRepository repository,
        ILogger<NotificationTemplateService> logger)
    {
        _repository = repository;
        _logger = logger;
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

    public async Task<(string Subject, string BodyHtml)> RenderNotificationAsync(string code, IDictionary<string, object?> context, CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetDefaultByCodeAsync(code, cancellationToken);
        if (template != null)
        {
            var subject = Render(template.SubjectTemplate, context);
            var body = Render(template.BodyHtmlTemplate, context);
            return (subject, body);
        }

        // Fallback to built-in default templates
        return GetFallbackDefaultTemplate(code, context);
    }

    public Task<PreviewNotificationTemplateResponse> PreviewAsync(PreviewNotificationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var sampleContext = GetSampleVariablesForCode(request.Code);
        if (request.CustomVariables != null)
        {
            foreach (var kv in request.CustomVariables)
            {
                sampleContext[kv.Key] = kv.Value;
            }
        }

        var renderedSubject = Render(request.SubjectTemplate, sampleContext);
        var renderedBody = Render(request.BodyHtmlTemplate, sampleContext);

        return Task.FromResult(new PreviewNotificationTemplateResponse
        {
            Subject = renderedSubject,
            BodyHtml = renderedBody
        });
    }

    public static Dictionary<string, object?> GetSampleVariablesForCode(string? code)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["CustomerName"] = "CÔNG TY TNHH KYOCERA VIỆT NAM",
            ["PointCode"] = "101.402.001",
            ["SerialNumber"] = "PB-202610-001",
            ["Address"] = "Lô đất số IN1-4, KCN VSIP Hải Phòng, Xã An Lư, Huyện Thủy Nguyên, TP. Hải Phòng",
            ["FromDate"] = "01/10/2026",
            ["ToDate"] = "31/10/2026",
            ["Month"] = "10",
            ["Year"] = "2026",
            ["Period"] = "Kỳ 1 Tháng 10/2026",
            ["PreviousBt"] = "1,245,670",
            ["PreviousCd"] = "345,120",
            ["PreviousTd"] = "560,890",
            ["PreviousSg"] = "2,151,680",
            ["Bt"] = "1,295,430",
            ["Cd"] = "362,450",
            ["Td"] = "581,200",
            ["Sg"] = "2,239,080",
            ["ConsumtionBt"] = "49,760",
            ["ConsumtionCd"] = "17,330",
            ["ConsumtionTd"] = "20,310",
            ["ConsumtionSg"] = "87,400",
            ["ConsumtionTotal"] = "87,400",
            ["ConsumtionVc"] = "12,450",
            ["Pmax"] = "1,450.85 kW",
            ["PmaxTime"] = "15/10/2026 14:30:00",
            ["CareFullname"] = "Phòng Quản Lý Vận Hành Cơ Điện VSIP",
            ["CareEmail"] = "noreply.hoadondientu.hp@vsip.com.vn",
            ["CarePhone"] = "0225 3959868",
            ["Username"] = "kyocera_user",
            ["Password"] = "Vsip@2026#",
            ["ResetLink"] = "https://amr.nobisoft.vn/reset-password?token=sample_demo_token&email=admin@kyocera.com.vn",
            ["CurrentDate"] = DateTime.Now.ToString("dd/MM/yyyy")
        };

        return dict;
    }

    private (string Subject, string BodyHtml) GetFallbackDefaultTemplate(string code, IDictionary<string, object?> context)
    {
        code = code.ToUpperInvariant();
        if (code == Constants.NotificationTemplateCode.StatementNotification)
        {
            var subjectTpl = "Phiếu Báo Số {{SerialNumber}} - {{CustomerName}}";
            var bodyTpl = @"<html>
<head>
    <meta charset='utf-8'>
    <title>Phiếu Báo Điện Năng</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f5f7fc;padding:30px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:620px;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>CÔNG TY TNHH VSIP HẢI PHÒNG</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;'>THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG HÀNG THÁNG</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:30px 30px;'>
                            <p style='margin:0 0 16px 0;font-size:15px;'>Kính gửi Quý khách hàng <strong>{{CustomerName}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.6;color:#475569;'>
                                Chúng tôi xin trân trọng gửi đến Quý khách hàng thông tin chỉ số và sản lượng điện năng của điểm đo <strong>{{PointCode}}</strong> trong <strong>{{Period}}</strong> (từ ngày {{FromDate}} đến ngày {{ToDate}}).
                            </p>
                            <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse;border-color:#e2e8f0;margin-bottom:20px;font-size:13px;'>
                                <tr style='background-color:#f8fafc;font-weight:600;text-align:center;'>
                                    <td>Biểu giá</td>
                                    <td>Chỉ số cũ</td>
                                    <td>Chỉ số mới</td>
                                    <td>Sản lượng (kWh)</td>
                                </tr>
                                <tr>
                                    <td>Bình thường (BT)</td>
                                    <td align='right'>{{PreviousBt}}</td>
                                    <td align='right'>{{Bt}}</td>
                                    <td align='right'><strong>{{ConsumtionBt}}</strong></td>
                                </tr>
                                <tr>
                                    <td>Cao điểm (CD)</td>
                                    <td align='right'>{{PreviousCd}}</td>
                                    <td align='right'>{{Cd}}</td>
                                    <td align='right'><strong>{{ConsumtionCd}}</strong></td>
                                </tr>
                                <tr>
                                    <td>Thấp điểm (TD)</td>
                                    <td align='right'>{{PreviousTd}}</td>
                                    <td align='right'>{{Td}}</td>
                                    <td align='right'><strong>{{ConsumtionTd}}</strong></td>
                                </tr>
                                <tr style='background-color:#fef2f2;font-weight:700;'>
                                    <td>Tổng cộng (SG)</td>
                                    <td align='right'>{{PreviousSg}}</td>
                                    <td align='right'>{{Sg}}</td>
                                    <td align='right' style='color:#b22222;'>{{ConsumtionTotal}}</td>
                                </tr>
                            </table>
                            <p style='font-size:13px;color:#475569;margin-bottom:12px;'>
                                * Công suất cực đại Pmax ghi nhận: <strong>{{Pmax}}</strong> (thời điểm: {{PmaxTime}})<br>
                                * Vui lòng xem chi tiết tại tệp đính kèm: <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>
                            </p>
                            <div style='background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;margin-bottom:20px;font-size:13px;color:#475569;'>
                                Mọi thắc mắc hoặc yêu cầu đối soát số liệu, xin vui lòng liên hệ <strong>{{CareFullname}}</strong> qua email <strong>{{CareEmail}}</strong> hoặc hotline <strong>{{CarePhone}}</strong>.
                            </div>
                            <p style='margin:0;font-size:14px;'>Trân trọng cảm ơn sự hợp tác của Quý khách!</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS. Vui lòng không phản hồi thư này.
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
            var subjectTpl = "Yêu cầu đặt lại mật khẩu - Hệ thống MDMS";
            var bodyTpl = @"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:560px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM (MDMS)</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;'>Yêu cầu đặt lại mật khẩu người dùng</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:36px 32px 28px 32px;'>
                            <p style='margin:0 0 16px 0;font-size:15px;'>Kính gửi <strong>{{Username}}</strong>,</p>
                            <p style='margin:0 0 24px 0;font-size:14px;line-height:1.7;color:#475569;'>
                                Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu cho tài khoản liên kết với email này trên hệ thống MDMS. Để hoàn tất, vui lòng nhấp vào nút bên dưới:
                            </p>
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:28px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{ResetLink}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;'>
                                            Thiết lập lại mật khẩu
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:24px;border-radius:4px;'>
                                <p style='margin:0;font-size:12.5px;color:#92400e;'>
                                    <strong>Lưu ý bảo mật:</strong> Liên kết có hiệu lực trong vòng <strong>10 phút</strong>.
                                </p>
                            </div>
                            <p style='margin:0;font-size:13px;color:#64748b;'>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
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
}
