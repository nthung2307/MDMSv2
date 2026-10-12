using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Interfaces;

public interface ISnowflakeIdService: ISingletonDependency
{
    Task<long> GenerateId(CancellationToken cancellationToken = default);
    Task<long> TryCreateId(CancellationToken cancellationToken = default);
    long Max();
    long Min();
}