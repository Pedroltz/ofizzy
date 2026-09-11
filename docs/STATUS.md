# Status do projeto

Atualizado em: 2026-09-10

## Estado vigente — evolução SaaS

Fase ativa: [PHASE-07-SAAS.md](phases/PHASE-07-SAAS.md), antecipada antes do financeiro.
Implementados tenancy, TenantSettings, vínculos, papéis, módulos, isolamento EF/FKs,
migration legada, autenticação multi-tenant, provisionamento, PlatformAdmin e onboarding.
Aceite local concluído em Release e pelo Nginx, com persistência após restart. Evidências consolidadas abaixo.
As seções seguintes preservam o histórico da base Automotive e não definem mais
uma instalação = uma oficina.

## Estado anterior

Fases 1, 2, 3 e 4 concluídas e validadas.

- Módulos de Clientes e Veículos totalmente padronizados com visualização em Lista (padrão inicial) e Blocos (grid responsivo com cards modernos).
- Responsividade Total para Mobile e Tablets:
  - Layout desktop (> 900px) 100% preservado e inalterado.
  - Tablets (641px a 900px): navegação fluida por menu drawer animado, grids em 2 colunas e modais em 94vw com rolagem interna.
  - Celulares (<= 640px): operação completa da oficina na ponta dos dedos; abertura, edição e impressão de OS, gerenciamento de clientes/veículos/catálogo, touch-targets de 44px, grids em 1 coluna, prevenção de zoom no Safari (16px font-size) e rolagem suave táctil em tabelas.
  - Criação/edição de OS em tela cheia móvel (`100dvh`), com serviços e peças em cards editáveis, total e ações sempre acessíveis e suporte às áreas seguras do aparelho.
  - Listagens de clientes, veículos e OS usam obrigatoriamente cards no celular, mantendo a preferência tabela/cards exclusiva do desktop.
  - Playwright cobre todas as rotas em desktop, Pixel 7 e tablet: 23 cenários aprovados e 4 ignorados por não se aplicarem ao viewport.
- Navegação lateral modernizada com identidade tipográfica sóbria, grupos de Operação/Gestão, estado ativo acessível e uma única fonte de rotas para desktop e drawer móvel.
- Otimizações de performance implementadas e validadas:
  - Nginx com compressão gzip (nível 6), cache permanente imutável de 1 ano para assets estáticos e proxy keep-alive HTTP 1.1.
  - Endpoint dedicado e atômico `GET /api/dashboard/summary` que substitui 3 chamadas concorrentes pesadas por 1 consulta agregada ultraveloz (< 15ms).
  - Índices de performance no PostgreSQL aplicados via migration `AddPerformanceIndexes` em `work_orders` (`Status`, `CreatedAt`, `Status_CreatedAt`, `CustomerName`, `VehiclePlate`), `vehicles` (`IsActive`, `CustomerId`) e `customers` (`IsActive`).
  - Proteção contra race conditions e debounce otimizado (300ms) com versionamento de requisição nas buscas de Clientes, Veículos e Ordens de Serviço.
- Módulo de Ordens de Serviço completo: abertura, listagem, visualização detalhada, edição (em aberto/em andamento), transições de estado com confirmação e persistência total no PostgreSQL.
- Módulo de Configurações da Oficina (`/configuracoes` -> Dados da Oficina): persistência no PostgreSQL de Razão Social, Nome Fantasia, CNPJ, Telefones, WhatsApp, Endereço e Termo de Garantia da OS.
- Impressão A4 minimalista e profissional: modelo limpo baseado em texto e linhas divisórias (`.wo-print-sheet`), sem fundos que gastem tinta, com cabeçalho, dados do cliente/veículo, apontamentos, tabelas de serviços/peças, totais e termo com campos de assinatura.
- Geração de PDF no backend com QuestPDF: endpoint `GET /api/work-orders/{id}/pdf` emitindo arquivo `OS-0001.pdf` diagramado em A4 com suporte a múltiplas páginas.
- Stack Docker Compose (PostgreSQL, Backend, Frontend e Nginx) 100% saudável e testada com persistência após reinício na porta 8080.

## Próximo marco

Fase 5 — Financeiro e Pagamentos (registro de pagamentos, formas de pagamento, parcelas, caixa e fechamento financeiro).

## Atenções

