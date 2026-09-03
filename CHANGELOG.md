# Changelog

Todas as alterações relevantes serão registradas aqui seguindo Keep a Changelog.

## [Unreleased]

### Added

- Fundação do monólito modular em .NET 10 e Angular 21.
- Setup inicial transacional da oficina e do administrador.
- Autenticação JWT com cookies, refresh rotation, CSRF e logout.
- Shell responsivo PrimeNG/Tailwind e infraestrutura Docker/Nginx/PostgreSQL.
- Migrations, health checks, testes e pipelines de CI/CD.
- Cadastros integrados de clientes, veículos, serviços e peças, com busca, validação, arquivamento e migration `AddCatalogs`.
- Ordens de serviço com numeração sequencial, snapshots históricos, serviços, peças, totalização no backend, estados e migration `AddWorkOrders`.
- Tela integrada para abertura, busca e acompanhamento de ordens de serviço.
- Configuração persistente da oficina, impressão HTML A4 e PDF oficial via QuestPDF.
- Dashboard com endpoint agregado e índices PostgreSQL para clientes, veículos e ordens.
- Experiência responsiva completa para login, setup, dashboard, cadastros, configurações e OS.
- Editor móvel de serviços e peças em cards, com criação/edição de OS em tela cheia.
- Matriz Playwright para desktop, celular e tablet, cobrindo todas as rotas atuais.

### Changed

- Stack frontend fixada em Angular 21 + PrimeNG 21 (MIT), substituindo a tentativa interrompida de Angular Material, conforme ADR 0005.
- Fluxo CSRF pós-login/setup ajustado para renovar o token vinculado ao usuário autenticado.
- Listagens de clientes, veículos e OS passam a usar cards obrigatoriamente no celular, preservando tabela/cards no desktop.
- Busca de clientes, veículos e OS protegida contra respostas assíncronas fora de ordem.
