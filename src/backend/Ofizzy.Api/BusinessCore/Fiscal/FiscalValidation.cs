using System.Text.RegularExpressions;
using FluentValidation;

namespace Ofizzy.Api.Modules.Fiscal;

public sealed class FiscalSettingsValidator : AbstractValidator<FiscalSettingsData>
{
    public FiscalSettingsValidator()
    {
        RuleFor(x => x.Cnpj)
            .Must(FiscalValidation.IsCnpj)
            .WithMessage("Informe um CNPJ válido, sem máscara.");

        RuleFor(x => x.LegalName)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.Regime)
            .Must(x => x is "MEI" or "SimplesNacional")
            .WithMessage("Esta versão atende MEI e Simples Nacional.");

        RuleFor(x => x.Environment)
            .IsInEnum();

        RuleFor(x => x.Address)
            .NotNull();

        RuleFor(x => x.Address!)
            .SetValidator(new FiscalAddressValidator())
            .When(x => x.Address != null);

        RuleFor(x => x.StateRegistration)
            .MaximumLength(14)
            .Matches("^[0-9]*$");

        RuleFor(x => x.StateRegistration)
            .NotEmpty()
            .When(x => x.NfeEnabled);

        RuleFor(x => x.MunicipalRegistration)
            .MaximumLength(15);

        RuleFor(x => x.NfeSeries)
            .InclusiveBetween(1, 889);

        RuleFor(x => x.DpsSeries)
            .InclusiveBetween(1, 49999);
    }
}

public sealed class FiscalAddressValidator : AbstractValidator<FiscalAddress>
{
    public FiscalAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.CityCode)
            .Matches("^[0-9]{7}$");

        RuleFor(x => x.State)
            .Must(FiscalValidation.StateCodes.ContainsKey)
            .WithMessage("UF inválida.");

        RuleFor(x => x)
            .Must(x => FiscalValidation.StateCodes.TryGetValue(x.State, out var code) && x.CityCode.StartsWith(code, StringComparison.Ordinal))
            .WithMessage("Código IBGE incompatível com a UF.");

        RuleFor(x => x.PostalCode)
            .Matches("^[0-9]{8}$");
    }
}

public sealed class ProductFiscalValidator : AbstractValidator<ProductFiscalData>
{
    public ProductFiscalValidator()
    {
        RuleFor(x => x.Ncm)
            .Matches("^[0-9]{8}$");

        RuleFor(x => x.Cest)
            .Matches("^[0-9]{7}$")
            .When(x => !string.IsNullOrEmpty(x.Cest));

        RuleFor(x => x.Origin)
            .Matches("^[0-8]$");

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(6);

        RuleFor(x => x.Gtin)
            .Must(x => x == "SEM GTIN" || Regex.IsMatch(x ?? string.Empty, "^([0-9]{8}|[0-9]{12,14})$"))
            .WithMessage("Informe GTIN válido ou SEM GTIN.");

        RuleFor(x => x)
            .Must(x => (x.Csosn == "102" && x.Cfop == "5102") || (x.Csosn == "500" && x.Cfop == "5405"))
            .WithMessage("Perfil suportado: revenda interna 5102/102 ou 5405/500 (ST anterior).");

        RuleFor(x => x.PisCst)
            .Must(x => x is "04" or "06" or "07" or "08" or "09")
            .WithMessage("Informe CST de PIS compatível com perfil sem destaque.");

        RuleFor(x => x.CofinsCst)
            .Must(x => x is "04" or "06" or "07" or "08" or "09")
            .WithMessage("Informe CST de COFINS compatível com perfil sem destaque.");

        RuleFor(x => x.RetainedStBase)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RetainedStAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SubstituteAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.StRate)
            .InclusiveBetween(0, 100);

        RuleFor(x => x)
            .Must(x => x.Csosn != "500" || (x.RetainedStBase.HasValue && x.RetainedStAmount.HasValue && x.SubstituteAmount.HasValue && x.StRate.HasValue))
            .WithMessage("Informe os valores unitários de ST anterior e a alíquota, conforme os documentos de entrada.");
    }
}

public sealed class ServiceFiscalValidator : AbstractValidator<ServiceFiscalData>
{
    public ServiceFiscalValidator()
    {
        RuleFor(x => x.NationalCode)
            .Matches("^[0-9]{6}$")
            .WithMessage("O código de tributação nacional deve conter exatamente 6 dígitos numéricos.");

        RuleFor(x => x.MunicipalCode)
            .Matches("^[0-9]{3}$")
            .WithMessage("O código municipal (quando informado) deve conter exatamente 3 dígitos numéricos.")
            .When(x => !string.IsNullOrWhiteSpace(x.MunicipalCode));

        RuleFor(x => x.Nbs)
            .Matches("^[0-9]{9}$")
            .WithMessage("O NBS (quando informado) deve conter exatamente 9 dígitos numéricos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Nbs));

        RuleFor(x => x.ApproximateTaxRate)
            .InclusiveBetween(0, 100);
    }
}

public static class FiscalValidation
{
    public static readonly IReadOnlyDictionary<string, string> StateCodes = new Dictionary<string, string>
    {
        ["RO"] = "11",
        ["AC"] = "12",
        ["AM"] = "13",
        ["RR"] = "14",
        ["PA"] = "15",
        ["AP"] = "16",
        ["TO"] = "17",
        ["MA"] = "21",
        ["PI"] = "22",
        ["CE"] = "23",
        ["RN"] = "24",
        ["PB"] = "25",
        ["PE"] = "26",
        ["AL"] = "27",
        ["SE"] = "28",
        ["BA"] = "29",
        ["MG"] = "31",
        ["ES"] = "32",
        ["RJ"] = "33",
        ["SP"] = "35",
        ["PR"] = "41",
        ["SC"] = "42",
        ["RS"] = "43",
        ["MS"] = "50",
        ["MT"] = "51",
        ["GO"] = "52",
        ["DF"] = "53"
    };

    public static decimal Money(decimal x) => decimal.Round(x, 2, MidpointRounding.AwayFromZero);

    public static bool IsCnpj(string? x) => CheckDocument(x, 14);

    public static bool IsDocument(string? x) => CheckDocument(x, x?.Length ?? 0);

    private static bool CheckDocument(string? value, int length)
    {
        if (length is not (11 or 14) || value?.Length != length || value.Any(c => c < '0' || c > '9') || value.Distinct().Count() == 1)
        {
            return false;
        }

        for (var pass = 0; pass < 2; pass++)
        {
            var n = length - 2 + pass;
            var sum = 0;

            for (var i = 0; i < n; i++)
            {
                var factor = length == 11 ? n + 1 - i : (n - 1 - i) % 8 + 2;
                sum += (value[i] - '0') * factor;
            }

            var digit = sum % 11 < 2 ? 0 : 11 - sum % 11;
            if (value[n] - '0' != digit)
            {
                return false;
            }
        }

        return true;
    }
}
