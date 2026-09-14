using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalReleaseGate
{
    public static bool DevToolsAvailable(IWebHostEnvironment env, IConfiguration config, FiscalEnvironment environment)
        => env.IsDevelopment() && config.GetValue<bool>("Fiscal:SimulateGateway") && environment == FiscalEnvironment.Homologation;

    public static void EnsureAllowed(IConfiguration config, Guid? tenantId, FiscalEnvironment environment)
    {
        if (environment != FiscalEnvironment.Production) return;
        var allowed = config.GetSection("Fiscal:HomologatedTenants").Get<string[]>() ?? [];
        if (!config.GetValue<bool>("Fiscal:ProductionEnabled") || !allowed.Contains(tenantId.ToString()))
            throw new ConflictException("Produção ainda não homologada para esta organização.");
    }
}
