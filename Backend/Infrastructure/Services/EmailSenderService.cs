using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Email;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Infrastructure.Interfaces;
using Nobisoft.Core.Mailer.Interfaces;

namespace Infrastructure.Services;

public class EmailSenderService
{
    private readonly IPeriodicEnergyStatementReadWriteRepository _periodicEnergyStatementReadWriteRepository;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly INotificationTemplateService _notificationTemplateService;
    private readonly IStatementPdfGenerator _statementPdfGenerator;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<EmailSenderService> _loggerService;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;
    private readonly IMailingService? _mailingService;

    public EmailSenderService(
        IEmailProviderFactory emailProviderFactory,
        INotificationTemplateService notificationTemplateService,
        IStatementPdfGenerator statementPdfGenerator,
        IDateTimeService dateTimeService,
        ILogger<EmailSenderService> loggerService,
        IPeriodicEnergyStatementReadWriteRepository periodicEnergyStatementReadWriteRepository,
        ISystemConfigReadWriteRepository systemConfigReadWriteRepository,
        ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository,
        IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository,
        IMailingService? mailingService = null)
    {
        _emailProviderFactory = emailProviderFactory;
        _notificationTemplateService = notificationTemplateService;
        _statementPdfGenerator = statementPdfGenerator;
        _dateTimeService = dateTimeService;
        _loggerService = loggerService;
        _periodicEnergyStatementReadWriteRepository = periodicEnergyStatementReadWriteRepository;
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
        _mailingService = mailingService;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new MailAddress(email.Trim());
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    private static StatementEmailStatusDetail ParseStatusDetail(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return new StatementEmailStatusDetail { Overall = "UNSENT" };
        }

        var trimmed = rawStatus.Trim();
        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<StatementEmailStatusDetail>(trimmed, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Overall))
                {
                    return parsed;
                }
            }
            catch
            {
                // Fallback nếu JSON lỗi
            }
        }

        return new StatementEmailStatusDetail
        {
            Overall = trimmed,
            Details = new List<StatementEmailRecipientResult>()
        };
    }

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessSingleStatementEmailAsync(long statementId, CancellationToken cancellationToken)
    {
        var statements = await _periodicEnergyStatementReadWriteRepository
            .GetPeriodicEnergyStatementByIdsAsync(new List<long> { statementId }, cancellationToken);
        var item = statements.FirstOrDefault();
        if (item == null)
        {
            _loggerService.LogWarning("Không tìm thấy PeriodicEnergyStatement với Id = {StatementId}", statementId);
            return;
        }

        // 1. Thu thập tất cả các email được cấu hình cho điểm đo
        var recipients = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.MeasurementPoint?.Email))
            recipients.Add(item.MeasurementPoint.Email.Trim());
        if (!string.IsNullOrWhiteSpace(item.MeasurementPoint?.Email2))
            recipients.Add(item.MeasurementPoint.Email2.Trim());
        if (!string.IsNullOrWhiteSpace(item.MeasurementPoint?.Email3))
            recipients.Add(item.MeasurementPoint.Email3.Trim());

        // Nếu điểm đo không có email nào, fallback sang email của khách hàng liên kết
        if (recipients.Count == 0 && !string.IsNullOrWhiteSpace(item.MeasurementPoint?.Customer?.Email))
        {
            recipients.Add(item.MeasurementPoint.Customer.Email.Trim());
        }

        recipients = recipients.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // TH1: Chưa cấu hình email
        if (recipients.Count == 0)
        {
            _loggerService.LogInformation("Điểm đo {PointCode} (StatementId = {StatementId}) chưa cấu hình email nhận phiếu báo.",
                item.MeasurementPoint?.Code, statementId);
            var emptyStatus = new StatementEmailStatusDetail
            {
                Overall = "NO_EMAIL_CONFIGURED",
                SentTime = _dateTimeService.UtcNow(),
                Details = new List<StatementEmailRecipientResult>()
            };
            item.Status = JsonSerializer.Serialize(emptyStatus);
            item.SentTime = emptyStatus.SentTime;
            await _periodicEnergyStatementReadWriteRepository.UpdateAsync(item, cancellationToken);
            await _periodicEnergyStatementReadWriteRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        // 2. Phân tích trạng thái cũ để nhận diện email nào ĐÃ THÀNH CÔNG trước đó (Smart Selective Resend)
        var previousStatus = ParseStatusDetail(item.Status);
        var alreadySuccessfulMap = previousStatus.Details
            .Where(d => d.Status == "SUCCESS")
            .GroupBy(d => d.Email.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Lọc danh sách email CẦN gửi (chưa gửi hoặc từng bị lỗi)
        var emailsToSend = recipients
            .Where(e => !alreadySuccessfulMap.ContainsKey(e))
            .ToList();

        // Nếu tất cả email đều đã gửi thành công trước đó -> Hoàn tất, không gửi trùng lặp
        if (emailsToSend.Count == 0)
        {
            _loggerService.LogInformation("Tất cả email của điểm đo {PointCode} (StatementId = {StatementId}) đều đã gửi thành công trước đó. Bỏ qua gửi lại.",
                item.MeasurementPoint?.Code, statementId);
            if (previousStatus.Overall != "SEND_SUCCESS")
            {
                previousStatus.Overall = "SEND_SUCCESS";
                item.Status = JsonSerializer.Serialize(previousStatus);
                await _periodicEnergyStatementReadWriteRepository.UpdateAsync(item, cancellationToken);
                await _periodicEnergyStatementReadWriteRepository.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        var systemConfig = await _systemConfigReadOnlyRepository
            .GetListSystemConfigAsync(cancellationToken);

        var day = item.Day == 1
            ? DateTime.DaysInMonth(
                item.Year,
                item.Month == 1 ? 12 : item.Month - 1
            )
            : item.Day - 1;
        var month = item.Day == 1
            ? (item.Month == 1 ? 12 : item.Month - 1)
            : item.Month;
        var year = item.Day == 1 && item.Month == 1
            ? item.Year - 1
            : item.Year;

        var careFullname = systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.CustomerCareFullname)?.Value ?? "Phòng Cơ Điện VSIP Hải Phòng";
        var careEmail = systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.CustomerCareEmail)?.Value ?? "noreply.hoadondientu.hp@vsip.com.vn";
        var carePhone = systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.CustomerCarePhone)?.Value ?? "0225 3959868";

        // Chuẩn bị biến Template động (Merge Tags)
        var fromDateStr = item.PreviousDay.HasValue ? $"{item.PreviousDay:00}/{item.PreviousMonth:00}/{item.PreviousYear}" : $"01/{month:00}/{year}";
        var toDateStr = $"{day:00}/{month:00}/{year}";

        var templateContext = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["CustomerName"] = item.MeasurementPoint?.Customer?.FullName ?? string.Empty,
            ["PointCode"] = item.MeasurementPoint?.Code ?? string.Empty,
            ["SerialNumber"] = item.SerialNumber,
            ["Address"] = item.MeasurementPoint?.Address ?? item.MeasurementPoint?.Customer?.Address ?? string.Empty,
            ["FromDate"] = fromDateStr,
            ["ToDate"] = toDateStr,
            ["Month"] = month.ToString(),
            ["Year"] = year.ToString(),
            ["Period"] = $"Kỳ {item.PeriodNumber} Tháng {month}/{year}",
            ["PreviousBt"] = $"{item.PreviousBt:N2}",
            ["PreviousCd"] = $"{item.PreviousCd:N2}",
            ["PreviousTd"] = $"{item.PreviousTd:N2}",
            ["PreviousSg"] = $"{item.PreviousSg:N2}",
            ["Bt"] = $"{item.Bt:N2}",
            ["Cd"] = $"{item.Cd:N2}",
            ["Td"] = $"{item.Td:N2}",
            ["Sg"] = $"{item.Sg:N2}",
            ["ConsumtionBt"] = $"{item.ConsumtionBt:N2}",
            ["ConsumtionCd"] = $"{item.ConsumtionCd:N2}",
            ["ConsumtionTd"] = $"{item.ConsumtionTd:N2}",
            ["ConsumtionSg"] = $"{item.ConsumtionSg:N2}",
            ["ConsumtionTotal"] = $"{item.ConsumtionSg:N2}",
            ["ConsumtionVc"] = $"{item.ConsumtionVc:N2}",
            ["Pmax"] = $"{item.Pmax:N2} kW",
            ["PmaxTime"] = item.PmaxTime.HasValue ? item.PmaxTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "---",
            ["CareFullname"] = careFullname,
            ["CareEmail"] = careEmail,
            ["CarePhone"] = carePhone
        };

        // Render Email Subject và HTML Body thông qua NotificationTemplateService
        var (renderedSubject, renderedBodyHtml) = await _notificationTemplateService.RenderNotificationAsync(
            Constants.NotificationTemplateCode.StatementNotification,
            templateContext,
            cancellationToken);

        // Sinh file PDF phiếu báo thông qua StatementPdfGenerator (QuestPDF)
        byte[] pdfBytes;
        try
        {
            pdfBytes = await _statementPdfGenerator.GeneratePdfAsync(item, null, cancellationToken);
        }
        catch (Exception pdfEx)
        {
            _loggerService.LogError(pdfEx, "Lỗi khi render PDF phiếu báo cho StatementId = {StatementId}", statementId);
            throw;
        }

        var newResults = new Dictionary<string, StatementEmailRecipientResult>(StringComparer.OrdinalIgnoreCase);
        var activeProvider = await _emailProviderFactory.GetActiveProviderAsync(cancellationToken);

        // 3. Tiến hành gửi bù cho từng email chưa hoàn tất
        foreach (var email in emailsToSend)
        {
            if (!IsValidEmail(email))
            {
                newResults[email] = new StatementEmailRecipientResult
                {
                    Email = email,
                    Status = "FAILED",
                    Error = "Địa chỉ email không đúng định dạng cú pháp"
                };
                _loggerService.LogWarning("Email '{Email}' của điểm đo {PointCode} không đúng định dạng.", email, item.MeasurementPoint?.Code);
                continue;
            }

            try
            {
                var emailMessage = new EmailMessageDto
                {
                    To = new List<string> { email.Trim() },
                    Subject = renderedSubject,
                    BodyHtml = renderedBodyHtml,
                    Attachments = new List<EmailAttachmentDto>
                    {
                        new EmailAttachmentDto($"PhieuBaoSo_{item.SerialNumber}.pdf", "application/pdf", pdfBytes)
                    }
                };

                await activeProvider.SendEmailAsync(emailMessage, cancellationToken);

                newResults[email] = new StatementEmailRecipientResult
                {
                    Email = email,
                    Status = "SUCCESS",
                    Error = null
                };
                _loggerService.LogInformation("Gửi email phiếu báo thành công tới {Email} (StatementId = {StatementId}) qua Provider {Provider}",
                    email, statementId, activeProvider.ProviderType);
            }
            catch (Exception ex)
            {
                newResults[email] = new StatementEmailRecipientResult
                {
                    Email = email,
                    Status = "FAILED",
                    Error = ex.Message
                };
                _loggerService.LogError(ex, "Lỗi khi gửi email tới {Email} (StatementId = {StatementId}): {Message}", email, statementId, ex.Message);
            }
        }

        // 4. Tổng hợp chi tiết kết quả cho toàn bộ recipients
        var finalDetails = new List<StatementEmailRecipientResult>();
        foreach (var r in recipients)
        {
            if (newResults.TryGetValue(r, out var nr))
            {
                finalDetails.Add(nr);
            }
            else if (alreadySuccessfulMap.TryGetValue(r, out var ar))
            {
                finalDetails.Add(ar);
            }
            else
            {
                finalDetails.Add(new StatementEmailRecipientResult
                {
                    Email = r,
                    Status = "PENDING"
                });
            }
        }

        var totalCount = recipients.Count;
        var successCount = finalDetails.Count(d => d.Status == "SUCCESS");

        string overallStatus;
        if (successCount == totalCount)
        {
            overallStatus = "SEND_SUCCESS";
        }
        else if (successCount > 0)
        {
            overallStatus = "PARTIAL_SUCCESS";
        }
        else
        {
            overallStatus = "SEND_FAILED";
        }

        var statusDetail = new StatementEmailStatusDetail
        {
            Overall = overallStatus,
            SentTime = _dateTimeService.UtcNow(),
            Details = finalDetails
        };

        item.Status = JsonSerializer.Serialize(statusDetail);
        item.SentTime = statusDetail.SentTime;
        await _periodicEnergyStatementReadWriteRepository.UpdateAsync(item, cancellationToken);
        await _periodicEnergyStatementReadWriteRepository.SaveChangesAsync(cancellationToken);

        _loggerService.LogInformation("Hoàn tất xử lý gửi phiếu báo StatementId = {StatementId}: Trạng thái={Overall}, Thành công={Success}/{Total}",
            statementId, overallStatus, successCount, totalCount);
    }

    public async Task SendEmailsInBackground(List<long> statementIds, int monthGcs, int yearGcs, int periodNumber, long gcsId)
    {
        try
        {
            foreach (var id in statementIds)
            {
                try
                {
                    await ProcessSingleStatementEmailAsync(id, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _loggerService.LogError(ex, "Lỗi khi xử lý email cho StatementId {Id}", id);
                }
            }

            var newGcs = await _gcsCutOffCycleReadWriteRepository.GetGcsCutOffCycleEntityAsync(gcsId, monthGcs, yearGcs, periodNumber, CancellationToken.None);
            if (newGcs != null)
            {
                newGcs.IsSendingMail = false;
                await _gcsCutOffCycleReadWriteRepository.UpdateAsync(newGcs, CancellationToken.None);
                await _gcsCutOffCycleReadWriteRepository.SaveChangesAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(SendEmailsInBackground));
        }
    }
}