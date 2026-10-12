using System.Net;
using Application.Common;
using Application.Common.Responses;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nobisoft.Core.AspNetCore.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.ErrorModels;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Only work if request or action is not within graphql context
/// </summary>
public class ExceptionKillerBuilder : IExceptionKillerBuilder
{
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public ExceptionKillerBuilder(IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }

    public Task HandleExceptionAsync(bool includeDetailResponse, Exception ex)
    {
        var context = _httpContextAccessorService.HttpContextAccessor().HttpContext;
        if (context == null)
            return null;
        _loggerService.LogInformation(JsonConvert.SerializeObject(ex));
        var statusCode = GetResponseStatusCode(ex);
        context.Response.StatusCode = (int) statusCode;
        context.Response.ContentType = HttpContentType.Application.Json;

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            context.Response.Headers.Add("Access-Control-Allow-Origin", (StringValues) "*");
        var jsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
            }
        };
        if (includeDetailResponse)
        {
            // Try to get errors from ex.Data["Errors"] if any
            var errors = ex.Data["Errors"];
            var errs = new List<ErrorItem>();
            if (errors == null)
            {
                errs.Add(new ErrorItem()
                {
                    Error = ex.Message,
                    FieldName = LocalizationString.Common.UnknownFieldName
                });
                return context.Response.WriteAsync(JsonConvert.SerializeObject(RequestResult<CommonResponse>.Fail(errs), jsonSettings));
            }

            errs.AddRange(((ICollection<ErrorModel>) errors).Select(error => new ErrorItem() {Error = error.Error, FieldName = error.FieldName}));

            var text = JsonConvert.SerializeObject(RequestResult<CommonResponse>.Fail(errs), jsonSettings);
            return context.Response.WriteAsync(text);
        }

        // Clear the sensitive data before sending back to the client
        ex.Data.Clear();
        //var text1 = JsonConvert.SerializeObject(HttpResponseMessage.Fail(statusCode, ex), jsonSettings);
        var text1 = JsonConvert.SerializeObject(RequestResult<CommonResponse>.Fail(new[]
        {
            new ErrorItem()
            {
                Error = LocalizationString.Common.DataValidationError,
                FieldName = LocalizationString.Common.UnknownFieldName
            }
        }), jsonSettings);

        return context.Response.WriteAsync(text1);
    }


    public HttpStatusCode GetResponseStatusCode(Exception ex)
    {
        try
        {
            // TODO: Should double check this item
            var result = 500;
            var type = ex.GetType();

            if (type.IsAssignableFrom(System.Type.GetType("Nobisoft.Core.Exceptions.UnauthorizedException, Nobisoft.Core")))
                return HttpStatusCode.Unauthorized;

            // if ((type.IsAssignableFrom(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")) || type.IsSubclassOf(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")??typeof(Exception)))
            //     && int.TryParse(Type.GetType("Nobisoft.Core.Infrastructure.Exceptions.BusinessException, Nobisoft.Core.Infrastructure")?.GetProperty("Code")?.GetValue(ex, null).ToString(), out result))
            //     return (HttpStatusCode) result;
            if (type.IsAssignableFrom(System.Type.GetType("Nobisoft.Core.Exceptions.BadRequestException, Nobisoft.Core")) || type.IsSubclassOf(System.Type.GetType("Nobisoft.Core.Exceptions.BadRequestException, Nobisoft.Core")))
                return HttpStatusCode.BadRequest;
            if (type.IsAssignableFrom(System.Type.GetType("Nobisoft.Core.Exceptions.SystemException, Nobisoft.Core")) || type.IsSubclassOf(System.Type.GetType("Nobisoft.Core.Exceptions.SystemException, Nobisoft.Core")))
                return HttpStatusCode.InternalServerError;
            if (!type.IsAssignableFrom(typeof(ArgumentNullException)) && !type.IsSubclassOf(typeof(ArgumentNullException)))
                return (HttpStatusCode) result;
            return HttpStatusCode.BadRequest;
        }
        catch (Exception e)
        {
            e.Data["Context"] = "General";
            e.Data["Key"] = nameof(GetResponseStatusCode);
            _loggerService.LogError(e, nameof(GetResponseStatusCode));
            return HttpStatusCode.InternalServerError;
        }
    }
}