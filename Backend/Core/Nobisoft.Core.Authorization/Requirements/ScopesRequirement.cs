using System;
using Microsoft.AspNetCore.Authorization;

namespace Nobisoft.Core.Authorization.Requirements
{
    public class ScopesRequirement : IAuthorizationRequirement, IIdentifiable
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="scopes"></param>
        /// <param name="identifier"></param>
        public ScopesRequirement(string scopes, Guid identifier)
        {
            Scopes = scopes;
            Identifier = identifier;
        }

        /// <summary>
        /// Scope
        /// </summary>
        public string Scopes { get; }

        /// <summary>
        /// Scope GUID
        /// </summary>
        public Guid Identifier { get; set; }
    }
}