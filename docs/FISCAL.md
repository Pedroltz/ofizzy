# Fiscal — operação e continuidade

Atualizado em **24/09/2026**. Estado: fluxos locais implementados; homologação externa do piloto ainda pendente.

## Ambientes e gateway

`NationalFiscalGateway` contém integração direta NF-e/NFS-e. Em `Development`, `Fiscal:SimulateGateway=true` seleciona `DevSimulatedFiscalGateway`.

Homologação real deve usar ambiente/configuração isolados, simulador desligado e credenciais reais. PFX, senha, chave AES, JWT, cookies e dados fiscais reais nunca devem ser versionados.

## Gates de produção

A intenção operacional é exigir cumulativamente `Fiscal:ProductionEnabled=true`, tenant em `Fiscal:HomologatedTenants` e `Tenant.FiscalProductionReleased=true`.

**Pendência conhecida:** em 24/09/2026 o código retorna `true` antecipadamente quando o tenant está liberado no banco. Corrigir antes de qualquer emissão produtiva.

## Schemas

- NF-e nova: `PL_010f_v1.04` em `Schemas/Nfe010f`.
- NF-e histórica: pacote persistido no documento; não reinterpretar.
- Eventos RTC NF-e: pacote separado em `Schemas/NfeEventsRtc`.
- NFS-e: pacote nacional 1.01 em `Schemas/Nfse`.

Schema válido não equivale a autorização fiscal.

## Reforma Tributária

O cronograma oficial coloca os documentos fiscais do **Simples Nacional** em obrigatoriedade RTC em **01/01/2027**. Até esse marco, concluir domínio, cálculo, snapshot e XML dos grupos aplicáveis, usando regras fornecidas pela contabilidade.

A NFS-e Nacional colocou tratamento de CNPJ alfanumérico em produção em 10/08/2026. O Ofizzy ainda não deve declarar suporte completo: existe somente o cálculo isolado de DV.

No cronograma nacional, ME/EPP do Simples passam a usar obrigatoriamente o Emissor Nacional em 01/11/2026; Igaraçu do Tietê comunicou migração local dessas empresas desde 01/08/2026.

## NFS-e e ADN

A documentação nacional mantém manuais para emissão e ADN. O código atual ainda bloqueia `NfseAdnBase(Production)`; a atualização está no plano H1.

## Split Payment

A Plataforma Pública possui Manual de Integração/Swagger e recebeu novo ato técnico em 23/09/2026. Preparar vínculo DF-e ↔ transação, valores bruto/segregado/líquido, estornos e identificadores externos para conciliação. Integração direta só entra se houver papel técnico aplicável ao Ofizzy.

## Referências

- [Próximos passos](NEXT-STEPS.md)
- [Auditoria regulatória](fiscal/FISCAL-REGULATORY-AUDIT-2026.md)
- [Runbook de homologação](fiscal/HOMOLOGATION-RUNBOOK.md)
- [Checklist de produção](fiscal/PRODUCTION-READINESS.md)

Não habilitar produção com base apenas em build, XSD, fixture ou gateway simulado.
