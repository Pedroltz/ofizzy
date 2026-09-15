# Continuidade do Ofizzy

Atualizado em 13/09/2026. Trabalhar em `develop`; `main` é exclusiva de releases. Ler `docs/PROJECT.md`, `docs/STATUS.md`, este arquivo, `docs/ARCHITECTURE.md` e a [fase ativa](docs/phases/PHASE-08-FISCAL.md) antes de alterar código.

## Ponto de retomada

O usuário priorizou emissão oficial de serviços e produtos a partir da OS, sem intermediário pago. Piloto já informado: **Igaraçu do Tietê/SP, Simples Nacional, IBGE 3520004**. Não perguntar novamente município/regime. O certificado A1 é uma dependência de homologação; não foi fornecido nem usado certificado real nos testes.

Fundação SaaS e fases 1–4 têm aceite local. Fiscal está implementado localmente com leiautes regulamentares concluídos (DANFE com canhoto e barcode, DANFSe com QR Code e detalhamento tributário, regras obrigatórias do Simples Nacional em infAdic/infCpl e infoCompl/xInfComp). Homologação externa pendente. Financeiro não começou; pagamentos declarados na NF-e não registram recebimentos.

## Implementação para localizar

- Backend: `src/backend/Ofizzy.Api/BusinessCore/Fiscal/`. Serviços concretos, DTOs e regras; um `ApplicationDbContext`.
- `FiscalEmissionService`: preparação, reserva, assinatura, envio/consulta e cancelamento; mantém identidade e XML em resultado inconclusivo.
- `FiscalGateway`: comunicação oficial direta e correlação de resposta; `DevSimulatedFiscalGateway`: gateway simulado local ativo exclusivamente em `Development` (`Fiscal:SimulateGateway: true`); `FiscalDevController`: gerador de certificado A1 autoassinado para testes locais; `FiscalXml`: assinatura, XSD local, regras do Simples Nacional (`infAdic`/`infCpl`) e DPS (`infoCompl`); `FiscalPdf`: DANFE oficial com canhoto destacável e DANFSe nacional com QR Code; `FiscalCertificateVault`: A1 cifrado com chave externa versionada.
- `FiscalInutilizationsController`: histórico e recuperação de intervalo, XML original, lease de dois minutos, confirmação estrita de protocolo e identificação. Incerteza mantém números reservados.
- Frontend: `src/frontend/ofizzy-web/src/app/features/fiscal/`, integrado às configurações e detalhes da OS finalizada. Manter PrimeNG 21, tokens, desktop e 320/768 px.
- Migrations fiscais: `AddFiscalFoundation`, `CompleteFiscalInutilization`, `AddFiscalInutilizationLease`. Dez migrations no banco de smoke; não remover históricos ou reescrever migrations aplicadas.

## Restrições que devem permanecer

Todas as entidades fiscais usam TenantId, filtros e FKs compostas. Owner/Admin administra configuração, perfis, certificado, cancelamento e inutilização; membros preparam/emitem/baixam conforme permissões. Não expor entidades EF ou certificados nos DTOs/logs.

Uma OS mista tem NF-e e NFS-e independentes. Autorização parcial não autoriza reenviar o documento já autorizado. Resultado inconclusivo exige consulta/recuperação; não liberar numeração nem substituir o XML assinado. Downloads dependem de XML autorizado persistido. Cancelamento preserva histórico.

Produção permanece condicionada à liberação do operador da plataforma em `/plataforma` (`Tenant.FiscalProductionReleased`), com suporte mantido a `Fiscal:ProductionEnabled` e `Fiscal:HomologatedTenants`. O simulador de desenvolvimento nunca opera em ambiente de produção. XSD NF-e incorporado é baseline antigo, não prova conformidade vigente. DANFE e DANFSe atendem aos manuais e foram testados com fixtures autorizadas; homologação oficial permanece obrigatória antes de produção. Não apresentar teste com gateway substituído como homologação oficial.

## Retomar o trabalho

Seguir [Próximos passos](docs/NEXT-STEPS.md). [FISCAL.md](docs/FISCAL.md) contém configuração, contratos e fontes. Não há autorização implícita para transmitir notas reais ou publicar produção sem homologação comprovada.

Desenvolvimento habitual: PostgreSQL via `compose.local.yaml`, API no host (`dotnet run --project src/backend/Ofizzy.Api --launch-profile local`) e Angular (`npm start` em `src/frontend/ofizzy-web`). Proxy 4200 → 5154. Smoke fiscal usa stack isolada `ofizzy-fiscal-smoke`, porta 18082; confirmar estado antes de reutilizar. Não alterar o volume de desenvolvimento para executar smoke.

Último aceite da implementação em 15/09/2026: backend build/56 unitários/10 integrações aprovados (100%); frontend lint/40 unitários/build; 21 E2E de plataforma aprovados em Desktop, Tablet e Mobile; migration AddTenantFiscalProductionRelease adicionada. Comandos e limites em [TESTING.md](docs/TESTING.md).

Preservar alterações de código já presentes na árvore. Registrar novas evidências em STATUS, IMPLEMENTATION-LOG e fase 8. O [handoff anterior](docs/archive/HANDOFF-ATE-2026-09-13.md) foi arquivado para consulta histórica; não define tarefas vigentes.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](docs/REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.
