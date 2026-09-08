# Status do projeto

Atualizado em: 2026-09-08

## Estado

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
- A migração do ambiente publicado e a renomeação externa do repositório devem ocorrer somente depois da validação local descrita nesta entrega.
- Validação local: backend build sem avisos, 14/14 unitários e 4/4 integrações; frontend lint, 26/26 unitários, build e 27 E2E aprovados com 9 skips condicionais.
- Compose criou banco/schema `ofizzy`, aplicou 6 migrations, deixou os quatro serviços saudáveis pelo Nginx e preservou autenticação após reinício do backend.
- Permanece apenas o aviso conhecido do bundle inicial do frontend: 740,72 kB para orçamento de 500 kB.
