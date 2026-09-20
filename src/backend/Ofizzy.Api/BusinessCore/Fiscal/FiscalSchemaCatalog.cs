namespace Ofizzy.Api.Modules.Fiscal;

public sealed record FiscalSchemaReference(
    string Package,
    string Folder,
    string EntryPoint,
    bool HasDedicatedSignatureSchema = false);

public static class FiscalSchemaCatalog
{
    // NF-e 010c remains the active package until the 010f domain/XML migration is complete.
    private static readonly FiscalSchemaReference Nfe010cDocument = new(
        "NF-e PL_010c",
        "Nfe",
        "nfe_v4.00.xsd");

    private static readonly FiscalSchemaReference Nfe010cCancellation = new(
        "NF-e cancellation baseline",
        "NfeEvents",
        "envEventoCancNFe_v1.00.xsd");

    private static readonly FiscalSchemaReference Nfe010cInutilization = new(
        "NF-e inutilization baseline",
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
        FiscalKind.Nfe => Nfe010cDocument,
        FiscalKind.Nfse => Nfse101Dps,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Tipo fiscal sem schema configurado.")
    };

    public static FiscalSchemaReference AuthorizedNfse() => Nfse101Authorized;

    public static FiscalSchemaReference Cancellation(FiscalKind kind) => kind switch
    {
        FiscalKind.Nfe => Nfe010cCancellation,
        FiscalKind.Nfse => Nfse101Cancellation,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Tipo fiscal sem schema configurado.")
    };

    public static FiscalSchemaReference Inutilization() => Nfe010cInutilization;
}
