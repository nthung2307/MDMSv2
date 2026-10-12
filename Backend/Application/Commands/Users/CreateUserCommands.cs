using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Users;

public class CreateUserCommands : CommandBase, IRequest<RequestResult<CreateUserResponse>>
{
    public required string UserName { get; set; }
    public required string RoleName { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string? CustomerCode { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
}