using System.Threading;
using System.Threading.Tasks;

namespace Application.Services;

public interface IEmailProviderFactory
{
    Task<IEmailProviderService> GetActiveProviderAsync(CancellationToken cancellationToken = default);
}
