using Ofizzy.Api.Modules.Company;

namespace Ofizzy.UnitTests;

public class CompanyValidatorsTests
{
    private readonly UpdateCompanyRequestValidator _validator = new();

    [Fact]
    public void Valid_Company_Request_Passes()
    {
        // Arrange
        var request = new UpdateCompanyRequest(
            Name: "Ofizzy",
            LegalName: "Ofizzy LTDA",
            Cnpj: "12.345.678/0001-90",
            Phone: "(11) 99999-9999",
            WhatsApp: "(11) 99999-9999",
            Email: "contato@ofizzy.com",
            Address: "Rua A, 123",
            City: "São Paulo",
            State: "SP",
            PostalCode: "01001-000",
            WarrantyTerms: "Garantia de 90 dias",
            ReceiptNotes: "Obrigado pela preferência");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_Name_Fails()
    {
        // Arrange
        var request = new UpdateCompanyRequest(
            Name: string.Empty,
            LegalName: null,
            Cnpj: null,
            Phone: null,
            WhatsApp: null,
            Email: null,
            Address: null,
            City: null,
            State: null,
            PostalCode: null,
            WarrantyTerms: null,
            ReceiptNotes: null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCompanyRequest.Name));
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        // Arrange
        var request = new UpdateCompanyRequest(
            Name: "Ofizzy",
            LegalName: null,
            Cnpj: null,
            Phone: null,
            WhatsApp: null,
            Email: "invalid-email",
            Address: null,
            City: null,
            State: null,
            PostalCode: null,
            WarrantyTerms: null,
            ReceiptNotes: null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCompanyRequest.Email));
    }
}
