using System.Text.Json;
using Ofizzy.Api.Modules.Tenancy;

namespace Ofizzy.Api.Modules.Fiscal;

public enum FiscalKind
{
    Nfe,
    Nfse
}

public enum FiscalEnvironment
{
    Homologation = 2,
    Production = 1
}

public enum FiscalState
{
    Prepared,
    Processing,
    AwaitingConfirmation,
    Authorized,
    Rejected,
    CancellationPending,
    Cancelled,
    Inutilized
}

public sealed record FiscalAddress(
    string Street = "",
    string Number = "",
    string District = "",
    string City = "",
    string CityCode = "",
    string State = "",
    string PostalCode = "");

public sealed record FiscalSettingsData(
    string Cnpj = "",
    string LegalName = "",
    string StateRegistration = "",
    string MunicipalRegistration = "",
    string Regime = "",
    FiscalAddress? Address = null,
    bool NfeEnabled = false,
    bool NfseEnabled = false,
    FiscalEnvironment Environment = FiscalEnvironment.Homologation,
    int NfeSeries = 1,
    int DpsSeries = 1);

public sealed record ProductFiscalData(
    string Ncm = "",
    string? Cest = null,
    string Origin = "0",
    string Unit = "UN",
    string Gtin = "SEM GTIN",
    string Cfop = "",
    string Csosn = "",
    string PisCst = "",
    string CofinsCst = "",
    decimal? RetainedStBase = null,
    decimal? RetainedStAmount = null,
    decimal? SubstituteAmount = null,
    decimal? StRate = null);

public sealed record ServiceFiscalData(
    string NationalCode = "",
    string? MunicipalCode = null,
    string? Nbs = null,
    decimal? ApproximateTaxRate = null);

public sealed record FiscalPreparationData(
    FiscalAddress? Address = null,
    string? Document = null,
    string? Name = null,
    string? StateRegistration = null,
    string RecipientIeIndicator = "9",
    DateOnly? Competence = null,
    string PaymentCode = "",
    decimal? PaymentAmount = null,
    Dictionary<Guid, ProductFiscalData>? Products = null,
    Dictionary<Guid, ServiceFiscalData>? Services = null);

public sealed record FiscalIssue(
    string Field,
    string Message);

public sealed record CertificateInfo(
    string Subject,
    DateTimeOffset ExpiresAt,
    string Thumbprint);

public sealed record FiscalSettingsResponse(
    FiscalSettingsData Settings,
    CertificateInfo? Certificate,
    bool EncryptionConfigured,
    bool ProductionAllowed,
    bool DevToolsAvailable = false);

public sealed record FiscalHomologationCheck(
    string Code,
    string Label,
    bool Passed,
    string Detail,
    bool RequiresExternalConfirmation = false);

public sealed record FiscalHomologationReadinessResponse(
    bool ReadyForExternalHomologation,
    IReadOnlyList<FiscalHomologationCheck> Checks);

public sealed record FiscalDocumentResponse(
    Guid Id,
    FiscalKind Kind,
    FiscalEnvironment Environment,
    FiscalState State,
    long Number,
    string SchemaPackage,
    string? AccessKey,
    decimal Total,
    string? Message,
    DateTimeOffset CreatedAt,
    bool CanDownload);

public sealed record FiscalOrderResponse(
    FiscalPreparationData Preparation,
    IReadOnlyList<FiscalIssue> Issues,
    decimal ServicesTotal,
    decimal ProductsTotal,
    string Status,
    IReadOnlyList<FiscalDocumentResponse> Documents);

public sealed record CancelFiscalRequest(
    string Reason);

public sealed record InutilizationRequest(
    int Series,
    int Year,
    long FirstNumber,
    long LastNumber,
    string Reason);

public sealed record FiscalLine(
    Guid Id,
    string Code,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    ProductFiscalData? Product,
    ServiceFiscalData? Service);

public sealed record FiscalSnapshot(
    FiscalSettingsData Issuer,
    FiscalPreparationData Recipient,
    long OrderNumber,
    DateTimeOffset IssuedAt,
    IReadOnlyList<FiscalLine> Lines);

public static class FiscalJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Read<T>(string value) where T : new()
    {
        return JsonSerializer.Deserialize<T>(value, Options) ?? new T();
    }

    public static T Required<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value, Options)
            ?? throw new InvalidOperationException("Snapshot fiscal ausente.");
    }
}

public sealed class FiscalSettings : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Data { get; set; } = "{}";
    public byte[]? Certificate { get; set; }
    public string? KeyId { get; set; }
    public string? CertificateSubject { get; set; }
    public string? CertificateThumbprint { get; set; }
    public DateTimeOffset? CertificateExpiresAt { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}

public sealed class ProductFiscalProfile : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PartId { get; set; }
    public string Data { get; set; } = "{}";
}

public sealed class ServiceFiscalProfile : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ServiceId { get; set; }
    public string Data { get; set; } = "{}";
}

public sealed class FiscalPreparation : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkOrderId { get; set; }
    public string Data { get; set; } = "{}";
    public Guid Version { get; set; } = Guid.NewGuid();
}

public sealed class FiscalSequence : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public FiscalKind Kind { get; set; }
    public FiscalEnvironment Environment { get; set; }
    public int Series { get; set; }
    public long LastNumber { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}

public sealed class FiscalDocument : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkOrderId { get; set; }
    public FiscalKind Kind { get; set; }
    public FiscalEnvironment Environment { get; set; }
    public FiscalState State { get; set; }
    public int Series { get; set; }
    public long Number { get; set; }
    public string SchemaPackage { get; set; } = "Legado sem pacote identificado";
    public string Identity { get; set; } = "";
    public string? AccessKey { get; set; }
    public string? Protocol { get; set; }
    public string? Receipt { get; set; }
    public decimal Total { get; set; }
    public string Snapshot { get; set; } = "{}";
    public string? SubmittedXml { get; set; }
    public string? AuthorizedXml { get; set; }
    public string? Message { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LeaseUntil { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}

public sealed class FiscalEvent : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DocumentId { get; set; }
    public string Operation { get; set; } = "";
    public string? RequestXml { get; set; }
    public string? ResponseXml { get; set; }
    public string? Message { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class FiscalInutilization : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public FiscalEnvironment Environment { get; set; }
    public int Series { get; set; }
    public int Year { get; set; }
    public long FirstNumber { get; set; }
    public long LastNumber { get; set; }
    public string Reason { get; set; } = "";
    public string State { get; set; } = "Pending";
    public string? RequestXml { get; set; }
    public string? ResponseXml { get; set; }
    public string? Protocol { get; set; }
    public string? Message { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LeaseUntil { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}
