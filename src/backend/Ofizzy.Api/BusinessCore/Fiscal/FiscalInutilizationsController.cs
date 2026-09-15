using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.Api.Modules.Fiscal;
[Authorize,ApiController,Route("api/fiscal/nfe/inutilizations"),TenantAccess(Admin=true,Modules=new[]{ProductModule.WorkOrders,ProductModule.Catalog,ProductModule.Customers,ProductModule.Automotive})]
public sealed class FiscalInutilizationsController(ApplicationDbContext db,CurrentTenant tenant,FiscalCertificateVault vault,IFiscalGateway gateway,IConfiguration config) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.FiscalInutilizationEntries.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(100).Select(x=>new{x.Id,x.Series,x.Year,x.FirstNumber,x.LastNumber,x.Environment,x.State,x.Protocol,x.Message,x.CreatedAt}).ToListAsync(ct));
    [HttpPost]
    public async Task<IActionResult> Create(InutilizationRequest request,CancellationToken ct)
    {
        if(request.Series is <1 or >889 || request.FirstNumber<1 || request.LastNumber>999999999 || request.LastNumber<request.FirstNumber || request.LastNumber-request.FirstNumber>=100 || request.Year is <2000 or >2099 || request.Reason?.Trim().Length is not (>=15 and <=255)) throw new ConflictException("Informe série, ano, intervalo de até 100 números e justificativa de 15 a 255 caracteres.");
        var settings=await db.FiscalSettingsEntries.SingleOrDefaultAsync(ct)??throw new ConflictException("Configure os dados fiscais.");
        var issuer=FiscalJson.Required<FiscalSettingsData>(settings.Data);
        await FiscalReleaseGate.EnsureAllowedAsync(db, config, issuer.Environment, ct);
        // Only numbers reserved by Ofizzy and definitively rejected may be invalidated.
        var docs=await db.FiscalDocumentEntries.Where(x=>x.Kind==FiscalKind.Nfe&&x.Environment==issuer.Environment&&x.Series==request.Series&&x.Number>=request.FirstNumber&&x.Number<=request.LastNumber).ToListAsync(ct);
        if(docs.Count!=request.LastNumber-request.FirstNumber+1 || docs.Any(x=>x.State!=FiscalState.Rejected || x.AuthorizedXml!=null || FiscalJson.Required<FiscalSnapshot>(x.Snapshot).IssuedAt.Year!=request.Year))
            throw new ConflictException("O intervalo deve conter somente números reservados neste sistema e rejeitados, sem autorização ou resultado inconclusivo.");
        using var cert=vault.Load(settings);
        foreach(var d in docs)
        {
            var result=await gateway.Query(d,cert,ct);
            if(!result.NotFound)throw new ConflictException("A SEFAZ não confirmou ausência de todos os números. Consulte os documentos antes de inutilizar.");
        }
        var record=new FiscalInutilization{Environment=issuer.Environment,Series=request.Series,Year=request.Year,FirstNumber=request.FirstNumber,LastNumber=request.LastNumber,Reason=request.Reason.Trim(),UserId=tenant.UserId!.Value};
        var ns=FiscalXml.Nfe;XElement E(string n,params object?[] c)=>new(ns+n,c);
        var identity=$"ID{FiscalValidation.StateCodes[issuer.Address!.State]}{request.Year%100:D2}{issuer.Cnpj}55{request.Series:D3}{request.FirstNumber:D9}{request.LastNumber:D9}";
        var xml=new XDocument(E("inutNFe",new XAttribute("versao","4.00"),E("infInut",new XAttribute("Id",identity),E("tpAmb",(int)issuer.Environment),E("xServ","INUTILIZAR"),E("cUF",FiscalValidation.StateCodes[issuer.Address.State]),E("ano",(request.Year%100).ToString("D2")),E("CNPJ",issuer.Cnpj),E("mod",55),E("serie",request.Series),E("nNFIni",request.FirstNumber),E("nNFFin",request.LastNumber),E("xJust",record.Reason))));
        record.RequestXml=FiscalXml.Sign(xml,"infInut",cert);
        FiscalXml.ValidateInutilization(record.RequestXml);
        if(docs.Any(d => FiscalJson.Required<FiscalSnapshot>(d.Snapshot).Issuer.Address?.State != issuer.Address.State))
            throw new ConflictException("A UF do emitente deve corresponder ao histórico da numeração.");
        await using(var transaction=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct))
        {
            if(await db.FiscalInutilizationEntries.AnyAsync(x=>x.Environment==issuer.Environment&&x.Series==request.Series&&x.Year==request.Year&&x.FirstNumber<=request.LastNumber&&x.LastNumber>=request.FirstNumber&&x.State!="Rejected",ct))throw new ConflictException("Já existe um pedido para esse intervalo. Consulte o histórico.");
            foreach(var d in docs){d.State=FiscalState.AwaitingConfirmation;d.Message="Inutilização pendente; não reenviar.";d.Version=Guid.NewGuid();}
            db.FiscalInutilizationEntries.Add(record);await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);
        }
        return await Process(record, false, ct);
    }
    [HttpPost("{id:guid}/sync")]
    public async Task<IActionResult> Sync(Guid id, CancellationToken ct)
    {
        var record = await db.FiscalInutilizationEntries.SingleOrDefaultAsync(x => x.Id == id, ct);
        if(record == null) return NotFound();
        return await Process(record, true, ct);
    }
    private async Task<IActionResult> Process(FiscalInutilization record, bool recovery, CancellationToken ct)
    {
        await FiscalReleaseGate.EnsureAllowedAsync(db, config, record.Environment, ct);
        if(record.State == "Confirmed") return NoContent();
        if(record.State == "Rejected") throw new ConflictException("Pedido rejeitado. Revise a justificativa e solicite novamente pelo formulário.");
        if(record.LeaseUntil > DateTimeOffset.UtcNow) throw new ConflictException("Uma consulta desta inutilização já está em andamento. Aguarde antes de atualizar.");
        if(string.IsNullOrWhiteSpace(record.RequestXml)) throw new ConflictException("Pedido fiscal original não encontrado. Não reutilize os números.");
        FiscalXml.ValidateInutilization(record.RequestXml);
        var docs = await db.FiscalDocumentEntries.Where(x => x.Kind == FiscalKind.Nfe && x.Environment == record.Environment && x.Series == record.Series && x.Number >= record.FirstNumber && x.Number <= record.LastNumber).ToListAsync(ct);
        if(docs.Count != record.LastNumber - record.FirstNumber + 1 || docs.Any(x => x.AuthorizedXml != null || x.State != FiscalState.AwaitingConfirmation))
            throw new ConflictException("O histórico dos documentos não permite recuperar esta inutilização automaticamente.");
        var settings = await db.FiscalSettingsEntries.SingleAsync(ct);
        using var cert = vault.Load(settings);
        record.LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(2); record.Version = Guid.NewGuid();
        db.FiscalEventEntries.Add(new() { DocumentId = docs[0].Id, Operation = "InutilizationRequested", UserId = tenant.UserId!.Value, Message = record.Id.ToString() });
        await db.SaveChangesAsync(ct);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        try
        {
            var result = await gateway.Inutilize(FiscalJson.Required<FiscalSnapshot>(docs[0].Snapshot), record.RequestXml, cert, timeout.Token);
            record.ResponseXml = result.RawResponse; record.Protocol = result.Protocol; record.Message = result.Message;
            // After an uncertain attempt only a matching protocol can release the reservation.
            record.State = result.State == FiscalState.Inutilized ? "Confirmed" : result.State == FiscalState.Rejected && !recovery ? "Rejected" : "Pending";
            foreach(var d in docs)
            {
                d.State = record.State == "Confirmed" ? FiscalState.Inutilized : record.State == "Rejected" ? FiscalState.Rejected : FiscalState.AwaitingConfirmation;
                d.Message = record.Message; d.Version = Guid.NewGuid();
            }
            db.FiscalEventEntries.Add(new() { DocumentId = docs[0].Id, Operation = "InutilizationResult", UserId = tenant.UserId!.Value, ResponseXml = result.RawResponse, Message = record.Id + ": " + result.Message });
        }
        catch(Exception e) when(e is HttpRequestException or OperationCanceledException or System.Xml.XmlException or FormatException or InvalidDataException)
        {
            record.Message = "Resultado inconclusivo. Atualize o pedido para recuperar o protocolo; os números permanecem reservados.";
            db.FiscalEventEntries.Add(new() { DocumentId = docs[0].Id, Operation = "InutilizationPending", UserId = tenant.UserId!.Value, Message = record.Id + ": " + record.Message });
        }
        finally
        {
            record.LeaseUntil = null; record.Version = Guid.NewGuid();
            using var saveTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await db.SaveChangesAsync(saveTimeout.Token);
        }
        return Accepted(new { record.Id, record.State, record.Message });
    }
}
