using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Ofizzy.Api.Modules.Fiscal;
using Ofizzy.Api.Shared;

namespace Ofizzy.UnitTests;

public sealed class FiscalRtcTests
{
    [Fact]
    public void Simples_transition_does_not_silently_continue_with_the_old_flow()
    {
        Assert.False(FiscalRtc.RequiresConfiguration("SimplesNacional", new DateOnly(2026, 12, 31)));
        Assert.True(FiscalRtc.RequiresConfiguration("SimplesNacional", new DateOnly(2027, 1, 1)));
        Assert.False(FiscalRtc.RequiresConfiguration("MEI", new DateOnly(2027, 1, 1)));
    }

    [Theory]
    [InlineData("12.ABC.345/01de-35", "12ABC34501DE35")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData("12ABC34501DE?35", "12ABC34501DE?35")]
    public void Document_normalization_preserves_letters_and_invalid_symbols(string input, string expected)
    {
        Assert.Equal(expected, TextNormalization.Document(input));
    }

    [Fact]
    public void Alphanumeric_identification_is_validated_end_to_end_in_customer_contract()
    {
        Assert.True(FiscalValidation.IsDocument("12ABC34501DE35"));
        Assert.False(FiscalValidation.IsDocument("12ABC34501DE36"));
        var validator = new Ofizzy.Api.Modules.Customers.CustomerRequestValidator();
        var request = new Ofizzy.Api.Modules.Customers.CustomerRequest("Cliente Teste", "12.ABC.345/01de-35", null, null, null, null, null);
        Assert.True(validator.Validate(request).IsValid);
        Assert.False(validator.Validate(request with { Document = "12ABC34501DE36" }).IsValid);
    }

    [Fact]
    public void Drafts_preserve_incomplete_configuration_but_never_calculate_it()
    {
        var draft = new RtcProfile(true, "200", null, null, null, null, null);
        Assert.Empty(FiscalRtc.DraftIssues(draft));
        Assert.NotEmpty(FiscalRtc.EmissionIssues(draft, false));
        Assert.Throws<InvalidOperationException>(() => FiscalRtc.Calculate(draft, 100));
    }

    [Theory]
    [InlineData(100, 0.10, 0.00, 0.90)]
    [InlineData(12.50, 0.01, 0.00, 0.11)]
    public void Rates_are_explicit_and_calculated_in_backend(decimal value, decimal uf, decimal municipal, decimal cbs)
    {
        var calculated = FiscalRtc.Calculate(new(true, "000", "000001", 100, .1m, 0m, .9m), value);
        Assert.Equal(value, calculated.Base);
        Assert.Equal(uf, calculated.IbsUf);
        Assert.Equal(municipal, calculated.IbsMunicipal);
        Assert.Equal(cbs, calculated.Cbs);
        Assert.Equal(uf + municipal, calculated.Ibs);
    }

    [Fact]
    public void Rtc_auxiliary_pdf_is_not_advertised_before_layout_is_supported()
    {
        var document = new FiscalDocument { AuthorizedXml = "<NFe><infNFe><IBSCBS /></infNFe></NFe>" };
        var response = FiscalPreparationService.Map(document);
        Assert.True(response.CanDownload);
        Assert.False(response.CanDownloadPdf);
        document.AuthorizedXml = "<NFe><infNFe /></NFe>";
        Assert.True(FiscalPreparationService.Map(document).CanDownloadPdf);
    }

    [Theory]
    [InlineData(FiscalKind.Nfe)]
    [InlineData(FiscalKind.Nfse)]
    public void Rtc_fixture_is_signed_and_validates_in_official_schema(FiscalKind kind)
    {
        var snapshot = FiscalTests.Snapshot(kind);
        var line = snapshot.Lines[0];
        var profile = new RtcProfile(true, "000", "000001", 100, .1m, 0m, .9m, "100001");
        line = kind == FiscalKind.Nfe
            ? line with { Product = line.Product! with { RtcEnabled = true, RtcCst = "000", RtcClassTrib = "000001", RtcBasePercent = 100,
                RtcIbsUfRate = .1m, RtcIbsMunicipalRate = 0, RtcCbsRate = .9m }, Rtc = FiscalRtc.Calculate(profile, 100) }
            : line with { Service = line.Service! with { RtcEnabled = true, RtcCst = "000", RtcClassTrib = "000001", RtcBasePercent = 100,
                RtcIbsUfRate = .1m, RtcIbsMunicipalRate = 0, RtcCbsRate = .9m, RtcOperationCode = "100001" }, Rtc = FiscalRtc.Calculate(profile, 100) };
        snapshot = snapshot with { Lines = [line] };
        var doc = new FiscalDocument { Kind = kind, Environment = FiscalEnvironment.Homologation, Series = 1, Number = 1,
            Total = 100, SchemaPackage = FiscalSchemaCatalog.Document(kind).Package };
        doc.Identity = kind == FiscalKind.Nfe ? FiscalXml.NfeKey(snapshot, 1, 1, 12345678)
            : $"DPS{snapshot.Issuer.Address!.CityCode}2{snapshot.Issuer.Cnpj}00001000000000000001";
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=RTC Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var xml = kind == FiscalKind.Nfe ? FiscalXml.Invoice(doc, snapshot) : FiscalXml.Dps(doc, snapshot);
        var signed = FiscalXml.Sign(xml, kind == FiscalKind.Nfe ? "infNFe" : "infDPS", certificate);
        FiscalXml.Validate(signed, kind);
        Assert.Contains("cClassTrib>000001", signed);
        if (kind == FiscalKind.Nfe) Assert.Contains("vCBS>0.90", signed);
        Assert.Contains("rtc", FiscalJson.Write(snapshot).ToLowerInvariant());
    }
}
