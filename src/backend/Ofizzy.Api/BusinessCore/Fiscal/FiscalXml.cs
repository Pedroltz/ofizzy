using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalXml
{
    public static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    public static readonly XNamespace Nfse = "http://www.sped.fazenda.gov.br/nfse";

    public static string Amount(decimal x) => FiscalValidation.Money(x).ToString("F2", CultureInfo.InvariantCulture);

    public static XDocument Parse(string xml)
    {
        using var input = new StringReader(xml);
        using var reader = XmlReader.Create(input, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 4_000_000
        });

        return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    }

    public static string Sign(XDocument source, string target, X509Certificate2 certificate)
    {
        var doc = new XmlDocument
        {
            PreserveWhitespace = true,
            XmlResolver = null
        };

        using var reader = source.CreateReader();
        doc.Load(reader);

        var element = doc.GetElementsByTagName(target).OfType<XmlElement>().Single();
        using var rsa = certificate.GetRSAPrivateKey()
            ?? throw new ConflictException("Certificado sem chave privada RSA.");

        var signer = new SignedXml(doc) { SigningKey = rsa };
        signer.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        // The installed official baseline (PL_010c) fixes XMLDSIG to SHA-1. Do not
        // switch algorithms until the successor package and its transition rules
        // have been installed and validated together.
        signer.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

        var reference = new Reference($"#{element.GetAttribute("Id")}")
        {
            DigestMethod = SignedXml.XmlDsigSHA1Url
        };
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());

        signer.AddReference(reference);
        signer.KeyInfo = new KeyInfo();
        signer.KeyInfo.AddClause(new KeyInfoX509Data(certificate));
        signer.ComputeSignature();

        element.ParentNode!.AppendChild(doc.ImportNode(signer.GetXml(), true));
        return doc.OuterXml;
    }

    public static void Validate(string xml, FiscalKind kind)
    {
        ValidateSchema(xml, kind, FiscalSchemaCatalog.Document(kind));
    }

    public static void ValidateAuthorizedNfse(string xml)
    {
        ValidateSchema(xml, FiscalKind.Nfse, FiscalSchemaCatalog.AuthorizedNfse());
    }

    public static void ValidateCancellation(string xml, FiscalKind kind)
    {
        ValidateSchema(xml, kind, FiscalSchemaCatalog.Cancellation(kind));
    }

    public static void ValidateInutilization(string xml)
    {
        ValidateSchema(xml, FiscalKind.Nfe, FiscalSchemaCatalog.Inutilization());
    }

    private static void ValidateSchema(string xml, FiscalKind kind, FiscalSchemaReference schema)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "BusinessCore", "Fiscal", "Schemas", schema.Folder);
        var path = Path.Combine(directory, schema.EntryPoint);

        if (!File.Exists(path))
        {
            throw new ConflictException($"Esquemas oficiais de {kind} não instalados no servidor. Emissão bloqueada.");
        }

        var schemas = new XmlSchemaSet
        {
            XmlResolver = new LocalSchemaResolver(directory)
        };

        if (schema.HasDedicatedSignatureSchema)
        {
            // The bundled W3C schema has a legacy DOCTYPE; ignore it without resolving external resources.
            using var signatureSchema = XmlReader.Create(
                Path.Combine(directory, "xmldsig-core-schema.xsd"),
                new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                });

            schemas.Add(SignedXml.XmlDsigNamespaceUrl, signatureSchema);
        }

        schemas.Add(kind == FiscalKind.Nfse ? Nfse.NamespaceName : Nfe.NamespaceName, path);
        schemas.Compile();

        var errors = new List<string>();
        Parse(xml).Validate(schemas, (_, e) => errors.Add(e.Message));

        if (errors.Count > 0)
        {
            var firstError = errors[0].Split('\n')[0];
            throw new ConflictException($"O documento não atende ao leiaute oficial. Revise os dados fiscais. Campo: {firstError}");
        }
    }

    private sealed class LocalSchemaResolver(string directory) : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
        {
            var path = Path.GetFullPath(absoluteUri.LocalPath);
            var isAllowed = absoluteUri.IsFile
                && path.StartsWith(Path.GetFullPath(directory) + Path.DirectorySeparatorChar, StringComparison.Ordinal);

            if (!isAllowed)
            {
                throw new XmlException("Schema externo não permitido.");
            }

            return File.OpenRead(path);
        }
    }

    public static string NfeKey(FiscalSnapshot s, int series, long number, int randomCode)
    {
        var stateCode = FiscalValidation.StateCodes[s.Issuer.Address!.State];
        var yearMonth = s.IssuedAt.ToString("yyMM", CultureInfo.InvariantCulture);
        var prefix = $"{stateCode}{yearMonth}{s.Issuer.Cnpj}55{series:D3}{number:D9}1{randomCode:D8}";

        if (prefix.Length != 43)
        {
            throw new ConflictException("Numeração fiscal fora dos limites.");
        }

        var sum = 0;
        var weight = 2;

        for (var i = prefix.Length - 1; i >= 0; i--)
        {
            sum += (prefix[i] - '0') * weight;
            weight = weight == 9 ? 2 : weight + 1;
        }

        var digit = 11 - (sum % 11);
        return $"{prefix}{(digit >= 10 ? 0 : digit)}";
    }

    public static XDocument Dps(FiscalDocument document, FiscalSnapshot s)
    {
        XElement E(string name, params object?[] content) => new(Nfse + name, content);

        var issuer = s.Issuer;
        var recipient = s.Recipient;
        var address = issuer.Address!;
        var profile = s.Lines.First().Service!;

        XElement Address(FiscalAddress a) => E("end",
            E("endNac",
                E("cMun", a.CityCode),
                E("CEP", a.PostalCode)),
            E("xLgr", a.Street),
            E("nro", a.Number),
            E("xBairro", a.District));

        var recipientDocTag = recipient.Document!.Length == 14 ? "CNPJ" : "CPF";
        var servicesAggregateDescription = string.Join("; ", s.Lines.Select(x => $"{x.Description} ({x.Quantity.ToString(CultureInfo.InvariantCulture)})"));
        var orderNumberFormatted = s.OrderNumber.ToString("D4");

        return new XDocument(
            E("DPS", new XAttribute("versao", "1.01"),
                E("infDPS", new XAttribute("Id", document.Identity),
                    E("tpAmb", (int)document.Environment),
                    E("dhEmi", s.IssuedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                    E("verAplic", "Ofizzy_1.0"),
                    E("serie", document.Series),
                    E("nDPS", document.Number),
                    E("dCompet", recipient.Competence!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    E("tpEmit", 1),
                    E("cLocEmi", address.CityCode),
                    E("prest",
                        E("CNPJ", issuer.Cnpj),
                        string.IsNullOrEmpty(issuer.MunicipalRegistration) ? null : E("IM", issuer.MunicipalRegistration),
                        E("regTrib",
                            E("opSimpNac", issuer.Regime == "MEI" ? 2 : 3),
                            issuer.Regime == "MEI" ? null : E("regApTribSN", 1),
                            E("regEspTrib", 0))),
                    E("toma",
                        E(recipientDocTag, recipient.Document),
                        E("xNome", recipient.Name),
                        Address(recipient.Address!)),
                    E("serv",
                        E("locPrest", E("cLocPrestacao", address.CityCode)),
                        E("cServ",
                            E("cTribNac", profile.NationalCode),
                            string.IsNullOrEmpty(profile.MunicipalCode) ? null : E("cTribMun", profile.MunicipalCode),
                            E("xDescServ", servicesAggregateDescription),
                            string.IsNullOrEmpty(profile.Nbs) ? null : E("cNBS", profile.Nbs)),
                        E("infoCompl",
                            E("xPed", $"OS {orderNumberFormatted}"),
                            E("xInfComp", $"Documento emitido por ME ou EPP optante pelo Simples Nacional. Ordem de Servico: OS #{orderNumberFormatted}."))),
                    E("valores",
                        E("vServPrest", E("vServ", Amount(document.Total))),
                        E("trib",
                            E("tribMun", E("tribISSQN", 1), E("tpRetISSQN", 1)),
                            E("totTrib", issuer.Regime == "MEI"
                                ? E("indTotTrib", 0)
                                : E("pTotTribSN", Amount(profile.ApproximateTaxRate!.Value))))))));
    }

    public static XDocument Invoice(FiscalDocument d, FiscalSnapshot s)
    {
        XElement E(string name, params object?[] content) => new(Nfe + name, content);

        XElement Address(string name, FiscalAddress a) => E(name,
            E("xLgr", a.Street),
            E("nro", a.Number),
            E("xBairro", a.District),
            E("cMun", a.CityCode),
            E("xMun", a.City),
            E("UF", a.State),
            E("CEP", a.PostalCode),
            E("cPais", "1058"),
            E("xPais", "BRASIL"));

        var a = s.Issuer.Address!;
        var r = s.Recipient;

        var details = s.Lines.Select((line, index) =>
        {
            var p = line.Product!;
            var icms = E(p.Csosn == "500" ? "ICMSSN500" : "ICMSSN102",
                E("orig", p.Origin),
                E("CSOSN", p.Csosn));

            if (p.Csosn == "500")
            {
                icms.Add(
                    E("vBCSTRet", Amount(p.RetainedStBase!.Value * line.Quantity)),
                    E("pST", Amount(p.StRate!.Value)),
                    E("vICMSSubstituto", Amount(p.SubstituteAmount!.Value * line.Quantity)),
                    E("vICMSSTRet", Amount(p.RetainedStAmount!.Value * line.Quantity)));
            }

            return E("det", new XAttribute("nItem", index + 1),
                E("prod",
                    E("cProd", line.Code),
                    E("cEAN", p.Gtin),
                    E("xProd", line.Description),
                    E("NCM", p.Ncm),
                    string.IsNullOrEmpty(p.Cest) ? null : E("CEST", p.Cest),
                    E("CFOP", p.Cfop),
                    E("uCom", p.Unit),
                    E("qCom", line.Quantity.ToString("F4", CultureInfo.InvariantCulture)),
                    E("vUnCom", line.UnitPrice.ToString("F10", CultureInfo.InvariantCulture)),
                    E("vProd", Amount(line.Quantity * line.UnitPrice)),
                    E("cEANTrib", p.Gtin),
                    E("uTrib", p.Unit),
                    E("qTrib", line.Quantity.ToString("F4", CultureInfo.InvariantCulture)),
                    E("vUnTrib", line.UnitPrice.ToString("F10", CultureInfo.InvariantCulture)),
                    E("indTot", 1)),
                E("imposto",
                    E("ICMS", icms),
                    E("PIS", E("PISNT", E("CST", p.PisCst))),
                    E("COFINS", E("COFINSNT", E("CST", p.CofinsCst)))));
        });

        var total = E("ICMSTot");
        var totalFieldNames = new[]
        {
            "vBC", "vICMS", "vICMSDeson", "vFCP", "vBCST", "vST", "vFCPST", "vFCPSTRet",
            "vProd", "vFrete", "vSeg", "vDesc", "vII", "vIPI", "vIPIDevol", "vPIS", "vCOFINS", "vOutro", "vNF"
        };

        foreach (var name in totalFieldNames)
        {
            total.Add(E(name, Amount(name is "vProd" or "vNF" ? d.Total : 0)));
        }

        var orderNumberFormatted = s.OrderNumber.ToString("D4");
        var infCpl = $"DOCUMENTO EMITIDO POR ME OU EPP OPTANTE PELO SIMPLES NACIONAL. NAO GERA DIREITO A CREDITO FISCAL DE IPI. Ordem de Servico: OS #{orderNumberFormatted}.";
        var destDocTag = r.Document!.Length == 14 ? "CNPJ" : "CPF";
        var destName = d.Environment == FiscalEnvironment.Homologation
            ? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
            : r.Name;

        return new XDocument(
            E("NFe",
                E("infNFe",
                    new XAttribute("Id", $"NFe{d.Identity}"),
                    new XAttribute("versao", "4.00"),
                    E("ide",
                        E("cUF", FiscalValidation.StateCodes[a.State]),
                        E("cNF", d.Identity.Substring(35, 8)),
                        E("natOp", "VENDA DE MERCADORIAS"),
                        E("mod", 55),
                        E("serie", d.Series),
                        E("nNF", d.Number),
                        E("dhEmi", s.IssuedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                        E("tpNF", 1),
                        E("idDest", 1),
                        E("cMunFG", a.CityCode),
                        E("tpImp", 1),
                        E("tpEmis", 1),
                        E("cDV", d.Identity[^1]),
                        E("tpAmb", (int)d.Environment),
                        E("finNFe", 1),
                        E("indFinal", 1),
                        E("indPres", 1),
                        E("procEmi", 0),
                        E("verProc", "Ofizzy_1.0")),
                    E("emit",
                        E("CNPJ", s.Issuer.Cnpj),
                        E("xNome", s.Issuer.LegalName),
                        Address("enderEmit", a),
                        E("IE", s.Issuer.StateRegistration),
                        E("CRT", s.Issuer.Regime == "MEI" ? 4 : 1)),
                    E("dest",
                        E(destDocTag, r.Document),
                        E("xNome", destName),
                        Address("enderDest", r.Address!),
                        E("indIEDest", r.RecipientIeIndicator),
                        r.RecipientIeIndicator == "1" ? E("IE", r.StateRegistration) : null),
                    details,
                    E("total", total),
                    E("transp", E("modFrete", 9)),
                    E("pag",
                        E("detPag",
                            E("tPag", r.PaymentCode),
                            E("vPag", Amount(r.PaymentAmount!.Value)))),
                    E("infAdic", E("infCpl", infCpl)))));
    }
}
