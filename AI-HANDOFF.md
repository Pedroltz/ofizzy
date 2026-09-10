# Handoff para IA

Atualizado em: 2026-09-10

## Prioridade vigente — SaaS (aceite local concluído)

Leia a ADR 0006 e `docs/phases/PHASE-07-SAAS.md` antes de continuar. A fundação
multi-tenant foi antecipada antes do financeiro. Company virou TenantSettings,
preservando tabela `companies` e `/api/company`. CurrentTenant é scoped e validado
no banco; todo dado operacional tem filtro EF e FKs compostas. Sem contexto falha
fechado. Não use IgnoreQueryFilters para operacional ou assuma User.TenantId.

Provisionamento é transacional, com template Automotive e Owner. User é global;
TenantUser contém role/ativo. PlatformAdmin é persistido, não derivado de Owner.
Bootstrap só cria operador, é opt-in e deve ocorrer em acesso privado. Base legada
exige concessão operacional explícita via `--grant-platform-admin <UUID>`.

Frontend: `/organizacoes`, `/plataforma`, `/onboarding`; TenantContextService e
AuthService centralizam contexto. Troca de tenant limpa cache e passa fora do shell.
OS mantém contrato automotivo obrigatório, protegido por módulos. Não generalize
Vehicle artificialmente antes de implementar outra vertical real.

A stack `ofizzy-saas-smoke` usa porta 18081 e volume separado do ambiente local.
Credenciais fictícias ficam em `/tmp/ofizzy-saas-smoke.env`; não as versione.
O E2E real usa `playwright.live.config.ts`, sem mocks nem trace de tokens.
Os registros anteriores abaixo descrevem o histórico e podem ter contagens antigas.

## Leia primeiro

- Produto e limites: `docs/PROJECT.md`.
- Estado atual e próximo passo: `docs/STATUS.md`.
- Decisões técnicas: `docs/adr/`.
- Contratos HTTP: `docs/API.md`.
- Evidências cronológicas: `docs/IMPLEMENTATION-LOG.md`.

## Baseline

- Branch ativa: `develop`; `main` aponta para a última base estável.
- Backend: .NET 10, ASP.NET Core controllers, EF Core/Npgsql, PostgreSQL 18.
- Frontend: Angular 21 standalone, PrimeNG 21 (MIT), PrimeIcons e Tailwind 4. Consulte a ADR 0005 antes de alterar dependências.
- Infra: `compose.yaml`, Nginx como origem única e migrations via serviço `migrate`.
- Sessão: JWT/refresh em cookies HttpOnly, antiforgery `XSRF-TOKEN`/`X-XSRF-TOKEN`.

## Mapa do código

- `src/backend/Ofizzy.Api/Modules`: módulos de negócio.
- `src/backend/Ofizzy.Api/Infrastructure`: EF, migrations e erros.
- `src/frontend/ofizzy-web/src/app/features`: páginas lazy-loaded.
- `src/frontend/ofizzy-web/src/app/core`: sessão, HTTP e clientes de API.
- `docs/phases`: escopo e aceite por fase.

## Comandos sem toolchain no host

Use os containers oficiais descritos no README. Testes de integração precisam do socket Docker. Para execução normal, copie `.env.example` para `.env`, gere secrets e use Compose.

Comandos frontend vigentes: `npm run lint`, `npm test`, `npm run e2e` e `npm run build`. O E2E usa Playwright com APIs determinísticas interceptadas e inicia o Angular em `127.0.0.1:4300`; instale o Chromium uma vez com `npx playwright install chromium`.

A execução direta da API usa .NET User Secrets no perfil `Development`. O `UserSecretsId` está no projeto da API; nunca copie os valores locais para `appsettings*.json`.

## Estado de implementação

Fases 1, 2, 3 e 4 concluídas e aprovadas.

