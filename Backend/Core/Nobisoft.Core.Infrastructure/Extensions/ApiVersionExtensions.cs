using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Nobisoft.Core.Exceptions;

namespace Nobisoft.Core.Infrastructure.Extensions
{
    public static class ApiVersionExtensions
    {
        /// <summary>
        /// Configure API Version
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddApiVersionService(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true; //should be true but false for testing purpose
                config.ReportApiVersions = true;
                // Set version (with Api-Version param) location in query string like /api/Identity/Connect/Token?Api-Version=1.0
                // Or, in the request header like "API-VERSION: 1.0"
                config.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("Api-Version"));
                //new HeaderApiVersionReader("API-VERSION")); // Disable header version
                config.ErrorResponses = new ApiVersionErrorException();
            });

            services.AddVersionedApiExplorer(config =>
            {
                config.GroupNameFormat = "'v'VVV";
                config.SubstituteApiVersionInUrl = true; // To show api version param in swagger
            });
           
        }
    }
}