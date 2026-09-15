using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed class FiscalEmissionService(ApplicationDbContext db, CurrentTenant tenant, FiscalPreparationService preparations,
    FiscalCertificateVault vault, IFiscalGateway gateway, IConfiguration config)
{
    public async Task Issue(Guid id, CancellationToken ct)
    {
        var prepared = await preparations.Prepare(id,ct) ?? throw new ConflictException("OS não encontrada.");
        if(prepared.Issues.Count>0) throw new FluentValidation.ValidationException(prepared.Issues.Select(x=>new FluentValidation.Results.ValidationFailure(x.Field,x.Message)));
        var snapshot=prepared.Snapshot;
        var kinds=new List<FiscalKind>();if(prepared.Order.Parts.Count>0) kinds.Add(FiscalKind.Nfe);if(prepared.Order.Services.Count>0) kinds.Add(FiscalKind.Nfse);
        if(kinds.Contains(FiscalKind.Nfe)) _=NationalFiscalGateway.NfeEndpoint(snapshot,"authorize");
        using var cert=vault.Load(prepared.Settings!);
        var ids=new List<Guid>();
        await using(var transaction=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct))
        {
            foreach(var kind in kinds)
            {
                var existing=await db.FiscalDocumentEntries.SingleOrDefaultAsync(x=>x.WorkOrderId==id&&x.Kind==kind&&x.Environment==snapshot.Issuer.Environment&&x.State!=FiscalState.Cancelled&&x.State!=FiscalState.Inutilized,ct);
                if(existing!=null)
                {
                    if(existing.State==FiscalState.Rejected)
                    {
                        AddEvent(existing,"PreviousAttempt",existing.SubmittedXml,null,existing.Message);
                        var original = FiscalJson.Required<FiscalSnapshot>(existing.Snapshot);
                        var corrected = ForKind(snapshot,kind) with { IssuedAt = original.IssuedAt };
                        existing.Snapshot=FiscalJson.Write(corrected);
                        existing.Total=kind==FiscalKind.Nfe?corrected.Lines.Sum(x=>FiscalValidation.Money(x.Quantity*x.UnitPrice)):FiscalValidation.Money(corrected.Lines.Sum(x=>x.Quantity*x.UnitPrice));
                        existing.SubmittedXml=null;existing.State=FiscalState.Prepared;existing.Version=Guid.NewGuid();
                    }
                    ids.Add(existing.Id);continue;
                }
                if(await db.FiscalDocumentEntries.AnyAsync(x=>x.WorkOrderId==id&&x.Kind==kind&&x.State==FiscalState.Cancelled,ct)&&!tenant.IsAdmin)
                    throw new ConflictException("Uma nova emissão após cancelamento exige Owner/Admin.");
                var series=kind==FiscalKind.Nfe?snapshot.Issuer.NfeSeries:snapshot.Issuer.DpsSeries;
                var sequence=await db.FiscalSequenceEntries.SingleOrDefaultAsync(x=>x.Kind==kind&&x.Environment==snapshot.Issuer.Environment&&x.Series==series,ct);
                if(sequence==null) { sequence=new(){Kind=kind,Environment=snapshot.Issuer.Environment,Series=series};db.FiscalSequenceEntries.Add(sequence); }
                sequence.LastNumber++;sequence.Version=Guid.NewGuid();
                if(sequence.LastNumber>(kind==FiscalKind.Nfe?999999999L:999999999999999L)) throw new ConflictException("Numeração fiscal esgotada. Configure nova série.");
                var specific=ForKind(snapshot,kind);
                var d=new FiscalDocument{WorkOrderId=id,Kind=kind,Environment=snapshot.Issuer.Environment,Series=series,Number=sequence.LastNumber,CreatedBy=tenant.UserId!.Value,Snapshot=FiscalJson.Write(specific),Total=kind==FiscalKind.Nfe?specific.Lines.Sum(x=>FiscalValidation.Money(x.Quantity*x.UnitPrice)):FiscalValidation.Money(specific.Lines.Sum(x=>x.Quantity*x.UnitPrice))};
                d.Identity=kind==FiscalKind.Nfe?FiscalXml.NfeKey(specific,series,d.Number,RandomNumberGenerator.GetInt32(10000000,100000000)):$"DPS{snapshot.Issuer.Address!.CityCode}2{snapshot.Issuer.Cnpj}{series:D5}{d.Number:D15}";
                db.FiscalDocumentEntries.Add(d);ids.Add(d.Id);
            }
            // Validate/sign every applicable document before committing or transmitting either one.
            foreach(var d in db.ChangeTracker.Entries<FiscalDocument>().Select(x=>x.Entity).Where(x=>ids.Contains(x.Id)&&x.State==FiscalState.Prepared))
            {
                var s=FiscalJson.Required<FiscalSnapshot>(d.Snapshot);
                d.SubmittedXml=FiscalXml.Sign(d.Kind==FiscalKind.Nfe?FiscalXml.Invoice(d,s):FiscalXml.Dps(d,s),d.Kind==FiscalKind.Nfe?"infNFe":"infDPS",cert);
                FiscalXml.Validate(d.SubmittedXml,d.Kind);
            }
            await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);
        }
        foreach(var docId in ids) await Process(docId,false,null,ct);
    }
    private static FiscalSnapshot ForKind(FiscalSnapshot snapshot,FiscalKind kind) => snapshot with { Lines=snapshot.Lines.Where(x=>kind==FiscalKind.Nfe?x.Product!=null:x.Service!=null).ToList() };
    public async Task Process(Guid id,bool queryOnly,string? cancelReason,CancellationToken ct)
    {
        var d=await db.FiscalDocumentEntries.SingleOrDefaultAsync(x=>x.Id==id,ct) ?? throw new ConflictException("Documento não encontrado.");
        if(d.State is FiscalState.Cancelled or FiscalState.Inutilized || (!queryOnly&&cancelReason==null&&d.State==FiscalState.Authorized)) return;
        await FiscalReleaseGate.EnsureAllowedAsync(db, config, d.Environment, ct);
        if(d.Kind==FiscalKind.Nfe && await db.FiscalInutilizationEntries.AnyAsync(x=>x.Environment==d.Environment&&x.Series==d.Series&&x.FirstNumber<=d.Number&&x.LastNumber>=d.Number&&x.State=="Pending",ct))
            throw new ConflictException("Existe uma inutilização pendente para este número. O administrador deve recuperar o protocolo em Configurações > Fiscal.");
        if(d.LeaseUntil>DateTimeOffset.UtcNow) return;
        var settings=await db.FiscalSettingsEntries.SingleAsync(ct);
        using var cert=vault.Load(settings);
        var previous=d.State;
        if(cancelReason!=null && previous!=FiscalState.Authorized) throw new ConflictException("Consulte a situação antes de solicitar cancelamento.");
        var requestXml = cancelReason == null ? null : CancellationXml(d,cancelReason,cert);
        if(requestXml != null) FiscalXml.ValidateCancellation(requestXml,d.Kind);
        d.State=cancelReason!=null?FiscalState.CancellationPending:previous==FiscalState.Prepared?FiscalState.Processing:previous;
        d.LeaseUntil=DateTimeOffset.UtcNow.AddMinutes(2);d.Version=Guid.NewGuid();await db.SaveChangesAsync(ct);
        // From this point a request disconnect must not erase the official result.
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(50));
        try
        {
            FiscalGatewayResult result;
            if(cancelReason!=null)
            {
                AddEvent(d,"CancellationRequested",requestXml,null,null);await db.SaveChangesAsync(timeout.Token);
                result=await gateway.Cancel(d,requestXml!,cert,timeout.Token);
            }
            else if(previous==FiscalState.Prepared&&!queryOnly) result=await gateway.Send(d,cert,timeout.Token);
            else
            {
                result=await gateway.Query(d,cert,timeout.Token);
                if(result.NotFound && !queryOnly && previous==FiscalState.AwaitingConfirmation)
                {
                    AddEvent(d,"NotFoundBeforeResend",null,result.RawResponse,result.Message);
                    await db.SaveChangesAsync(timeout.Token);
                    result=await gateway.Send(d,cert,timeout.Token);
                }
            }
            // Queries cannot downgrade an issued note or an unknown emission to rejected.
            if((queryOnly||previous!=FiscalState.Prepared)&&result.State==FiscalState.Rejected)
                result=result with {State=previous is FiscalState.Authorized or FiscalState.CancellationPending?previous:FiscalState.AwaitingConfirmation};
            if(previous==FiscalState.Authorized && result.State is not (FiscalState.Authorized or FiscalState.Cancelled or FiscalState.CancellationPending))
                result=result with {State=FiscalState.Authorized};
            d.State=result.State;d.AccessKey=result.Key??d.AccessKey;d.Protocol=result.Protocol??d.Protocol;d.Receipt=result.Receipt??d.Receipt;
            d.AuthorizedXml=result.AuthorizedXml??d.AuthorizedXml;d.Message=result.Message;
            AddEvent(d,cancelReason!=null?"CancellationResult":previous==FiscalState.Prepared&&!queryOnly?"Emission":"Consultation",null,result.RawResponse,result.Message);
        }
        catch(Exception e) when(e is HttpRequestException or OperationCanceledException or System.Text.Json.JsonException or System.Xml.XmlException or InvalidDataException or FormatException)
        {
            d.State=cancelReason!=null||previous==FiscalState.CancellationPending?FiscalState.CancellationPending:previous==FiscalState.Authorized?FiscalState.Authorized:FiscalState.AwaitingConfirmation;
            d.Message="Comunicação inconclusiva. Atualize a situação antes de qualquer novo envio.";
            AddEvent(d,"CommunicationPending",null,null,d.Message);
        }
        finally
        {
            d.LeaseUntil=null;d.Version=Guid.NewGuid();
            using var persistenceTimeout=new CancellationTokenSource(TimeSpan.FromSeconds(10));await db.SaveChangesAsync(persistenceTimeout.Token);
        }
    }
    private void AddEvent(FiscalDocument document,string operation,string? request,string? response,string? message) => db.FiscalEventEntries.Add(new(){DocumentId=document.Id,Operation=operation,RequestXml=request,ResponseXml=response,Message=message,UserId=tenant.UserId!.Value});
    public static string CancellationXml(FiscalDocument d,string reason,System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
    {
        var s=FiscalJson.Required<FiscalSnapshot>(d.Snapshot);
        var now=DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz",System.Globalization.CultureInfo.InvariantCulture);
        if(d.Kind==FiscalKind.Nfe)
        {
            var ns=FiscalXml.Nfe;XElement E(string n,params object?[] c)=>new(ns+n,c);
            var evt=new XDocument(E("evento",new XAttribute("versao","1.00"),E("infEvento",new XAttribute("Id","ID110111"+d.Identity+"01"),E("cOrgao",FiscalValidation.StateCodes[s.Issuer.Address!.State]),E("tpAmb",(int)d.Environment),E("CNPJ",s.Issuer.Cnpj),E("chNFe",d.Identity),E("dhEvento",now),E("tpEvento","110111"),E("nSeqEvento",1),E("verEvento","1.00"),E("detEvento",new XAttribute("versao","1.00"),E("descEvento","Cancelamento"),E("nProt",d.Protocol),E("xJust",reason)))));
            return E("envEvento",new XAttribute("versao","1.00"),E("idLote",d.Number),FiscalXml.Parse(FiscalXml.Sign(evt,"infEvento",certificate)).Root).ToString(SaveOptions.DisableFormatting);
        }
        else
        {
            var ns=FiscalXml.Nfse;XElement E(string n,params object?[] c)=>new(ns+n,c);
            var evt=new XDocument(E("pedRegEvento",new XAttribute("versao","1.01"),E("infPedReg",new XAttribute("Id","PRE"+d.AccessKey+"101101"),E("tpAmb",(int)d.Environment),E("verAplic","Ofizzy_1.0"),E("dhEvento",now),E("CNPJAutor",s.Issuer.Cnpj),E("chNFSe",d.AccessKey),E("e101101",E("xDesc","Cancelamento de NFS-e"),E("cMotivo","9"),E("xMotivo",reason)))));
            return FiscalXml.Sign(evt,"infPedReg",certificate);
        }
    }
}
