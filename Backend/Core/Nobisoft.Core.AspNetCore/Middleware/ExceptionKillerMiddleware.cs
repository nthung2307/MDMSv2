using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nobisoft.Core.AspNetCore.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using HttpResponseMessage = Nobisoft.Core.AspNetCore.Http.HttpResponseMessage;
namespace Nobisoft.Core.AspNetCore.Middleware
{
    /// <summary>
    /// Handle all the exceptions in the system
    /// </summary>
    public class ExceptionKillerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _includeDetailResponse;
        private readonly IExceptionKillerBuilder _exceptionKillerBuilder;

        public ExceptionKillerMiddleware(RequestDelegate next, IExceptionKillerBuilder exceptionKillerBuilder, bool includeDetailResponse = false)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _exceptionKillerBuilder = exceptionKillerBuilder;
            _includeDetailResponse = includeDetailResponse;
        }

        /// <summary>
        /// Invoke the middleware function
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Move next
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle exception if any
                await _exceptionKillerBuilder.HandleExceptionAsync(_includeDetailResponse, ex);
                //await HandleExceptionAsync(context, ex);
            }
        }

        // private Task HandleExceptionAsync(HttpContext context, Exception ex)
        // {
        //     // Get logger service
        //     var loggerService = context.RequestServices.GetRequiredService<ILoggerService>();
        //     loggerService.LogInformation(JsonConvert.SerializeObject(ex));
        //     var statusCode = GetResponseStatusCode(loggerService, ex);
        //     context.Response.StatusCode = (int) statusCode;
        //     context.Response.ContentType = HttpContentType.Application.Json;
        //
        //     if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        //         context.Response.Headers.Add("Access-Control-Allow-Origin", (StringValues) "*");
        //     if (!_includeDetailResponse)
        //     {
        //         var text = JsonConvert.SerializeObject(ex);
        //         return context.Response.WriteAsync(text);
        //     }
        //
        //     var text1 = JsonConvert.SerializeObject(HttpResponseMessage.Fail(statusCode, ex));
        //     return context.Response.WriteAsync(text1);
        // }
        //
        // private HttpStatusCode GetResponseStatusCode(ILoggerService loggerService, Exception ex)
        // {
        //     try
        //     {
        //         // TODO: Should double check this item
        //         var result = 500;
        //         var type = ex.GetType();
        //         if ((type.IsAssignableFrom(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")))
        //             && int.TryParse(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")?.GetProperty("Code")?.GetValue(ex, null).ToString(), out result))
        //             return (HttpStatusCode) result;
        //         if (type.IsAssignableFrom(Type.GetType("Nobisoft.Core.AspNetCore.Exceptions.BadRequestException, Nobisoft.Core.AspNetCore")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.AspNetCore.Exceptions.BadRequestException, Nobisoft.Core.AspNetCore")))
        //             return HttpStatusCode.BadRequest;
        //         if (type.IsAssignableFrom(Type.GetType("Nobisoft.Core.Exceptions.SystemException, Nobisoft.Core")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.Exceptions.SystemException, Nobisoft.Core")))
        //             return HttpStatusCode.InternalServerError;
        //         if (type.IsAssignableFrom(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.EntityNotFoundException, Nobisoft.Core.Infrastructure")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.EntityNotFoundException, Nobisoft.Core.Infrastructure")))
        //             return HttpStatusCode.NotFound;
        //         if (type.IsAssignableFrom(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.EntityAlreadyExistsException, Nobisoft.Core.Infrastructure")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.EntityAlreadyExistsException, Nobisoft.Core.Infrastructure")))
        //             return HttpStatusCode.Found;
        //         if (!type.IsAssignableFrom(typeof(ArgumentNullException)) && !type.IsSubclassOf(typeof(ArgumentNullException)))
        //             return (HttpStatusCode) result;
        //         return HttpStatusCode.BadRequest;
        //     }
        //     catch (Exception e)
        //     {
        //         e.Data["Context"] = "General";
        //         e.Data["Key"] = nameof(GetResponseStatusCode);
        //         loggerService.LogError(e, nameof(GetResponseStatusCode));
        //         return HttpStatusCode.InternalServerError;
        //     }
        // }
    }
}