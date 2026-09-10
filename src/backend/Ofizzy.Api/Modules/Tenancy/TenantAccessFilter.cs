using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ofizzy.Api.Modules.Tenancy;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantAccessAttribute : Attribute
{
    public bool Admin { get; set; }
    public bool AllowPending { get; set; }
    public ProductModule[] Modules { get; set; } = [];
}

public sealed class TenantAccessFilter(CurrentTenant current) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var requirements = context.ActionDescriptor.EndpointMetadata.OfType<TenantAccessAttribute>().ToList();
        if (requirements.Count == 0) return Task.CompletedTask;
        var denied = current.Tenant is null || requirements.Any(x =>
            (!x.AllowPending && current.Tenant.Status != TenantStatus.Active) ||
            (x.Admin && !current.IsAdmin) || x.Modules.Any(m => !current.HasModule(m)));
        if (denied) context.Result = new ObjectResult(new ProblemDetails { Status = 403, Title = "Acesso indisponível", Detail = "Selecione uma organização ativa com as permissões e módulos necessários." }) { StatusCode = 403 };
        return Task.CompletedTask;
    }
}
