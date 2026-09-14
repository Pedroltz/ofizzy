# Testes

- Backend: `dotnet build Ofizzy.slnx --configuration Release` e `dotnet test Ofizzy.slnx --configuration Release`.
- Frontend: `npm run lint`, `npm test`, `npm run e2e` e `npm run build`. Em execução sem TTY, o runner Vitest encerra após uma passagem; não use `--run`, opção não reconhecida pelo builder Angular atual.
- Instale o navegador E2E uma vez com `npx playwright install chromium`. O Playwright inicia o servidor em `127.0.0.1:4300` e usa mocks de API determinísticos, sem alterar o banco da oficina.
- Integração usa PostgreSQL real via Testcontainers e requer Docker.
- Antes de concluir uma fase, validar o fluxo completo pelo Nginx e registrar comandos/resultados em `IMPLEMENTATION-LOG.md`.

## Baseline da implementação — 13/09/2026

Backend Release: build sem avisos/erros, 40 unitários e 9 integrações aprovados. Frontend: lint, 39 unitários e build aprovados; 52 E2E aprovados e 11 skips condicionais. Bundle inicial 780,45 kB para budget de 500 kB. Dez migrations aplicadas no banco isolado e modelo EF sem mudanças pendentes.

Esses números são evidências da implementação anterior à consolidação documental, não execuções novas a cada edição deste arquivo.

## Aceite SaaS

Integration Tests usam PostgreSQL 18 via Testcontainers. TenantIsolationTests cobre
Mecânica Alpha/João/ABC1D23/OS 1 e Mecânica Beta/Maria/XYZ9Z99/OS 1, ID/mutações,
catálogo, configurações, links cruzados, duplicidade por tenant, concorrência,
suspensão, módulos e vínculo revogado. TenantAuthenticationTests cobre múltiplos
vínculos, seleção/refresh, replay e rollback. TenantMigrationTests aplica a migration
sobre schema e dados legados reais, verificando IDs, snapshots, hashes e constraints.

Frontend unitários adicionais cobrem contexto/permissões e limpeza de cache ao trocar
tenant. CI usa Node 24; no host Node 26, execute
`NODE_OPTIONS=--no-experimental-webstorage npm test -- --watch=false` para impedir
que Web Storage experimental do Node masque o localStorage fornecido pelo jsdom.

`npm run e2e -- --config playwright.live.config.ts` usa a stack real pelo Nginx na
porta 18081, banco vazio descartável e bootstrap habilitado. Não intercepta APIs:
bootstrap, criação pela UI, onboarding, Alpha/Beta, atualização OS e PDF são reais.
Verifica 1440/768/320 px, overflow e altura dos controles nas novas telas. Salva
somente IDs/dados fictícios em `/tmp/ofizzy-saas-live.json`, nunca cookies/tokens.
Após `docker compose ... restart`, execute
`OFIZZY_VERIFY_RESTART=1 npm run e2e -- --config playwright.live.config.ts` para
confirmar persistência. Essa suíte não deve apontar para produção. A suíte
`responsive.spec.ts` continua isolada e determinística, com APIs interceptadas.

## Aceite fiscal e reprodução — 13/09/2026

FiscalEmissionTests usa HTTP, PostgreSQL, certificado fictício e assinatura/XSD reais, substituindo somente o gateway oficial. Cobre emissão mista, timeout/reenvio do mesmo XML, NF-e não reenviada, inutilização/recuperação, protocolo e concorrência. FiscalIsolationTests e verificações de recuperação cobrem papéis e isolamento.

Em src/frontend/ofizzy-web, executar `npm run e2e -- --config playwright.fiscal.config.ts` contra a stack isolada na porta 18082. Após reiniciar somente essa stack, executar `OFIZZY_VERIFY_RESTART=1 npm run e2e -- --config playwright.fiscal.config.ts`. O smoke real usa dados fictícios, trace desligado e confirma persistência, rejeição sem A1 e bloqueio de faixa não reservada. Não transmite a órgão fiscal.

Homologação oficial, conformidade integral de PDF e exercício de restauração permanecem no [plano de próximos passos](NEXT-STEPS.md).
