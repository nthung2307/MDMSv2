using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Email;
using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.EmailProviders;

public class MicrosoftGraphEmailProvider : IEmailProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MicrosoftGraphEmailProvider> _logger;

    public string ProviderType => Constants.EmailProvider.MicrosoftGraph;

    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;

    public MicrosoftGraphEmailProvider(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        ILogger<MicrosoftGraphEmailProvider> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(TenantId) || string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException("Microsoft Graph API chưa được cấu hình đầy đủ TenantId, ClientId hoặc ClientSecret.");
        }

        var cacheKey = $"GraphToken_{TenantId}_{ClientId}";
        if (_memoryCache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        var tokenEndpoint = $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
            new KeyValuePair<string, string>("client_secret", ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        _logger.LogInformation("Đang yêu cầu Access Token từ Microsoft Entra ID cho ClientId: {ClientId}", ClientId);
        var response = await _httpClient.PostAsync(tokenEndpoint, formContent, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Lỗi khi lấy Microsoft Graph Token: HTTP {StatusCode} - {Content}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Không thể xác thực với Microsoft Azure AD (HTTP {response.StatusCode}): {responseContent}");
        }

        var tokenResponse = JsonSerializer.Deserialize<GraphTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Phản hồi Access Token từ Microsoft Graph không hợp lệ.");
        }

        // Cache token với thời hạn an toàn (trừ đi 300 giây tránh rủi ro hết hạn giữa chừng)
        var expiresInSeconds = Math.Max(tokenResponse.ExpiresIn - 300, 60);
        _memoryCache.Set(cacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(expiresInSeconds));

        return tokenResponse.AccessToken;
    }

    public async Task SendEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(SenderEmail))
        {
            throw new InvalidOperationException("Chưa cấu hình địa chỉ email gửi (SenderEmail).");
        }

        if (message.To == null || message.To.Count == 0)
        {
            throw new ArgumentException("Danh sách người nhận không được để trống.", nameof(message.To));
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var sendMailEndpoint = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(SenderEmail.Trim())}/sendMail";

        var payload = new
        {
            message = new
            {
                subject = message.Subject,
                body = new
                {
                    contentType = "HTML",
                    content = message.BodyHtml
                },
                toRecipients = message.To.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new
                {
                    emailAddress = new
                    {
                        address = e.Trim()
                    }
                }).ToArray(),
                ccRecipients = message.Cc?.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new
                {
                    emailAddress = new
                    {
                        address = e.Trim()
                    }
                }).ToArray() ?? Array.Empty<object>(),
                bccRecipients = message.Bcc?.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new
                {
                    emailAddress = new
                    {
                        address = e.Trim()
                    }
                }).ToArray() ?? Array.Empty<object>(),
                attachments = message.Attachments?.Select(att => new
                {
                    odataType = "#microsoft.graph.fileAttachment",
                    name = att.FileName,
                    contentType = att.ContentType,
                    contentBytes = Convert.ToBase64String(att.ContentBytes)
                }).ToArray() ?? Array.Empty<object>()
            },
            saveToSentItems = true
        };

        var json = JsonSerializer.Serialize(payload);
        // Replace odataType attribute to @odata.type required by Microsoft Graph
        json = json.Replace("\"odataType\":", "\"@odata.type\":");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendMailEndpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Gửi email qua Microsoft Graph API từ {SenderEmail} tới {RecipientsCount} người nhận...", 
            SenderEmail, message.To.Count);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gửi mail thất bại qua Microsoft Graph API: HTTP {StatusCode} - {ErrorContent}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException($"Microsoft Graph API trả về lỗi (HTTP {response.StatusCode}): {errorContent}");
        }

        _logger.LogInformation("Gửi email thành công qua Microsoft Graph API tới: {To}", string.Join(", ", message.To));
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
            Subject = $"[MDMSv2] Kiểm Tra Kết Nối Microsoft Graph API Thành Công ({DateTime.Now:dd/MM/yyyy HH:mm:ss})",
            BodyHtml = $@"<html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h3 style='color: #0284c7;'>Hệ Thống Quản Lý Dữ Liệu Đo Đếm MDMSv2</h3>
                    <p>Xin chào,</p>
                    <p>Đây là email kiểm tra kết nối từ hệ thống MDMSv2 được gửi thông qua dịch vụ <b>Microsoft Graph API (OAuth 2.0)</b>.</p>
                    <div style='background: #f0fdf4; border: 1px solid #bbf7d0; padding: 12px; border-radius: 6px; margin: 15px 0;'>
                        <p style='margin: 0; color: #166534;'><b>Trạng thái:</b> Kết nối thông suốt và xác thực thành công!</p>
                        <ul style='margin: 8px 0 0 0; color: #166534;'>
                            <li><b>Tài khoản gửi:</b> {SenderEmail}</li>
                            <li><b>Tenant ID:</b> {TenantId}</li>
                            <li><b>Client ID:</b> {ClientId}</li>
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

    private class GraphTokenResponse
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
