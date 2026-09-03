# Roadmap

- [x] Fase 1 — Fundação: projetos, banco, migration, setup, autenticação, shell, containers e CI.
- [x] Fase 2 — Clientes, veículos, serviços e peças.
- [x] Fase 3 — Ordem de Serviço, snapshots, valores e estados (abertura, listagem, detalhes, edição, transições e persistência validadas).
- [x] Fase 4 — Configuração da oficina, impressão HTML e PDF QuestPDF.
- [ ] Fase 5 — Pagamentos e financeiro básico.
- [ ] Fase 6 — Histórico, busca, UX final, E2E, logs e backup/restauração. Responsividade final e matriz E2E foram antecipadas; os demais itens permanecem pendentes.

Os critérios detalhados vivem em `docs/phases/`. Um item só é concluído após build/teste e registro no histórico.

---

## UI/UX Redesign — Modern Workshop Dashboard

### R1 — Auditoria visual
- [x] Mapear estilos atuais
- [x] Mapear componentes reutilizáveis
- [x] Mapear componentes PrimeNG
- [x] Identificar CSS duplicado
- [x] Identificar inconsistências de layout
- [x] Definir estratégia de migração incremental

### R2 — Design System
- [x] Criar tokens de cores
- [x] Criar tokens tipográficos
- [x] Criar escala de spacing
- [x] Criar radius
- [x] Criar sombras
- [x] Criar estados semânticos
- [x] Implementar Light Mode
- [x] Implementar Dark Mode
- [x] Implementar System Theme
- [x] Implementar persistência do tema
- [x] Criar ThemeService

### R3 — Componentes compartilhados
- [x] PageHeader
- [x] SectionCard
- [x] StatCard
- [x] StatusBadge
- [x] DataToolbar
- [x] SearchField
- [x] EmptyState
- [x] LoadingState
- [x] DataTableWrapper
- [x] ThemeToggle

### R4 — App Shell
- [x] Sidebar desktop
- [x] Header desktop
- [x] Navegação
- [x] Usuário
- [x] Theme Toggle
- [x] Drawer mobile
- [x] Header mobile
- [x] Responsividade

### R5 — Dashboard
- [x] Header operacional
- [x] KPIs
- [x] Veículos na oficina
- [x] Ordens recentes
- [x] Atividade recente
- [x] Distribuição de status
- [x] Dashboard responsivo
- [x] Dark Mode

### R6 — Ordens de Serviço
- [x] Toolbar
- [x] Busca
- [x] Filtros
- [x] Lista desktop
- [x] Cards mobile
- [x] Badges de status
- [x] Refatorar componentes excessivamente grandes
- [x] Editor de OS
- [x] Summary lateral
- [x] Footer mobile fixo
- [x] Dark Mode

### R7 — Clientes e Veículos
- [x] Clientes
- [x] Veículos
- [x] CRUD visual padronizado
- [x] Responsividade
- [x] Dark Mode

### R8 — Configurações e Catálogo
- [ ] Configurações
- [ ] Serviços
- [ ] Peças
- [ ] Empresa
- [ ] Dark Mode

### R9 — Autenticação
- [ ] Login
- [ ] Setup inicial
- [ ] Dark Mode
- [ ] Estados de erro
- [ ] Estados de loading

### R10 — Polish
- [ ] Skeletons
- [ ] Empty states
- [ ] Feedback visual
- [ ] Toasts
- [ ] Focus states
- [ ] Keyboard navigation
- [ ] Acessibilidade
- [ ] Testes E2E
- [ ] Mobile 320px
- [ ] Tablet
- [ ] Desktop
- [ ] Light Mode
- [ ] Dark Mode
- [ ] System Theme

