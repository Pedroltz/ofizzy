using System.Xml.Linq;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Modules.Fiscal;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Ofizzy.UnitTests;

internal static class FiscalPdfFixtures
{
    public static async Task<FiscalDocument> Authorized(FiscalKind kind, int lines = 1)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        using var cert = FiscalTests.Certificate();
        var s = FiscalTests.Snapshot(kind);
        s = s with
        {
            Issuer = s.Issuer with { LegalName = "EMITENTE DO XML", Address = s.Issuer.Address! with { Street = "Rua do Emitente" } },
            Recipient = s.Recipient with { Name = "TOMADOR DO XML", Address = s.Recipient.Address! with { Street = "Rua do Tomador" }, PaymentAmount = lines * 100 },
            Lines = Enumerable.Range(1, lines).Select(i => s.Lines[0] with { Id = Guid.NewGuid(), Code = $"P-{i:D3}", Description = $"Produto {i:D3} revisao e alinhamento" }).ToList()
        };
        var doc = new FiscalDocument
        {
            Kind = kind,
            Environment = FiscalEnvironment.Homologation,
            Series = 1,
            Number = 101,
            Snapshot = FiscalJson.Write(s),
            Total = lines * 100
        };
        doc.Identity = kind == FiscalKind.Nfe ? FiscalXml.NfeKey(s, 1, doc.Number, 12345678) : "DPS355030821122233300018100001000000000000101";
        doc.SubmittedXml = FiscalXml.Sign(kind == FiscalKind.Nfe ? FiscalXml.Invoice(doc, s) : FiscalXml.Dps(doc, s), kind == FiscalKind.Nfe ? "infNFe" : "infDPS", cert);
        FiscalXml.Validate(doc.SubmittedXml, kind);
        var response = await new DevSimulatedFiscalGateway().Send(doc, cert, default);
        doc.AuthorizedXml = response.AuthorizedXml; doc.AccessKey = response.Key; doc.Protocol = response.Protocol; doc.State = response.State;
        return doc;
    }
}

public sealed class FiscalPdfVisualTests
{
    private static byte[] Generate(FiscalDocument doc)
    {
        var bytes = FiscalPdf.Generate(doc);
        var directory = Environment.GetEnvironmentVariable("OFIZZY_PDF_ARTIFACTS");
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, $"{doc.Kind}-{doc.Total}-{doc.State}.pdf"), bytes);
        }
        return bytes;
    }
    private static string Text(PdfDocument pdf) => string.Join("\n", pdf.GetPages().Select(page => ContentOrderTextExtractor.GetText(page))).Normalize(System.Text.NormalizationForm.FormKC);

    [Fact]
    public async Task Danfse_uses_authorized_issuer_values_number_and_dps_recipient()
    {
        var doc = await FiscalPdfFixtures.Authorized(FiscalKind.Nfse);
        var ns = FiscalXml.Nfse;
        var xml = FiscalXml.Parse(doc.AuthorizedXml!);
        var info = xml.Root!.Element(ns + "infNFSe")!;
        info.Element(ns + "nNFSe")!.Value = "987654321";
        info.Element(ns + "valores")!.AddFirst(new XElement(ns + "vBC", "100.00"), new XElement(ns + "pAliqAplic", "3.25"), new XElement(ns + "vISSQN", "3.25"));
        doc.AuthorizedXml = xml.ToString(SaveOptions.DisableFormatting);
        var original = FiscalJson.Required<FiscalSnapshot>(doc.Snapshot);
        doc.Snapshot = FiscalJson.Write(original with { Issuer = original.Issuer with { LegalName = "NOME OBSOLETO", Address = original.Issuer.Address! with { Street = "RUA OBSOLETA" } } });
        using var pdf = PdfDocument.Open(Generate(doc));
        var text = Text(pdf);
        Assert.Equal(1, pdf.NumberOfPages);
        foreach (var expected in new[] { "EMITENTE DO XML", "Rua do Emitente", "TOMADOR DO XML", "Rua do Tomador", "987654321", "3,25%", "100,00", "SIMULAÇÃO LOCAL", "HOMOLOGAÇÃO" }) Assert.Contains(expected, text);
        Assert.Contains("R$ 100,00", text);
        Assert.All(pdf.GetPages().SelectMany(page => page.Letters), letter => Assert.InRange(letter.GlyphRectangle.Right, 0, 581));
        Assert.DoesNotContain("OBSOLETO", text); Assert.DoesNotContain("OBSOLETA", text);
        Assert.Contains(doc.AccessKey!, string.Concat(text.Where(c => !char.IsWhiteSpace(c))));
    }

    [Fact]
    public async Task Danfse_rejects_dps_or_missing_official_number_instead_of_inventing_it()
    {
        var doc = await FiscalPdfFixtures.Authorized(FiscalKind.Nfse);
        var xml = FiscalXml.Parse(doc.AuthorizedXml!);
        xml.Descendants(FiscalXml.Nfse + "nNFSe").Single().Remove();
        doc.AuthorizedXml = xml.ToString();
        Assert.Throws<ConflictException>(() => FiscalPdf.Generate(doc));
        doc.AuthorizedXml = doc.SubmittedXml;
        Assert.Throws<ConflictException>(() => FiscalPdf.Generate(doc));
    }

    [Theory]
    [InlineData(FiscalKind.Nfe, 1)]
    [InlineData(FiscalKind.Nfe, 80)]
    [InlineData(FiscalKind.Nfse, 40)]
    public async Task Pdf_preserves_all_items_across_pages_and_identifies_cancellation(FiscalKind kind, int lines)
    {
        var doc = await FiscalPdfFixtures.Authorized(kind, lines);
        doc.State = FiscalState.Cancelled;
        using var pdf = PdfDocument.Open(Generate(doc));
        var text = Text(pdf);
        Assert.Contains("DOCUMENTO CANCELADO", text);
        Assert.Contains("SIMULAÇÃO LOCAL", text);
        Assert.Contains("Produto001", string.Concat(text.Where(c => !char.IsWhiteSpace(c))));
        Assert.Contains($"Produto{lines:D3}", string.Concat(text.Where(c => !char.IsWhiteSpace(c))));
        Assert.Contains((lines * 100).ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), text);
        if (kind == FiscalKind.Nfe && lines > 1) Assert.True(pdf.NumberOfPages > 1);
        if (lines == 1) Assert.Equal(1, pdf.NumberOfPages);
        Assert.All(pdf.GetPages(), page => { Assert.True(page.Width > 590); Assert.True(page.Height > 840); });
    }
}
