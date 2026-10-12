using Application.Commands.Customers;
using Application.Commands.Users;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommands>();
        CreateMap<UpdateUserRequest, UpdateUserCommand>();
        CreateMap<UserEntity, UserDto>();
        CreateMap<UserEntity, CustomerDto>();
    }
}