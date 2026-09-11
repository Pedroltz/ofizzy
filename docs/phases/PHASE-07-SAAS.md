# Fase SaaS — multi-tenancy e plataforma modular

Antecipada antes da Fase 5 financeira por decisão de produto em 2026-09-09.
ADR: `docs/adr/0006-saas-multi-tenancy.md`.

## Implementação

- [x] Discovery de entidades, DTOs, auth/refresh, Company, migrations/índices/FKs,
  WorkOrders, frontend/cache/guards, Docker/Nginx e CI.
- [x] Tenant, TenantUser, TenantSettings, vertical Automotive e módulos tipados.
- [x] Contexto scoped validado no banco, roles e policy PlatformAdmin independente.
- [x] TenantId em todos os dados operacionais, filtros EF e proteção de escrita.
- [x] FKs/índices compostos e numeração transacional independente por tenant.
- [x] Migration legada preservando IDs, snapshots, hashes e números.
- [x] Login único/múltiplos tenants, seleção/troca e rotação de refresh serializada.
- [x] Provisionamento transacional, template e associação segura de usuário existente.
- [x] APIs/UI reais de administração global, estados e módulos.
- [x] Bootstrap de operador separado do onboarding individual por tenant.
- [x] Contexto/menu/guards Angular, cache invalidado e frontend Automotive preservado.
- [x] Testes reais Alpha/Beta, concorrência, suspensão, roles, módulos e replay.
- [x] Aceite final de build/test, Compose/Nginx e persistência após restart.

## Limites intencionais

Somente Automotive é vertical implementada. WorkOrders atual exige Vehicle e módulos
Customers/Catalog/Automotive. Tenant pode operar somente Customers/Catalog; futura OS
genérica será adicionada quando existir uma segunda vertical real. Não existem
pagamentos operacionais ou cobrança SaaS implementados. Senha inicial é disponibilizada
pelo operador em canal privado; não há envio automático de convite. Branding usa dados
por tenant existentes; não há editor ou upload de logo novo.

## Evidências — 2026-09-10

Backend: 18 unitários e 7 integrações passaram em PostgreSQL 18 real. Incluem migração
legada, sete entidades operacionais com TenantId, isolamento por ID/mutações, FKs,
numeração concorrente, seleção múltipla e replay de refresh. Falha injetada na inserção de vínculo confirmou rollback completo do provisioning.
Frontend: lint/build aprovados; 28 unitários aprovados com workaround Node 26;
27 E2E determinísticos aprovados e 9 skips por viewport. Build mantém aviso conhecido
(~744 kB para orçamento de 500 kB). Imagens construídas e 7 migrations aplicadas em
banco vazio da stack isolada; quatro serviços saudáveis.


Aceite final Nginx: suíte real passou (1 cenário de criação e 1 cenário após restart,
executados em duas passagens; cada passagem ignora somente o cenário da outra etapa).
Alpha e Beta preservam OS número 1 e dados isolados após reiniciar PostgreSQL/API/
frontend/Nginx. Impressão HTML e PDF exercitados. Novas telas passaram em 320, 768 e
1440 px com controles de pelo menos 44 px. Artefatos fictícios em
`/tmp/ofizzy-live-results` e `/tmp/ofizzy-saas-live.json`; traces desativados.
Deploy remoto não executado. Banco local apenas inspecionado por contagens: uma
configuração, um usuário e nenhum customer/vehicle/work order antes da evolução.

## Desenvolvimento local — 2026-09-10

Por decisão do usuário, somente PostgreSQL permanece em Docker (`compose.local.yaml`,
localhost:5432). API e Angular executam no host: `dotnet run --project
src/backend/Ofizzy.Api --launch-profile local` e `npm start` em
`src/frontend/ofizzy-web`. Perfil local habilita bootstrap e migrations em Development;
proxy Angular liga 4200 à API 5154. Instruções completas no README.
Stack Docker de smoke parada, volumes preservados. PostgreSQL nativo instalado
durante a avaliação foi parado; não é utilizado nem necessário neste fluxo.
Banco de desenvolvimento confirmado com zero usuários e tenants após migrations.
Smoke local: `/health/ready` via Angular retornou Healthy e `/api/setup/status`
retornou required=true. Backend build, 18 unitários e 7 integrações aprovados;
frontend lint, 28 unitários e build aprovados (aviso de bundle conhecido, 744 kB).
E2E determinístico nesta execução: 25 aprovados, 9 skips e 2 falhas de timeout
ao aguardar main (mobile / e tablet /ordens); ambos passaram na repetição
isolada com um worker (2/2). Sem alterações visuais
ou de domínio; aceite real Nginx/Alpha/Beta da fase SaaS permanece documentado acima.

## Formulário de provisionamento — 2026-09-10

Tela organizada com Fieldset, Message e controles PrimeNG existentes, em seções
de empresa, administrador e módulos. Identificador converte maiúsculas para
minúsculas; envio inválido mostra mensagens nos campos, sem botão silenciosamente
desabilitado. Contratos/backend e dados preservados.
Validação frontend: lint/build, 28 unitários, 30 E2E aprovados e 9 skips condicionais.
Após ajuste final de CSS, 3 E2E direcionados aprovados em 1440/768/320 px, sem
overflow e campos/botão com pelo menos 44 px. Teste usa API interceptada apenas
no Playwright; produção mantém chamadas reais. Não reexecutados smoke de criação
real, Compose/migrations ou backend nesta mudança exclusiva de interface.
Aviso de bundle permanece (~749 kB). Capturas em /tmp/ofizzy-platform-*.png.
