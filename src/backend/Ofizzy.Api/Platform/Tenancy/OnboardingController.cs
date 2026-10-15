using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Tenancy;

[Authorize]
[ApiController]
[Route("api/tenant/onboarding")]
[TenantAccess(Admin = true, AllowPending = true)]
public sealed class OnboardingController(
    ApplicationDbContext db,
    CurrentTenant current) : ControllerBase
{
    [HttpPost("complete")]
    public async Task<IActionResult> Complete(CancellationToken ct)
    {
        var settings = await db.TenantSettings.SingleAsync(ct);
        if (string.IsNullOrWhiteSpace(settings.Name))
        {
            return Problem(statusCode: 400, title: "Preencha os dados da empresa.");
        }

        var tenant = await db.Tenants.SingleAsync(x => x.Id == current.TenantId, ct);
        if (tenant.Status is not (TenantStatus.Pending or TenantStatus.Active))
        {
            return Forbid();
        }

        tenant.OnboardingCompletedAt ??= DateTimeOffset.UtcNow;
        tenant.Status = TenantStatus.Active;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedByUserId = current.UserId;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
