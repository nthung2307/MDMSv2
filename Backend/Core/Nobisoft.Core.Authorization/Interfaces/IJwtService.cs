using System.Security.Claims;
using Nobisoft.Core.Lifetimes;
using Nobisoft.Core.ValueObjects;

// ReSharper disable once CheckNamespace
namespace Nobi.Core.Authorization.Interfaces;

public interface IJwtService : IScopedDependency
{
    Task<CreateJwtResponse> GenerateJwtAsync(long accountId, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken);
}