using Ofizzy.Api.Authentication;

namespace Ofizzy.UnitTests;

public sealed class SetupRequestValidatorTests
{
    private readonly SetupRequestValidator _validator = new();

    [Fact]
    public void AcceptsValidInitialSetup()
    {
        // Arrange
        var request = new SetupRequest(
            CompanyName: "Ofizzy",
            Cnpj: null,
            Phone: null,
            AdminName: "Administrador",
            Email: "admin@ofizzy.local",
            Password: "Oficina2026");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("12345678000190")]
    [InlineData("12.345.678/0001-90")]
    public void AcceptsCnpjWithOrWithoutFormatting(string cnpj)
    {
        // Arrange
        var request = new SetupRequest(
            CompanyName: "Ofizzy",
            Cnpj: cnpj,
            Phone: null,
            AdminName: "Administrador",
            Email: "admin@ofizzy.local",
            Password: "Oficina2026");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("12.345.678/0001-9")]
    [InlineData("123456780001900")]
    public void RejectsCnpjWithoutFourteenDigits(string cnpj)
    {
        // Arrange
        var request = new SetupRequest(
            CompanyName: "Ofizzy",
            Cnpj: cnpj,
            Phone: null,
            AdminName: "Administrador",
            Email: "admin@ofizzy.local",
            Password: "Oficina2026");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SetupRequest.Cnpj));
    }

    [Theory]
    [InlineData("curta1A")]
    [InlineData("semm maiuscula1")]
    [InlineData("SEMMINUSCULA1")]
    [InlineData("SemNumeroAqui")]
    public void RejectsWeakPassword(string password)
    {
        // Arrange
        var request = new SetupRequest(
            CompanyName: "Ofizzy",
            Cnpj: null,
            Phone: null,
            AdminName: "Administrador",
            Email: "admin@ofizzy.local",
            Password: password);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }
}
