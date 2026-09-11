# Handoff para IA

Atualizado em: 2026-09-11

## Prioridade vigente — SaaS (aceite local concluído)

Ajustes recentes:
- Planejamento arquitetural de PWA & Offline-First (Store & Forward): especificação de persistência local em `IndexedDB` resistente a quedas de energia e sincronização em segundo plano na nuvem documentada em `docs/ARCHITECTURE.md`, `docs/ROADMAP.md` e `docs/PRODUCT-STRATEGY.md`.
- Redesenho e conforto visual dos blocos de clientes e veículos: preenchimento amplo do frame com `align-items: stretch`, máscara de telefone e agrupamento harmônico de identidade.
- Persistência do modo de visualização (tabela/cards) via `ViewPreferenceService` conectada ao `localStorage`.
- Listagem tabular de clientes otimizada para 6 colunas, com largura de 12.5rem no documento e sem quebras de linha (white-space: nowrap). WhatsApp e cadastro mantidos no modal e nos cartões móveis.

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

- `src/backend/Ofizzy.Api/Platform`: identidade, autenticação, tenancy e configurações da empresa.
- `src/backend/Ofizzy.Api/BusinessCore`: módulos universais de negócio (clientes, catálogo, ordens de serviço, dashboard).
- `src/backend/Ofizzy.Api/Verticals`: verticais de negócio por nicho (atualmente `Automotive` para veículos).
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

## Ambiente local — 2026-09-09

- Para desenvolvimento no host, execute `docker compose -f compose.local.yaml up -d`, entre em `src/backend/Ofizzy.Api` e use apenas `dotnet run`.
- O Compose local contém somente PostgreSQL e reutiliza o volume nomeado `ofizzy_postgres_data`; a porta é publicada apenas no loopback.
- A API em `Development` assume `Host=localhost;Port=5432;Database=ofizzy;Username=ofizzy`, cria uma chave JWT efêmera se nenhuma for configurada e aplica migrations ao iniciar. User Secrets e variáveis de ambiente continuam tendo precedência.
- Não replique esses defaults em produção: fora de `Development`, conexão e chave JWT permanecem obrigatórias.
- O bootstrap reconcilia `public.__EFMigrationsHistory` e `ofizzy.__EFMigrationsHistory` antes/depois de migrar. Isso é necessário porque bancos criados ou renomeados em momentos diferentes podem conter o histórico em schemas distintos; não remova essa compatibilidade sem uma migration de consolidação validada sobre os dois formatos.
- O setup aceita CNPJ com ou sem máscara; a validação conta 14 dígitos e o controller normaliza o valor antes de persistir.
Atualização visual — 2026-09-09: removido o brand-mark do login. Seletor de tema de login/setup abre abaixo e alinhado à direita; sidebar mantém abertura acima. Controles com mínimo de 44 px. Lint e build frontend aprovados (aviso de bundle conhecido). Nenhuma alteração de API ou migration nesta tarefa.


## Evidência final SaaS — 2026-09-10

Build Release sem warnings, 18 unitários/7 integrações backend; frontend lint,
28 unitários, build; E2E visual 27 aprovados/9 skips; E2E real de criação e após
restart aprovados. Numeração Alpha/Beta = 1, isolamento por ID, PDF/HTML e
persistência demonstrados. Imagens/7 migrations/Compose/Nginx validados na stack
isolada. O bundle de 743,95 kB mantém aviso conhecido. Consulte STATUS e fase SaaS
para comandos e limites. Não confundir evidência local com deploy remoto.

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
