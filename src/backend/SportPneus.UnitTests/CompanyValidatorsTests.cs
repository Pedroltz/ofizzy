using SportPneus.Api.Modules.Company;
using Xunit;

namespace SportPneus.UnitTests;

public class CompanyValidatorsTests
{
    private readonly UpdateCompanyRequestValidator _validator = new();

    [Fact]
    public void Valid_Company_Request_Passes()
    {
        var request = new UpdateCompanyRequest(
            "Ofizzy",
            "Ofizzy LTDA",
            "12.345.678/0001-90",
            "(11) 99999-9999",
            "(11) 99999-9999",
            "contato@sportpneus.com",
            "Rua A, 123",
            "São Paulo",
            "SP",
            "01001-000",
            "Garantia de 90 dias",
            "Obrigado pela preferência");

        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_Name_Fails()
    {
        var request = new UpdateCompanyRequest(
            "",
            null, null, null, null, null, null, null, null, null, null, null);

        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCompanyRequest.Name));
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        var request = new UpdateCompanyRequest(
            "Ofizzy",
            null, null, null, null,
            "invalid-email",
            null, null, null, null, null, null);

        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCompanyRequest.Email));
    }
}
