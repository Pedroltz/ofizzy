using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Modules.Fiscal;

namespace Ofizzy.UnitTests;

public sealed class FiscalDevSimulationTests
{
    private static X509Certificate2 CreateTestCertificate(string cnpj = "11222333000181")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN=Oficina Teste Dev {cnpj}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        var writer = new AsnWriter(AsnEncodingRules.DER);
        var tag = new Asn1Tag(TagClass.ContextSpecific, 0, true);

        writer.PushSequence();
        writer.PushSequence(tag);
        writer.WriteObjectIdentifier("2.16.76.1.3.3");
        writer.PushSequence(tag);
        writer.WriteCharacterString(UniversalTagNumber.UTF8String, cnpj);
        writer.PopSequence(tag);
        writer.PopSequence(tag);
        writer.PopSequence();

        request.CertificateExtensions.Add(new X509Extension("2.5.29.17", writer.Encode(), false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
    }

    [Fact]
    public void DevCertificate_Matches_Configured_Cnpj()
    {
        // Arrange
        using var cert = CreateTestCertificate("11222333000181");

        // Act & Assert
        Assert.True(FiscalCertificateVault.MatchesCnpj(cert, "11222333000181"));
        Assert.False(FiscalCertificateVault.MatchesCnpj(cert, "99999999000199"));
    }

    [Fact]
    public async Task DevSimulatedGateway_Authorizes_Nfe_And_Enables_Danfe_Pdf_Generation()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var snapshot = FiscalTests.Snapshot(FiscalKind.Nfe);

        var doc = new FiscalDocument
        {
            Kind = FiscalKind.Nfe,
            Series = 1,
            Number = 1001,
            Environment = FiscalEnvironment.Homologation,
            Snapshot = FiscalJson.Write(snapshot),
            Total = 100m
        };
        doc.Identity = FiscalXml.NfeKey(snapshot, doc.Series, doc.Number, 12345678);
        doc.SubmittedXml = FiscalXml.Sign(FiscalXml.Invoice(doc, snapshot), "infNFe", cert);

        var gateway = new DevSimulatedFiscalGateway();

        // Act
        var result = await gateway.Send(doc, cert, CancellationToken.None);

        // Assert
        Assert.Equal(FiscalState.Authorized, result.State);
        Assert.NotNull(result.AuthorizedXml);
        Assert.Contains("nfeProc", result.AuthorizedXml);
        Assert.Contains("protNFe", result.AuthorizedXml);
        Assert.Equal("135260000000001", result.Protocol);

        doc.AuthorizedXml = result.AuthorizedXml;
        doc.Protocol = result.Protocol;
        doc.State = FiscalState.Authorized;

        var pdf = FiscalPdf.Generate(doc);
        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
    }

    [Fact]
    public async Task DevSimulatedGateway_Authorizes_Nfse_And_Enables_Danfse_Pdf_Generation()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var snapshot = FiscalTests.Snapshot(FiscalKind.Nfse);

        var doc = new FiscalDocument
        {
            Kind = FiscalKind.Nfse,
            Series = 1,
            Number = 2001,
            Environment = FiscalEnvironment.Homologation,
            Snapshot = FiscalJson.Write(snapshot),
            Total = 100m,
            Identity = "DPS355030821122233300018100001000000000002001"
        };
        doc.SubmittedXml = FiscalXml.Sign(FiscalXml.Dps(doc, snapshot), "infDPS", cert);

        var gateway = new DevSimulatedFiscalGateway();

        // Act
        var result = await gateway.Send(doc, cert, CancellationToken.None);

        // Assert
        Assert.Equal(FiscalState.Authorized, result.State);
        Assert.NotNull(result.AuthorizedXml);
        Assert.Equal("135260000000001", result.Protocol);

        doc.AuthorizedXml = result.AuthorizedXml;
        doc.Protocol = result.Protocol;
        doc.State = FiscalState.Authorized;

        var pdf = FiscalPdf.Generate(doc);
        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000);
    }

    [Theory]
    [InlineData(FiscalKind.Nfe)]
    [InlineData(FiscalKind.Nfse)]
    public async Task Simulated_documents_preserve_identity_and_support_validated_cancellation(FiscalKind kind)
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var doc = await FiscalPdfFixtures.Authorized(kind);
        var gateway = new DevSimulatedFiscalGateway();

        // Act
        var again = await gateway.Send(doc, cert, CancellationToken.None);
        var queried = await gateway.Query(doc, cert, CancellationToken.None);

        // Assert
        Assert.Equal(doc.AccessKey, again.Key);
        Assert.Equal(doc.AuthorizedXml, queried.AuthorizedXml);

        if (kind == FiscalKind.Nfse)
        {
            Assert.Matches("^[0-9]{50}$", doc.AccessKey!);
            FiscalXml.ValidateAuthorizedNfse(doc.AuthorizedXml!);
            var original = FiscalXml.Parse(doc.SubmittedXml!);
            var nested = FiscalXml.Parse(doc.AuthorizedXml!).Descendants(FiscalXml.Nfse + "DPS").Single();
            Assert.True(XNode.DeepEquals(original.Root, nested));
        }

        var cancelXml = FiscalEmissionService.CancellationXml(doc, "Cancelamento fictício para teste local", cert);
        var cancelResult = await gateway.Cancel(doc, cancelXml, cert, CancellationToken.None);
        Assert.Equal(FiscalState.Cancelled, cancelResult.State);
    }

    [Fact]
    public async Task Simulator_refuses_every_production_operation()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var doc = new FiscalDocument { Environment = FiscalEnvironment.Production };
        var gateway = new DevSimulatedFiscalGateway();
        var s = FiscalTests.Snapshot(FiscalKind.Nfe);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => gateway.Send(doc, cert, default));
        await Assert.ThrowsAsync<ConflictException>(() => gateway.Query(doc, cert, default));
        await Assert.ThrowsAsync<ConflictException>(() => gateway.Cancel(doc, string.Empty, cert, default));

        var prodSnapshot = s with
        {
            Issuer = s.Issuer with { Environment = FiscalEnvironment.Production }
        };
        await Assert.ThrowsAsync<ConflictException>(() => gateway.Inutilize(prodSnapshot, string.Empty, cert, default));
    }
}
