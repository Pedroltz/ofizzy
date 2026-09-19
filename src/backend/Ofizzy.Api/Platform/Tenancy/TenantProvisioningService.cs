using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Tenancy;

public sealed record ProvisionTenantRequest(
    string Name,
    string Slug,
    BusinessVertical Vertical,
    string AdminName,
    string Email,
    string? Password,
    ProductModule[]? Modules = null);

public sealed class ProvisionTenantValidator : AbstractValidator<ProvisionTenantRequest>
{
    public ProvisionTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");

        RuleFor(x => x.Vertical)
            .IsInEnum();

        RuleFor(x => x.AdminName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.Modules)
            .Must(x => x is null || (x.Distinct().Count() == x.Length && x.All(Enum.IsDefined)))
            .WithMessage("Módulos inválidos.");

        RuleFor(x => x.Password)
            .MinimumLength(10)
            .MaximumLength(200)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .When(x => x.Password is not null);
    }
}

public static class AutomotiveTenantTemplate
{
    public static readonly ProductModule[] Modules =
    [
        ProductModule.Customers,
        ProductModule.WorkOrders,
        ProductModule.Catalog,
        ProductModule.Automotive
    ];

    public static bool ValidModules(IEnumerable<ProductModule> modules)
    {
        var set = modules.ToHashSet();
        var allDefined = set.All(Enum.IsDefined);
        var automotiveRequiresCustomers = !set.Contains(ProductModule.Automotive) || set.Contains(ProductModule.Customers);
        var workOrdersRequireAll = !set.Contains(ProductModule.WorkOrders) || Modules.All(set.Contains);

        return allDefined && automotiveRequiresCustomers && workOrdersRequireAll;
    }
}

public sealed class TenantProvisioningService(
    ApplicationDbContext db,
    CurrentTenant current,
    IPasswordHasher<User> hasher,
    IValidator<ProvisionTenantRequest> validator)
{
    public async Task<Tenant> ProvisionAsync(ProvisionTenantRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var modules = request.Modules ?? AutomotiveTenantTemplate.Modules;
        if (!AutomotiveTenantTemplate.ValidModules(modules))
        {
            throw new ConflictException("Ordens automotivas exigem Clientes, Catálogo e Automotive.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        if (await db.Tenants.AnyAsync(x => x.Slug == request.Slug, ct))
        {
            throw new ConflictException("Este identificador de organização já está em uso.");
        }

        var email = request.Email.Trim().ToUpperInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email, ct);

        if (user is null)
        {
            if (request.Password is null)
            {
                throw new ConflictException("Informe uma senha inicial para disponibilizar o acesso ao novo usuário.");
            }

            user = new User
            {
                Name = request.AdminName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                NormalizedEmail = email
            };

            user.PasswordHash = hasher.HashPassword(user, request.Password);
            db.Users.Add(user);
        }
        else if (!user.IsActive || request.Password is not null)
        {
            throw new ConflictException("Para associar um usuário ativo existente, deixe a senha vazia. Sua senha não será alterada.");
        }

        var tenant = new Tenant
        {
            Name = request.Name.Trim(),
            Slug = request.Slug,
            Vertical = request.Vertical,
            CreatedByUserId = current.UserId,
            UpdatedByUserId = current.UserId
        };

        tenant.Modules = modules
            .Select(x => new TenantModule
            {
                TenantId = tenant.Id,
                Module = x
            })
            .ToList();

        tenant.Settings = new TenantSettings
        {
            TenantId = tenant.Id,
            Name = tenant.Name
        };

        db.Tenants.Add(tenant);
        db.TenantUsers.Add(new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            Role = TenantRole.Owner
        });

        var previous = current.TenantId;
        try
        {
            // Trusted internal scope for the new tenant only, never a query-filter bypass.
            current.TenantId = tenant.Id;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        finally
        {
            current.TenantId = previous;
        }

        return tenant;
    }
}
