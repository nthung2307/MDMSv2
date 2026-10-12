using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Infrastructure.Interfaces;

public interface IRecaptchaService: ISingletonDependency
{
    /// <summary>
    /// To verify a client token is valid or not.
    /// This will make a call to the google service to check the submitted client token
    /// </summary>
    /// <param name="clientToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> VerifyClientTokenAsync(string clientToken, CancellationToken cancellationToken = default(CancellationToken));
}