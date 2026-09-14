using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.WorkOrders;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed record FiscalPreparedOrder(WorkOrder Order, FiscalSettings? Settings, FiscalSnapshot Snapshot, List<FiscalIssue> Issues);
public sealed class FiscalPreparationService(ApplicationDbContext db, FiscalCertificateVault vault, IConfiguration config)
{
    public async Task<FiscalPreparedOrder?> Prepare(Guid id, CancellationToken ct)
    {
        var order = await db.WorkOrders.AsNoTracking().Include(x => x.Services).Include(x => x.Parts).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (order == null) return null;
        var settings = await db.FiscalSettingsEntries.AsNoTracking().SingleOrDefaultAsync(ct);
        var issuer = settings == null ? new FiscalSettingsData() : FiscalJson.Required<FiscalSettingsData>(settings.Data);
        var preparation = await db.FiscalPreparationEntries.AsNoTracking().SingleOrDefaultAsync(x => x.WorkOrderId == id, ct);
        var timezone = await db.TenantSettings.Select(x => x.Timezone).SingleAsync(ct);
        var competence = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(order.CompletedAt ?? order.CreatedAt, TimeZoneInfo.FindSystemTimeZoneById(timezone)).DateTime);
        var recipient = preparation == null ? new FiscalPreparationData(Document: order.CustomerDocument, Name: order.CustomerName, Competence: competence) : FiscalJson.Required<FiscalPreparationData>(preparation.Data);
        var products = await db.ProductFiscalProfileEntries.AsNoTracking().ToDictionaryAsync(x => x.PartId, x => x.Data, ct);
        var services = await db.ServiceFiscalProfileEntries.AsNoTracking().ToDictionaryAsync(x => x.ServiceId, x => x.Data, ct);
        var lines = new List<FiscalLine>();
        foreach (var p in order.Parts.OrderBy(x => x.Id))
        {
            var profile = recipient.Products?.GetValueOrDefault(p.Id);
            if (profile == null && p.PartId is {} pid && products.TryGetValue(pid, out var data)) profile = FiscalJson.Required<ProductFiscalData>(data);
            lines.Add(new(p.Id, p.Code ?? p.Id.ToString("N"), p.Description, p.Quantity, p.UnitPrice, profile, null));
        }
        foreach (var s in order.Services.OrderBy(x => x.Id))
        {
            var profile = recipient.Services?.GetValueOrDefault(s.Id);
            if (profile == null && s.ServiceId is {} sid && services.TryGetValue(sid, out var data)) profile = FiscalJson.Required<ServiceFiscalData>(data);
            lines.Add(new(s.Id, s.Id.ToString("N"), s.Description, s.Quantity, s.UnitPrice, null, profile));
        }
        var issues = new List<FiscalIssue>();
        if (order.Status != WorkOrderStatus.Completed) issues.Add(new("order.status", "Finalize a OS antes de emitir os documentos fiscais."));
        if (settings == null) issues.Add(new("settings", "O administrador deve configurar os dados fiscais da empresa."));
        else foreach (var e in new FiscalSettingsValidator().Validate(issuer).Errors) issues.Add(new("settings." + e.PropertyName, e.ErrorMessage));
        if (!vault.Configured) issues.Add(new("certificate", "A proteção do certificado precisa ser configurada no servidor."));
        if (settings?.Certificate == null) issues.Add(new("certificate", "O administrador deve cadastrar o certificado A1."));
        else if (settings.CertificateExpiresAt <= DateTimeOffset.UtcNow) issues.Add(new("certificate", "O certificado A1 está vencido."));
        if (!FiscalValidation.IsDocument(recipient.Document)) issues.Add(new("document", "Informe CPF/CNPJ válido do cliente."));
        if (string.IsNullOrWhiteSpace(recipient.Name) || recipient.Name.Length > 60) issues.Add(new("name", "Informe o nome fiscal do cliente (até 60 caracteres)."));
        if (recipient.Address == null) issues.Add(new("address", "Preencha o endereço fiscal do cliente."));
        else foreach (var e in new FiscalAddressValidator().Validate(recipient.Address).Errors) issues.Add(new("address." + e.PropertyName, e.ErrorMessage));
        if (recipient.Competence == null || recipient.Competence > DateOnly.FromDateTime(DateTime.UtcNow)) issues.Add(new("competence", "Informe uma competência válida, não futura."));
        if (order.Parts.Count > 0)
        {
            if (!issuer.NfeEnabled) issues.Add(new("settings.nfeEnabled", "Habilite a emissão de produtos nas configurações fiscais."));
            if (recipient.Address?.State != issuer.Address?.State) issues.Add(new("address.state", "Esta versão atende somente vendas internas na UF da oficina."));
            if (recipient.RecipientIeIndicator is not ("1" or "2" or "9")) issues.Add(new("recipientIeIndicator", "Informe a situação da inscrição estadual."));
            if (recipient.RecipientIeIndicator == "1" && !System.Text.RegularExpressions.Regex.IsMatch(recipient.StateRegistration ?? "", "^[0-9]{2,14}$")) issues.Add(new("stateRegistration", "Informe a inscrição estadual do destinatário."));
            if (recipient.PaymentCode is not ("01" or "02" or "17" or "18" or "90")) issues.Add(new("paymentCode", "Informe dinheiro, cheque, PIX, transferência ou sem pagamento. Cartões ainda não são suportados."));
            var total = order.Parts.Sum(x => FiscalValidation.Money(x.Quantity * x.UnitPrice));
            if (total <= 0) issues.Add(new("products", "Os produtos devem ter valor fiscal positivo."));
            if (recipient.PaymentAmount != (recipient.PaymentCode == "90" ? 0 : total)) issues.Add(new("paymentAmount", "O pagamento declarado deve corresponder ao valor da NF-e, ou zero para sem pagamento."));
        }
        if (order.Services.Count > 0)
        {
            if (!issuer.NfseEnabled) issues.Add(new("settings.nfseEnabled", "Habilite a emissão de serviços nas configurações fiscais."));
            if (FiscalValidation.Money(order.Services.Sum(x => x.Quantity * x.UnitPrice)) <= 0) issues.Add(new("services", "Os serviços devem ter valor fiscal positivo."));
        }
        foreach (var line in lines)
        {
            var isProduct = order.Parts.Any(x => x.Id == line.Id);
            if (isProduct)
            {
                if (line.Product == null) issues.Add(new($"products.{line.Id}", $"Complete os dados fiscais de {line.Description}."));
                else foreach (var e in new ProductFiscalValidator().Validate(line.Product).Errors) issues.Add(new($"products.{line.Id}.{e.PropertyName}", e.ErrorMessage));
            }
            else
            {
                if (line.Service == null) issues.Add(new($"services.{line.Id}", $"Classifique o serviço {line.Description}."));
                else
                {
                    foreach (var e in new ServiceFiscalValidator().Validate(line.Service).Errors) issues.Add(new($"services.{line.Id}.{e.PropertyName}", e.ErrorMessage));
                    if (issuer.Regime == "SimplesNacional" && line.Service.ApproximateTaxRate == null) issues.Add(new($"services.{line.Id}.approximateTaxRate", "Informe o percentual aproximado dos tributos do Simples, validado pela contabilidade."));
                }
            }
        }
        if (lines.Where(x => x.Service != null).Select(x => x.Service).Distinct().Count() > 1) issues.Add(new("services", "Todos os serviços da NFS-e devem usar a mesma classificação e perfil fiscal."));
        if (string.Join("; ", order.Services.Select(x => x.Description)).Length > 1800) issues.Add(new("services", "A descrição agregada dos serviços excede 1800 caracteres."));
        // An explicit server allowlist is the release gate, separate from tenant settings.
        var allowed = config.GetSection("Fiscal:HomologatedTenants").Get<string[]>() ?? [];
        if (issuer.Environment == FiscalEnvironment.Production && (!config.GetValue<bool>("Fiscal:ProductionEnabled") || !allowed.Contains(db.TenantId.ToString())))
            issues.Add(new("environment", "Produção ainda não homologada para esta organização."));
        return new(order, settings, new(issuer, recipient, order.Number, DateTimeOffset.UtcNow, lines), issues);
    }
    public static FiscalDocumentResponse Map(FiscalDocument d) => new(d.Id,d.Kind,d.Environment,d.State,d.Number,d.AccessKey,d.Total,d.Message,d.CreatedAt,d.AuthorizedXml != null);
    public static string Status(IReadOnlyList<FiscalDocument> documents, int expected)
    {
        var current = documents.Where(x => x.State is not (FiscalState.Cancelled or FiscalState.Inutilized)).ToList();
        var authorized = current.Count(x => x.State == FiscalState.Authorized);
        if (expected > 0 && authorized == expected) return "Completed";
        if (authorized > 0) return "Partial";
        if (current.Any(x => x.State is FiscalState.Processing or FiscalState.AwaitingConfirmation or FiscalState.CancellationPending)) return "Processing";
        if (current.Any(x => x.State == FiscalState.Rejected)) return "Rejected";
        return documents.Any(x => x.State == FiscalState.Cancelled) ? "Cancelled" : "Pending";
    }
}