- `Modules/Customers`, `Modules/Vehicles`, `Modules/Services`, `Modules/Parts`, `Modules/WorkOrders` e `Modules/Company` 100% implementados e integrados com o frontend Angular.
- Rotas `/clientes`, `/veiculos`, `/ordens` e `/configuracoes` funcionais com validações visuais inline, formatação de documentos, autocomplete robusto e confirmações de transição/arquivamento.
- Configurações da oficina totalmente gerenciáveis na aba "Dados da Oficina" em `/configuracoes`.
- Impressão A4 minimalista profissional (`.wo-print-sheet`) baseada em texto e linhas divisórias, sem fundos que gastem tinta, acionada com um clique em "Imprimir Ficha".
- Geração de PDF oficial via backend com QuestPDF (download direto via botão "Baixar PDF" na OS ou endpoint `/api/work-orders/{id}/pdf`).
- Histórico de migrations atualizado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders`, `AddWorkshopSettings`).
- Aceite Compose/Nginx realizado com sucesso na porta 8080, validando fluxo completo e persistência após reinício dos contêineres.
- Responsividade antecipada da Fase 6 validada em desktop, Pixel 7 e tablet. O editor de linhas da OS fica em `features/work-orders/components`; o estado responsivo compartilhado fica em `shared/layout`.
- O shell mantém a navegação em uma única coleção no `AppShellComponent`, renderizada na sidebar desktop e no drawer móvel; preserve os grupos Operação/Gestão ao adicionar rotas.
- Sistema 100% harmonizado com Design Tokens (`var(--surface-*)`, `var(--text-*)`, `var(--border-*)`, etc.) e suporte nativo completo para Light e Dark Mode via `[data-theme]`. Nunca reintroduzir cores literais ou classes `text-slate-*`/`bg-slate-*` no HTML/CSS.
- Ações de tabela padronizadas com `.table-actions-group` e `.tbl-action-btn` (32px x 32px, acessíveis com tooltip e aria-label). Grids de cartões padronizados com `repeat(auto-fill, minmax(min(100%, 320px), 1fr))` e rodapé sticky.

## Cuidados conhecidos

### Ajuste visual mais recente (2026-09-04)

- A tabela do modo lista de Ordens de Serviço recebeu a classe `.work-orders-list-table`, com respiro adicional nas bordas, larguras de coluna revisadas e alinhamento consistente do grupo de ações.
- A alteração é exclusiva da tabela de OS; o modo bloco e a regra de cards em telas pequenas não foram alterados.
- O status fica na segunda coluna da lista, antes de Cliente/Veículo, com espaço reservado para textos longos como “Em andamento”.
- A célula usa `white-space: nowrap` e `overflow: visible` para impedir que o badge seja cortado.
- Na lista de OS, a linha inteira abre os detalhes; a coluna Ações contém um grupo compacto com apenas as operações contextuais restantes.
- Clientes e Veículos também usam linhas clicáveis e ações agrupadas; em até 900px, detalhes secundários são ocultados para manter a tabela sem scroll horizontal.
- Cards de Clientes e Veículos usam `.entity-card` e `.entity-card-action`; não use novamente botões PrimeNG text/rounded nesses rodapés, pois os ícones agora têm cor e visibilidade explícitas por CSS.
- Use `pi-folder-open` para arquivamento: `pi-archive` não é um glifo disponível no PrimeIcons instalado.
- O status ocupa a primeira coluna da lista de OS, antes do número e de Cliente/Veículo.
- A tabela de Ordens ativas do Dashboard usa `.dashboard-work-orders-table` para o mesmo espaçamento lateral e proteção visual do status.
- O `app-theme-toggle` fica somente no rodapé da sidebar e do drawer móvel; não o replique nos cabeçalhos.
- A impressão posiciona a ficha no topo da página, repete o cabeçalho de tabela e usa `break-inside: avoid` na seção final para preservar linhas de assinatura em OS longas.
- No modo de impressão, o shell, cabeçalhos e diálogos são removidos do layout; apenas `.wo-print-sheet` permanece para evitar páginas vazias.
- Ordem final da tabela: `OS → Cliente / Veículo → Status → Entrada → Total → Ações`.

- PostgreSQL 18 monta o volume em `/var/lib/postgresql`, não em `/var/lib/postgresql/data`.
- Não atualizar PrimeNG para a major 22. A linha 21 foi fixada para manter a licença MIT (ADR 0005).
- O deploy real exige secrets e servidor externos e não pode ser validado apenas localmente.
- Ao atualizar linhas de uma entidade dependente (como serviços e peças da OS), adicione as novas entidades explicitamente aos `DbSets` do EF Core para evitar `DbUpdateConcurrencyException`.

## Atualização de performance e marca — 2026-09-04

- O nome público do produto é **Ofizzy**.
- `SessionDataCacheService` mantém snapshots GET em memória por 30 segundos, deduplica chamadas simultâneas e impede que respostas antigas repovoem o cache após invalidação.
- Mutações invalidam listas dependentes e Dashboard; logout limpa todo o cache. Páginas usam snapshots sem reexibir loading bloqueante ao retornar.
- `PreloadAllModules` antecipa os chunks lazy após a navegação inicial.
- Testes vigentes: frontend 26 unitários e 27 E2E aprovados (9 skips condicionais por viewport); backend 14 unitários e 3 de integração aprovados.
- A lista de OS do Dashboard usa a largura total entre 901 e 1400 px, com os cards laterais abaixo dela e colunas compactas; a container query mantém tabela acima de 46rem e cards abaixo desse limite. Não volte à divisão 8/4 nesse intervalo, pois ela corta Cliente/Veículo.
- Em `/ordens`, `effectiveViewMode` força cards até 900 px; de 901 a 1400 px, `.work-orders-list-table` reduz as colunas auxiliares e preserva Cliente/Veículo. O seletor manual continua valendo em desktops maiores.
- A limpeza final foi deliberadamente conservadora: não remova os validators do frontend/backend nem as checagens de linhas de OS, pois eles fornecem feedback imediato e mantêm a API autoritativa.

## Identidade técnica Ofizzy — 2026-09-08

- Solução, projetos, namespaces, frontend, imagens, Compose, banco, usuário e schema usam exclusivamente `Ofizzy`/`ofizzy`.
- A migration `RenameTechnicalIdentifiersToOfizzy` preserva bancos existentes e não altera IDs registrados em `__EFMigrationsHistory`.
- O script `scripts/migrate-to-ofizzy.sh` faz backup lógico, restaura em um volume separado e mantém a origem intacta para rollback.
- Cookies de sessão e proteção antiforgery receberam novos nomes; após a atualização, todos os usuários precisam autenticar novamente.
- A Fase 5 permanece como próxima entrega funcional.


## Evidência final SaaS — 2026-09-10

Build Release sem warnings, 18 unitários/7 integrações backend; frontend lint,
28 unitários, build; E2E visual 27 aprovados/9 skips; E2E real de criação e após
restart aprovados. Numeração Alpha/Beta = 1, isolamento por ID, PDF/HTML e
persistência demonstrados. Imagens/7 migrations/Compose/Nginx validados na stack
isolada. O bundle de 743,95 kB mantém aviso conhecido. Consulte STATUS e fase SaaS
para comandos e limites. Não confundir evidência local com deploy remoto.
