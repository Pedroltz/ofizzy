using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

/// <summary>
/// Gateway simulado exclusivo para a fase de desenvolvimento local.
/// Permite testar o fluxo de emissão, cancelamento e inutilização sem requisições de rede à SEFAZ ou ao Sefin Nacional.
/// </summary>
public sealed class DevSimulatedFiscalGateway : IFiscalGateway
{
    public Task<FiscalGatewayResult> Send(FiscalDocument document, X509Certificate2 certificate, CancellationToken ct)
    {
        EnsureHomologation(document.Environment);
        ct.ThrowIfCancellationRequested();
        if (document.Kind == FiscalKind.Nfse)
        {
            var snapshot = FiscalJson.Required<FiscalSnapshot>(document.Snapshot);
            var issuer = snapshot.Issuer;
            var address = issuer.Address!;
            // Stable, numeric test identity, not an official access-key allocation.
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(document.Identity));
            var key = (new BigInteger(hash, isUnsigned: true, isBigEndian: true) % BigInteger.Pow(10, 50)).ToString("D50", CultureInfo.InvariantCulture);
            XElement E(string name, params object?[] content) => new(FiscalXml.Nfse + name, content);
            var originalDps = FiscalXml.Parse(document.SubmittedXml!);
            var response = new XDocument(E("NFSe", new XAttribute("versao", "1.01"),
                E("infNFSe", new XAttribute("Id", "NFS" + key),
                    E("xLocEmi", address.City), E("xLocPrestacao", address.City), E("nNFSe", document.Number),
                    E("xTribNac", "Servico ficticio para teste local"), E("verAplic", "Ofizzy_SIMULACAO"),
                    E("ambGer", 2), E("tpEmis", 1), E("cStat", issuer.Regime == "MEI" ? 107 : 100),
                    E("dhProc", snapshot.IssuedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)), E("nDFSe", document.Number),
                    E("emit", E("CNPJ", issuer.Cnpj), string.IsNullOrWhiteSpace(issuer.MunicipalRegistration) ? null : E("IM", issuer.MunicipalRegistration),
                        E("xNome", issuer.LegalName), E("enderNac", E("xLgr", address.Street), E("nro", address.Number),
                            E("xBairro", address.District), E("cMun", address.CityCode), E("UF", address.State), E("CEP", address.PostalCode))),
                    E("valores", E("vLiq", FiscalXml.Amount(document.Total))),
                    E("xOutInf", "SIMULACAO LOCAL - SEM AUTORIZACAO FISCAL"), originalDps.Root)));
            // Only a fictitious certificate is used here; the inner DPS is retained intact.
            var authorizedXml = FiscalXml.Sign(response, "infNFSe", certificate);
            FiscalXml.ValidateAuthorizedNfse(authorizedXml);
            return Task.FromResult(new FiscalGatewayResult(FiscalState.Authorized, Key: key,
                Protocol: "135260000000001", AuthorizedXml: authorizedXml,
                Message: "NFS-e simulada localmente; sem autorização do órgão fiscal."));
        }

        var ns = FiscalXml.Nfe;
        var original = FiscalXml.Parse(document.SubmittedXml!);
        var infNfe = original.Root?.Descendants(ns + "infNFe").FirstOrDefault()
                     ?? original.Descendants(ns + "infNFe").First();
        var chNfe = infNfe.Attribute("Id")?.Value.Replace("NFe", "") ?? document.Identity;
        var digest = original.Descendants(XNamespace.Get("http://www.w3.org/2000/09/xmldsig#") + "DigestValue").FirstOrDefault()?.Value ?? "SIMULATED_DIGEST_DEV";

        var protNfe = new XElement(ns + "protNFe", new XAttribute("versao", "4.00"),
            new XElement(ns + "infProt",
                new XElement(ns + "tpAmb", (int)document.Environment),
                new XElement(ns + "verAplic", "SP_NFE_PL_010c"),
                new XElement(ns + "chNFe", chNfe),
                new XElement(ns + "dhRecbto", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(ns + "nProt", "135260000000001"),
                new XElement(ns + "digVal", digest),
                new XElement(ns + "cStat", "100"),
                new XElement(ns + "xMotivo", "Autorizado o uso da NF-e (Simulação de Desenvolvimento)")
            )
        );

        var nfeProc = new XElement(ns + "nfeProc", new XAttribute("versao", "4.00"),
            original.Root,
            protNfe
        );

        return Task.FromResult(new FiscalGatewayResult(
            FiscalState.Authorized,
            Key: chNfe,
            Protocol: "135260000000001",
            AuthorizedXml: nfeProc.ToString(SaveOptions.DisableFormatting),
            Message: "NF-e autorizada com sucesso (Simulação de Desenvolvimento)"
        ));
    }

    public Task<FiscalGatewayResult> Query(FiscalDocument document, X509Certificate2 certificate, CancellationToken ct)
    {
        EnsureHomologation(document.Environment);
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new FiscalGatewayResult(
            document.State,
            Key: document.AccessKey ?? document.Identity,
            Protocol: document.Protocol,
            AuthorizedXml: document.AuthorizedXml,
            Message: "Situação consultada (Simulação de Desenvolvimento)"
        ));
    }

    public Task<FiscalGatewayResult> Cancel(FiscalDocument document, string signedEvent, X509Certificate2 certificate, CancellationToken ct)
    {
        EnsureHomologation(document.Environment);
        ct.ThrowIfCancellationRequested();
        FiscalXml.ValidateCancellation(signedEvent, document.Kind);
        return Task.FromResult(new FiscalGatewayResult(
            FiscalState.Cancelled,
            Message: "Cancelamento simulado (Simulação de Desenvolvimento)"
        ));
    }

    public Task<FiscalGatewayResult> Inutilize(FiscalSnapshot snapshot, string signedXml, X509Certificate2 certificate, CancellationToken ct)
    {
        EnsureHomologation(snapshot.Issuer.Environment);
        ct.ThrowIfCancellationRequested();
        FiscalXml.ValidateInutilization(signedXml);
        return Task.FromResult(new FiscalGatewayResult(
            FiscalState.Inutilized,
            Protocol: "135260000000002",
            Message: "Inutilização simulada (Simulação de Desenvolvimento)"
        ));
    }
    private static void EnsureHomologation(FiscalEnvironment environment)
    {
        if (environment != FiscalEnvironment.Homologation)
            throw new ConflictException("O simulador permite somente o ambiente fiscal de homologação.");
    }
}
