# Roadmap

Prioridade vigente: fundação SaaS multi-tenant ([Fase SaaS](phases/PHASE-07-SAAS.md)), antecipada antes do financeiro. Automotive preservada. Novos tenants são provisionados sem deploy.


- [x] Fase 1 — Fundação: projetos, banco, migration, setup, autenticação, shell, containers e CI.
- [x] Fase 2 — Clientes, veículos, serviços e peças.
- [x] Fase 3 — Ordem de Serviço, snapshots, valores e estados (abertura, listagem, detalhes, edição, transições e persistência validadas).
- [x] Fase 4 — Configuração da oficina, impressão HTML e PDF QuestPDF.
- [ ] Fase 5 — Pagamentos e financeiro básico.
- [ ] Fase 6 — Histórico, busca, UX final, E2E, logs e backup/restauração. Responsividade final e matriz E2E foram antecipadas; os demais itens permanecem pendentes.

Os critérios detalhados vivem em `phases/`. Um item só é concluído após build/teste e registro no histórico.

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
- [x] Configurações
- [x] Serviços
- [x] Peças
- [x] Empresa
- [x] Dark Mode

### R9 — Autenticação
- [x] Login
- [x] Setup inicial
- [x] Dark Mode
- [x] Estados de erro
- [x] Estados de loading

### R10 — Polish
- [x] Skeletons
- [x] Empty states
- [x] Feedback visual
- [x] Toasts
- [x] Focus states
- [x] Keyboard navigation
- [x] Acessibilidade
- [x] Testes E2E
- [x] Mobile 320px
- [x] Tablet
- [x] Desktop
- [x] Light Mode
- [x] Dark Mode
- [x] System Theme

## Plataforma SaaS

- Fundação tenancy, vínculos, papéis e módulos.
- Migração legada e isolamento automático com integridade relacional.
- Autenticação com seleção/troca de tenant e refresh preservado.
- Provisionamento transacional e administração global separada.
- Onboarding e frontend Automotive adaptado.
- Aceite PostgreSQL real, Nginx, responsividade e persistência após restart.

Evoluções posteriores: convites e recuperação de acesso, autosserviço com
SaaS Subscriptions, outras verticais e branding com upload. Nenhuma dessas
extensões demanda agora microserviços, bancos dedicados ou frontend por cliente.
