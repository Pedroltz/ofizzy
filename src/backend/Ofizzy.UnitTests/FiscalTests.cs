using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Modules.Fiscal;
using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;

namespace Ofizzy.UnitTests;

public sealed class FiscalTests
{
    [Theory]
    [InlineData(FiscalKind.Nfe)]
    [InlineData(FiscalKind.Nfse)]
    public void Cancellation_request_matches_official_schema(FiscalKind kind)
    {
        // Arrange
        using var certificate = Certificate();
        var snapshot = Snapshot(kind);
        var document = new FiscalDocument
        {
            Kind = kind,
            Environment = FiscalEnvironment.Homologation,
            Number = 1,
            Identity = FiscalXml.NfeKey(snapshot, 1, 1, 12345678),
            AccessKey = new string('1', 50),
            Protocol = "135260000000001",
            Snapshot = FiscalJson.Write(snapshot)
        };

        // Act
        var cancellationXml = FiscalEmissionService.CancellationXml(document, "Serviço não prestado ao cliente", certificate);

        // Assert
        FiscalXml.ValidateCancellation(cancellationXml, kind);
    }

    [Theory]
    [InlineData("102", true, true)]
    [InlineData("563", true, true)]
    [InlineData("563", false, false)]
    [InlineData("102", false, false)]
    public void Inutilization_confirmation_requires_matching_range_and_protocol(string code, bool matches, bool confirmed)
    {
        // Arrange
        var ns = FiscalXml.Nfe;
        var request = new XElement(ns + "inutNFe",
            new XElement(ns + "infInut",
                new XElement(ns + "tpAmb", 2),
                new XElement(ns + "cUF", 35),
                new XElement(ns + "ano", 26),
                new XElement(ns + "CNPJ", "11222333000181"),
                new XElement(ns + "mod", 55),
                new XElement(ns + "serie", 1),
                new XElement(ns + "nNFIni", 1),
                new XElement(ns + "nNFFin", 2)));

        var response = new XElement(ns + "retInutNFe", new XElement(request.Element(ns + "infInut")!));
        response.Element(ns + "infInut")!.Add(
            new XElement(ns + "cStat", code),
            new XElement(ns + "nProt", "135260000000001"));

        if (!matches)
        {
            response.Descendants(ns + "nNFFin").Single().Value = "3";
        }

        // Act
        var result = NationalFiscalGateway.ParseNfe(
            response.ToString(),
            new FiscalDocument { SubmittedXml = request.ToString() },
            "void");

        // Assert
        var expectedState = confirmed ? FiscalState.Inutilized : FiscalState.AwaitingConfirmation;
        Assert.Equal(expectedState, result.State);
    }

