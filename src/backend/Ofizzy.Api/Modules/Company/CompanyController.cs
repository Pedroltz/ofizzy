using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Company;

[ApiController]
[Route("api/company")]
[Authorize]
public sealed class CompanyController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanyResponse>> Get(CancellationToken cancellationToken)
    {
        var company = await db.Companies.FirstOrDefaultAsync(cancellationToken);
        if (company is null)
        {
            company = new Infrastructure.Persistence.Company
            {
                Name = "Ofizzy",
                LegalName = "Ofizzy Gestão de Oficinas LTDA",
                Phone = "(11) 99999-9999",
                WhatsApp = "(11) 99999-9999",
                Email = "contato@ofizzy.local",
                Address = "Rua das Oficinas, 100",
                City = "São Paulo",
                State = "SP",
                PostalCode = "01001-000",
                WarrantyTerms = "Garantia legal de 90 dias para os serviços prestados e peças aplicadas, conforme artigo 26 do Código de Defesa do Consumidor.",
                ReceiptNotes = "Agradecemos pela preferência! Mantenha suas revisões preventivas em dia.",
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Ok(ToResponse(company));
    }

    [HttpPut]
    public async Task<ActionResult<CompanyResponse>> Update([FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await db.Companies.FirstOrDefaultAsync(cancellationToken);
        if (company is null)
        {
            company = new Infrastructure.Persistence.Company();
            db.Companies.Add(company);
        }

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

    private static CompanyResponse ToResponse(Infrastructure.Persistence.Company c) => new(
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
