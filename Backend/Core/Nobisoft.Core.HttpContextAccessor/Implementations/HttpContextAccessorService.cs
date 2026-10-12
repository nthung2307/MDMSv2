using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nobisoft.Core.Domain.ValueObjects;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.HttpContextAccessor.Implementations;

public class HttpContextAccessorService: IHttpContextAccessorService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IHttpContextAccessor HttpContextAccessor()
    {
        return _httpContextAccessor;
    }

    public string? GetRequestHeader(string key)
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;
        return !_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(key) ? null : _httpContextAccessor.HttpContext?.Request.Headers[key];
    }

    public void SetResponseHeader(string key, StringValues value)
    {
        if (_httpContextAccessor.HttpContext != null) _httpContextAccessor.HttpContext.Response.Headers[key] = value;
    }

    public Tracker Tracker()
    {
        var tracker = new Tracker
        {
            RequestId =GetRequestHeader(Constants.Tracker.Header.XRequestId) ?? string.Empty
        };
        return tracker;
    }
}