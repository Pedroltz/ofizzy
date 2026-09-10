using FluentValidation;

namespace Ofizzy.Api.Authentication;
public sealed record SetupRequest(string CompanyName, string? Cnpj, string? Phone, string AdminName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record TenantContextResponse(Guid Id, string Name, string Slug, string Status, string Vertical, string Role, bool OnboardingCompleted, string[] Modules);
public sealed record CurrentUserResponse(Guid Id, string Name, string Email, bool IsPlatformAdmin = false, TenantContextResponse? Tenant = null);
public sealed record SelectTenantRequest(Guid TenantId);
public sealed record SetupStatusResponse(bool Required);

public sealed class SetupRequestValidator : AbstractValidator<SetupRequest>
{
    public SetupRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Cnpj).Matches("^[0-9]{14}$").When(x => !string.IsNullOrWhiteSpace(x.Cnpj)).WithMessage("Informe 14 dígitos.");
        RuleFor(x => x.Phone).MaximumLength(20); RuleFor(x => x.AdminName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10).Matches("[A-Z]").WithMessage("Inclua uma letra maiúscula.").Matches("[a-z]").WithMessage("Inclua uma letra minúscula.").Matches("[0-9]").WithMessage("Inclua um número.");
    }
}
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
