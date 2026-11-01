using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Email;
using Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.EmailProviders;

public class SmtpEmailProvider : IEmailProviderService
{
    private readonly ILogger<SmtpEmailProvider> _logger;

    public string ProviderType { get; set; } = Constants.EmailProvider.CustomSmtp; // CUSTOM_SMTP hoặc GMAIL

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SecurityMode { get; set; } = "StartTls"; // None, Auto, SslOnConnect, StartTls
    public bool RequireAuth { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;

    public SmtpEmailProvider(ILogger<SmtpEmailProvider> logger)
    {
        _logger = logger;
    }

    private SecureSocketOptions ResolveSecurityOptions()
    {
        return SecurityMode?.Trim().ToLowerInvariant() switch
        {
            "none" => SecureSocketOptions.None,
            "sslonconnect" or "ssl" => SecureSocketOptions.SslOnConnect,
            "starttls" or "tls" => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.Auto
        };
    }

    public async Task SendEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(SmtpHost))
        {
            throw new InvalidOperationException("Chưa cấu hình địa chỉ máy chủ SMTP (SmtpHost).");
        }

        if (string.IsNullOrWhiteSpace(SenderEmail))
        {
            throw new InvalidOperationException("Chưa cấu hình địa chỉ email gửi (SenderEmail).");
        }

        if (message.To == null || message.To.Count == 0)
        {
            throw new ArgumentException("Danh sách người nhận không được để trống.", nameof(message.To));
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(SenderDisplayName ?? SenderEmail, SenderEmail.Trim()));

        foreach (var to in message.To.Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            mimeMessage.To.Add(MailboxAddress.Parse(to.Trim()));
        }

        if (message.Cc != null)
        {
            foreach (var cc in message.Cc.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                mimeMessage.Cc.Add(MailboxAddress.Parse(cc.Trim()));
            }
        }

        if (message.Bcc != null)
        {
            foreach (var bcc in message.Bcc.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));
            }
        }

        mimeMessage.Subject = message.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = message.BodyHtml
        };

        if (message.Attachments != null && message.Attachments.Count > 0)
        {
            foreach (var att in message.Attachments)
            {
                if (att.ContentBytes != null && att.ContentBytes.Length > 0)
                {
                    builder.Attachments.Add(att.FileName, att.ContentBytes, ContentType.Parse(att.ContentType ?? "application/pdf"));
                }
            }
        }

        mimeMessage.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            var security = ResolveSecurityOptions();
            _logger.LogInformation("Đang kết nối tới máy chủ SMTP {Host}:{Port} ({Security})...", SmtpHost, SmtpPort, security);
            
            await smtp.ConnectAsync(SmtpHost, SmtpPort, security, cancellationToken);

            if (RequireAuth && !string.IsNullOrWhiteSpace(SmtpUsername))
            {
                var cleanPassword = SmtpPassword?.Trim() ?? string.Empty;
                if (string.Equals(ProviderType, Constants.EmailProvider.Gmail, StringComparison.OrdinalIgnoreCase))
                {
                    // Đối với Gmail, tự động loại bỏ khoảng trắng nếu người dùng copy chuỗi 16 ký tự có dấu cách (vd: abcd efgh ijkl mnop)
                    cleanPassword = cleanPassword.Replace(" ", string.Empty);
                }

                _logger.LogInformation("Đang xác thực tài khoản SMTP cho {Username}...", SmtpUsername);
                await smtp.AuthenticateAsync(SmtpUsername.Trim(), cleanPassword, cancellationToken);
            }

            _logger.LogInformation("Đang gửi email tới {Count} người nhận qua SMTP...", message.To.Count);
            await smtp.SendAsync(mimeMessage, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            _logger.LogInformation("Gửi email thành công qua SMTP tới: {To}", string.Join(", ", message.To));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email qua máy chủ SMTP {Host}:{Port}", SmtpHost, SmtpPort);
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync(string testRecipientEmail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(testRecipientEmail))
        {
            throw new ArgumentException("Email nhận kiểm tra không được để trống.", nameof(testRecipientEmail));
        }

        var testMessage = new EmailMessageDto
        {
            To = new List<string> { testRecipientEmail.Trim() },
            Subject = $"[MDMSv2] Kiểm Tra Kết Nối SMTP Server Thành Công ({DateTime.Now:dd/MM/yyyy HH:mm:ss})",
            BodyHtml = $@"<html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h3 style='color: #0284c7;'>Hệ Thống Quản Lý Dữ Liệu Đo Đếm MDMSv2</h3>
                    <p>Xin chào,</p>
                    <p>Đây là email kiểm tra kết nối từ hệ thống MDMSv2 được gửi thông qua dịch vụ <b>{ProviderType}</b>.</p>
                    <div style='background: #f0fdf4; border: 1px solid #bbf7d0; padding: 12px; border-radius: 6px; margin: 15px 0;'>
                        <p style='margin: 0; color: #166534;'><b>Trạng thái:</b> Kết nối thông suốt và gửi thư thành công!</p>
                        <ul style='margin: 8px 0 0 0; color: #166534;'>
                            <li><b>Tài khoản gửi:</b> {SenderEmail}</li>
                            <li><b>Máy chủ SMTP:</b> {SmtpHost}:{SmtpPort}</li>
                            <li><b>Phương thức bảo mật:</b> {SecurityMode}</li>
                            <li><b>Thời gian kiểm tra:</b> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                    </div>
                    <p>Nếu bạn nhận được email này, tính năng gửi thông báo tài khoản và phiếu báo số điện đã sẵn sàng hoạt động.</p>
                    <p style='color: #64748b; font-size: 13px; margin-top: 30px;'>Trân trọng,<br>Hệ thống MDMSv2 - Ban Quản trị</p>
                </body>
            </html>"
        };

        await SendEmailAsync(testMessage, cancellationToken);
        return true;
    }
}
