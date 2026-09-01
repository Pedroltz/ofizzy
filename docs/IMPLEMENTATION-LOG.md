# Histórico de implementação

## 2026-09-01 — Fase 1 concluída

- Criados repositório, solução, API, frontend, documentos e estrutura de deploy.
- Implementados setup, login, refresh rotation, logout, CSRF, rate limit e migrations.
- Implementado shell responsivo com PrimeNG 22.1 e Angular 22.1.
- Backend: 5 testes unitários e 1 teste de integração aprovados com PostgreSQL 18 real.
- Frontend: lint aprovado, build de produção aprovado e 1 teste Vitest aprovado.
- Infra: configurações Compose local/produção validadas, imagens construídas e os quatro serviços iniciados saudáveis.
- Smoke test via Nginx: SPA, setup protegido por CSRF e sessão autenticada aprovados.
- O volume usado no smoke test foi removido e recriado; o ambiente final está limpo e retorna `required: true` no setup.

Este arquivo é append-only: correções futuras ganham uma nova entrada.
