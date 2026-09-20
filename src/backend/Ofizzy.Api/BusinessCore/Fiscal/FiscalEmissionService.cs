using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed class FiscalEmissionService(
    ApplicationDbContext db,
    CurrentTenant tenant,
    FiscalPreparationService preparations,
    FiscalCertificateVault vault,
    IFiscalGateway gateway,
    IConfiguration config)
{
    public async Task Issue(Guid id, CancellationToken ct)
    {
        var prepared = await preparations.Prepare(id, ct)
            ?? throw new ConflictException("OS não encontrada.");

        if (prepared.Issues.Count > 0)
        {
            throw new FluentValidation.ValidationException(
                prepared.Issues.Select(x => new FluentValidation.Results.ValidationFailure(x.Field, x.Message)));
        }

        var snapshot = prepared.Snapshot;
        var kinds = new List<FiscalKind>();

        if (prepared.Order.Parts.Count > 0)
        {
            kinds.Add(FiscalKind.Nfe);
        }

        if (prepared.Order.Services.Count > 0)
        {
            kinds.Add(FiscalKind.Nfse);
        }

        if (kinds.Contains(FiscalKind.Nfe))
        {
            _ = NationalFiscalGateway.NfeEndpoint(snapshot, "authorize");
        }

        using var cert = vault.Load(prepared.Settings!);
        var ids = new List<Guid>();

        await using (var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct))
        {
            foreach (var kind in kinds)
            {
                var existing = await db.FiscalDocumentEntries.SingleOrDefaultAsync(
                    x => x.WorkOrderId == id
                        && x.Kind == kind
                        && x.Environment == snapshot.Issuer.Environment
                        && x.State != FiscalState.Cancelled
                        && x.State != FiscalState.Inutilized,
                    ct);

                if (existing != null)
                {
                    if (existing.State == FiscalState.Rejected)
                    {
                        AddEvent(existing, "PreviousAttempt", existing.SubmittedXml, null, existing.Message);

                        var original = FiscalJson.Required<FiscalSnapshot>(existing.Snapshot);
                        var corrected = ForKind(snapshot, kind) with { IssuedAt = original.IssuedAt };

                        existing.Snapshot = FiscalJson.Write(corrected);
                        existing.Total = kind == FiscalKind.Nfe
                            ? corrected.Lines.Sum(x => FiscalValidation.Money(x.Quantity * x.UnitPrice))
                            : FiscalValidation.Money(corrected.Lines.Sum(x => x.Quantity * x.UnitPrice));
                        existing.SubmittedXml = null;
                        existing.State = FiscalState.Prepared;
                        existing.Version = Guid.NewGuid();
                    }

                    ids.Add(existing.Id);
                    continue;
                }

                var hasCancelledDoc = await db.FiscalDocumentEntries.AnyAsync(
                    x => x.WorkOrderId == id && x.Kind == kind && x.State == FiscalState.Cancelled,
                    ct);

                if (hasCancelledDoc && !tenant.IsAdmin)
                {
                    throw new ConflictException("Uma nova emissão após cancelamento exige Owner/Admin.");
                }

                var series = kind == FiscalKind.Nfe ? snapshot.Issuer.NfeSeries : snapshot.Issuer.DpsSeries;
                var sequence = await db.FiscalSequenceEntries.SingleOrDefaultAsync(
                    x => x.Kind == kind && x.Environment == snapshot.Issuer.Environment && x.Series == series,
                    ct);

                if (sequence == null)
                {
                    sequence = new FiscalSequence
                    {
                        Kind = kind,
                        Environment = snapshot.Issuer.Environment,
                        Series = series
                    };
                    db.FiscalSequenceEntries.Add(sequence);
                }

                sequence.LastNumber++;
                sequence.Version = Guid.NewGuid();

                var maxNumber = kind == FiscalKind.Nfe ? 999999999L : 999999999999999L;
                if (sequence.LastNumber > maxNumber)
                {
                    throw new ConflictException("Numeração fiscal esgotada. Configure nova série.");
                }

                var specific = ForKind(snapshot, kind);
                var doc = new FiscalDocument
                {
                    WorkOrderId = id,
                    Kind = kind,
                    Environment = snapshot.Issuer.Environment,
                    Series = series,
                    Number = sequence.LastNumber,
                    SchemaPackage = FiscalSchemaCatalog.Document(kind).Package,
                    CreatedBy = tenant.UserId!.Value,
                    Snapshot = FiscalJson.Write(specific),
                    Total = kind == FiscalKind.Nfe
                        ? specific.Lines.Sum(x => FiscalValidation.Money(x.Quantity * x.UnitPrice))
                        : FiscalValidation.Money(specific.Lines.Sum(x => x.Quantity * x.UnitPrice))
                };

                doc.Identity = kind == FiscalKind.Nfe
                    ? FiscalXml.NfeKey(specific, series, doc.Number, RandomNumberGenerator.GetInt32(10000000, 100000000))
                    : $"DPS{snapshot.Issuer.Address!.CityCode}2{snapshot.Issuer.Cnpj}{series:D5}{doc.Number:D15}";

                db.FiscalDocumentEntries.Add(doc);
                ids.Add(doc.Id);
            }

            // Validate/sign every applicable document before committing or transmitting either one.
            var preparedDocs = db.ChangeTracker.Entries<FiscalDocument>()
                .Select(x => x.Entity)
                .Where(x => ids.Contains(x.Id) && x.State == FiscalState.Prepared);

            foreach (var doc in preparedDocs)
            {
                var s = FiscalJson.Required<FiscalSnapshot>(doc.Snapshot);
                var xmlTree = doc.Kind == FiscalKind.Nfe
                    ? FiscalXml.Invoice(doc, s)
                    : FiscalXml.Dps(doc, s);
                var rootTag = doc.Kind == FiscalKind.Nfe ? "infNFe" : "infDPS";

                doc.SubmittedXml = FiscalXml.Sign(xmlTree, rootTag, cert);
                FiscalXml.Validate(doc.SubmittedXml, doc.Kind);
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        foreach (var docId in ids)
        {
            await Process(docId, false, null, ct);
        }
    }

    private static FiscalSnapshot ForKind(FiscalSnapshot snapshot, FiscalKind kind)
    {
        return snapshot with
        {
            Lines = snapshot.Lines
                .Where(x => kind == FiscalKind.Nfe ? x.Product != null : x.Service != null)
                .ToList()
        };
    }

    public async Task Process(Guid id, bool queryOnly, string? cancelReason, CancellationToken ct)
    {
        var doc = await db.FiscalDocumentEntries.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ConflictException("Documento não encontrado.");

        if (doc.State is FiscalState.Cancelled or FiscalState.Inutilized || (!queryOnly && cancelReason == null && doc.State == FiscalState.Authorized))
        {
            return;
        }

        await FiscalReleaseGate.EnsureAllowedAsync(db, config, doc.Environment, ct);

        if (doc.Kind == FiscalKind.Nfe)
        {
            var hasPendingInutilization = await db.FiscalInutilizationEntries.AnyAsync(
                x => x.Environment == doc.Environment
                    && x.Series == doc.Series
                    && x.FirstNumber <= doc.Number
                    && x.LastNumber >= doc.Number
                    && x.State == "Pending",
                ct);

            if (hasPendingInutilization)
            {
                throw new ConflictException("Existe uma inutilização pendente para este número. O administrador deve recuperar o protocolo em Configurações > Fiscal.");
            }
        }

        if (doc.LeaseUntil > DateTimeOffset.UtcNow)
        {
            return;
        }

        var settings = await db.FiscalSettingsEntries.SingleAsync(ct);
        using var cert = vault.Load(settings);
        var previous = doc.State;

        if (cancelReason != null && previous != FiscalState.Authorized)
        {
            throw new ConflictException("Consulte a situação antes de solicitar cancelamento.");
        }

        var requestXml = cancelReason == null ? null : CancellationXml(doc, cancelReason, cert);
        if (requestXml != null)
        {
            FiscalXml.ValidateCancellation(requestXml, doc.Kind);
        }

        doc.State = cancelReason != null
            ? FiscalState.CancellationPending
            : (previous == FiscalState.Prepared ? FiscalState.Processing : previous);

        doc.LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(2);
        doc.Version = Guid.NewGuid();
        await db.SaveChangesAsync(ct);

        // From this point a request disconnect must not erase the official result.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(50));
        try
        {
            FiscalGatewayResult result;
            if (cancelReason != null)
            {
                AddEvent(doc, "CancellationRequested", requestXml, null, null);
                await db.SaveChangesAsync(timeout.Token);
                result = await gateway.Cancel(doc, requestXml!, cert, timeout.Token);
            }
            else if (previous == FiscalState.Prepared && !queryOnly)
            {
                result = await gateway.Send(doc, cert, timeout.Token);
            }
            else
            {
                result = await gateway.Query(doc, cert, timeout.Token);
                if (result.NotFound && !queryOnly && previous == FiscalState.AwaitingConfirmation)
                {
                    AddEvent(doc, "NotFoundBeforeResend", null, result.RawResponse, result.Message);
                    await db.SaveChangesAsync(timeout.Token);
                    result = await gateway.Send(doc, cert, timeout.Token);
                }
            }

            // Queries cannot downgrade an issued note or an unknown emission to rejected.
            if ((queryOnly || previous != FiscalState.Prepared) && result.State == FiscalState.Rejected)
            {
                result = result with
                {
                    State = previous is FiscalState.Authorized or FiscalState.CancellationPending
                        ? previous
                        : FiscalState.AwaitingConfirmation
                };
            }

            if (previous == FiscalState.Authorized && result.State is not (FiscalState.Authorized or FiscalState.Cancelled or FiscalState.CancellationPending))
            {
                result = result with { State = FiscalState.Authorized };
            }

            doc.State = result.State;
            doc.AccessKey = result.Key ?? doc.AccessKey;
            doc.Protocol = result.Protocol ?? doc.Protocol;
            doc.Receipt = result.Receipt ?? doc.Receipt;
            doc.AuthorizedXml = result.AuthorizedXml ?? doc.AuthorizedXml;
            doc.Message = result.Message;

            var opName = cancelReason != null
                ? "CancellationResult"
                : (previous == FiscalState.Prepared && !queryOnly ? "Emission" : "Consultation");

            AddEvent(doc, opName, null, result.RawResponse, result.Message);
        }
        catch (Exception e) when (e is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or System.Xml.XmlException or InvalidDataException or FormatException)
        {
            doc.State = cancelReason != null || previous == FiscalState.CancellationPending
                ? FiscalState.CancellationPending
                : (previous == FiscalState.Authorized ? FiscalState.Authorized : FiscalState.AwaitingConfirmation);

            doc.Message = "Comunicação inconclusiva. Atualize a situação antes de qualquer novo envio.";
            AddEvent(doc, "CommunicationPending", null, null, doc.Message);
        }
        finally
        {
            doc.LeaseUntil = null;
            doc.Version = Guid.NewGuid();

            using var persistenceTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await db.SaveChangesAsync(persistenceTimeout.Token);
        }
    }

    private void AddEvent(FiscalDocument document, string operation, string? request, string? response, string? message)
    {
        db.FiscalEventEntries.Add(new FiscalEvent
        {
            DocumentId = document.Id,
            Operation = operation,
            RequestXml = request,
            ResponseXml = response,
            Message = message,
            UserId = tenant.UserId!.Value
        });
    }

    public static string CancellationXml(FiscalDocument d, string reason, X509Certificate2 certificate)
    {
        var s = FiscalJson.Required<FiscalSnapshot>(d.Snapshot);
        var now = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        if (d.Kind == FiscalKind.Nfe)
        {
            var ns = FiscalXml.Nfe;
            XElement E(string n, params object?[] c) => new(ns + n, c);

            var evt = new XDocument(
                E("evento", new XAttribute("versao", "1.00"),
                    E("infEvento", new XAttribute("Id", $"ID110111{d.Identity}01"),
                        E("cOrgao", FiscalValidation.StateCodes[s.Issuer.Address!.State]),
                        E("tpAmb", (int)d.Environment),
                        E("CNPJ", s.Issuer.Cnpj),
                        E("chNFe", d.Identity),
                        E("dhEvento", now),
                        E("tpEvento", "110111"),
                        E("nSeqEvento", 1),
                        E("verEvento", "1.00"),
                        E("detEvento", new XAttribute("versao", "1.00"),
                            E("descEvento", "Cancelamento"),
                            E("nProt", d.Protocol),
                            E("xJust", reason)))));

            var signed = FiscalXml.Sign(evt, "infEvento", certificate);
            return E("envEvento",
                new XAttribute("versao", "1.00"),
                E("idLote", d.Number),
                FiscalXml.Parse(signed).Root).ToString(SaveOptions.DisableFormatting);
        }
        else
        {
            var ns = FiscalXml.Nfse;
            XElement E(string n, params object?[] c) => new(ns + n, c);

            var evt = new XDocument(
                E("pedRegEvento", new XAttribute("versao", "1.01"),
                    E("infPedReg", new XAttribute("Id", $"PRE{d.AccessKey}101101"),
                        E("tpAmb", (int)d.Environment),
                        E("verAplic", "Ofizzy_1.0"),
                        E("dhEvento", now),
                        E("CNPJAutor", s.Issuer.Cnpj),
                        E("chNFSe", d.AccessKey),
                        E("e101101",
                            E("xDesc", "Cancelamento de NFS-e"),
                            E("cMotivo", "9"),
                            E("xMotivo", reason)))));

            return FiscalXml.Sign(evt, "infPedReg", certificate);
        }
    }
}
