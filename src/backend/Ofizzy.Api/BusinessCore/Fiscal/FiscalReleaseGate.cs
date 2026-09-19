using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalReleaseGate
{
    public static bool DevToolsAvailable(IWebHostEnvironment env, IConfiguration config, FiscalEnvironment environment)
    {
        return env.IsDevelopment()
            && config.GetValue<bool>("Fiscal:SimulateGateway")
            && environment == FiscalEnvironment.Homologation;
    }

    public static bool IsAllowed(
        IConfiguration config,
        Guid? tenantId,
        FiscalEnvironment environment,
        bool isTenantReleased = false)
    {
        if (environment != FiscalEnvironment.Production)
        {
            return true;
        }

        if (isTenantReleased)
        {
            return true;
        }

        var allowed = config.GetSection("Fiscal:HomologatedTenants").Get<string[]>() ?? [];
        if (allowed.Length == 0)
        {
            return config.GetValue<bool>("Fiscal:ProductionEnabled");
        }

        return config.GetValue<bool>("Fiscal:ProductionEnabled")
            && tenantId.HasValue
            && allowed.Contains(tenantId.Value.ToString());
    }

    public static void EnsureAllowed(
        IConfiguration config,
        Guid? tenantId,
        FiscalEnvironment environment,
        bool isTenantReleased = false)
    {
        if (!IsAllowed(config, tenantId, environment, isTenantReleased))
        {
            throw new ConflictException("Produção ainda não homologada para esta organização.");
        }
    }

    public static async Task<bool> IsTenantAllowedAsync(
        ApplicationDbContext db,
        IConfiguration config,
        FiscalEnvironment environment,
        CancellationToken ct = default)
    {
        if (environment != FiscalEnvironment.Production)
        {
            return true;
        }

        if (db.TenantId is not { } tid)
        {
            return false;
        }

        var isReleased = await db.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tid)
            .Select(t => t.FiscalProductionReleased)
            .FirstOrDefaultAsync(ct);

        return IsAllowed(config, tid, environment, isReleased);
    }

    public static async Task EnsureAllowedAsync(
        ApplicationDbContext db,
        IConfiguration config,
        FiscalEnvironment environment,
        CancellationToken ct = default)
    {
        var isAllowed = await IsTenantAllowedAsync(db, config, environment, ct);
        if (!isAllowed)
        {
            throw new ConflictException("Produção ainda não homologada para esta organização.");
        }
    }
}
