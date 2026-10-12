using Microsoft.Extensions.Primitives;
using Nobisoft.Core.Domain.ValueObjects;
using Nobisoft.Core.Lifetimes;
using Nobisoft.Core.ValueObjects;
using MsHttp = Microsoft.AspNetCore.Http;
namespace Nobisoft.Core.HttpContextAccessor.Interfaces;

public interface IHttpContextAccessorService: ISingletonDependency
{
    /// <summary>
    /// Get Microsoft's original http context accessor
    /// </summary>
    /// <returns></returns>
    public MsHttp.IHttpContextAccessor HttpContextAccessor();
    /// <summary>
    /// Read the request header by key if any
    /// </summary>
    /// <param name="key">Header key</param>
    /// <returns>Header value if key is existed, null if not</returns>
    public string? GetRequestHeader(string key);

    /// <summary>
    /// Set the the header key with the value
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    public void SetResponseHeader(string key, StringValues value);

    public Tracker Tracker();
}