- PrimeNG 22 não será utilizado por causa do seu modelo de licença. A decisão vigente está em `adr/0005-primeng-21-mit.md`.
- O build do frontend passa, com aviso não bloqueante de bundle inicial: ~708 kB para um orçamento de 500 kB. Revisar esse orçamento ou otimizar dependências antes do primeiro deploy público.
- O deploy remoto depende da configuração dos secrets e do servidor descrita em `DEPLOYMENT.md`.
- O histórico de migrations no volume local foi reconciliado e atualizado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders`, `AddWorkshopSettings` aplicadas com sucesso).

## UI Redesign & Visual Consistency

### Redesenho e conforto visual dos blocos de clientes e veículos — 2026-09-11

- Otimizado o layout dos cards (`catalog-card`) com `align-items: stretch` e `width: 100%`, eliminando o encolhimento e centralização forçada dos elementos internos.
- Clientes: agrupamento harmônico de Avatar, Nome e E-mail à esquerda com Documento à direita; máscara automática de telefone `(DD) 9XXXX-XXXX`.
- Veículos: placa em destaque à esquerda e ano/cor à direita; especificações e observações ocupando a largura integral do frame.
- Validação: lint, build, 39 testes unitários e 28 testes e2e Playwright aprovados.

### Persistência do modo de visualização (cards/tabela) — 2026-09-11

- Implementado `ViewPreferenceService` conectando a preferência do usuário (tabela vs. blocos) ao `localStorage` com cache em memória e signals reativos.
- A preferência é memorizada individualmente por tela (`/clientes`, `/veiculos`, `/ordens`, `/plataforma`), permanecendo ativa ao trocar de rota ou atualizar a página.
- Validação: lint, build, 39 testes unitários (6 no serviço) e 28 testes e2e Playwright aprovados.

### Ajuste na tabela de clientes e formatação de documentos — 2026-09-11

- Eliminada quebra de linha no badge de Documento (CPF/CNPJ) na listagem tabular de clientes através de `white-space: nowrap`, `flex-shrink: 0` e largura dedicada de `12.5rem`.
- Grade de clientes simplificada de 8 para 6 colunas, removendo colunas secundárias de cadastro e WhatsApp da tabela (mantidas na íntegra na ficha ao clicar e na visualização mobile por cartões).
- Validação: lint, build, 33 testes unitários e 27 testes responsivos Playwright aprovados em 320px, 768px e 1440px.

### Ajuste visual da lista de Ordens de Serviço — 2026-09-04

- Ajustado o modo lista de `/ordens` com espaçamento lateral interno maior nas extremidades, colunas mais equilibradas e área de ações dimensionada para os cinco botões possíveis.
- O modo bloco e o comportamento responsivo permanecem inalterados.
- Evidência: classe específica `.work-orders-list-table` aplicada no template; validação final pelo build do frontend.
- Aplicado o mesmo respiro e alinhamento à tabela de Ordens ativas da visão geral da Oficina, com classe exclusiva `.dashboard-work-orders-table`.
- Corrigida a impressão de OS longas: a ficha é posicionada no topo da impressão e a seção de termos/assinaturas é mantida junta em uma quebra de página.
- Corrigida a página em branco na impressão: o shell e os diálogos da SPA são removidos do layout impresso, preservando somente a ficha da OS.
- Correção complementar: status reposicionado para a segunda coluna, antes de Cliente/Veículo, com largura de `9.5rem` para evitar corte em “Em andamento”.
- A listagem de OS agora abre a ficha ao clicar em qualquer ponto da linha; ações de fluxo, edição e cancelamento foram agrupadas em um controle compacto.
- Clientes e Veículos seguem o mesmo padrão: linha inteira abre a edição, ações agrupadas e colunas secundárias ocultadas em telas médias para evitar rolagem horizontal.
- Refatorados os cards de Clientes e Veículos: estrutura interativa unificada, ações com botões HTML explícitos e cores semânticas garantem ícones visíveis em todos os temas.
- Corrigido o ícone de arquivamento: `pi-archive` não existe na versão instalada do PrimeIcons e foi substituído por `pi-folder-open` nas telas e confirmações de Clientes e Veículos.
- Removido o atalho duplicado de Nova Ordem de Serviço da Visão Geral; a criação permanece no cabeçalho global. O seletor de tema foi centralizado na barra lateral e no rodapé do menu móvel, sem duplicação no cabeçalho.
- A célula de status também foi protegida contra clipping, mantendo o badge inteiro e imediatamente antes de Cliente/Veículo.
- O status foi elevado para a primeira coluna da lista, antes do número da OS e de Cliente/Veículo, eliminando qualquer ambiguidade visual.
- Ordem final confirmada na lista: `OS → Cliente / Veículo → Status → Entrada → Total → Ações`.

Fase atual:
Revisão e Correção Visual Concluída — Dark/Light Mode 100% Baseado em Tokens, Grids Padronizados e Tabelas Compactas.

Última tarefa concluída:
Eliminação de cores hardcoded e classes literais em todo o projeto, migração para tokens semânticos (`var(--surface-*)`, `var(--text-*)`, `var(--border-*)`, etc.), harmonização temática de componentes PrimeNG (`p-dialog`, `p-autocomplete`, `p-paginator`, `p-tooltip`, `select`), padronização de grids (`minmax(min(100%, 320px), 1fr)`) e cartões de Clientes, Veículos e OS com rodapé sticky, e unificação de ações de tabela compactas (`.tbl-action-btn`).

Próxima tarefa:
Fase 5 — Financeiro e Pagamentos (registro de pagamentos, formas de pagamento, parcelas, caixa e fechamento financeiro).

Arquivos principais alterados:
- `src/frontend/ofizzy-web/src/styles.css`
- `src/frontend/ofizzy-web/src/app/features/work-orders/work-orders.page.html`
- `src/frontend/ofizzy-web/src/app/features/customers/customers.page.html`
- `src/frontend/ofizzy-web/src/app/features/vehicles/vehicles.page.html`

Pendências:
- Nenhuma. Dark mode, grids e tabelas 100% consistentes e validados.

Problemas conhecidos:
- Aviso não bloqueante de budget de bundle inicial no build de produção mantido (~730 kB contra 500 kB).

Testes:
- `npm run lint`: 0 erros e 0 avisos.
- `npm test`: 22/22 testes unitários aprovados.
- `npm run build`: bundle compilado com sucesso.
- `npm run e2e`: 23 cenários aprovados pelo Playwright em desktop, Pixel 7 e tablet.

## Performance de navegação e marca Ofizzy — 2026-09-04

- Rotas lazy agora são pré-carregadas em segundo plano, reduzindo a espera no primeiro acesso às telas.
- Dashboard, Clientes, Veículos, Ordens, Configurações e catálogos da OS reutilizam cache de sessão por 30 segundos, com deduplicação, invalidação após mutações e limpeza no logout.
- Ao retornar para uma listagem, os dados já obtidos aparecem imediatamente; snapshots expirados permanecem visíveis durante a revalidação.
- A marca visível Ofizzy foi aplicada ao shell, login, título, impressão, PDF, defaults e documentação.
- Evidências: frontend lint aprovado; 26/26 testes unitários; build aprovado; 27 E2E aprovados e 9 ignorados condicionalmente por viewport; backend 14/14 testes unitários e 3/3 de integração.
- O build mantém o aviso conhecido de budget inicial em 739,06 kB. Não houve alteração de modelo ou nova migration.
- Dashboard responsivo por largura real do conteúdo: entre 901 e 1400 px, a lista usa a largura total e a lateral é movida para baixo; abaixo de 46rem na coluna principal, a tabela troca para cards. As colunas fixas ficam compactas em notebooks, preservando espaço para Cliente/Veículo.
- A listagem completa de OS agora compacta colunas auxiliares entre 901 e 1400 px, reserva mais de 200 px para Cliente/Veículo em notebook de 1180 px e força cards em tablets/celulares. Teste E2E direcionado aprovado em desktop e tablet.
- Limpeza conservadora removeu estado e método não utilizados, consolidou regra CSS duplicada e eliminou logging local redundante; validações de domínio, segurança e entrada foram preservadas.

## Identidade técnica Ofizzy — 2026-09-08

- Solução, projetos e namespaces backend usam `Ofizzy.*`; o frontend usa `ofizzy-web`.
- Compose, imagens, banco, usuário, schema, cookies e exemplos usam identificadores Ofizzy.
- Migration idempotente e script de backup/restauração adicionados para preservar dados em um volume separado e permitir rollback.
- Contratos HTTP e regras de negócio permaneceram inalterados. A troca dos cookies encerra as sessões existentes de forma intencional.
- O repositório remoto foi renomeado para `Pedroltz/ofizzy` após a validação local; a migração do ambiente publicado continua exigindo a janela documentada de backup e restauração.
- Validação local: backend build sem avisos, 14/14 unitários e 4/4 integrações; frontend lint, 26/26 unitários, build e 27 E2E aprovados com 9 skips condicionais.
- Compose criou banco/schema `ofizzy`, aplicou 6 migrations, deixou os quatro serviços saudáveis pelo Nginx e preservou autenticação após reinício do backend.
- Permanece apenas o aviso conhecido do bundle inicial do frontend: 740,72 kB para orçamento de 500 kB.
- Workflow remoto `Pull request` aprovado no commit `4670c67`: backend, frontend e validação Compose concluídos com sucesso.

## Execução local simplificada — 2026-09-09

- `compose.local.yaml` agora é autônomo e inicia somente PostgreSQL 18, exposto exclusivamente em `127.0.0.1:5432`, com volume persistente `ofizzy_postgres_data`.
- No perfil `Development`, `dotnet run` usa defaults locais sem secrets persistidos, gera uma chave JWT efêmera e aplica automaticamente as migrations.
- Produção continua exigindo `ConnectionStrings:Postgres` e uma chave JWT real com ao menos 32 bytes.
- Evidências: backend build sem avisos; 14/14 testes unitários e 4/4 de integração; API iniciada com `dotnet run`; `/health/live` e `/health/ready` saudáveis; 6 migrations aplicadas; frontend lint e build aprovados.
- Validações frontend não relacionadas ao ajuste ficaram limitadas pelo ambiente: testes unitários falham porque `localStorage` não é fornecido pelo runner atual em Node 26; E2E requer baixar o Chromium do Playwright. O build mantém o aviso conhecido de 740,72 kB.
- Corrigida a reinicialização da API em bancos onde o EF manteve históricos em `public` e `ofizzy`: a inicialização reconcilia os registros de forma bidirecional e idempotente antes/depois da migration. Duas execuções consecutivas de `dotnet run` foram validadas sem reaplicar migrations; ambos os históricos registram as 6 migrations.
- Corrigida a configuração inicial com CNPJ formatado: o backend aceita tanto `12345678000190` quanto `12.345.678/0001-90`, valida os 14 dígitos e persiste o valor normalizado.
Atualização visual — 2026-09-09: removido o brand-mark do login. Seletor de tema de login/setup abre abaixo e alinhado à direita; sidebar mantém abertura acima. Controles com mínimo de 44 px. Lint e build frontend aprovados (aviso de bundle conhecido). Nenhuma alteração de API ou migration nesta tarefa.


## Aceite SaaS — 2026-09-10

- Backend Release: build sem warnings/erros; 18 testes unitários e 7 integrações
  aprovados, incluindo falha injetada de provisioning com rollback completo.
- Frontend: lint aprovado, 28 unitários aprovados, build aprovado (aviso conhecido
  de 743,95 kB para budget 500 kB). Node 26 exige o workaround de Web Storage em TESTING.
- E2E determinístico: 27 aprovados, 9 skips condicionais por viewport.
- E2E real Nginx: bootstrap de operador, provisionamento Alpha pela UI, Beta pela API,
  onboarding dos dois, login, dashboard, clientes, veículos, criação/edição de OS,
  impressão HTML e PDF aprovados. Ambos OS 1; IDs do outro tenant retornam 404.
- Novas telas verificadas em 1440/768/320 px, sem overflow e controles com 44 px.
- Compose: imagens construídas, 7 migrations aplicadas em banco vazio; serviços com
  healthchecks saudáveis e Nginx respondendo. Reinício de toda a stack preservou
  IDs, OS 1 e diagnósticos dos dois tenants, verificados por novo login.
- Migration também validada sobre dados legados fictícios em PostgreSQL 18,
  preservando hashes, IDs, snapshots e numeração histórica 42.
- Stack de aceite isolada `ofizzy-saas-smoke`, porta 18081; banco local não alterado.
  Deploy remoto não executado. Bootstrap deve ser privado/temporário; migração de
  produção exige backup e janela sem writers antigos, conforme DEPLOYMENT.

Próximo marco funcional: financeiro operacional tenant-scoped. Convites automáticos,
recuperação de acesso, novas verticais, upload de logo e cobrança SaaS ficam futuros.

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

## Organização modular por tipos e verticais — 2026-09-10

- Reorganizada a arquitetura do backend em fronteiras explícitas:
  - `Platform/`: Autenticação, Tenancy e Company Settings (TenantSettings).
  - `BusinessCore/`: Clientes, Catálogo (Services e Parts), Ordens de Serviço e Dashboard.
  - `Verticals/`: Verticais de negócio especializadas por nicho, iniciando com `Automotive/` (Veículos e contratos automotivos).
- Namespace `Ofizzy.Api.Verticals.Automotive` reflete a vertical de veículos sem alterar contratos HTTP (`/api/vehicles`).
- Snapshot do EF Core sincronizado; compilação do backend limpa sem avisos.
- Validação backend: build com êxito, 18 testes unitários e 7 testes de integração aprovados.
- Validação frontend: lint e build aprovados com sucesso.

