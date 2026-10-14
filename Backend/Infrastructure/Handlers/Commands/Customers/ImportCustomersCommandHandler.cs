using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Customers;
using Application.Handlers.Commands.Customers;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Customers;

public class ImportCustomersCommandHandler : IImportCustomersCommandHandler
{
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;

    public ImportCustomersCommandHandler(
        ICustomerReadWriteRepository customerReadWriteRepository,
        ICurrentUserService currentUserService,
        ILoggerService loggerService)
    {
        _customerReadWriteRepository = customerReadWriteRepository;
        _currentUserService = currentUserService;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<ImportCustomerResponse>> Handle(ImportCustomersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserName = _currentUserService.UserName ?? "System";

            var result = await _customerReadWriteRepository.ImportCustomersFromCsvAsync(
                request.FileStream,
                currentUserId,
                currentUserName,
                cancellationToken);

            return RequestResult<ImportCustomerResponse>.Succeed(result);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(ImportCustomersCommandHandler));
            return RequestResult<ImportCustomerResponse>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "file",
                    Error = $"Có lỗi xảy ra khi nhập tệp CSV: {ex.Message}"
                }
            });
        }
    }
}
