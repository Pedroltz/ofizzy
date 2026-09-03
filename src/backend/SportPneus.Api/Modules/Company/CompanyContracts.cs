namespace SportPneus.Api.Modules.Company;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string? LegalName,
    string? Cnpj,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? City,
    string? State,
    string? PostalCode,
    string? WarrantyTerms,
    string? ReceiptNotes,
    DateTimeOffset UpdatedAt);

public sealed record UpdateCompanyRequest(
    string Name,
    string? LegalName,
    string? Cnpj,
    string? Phone,
    string? WhatsApp,
    string? Email,
    string? Address,
    string? City,
    string? State,
    string? PostalCode,
    string? WarrantyTerms,
    string? ReceiptNotes);
