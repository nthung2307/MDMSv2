using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DataTransferObjects.Email;
using Application.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailServiceService : IEmailService
{
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly IConfiguration _config;

    public EmailServiceService(IEmailProviderFactory emailProviderFactory, IConfiguration config)
    {
        _emailProviderFactory = emailProviderFactory;
        _config = config;
    }

    public async Task SendEmailForgotPassword(string to, string subject, string token, string userName)
    {
        var frontendUrl = _config["FrontEndUrl"] ?? "https://amr.nobisoft.vn";
        var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?token={token}&email={to}";

        var bodyHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Yêu cầu đặt lại mật khẩu</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f5f7fc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #334155; -webkit-font-smoothing: antialiased;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f5f7fc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width: 560px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.06); border: 1px solid #e2e8f0;'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #b22222 0%, #8b1a1a 100%); padding: 28px 32px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 20px; font-weight: 700; letter-spacing: 0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM (MDMS)</h1>
                            <p style='margin: 6px 0 0 0; color: #fecaca; font-size: 13px;'>Yêu cầu đặt lại mật khẩu người dùng</p>
                        </td>
                    </tr>
                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 36px 32px 28px 32px;'>
                            <p style='margin: 0 0 16px 0; font-size: 15px; line-height: 1.6;'>Kính gửi <strong>{userName}</strong>,</p>
                            <p style='margin: 0 0 24px 0; font-size: 14px; line-height: 1.7; color: #475569;'>
                                Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu cho tài khoản liên kết với email này trên hệ thống MDMS. Để hoàn tất quá trình đổi mật khẩu, vui lòng nhấp vào nút bên dưới:
                            </p>
                            
                            <!-- Action Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 30px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{resetLink}' target='_blank' style='display: inline-block; background-color: #b22222; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; padding: 13px 36px; border-radius: 6px; box-shadow: 0 2px 6px rgba(178,34,34,0.35); text-align: center;'>
                                            Thiết lập lại mật khẩu
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Expiry Note -->
                            <div style='background-color: #fffbeb; border: 1px solid #fef3c7; border-left: 4px solid #f59e0b; border-radius: 6px; padding: 12px 16px; margin-bottom: 24px;'>
                                <p style='margin: 0; font-size: 12.5px; line-height: 1.6; color: #92400e;'>
                                    <strong>Lưu ý bảo mật:</strong> Liên kết này chỉ có hiệu lực trong vòng <strong>10 phút</strong>. Nếu quá thời hạn trên, vui lòng thực hiện lại thao tác quên mật khẩu.
                                </p>
                            </div>

                            <p style='margin: 0 0 12px 0; font-size: 13px; line-height: 1.6; color: #64748b;'>
                                Nếu nút bấm trên không hoạt động, bạn có thể sao chép và dán liên kết sau vào trình duyệt:
                            </p>
                            <p style='margin: 0 0 24px 0; word-break: break-all; font-size: 12px; line-height: 1.5; color: #0284c7;'>
                                <a href='{resetLink}' style='color: #0284c7; text-decoration: underline;'>{resetLink}</a>
                            </p>

                            <p style='margin: 0 0 28px 0; font-size: 13px; line-height: 1.6; color: #64748b;'>
                                Nếu bạn không thực hiện yêu cầu này, xin vui lòng bỏ qua email. Mật khẩu hiện tại của bạn vẫn được giữ nguyên an toàn.
                            </p>

                            <div style='border-top: 1px solid #f1f5f9; padding-top: 20px;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>Trân trọng,</p>
                                <p style='margin: 4px 0 0 0; font-size: 14px; font-weight: 600; color: #1e293b;'>Ban Quản Trị Hệ Thống MDMS</p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 18px 32px; text-align: center;'>
                            <p style='margin: 0; font-size: 12px; color: #94a3b8; line-height: 1.5;'>
                                Đây là email tự động từ hệ thống MDMS. Vui lòng không trả lời trực tiếp email này.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

        var provider = await _emailProviderFactory.GetActiveProviderAsync();
        await provider.SendEmailAsync(new EmailMessageDto
        {
            To = new List<string> { to },
            Subject = subject,
            BodyHtml = bodyHtml
        });
    }

    public async Task SendEmailGeneratePassword(string to, string subject, string password, string fullName, string userName)
    {
        var frontendUrl = _config["FrontEndUrl"] ?? "https://amr.nobisoft.vn";
        var loginUrl = $"{frontendUrl.TrimEnd('/')}/login";

        var bodyHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thông tin tài khoản hệ thống MDMS</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f5f7fc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #334155; -webkit-font-smoothing: antialiased;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f5f7fc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width: 560px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.06); border: 1px solid #e2e8f0;'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #b22222 0%, #8b1a1a 100%); padding: 28px 32px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 20px; font-weight: 700; letter-spacing: 0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM (MDMS)</h1>
                            <p style='margin: 6px 0 0 0; color: #fecaca; font-size: 13px;'>Thông tin cấp tài khoản người dùng</p>
                        </td>
                    </tr>
                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 36px 32px 28px 32px;'>
                            <p style='margin: 0 0 16px 0; font-size: 15px; line-height: 1.6;'>Kính gửi <strong>{fullName}</strong>,</p>
                            <p style='margin: 0 0 20px 0; font-size: 14px; line-height: 1.7; color: #475569;'>
                                Tài khoản truy cập Hệ thống Quản lý Dữ liệu Đo đếm (MDMS) của bạn đã được khởi tạo thành công. Dưới đây là thông tin đăng nhập:
                            </p>
                            
                            <!-- Credentials Box -->
                            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 18px 20px; margin: 20px 0;'>
                                <p style='margin: 0 0 10px 0; font-size: 14px;'><strong>Tên đăng nhập:</strong> <code style='color: #b22222; font-size: 15px; font-weight: 700; background: #fee2e2; padding: 2px 8px; border-radius: 4px;'>{userName}</code></p>
                                <p style='margin: 0; font-size: 14px;'><strong>Mật khẩu khởi tạo:</strong> <code style='color: #0f172a; font-size: 15px; font-weight: 700; background: #e2e8f0; padding: 2px 8px; border-radius: 4px;'>{password}</code></p>
                            </div>

                            <!-- Action Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 28px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{loginUrl}' target='_blank' style='display: inline-block; background-color: #b22222; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; padding: 13px 36px; border-radius: 6px; box-shadow: 0 2px 6px rgba(178,34,34,0.35); text-align: center;'>
                                            Đăng nhập hệ thống
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <div style='background-color: #eff6ff; border: 1px solid #dbeafe; border-left: 4px solid #3b82f6; border-radius: 6px; padding: 12px 16px; margin-bottom: 24px;'>
                                <p style='margin: 0; font-size: 12.5px; line-height: 1.6; color: #1e40af;'>
                                    <strong>Khuyến nghị an toàn:</strong> Vì lý do bảo mật, vui lòng tiến hành thay đổi mật khẩu ngay sau lần đăng nhập đầu tiên.
                                </p>
                            </div>

                            <div style='border-top: 1px solid #f1f5f9; padding-top: 20px;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>Trân trọng,</p>
                                <p style='margin: 4px 0 0 0; font-size: 14px; font-weight: 600; color: #1e293b;'>Ban Quản Trị Hệ Thống MDMS</p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 18px 32px; text-align: center;'>
                            <p style='margin: 0; font-size: 12px; color: #94a3b8; line-height: 1.5;'>
                                Đây là email tự động từ hệ thống MDMS. Vui lòng không trả lời trực tiếp email này.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

        var provider = await _emailProviderFactory.GetActiveProviderAsync();
        await provider.SendEmailAsync(new EmailMessageDto
        {
            To = new List<string> { to },
            Subject = subject,
            BodyHtml = bodyHtml
        });
    }
}