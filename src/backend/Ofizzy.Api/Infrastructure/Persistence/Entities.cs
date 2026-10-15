namespace Ofizzy.Api.Infrastructure.Persistence;

public sealed class TenantSettings : Ofizzy.Api.Modules.Tenancy.ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Timezone { get; set; } = "America/Sao_Paulo";
    public string Currency { get; set; } = "BRL";
    public long LastWorkOrderNumber { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Cnpj { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? LogoPath { get; set; }
    public string? WarrantyTerms { get; set; }
    public string? ReceiptNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class User
{
    public bool IsPlatformAdmin { get; set; }
    public DateTimeOffset? PlatformAdminGrantedAt { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}

public sealed class RefreshToken
{
    public Guid? TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
}
