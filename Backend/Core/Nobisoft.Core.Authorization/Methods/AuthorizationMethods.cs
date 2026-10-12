using HotChocolate.Types;

namespace Nobisoft.Core.Authorization.Methods
{
    public static class AuthorizationMethods
    {
        /// <summary>
        /// Check required perms
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="perms"></param>
        /// <returns></returns>
        public static IObjectFieldDescriptor RequiredPermissions(this IObjectFieldDescriptor descriptor, string[] perms = null)
        {
            descriptor.Authorize($"Permissions${string.Join("|", perms)}");
            return descriptor;
        }

        /// <summary>
        /// Check required scopes
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static IObjectFieldDescriptor RequiredScopes(this IObjectFieldDescriptor descriptor, string[] scopes = null)
        {
            descriptor.Authorize($"Scopes${string.Join("|", scopes)}");
            return descriptor;
        }

        /// <summary>
        /// Check required roles
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static IObjectFieldDescriptor RequiredRoles(this IObjectFieldDescriptor descriptor, string[] roles = null)
        {
            descriptor.Authorize($"Roles${string.Join("|", roles)}");
            return descriptor;
        }

       
    }
}