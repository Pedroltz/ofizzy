using FluentValidation;
using Ofizzy.Api.Shared.Validation;
using Ofizzy.Api.Modules.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Company;

[ApiController]
[Route("api/company")]
[Authorize]
[TenantAccess(AllowPending = true)]
[Route("api/tenant/settings")]
public sealed class CompanyController(ApplicationDbContext db, IValidator<UpdateCompanyRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanyResponse>> Get(CancellationToken cancellationToken)
    {
        var company = await db.TenantSettings.FirstOrDefaultAsync(cancellationToken);
        if (company is null) return NotFound();

        return Ok(ToResponse(company));
    }

    [HttpPut, TenantAccess(Admin = true, AllowPending = true)]
    public async Task<ActionResult<CompanyResponse>> Update([FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var company = await db.TenantSettings.SingleAsync(cancellationToken);

        company.Name = request.Name.Trim();
        company.LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim();
        company.Cnpj = string.IsNullOrWhiteSpace(request.Cnpj) ? null : request.Cnpj.Trim();
        company.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        company.WhatsApp = string.IsNullOrWhiteSpace(request.WhatsApp) ? null : request.WhatsApp.Trim();
        company.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        company.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        company.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        company.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim().ToUpperInvariant();
        company.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
        company.WarrantyTerms = string.IsNullOrWhiteSpace(request.WarrantyTerms) ? null : request.WarrantyTerms.Trim();
        company.ReceiptNotes = string.IsNullOrWhiteSpace(request.ReceiptNotes) ? null : request.ReceiptNotes.Trim();
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(company));
    }

    private static CompanyResponse ToResponse(Infrastructure.Persistence.TenantSettings c) => new(
        c.Id,
        c.Name,
        c.LegalName,
        c.Cnpj,
        c.Phone,
        c.WhatsApp,
        c.Email,
        c.Address,
        c.City,
        c.State,
        c.PostalCode,
        c.WarrantyTerms,
        c.ReceiptNotes,
        c.UpdatedAt);
}
