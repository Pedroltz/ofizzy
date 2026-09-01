using SportPneus.Api.Authentication;

namespace SportPneus.UnitTests;

public sealed class SetupRequestValidatorTests
{
    private readonly SetupRequestValidator _validator = new();

    [Fact]
    public void AcceptsValidInitialSetup()
    {
        var result = _validator.Validate(new SetupRequest("Sport Pneus", null, null, "Administrador", "admin@sportpneus.local", "Oficina2026"));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("curta1A")]
    [InlineData("semm maiuscula1")]
    [InlineData("SEMMINUSCULA1")]
    [InlineData("SemNumeroAqui")]
    public void RejectsWeakPassword(string password)
    {
        var result = _validator.Validate(new SetupRequest("Sport Pneus", null, null, "Administrador", "admin@sportpneus.local", password));
        Assert.False(result.IsValid);
    }
}
