using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Lifetimes;
using HttpResponseMessage = Nobisoft.Core.AspNetCore.Http.HttpResponseMessage;

namespace Nobisoft.Core.AspNetCore.Features
{
    /// <summary>
    /// To handle error if a feature, function is disabled
    /// </summary>
    public class DisabledFeaturesHandler : IDisabledFeaturesHandler, IScopedDependency
    {
        /// <inheritdoc />
        public async Task HandleDisabledFeatures(IEnumerable<string> features, ActionExecutingContext context)
        {
            context.HttpContext.Response.StatusCode = 503;
            context.HttpContext.Response.ContentType = HttpContentType.Application.Json;
            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(HttpResponseMessage.Fail(
                HttpStatusCode.Forbidden, "This function is disabled or not existed.")));
        }
    }
}