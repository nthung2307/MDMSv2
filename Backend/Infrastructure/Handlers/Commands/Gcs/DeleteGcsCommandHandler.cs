using Application.Commands.Gcs;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Handlers.Commands.Gcs;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Gcs;

public class DeleteGcsCommandHandler : IDeleteGcsCommandHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;

    public DeleteGcsCommandHandler(ICurrentUserService currentUserService, ILoggerService loggerService, IGcsReadWriteRepository gcsReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _loggerService = loggerService;
        _gcsReadWriteRepository = gcsReadWriteRepository;
    }

    public async Task<RequestResult<GcsResponse>> Handle(DeleteGcsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var gcs = await _gcsReadWriteRepository.GetGcsByIdAsync(command.Id, cancellationToken);
            if (gcs == null)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy sổ ghi chốt."
                    }
                });
            }

            if (gcs.Status == EntityStatus.Active)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động hoặc còn liên kết với điểm đo."
                    }
                });
            }

            if (gcs.MeasurementPoints.Count != 0 && gcs.MeasurementPoints.Any(mp => mp.Status != EntityStatus.Deleted))
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động hoặc còn liên kết với điểm đo."
                    }
                });
            }

            gcs.Deleted = true;
            gcs.DeletedBy = currentId;
            gcs.DeletedTime = DateTimeOffset.Now;
            gcs.DeletedName = userName;
            await _gcsReadWriteRepository.UpdateAsync(gcs, cancellationToken);

            // Save all changes
            await _gcsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}