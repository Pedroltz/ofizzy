using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Tenancy;

public sealed record PlatformTenantResponse(Guid Id, string Name, string Slug, TenantStatus Status, BusinessVertical Vertical, DateTimeOffset? OnboardingCompletedAt, ProductModule[] Modules, bool FiscalProductionReleased, DateTimeOffset? FiscalProductionReleasedAt);
public sealed record UpdateTenantRequest(TenantStatus Status, ProductModule[] Modules, bool? FiscalProductionReleased = null);

[Authorize(Policy = "PlatformAdmin"), ApiController, Route("api/platform/tenants")]
public sealed class PlatformController(ApplicationDbContext db, TenantProvisioningService provisioning, CurrentTenant current) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlatformTenantResponse>>> List(CancellationToken ct) => Ok(await db.Tenants.AsNoTracking().OrderBy(x => x.Name).Select(x => new PlatformTenantResponse(x.Id, x.Name, x.Slug, x.Status, x.Vertical, x.OnboardingCompletedAt, x.Modules.Where(m => m.Enabled).Select(m => m.Module).ToArray(), x.FiscalProductionReleased, x.FiscalProductionReleasedAt)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlatformTenantResponse>> Get(Guid id, CancellationToken ct)
    {
        var tenant = await db.Tenants.AsNoTracking().Include(x => x.Modules).SingleOrDefaultAsync(x => x.Id == id, ct);
        return tenant is null ? NotFound() : Ok(Map(tenant));
    }

    [HttpPost]
    public async Task<ActionResult<PlatformTenantResponse>> Create(ProvisionTenantRequest request, CancellationToken ct)
    {
        var tenant = await provisioning.ProvisionAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { tenant.Id }, Map(tenant));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlatformTenantResponse>> Update(Guid id, UpdateTenantRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(request.Status) || request.Modules is null || !AutomotiveTenantTemplate.ValidModules(request.Modules) || request.Modules.Distinct().Count() != request.Modules.Length)
            return Problem(statusCode: 400, title: "Estado ou módulos inválidos");
        var tenant = await db.Tenants.Include(x => x.Modules).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tenant is null) return NotFound();
        if (request.Status == TenantStatus.Active && tenant.OnboardingCompletedAt is null)
            return Problem(statusCode: 409, title: "O administrador da organização precisa concluir o onboarding.");
        tenant.Status = request.Status; tenant.UpdatedAt = DateTimeOffset.UtcNow; tenant.UpdatedByUserId = current.UserId;
        if (request.FiscalProductionReleased.HasValue && request.FiscalProductionReleased.Value != tenant.FiscalProductionReleased)
        {
            tenant.FiscalProductionReleased = request.FiscalProductionReleased.Value;
            tenant.FiscalProductionReleasedAt = request.FiscalProductionReleased.Value ? DateTimeOffset.UtcNow : null;
            tenant.FiscalProductionReleasedByUserId = request.FiscalProductionReleased.Value ? current.UserId : null;
        }
        foreach (var module in Enum.GetValues<ProductModule>())
        {
            var existing = tenant.Modules.SingleOrDefault(x => x.Module == module);
            if (existing is null) tenant.Modules.Add(new TenantModule { TenantId = id, Module = module, Enabled = request.Modules.Contains(module) });
            else existing.Enabled = request.Modules.Contains(module);
        }
        await db.SaveChangesAsync(ct); return Ok(Map(tenant));
    }
    private static PlatformTenantResponse Map(Tenant x) => new(x.Id, x.Name, x.Slug, x.Status, x.Vertical, x.OnboardingCompletedAt, x.Modules.Where(m => m.Enabled).Select(m => m.Module).ToArray(), x.FiscalProductionReleased, x.FiscalProductionReleasedAt);
}
