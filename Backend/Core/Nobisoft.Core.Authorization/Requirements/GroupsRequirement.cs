using System;
using Microsoft.AspNetCore.Authorization;

namespace Nobisoft.Core.Authorization.Requirements
{
    public class GroupsRequirement : IAuthorizationRequirement, IIdentifiable
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="identifier"></param>
        public GroupsRequirement(string groups, Guid identifier)
        {
            Groups = groups;
            Identifier = identifier;
        }

        /// <summary>
        /// Group
        /// </summary>
        public string Groups { get; }

        /// <summary>
        /// Role GUID
        /// </summary>
        public Guid Identifier { get; set; }
    }
}