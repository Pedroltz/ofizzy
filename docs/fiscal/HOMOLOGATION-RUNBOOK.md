# Runbook de homologação fiscal externa

Atualizado em **24/09/2026**. Nenhuma etapa deste documento, por si só, constitui homologação.

## Pré-requisitos

1. H1-H3 de [NEXT-STEPS.md](../NEXT-STEPS.md) concluídos.
2. CNPJ, IE/IM, endereço, regime, série e classificação aprovados pela oficina/contabilidade.
3. A1 real válido e credenciamento confirmado.
4. Ambiente oficial de homologação com `Fiscal:SimulateGateway=false`.
5. Produção continua bloqueada.
6. Dossiê de evidências preparado sem segredos.

## Matriz mínima

| Caso | Resultado esperado |
| --- | --- |
| NF-e normal | autorização, chave/protocolo coerentes e `AuthorizedXml` persistido |
| NF-e ST | somente se o perfil exigir; cálculo/classificação aprovados |
| NFS-e | chave/número oficial e XML autorizado persistido |
| OS mista | NF-e/NFS-e independentes, inclusive autorização parcial |
| Rejeição/correção | motivo preservado e nova tentativa segura |
| Timeout | `AwaitingConfirmation`; consulta antes de reenvio |
| Cancelamentos | evento/protocolo correlacionado |
| Inutilização | faixa/CNPJ/modelo/série/ambiente/protocolo conferem |
| Download | XML autorizado e PDF consistente |
| Restart | estado, sequência e retomada preservados sem duplicidade |

## Encerramento

Anexar evidências sanitizadas ao dossiê e obter aceite da contabilidade. Só então marcar a fase como homologada para o perfil testado. Produção segue [PRODUCTION-READINESS.md](PRODUCTION-READINESS.md).
