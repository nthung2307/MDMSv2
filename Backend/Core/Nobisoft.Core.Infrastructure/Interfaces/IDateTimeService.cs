using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Infrastructure.Interfaces;

public interface IDateTimeService: IScopedDependency
{
    DateTimeOffset Now();
    DateTimeOffset UtcNow();
}