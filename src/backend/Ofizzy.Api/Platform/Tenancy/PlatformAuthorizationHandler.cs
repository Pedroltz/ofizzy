using Microsoft.AspNetCore.Authorization;
namespace Ofizzy.Api.Modules.Tenancy;
public sealed class PlatformAdminRequirement : IAuthorizationRequirement;
public sealed class PlatformAuthorizationHandler(CurrentTenant current) : AuthorizationHandler<PlatformAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformAdminRequirement requirement)
    {
        if (current.IsPlatformAdmin && current.UserId.HasValue) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
