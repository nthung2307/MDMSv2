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
            ["CurrentDate"] = DateTime.Now.ToString("dd/MM/yyyy")
        };

        return dict;
    }

    private (string Subject, string BodyHtml) GetFallbackDefaultTemplate(string code, IDictionary<string, object?> context)
    {
        code = code.ToUpperInvariant();
        if (code == Constants.NotificationTemplateCode.StatementNotification)
        {
            var subjectTpl = "Phiếu Báo Chỉ Số Điện Số {{SerialNumber}} - {{CustomerName}} (Kỳ {{Period}})";
            var bodyTpl = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f5f7fc;padding:30px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:640px;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên có thể thay đổi tên đơn vị tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
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
                                Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: <strong>{{Period}}</strong> (từ ngày {{FromDate}} đến ngày {{ToDate}}).
                            </p>
                            <table width='100%' cellpadding='8' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13px;'>
                                <tr>
                                    <td width='25%' style='color:#64748b;'>Mã khách hàng:</td>
                                    <td width='25%' style='font-weight:600;color:#0f172a;'>{{CustomerCode}}</td>
                                    <td width='25%' style='color:#64748b;'>Tên điểm đo:</td>
                                    <td width='25%' style='font-weight:600;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;'>Mã điểm đo:</td>
                                    <td style='font-weight:600;color:#0f172a;'>{{PointCode}}</td>
                                    <td style='color:#64748b;'>Số công tơ:</td>
                                    <td style='font-weight:600;color:#0f172a;'>{{MeterSerial}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;'>Hệ số nhân (HSN):</td>
                                    <td style='font-weight:700;color:#b22222;'>{{Multiplier}}</td>
                                    <td style='color:#64748b;'>Địa chỉ:</td>
                                    <td style='color:#334155;'>{{Address}}</td>
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
                                    <td style='padding-left:12px;color:#991b1b;'>Tổng điện hữu công (SG)</td>
                                    <td align='right' style='color:#991b1b;'>{{PreviousSg}}</td>
                                    <td align='right' style='color:#991b1b;'>{{Sg}}</td>
                                    <td align='right' style='color:#b22222;font-size:14px;'>{{ConsumtionTotal}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;color:#64748b;'>Điện phản kháng (VC - kVARh)</td>
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
                                    <td width='50%'>
                                        • Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>
                                        • Thời điểm xuất hiện Pmax: <strong>{{PmaxTime}}</strong>
                                    </td>
                                    <td width='50%' style='vertical-align:top;'>
                                        • Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong><br>
                                        • Tình trạng chốt: <span style='color:#16a34a;font-weight:600;'>Đã xác nhận dữ liệu</span>
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
                                📎 <strong>Tệp đính kèm:</strong> Chi tiết phiếu báo chỉ số điện năng định dạng PDF đã được đính kèm theo thư này: <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>. Quý khách có thể tải về và lưu trữ để phục vụ đối soát và thanh toán.
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
                                Mọi thắc mắc hoặc yêu cầu đối soát số liệu đo đếm, Quý khách vui lòng liên hệ: <strong>{{CareFullname}}</strong><br>
                                📞 Hotline CSKH: <strong>{{CarePhone}}</strong> | ✉️ Email tiếp nhận: <strong>{{CareEmail}}</strong>
                            </div>
                            <p style='margin:16px 0 0 0;font-size:13.5px;color:#334155;'>Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 7] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.
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
            var subjectTpl = "[MDMS] Thông tin cấp tài khoản người dùng hệ thống - {{FullName}}";
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
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>THÔNG TIN CẤP TÀI KHOẢN NGƯỜI DÙNG</p>
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
                                <tr>
                                    <td style='color:#64748b;font-weight:500;'>Đơn vị liên kết:</td>
                                    <td style='color:#0f172a;font-weight:600;'>{{CustomerName}}</td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 3] ĐƯỜNG DẪN TRUY CẬP HỆ THỐNG (NÚT BẤM CTA)  -->
                            <!-- ==================================================== -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{LoginUrl}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                                            Đăng Nhập Vào Hệ Thống
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
                                Nếu Quý vị cần hỗ trợ kỹ thuật hoặc cấp lại quyền truy cập, vui lòng liên hệ: <strong>{{CareFullname}}</strong><br>
                                📞 Hotline hỗ trợ: <strong>{{SupportPhone}}</strong> | ✉️ Email kỹ thuật: <strong>{{SystemEmail}}</strong>
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
            var subjectTpl = "Yêu cầu đặt lại mật khẩu - Hệ thống MDMS";
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
                                Hotline hỗ trợ: <strong>{{SupportPhone}}</strong> | Email tiếp nhận: <strong>{{SystemEmail}}</strong>
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

        // 1. STATEMENT_NOTIFICATION (Tiêu chuẩn doanh nghiệp) - Default
        var (stmtSubj, stmtBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.StatementNotification, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.StatementNotification,
            Name = "Mẫu Phiếu Báo Điện Năng - Tiêu Chuẩn Doanh Nghiệp (Phân Khối Khoa Học)",
            SubjectTemplate = stmtSubj,
            BodyHtmlTemplate = stmtBody,
            Description = "Mẫu chuẩn doanh nghiệp gửi phiếu báo chỉ số và sản lượng điện năng hàng tháng, cấu trúc phân khối comment rõ ràng, đầy đủ bảng 3 giá, Pmax, Cos phi và tệp đính kèm PDF.",
            IsDefault = true,
            IsActive = true
        });

        // 2. STATEMENT_NOTIFICATION (Hiện đại tinh gọn)
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.StatementNotification,
            Name = "Mẫu Phiếu Báo Điện Năng - Hiện Đại Tinh Gọn",
            SubjectTemplate = "Thông báo sản lượng điện năng tiêu thụ {{Period}} - {{CustomerName}}",
            BodyHtmlTemplate = @"<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background-color:#f1f5f9;font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,sans-serif;color:#1e293b;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:32px 16px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:600px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 25px -5px rgba(0,0,0,0.05);border:1px solid #e2e8f0;'>
                    <tr>
                        <td style='background:#0f172a;padding:28px 32px;'>
                            <span style='background:#b22222;color:#ffffff;font-size:11px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;'>BÁO CÁO ĐIỆN NĂNG</span>
                            <h2 style='margin:12px 0 4px 0;color:#ffffff;font-size:20px;'>{{CustomerName}}</h2>
                            <p style='margin:0;color:#94a3b8;font-size:13px;'>Kỳ ghi chỉ số: {{Period}} (Điểm đo: {{PointCode}})</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:32px;'>
                            <div style='background:#f8fafc;border-radius:12px;padding:20px;text-align:center;margin-bottom:24px;border:1px solid #e2e8f0;'>
                                <div style='color:#64748b;font-size:13px;font-weight:600;text-transform:uppercase;'>Tổng Điện Năng Hữu Công Tiêu Thụ</div>
                                <div style='color:#b22222;font-size:36px;font-weight:800;margin:8px 0;'>{{ConsumtionTotal}} <span style='font-size:18px;font-weight:500;color:#64748b;'>kWh</span></div>
                                <div style='font-size:12.5px;color:#64748b;'>Từ ngày {{FromDate}} đến {{ToDate}}</div>
                            </div>
                            <table width='100%' cellpadding='10' cellspacing='0' border='0' style='margin-bottom:24px;font-size:13px;'>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Bình thường (BT):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionBt}} kWh</td>
                                </tr>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Cao điểm (CD):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionCd}} kWh</td>
                                </tr>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Thấp điểm (TD):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionTd}} kWh</td>
                                </tr>
                                <tr>
                                    <td style='color:#475569;'>• Công suất cực đại Pmax:</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{Pmax}}</td>
                                </tr>
                            </table>
                            <div style='background:#f1f5f9;border-radius:8px;padding:14px;font-size:12.5px;color:#475569;line-height:1.6;'>
                                Chi tiết bảng kê chỉ số công tơ được đính kèm trong tệp <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>. Hotline giải đáp: <strong>{{SupportPhone}}</strong>.
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
            Description = "Mẫu phong cách thẻ Card hiện đại, làm nổi bật chỉ số tổng và biểu giá 3 pha.",
            IsDefault = false,
            IsActive = true
        });

        // 3. USER_CREDENTIAL (Cấp tài khoản mới) - Default
        var (userSubj, userBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.UserCredential, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.UserCredential,
            Name = "Mẫu Cấp Tài Khoản & Mật Khẩu Người Dùng Mới (Phân Khối Khoa Học)",
            SubjectTemplate = userSubj,
            BodyHtmlTemplate = userBody,
            Description = "Mẫu chuẩn doanh nghiệp gửi thông tin cấp tài khoản người dùng mới, cấu trúc phân khối rõ ràng, nút đăng nhập nhanh và lưu ý bảo mật an toàn.",
            IsDefault = true,
            IsActive = true
        });

        // 4. FORGOT_PASSWORD (Đặt lại mật khẩu) - Default
        var (forgotSubj, forgotBody) = dummyService.GetFallbackDefaultTemplate(Constants.NotificationTemplateCode.ForgotPassword, dummyContext);
        list.Add(new NotificationTemplateEntity
        {
            Code = Constants.NotificationTemplateCode.ForgotPassword,
            Name = "Mẫu Yêu Cầu Thiết Lập Lại Mật Khẩu (Phân Khối Khoa Học)",
            SubjectTemplate = forgotSubj,
            BodyHtmlTemplate = forgotBody,
            Description = "Mẫu chuẩn doanh nghiệp gửi link xác thực đặt lại mật khẩu người dùng với nút thao tác nhanh, cảnh báo thời hạn 10 phút và lưu ý an toàn.",
            IsDefault = true,
            IsActive = true
        });

        return list;
    }
}
