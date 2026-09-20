using System.Text.RegularExpressions;
using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed record FiscalPreparedOrder(
    WorkOrder Order,
    FiscalSettings? Settings,
    FiscalSnapshot Snapshot,
    List<FiscalIssue> Issues);

public sealed class FiscalPreparationService(
    ApplicationDbContext db,
    FiscalCertificateVault vault,
    IConfiguration config)
{
    public async Task<FiscalPreparedOrder?> Prepare(Guid id, CancellationToken ct)
    {
        var order = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (order == null)
        {
            return null;
        }

        var settings = await db.FiscalSettingsEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(ct);

        var issuer = settings == null
            ? new FiscalSettingsData()
            : FiscalJson.Required<FiscalSettingsData>(settings.Data);

        var preparation = await db.FiscalPreparationEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkOrderId == id, ct);

        var timezoneStr = await db.TenantSettings
            .Select(x => x.Timezone)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(timezoneStr))
        {
            timezoneStr = "America/Sao_Paulo";
        }

        TimeZoneInfo tzInfo;
        try
        {
            tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneStr);
        }
        catch
        {
            tzInfo = TimeZoneInfo.Utc;
        }

        var competence = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(order.CompletedAt ?? order.CreatedAt, tzInfo).DateTime);
        var partsTotal = FiscalValidation.Money(order.Parts.Sum(x => x.Quantity * x.UnitPrice));
        var customer = await db.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == order.CustomerId, ct);

        FiscalAddress? defaultAddress = null;
        if (customer != null && !string.IsNullOrWhiteSpace(customer.Street) && !string.IsNullOrWhiteSpace(customer.CityCode))
        {
            defaultAddress = new FiscalAddress(
                Street: customer.Street ?? string.Empty,
                Number: customer.Number ?? "S/N",
                District: customer.District ?? string.Empty,
                City: customer.City ?? string.Empty,
                CityCode: customer.CityCode ?? string.Empty,
                State: customer.State ?? string.Empty,
                PostalCode: customer.PostalCode ?? string.Empty);
        }

        var defaultIe = customer?.StateRegistration;
        var defaultIeIndicator = string.IsNullOrWhiteSpace(defaultIe) ? "9" : "1";

        var recipient = preparation == null
            ? new FiscalPreparationData(
                Address: defaultAddress,
                Document: order.CustomerDocument ?? customer?.Document,
                Name: order.CustomerName ?? customer?.Name,
                StateRegistration: defaultIe,
                RecipientIeIndicator: defaultIeIndicator,
                Competence: competence,
                PaymentCode: order.Parts.Count > 0 ? "01" : "90",
                PaymentAmount: order.Parts.Count > 0 ? partsTotal : 0)
            : FiscalJson.Required<FiscalPreparationData>(preparation.Data);

        if (preparation != null)
        {
            if (recipient.Address == null && defaultAddress != null)
            {
                recipient = recipient with { Address = defaultAddress };
            }

            if (string.IsNullOrWhiteSpace(recipient.StateRegistration) && !string.IsNullOrWhiteSpace(defaultIe))
            {
                recipient = recipient with
                {
                    StateRegistration = defaultIe,
                    RecipientIeIndicator = defaultIeIndicator
                };
            }

            if (order.Parts.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(recipient.PaymentCode))
                {
                    recipient = recipient with { PaymentCode = "01" };
                }

                if (recipient.PaymentAmount == null)
                {
                    recipient = recipient with
                    {
                        PaymentAmount = recipient.PaymentCode == "90" ? 0 : partsTotal
                    };
                }
            }
        }

        var products = await db.ProductFiscalProfileEntries
            .AsNoTracking()
            .ToDictionaryAsync(x => x.PartId, x => x.Data, ct);

        var services = await db.ServiceFiscalProfileEntries
            .AsNoTracking()
            .ToDictionaryAsync(x => x.ServiceId, x => x.Data, ct);

        var lines = new List<FiscalLine>();

        foreach (var p in order.Parts.OrderBy(x => x.Id))
        {
            var profile = recipient.Products?.GetValueOrDefault(p.Id);
            if (profile == null && p.PartId is { } pid && products.TryGetValue(pid, out var data))
            {
                profile = FiscalJson.Required<ProductFiscalData>(data);
            }

            lines.Add(new FiscalLine(
                p.Id,
                p.Code ?? p.Id.ToString("N"),
                p.Description,
                p.Quantity,
                p.UnitPrice,
                profile,
                null));
        }

        foreach (var s in order.Services.OrderBy(x => x.Id))
        {
            var profile = recipient.Services?.GetValueOrDefault(s.Id);
            if (profile == null && s.ServiceId is { } sid && services.TryGetValue(sid, out var data))
            {
                profile = FiscalJson.Required<ServiceFiscalData>(data);
            }

            if (profile != null)
            {
                profile = profile with
                {
                    MunicipalCode = string.IsNullOrWhiteSpace(profile.MunicipalCode) ? null : profile.MunicipalCode.Trim(),
                    Nbs = string.IsNullOrWhiteSpace(profile.Nbs) ? null : profile.Nbs.Trim()
                };
            }

            lines.Add(new FiscalLine(
                s.Id,
                s.Id.ToString("N"),
                s.Description,
                s.Quantity,
                s.UnitPrice,
                null,
                profile));
        }

        var issues = new List<FiscalIssue>();

        if (order.Status != WorkOrderStatus.Completed)
        {
            issues.Add(new FiscalIssue("order.status", "Finalize a OS antes de emitir os documentos fiscais."));
        }

        if (settings == null)
        {
            issues.Add(new FiscalIssue("settings", "O administrador deve configurar os dados fiscais da empresa."));
        }
        else
        {
            foreach (var e in new FiscalSettingsValidator().Validate(issuer).Errors)
            {
                issues.Add(new FiscalIssue($"settings.{e.PropertyName}", e.ErrorMessage));
            }
        }

        if (!vault.Configured)
        {
            issues.Add(new FiscalIssue("certificate", "A proteção do certificado precisa ser configurada no servidor."));
        }

        if (settings?.Certificate == null)
        {
            issues.Add(new FiscalIssue("certificate", "O administrador deve cadastrar o certificado A1."));
        }
        else if (settings.CertificateExpiresAt <= DateTimeOffset.UtcNow)
        {
            issues.Add(new FiscalIssue("certificate", "O certificado A1 está vencido."));
        }

        if (!FiscalValidation.IsDocument(recipient.Document))
        {
            issues.Add(new FiscalIssue("document", "Informe CPF/CNPJ válido do cliente."));
        }

        if (string.IsNullOrWhiteSpace(recipient.Name) || recipient.Name.Length > 60)
        {
            issues.Add(new FiscalIssue("name", "Informe o nome fiscal do cliente (até 60 caracteres)."));
        }

        if (recipient.Address == null)
        {
            issues.Add(new FiscalIssue("address", "Preencha o endereço fiscal do cliente."));
        }
        else
        {
            foreach (var e in new FiscalAddressValidator().Validate(recipient.Address).Errors)
            {
                issues.Add(new FiscalIssue($"address.{e.PropertyName}", e.ErrorMessage));
            }
        }

        if (recipient.Competence == null || recipient.Competence > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            issues.Add(new FiscalIssue("competence", "Informe uma competência válida, não futura."));
        }

        if (order.Parts.Count > 0)
        {
            if (!issuer.NfeEnabled)
            {
                issues.Add(new FiscalIssue("settings.nfeEnabled", "Habilite a emissão de produtos nas configurações fiscais."));
            }

            if (recipient.Address?.State != issuer.Address?.State)
            {
                issues.Add(new FiscalIssue("address.state", "Esta versão atende somente vendas internas na UF da oficina."));
            }

            if (recipient.RecipientIeIndicator is not ("1" or "2" or "9"))
            {
                issues.Add(new FiscalIssue("recipientIeIndicator", "Informe a situação da inscrição estadual."));
            }

            if (recipient.RecipientIeIndicator == "1" && !Regex.IsMatch(recipient.StateRegistration ?? string.Empty, "^[0-9]{2,14}$"))
            {
                issues.Add(new FiscalIssue("stateRegistration", "Informe a inscrição estadual do destinatário."));
            }

            if (recipient.PaymentCode is not ("01" or "02" or "17" or "18" or "90"))
            {
                issues.Add(new FiscalIssue("paymentCode", "Informe dinheiro, cheque, PIX, transferência ou sem pagamento. Cartões ainda não são suportados."));
            }

            var total = order.Parts.Sum(x => FiscalValidation.Money(x.Quantity * x.UnitPrice));
            if (total <= 0)
            {
                issues.Add(new FiscalIssue("products", "Os produtos devem ter valor fiscal positivo."));
            }

            if (recipient.PaymentAmount != (recipient.PaymentCode == "90" ? 0 : total))
            {
                issues.Add(new FiscalIssue("paymentAmount", "O pagamento declarado deve corresponder ao valor da NF-e, ou zero para sem pagamento."));
            }
        }

        if (order.Services.Count > 0)
        {
            if (!issuer.NfseEnabled)
            {
                issues.Add(new FiscalIssue("settings.nfseEnabled", "Habilite a emissão de serviços nas configurações fiscais."));
            }

            if (FiscalValidation.Money(order.Services.Sum(x => x.Quantity * x.UnitPrice)) <= 0)
            {
                issues.Add(new FiscalIssue("services", "Os serviços devem ter valor fiscal positivo."));
            }
        }

        static string ToCamel(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

        foreach (var line in lines)
        {
            var isProduct = order.Parts.Any(x => x.Id == line.Id);
            if (isProduct)
            {
                if (line.Product == null)
                {
                    issues.Add(new FiscalIssue($"products.{line.Id}", $"Complete os dados fiscais de {line.Description}."));
                }
                else
                {
                    foreach (var e in new ProductFiscalValidator().Validate(line.Product).Errors)
                    {
                        issues.Add(new FiscalIssue($"products.{line.Id}.{ToCamel(e.PropertyName)}", $"{line.Description}: {e.ErrorMessage}"));
                    }
                }
            }
            else
            {
                if (line.Service == null)
                {
                    issues.Add(new FiscalIssue($"services.{line.Id}", $"Classifique o serviço {line.Description}."));
                }
                else
                {
                    foreach (var e in new ServiceFiscalValidator().Validate(line.Service).Errors)
                    {
                        issues.Add(new FiscalIssue($"services.{line.Id}.{ToCamel(e.PropertyName)}", $"{line.Description}: {e.ErrorMessage}"));
                    }

                    if (issuer.Regime == "SimplesNacional" && line.Service.ApproximateTaxRate == null)
                    {
                        issues.Add(new FiscalIssue($"services.{line.Id}.approximateTaxRate", $"{line.Description}: Informe o percentual aproximado dos tributos do Simples, validado pela contabilidade."));
                    }
                }
            }
        }

        if (lines.Where(x => x.Service != null).Select(x => x.Service).Distinct().Count() > 1)
        {
            issues.Add(new FiscalIssue("services", "Todos os serviços da NFS-e devem usar a mesma classificação e perfil fiscal."));
        }

        if (string.Join("; ", order.Services.Select(x => x.Description)).Length > 1800)
        {
            issues.Add(new FiscalIssue("services", "A descrição agregada dos serviços excede 1800 caracteres."));
        }

        if (!await FiscalReleaseGate.IsTenantAllowedAsync(db, config, issuer.Environment, ct))
        {
            issues.Add(new FiscalIssue("environment", "Produção ainda não homologada para esta organização."));
        }

        var snapshot = new FiscalSnapshot(issuer, recipient, order.Number, DateTimeOffset.UtcNow, lines);
        return new FiscalPreparedOrder(order, settings, snapshot, issues);
    }

    public static FiscalDocumentResponse Map(FiscalDocument d)
    {
        return new FiscalDocumentResponse(
            d.Id,
            d.Kind,
            d.Environment,
            d.State,
            d.Number,
            d.SchemaPackage,
            d.AccessKey,
            d.Total,
            d.Message,
            d.CreatedAt,
            d.AuthorizedXml != null);
    }

    public static string Status(IReadOnlyList<FiscalDocument> documents, int expected)
    {
        var current = documents
            .Where(x => x.State is not (FiscalState.Cancelled or FiscalState.Inutilized))
            .ToList();

        var authorized = current.Count(x => x.State == FiscalState.Authorized);

        if (expected > 0 && authorized == expected)
        {
            return "Completed";
        }

        if (authorized > 0)
        {
            return "Partial";
        }

        if (current.Any(x => x.State is FiscalState.Processing or FiscalState.AwaitingConfirmation or FiscalState.CancellationPending))
        {
            return "Processing";
        }

        if (current.Any(x => x.State == FiscalState.Rejected))
        {
            return "Rejected";
        }

        return documents.Any(x => x.State == FiscalState.Cancelled) ? "Cancelled" : "Pending";
    }
}
