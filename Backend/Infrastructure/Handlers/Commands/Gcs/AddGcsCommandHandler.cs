using Application.Commands.Gcs;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Handlers.Commands.Gcs;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Gcs;

public class AddGcsCommandHandler : ICreateGcsCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;

    public AddGcsCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, IGcsReadWriteRepository gcsReadWriteRepository)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _gcsReadWriteRepository = gcsReadWriteRepository;
    }

    public async Task<RequestResult<GcsResponse>> Handle(CreateGcsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var isExistGcsCode = await _gcsReadWriteRepository.IsExistGcsCodeAsync(null, request.Request.Code, cancellationToken);
            if (isExistGcsCode)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Code),
                        Error = "Mã sổ đã tồn tại. Vui lòng thử lại"
                    }
                });
            }

            if (request.Request.DayPeriod.Any(x => (x <= 0 || x > 28) && x != 999))
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.DayPeriod),
                        Error = "Ngày ghi chỉ số không được lớn hơn 28"
                    }
                });
            }

            if (request.Request.DayPeriod.Count != request.Request.PeriodNumber)
            {

                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.DayPeriod),
                        Error = "Vui lòng nhập ngày chốt đúng với số ngày chốt."
                    }
                });
            }

            for (int i = 0; i < request.Request.DayPeriod.Count - 1; i++)
            {
                var has28 = request.Request.DayPeriod.Contains(28);
                var has999 = request.Request.DayPeriod.Contains(999);

                if (has28 && has999)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(request.Request.DayPeriod),
                            Error = "Không thể thêm kì chốt cuối tháng khi đã có kì chốt ngày 28."
                        }
                    });
                }

                // If 999 is present, it must be the last element
                if (request.Request.DayPeriod[i] == 999)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(request.Request.DayPeriod),
                            Error = "Ngày cuối tháng chỉ được đặt ở vị trí cuối cùng."
                        }
                    });
                }

                // Ensure days are strictly increasing (except for the last 0)
                if (request.Request.DayPeriod[i] >= request.Request.DayPeriod[i + 1] && request.Request.DayPeriod[i + 1] != 0)
                {
                    return RequestResult<GcsResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(request.Request.DayPeriod),
                            Error = "Ngày của kì sau phải lớn hơn kì trước"
                        }
                    });
                }
            }

            var response = new GcsEntity()
            {
                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                Code = request.Request.Code,
                Name = request.Request.Name,
                PeriodNumber = request.Request.PeriodNumber,
                Description = request.Request.Description,
                Status = request.Request.Status,
                DayPeriod = request.Request.DayPeriod,
                CreatedBy = currentId,
                CreatedName = userName,
                CreatedTime = DateTimeOffset.UtcNow,
                ModifiedBy = currentId,
                ModifiedName = userName,
                ModifiedTime = DateTimeOffset.UtcNow
            };
            await _gcsReadWriteRepository.AddAsync(response, cancellationToken);
            await _gcsReadWriteRepository.SaveChangesAsync(cancellationToken);

            return RequestResult<GcsResponse>.Succeed(new GcsResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}