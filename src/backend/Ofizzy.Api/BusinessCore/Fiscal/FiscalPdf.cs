using System.Globalization;
using System.Text;
using System.Xml.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;
using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalPdf
{
    public static byte[] Generate(FiscalDocument document)
    {
        if (document.AuthorizedXml == null) throw new InvalidOperationException("Documento não autorizado.");
        var xml = FiscalXml.Parse(document.AuthorizedXml);
        return document.Kind == FiscalKind.Nfe
            ? GenerateDanfe(document, xml)
            : GenerateDanfse(document, xml);
    }

    private static byte[] GenerateDanfe(FiscalDocument document, XDocument xml)
    {
        var ns = FiscalXml.Nfe;
        string Value(string name) => xml.Descendants(ns + name).FirstOrDefault()?.Value ?? "";
        string ValueOrDash(string name)
        {
            var v = Value(name);
            return string.IsNullOrWhiteSpace(v) ? "—" : v;
        }

        var nfe = xml.Descendants(ns + "infNFe").FirstOrDefault() ?? xml.Root!;
        var emit = nfe.Element(ns + "emit");
        var dest = nfe.Element(ns + "dest");
        var enderEmit = emit?.Element(ns + "enderEmit");
        var enderDest = dest?.Element(ns + "enderDest");
        var total = xml.Descendants(ns + "ICMSTot").FirstOrDefault();
        var prot = xml.Descendants(ns + "infProt").FirstOrDefault();

        var key = document.AccessKey ?? document.Identity;
        var formattedKey = FormatAccessKey(key);
        var nNf = Value("nNF");
        var formattedNumber = long.TryParse(nNf, out var num) ? num.ToString("N0", new CultureInfo("pt-BR")).Replace(",", ".") : nNf;
        var serie = document.Series.ToString();
        var nProt = prot?.Element(ns + "nProt")?.Value ?? Value("nProt");
        var dhRecbto = prot?.Element(ns + "dhRecbto")?.Value ?? Value("dhRecbto");

        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15);
            page.DefaultTextStyle(x => x.FontSize(7).FontFamily(Fonts.Arial));

            page.Header().Column(header =>
            {
                if (xml.Descendants().Any(x => x.Name.LocalName == "verAplic" && x.Value == "Ofizzy_SIMULACAO") || xml.Descendants().Any(x => x.Name.LocalName == "xMotivo" && x.Value.Contains("Simulação de Desenvolvimento", StringComparison.Ordinal)))
                    header.Item().AlignCenter().Text("SIMULAÇÃO LOCAL — SEM AUTORIZAÇÃO FISCAL").Bold().FontSize(9);
                if (document.Environment == FiscalEnvironment.Homologation)
                    header.Item().PaddingBottom(2).AlignCenter().Text("SEM VALOR FISCAL — EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO").Bold().FontSize(9).FontColor(Colors.Red.Medium);
                if (document.State == FiscalState.Cancelled)
                    header.Item().PaddingBottom(2).AlignCenter().Text("DOCUMENTO CANCELADO").Bold().FontSize(10).FontColor(Colors.Red.Darken2);

                // 1. Canhoto destacável de recebimento
                header.Item().PaddingBottom(4).Column(stub =>
                {
                    stub.Item().Border(0.5f).BorderColor(Colors.Black).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Padding(2)
                                .Text($"RECEBEMOS DE {emit?.Element(ns + "xNome")?.Value ?? "EMITENTE"} OS PRODUTOS CONSTANTES DA NOTA FISCAL INDICADA AO LADO")
                                .FontSize(5.5f).Bold();
                            col.Item().Row(r =>
                            {
                                r.ConstantItem(120).BorderRight(0.5f).BorderColor(Colors.Black).Padding(2).Height(22).Column(c =>
                                {
                                    c.Item().Text("DATA DE RECEBIMENTO").FontSize(5).Bold();
                                });
                                r.RelativeItem().Padding(2).Height(22).Column(c =>
                                {
                                    c.Item().Text("IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR").FontSize(5).Bold();
                                });
                            });
                        });
                        row.ConstantItem(100).BorderLeft(0.5f).BorderColor(Colors.Black).Padding(3).AlignCenter().AlignMiddle().Column(col =>
                        {
                            col.Item().AlignCenter().Text("NF-e").Bold().FontSize(10);
                            col.Item().AlignCenter().Text($"Nº {formattedNumber}").Bold().FontSize(7.5f);
                            col.Item().AlignCenter().Text($"SÉRIE {serie}").Bold().FontSize(7.5f);
                        });
                    });
                    stub.Item().PaddingVertical(2).AlignCenter().Text("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -").FontSize(5).FontColor(Colors.Grey.Darken1);
                });

                // 2. Cabeçalho oficial: Emitente | DANFE | Código de barras e chave de acesso
                header.Item().Border(0.5f).BorderColor(Colors.Black).Row(headRow =>
                {
                    headRow.RelativeItem(4.5f).Padding(3).Column(col =>
                    {
                        col.Item().Text(emit?.Element(ns + "xNome")?.Value ?? "EMITENTE").Bold().FontSize(8.5f);
                        var fant = emit?.Element(ns + "xFant")?.Value;
                        if (!string.IsNullOrWhiteSpace(fant)) col.Item().Text(fant).FontSize(7).Italic();
                        var rua = enderEmit?.Element(ns + "xLgr")?.Value;
                        var nro = enderEmit?.Element(ns + "nro")?.Value;
                        var bairro = enderEmit?.Element(ns + "xBairro")?.Value;
                        var cep = enderEmit?.Element(ns + "CEP")?.Value;
                        var mun = enderEmit?.Element(ns + "xMun")?.Value;
                        var uf = enderEmit?.Element(ns + "UF")?.Value;
                        var fone = enderEmit?.Element(ns + "fone")?.Value;
                        col.Item().Text($"{rua}, {nro} - {bairro}").FontSize(6.5f);
                        col.Item().Text($"CEP: {FormatCep(cep)} - {mun}/{uf}{(string.IsNullOrEmpty(fone) ? "" : " - Fone: " + fone)}").FontSize(6.5f);
                    });

                    headRow.ConstantItem(105).BorderLeft(0.5f).BorderRight(0.5f).BorderColor(Colors.Black).Padding(2).AlignCenter().Column(col =>
                    {
                        col.Item().AlignCenter().Text("DANFE").Bold().FontSize(12);
                        col.Item().AlignCenter().Text("Documento Auxiliar da Nota Fiscal Eletrônica").FontSize(5);
                        col.Item().PaddingVertical(1).AlignCenter().Row(r =>
                        {
                            r.AutoItem().Text("0 - ENTRADA\n1 - SAÍDA").FontSize(5f);
                            r.AutoItem().PaddingLeft(3).Border(0.5f).BorderColor(Colors.Black).PaddingHorizontal(3).PaddingVertical(1).Text("1").Bold().FontSize(7.5f);
                        });
                        col.Item().AlignCenter().Text($"Nº {formattedNumber}").Bold().FontSize(7.5f);
                        col.Item().AlignCenter().Text($"SÉRIE: {serie}").Bold().FontSize(7.5f);
                        col.Item().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(6f));
                            t.Span("FOLHA ");
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });

                    headRow.RelativeItem(5.5f).Padding(3).Column(col =>
                    {
                        col.Item().Height(28).Svg(Code(key, BarcodeFormat.CODE_128));
                        col.Item().PaddingTop(1).Text("CHAVE DE ACESSO").FontSize(5.5f).Bold();
                        col.Item().Text(formattedKey).FontSize(6.5f).Bold();
                        col.Item().PaddingTop(1).Text("Consulta de autenticidade no portal nacional da NF-e www.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora").FontSize(5).FontColor(Colors.Grey.Darken2);
                    });
                });

                // Natureza da Operação e Protocolo
                header.Item().Row(r =>
                {
                    r.RelativeItem(6).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
                    {
                        c.Item().Text("NATUREZA DA OPERAÇÃO").FontSize(5).Bold();
                        c.Item().Text(ValueOrDash("natOp")).FontSize(7);
                    });
                    r.RelativeItem(4).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
                    {
                        c.Item().Text("PROTOCOLO DE AUTORIZAÇÃO DE USO").FontSize(5).Bold();
                        c.Item().Text(string.IsNullOrWhiteSpace(nProt) ? "—" : $"{nProt} - {FormatDateTime(dhRecbto)}").FontSize(6.5f);
                    });
                });

                // Inscrições e CNPJ do Emitente
                header.Item().Row(r =>
                {
                    r.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
                    {
                        c.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(5).Bold();
                        c.Item().Text(emit?.Element(ns + "IE")?.Value ?? "—").FontSize(7);
                    });
                    r.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
                    {
                        c.Item().Text("INSC. ESTADUAL DO SUBST. TRIB.").FontSize(5).Bold();
                        c.Item().Text(emit?.Element(ns + "IEST")?.Value ?? "—").FontSize(7);
                    });
                    r.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
                    {
                        c.Item().Text("CNPJ").FontSize(5).Bold();
                        c.Item().Text(FormatCnpj(emit?.Element(ns + "CNPJ")?.Value)).FontSize(7);
                    });
                });
            });

            page.Content().Column(c =>
            {
                c.Spacing(3);

                // 3. DESTINATÁRIO / REMETENTE
                c.Item().Text("DESTINATÁRIO / REMETENTE").Bold().FontSize(6.5f);
                c.Item().Column(destCol =>
                {
                    destCol.Item().Row(r =>
                    {
                        r.RelativeItem(7).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("NOME / RAZÃO SOCIAL").FontSize(5).Bold();
                            col.Item().Text(dest?.Element(ns + "xNome")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(3).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("CNPJ / CPF").FontSize(5).Bold();
                            col.Item().Text(FormatCpfCnpj(dest?.Element(ns + "CNPJ")?.Value ?? dest?.Element(ns + "CPF")?.Value)).FontSize(7);
                        });
                        r.RelativeItem(2).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("DATA DA EMISSÃO").FontSize(5).Bold();
                            col.Item().Text(FormatDate(Value("dhEmi"))).FontSize(6.5f);
                        });
                    });
                    destCol.Item().Row(r =>
                    {
                        r.RelativeItem(5).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("ENDEREÇO").FontSize(5).Bold();
                            col.Item().Text($"{enderDest?.Element(ns + "xLgr")?.Value ?? "—"}, {enderDest?.Element(ns + "nro")?.Value ?? ""}".TrimEnd(',', ' ')).FontSize(7);
                        });
                        r.RelativeItem(3).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("BAIRRO / DISTRITO").FontSize(5).Bold();
                            col.Item().Text(enderDest?.Element(ns + "xBairro")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(2).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("CEP").FontSize(5).Bold();
                            col.Item().Text(FormatCep(enderDest?.Element(ns + "CEP")?.Value)).FontSize(7);
                        });
                        r.RelativeItem(2).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("DATA DA SAÍDA").FontSize(5).Bold();
                            col.Item().Text(FormatDate(Value("dhSaiEnt") == "" ? Value("dhEmi") : Value("dhSaiEnt"))).FontSize(6.5f);
                        });
                    });
                    destCol.Item().Row(r =>
                    {
                        r.RelativeItem(4).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("MUNICÍPIO").FontSize(5).Bold();
                            col.Item().Text(enderDest?.Element(ns + "xMun")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(2).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("FONE / FAX").FontSize(5).Bold();
                            col.Item().Text(enderDest?.Element(ns + "fone")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(1).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("UF").FontSize(5).Bold();
                            col.Item().Text(enderDest?.Element(ns + "UF")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(3).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("INSCRIÇÃO ESTADUAL").FontSize(5).Bold();
                            col.Item().Text(dest?.Element(ns + "IE")?.Value ?? "—").FontSize(7);
                        });
                        r.RelativeItem(2).Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(col =>
                        {
                            col.Item().Text("HORA DA SAÍDA").FontSize(5).Bold();
                            col.Item().Text(FormatTime(Value("dhSaiEnt") == "" ? Value("dhEmi") : Value("dhSaiEnt"))).FontSize(6.5f);
                        });
                    });
                });

                // 4. FORMA DE PAGAMENTO
                c.Item().Text("FORMA DE PAGAMENTO").Bold().FontSize(6.5f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(2.5f).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });
                    t.Cell().Text($"Forma: {PaymentDescription(Value("tPag"))}").FontSize(7);
                    t.Cell().Text($"Valor Pago: R$ {FormatCurrency(ValueOrDash("vPag"))}").Bold().FontSize(7);
                });

                // 5. CÁLCULO DO IMPOSTO
                c.Item().Text("CÁLCULO DO IMPOSTO").Bold().FontSize(6.5f);
                c.Item().Column(taxCol =>
                {
                    taxCol.Item().Row(r =>
                    {
                        TaxField(r, "BASE DE CÁLCULO DO ICMS", total?.Element(ns + "vBC")?.Value);
                        TaxField(r, "VALOR DO ICMS", total?.Element(ns + "vICMS")?.Value);
                        TaxField(r, "BASE DE CÁLCULO ICMS ST", total?.Element(ns + "vBCST")?.Value);
                        TaxField(r, "VALOR DO ICMS ST", total?.Element(ns + "vST")?.Value);
                        TaxField(r, "VALOR TOTAL DOS PRODUTOS", total?.Element(ns + "vProd")?.Value);
                    });
                    taxCol.Item().Row(r =>
                    {
                        TaxField(r, "VALOR DO FRETE", total?.Element(ns + "vFrete")?.Value);
                        TaxField(r, "VALOR DO SEGURO", total?.Element(ns + "vSeg")?.Value);
                        TaxField(r, "DESCONTO", total?.Element(ns + "vDesc")?.Value);
                        TaxField(r, "OUTRAS DESPESAS", total?.Element(ns + "vOutro")?.Value);
                        TaxField(r, "VALOR DO IPI", total?.Element(ns + "vIPI")?.Value);
                        TaxField(r, "VALOR TOTAL DA NOTA", total?.Element(ns + "vNF")?.Value, true);
                    });
                });

                // 6. TRANSPORTADOR / VOLUMES TRANSPORTADOS
                c.Item().Text("TRANSPORTADOR / VOLUMES TRANSPORTADOS").Bold().FontSize(6.5f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(2).Row(r =>
                {
                    r.RelativeItem().Text("FRETE POR CONTA: 9 - Sem Ocorrência de Transporte (retirada no estabelecimento)").FontSize(6.5f);
                });

                // 7. DADOS DOS PRODUTOS / SERVIÇOS
                c.Item().Text("DADOS DOS PRODUTOS / SERVIÇOS").Bold().FontSize(6.5f);
                c.Item().Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.8f); // Código
                        cols.RelativeColumn(3.8f); // Descrição
                        cols.RelativeColumn(1.1f); // NCM
                        cols.RelativeColumn(0.7f); // CSOSN
                        cols.RelativeColumn(0.7f); // CFOP
                        cols.RelativeColumn(0.5f); // Un
                        cols.RelativeColumn(0.8f); // Qtd
                        cols.RelativeColumn(1.1f); // V. Unit
                        cols.RelativeColumn(1.1f); // V. Total
                        cols.RelativeColumn(1.0f); // BC ICMS
                        cols.RelativeColumn(0.9f); // V. ICMS
                        cols.RelativeColumn(0.9f); // V. IPI
                    });

                    t.Header(h =>
                    {
                        foreach (var (lbl, align) in new[]
                        {
                            ("CÓDIGO", true), ("DESCRIÇÃO DO PRODUTO", false), ("NCM/SH", true),
                            ("CSOSN", true), ("CFOP", true), ("UN", true), ("QTD", true),
                            ("V. UNIT.", true), ("V. TOTAL", true), ("BC ICMS", true), ("V. ICMS", true), ("V. IPI", true)
                        })
                        {
                            var cell = h.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.5f);
                            if (align) cell.Text(lbl).AlignCenter().Bold().FontSize(5);
                            else cell.Text(lbl).Bold().FontSize(5);
                        }
                    });

                    foreach (var det in xml.Descendants(ns + "det"))
                    {
                        var prod = det.Element(ns + "prod");
                        var imp = det.Element(ns + "imposto");
                        var csosn = imp?.Descendants(ns + "CSOSN").FirstOrDefault()?.Value ?? "—";
                        var bcIcms = imp?.Descendants(ns + "vBC").FirstOrDefault()?.Value ?? "0.00";
                        var vIcms = imp?.Descendants(ns + "vICMS").FirstOrDefault()?.Value ?? "0.00";
                        var vIpi = imp?.Descendants(ns + "vIPI").FirstOrDefault()?.Value ?? "0.00";

                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(prod?.Element(ns + "cProd")?.Value ?? "—").FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(prod?.Element(ns + "xProd")?.Value ?? "—").FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(prod?.Element(ns + "NCM")?.Value ?? "—").AlignCenter().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(csosn).AlignCenter().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(prod?.Element(ns + "CFOP")?.Value ?? "—").AlignCenter().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(prod?.Element(ns + "uCom")?.Value ?? "—").AlignCenter().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatQuantity(prod?.Element(ns + "qCom")?.Value)).AlignRight().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatUnitPrice(prod?.Element(ns + "vUnCom")?.Value)).AlignRight().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatCurrency(prod?.Element(ns + "vProd")?.Value)).AlignRight().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatCurrency(bcIcms)).AlignRight().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatCurrency(vIcms)).AlignRight().FontSize(5.5f);
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(1.2f).Text(FormatCurrency(vIpi)).AlignRight().FontSize(5.5f);
                    }
                });

                // 8. DADOS ADICIONAIS
                c.Item().Text("DADOS ADICIONAIS").Bold().FontSize(6.5f);
                c.Item().Row(r =>
                {
                    r.RelativeItem(7).Border(0.5f).BorderColor(Colors.Black).Padding(3).MinHeight(45).Column(col =>
                    {
                        col.Item().Text("INFORMAÇÕES COMPLEMENTARES").FontSize(5.5f).Bold();
                        col.Item().Text(ValueOrDash("infCpl")).FontSize(6.5f);
                    });
                    r.RelativeItem(3).Border(0.5f).BorderColor(Colors.Black).Padding(3).MinHeight(45).Column(col =>
                    {
                        col.Item().Text("RESERVADO AO FISCO").FontSize(5.5f).Bold();
                    });
                });
            });

            page.Footer().Row(r =>
            {
                r.RelativeItem().Text("Ofizzy · Sistema de Gestão para Oficinas").FontSize(6f).FontColor(Colors.Grey.Darken1);
                r.RelativeItem().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(6f));
                    t.Span("DANFE - Folha ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        })).GeneratePdf();
    }

    private static byte[] GenerateDanfse(FiscalDocument document, XDocument xml)
    {
        var ns = FiscalXml.Nfse;
        var nfse = xml.Root?.Name == ns + "NFSe" ? xml.Root.Element(ns + "infNFSe") : null;
        var dps = nfse?.Element(ns + "DPS")?.Element(ns + "infDPS");
        var number = nfse?.Element(ns + "nNFSe")?.Value;
        var key = nfse?.Attribute("Id")?.Value;
        if (dps == null || string.IsNullOrWhiteSpace(number) || key == null || !key.StartsWith("NFS", StringComparison.Ordinal)
            || key.Length != 53 || !key.AsSpan(3).ToArray().All(char.IsAsciiDigit))
            throw new ConflictException("O XML não contém uma NFS-e autorizada válida para gerar o DANFSe. Consulte a situação do documento.");
        key = key[3..];
        var formattedKey = FormatNfseKey(key);
        var qrUrl = $"https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave={key}";
        var prest = nfse!.Element(ns + "emit");
        var toma = dps.Element(ns + "toma");
        var endPrest = prest?.Element(ns + "enderNac");
        var endToma = toma?.Element(ns + "end");
        var valores = nfse.Element(ns + "valores");
        var dpsValues = dps.Element(ns + "valores");
        var service = dps.Element(ns + "serv");
        string Value(string name) => name switch
        {
            "pAliqAplic" or "vBC" or "vISSQN" => valores?.Element(ns + name)?.Value ?? "",
            "vDescIncond" => dpsValues?.Element(ns + "vDescCondIncond")?.Element(ns + "vDescIncond")?.Value ?? "",
            "vDed" => valores?.Element(ns + "vCalcDR")?.Value ?? "",
            "tpRetISSQN" => dpsValues?.Element(ns + "trib")?.Element(ns + "tribMun")?.Element(ns + name)?.Value ?? "",
            "pTotTribSN" => dpsValues?.Element(ns + "trib")?.Element(ns + "totTrib")?.Element(ns + name)?.Value ?? "",
            "xInfComp" => service?.Element(ns + "infoCompl")?.Element(ns + name)?.Value ?? "",
            _ => service?.Element(ns + "cServ")?.Element(ns + name)?.Value ?? ""
        };
        string ValueOrDash(string name) => string.IsNullOrWhiteSpace(Value(name)) ? "—" : Value(name);
        FiscalSnapshot? snapshot = null;
        try { if (!string.IsNullOrWhiteSpace(document.Snapshot)) snapshot = FiscalJson.Required<FiscalSnapshot>(document.Snapshot); }
        catch (System.Text.Json.JsonException) { /* Optional metadata must not override the authorized XML. */ }
        var issuer = snapshot?.Issuer;
        var prestName = prest?.Element(ns + "xNome")?.Value;
        if (string.IsNullOrWhiteSpace(prestName) || endPrest == null || valores?.Element(ns + "vLiq") == null)
            throw new ConflictException("O XML da NFS-e não contém emitente, endereço ou valores autorizados. Consulte a situação do documento.");
        var prestAddress = FormatAddress(endPrest);
        var issuerCity = nfse.Element(ns + "xLocEmi")?.Value;
        if (!string.IsNullOrWhiteSpace(issuerCity)) prestAddress += " - " + issuerCity;
        var nNfse = number;
        var nDps = dps.Element(ns + "nDPS")?.Value ?? "—";
        var serieDps = dps.Element(ns + "serie")?.Value ?? "—";
        var dhEmi = nfse.Element(ns + "dhProc")?.Value ?? "";
        var dCompet = dps.Element(ns + "dCompet")?.Value ?? "";
        var vServ = dpsValues?.Element(ns + "vServPrest")?.Element(ns + "vServ")?.Value;
        if (string.IsNullOrWhiteSpace(vServ)) throw new ConflictException("O XML da NFS-e não contém o valor do serviço.");
        var vLiq = valores!.Element(ns + "vLiq")!.Value;
        var simples = dps.Element(ns + "prest")?.Element(ns + "regTrib")?.Element(ns + "opSimpNac")?.Value;

        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15);
            page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily(Fonts.Arial));

            page.Header().Column(header =>
            {
                if (xml.Descendants().Any(x => x.Name.LocalName == "verAplic" && x.Value == "Ofizzy_SIMULACAO") || xml.Descendants().Any(x => x.Name.LocalName == "xMotivo" && x.Value.Contains("Simulação de Desenvolvimento", StringComparison.Ordinal)))
                    header.Item().Text("SIMULAÇÃO LOCAL — SEM AUTORIZAÇÃO FISCAL").AlignCenter().Bold().FontSize(9);
                if (document.Environment == FiscalEnvironment.Homologation)
                    header.Item().PaddingBottom(2).Text("SEM VALOR FISCAL — EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO").AlignCenter().Bold().FontSize(9).FontColor(Colors.Red.Medium);
                if (document.State == FiscalState.Cancelled)
                    header.Item().PaddingBottom(2).Text("DOCUMENTO CANCELADO").AlignCenter().Bold().FontSize(10).FontColor(Colors.Red.Darken2);

                // Cabeçalho Oficial do Padrão Nacional
                header.Item().Border(0.5f).BorderColor(Colors.Black).Row(row =>
                {
                    row.RelativeItem(6).Padding(4).Column(col =>
                    {
                        col.Item().Text("NFS-e - NOTA FISCAL DE SERVIÇOS ELETRÔNICA").Bold().FontSize(11);
                        col.Item().Text("Documento Auxiliar da NFS-e (DANFSe)").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text("Emitida nos termos da Resolução CGSN nº 169/2022 e do Convênio Nacional da NFS-e").FontSize(6).Italic();
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("NÚMERO DA NFS-e").FontSize(5.5f).Bold();
                                c.Item().Text(nNfse).Bold().FontSize(9);
                            });
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DATA / HORA EMISSÃO").FontSize(5.5f).Bold();
                                c.Item().Text(FormatDateTime(dhEmi)).FontSize(7.5f);
                            });
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("COMPETÊNCIA").FontSize(5.5f).Bold();
                                c.Item().Text(FormatDate(dCompet)).FontSize(7.5f);
                            });
                        });
                        col.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("NÚMERO DA DPS").FontSize(5.5f).Bold();
                                c.Item().Text(nDps).FontSize(7.5f);
                            });
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SÉRIE DA DPS").FontSize(5.5f).Bold();
                                c.Item().Text(serieDps).FontSize(7.5f);
                            });
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CÓDIGO MUNICÍPIO").FontSize(5.5f).Bold();
                                c.Item().Text(dps?.Element(ns + "cLocEmi")?.Value ?? "—").FontSize(7.5f);
                            });
                        });
                    });

                    row.ConstantItem(120).BorderLeft(0.5f).BorderColor(Colors.Black).Padding(3).AlignCenter().Column(col =>
                    {
                        col.Item().Height(60).Svg(Code(qrUrl, BarcodeFormat.QR_CODE));
                        col.Item().PaddingTop(2).Text("CHAVE DE ACESSO").FontSize(5).Bold();
                        col.Item().Text(formattedKey).FontSize(5.5f).Bold().AlignCenter();
                        col.Item().Text("Consulte autenticidade via QR Code no Portal Nacional").FontSize(4.5f).FontColor(Colors.Grey.Darken2).AlignCenter();
                    });
                });
            });

            page.Content().Column(c =>
            {
                c.Spacing(4);

                // PRESTADOR DE SERVIÇOS
                c.Item().Text("PRESTADOR DE SERVIÇOS").Bold().FontSize(7f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(3).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(6.5f);
                        cols.RelativeColumn(3.5f);
                    });

                    t.Cell().Text(prestName).Bold().FontSize(8.5f);
                    t.Cell().Text($"CNPJ: {FormatCnpj(prest?.Element(ns + "CNPJ")?.Value ?? issuer?.Cnpj)}").AlignRight().Bold().FontSize(7.5f);

                    t.Cell().Text(prestAddress).FontSize(7f);
                    t.Cell().Text($"Inscrição Municipal: {prest?.Element(ns + "IM")?.Value ?? issuer?.MunicipalRegistration ?? "—"}").AlignRight().FontSize(7f);

                    t.Cell().ColumnSpan(2).PaddingTop(2).Text($"Opção pelo Simples Nacional: {simples switch { "2" => "Sim (MEI)", "3" => "Sim (ME/EPP)", _ => "Não optante" }}").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                });

                // TOMADOR DO SERVIÇO
                c.Item().Text("TOMADOR DO SERVIÇO").Bold().FontSize(7f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(3).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(6.5f);
                        cols.RelativeColumn(3.5f);
                    });

                    t.Cell().Text(toma?.Element(ns + "xNome")?.Value ?? "TOMADOR").Bold().FontSize(8.5f);
                    t.Cell().Text($"CPF/CNPJ: {FormatCpfCnpj(toma?.Element(ns + "CNPJ")?.Value ?? toma?.Element(ns + "CPF")?.Value)}").AlignRight().Bold().FontSize(7.5f);

                    t.Cell().ColumnSpan(2).Text(FormatAddress(endToma)).FontSize(7f);
                });

                // SERVIÇO PRESTADO
                c.Item().Text("DISCRIMINAÇÃO DOS SERVIÇOS").Bold().FontSize(7f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(3).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Código Tributação Nacional: {ValueOrDash("cTribNac")}").Bold().FontSize(7f);
                        var cTribMun = Value("cTribMun");
                        if (!string.IsNullOrWhiteSpace(cTribMun)) r.RelativeItem().Text($"Código Municipal: {cTribMun}").FontSize(7f);
                        var nbs = Value("cNBS");
                        if (!string.IsNullOrWhiteSpace(nbs)) r.RelativeItem().Text($"NBS: {nbs}").FontSize(7f);
                    });
                    col.Item().PaddingTop(3).Text(ValueOrDash("xDescServ")).FontSize(7.5f);
                });

                // TRIBUTAÇÃO E VALORES
                c.Item().Text("TRIBUTAÇÃO MUNICIPAL E VALORES").Bold().FontSize(7f);
                c.Item().Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    foreach (var h in new[] { "VALOR DO SERVIÇO", "DESCONTO INCOND.", "DEDUÇÕES LEGAIS", "BASE DE CÁLCULO", "ALÍQUOTA ISSQN", "VALOR DO ISSQN" })
                    {
                        t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(h).FontSize(5).Bold();
                    }

                    var aliqText = Value("pAliqAplic") == "" ? "—" : $"{FormatDecimal(Value("pAliqAplic"))}%";
                    var bcText = Value("vBC") == "" ? vServ : Value("vBC");
                    var issVal = Value("vISSQN");
                    var issText = string.IsNullOrWhiteSpace(issVal) ? "—" : FormatCurrency(issVal);

                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(FormatCurrency(vServ)).AlignRight().FontSize(6.5f);
                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(FormatCurrency(Value("vDescIncond"))).AlignRight().FontSize(6.5f);
                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(FormatCurrency(Value("vDed"))).AlignRight().FontSize(6.5f);
                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(FormatCurrency(bcText)).AlignRight().FontSize(6.5f);
                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(aliqText).AlignRight().FontSize(6.5f);
                    t.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(2).Text(issText).AlignRight().FontSize(6.5f);

                    t.Cell().ColumnSpan(6).Border(0.5f).BorderColor(Colors.Black).Padding(2.5f).Row(r =>
                    {
                        r.RelativeItem().Text($"ISSQN Retido: {(Value("tpRetISSQN") == "2" ? "SIM" : "NÃO")}").FontSize(6.5f);
                        var totTrib = Value("pTotTribSN");
                        if (!string.IsNullOrWhiteSpace(totTrib)) r.RelativeItem().Text($"Tributos Aproximados: {FormatDecimal(totTrib)}% (Lei 12.741/2012)").AlignRight().FontSize(6.5f);
                    });
                });

                // VALOR LÍQUIDO EM DESTAQUE
                c.Item().Border(0.5f).BorderColor(Colors.Green.Darken2).Background(Colors.Green.Lighten5).Padding(4).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    t.Cell().AlignMiddle().Text("VALOR LÍQUIDO DA NFS-e").Bold().FontSize(9).FontColor(Colors.Green.Darken3);
                    t.Cell().AlignMiddle().Text($"R$ {FormatCurrency(vLiq)}").AlignRight().Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                });

                // INFORMAÇÕES COMPLEMENTARES
                c.Item().Text("INFORMAÇÕES COMPLEMENTARES").Bold().FontSize(7f);
                c.Item().Border(0.5f).BorderColor(Colors.Black).Padding(3).MinHeight(35).Column(col =>
                {
                    col.Item().Text(ValueOrDash("xInfComp")).FontSize(6.5f);
                });
            });

            page.Footer().Row(r =>
            {
                r.RelativeItem().Text("Ofizzy · Sistema de Gestão para Oficinas").FontSize(6f).FontColor(Colors.Grey.Darken1);
                r.RelativeItem().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(6f));
                    t.Span("DANFSe - Folha ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        })).GeneratePdf();
    }

    private static void TaxField(RowDescriptor row, string label, string? value, bool highlight = false)
    {
        row.RelativeItem().Border(0.5f).BorderColor(Colors.Black).Padding(2).Column(c =>
        {
            c.Item().Text(label).FontSize(5).Bold();
            var text = c.Item().Text(FormatCurrency(value)).AlignRight().FontSize(6.5f);
            if (highlight) text.Bold();
        });
    }

    private static string FormatCurrency(string? value)
    {
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var d))
            return d.ToString("N2", new CultureInfo("pt-BR"));
        return string.IsNullOrWhiteSpace(value) ? "0,00" : value;
    }

    private static string FormatUnitPrice(string? value)
    {
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var d))
        {
            var decPart = d - Math.Truncate(d);
            if (Math.Round(decPart, 2) != decPart)
                return d.ToString("N4", new CultureInfo("pt-BR"));
            return d.ToString("N2", new CultureInfo("pt-BR"));
        }
        return string.IsNullOrWhiteSpace(value) ? "0,00" : value;
    }

    private static string FormatQuantity(string? value)
    {
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var d))
        {
            if (d == Math.Truncate(d))
                return d.ToString("N2", new CultureInfo("pt-BR"));
            return d.ToString("N3", new CultureInfo("pt-BR"));
        }
        return string.IsNullOrWhiteSpace(value) ? "0,00" : value;
    }

    private static string FormatDecimal(string? value)
    {
        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var d))
            return d.ToString("N2", new CultureInfo("pt-BR"));
        return string.IsNullOrWhiteSpace(value) ? "0,00" : value;
    }

    private static string FormatAddress(XElement? address)
    {
        if (address == null) return "—";
        var rua = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "xLgr" or "xLgrComp")?.Value;
        var nro = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "nro")?.Value;
        var bairro = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "xBairro")?.Value;
        var cep = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "CEP")?.Value;
        var mun = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "xMun")?.Value;
        var uf = address.Descendants().FirstOrDefault(e => e.Name.LocalName is "UF")?.Value;
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(rua))
        {
            var logradouro = rua;
            if (!string.IsNullOrWhiteSpace(nro)) logradouro += $", {nro}";
            parts.Add(logradouro);
        }
        if (!string.IsNullOrWhiteSpace(bairro)) parts.Add(bairro);
        if (!string.IsNullOrWhiteSpace(cep)) parts.Add($"CEP: {FormatCep(cep)}");
        if (!string.IsNullOrWhiteSpace(mun))
        {
            var cityState = mun;
            if (!string.IsNullOrWhiteSpace(uf)) cityState += $"/{uf}";
            parts.Add(cityState);
        }
        else if (!string.IsNullOrWhiteSpace(uf)) parts.Add(uf);
        return parts.Count > 0 ? string.Join(" - ", parts) : "—";
    }

    private static string FormatDateTime(string iso)
    {
        if (DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            return dto.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(iso) ? "—" : iso;
    }

    private static string FormatDate(string iso)
    {
        if (DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            return dto.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (DateOnly.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(iso) ? "—" : iso;
    }

    private static string FormatTime(string iso)
    {
        if (DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            return dto.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(iso) ? "—" : iso;
    }

    private static string FormatCpfCnpj(string? doc)
    {
        if (string.IsNullOrWhiteSpace(doc)) return "—";
        var digits = new string(doc.Where(char.IsDigit).ToArray());
        if (digits.Length == 11) return $"{digits[..3]}.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-{digits.Substring(9, 2)}";
        if (digits.Length == 14) return $"{digits[..2]}.{digits.Substring(2, 3)}.{digits.Substring(5, 3)}/{digits.Substring(8, 4)}-{digits.Substring(12, 2)}";
        return doc;
    }

    private static string FormatCnpj(string? doc) => FormatCpfCnpj(doc);

    private static string FormatCep(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep)) return "—";
        var digits = new string(cep.Where(char.IsDigit).ToArray());
        return digits.Length == 8 ? $"{digits[..5]}-{digits[5..]}" : cep;
    }

    private static string FormatAccessKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length != 44) return key;
        var sb = new StringBuilder();
        for (var i = 0; i < key.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(' ');
            sb.Append(key[i]);
        }
        return sb.ToString();
    }

    private static string FormatNfseKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length != 50) return key;
        var sb = new StringBuilder();
        for (var i = 0; i < key.Length; i++)
        {
            if (i > 0 && i % 5 == 0) sb.Append(' ');
            sb.Append(key[i]);
        }
        return sb.ToString();
    }

    private static string PaymentDescription(string code) => code switch
    {
        "01" => "Dinheiro",
        "02" => "Cheque",
        "03" => "Cartão de Crédito",
        "04" => "Cartão de Débito",
        "15" => "Boleto Bancário",
        "16" => "Depósito Bancário",
        "17" => "Pagamento Instantâneo (PIX)",
        "18" => "Transferência Bancária",
        "90" => "Sem Pagamento",
        _ => "Outros"
    };

    private static string Code(string value, BarcodeFormat format)
    {
        var matrix = new MultiFormatWriter().encode(value, format,
            format == BarcodeFormat.CODE_128 ? 1 : 180,
            format == BarcodeFormat.CODE_128 ? 40 : 180,
            new Dictionary<EncodeHintType, object> { { EncodeHintType.MARGIN, format == BarcodeFormat.CODE_128 ? 20 : 4 } });
        var svg = new StringBuilder($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {matrix.Width} {matrix.Height}\"><rect width=\"100%\" height=\"100%\" fill=\"white\"/><g fill=\"black\">");
        // Merge contiguous bars: individual pixel rectangles introduce raster seams that hinder scanners.
        var rowStep = format == BarcodeFormat.CODE_128 ? matrix.Height : 1;
        for (var y = 0; y < matrix.Height; y += rowStep)
        {
            for (var x = 0; x < matrix.Width; x++)
            {
                if (!matrix[x, y]) continue;
                var first = x;
                while (x + 1 < matrix.Width && matrix[x + 1, y]) x++;
                svg.Append($"<rect x=\"{first}\" y=\"{y}\" width=\"{x - first + 1}\" height=\"{rowStep}\"/>");
            }
        }
        return svg.Append("</g></svg>").ToString();
    }
}
