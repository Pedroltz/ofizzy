# Production readiness fiscal

Status em **24/09/2026**: **não liberado**.

## Bloqueios obrigatórios

- [ ] `FiscalReleaseGate` corrigido para gates cumulativos e testes cobrindo combinações.
- [ ] Catálogo de endpoints oficiais revisado, inclusive ADN NFS-e produtivo.
- [ ] Pacotes de schema/versões usados pelo piloto registrados e protegidos.
- [ ] XML/assinatura aceitos em homologação externa para NF-e e NFS-e.
- [ ] Perfil tributário aprovado pela contabilidade.
- [ ] RTC aplicável ao Simples Nacional preparada para 01/01/2027.
- [ ] CNPJ alfanumérico suportado ponta a ponta ou explicitamente bloqueado onde não suportado.
- [ ] Rejeição/correção, timeout/consulta, cancelamento e inutilização comprovados.
- [ ] DANFE/DANFSe conferidos contra XML autorizado.
- [ ] A1, custódia/rotação da chave AES e restauração exercitados.
- [ ] Isolamento por tenant, idempotência, sequência e lease testados.
- [ ] Backup restaurável e procedimento de indisponibilidade aceitos.
- [ ] Evidências sanitizadas e aceite do responsável/contabilidade arquivados.

## Liberação cumulativa

Somente depois dos itens acima: `Fiscal:ProductionEnabled=true`, UUID do tenant em `Fiscal:HomologatedTenants`, `Tenant.FiscalProductionReleased=true`, série exclusiva e responsável registrados.

Ausência de qualquer condição deve bloquear emissão em produção.

## Atualização de implementação — 27/09/2026

Gates cumulativos, origem persistida e endpoints estão corrigidos localmente. Isso não marca a checklist produtiva como aprovada. Permanecem pendentes homologação externa/A1/credenciamento, cadeia alfanumérica completa, cenários/pacote/resposta/PDF RTC e critérios adicionais de resiliência. Origem desconhecida exige reconciliação. Financeiro manual não executa Split Payment automático.
