using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class ChangeStatusMeasurementPointCommandHandler : IChangeStatusMeasurementPointsCommandHandler
{
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;

    public ChangeStatusMeasurementPointCommandHandler(ICurrentUserService currentUserService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, ILoggerService loggerService)
    {
        _currentUserService = currentUserService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(ChangeStatusMeasurementPointCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy điểm đo."
                    }
                });
            }

            profile.Status = profile.Status == EntityStatus.Active ? EntityStatus.Deactivate : EntityStatus.Active;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;
            await _measurementPointsReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<MeasurementPointResponse>.Succeed(new MeasurementPointResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}