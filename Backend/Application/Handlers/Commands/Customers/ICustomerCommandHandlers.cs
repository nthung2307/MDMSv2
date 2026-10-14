using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using MediatR;

namespace Application.Handlers.Commands.Customers;

public interface ICreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommands, RequestResult<CreateCustomerResponse>>
{
}

public interface IUpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, RequestResult<UpdateCustomerResponse>>
{
}

public interface IDeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, RequestResult<DeleteCustomerResponse>>
{
}

public interface IChangeStatusCustomerCommandHandler : IRequestHandler<ChangeStatusCustomerCommand, RequestResult<UpdateCustomerResponse>>
{
}

public interface IImportCustomersCommandHandler : IRequestHandler<ImportCustomersCommand, RequestResult<ImportCustomerResponse>>
{
}
