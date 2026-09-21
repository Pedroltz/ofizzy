namespace Ofizzy.Api.Modules.Fiscal;

public static class FiscalHomologationReadiness
{
    public static FiscalHomologationReadinessResponse Evaluate(
        FiscalSettingsData settings,
        bool encryptionConfigured,
        CertificateInfo? certificate,
        DateTimeOffset now)
    {
        var settingsValid = new FiscalSettingsValidator().Validate(settings).IsValid;
        var certificateValid = certificate is not null && certificate.ExpiresAt > now;
        var checks = new List<FiscalHomologationCheck>
        {
            new(
                "settings",
                "Dados fiscais validados",
                settingsValid,
                settingsValid
                    ? "CNPJ, regime, endereço, séries e campos exigidos pelo perfil local estão válidos."
                    : "Revise os dados fiscais obrigatórios antes de iniciar qualquer transmissão de homologação."),
            new(
                "environment",
                "Ambiente de homologação",
                settings.Environment == FiscalEnvironment.Homologation,
                settings.Environment == FiscalEnvironment.Homologation
                    ? "O envio está direcionado ao ambiente de homologação."
                    : "Mantenha a configuração em Homologação; produção não é necessária para este aceite."),
            new(
                "certificate-protection",
                "Proteção da chave do certificado",
                encryptionConfigured,
                encryptionConfigured
                    ? "A proteção criptográfica do certificado A1 está configurada no servidor."
                    : "Configure a chave externa de proteção do A1 no servidor antes de cadastrar o certificado."),
            new(
                "certificate",
                "Certificado A1 válido",
                certificateValid,
                certificateValid
                    ? "Há certificado cadastrado e dentro da validade registrada. A identidade é revalidada a cada uso."
                    : "Cadastre um A1 válido da empresa; o certificado de desenvolvimento não serve para homologação oficial."),
            new(
                "nfe-schema",
                "Leiaute NF-e vigente",
                !settings.NfeEnabled || FiscalSchemaCatalog.Document(FiscalKind.Nfe).Package == "NF-e PL_010f v1.04",
                settings.NfeEnabled
                    ? "O XML de NF-e usa o pacote oficial PL_010f v1.04. Eventos e inutilização mantêm schemas oficiais próprios e as classificações RTC continuam dependentes de validação contábil por operação."
                    : "NF-e não está habilitada para esta organização; este requisito só se aplica quando houver produtos."),
            new(
                "nfse-schema",
                "Leiaute NFS-e Nacional",
                !settings.NfseEnabled || FiscalSchemaCatalog.Document(FiscalKind.Nfse).Package == "NFS-e Nacional 1.01",
                settings.NfseEnabled
                    ? "O pacote local NFS-e Nacional 1.01 foi conferido contra a distribuição oficial registrada."
                    : "NFS-e não está habilitada para esta organização; este requisito só se aplica quando houver serviços."),
            new(
                "credentialing",
                "Credenciamento e parâmetros do emissor",
                false,
                "Confirme CNPJ, IE, IM, série exclusiva e credenciamento nos emissores com a oficina e os órgãos fiscais.",
                true),
            new(
                "accounting",
                "Classificações aprovadas pela contabilidade",
                false,
                "Confirme CFOP/CSOSN/ST, códigos de serviço e cenários RTC aplicáveis antes de transmitir documentos.",
                true)
        };

        return new FiscalHomologationReadinessResponse(
            checks.All(x => x.Passed || x.RequiresExternalConfirmation),
            checks);
    }
}
