using Ofizzy.Api.Infrastructure.Errors;

namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalOriginGuard
{
    public static void EnsureCompatible(FiscalOrigin stored, FiscalOrigin active, FiscalEnvironment environment)
    {
        if (stored == FiscalOrigin.Unknown || active == FiscalOrigin.Unknown || stored != active)
        {
            throw new ConflictException("A origem do documento não corresponde ao gateway ativo ou não foi comprovada. Não transmita novamente; reconcilie o histórico fiscal.");
        }

        if (stored == FiscalOrigin.Simulation && environment != FiscalEnvironment.Homologation)
        {
            throw new ConflictException("Simulação fiscal é permitida somente em homologação local.");
        }
    }
}
