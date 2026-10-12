using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Nobisoft.Core.Extensions;
using Nobisoft.Core.Logger.Interfaces;
using Nobisoft.Core.ValueObjects;
using SystemException = Nobisoft.Core.Exceptions.SystemException;

namespace Nobisoft.Core.AspNetCore.Middleware;

/// <summary>
/// Tracking request Id, execution time
/// This middle will extract X-Request-Id from the header.
/// If X-Request-Id is presented in the header, just forward it
/// If X-Request-Id is not presented in the header, generate a new one
/// X-Request-ID will be resent back to the client
/// </summary>
public class HeaderTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerService _loggerService;

    public HeaderTrackingMiddleware(RequestDelegate next, ILoggerService loggerService)
    {
        _next = next;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Invoke the middleware function
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {// Log raw header
        var headersStr = context.Request.Headers.Keys.Aggregate("", (current, key) => current + $"→ {key}: {(key.ToLower().Equals("authorization") ? context.Request.Headers[key].ToString().Masking() : context.Request.Headers[key])}\n");
        var preGeneratedRequestId = Guid.NewGuid().ToString();
        // Check the request before move to the next function in the pipeline
        var hasXRequestId = context.Request.Headers.ContainsKey(Constants.Tracker.Header.XRequestId);
        if (!hasXRequestId)
        {
            context.Request.Headers[Constants.Tracker.Header.XRequestId] = preGeneratedRequestId;
        }

        _loggerService.LogInformation($"\n✅ ---- BEGIN REQUEST ---- \n📌 Raw header of {context.Request.Method.ToUpper()} {context.Response.StatusCode} {context.Request.GetDisplayUrl()}: \n{headersStr}\n❓ Need system to generates a default request id: {(!hasXRequestId ? "OK" : "NO")}");
        try
        {
            
            // Start the stop watch
            var sw = new Stopwatch();
            sw.Start();
            // Observe the response event
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext) state;
                // Write the response back to client
                httpContext.Response.Headers.Add(Constants.Tracker.Header.XRequestId, context.Request.Headers[Constants.Tracker.Header.XRequestId]);
                httpContext.Response.Headers.Add(Constants.Tracker.Header.XTraceIdentifier, context.TraceIdentifier);
                sw.Stop();
                httpContext.Response.Headers.Add(Constants.Tracker.Header.XExecutionTime, $"+{sw.ElapsedMilliseconds.ToString()} ms");
                _loggerService.LogInformation(
                    $"{httpContext.Request.Method.ToUpper()} {httpContext.Response.StatusCode} {httpContext.Request.GetDisplayUrl()} with request id {context.Request.Headers[Constants.Tracker.Header.XRequestId]} (+{sw.ElapsedMilliseconds} ms) \n ↪️ ---- END REQUEST ----\n");
                return Task.CompletedTask;
            }, context);
            // Call the next function in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Handle exception if any
            // var e= new SystemException(ex.GetType().Name, ex.Message, new Tracker(preGeneratedRequestId));
            // e.Data.Add("OriginalExceptionName",ex.GetType().FullName);
            // throw e;
            throw;
        }
    }
}