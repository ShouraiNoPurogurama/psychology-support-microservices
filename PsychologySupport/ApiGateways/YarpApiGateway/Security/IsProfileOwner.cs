using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace YarpApiGateway.Security;

public class IsProfileOwner : IAuthorizationRequirement
{
}

public class IsProfileOwnerHandler : AuthorizationHandler<IsProfileOwner>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsProfileOwner requirement)
    {
        if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "User"))
        {
            var userId = context.User.FindFirstValue("userId");
            var profileId = context.User.FindFirstValue("profileId");

            // Check if the userId matches the profileId
            if (userId != null && profileId != null && userId == profileId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}