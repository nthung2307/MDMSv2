using Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services;

public class EmailServiceService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailServiceService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailForgotPassword(string to, string subject, string token, string userName)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config.GetSection("SendMail:EmailUsername").Value));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder();

        var test = $@"<html>
                    <body>
                        <p>Kính gửi {userName},</p>
                        <p></p>
                        <p>Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu cho tài khoản của bạn trên hệ thống MDMS. Nếu bạn đã yêu cầu thay đổi mật khẩu, vui lòng nhấp vào liên kết dưới đây để hoàn tất quá trình thiết lập lại:</p>
                        <p><b>https://amr.nobisoft.vn/reset-password?token={token}&email={to}</b></p>
                        <p></p>
                        <p>Liên kết này sẽ hết hạn sau 10 phút, vì vậy hãy đảm bảo bạn thực hiện thao tác trước thời gian này.</p>
                        <p>Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.</p>
                        <p></p>
                        <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ.</p>
                        <p>Trân trọng,</p>
                    </body>
                </html>";
        builder.HtmlBody = test;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config.GetSection("SendMail:EmailHost").Value, 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config.GetSection("SendMail:EmailUsername").Value, _config.GetSection("SendMail:EmailPassword").Value);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendEmailGeneratePassword(string to, string subject, string password, string fullName, string userName)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config.GetSection("SendMail:EmailUsername").Value));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder();

        var test = $@"<html>
                    <body>
                        <p>Kính gửi {fullName},</p>
                        <p></p>
                        <p>Chúng tôi xin thông báo rằng tài khoản của bạn trong hệ thống AMR đã được tạo thành công. Dưới đây là thông tin đăng nhập của bạn</p>
                        <p><strong>Tên đăng nhập: {userName}</strong></p>
                        <p><strong>Mật khẩu: {password}</strong></p>
                        <p>Liên kết đăng nhập: https://amr.nobisoft.vn/login</p>
                        <p></p>
                        <p>Nếu bạn gặp bất kỳ vấn đề gì trong quá trình đăng nhập, vui lòng liên hệ với bộ phận hỗ trợ để được giáp đáp thắc mắc.</p>
                        <p></p>
                        <p>Trân trọng.</p>
                    </body>
                </html>";
        builder.HtmlBody = test;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config.GetSection("SendMail:EmailHost").Value, 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config.GetSection("SendMail:EmailUsername").Value, _config.GetSection("SendMail:EmailPassword").Value);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}