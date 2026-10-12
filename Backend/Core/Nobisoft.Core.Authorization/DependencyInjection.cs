using Nobisoft.Core.Authorization.Handlers;
using Nobisoft.Core.Authorization.PolicyProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nobisoft.Core.Authorization
{
    public static class DependencyInjection
    {
        public static void AddCoreAuthorizationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionsPolicyProvider>();
            // services.AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ScopesAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, GroupsAuthorizationHandler>();
        }
    }
}