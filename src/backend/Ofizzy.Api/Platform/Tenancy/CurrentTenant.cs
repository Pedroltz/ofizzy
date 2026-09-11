namespace Ofizzy.Api.Modules.Tenancy;

// Populated only from validated authentication/provisioning, never from request DTOs.
public sealed class CurrentTenant
{
    public Guid? UserId { get; internal set; }
    public bool IsPlatformAdmin { get; internal set; }
    public Guid? TenantId { get; internal set; }
    public TenantRole? Role { get; internal set; }
    public Tenant? Tenant { get; internal set; }
    public bool IsAdmin => Role is TenantRole.Owner or TenantRole.Admin;
    public bool HasModule(ProductModule module) => Tenant?.Modules.Any(x => x.Module == module && x.Enabled) == true;
}
