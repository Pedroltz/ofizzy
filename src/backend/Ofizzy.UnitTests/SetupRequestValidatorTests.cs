using Ofizzy.Api.Authentication;

namespace Ofizzy.UnitTests;

public sealed class SetupRequestValidatorTests
{
    private readonly SetupRequestValidator _validator = new();

    [Fact]
    public void AcceptsValidInitialSetup()
    {
        var result = _validator.Validate(new SetupRequest("Ofizzy", null, null, "Administrador", "admin@ofizzy.local", "Oficina2026"));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("12345678000190")]
    [InlineData("12.345.678/0001-90")]
    public void AcceptsCnpjWithOrWithoutFormatting(string cnpj)
    {
        var result = _validator.Validate(new SetupRequest("Ofizzy", cnpj, null, "Administrador", "admin@ofizzy.local", "Oficina2026"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("12.345.678/0001-9")]
    [InlineData("123456780001900")]
    public void RejectsCnpjWithoutFourteenDigits(string cnpj)
    {
        var result = _validator.Validate(new SetupRequest("Ofizzy", cnpj, null, "Administrador", "admin@ofizzy.local", "Oficina2026"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SetupRequest.Cnpj));
    }

    [Theory]
    [InlineData("curta1A")]
    [InlineData("semm maiuscula1")]
    [InlineData("SEMMINUSCULA1")]
    [InlineData("SemNumeroAqui")]
    public void RejectsWeakPassword(string password)
    {
        var result = _validator.Validate(new SetupRequest("Ofizzy", null, null, "Administrador", "admin@ofizzy.local", password));
        Assert.False(result.IsValid);
    }
}
