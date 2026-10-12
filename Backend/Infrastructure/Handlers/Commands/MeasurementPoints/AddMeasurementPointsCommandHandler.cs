using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Handlers.Commands.MeasurementPoints;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.MeasurementPoints;

public class AddMeasurementPointsCommandHandler : ICreateMeasurementPointsCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;

    public AddMeasurementPointsCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
    }

    public async Task<RequestResult<MeasurementPointResponse>> Handle(CreateMeasurementPointsCommands request, CancellationToken cancellationToken)
    {
        try
        {
            if(request.Request.CustomerId == 0)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.CustomerId),
                        Error = "Thiếu thông tin người dùng."
                    }
                });
            }
            if(request.Request.GcsId == 0)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.CustomerId),
                        Error = "Thiếu thông tin điểm đo."
                    }
                });
            }

            var isExistCustomerCode = await _measurementPointsReadOnlyRepository.IsExistMeasurementPointNameAsync(request.Request.Code, 0, cancellationToken);
            if (isExistCustomerCode)
            {
                return RequestResult<MeasurementPointResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Code),
                        Error = "Mã điểm đo đã tồn tại trong hệ thống. Vui lòng thử lại!"
                    }
                });
            }

            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var measurementPointId = await _snowflakeIdService.GenerateId(cancellationToken);

            var response = new MeasurementPointsEntity()
            {
                Id = measurementPointId,
                Code = request.Request.Code,
                Type = request.Request.Type,
                Address = request.Request.Address,
                Description = request.Request.Description,
                CustomerId = request.Request.CustomerId,
                GcsId = request.Request.GcsId,
                Status = request.Request.Status,
                MultiplierFactor = request.Request.MultiplierFactor,
                CreatedBy = currentId,
                CreatedName = userName,
                CreatedTime = DateTimeOffset.UtcNow,
                ModifiedBy = currentId,
                ModifiedName = userName,
                ModifiedTime = DateTimeOffset.UtcNow
            };

            await _measurementPointsReadWriteRepository.AddAsync(response, cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<MeasurementPointResponse>.Succeed(new MeasurementPointResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}