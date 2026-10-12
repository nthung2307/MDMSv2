using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Customers;
using Application.Handlers.Commands.Customers;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Customers;

public class UpdateCustomerCommandHandler : IUpdateCustomerCommandHandler
{
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserCustomerReadWriteRepository _userCustomerReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;

    public UpdateCustomerCommandHandler(ICurrentUserService currentUserService, ICustomerReadWriteRepository customerReadWriteRepository, ILoggerService loggerService, IUserReadOnlyRepository userReadOnlyRepository, IUserCustomerReadWriteRepository userCustomerReadWriteRepository, ISnowflakeIdService snowflakeIdService, IUserReadWriteRepository userReadWriteRepository, ICustomerReadOnlyRepository customerReadOnlyRepository, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _customerReadWriteRepository = customerReadWriteRepository;
        _loggerService = loggerService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _userCustomerReadWriteRepository = userCustomerReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _userReadWriteRepository = userReadWriteRepository;
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
    }

    public async Task<RequestResult<UpdateCustomerResponse>> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _customerReadWriteRepository.GetCustomerByCustomerIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<UpdateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy người dùng."
                    }
                });
            }

            var existedCustomerCode = await _customerReadOnlyRepository.IsExistedCustomerCode(command.CustomerCode, profile.Id, cancellationToken);
            if (existedCustomerCode)
            {
                return RequestResult<UpdateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.CustomerCode),
                        Error = "Mã khách hàng đã tồn tại trong hệ thống. Vui lòng thử lại!"
                    }
                });
            }

            var existUsers = await _userReadOnlyRepository.IsExistUsersAsync(command.UserIds.ToList(), cancellationToken);
            if (!existUsers)
            {
                return RequestResult<UpdateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.UserIds),
                        Error = "Không tìm thấy tài khoản."
                    }
                });
            }

            if (profile.Status != command.Status && command.Status == EntityStatus.Deactivate)
            {
                if (profile.MeasurePoints.Any(x => x.Status == EntityStatus.Active))
                {
                    return RequestResult<UpdateCustomerResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Id),
                            Error = "Đang có điểm đo hoạt động."
                        }
                    });
                }
            }

            if (command.Status == EntityStatus.Deactivate)
            {
                var userCustomer= await _userCustomerReadWriteRepository.GetUserCustomerByCustomerIdAsync(command.Id, cancellationToken);
                if (userCustomer.Count > 0)
                {
                    var userIds = userCustomer.Select(uc => uc.UserId).ToList();
                    var users = await _userReadWriteRepository.GetUsersByUserIdsAsync(userIds, cancellationToken);
                    if (users.Count > 0)
                    {
                        foreach (var user in users)
                        {
                            user.Status = EntityStatus.Deactivate;
                        }

                        await _userReadWriteRepository.UpdateRangeAsync(users, cancellationToken);
                    }
                }
            }

            if (command.MeasurementPointCustomers.Count > 0)
            {
                var measurementPointIds = command.MeasurementPointCustomers.Select(mpc => mpc.MeasurementPointId).ToList();
                var measurementPoints = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdsAsync(measurementPointIds, command.Id, cancellationToken);
                if (measurementPoints.Count > 0)
                {
                    foreach (var measurementPoint in measurementPoints)
                    {
                        var measurementPointCustomer = command.MeasurementPointCustomers.FirstOrDefault(mpc => mpc.MeasurementPointId == measurementPoint.Id);
                        if (measurementPointCustomer != null)
                        {
                            var emails = new List<string?> { measurementPointCustomer.Email, measurementPointCustomer.Email2, measurementPointCustomer.Email3 }
                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                .ToList();

                            if (emails.Count != emails.Distinct().Count())
                            {
                                return RequestResult<UpdateCustomerResponse>.Fail(new[]
                                {
                                    new ErrorItem()
                                    {
                                        FieldName = "Emails",
                                        Error = "Email đã tồn tại trong hệ thống. Vui lòng thử lại!."
                                    }
                                });
                            }

                            measurementPoint.Email = measurementPointCustomer.Email;
                            measurementPoint.Email2 = measurementPointCustomer.Email2;
                            measurementPoint.Email3 = measurementPointCustomer.Email3;
                        }
                    }

                    await _measurementPointsReadWriteRepository.UpdateRangeAsync(measurementPoints, cancellationToken);
                }
            }

            profile.FullName = command.FullName;
            profile.Email = command.Email;
            profile.PhoneNumber = command.PhoneNumber;
            profile.Address = command.Address;
            profile.CustomerCode = command.CustomerCode;
            profile.Status = command.Status;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;

            var existingUserCustomerRelations = await _userCustomerReadWriteRepository.GetUserCustomerByCustomerIdAsync(profile.Id, cancellationToken);
            var existingUserIds = existingUserCustomerRelations.Select(uc => uc.UserId).ToList();

            var userIdsToAdd = command.UserIds.Except(existingUserIds).ToList();
            var userIdsToRemove = existingUserIds.Except(command.UserIds).ToList();

            foreach (var userId in userIdsToAdd)
            {
                var userCustomer = new UserCustomerEntity()
                {
                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                    UserId = userId,
                    CustomerId = profile.Id,
                };
                await _userCustomerReadWriteRepository.AddAsync(userCustomer, cancellationToken);
            }

            foreach (var userId in userIdsToRemove)
            {
                var userCustomerToRemove = existingUserCustomerRelations.FirstOrDefault(uc => uc.UserId == userId);
                if (userCustomerToRemove != null)
                {
                    await _userCustomerReadWriteRepository.RemoveAsync(userCustomerToRemove, cancellationToken);
                }
            }

            await _customerReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _customerReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _userCustomerReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<UpdateCustomerResponse>.Succeed(new UpdateCustomerResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}