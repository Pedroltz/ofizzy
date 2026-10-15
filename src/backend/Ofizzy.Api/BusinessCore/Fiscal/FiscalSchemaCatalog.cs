namespace Ofizzy.Api.Modules.Fiscal;

public sealed record FiscalSchemaReference(
    string Package,
    string Folder,
    string EntryPoint,
    bool HasDedicatedSignatureSchema = false);

public static class FiscalSchemaCatalog
{
    // PL_010f_v1.04 is installed in a separate folder so documents emitted under
    // the former PL_010c baseline are never revalidated against a newer layout.
    private static readonly FiscalSchemaReference Nfe010fDocument = new(
        "NF-e PL_010f v1.04",
        "Nfe010f",
        "nfe_v4.00.xsd");

    // The PL_010f distribution contains only the NF-e document schemas. The
    // official RTC event distribution is kept separately; cancellation and
    // inutilization retain their dedicated official schemas until an event that
    // changes those envelopes is applicable to a supported operation.
    private static readonly FiscalSchemaReference NfeCancellation = new(
        "NF-e cancelamento 110111",
        "NfeEvents",
        "envEventoCancNFe_v1.00.xsd");

    private static readonly FiscalSchemaReference NfeInutilization = new(
        "NF-e inutilização 4.00",
        "Nfe",
        "inutNFe_v4.00.xsd");

    private static readonly FiscalSchemaReference Nfse101Dps = new(
        "NFS-e Nacional 1.01",
        "Nfse",
        "DPS_v1.01.xsd",
        HasDedicatedSignatureSchema: true);

    private static readonly FiscalSchemaReference Nfse101Authorized = new(
        "NFS-e Nacional 1.01",
        "Nfse",
        "NFSe_v1.01.xsd",
        HasDedicatedSignatureSchema: true);

    private static readonly FiscalSchemaReference Nfse101Cancellation = new(
        "NFS-e Nacional 1.01",
        "Nfse",
        "pedRegEvento_v1.01.xsd",
        HasDedicatedSignatureSchema: true);

    public static FiscalSchemaReference Document(FiscalKind kind) => kind switch
    {
        FiscalKind.Nfe => Nfe010fDocument,
        FiscalKind.Nfse => Nfse101Dps,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Tipo fiscal sem schema configurado.")
    };

    public static FiscalSchemaReference AuthorizedNfse() => Nfse101Authorized;

    public static FiscalSchemaReference Cancellation(FiscalKind kind) => kind switch
    {
        FiscalKind.Nfe => NfeCancellation,
        FiscalKind.Nfse => Nfse101Cancellation,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Tipo fiscal sem schema configurado.")
    };

    public static FiscalSchemaReference Inutilization() => NfeInutilization;
}
