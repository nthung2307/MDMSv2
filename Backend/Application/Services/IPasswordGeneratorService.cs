using Microsoft.AspNetCore.Identity;
using Nobisoft.Core.Lifetimes;

namespace Application.Services;

public interface IPasswordGeneratorService : IScopedDependency
{
    public string HashPassword(string password);
    public bool VerifyHashPassword(string? hashedPassword, string password);
    public string GenerateRandomPassword(PasswordOptions opts = null!);

}