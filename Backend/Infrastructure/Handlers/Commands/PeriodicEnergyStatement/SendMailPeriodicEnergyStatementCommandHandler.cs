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

public class SendMailPeriodicEnergyStatementCommandHandler : ISendMailCommand
{
    private readonly ILoggerService _loggerService;
    private readonly IPeriodicEnergyStatementReadWriteRepository _periodicEnergyStatementReadWriteRepository;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;


    public SendMailPeriodicEnergyStatementCommandHandler(ILoggerService loggerService, IPeriodicEnergyStatementReadWriteRepository periodicEnergyStatementReadWriteRepository, IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository, IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository, IBackgroundJobClient backgroundJobClient)
    {
        _loggerService = loggerService;
        _periodicEnergyStatementReadWriteRepository = periodicEnergyStatementReadWriteRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _backgroundJobClient = backgroundJobClient;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<RequestResult<PeriodicEnergyStatementResponse>> Handle(SendMailPeriodicEnergyStatementCommand request, CancellationToken cancellationToken)
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
            
            var periodicEnergyStatement = await _periodicEnergyStatementReadWriteRepository.GetPeriodicEnergyStatementByIdsAsync(request.Request.Ids, cancellationToken);

            var statementIds = periodicEnergyStatement.Select(x => x.Id).ToList();

            // Resolve EmailSenderService and enqueue discrete jobs
            if (statementIds.Count > 0)
            {
                var gcs = await _gcsCutOffCycleReadWriteRepository.GetGcsCutOffCycleEntityAsync(request.Request.GcsId, request.Request.Month, request.Request.Year, request.Request.PeriodNumber, cancellationToken);
                if (gcs != null)
                {
                    gcs.IsSendingMail = false;
                    await _gcsCutOffCycleReadWriteRepository.UpdateAsync(gcs, cancellationToken);
                    await _gcsCutOffCycleReadWriteRepository.SaveChangesAsync(cancellationToken);
                }

                foreach (var id in statementIds)
                {
                    _backgroundJobClient.Enqueue<EmailSenderService>(x => x.ProcessSingleStatementEmailAsync(id, CancellationToken.None));
                }
            }

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
