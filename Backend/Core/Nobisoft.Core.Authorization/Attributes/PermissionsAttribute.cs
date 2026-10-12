using System;
using Microsoft.AspNetCore.Authorization;

namespace Nobisoft.Core.Authorization.Attributes
{
    /// <summary>
    /// Any user must be authenticated to reach out any protected resource.
    /// The context may be challenged against a set of service based permissions, roles or scopes.
    /// Permissions, roles or scopes may be defined as wild-cards.
    /// The context may be challenged with no expectations which means no permissions, roles or scopes at all or any are expected.
    /// All requirements should be combined as an OR relation. That is, if any of the requirements are satisfied, then the authorization should succeed.
    /// </summary>
    public class PermissionsAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Key for perm
        /// </summary>
        public const string PermissionsGroup = "permissions";

        /// <summary>
        /// Key for role
        /// </summary>
        public const string RolesGroup = "roles";

        private string[] _permissions;
        private string[] _roles;

        private bool _isDefault = true;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///
        /// Sample
        /// 
        ///     [Permissions(
        ///                 Permissions = new[] { "weather:*:*"},
        ///                 Roles = new [] { "user", "super admin"},
        ///                 Scopes = new[] { "App.Demo" },
        ///                 Groups = new[] { "App.Demo" })
        ///     ]
        /// 
        /// Note:
        ///
        ///     If we leave it as blank ([Permissions]), it work like [Authorize] attribute
        /// </remarks>
        public PermissionsAttribute()
        {
            _permissions = Array.Empty<string>();
            _roles = Array.Empty<string>();
        }

        /// <summary>
        /// Array of permissions to be evaluated by authorizer
        /// </summary>
        public string[] Permissions
        {
            get => _permissions;
            set => BuildPolicy(ref _permissions, value, PermissionsGroup);
        }

        /// <summary>
        /// Array of roles
        /// </summary>
        public new string[] Roles
        {
            get => _roles;
            set => BuildPolicy(ref _roles, value, RolesGroup);
        }

        /// <summary>
        /// To build list of policy
        /// </summary>
        /// <param name="target">Permission, Role or Policy</param>
        /// <param name="value">Come from constructor</param>
        /// <param name="group">Permission, Role or Policy</param>
        // ReSharper disable once RedundantAssignment
        private void BuildPolicy(ref string[] target, string[] value, string group)
        {
            target = value;

            if (_isDefault)
            {
                Policy = string.Empty;
                _isDefault = false;
            }

            Policy += $"{group}${string.Join("|", target)};";
        }
    }
}