    public static FiscalSnapshot Snapshot(FiscalKind kind)
    {
        var issuer = new FiscalSettingsData(
            Cnpj: "11222333000181",
            LegalName: "OFICINA FICTICIA",
            StateRegistration: "110042490114",
            Regime: "SimplesNacional",
            Address: new FiscalAddress("Rua Teste", "10", "Centro", "São Paulo", "3550308", "SP", "01001000"),
            NfeEnabled: true,
            NfseEnabled: true);

        var recipient = new FiscalPreparationData(
            Address: new FiscalAddress("Rua Teste", "20", "Centro", "São Paulo", "3550308", "SP", "01001000"),
            Document: "12345678909",
            Name: "Cliente Fictício",
            Competence: new DateOnly(2026, 9, 12),
            PaymentCode: "01",
            PaymentAmount: 100);

        var productData = kind == FiscalKind.Nfe
            ? new ProductFiscalData("40111000", Origin: "0", Cfop: "5102", Csosn: "102", PisCst: "07", CofinsCst: "07")
            : null;

        var serviceData = kind == FiscalKind.Nfse
            ? new ServiceFiscalData("140101", ApproximateTaxRate: 6)
            : null;

        var lines = new List<FiscalLine>
        {
            new(Guid.NewGuid(), "P-01", "Revisão & alinhamento <teste>", 1, 100, productData, serviceData)
        };

        return new FiscalSnapshot(
            Issuer: issuer,
            Recipient: recipient,
            OrderNumber: 1,
            IssuedAt: new DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.FromHours(-3)),
            Lines: lines);
    }

    public static X509Certificate2 Certificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Certificado Ficticio", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var writer = new AsnWriter(AsnEncodingRules.DER);
        var tag = new Asn1Tag(TagClass.ContextSpecific, 0, true);

        writer.PushSequence();
        writer.PushSequence(tag);
        writer.WriteObjectIdentifier("2.16.76.1.3.3");
        writer.PushSequence(tag);
        writer.WriteCharacterString(UniversalTagNumber.UTF8String, "11222333000181");
        writer.PopSequence(tag);
        writer.PopSequence(tag);
        writer.PopSequence();

        request.CertificateExtensions.Add(new X509Extension("2.5.29.17", writer.Encode(), false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    [Theory]
    [InlineData(FiscalKind.Nfe)]
    [InlineData(FiscalKind.Nfse)]
    public void Generated_document_matches_official_schema_and_signature(FiscalKind kind)
    {
        // Arrange
        using var cert = Certificate();
        var snapshot = Snapshot(kind);
        var document = new FiscalDocument
        {
            Kind = kind,
            Series = 1,
            Number = 1,
            Environment = FiscalEnvironment.Homologation,
            Total = 100
        };

        document.Identity = kind == FiscalKind.Nfe
            ? FiscalXml.NfeKey(snapshot, 1, 1, 12345678)
            : "DPS355030821122233300018100001000000000000001";

        var xmlTree = kind == FiscalKind.Nfe
            ? FiscalXml.Invoice(document, snapshot)
            : FiscalXml.Dps(document, snapshot);
        var rootTag = kind == FiscalKind.Nfe ? "infNFe" : "infDPS";

        // Act
        var xml = FiscalXml.Sign(xmlTree, rootTag, cert);

        // Assert
        FiscalXml.Validate(xml, kind);
        Assert.Contains("&amp;", xml);
        Assert.Contains("&lt;", xml);

        var dom = new XmlDocument { PreserveWhitespace = true };
        dom.LoadXml(xml);

        var verifier = new SignedXml(dom);
        verifier.LoadXml((XmlElement)dom.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0]!);
        Assert.True(verifier.CheckSignature(cert, true));
    }

    [Fact]
    public void Vault_authenticates_tenant_key_and_payload()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Fiscal:ActiveKeyId", "v1" },
                { "Fiscal:Keys:v1", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }
            })
            .Build();

        var vault = new FiscalCertificateVault(config);
        var tenant = Guid.NewGuid();
        byte[] value = [1, 2, 3, 4];

        // Act
        var encrypted = vault.Protect(tenant, value);

        // Assert
        Assert.Equal(value, vault.Unprotect(tenant, "v1", encrypted));
        Assert.ThrowsAny<CryptographicException>(() => vault.Unprotect(Guid.NewGuid(), "v1", encrypted));

        encrypted[^1] ^= 1;
        Assert.ThrowsAny<CryptographicException>(() => vault.Unprotect(tenant, "v1", encrypted));
    }

    [Fact]
    public void Certificate_uses_icp_brasil_cnpj_attribute()
    {
        // Arrange
        using var cert = Certificate();

        // Act & Assert
        Assert.True(FiscalCertificateVault.MatchesCnpj(cert, "11222333000181"));
        Assert.False(FiscalCertificateVault.MatchesCnpj(cert, "12345678000195"));
    }

    [Theory]
    [InlineData("11222333000181", true)]
    [InlineData("12345678909", true)]
    [InlineData("11111111111", false)]
    [InlineData("12345678000190", false)]
    public void Fiscal_documents_require_valid_check_digits(string value, bool valid)
    {
        Assert.Equal(valid, FiscalValidation.IsDocument(value));
    }

    [Fact]
    public void Unknown_official_response_does_not_become_authorization()
    {
        // Arrange
        var document = new FiscalDocument { Identity = "123" };
        var rawXml = "<retConsSitNFe xmlns='http://www.portalfiscal.inf.br/nfe'><cStat>217</cStat></retConsSitNFe>";

        // Act
        var result = NationalFiscalGateway.ParseNfe(rawXml, document, "query");

        // Assert
        Assert.Equal(FiscalState.AwaitingConfirmation, result.State);
        Assert.True(result.NotFound);
    }

    [Fact]
    public void Xml_rejects_external_entities()
    {
        var hostileXml = "<!DOCTYPE test [<!ENTITY ex SYSTEM 'file:///etc/passwd'>]><test>&ex;</test>";
        Assert.Throws<XmlException>(() => FiscalXml.Parse(hostileXml));
    }

    [Fact]
    public void Mixed_emission_reports_partial_and_rounding_is_explicit()
    {
        // Arrange
        var documents = new List<FiscalDocument>
        {
            new() { State = FiscalState.Authorized },
            new() { State = FiscalState.Rejected }
        };

        // Act & Assert
        Assert.Equal("Partial", FiscalPreparationService.Status(documents, 2));
        Assert.Equal(0.67m, FiscalValidation.Money(0.333m * 2));
        Assert.Equal(0.66m, FiscalValidation.Money(0.333m) * 2);
        Assert.Equal(0.01m, FiscalValidation.Money(0.005m));
    }

    [Fact]
    public void St_profile_requires_documented_values()
    {
        // Arrange
        var validator = new ProductFiscalValidator();
        var profile = new ProductFiscalData(
            Ncm: "40111000",
            Cfop: "5405",
            Csosn: "500",
            PisCst: "04",
            CofinsCst: "04");

        // Act & Assert
        Assert.False(validator.Validate(profile).IsValid);

        var validProfile = profile with
        {
            RetainedStBase = 120,
            RetainedStAmount = 20,
            SubstituteAmount = 10,
            StRate = 18
        };
        Assert.True(validator.Validate(validProfile).IsValid);
    }

    [Fact]
    public void Production_requires_both_server_gate_and_tenant_homologation()
    {
        // Arrange
        var tenant = Guid.NewGuid();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fiscal:ProductionEnabled"] = "true",
                ["Fiscal:HomologatedTenants:0"] = tenant.ToString()
            })
            .Build();

        // Act & Assert
        FiscalReleaseGate.EnsureAllowed(config, tenant, FiscalEnvironment.Production);
        Assert.Throws<ConflictException>(() => FiscalReleaseGate.EnsureAllowed(config, Guid.NewGuid(), FiscalEnvironment.Production));

        config["Fiscal:ProductionEnabled"] = "false";
        Assert.Throws<ConflictException>(() => FiscalReleaseGate.EnsureAllowed(config, tenant, FiscalEnvironment.Production));

        FiscalReleaseGate.EnsureAllowed(config, tenant, FiscalEnvironment.Homologation);
    }

    [Fact]
    public void Production_allowed_when_tenant_is_released_in_database()
    {
        // Arrange
        var tenant = Guid.NewGuid();
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        Assert.False(FiscalReleaseGate.IsAllowed(emptyConfig, tenant, FiscalEnvironment.Production, isTenantReleased: false));
        Assert.Throws<ConflictException>(() => FiscalReleaseGate.EnsureAllowed(emptyConfig, tenant, FiscalEnvironment.Production, isTenantReleased: false));

        Assert.True(FiscalReleaseGate.IsAllowed(emptyConfig, tenant, FiscalEnvironment.Production, isTenantReleased: true));
        FiscalReleaseGate.EnsureAllowed(emptyConfig, tenant, FiscalEnvironment.Production, isTenantReleased: true);
    }

    [Fact]
    public void Cancellation_for_another_key_is_never_applied()
    {
        // Arrange
        var document = new FiscalDocument { Identity = "123" };
        var rawXml = "<retEnvEvento xmlns='http://www.portalfiscal.inf.br/nfe'><retEvento><infEvento><cStat>135</cStat><chNFe>456</chNFe><tpEvento>110111</tpEvento></infEvento></retEvento></retEnvEvento>";

        // Act
        var result = NationalFiscalGateway.ParseNfe(rawXml, document, "event");

        // Assert
        Assert.Equal(FiscalState.CancellationPending, result.State);
    }

    [Fact]
    public void Authorization_protocol_must_match_the_signed_payload_digest()
    {
        // Arrange
        using var certificate = Certificate();
        var snapshot = Snapshot(FiscalKind.Nfe);
        var document = new FiscalDocument
        {
            Kind = FiscalKind.Nfe,
            Environment = FiscalEnvironment.Homologation,
            Series = 1,
            Number = 1,
            Total = 100
        };

        document.Identity = FiscalXml.NfeKey(snapshot, 1, 1, 12345678);
        document.SubmittedXml = FiscalXml.Sign(FiscalXml.Invoice(document, snapshot), "infNFe", certificate);

        var ns = FiscalXml.Nfe;
        var digest = FiscalXml.Parse(document.SubmittedXml)
            .Descendants(XName.Get("DigestValue", SignedXml.XmlDsigNamespaceUrl))
            .Single()
            .Value;

        string Response(string value) => new XElement(ns + "retConsSitNFe",
            new XElement(ns + "protNFe",
                new XElement(ns + "infProt",
                    new XElement(ns + "cStat", "100"),
                    new XElement(ns + "chNFe", document.Identity),
                    new XElement(ns + "nProt", "123456789012345"),
                    new XElement(ns + "digVal", value)))).ToString();

        // Act & Assert
        var mismatchResult = NationalFiscalGateway.ParseNfe(Response("different"), document, "query");
        Assert.Equal(FiscalState.AwaitingConfirmation, mismatchResult.State);

        var matchResult = NationalFiscalGateway.ParseNfe(Response(digest), document, "query");
        Assert.Equal(FiscalState.Authorized, matchResult.State);
    }

    [Fact]
    public void Pdf_requires_authorized_xml_and_renders_a_processed_invoice()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        Assert.Throws<InvalidOperationException>(() => FiscalPdf.Generate(new FiscalDocument()));

        var snapshot = Snapshot(FiscalKind.Nfe);
        var document = new FiscalDocument
        {
            Kind = FiscalKind.Nfe,
            Environment = FiscalEnvironment.Homologation,
            Series = 1,
            Number = 1,
            Total = 100
        };

        document.Identity = FiscalXml.NfeKey(snapshot, 1, 1, 12345678);
        document.AccessKey = document.Identity;
        document.AuthorizedXml = new XElement(FiscalXml.Nfe + "nfeProc", FiscalXml.Invoice(document, snapshot).Root).ToString();

        // Act
        var pdf = FiscalPdf.Generate(document);

        // Assert
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.AsSpan(0, 8)));
        Assert.True(pdf.Length > 1000);
    }

    [Fact]
    public async Task Danfse_renders_with_qr_code_national_layout_and_totals()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var document = await FiscalPdfFixtures.Authorized(FiscalKind.Nfse);

        // Act
        var pdf = FiscalPdf.Generate(document);

        // Assert
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.AsSpan(0, 8)));
        Assert.True(pdf.Length > 1000);
    }

    [Theory]
    [InlineData(FiscalKind.Nfe, FiscalEnvironment.Production, FiscalState.Cancelled)]
    [InlineData(FiscalKind.Nfse, FiscalEnvironment.Homologation, FiscalState.Authorized)]
    public async Task Pdf_renders_banners_for_homologation_and_cancellation(FiscalKind kind, FiscalEnvironment env, FiscalState state)
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var snapshot = Snapshot(kind);
        var document = new FiscalDocument
        {
            Kind = kind,
            Environment = env,
            State = state,
            Series = 1,
            Number = 1,
            Total = 100
        };

        if (kind == FiscalKind.Nfe)
        {
            document.Identity = FiscalXml.NfeKey(snapshot, 1, 1, 12345678);
            document.AccessKey = document.Identity;
            document.AuthorizedXml = new XElement(FiscalXml.Nfe + "nfeProc", FiscalXml.Invoice(document, snapshot).Root).ToString();
        }
        else
        {
            document = await FiscalPdfFixtures.Authorized(FiscalKind.Nfse);
            document.State = state;
        }

        // Act
        var pdf = FiscalPdf.Generate(document);

        // Assert
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.AsSpan(0, 8)));
        Assert.True(pdf.Length > 1000);
    }

    [Fact]
    public void Xml_invoice_and_dps_include_simples_nacional_and_order_references()
    {
        // Arrange
        using var cert = Certificate();

        var nfeSnapshot = Snapshot(FiscalKind.Nfe);
        var nfeDoc = new FiscalDocument
        {
            Kind = FiscalKind.Nfe,
            Series = 1,
            Number = 1,
            Environment = FiscalEnvironment.Homologation,
            Total = 100
        };
        nfeDoc.Identity = FiscalXml.NfeKey(nfeSnapshot, 1, 1, 12345678);

        // Act & Assert (NFe)
        var nfeXml = FiscalXml.Sign(FiscalXml.Invoice(nfeDoc, nfeSnapshot), "infNFe", cert);
        FiscalXml.Validate(nfeXml, FiscalKind.Nfe);
        Assert.Contains("SIMPLES NACIONAL", nfeXml);
        Assert.Contains("NAO GERA DIREITO A CREDITO FISCAL DE IPI", nfeXml);
        Assert.Contains("Ordem de Servico: OS #0001", nfeXml);

        // Act & Assert (NFSe)
        var nfseSnapshot = Snapshot(FiscalKind.Nfse);
        var nfseDoc = new FiscalDocument
        {
            Kind = FiscalKind.Nfse,
            Series = 1,
            Number = 1,
            Environment = FiscalEnvironment.Homologation,
            Total = 100,
            Identity = "DPS355030821122233300018100001000000000000001"
        };
        var nfseXml = FiscalXml.Sign(FiscalXml.Dps(nfseDoc, nfseSnapshot), "infDPS", cert);
        FiscalXml.Validate(nfseXml, FiscalKind.Nfse);
        Assert.Contains("Simples Nacional", nfseXml);
        Assert.Contains("OS 0001", nfseXml);
    }
}
