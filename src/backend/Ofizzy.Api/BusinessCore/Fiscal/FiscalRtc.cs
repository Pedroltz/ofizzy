using System.Text.RegularExpressions;

namespace Ofizzy.Api.Modules.Fiscal;

// Explicit classification and rates, supplied by the tenant. No NCM-based inference.
public sealed record RtcProfile(bool Enabled, string? Cst, string? ClassTrib, decimal? BasePercent,
    decimal? IbsUfRate, decimal? IbsMunicipalRate, decimal? CbsRate, string? OperationCode = null);
public sealed record RtcAmounts(string Cst, string ClassTrib, decimal Base, decimal IbsUfRate,
    decimal IbsMunicipalRate, decimal CbsRate, decimal IbsUf, decimal IbsMunicipal, decimal Cbs)
{
    public decimal Ibs => IbsUf + IbsMunicipal;
}

public static class FiscalRtc
{
    public static bool RequiresConfiguration(string regime, DateOnly emissionDate) =>
        regime == "SimplesNacional" && emissionDate >= new DateOnly(2027, 1, 1);
    public static RtcProfile Profile(ProductFiscalData p) => new(p.RtcEnabled, p.RtcCst, p.RtcClassTrib,
        p.RtcBasePercent, p.RtcIbsUfRate, p.RtcIbsMunicipalRate, p.RtcCbsRate);
    public static RtcProfile Profile(ServiceFiscalData p) => new(p.RtcEnabled, p.RtcCst, p.RtcClassTrib,
        p.RtcBasePercent, p.RtcIbsUfRate, p.RtcIbsMunicipalRate, p.RtcCbsRate, p.RtcOperationCode);

    // Draft profiles can be incomplete or describe a future unsupported scenario.
    public static IEnumerable<FiscalIssue> DraftIssues(RtcProfile p)
    {
        if (!string.IsNullOrEmpty(p.Cst) && !Regex.IsMatch(p.Cst, "^[0-9]{3}$"))
            yield return new("rtcCst", "CST IBS/CBS deve ter 3 dígitos.");
        if (!string.IsNullOrEmpty(p.ClassTrib) && !Regex.IsMatch(p.ClassTrib, "^[0-9]{6}$"))
            yield return new("rtcClassTrib", "cClassTrib deve ter 6 dígitos.");
        if (!string.IsNullOrEmpty(p.OperationCode) && !Regex.IsMatch(p.OperationCode, "^[0-9]{6}$"))
            yield return new("rtcOperationCode", "cIndOp deve ter 6 dígitos.");
        foreach (var (field, rate) in new[] { ("rtcBasePercent", p.BasePercent), ("rtcIbsUfRate", p.IbsUfRate),
            ("rtcIbsMunicipalRate", p.IbsMunicipalRate), ("rtcCbsRate", p.CbsRate) })
            if (rate.HasValue && (rate < 0 || rate > 100 || decimal.Round(rate.Value, 4) != rate))
                yield return new(field, "Informe percentual entre 0 e 100, com até 4 casas decimais.");
    }

    public static IEnumerable<FiscalIssue> EmissionIssues(RtcProfile p, bool service)
    {
        foreach (var issue in DraftIssues(p)) yield return issue;
        if (!p.Enabled) yield break;
        if (p.Cst != "000" || p.ClassTrib != "000001")
            yield return new("rtcClassTrib", "Cenário RTC ainda não suportado. A primeira implementação calcula tributação integral 000/000001; valide sua aplicabilidade com a contabilidade.");
        if (p.BasePercent != 100)
            yield return new("rtcBasePercent", "Informe base de 100% para o cenário integral. Bases reduzidas, deduções e regimes especiais exigem implementação própria.");
        if (!p.IbsUfRate.HasValue || !p.IbsMunicipalRate.HasValue || !p.CbsRate.HasValue)
            yield return new("rtcCbsRate", "Informe explicitamente as três alíquotas IBS/CBS, inclusive quando forem zero.");
        if (service && string.IsNullOrEmpty(p.OperationCode))
            yield return new("rtcOperationCode", "Informe cIndOp aprovado para a operação de serviço.");
    }

    public static RtcAmounts Calculate(RtcProfile p, decimal value)
    {
        if (!p.Enabled || EmissionIssues(p, false).Any() || value < 0)
            throw new InvalidOperationException("Perfil RTC incompleto ou cenário não suportado.");
        var basis = FiscalValidation.Money(value);
        return new(p.Cst!, p.ClassTrib!, basis, p.IbsUfRate!.Value, p.IbsMunicipalRate!.Value, p.CbsRate!.Value,
            FiscalValidation.Money(basis * p.IbsUfRate.Value / 100),
            FiscalValidation.Money(basis * p.IbsMunicipalRate.Value / 100),
            FiscalValidation.Money(basis * p.CbsRate.Value / 100));
    }
}
