using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.Authorization.Requirements;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Authorization.PolicyProviders
{
    /// <summary>
    /// Must be singleton as MS recommended.
    /// This class consumes the policy set which provided by PermissionsAttribute
    /// </summary>
    public class PermissionsPolicyProvider : IAuthorizationPolicyProvider, ISingletonDependency
    {
        private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        public PermissionsPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // Set fallback policy provider as default policy
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// Returns the default authorization policy (the policy used for [Authorize] attributes without a policy specified).
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            // You may leave it empty to use your custom authorization with the default authentication scheme
            //return Task.FromResult(new AuthorizationPolicyBuilder("SchemeName").RequireAuthenticatedUser().Build());
            return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        }

        /// <summary>
        /// Returns the fallback authorization policy (the policy used by the Authorization Middleware when no policy is specified).
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        /// <summary>
        /// Returns an authorization policy for a given name.
        /// </summary>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            var policyTokens = policyName.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (policyTokens?.Any() != true)
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            // You may leave it empty to use your custom authorization with the default authentication scheme
            // var policy = new AuthorizationPolicyBuilder("SchemeName");
            var policy = new AuthorizationPolicyBuilder();
            var identifier = Guid.NewGuid();
            // Split and transform string policy from PermissionsAttribute to requirement then add to the policy
            foreach (var token in policyTokens)
            {
                var pair = token.Split('$', StringSplitOptions.RemoveEmptyEntries);

                if (pair?.Any() != true || pair.Length != 2)
                {
                    return FallbackPolicyProvider.GetPolicyAsync(policyName);
                }

                IAuthorizationRequirement? requirement = (pair[0]) switch
                {
                    PermissionsAttribute.PermissionsGroup => new PermissionsRequirement(pair[1], identifier),
                    PermissionsAttribute.RolesGroup => new RolesRequirement(pair[1], identifier),
                    _ => null,
                };

                // Fallback to default of requirement is null (not permission, role or scope)
                if (requirement == null)
                {
                    return FallbackPolicyProvider.GetPolicyAsync(policyName);
                }

                policy.AddRequirements(requirement);
            }

            return Task.FromResult(policy.Build())!;
        }
    }
}