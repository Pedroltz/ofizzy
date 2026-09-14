using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed record FiscalGatewayResult(FiscalState State, string? Key = null, string? Protocol = null,
    string? Receipt = null, string? AuthorizedXml = null, string? RawResponse = null, string? Message = null, bool NotFound = false);
public interface IFiscalGateway
{
    Task<FiscalGatewayResult> Send(FiscalDocument document, X509Certificate2 certificate, CancellationToken ct);
    Task<FiscalGatewayResult> Query(FiscalDocument document, X509Certificate2 certificate, CancellationToken ct);
    Task<FiscalGatewayResult> Cancel(FiscalDocument document, string signedEvent, X509Certificate2 certificate, CancellationToken ct);
    Task<FiscalGatewayResult> Inutilize(FiscalSnapshot snapshot, string signedXml, X509Certificate2 certificate, CancellationToken ct);
}
public sealed class NationalFiscalGateway : IFiscalGateway
{
    private static HttpClient Client(X509Certificate2 certificate)
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false, CheckCertificateRevocationList = true };
        handler.ClientCertificates.Add(certificate);
        return new(handler) { Timeout = TimeSpan.FromSeconds(20) };
    }
    public static string NfseBase(FiscalEnvironment environment) => environment == FiscalEnvironment.Production
        ? "https://sefin.nfse.gov.br/SefinNacional" : "https://sefin.producaorestrita.nfse.gov.br/SefinNacional";
    public static string NfeEndpoint(FiscalSnapshot snapshot, string operation)
    {
        var state = snapshot.Issuer.Address!.State;
        var production = snapshot.Issuer.Environment == FiscalEnvironment.Production;
        if (state == "SP")
        {
            var service = operation switch { "authorize" => "nfeautorizacao4", "receipt" => "nferetautorizacao4", "query" => "nfeconsultaprotocolo4", "event" => "nferecepcaoevento4", "void" => "nfeinutilizacao4", _ => throw new ArgumentException(nameof(operation)) };
            return $"https://{(production ? "nfe" : "homologacao.nfe")}.fazenda.sp.gov.br/ws/{service}.asmx";
        }
        // Explicit normal authorization services, never contingency routing.
        if (!new[]{"AC","AL","AP","DF","ES","PA","PB","RJ","RN","RO","RR","SC","SE","TO"}.Contains(state))
            throw new ConflictException("A UF ainda não possui integração NF-e habilitada nesta versão. É necessária homologação do autorizador estadual.");
        var path = operation switch { "authorize" => "NfeAutorizacao/NFeAutorizacao4", "receipt" => "NfeRetAutorizacao/NFeRetAutorizacao4", "query" => "NfeConsulta/NfeConsulta4", "event" => "recepcaoevento/recepcaoevento4", "void" => "nfeinutilizacao/nfeinutilizacao4", _ => throw new ArgumentException(nameof(operation)) };
        return $"https://{(production ? "nfe" : "nfe-homologacao")}.svrs.rs.gov.br/ws/{path}.asmx";
    }
    public async Task<FiscalGatewayResult> Send(FiscalDocument document, X509Certificate2 certificate, CancellationToken ct)
    {
        using var client = Client(certificate);
        if (document.Kind == FiscalKind.Nfse)
        {
            using var response = await client.PostAsJsonAsync(NfseBase(document.Environment)+"/nfse", new { dpsXmlGZipB64 = Compress(document.SubmittedXml!) }, ct);
            return await ReadNfse(response, document, ct);
        }
        var ns=FiscalXml.Nfe;
        var xml = new XElement(ns+"enviNFe",new XAttribute("versao","4.00"),new XElement(ns+"idLote",document.Number),new XElement(ns+"indSinc",1),FiscalXml.Parse(document.SubmittedXml!).Root);
        return await Soap(client, document, "authorize", "NFeAutorizacao4", "nfeAutorizacaoLote", xml, ct);
    }
    public async Task<FiscalGatewayResult> Query(FiscalDocument d, X509Certificate2 certificate, CancellationToken ct)
    {
        using var client=Client(certificate);
        if(d.Kind==FiscalKind.Nfse)
        {
            var key=d.AccessKey;
            if(key==null)
            {
                using var response=await client.GetAsync(NfseBase(d.Environment)+"/dps/"+Uri.EscapeDataString(d.Identity),ct);
                if(response.StatusCode==HttpStatusCode.NotFound) return new(FiscalState.AwaitingConfirmation,NotFound:true,Message:"DPS ainda não localizada no emissor.");
                if(!response.IsSuccessStatusCode) return new(FiscalState.AwaitingConfirmation,Message:"Não foi possível confirmar a DPS no emissor nacional.");
                using var json=JsonDocument.Parse(await ReadBounded(response,ct));
                key=GetString(json.RootElement,"chaveAcesso");
                if(key==null) return new(FiscalState.AwaitingConfirmation,Message:"Emissor ainda não retornou a chave da NFS-e.");
            }
            using var note=await client.GetAsync(NfseBase(d.Environment)+"/nfse/"+Uri.EscapeDataString(key),ct);
            var result=await ReadNfse(note,d,ct);
            if(result.State!=FiscalState.Authorized) return result;
            using var events=await client.GetAsync(NfseBase(d.Environment)+"/nfse/"+Uri.EscapeDataString(key)+"/eventos/101101",ct);
            if(events.IsSuccessStatusCode)
            {
                var body=await ReadBounded(events,ct);
                using var json=JsonDocument.Parse(body);
                if(HasRegisteredCancellation(json.RootElement)) return result with {State=FiscalState.Cancelled,RawResponse=body,Message="Cancelamento confirmado no emissor nacional."};
            }
            return result;
        }
        var ns=FiscalXml.Nfe;
        if(d.Receipt!=null && d.AuthorizedXml==null)
            return await Soap(client,d,"receipt","NFeRetAutorizacao4","nfeRetAutorizacaoLote",new XElement(ns+"consReciNFe",new XAttribute("versao","4.00"),new XElement(ns+"tpAmb",(int)d.Environment),new XElement(ns+"nRec",d.Receipt)),ct);
        return await Soap(client,d,"query","NFeConsultaProtocolo4","nfeConsultaNF",new XElement(ns+"consSitNFe",new XAttribute("versao","4.00"),new XElement(ns+"tpAmb",(int)d.Environment),new XElement(ns+"xServ","CONSULTAR"),new XElement(ns+"chNFe",d.Identity)),ct);
    }
    public async Task<FiscalGatewayResult> Cancel(FiscalDocument d,string signedEvent,X509Certificate2 certificate,CancellationToken ct)
    {
        using var client=Client(certificate);
        if(d.Kind==FiscalKind.Nfse)
        {
            using var response=await client.PostAsJsonAsync(NfseBase(d.Environment)+"/nfse/"+Uri.EscapeDataString(d.AccessKey!)+"/eventos",new { pedidoRegistroEventoXmlGZipB64=Compress(signedEvent) },ct);
            var body=await ReadBounded(response,ct);
            if(response.IsSuccessStatusCode)
            {
                using var json=JsonDocument.Parse(body);
                if(HasRegisteredCancellation(json.RootElement)) return new(FiscalState.Cancelled,RawResponse:body,Message:"Cancelamento confirmado.");
                return new(FiscalState.CancellationPending,RawResponse:body,Message:"Pedido enviado. Aguardando confirmação do cancelamento.");
            }
            return new(response.StatusCode >= HttpStatusCode.InternalServerError?FiscalState.CancellationPending:FiscalState.Authorized,RawResponse:body,Message:"Cancelamento não confirmado pelo emissor. Consulte a situação.");
        }
        return await Soap(client,d,"event","NFeRecepcaoEvento4","nfeRecepcaoEvento",FiscalXml.Parse(signedEvent).Root!,ct);
    }
    internal static bool HasRegisteredCancellation(JsonElement json)
    {
        if(json.ValueKind==JsonValueKind.Object)
        {
            foreach(var property in json.EnumerateObject())
            {
                if(property.Name.Contains("XmlGZipB64",StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind==JsonValueKind.String)
                {
                    var xml=FiscalXml.Parse(Decompress(property.Value.GetString()!));
                    if(xml.Descendants().Any(x=>x.Name.LocalName=="e101101") && xml.Descendants().Any(x=>x.Name.LocalName=="nSeqEvento")) return true;
                }
                if(HasRegisteredCancellation(property.Value)) return true;
            }
        }
        else if(json.ValueKind==JsonValueKind.Array) foreach(var item in json.EnumerateArray()) if(HasRegisteredCancellation(item)) return true;
        return false;
    }
    private static async Task<FiscalGatewayResult> Soap(HttpClient client,FiscalDocument d,string operation,string service,string method,XElement payload,CancellationToken ct)
    {
        var snapshot=FiscalJson.Required<FiscalSnapshot>(d.Snapshot);
        XNamespace soap="http://www.w3.org/2003/05/soap-envelope";
        XNamespace ws="http://www.portalfiscal.inf.br/nfe/wsdl/"+service;
        var envelope=new XDocument(new XElement(soap+"Envelope",new XElement(soap+"Body",new XElement(ws+"nfeDadosMsg",payload))));
        using var request=new HttpRequestMessage(HttpMethod.Post,NfeEndpoint(snapshot,operation));
        request.Content=new StringContent(envelope.ToString(SaveOptions.DisableFormatting),Encoding.UTF8,"application/soap+xml");
        request.Content.Headers.ContentType!.Parameters.Add(new("action",'"'+ws.NamespaceName+"/"+method+'"'));
        using var response=await client.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,ct);
        var body=await ReadBounded(response,ct);
        if(!response.IsSuccessStatusCode) return new(operation=="event"?FiscalState.CancellationPending:FiscalState.AwaitingConfirmation,RawResponse:body,Message:"SEFAZ indisponível. A situação será consultada antes de novo envio.");
        return ParseNfe(body,d,operation);
    }
    public static FiscalGatewayResult ParseNfe(string body,FiscalDocument d,string operation)
    {
        var xml=FiscalXml.Parse(body);var ns=FiscalXml.Nfe;
        var info=xml.Descendants(ns+"infProt").FirstOrDefault();
        var root=xml.Descendants().FirstOrDefault(x=>x.Name.LocalName.StartsWith("ret",StringComparison.Ordinal));
        var code=(string?)info?.Element(ns+"cStat") ?? (string?)root?.Element(ns+"cStat");
        if(operation=="void")
        {
            var result=xml.Descendants(ns+"infInut").FirstOrDefault();
            var status=result?.Element(ns+"cStat")?.Value;
            var expected = FiscalXml.Parse(d.SubmittedXml!).Descendants(ns+"infInut").Single();
            var protocol = result?.Element(ns+"nProt")?.Value;
            var matches = result != null && expected.Element(ns+"CNPJ")?.Value == result.Element(ns+"CNPJ")?.Value && new[]{"tpAmb","cUF","ano","mod","serie","nNFIni","nNFFin"}.All(field =>
                decimal.TryParse(expected.Element(ns+field)?.Value, out var value) && decimal.TryParse(result.Element(ns+field)?.Value, out var actual) && value == actual);
            if(status is "102" or "563")
                return matches && System.Text.RegularExpressions.Regex.IsMatch(protocol??"", "^[0-9]{15,17}$")
                    ? new(FiscalState.Inutilized,Protocol:protocol,RawResponse:body,Message:"Numeração inutilizada; protocolo confirmado pela SEFAZ.")
                    : new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"A resposta da inutilização não confirma o intervalo e protocolo esperados.");
            return new(status==null?FiscalState.AwaitingConfirmation:FiscalState.Rejected,RawResponse:body,Message:"Inutilização não confirmada. Código: "+status);
        }
        if(operation=="event")
        {
            var evt=xml.Descendants(ns+"infEvento").FirstOrDefault();
            var status=(string?)evt?.Element(ns+"cStat");
            if(status is "135" or "155" && ((string?)evt?.Element(ns+"chNFe")!=d.Identity || (string?)evt?.Element(ns+"tpEvento")!="110111"))
                return new(FiscalState.CancellationPending,RawResponse:body,Message:"O evento retornado não corresponde ao cancelamento solicitado.");
            return new(status is "135" or "155"?FiscalState.Cancelled:status==null?FiscalState.CancellationPending:FiscalState.Authorized,RawResponse:body,Message:status is "135" or "155"?"Cancelamento confirmado.":"SEFAZ não confirmou o cancelamento. Código: "+status);
        }
        if(code is "100" or "150")
        {
            if((string?)info?.Element(ns+"chNFe")!=d.Identity) return new(FiscalState.AwaitingConfirmation,Message:"A resposta fiscal não corresponde à chave enviada.");
            var submitted = FiscalXml.Parse(d.SubmittedXml!);
            var digest = submitted.Descendants(XName.Get("DigestValue", "http://www.w3.org/2000/09/xmldsig#")).SingleOrDefault()?.Value;
            if(string.IsNullOrWhiteSpace((string?)info?.Element(ns+"nProt")) || digest == null || (string?)info?.Element(ns+"digVal") != digest)
                return new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"O protocolo não confirma o conteúdo do XML armazenado. Reconcilie o documento com a SEFAZ.");
            var processed=new XElement(ns+"nfeProc",new XAttribute("versao","4.00"),submitted.Root,info!.Parent);
            return new(FiscalState.Authorized,d.Identity,(string?)info.Element(ns+"nProt"),AuthorizedXml:processed.ToString(SaveOptions.DisableFormatting),RawResponse:body,Message:"NF-e autorizada.");
        }
        if(code is "101" or "151")
            return (string?)root?.Element(ns+"chNFe")==d.Identity
                ? new(FiscalState.Cancelled,RawResponse:body,Message:"Cancelamento confirmado na SEFAZ.")
                : new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"A consulta retornada não corresponde à chave solicitada.");
        if(code=="217") return new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"NF-e ainda não localizada na SEFAZ.",NotFound:true);
        if(code is "103" or "105" or "106" or "108" or "109" or "204" or "539" || code==null)
            return new(FiscalState.AwaitingConfirmation,Receipt:xml.Descendants(ns+"nRec").FirstOrDefault()?.Value,RawResponse:body,Message:"Aguardando confirmação da SEFAZ. Código: "+code);
        return new(FiscalState.Rejected,RawResponse:body,Message:"NF-e rejeitada pela SEFAZ. Código: "+code+". Revise os dados fiscais antes de tentar novamente.");
    }
    private static async Task<FiscalGatewayResult> ReadNfse(HttpResponseMessage response,FiscalDocument d,CancellationToken ct)
    {
        var body=await ReadBounded(response,ct);
        if(response.StatusCode>=HttpStatusCode.InternalServerError || response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.RequestTimeout) return new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"Não foi possível confirmar a emissão. Verifique disponibilidade e credenciais do emissor nacional.");
        using var json=JsonDocument.Parse(body);
        var encoded=GetString(json.RootElement,"nfseXmlGZipB64");
        if(response.IsSuccessStatusCode && encoded!=null)
        {
            var xml=Decompress(encoded); var document=FiscalXml.Parse(xml);
            if(!document.Descendants(FiscalXml.Nfse+"infDPS").Any(x=>(string?)x.Attribute("Id")==d.Identity)) return new(FiscalState.AwaitingConfirmation,Message:"A resposta fiscal não corresponde à DPS enviada.");
            var key=GetString(json.RootElement,"chaveAcesso") ?? document.Descendants(FiscalXml.Nfse+"infNFSe").FirstOrDefault()?.Attribute("Id")?.Value.Replace("NFS","");
            if(document.Root?.Name != FiscalXml.Nfse+"NFSe" || key == null || !System.Text.RegularExpressions.Regex.IsMatch(key,"^[0-9]{50}$") || document.Root.Element(FiscalXml.Nfse+"infNFSe")?.Attribute("Id")?.Value != "NFS"+key)
                return new(FiscalState.AwaitingConfirmation,RawResponse:body,Message:"O emissor não retornou uma NFS-e com identificação válida.");
            return new(FiscalState.Authorized,key,AuthorizedXml:xml,RawResponse:body,Message:"NFS-e emitida.");
        }
        return new(response.IsSuccessStatusCode?FiscalState.AwaitingConfirmation:FiscalState.Rejected,RawResponse:body,Message:"NFS-e não confirmada. Revise a configuração fiscal e consulte a situação antes de reenviar.");
    }
    public async Task<FiscalGatewayResult> Inutilize(FiscalSnapshot snapshot, string signedXml, X509Certificate2 certificate, CancellationToken ct)
    {
        using var client=Client(certificate);
        var document=new FiscalDocument { Snapshot=FiscalJson.Write(snapshot), SubmittedXml=signedXml };
        return await Soap(client,document,"void","NFeInutilizacao4","nfeInutilizacaoNF",FiscalXml.Parse(signedXml).Root!,ct);
    }
    private static string? GetString(JsonElement json,string name) => json.TryGetProperty(name,out var value)&&value.ValueKind==JsonValueKind.String?value.GetString():null;
    public static string Compress(string xml)
    {
        using var output=new MemoryStream();using(var gzip=new GZipStream(output,CompressionLevel.SmallestSize,true)) gzip.Write(Encoding.UTF8.GetBytes(xml));return Convert.ToBase64String(output.ToArray());
    }
    public static string Decompress(string encoded)
    {
        using var input=new MemoryStream(Convert.FromBase64String(encoded)); using var gzip=new GZipStream(input,CompressionMode.Decompress);using var output=new MemoryStream();var buffer=new byte[8192];int read;
        while((read=gzip.Read(buffer))>0) { if(output.Length+read>4_000_000) throw new InvalidDataException("Documento fiscal excede o limite.");output.Write(buffer,0,read); }
        return Encoding.UTF8.GetString(output.ToArray());
    }
    private static async Task<string> ReadBounded(HttpResponseMessage response,CancellationToken ct)
    {
        await response.Content.LoadIntoBufferAsync(4_000_000,ct);return await response.Content.ReadAsStringAsync(ct);
    }
}
