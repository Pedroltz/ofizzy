# Fase 2 — Cadastros

Clientes, veículos, serviços e peças com DTOs, validação, arquivamento, busca/autocomplete e cadastro inline.

Aceite concluído em 2026-09-01: cada fluxo funciona ponta a ponta pelo frontend e os dados são persistidos por migration do EF Core. A UI usa Angular 21, PrimeNG 21 (MIT) e Tailwind, conforme ADR 0005; PrimeNG 22 não é utilizado.

## Entregas

- [x] Clientes com busca, validação, edição e arquivamento em cascata dos veículos ativos.
- [x] Veículos vinculados a clientes, placa normalizada e busca.
- [x] Catálogos de serviços e peças com preços e arquivamento lógico.
- [x] DTOs, FluentValidation, índices e migration `AddCatalogs`.
- [x] Páginas Angular integradas às APIs e persistência PostgreSQL.

## Evidências

- 9 testes unitários e 2 testes de integração aprovados no aceite da fase.
- Lint, teste e build do frontend aprovados.
