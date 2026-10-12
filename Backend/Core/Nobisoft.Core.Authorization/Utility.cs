using Nobisoft.Core.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
// ReSharper disable All

namespace Nobisoft.Core.Authorization
{
    /// <summary>
    /// Utility class for [Permission] attribute
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Mark an authorization request as success
        /// </summary>
        /// <param name="context"></param>
        /// <param name="identifier"></param>
        public static void Succeed(AuthorizationHandlerContext context, Guid identifier)
        {
            var groupedRequirements = context.Requirements.Where(r => (r as IIdentifiable)?.Identifier == identifier);

            foreach (var requirement in groupedRequirements)
            {
                context.Succeed(requirement);
            }
        }
    }
}