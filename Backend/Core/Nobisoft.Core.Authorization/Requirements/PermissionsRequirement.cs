using System;
using Microsoft.AspNetCore.Authorization;

namespace Nobisoft.Core.Authorization.Requirements
{
    public class PermissionsRequirement : IAuthorizationRequirement, IIdentifiable
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="permissions"></param>
        /// <param name="identifier"></param>
        public PermissionsRequirement(string permissions, Guid identifier)
        {
            Permissions = permissions;
            Identifier = identifier;
        }

        /// <summary>
        /// Store permission value
        /// </summary>
        public string Permissions { get; }
        /// <summary>
        /// Internal Permission GUID
        /// </summary>

        public Guid Identifier { get; set; }
    }
}