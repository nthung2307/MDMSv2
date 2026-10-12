using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Authorization.Requirements;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Authorization.Handlers
{
    /// <summary>
    /// Groups Authorization Handler
    /// </summary>
    public class GroupsAuthorizationHandler : AuthorizationHandler<GroupsRequirement>, ISingletonDependency
    {
        private readonly ILogger<GroupsAuthorizationHandler> _logger;

        /// <inheritdoc />
        public GroupsAuthorizationHandler(ILogger<GroupsAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupsRequirement requirement)
        {
            try
            {
                if (context.User.Identity is {IsAuthenticated: false})
                {
                    return Task.CompletedTask;
                }

                if (context.HasSucceeded)
                {
                    return Task.CompletedTask;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (string.IsNullOrWhiteSpace(requirement.Groups))
                {
                    return Task.CompletedTask;
                }

                var requirementTokens = requirement.Groups.Split("|", StringSplitOptions.RemoveEmptyEntries);

                // ReSharper disable once ConstantConditionalAccessQualifier
                if (requirementTokens.Any() != true)
                {
                    return Task.CompletedTask;
                }

                var expectedRequirements = requirementTokens.ToList();

                if (expectedRequirements.Count == 0)
                {
                    return Task.CompletedTask;
                }

                // ReSharper disable once ConstantConditionalAccessQualifier
                var userRoleClaims = context.User.Claims.Where(c =>
                    string.Equals(c.Type, IdentityTypes.Group, StringComparison.OrdinalIgnoreCase));

                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ConstantNullCoalescingCondition
                foreach (var claim in userRoleClaims)
                {
                    var match = expectedRequirements
                        .Where(r => string.Equals(r, claim.Value, StringComparison.OrdinalIgnoreCase));

                    // ReSharper disable once InvertIf
                    if (match.Any())
                    {
                        Utility.Succeed(context, requirement.Identifier);
                        break;
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Group HandleRequirementAsync");
                throw;
            }
        }
    }
}