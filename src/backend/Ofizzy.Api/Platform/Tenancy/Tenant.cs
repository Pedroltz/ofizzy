using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Tenancy;

public enum BusinessVertical { Automotive }
public enum TenantStatus { Pending, Active, Suspended, Archived }
public enum TenantRole { Owner, Admin, Member }
public enum ProductModule { Customers, WorkOrders, Catalog, Automotive }

public interface ITenantScoped { Guid TenantId { get; set; } }

public sealed class Tenant
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Pending;
    public BusinessVertical Vertical { get; set; } = BusinessVertical.Automotive;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? OnboardingCompletedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public TenantSettings Settings { get; set; } = null!;
    public List<TenantModule> Modules { get; set; } = [];
    public bool FiscalProductionReleased { get; set; }
    public DateTimeOffset? FiscalProductionReleasedAt { get; set; }
    public Guid? FiscalProductionReleasedByUserId { get; set; }
}

public sealed class TenantUser
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public TenantRole Role { get; set; } = TenantRole.Member;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TenantModule
{
    public Guid TenantId { get; set; }
    public ProductModule Module { get; set; }
    public bool Enabled { get; set; } = true;
}
