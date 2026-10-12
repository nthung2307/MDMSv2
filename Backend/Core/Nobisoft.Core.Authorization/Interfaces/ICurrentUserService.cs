using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Authorization.Interfaces;

public interface ICurrentUserService : IScopedDependency
{
    long UserId { get; }
    string UserName { get; }
    string GetJwtToken();
}