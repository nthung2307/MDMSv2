using Application.Commands.Auth;
using Application.DataTransferObjects.Requests.Auth;
using AutoMapper;

namespace Infrastructure.MappingProfiles;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<SignInRequest, SignInCommands>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommands>();
        CreateMap<ConfirmForgotPasswordRequest, ConfirmForgotPasswordCommands>();
        CreateMap<ForgotPasswordRequest, ForgotPasswordCommands>();
    }
}