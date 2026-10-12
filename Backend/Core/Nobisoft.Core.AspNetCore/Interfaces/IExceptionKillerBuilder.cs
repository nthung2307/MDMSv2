using System.Net;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.AspNetCore.Interfaces;

public interface IExceptionKillerBuilder: ISingletonDependency
{
    Task HandleExceptionAsync(bool includeDetailResponse, Exception ex);
    HttpStatusCode GetResponseStatusCode(Exception ex);
}