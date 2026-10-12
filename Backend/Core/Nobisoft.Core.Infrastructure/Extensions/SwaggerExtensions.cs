using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nobisoft.Core.Infrastructure.Swagger;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nobisoft.Core.Infrastructure.Extensions
{
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configure Swagger
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddSwaggerUiService<TStartup>(this IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions<TStartup>>();
            services.AddSwaggerGen(c =>
            {
                c.ExampleFilters();
                c.EnableAnnotations();
                c.SchemaFilter<SchemaFilter>();
                // c.DocumentFilter<DocumentOperationFilter>();
                // c.OperationFilter<DocumentOperationFilter>();
            });
            services.AddSwaggerExamplesFromAssemblyOf<TStartup>();
        }
    }
}