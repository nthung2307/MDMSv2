using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Requests.EmailConfig;
using Domain.Entities;

namespace Application.Services;

public interface IEmailProviderFactory
{
    Task<IEmailProviderService> GetActiveProviderAsync(CancellationToken cancellationToken = default);
    IEmailProviderService CreateProviderFromTestRequest(TestEmailConfigRequest request, SystemEmailConfigEntity? existingConfig);
}
