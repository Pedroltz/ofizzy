# Status do projeto

Atualizado em: 2026-09-03

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

- PrimeNG 22 não será utilizado por causa do seu modelo de licença. A decisão vigente está em `docs/adr/0005-primeng-21-mit.md`.
- O build do frontend passa, com aviso não bloqueante de bundle inicial: ~708 kB para um orçamento de 500 kB. Revisar esse orçamento ou otimizar dependências antes do primeiro deploy público.
- O deploy remoto depende da configuração dos secrets e do servidor descrita em `docs/DEPLOYMENT.md`.
- O histórico de migrations no volume local foi reconciliado e atualizado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders`, `AddWorkshopSettings` aplicadas com sucesso).

## UI Redesign

Fase atual:
R10 concluído — UI/UX Redesign Integralmente Finalizado (R1 a R10)

Última tarefa concluída:
R10 — Polish (Scrollbars discretas com CSS nativo, anéis de foco acessíveis :focus-visible, atalho global '/' para focar pesquisa com indicador <kbd>/</kbd>, skeletons e empty states uniformizados e validação responsiva total)

Próxima tarefa:
Aguardando novas diretrizes do usuário (Sistema totalmente modernizado e validado).

Arquivos principais alterados:
- `src/frontend/sport-pneus-web/src/app/shared/components/search-field.component.ts`
- `src/frontend/sport-pneus-web/src/app/layout/app-shell.component.ts`
- `src/frontend/sport-pneus-web/src/styles.css`

Pendências:
- Nenhuma. Todas as 10 fases do redesign foram concluídas com sucesso.

Problemas conhecidos:
- Aviso não bloqueante de budget de bundle inicial no build de produção mantido (~722 kB contra 500 kB).

Testes:
- `npm test`: 22/22 testes unitários aprovados.
- `npm run lint`: 0 erros e 0 avisos.
- `npm run build`: bundle compilado com sucesso.
- `npm run e2e`: 23 cenários aprovados pelo Playwright em desktop, Pixel 7 e tablet.










