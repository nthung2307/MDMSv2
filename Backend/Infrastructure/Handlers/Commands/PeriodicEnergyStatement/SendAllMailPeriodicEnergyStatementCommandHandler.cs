using Application.Commands.PeriodicEnergyStatement;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Handlers.Commands.PeriodicEnergyStatement;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Hangfire;
using Infrastructure.Services;
using Nobisoft.Core.Logger.Interfaces;
using QuestPDF.Infrastructure;

namespace Infrastructure.Handlers.Commands.PeriodicEnergyStatement;

public class SendAllMailPeriodicEnergyStatementCommandHandler : ISendAllMailCommand
{
    private readonly ILoggerService _loggerService;
    private readonly IPeriodicEnergyStatementReadWriteRepository _periodicEnergyStatementReadWriteRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;

    public SendAllMailPeriodicEnergyStatementCommandHandler(
        ILoggerService loggerService,
        IPeriodicEnergyStatementReadWriteRepository periodicEnergyStatementReadWriteRepository,
        IBackgroundJobClient backgroundJobClient, IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository, IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository)
    {
        _loggerService = loggerService;
        _periodicEnergyStatementReadWriteRepository = periodicEnergyStatementReadWriteRepository;
        _backgroundJobClient = backgroundJobClient;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<RequestResult<PeriodicEnergyStatementResponse>> Handle(SendAllMailPeriodicEnergyStatementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isSendingMail = await _gcsCutOffCycleReadOnlyRepository.IsSendingMail(new SendAllMailRequest()
            {
                Month = request.Request.Month,
                Year = request.Request.Year,
                PeriodNumber = request.Request.PeriodNumber,
                GcsId = request.Request.GcsId
            }, cancellationToken);

            if (isSendingMail)
            {
                return RequestResult<PeriodicEnergyStatementResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.GcsId),
                        Error = $"Hệ thống đang gửi mail cho người dùng, vui lòng chờ trong ít phút."
                    }
                });
            }
            
            var periodicEnergyStatement = await _periodicEnergyStatementReadWriteRepository
                .GetPeriodicEnergyStatementByGcsIdAsync(request.Request, cancellationToken);

            var statementIds = periodicEnergyStatement.Select(x => x.Id).ToList();

            // Resolve EmailSenderService and enqueue the method
            if (statementIds.Count > 0)
            {
                var gcs = await _gcsCutOffCycleReadWriteRepository.GetGcsCutOffCycleEntityAsync(request.Request.GcsId, request.Request.Month, request.Request.Year, request.Request.PeriodNumber, cancellationToken);
                if (gcs != null)
                {
                    gcs.IsSendingMail = true;
                    await _gcsCutOffCycleReadWriteRepository.UpdateAsync(gcs, cancellationToken);
                }

                await _gcsCutOffCycleReadWriteRepository.SaveChangesAsync(cancellationToken);
                _backgroundJobClient.Enqueue<EmailSenderService>(x => x.SendEmailsInBackground(statementIds, request.Request.Month, request.Request.Year, request.Request.PeriodNumber, request.Request.GcsId));
            }

            Thread.Sleep(5000);
            return RequestResult<PeriodicEnergyStatementResponse>.Succeed(new PeriodicEnergyStatementResponse()
            {
            });
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }


}
