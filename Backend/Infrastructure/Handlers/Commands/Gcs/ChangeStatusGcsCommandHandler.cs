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

public class ChangeStatusGcsCommandHandler : IChangeStatusGcsCommandHandler
{
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;

    public ChangeStatusGcsCommandHandler(ICurrentUserService currentUserService, IGcsReadWriteRepository gcsReadWriteRepository, ILoggerService loggerService)
    {
        _currentUserService = currentUserService;
        _gcsReadWriteRepository = gcsReadWriteRepository;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<GcsResponse>> Handle(ChangeStatusGcsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _gcsReadWriteRepository.GetGcsByIdAsync(command.Id, cancellationToken);
            if (profile == null)
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

            if (EntityStatus.Active == profile.Status)
            {
                if (profile.MeasurementPoints.Count > 0 && profile.MeasurementPoints.Any(mp => mp.Status == EntityStatus.Active))
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(profile.MeasurementPoints),
                            Error = "Vui lòng inactive tất cả các điểm đo trước khi inactive sổ."
                        }
                    });
                }
            }

            profile.Status = profile.Status == EntityStatus.Active ? EntityStatus.Deactivate : EntityStatus.Active;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;
            await _gcsReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _gcsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}