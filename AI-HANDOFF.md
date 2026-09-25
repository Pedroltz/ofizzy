# Continuidade do Ofizzy

Atualizado em **24/09/2026**. Prioridade: levar a Fase 8 à homologação fiscal externa do piloto **Igaraçu do Tietê/SP — Simples Nacional**.

## Estado atual

- `NationalFiscalGateway` implementa integração direta NF-e/NFS-e.
- Em `Development`, `appsettings.Development.json` define `Fiscal:SimulateGateway=true`; o DI escolhe `DevSimulatedFiscalGateway`. Smokes locais não chegam aos órgãos fiscais.
- NF-e novas usam `PL_010f_v1.04`; documentos antigos preservam o pacote gravado.
- NFS-e usa o pacote nacional 1.01 versionado no projeto.
- DANFE/DANFSe são derivados do XML autorizado, mas a conferência externa ainda faz parte da homologação.
- Produção não está homologada.

## Bloqueios encontrados em 24/09/2026

1. `FiscalReleaseGate.IsAllowed` retorna `true` quando `isTenantReleased=true` antes de exigir `Fiscal:ProductionEnabled` e allow-list.
2. `NationalFiscalGateway.NfseAdnBase(Production)` ainda bloqueia o ADN produtivo; revisar com a documentação oficial vigente.
3. O código não possui domínio/XML completo de RTC (`gIBSCBS`, `cClassTrib`, valores/totais IBS/CBS).
4. O DV de CNPJ alfanumérico existe isoladamente, sem suporte ponta a ponta.
5. O marco oficial dos documentos fiscais do Simples Nacional na RTC é **01/01/2027**.

## Split Payment

Há documentação técnica oficial da Plataforma Pública e novo ato técnico publicado em 23/09/2026. O Ofizzy deve primeiro preservar vínculo DF-e ↔ transação ↔ liquidação e preparar conciliação.

## Próxima execução

Seguir [docs/NEXT-STEPS.md](docs/NEXT-STEPS.md): H1 gates/endpoints; H2 RTC/CNPJ alfanumérico; H3 resiliência/PDF; H4 credenciais reais; H5 homologação externa; H6 piloto produtivo.

Referências canônicas: [Fase 8](docs/phases/PHASE-08-FISCAL.md), [Operação fiscal](docs/FISCAL.md), [Auditoria regulatória](docs/fiscal/FISCAL-REGULATORY-AUDIT-2026.md), [Matriz RTC](docs/fiscal/RTC-DOMAIN-MATRIX.md), [Runbook](docs/fiscal/HOMOLOGATION-RUNBOOK.md) e [Checklist de produção](docs/fiscal/PRODUCTION-READINESS.md).

Relatórios e planos datados anteriores ficam em `docs/archive/` apenas como histórico.
