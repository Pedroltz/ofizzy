# Fase 1 — Fundação

## Entregas

- [x] Solução .NET e Angular.
- [x] PostgreSQL e migration inicial.
- [x] Setup único de oficina/administrador.
- [x] Login, refresh rotation, logout, CSRF e rate limit.
- [x] Shell responsivo PrimeNG/Tailwind.
- [x] Dockerfiles, Compose, Nginx e health checks.
- [x] CI de PR e deploy da main.
- [x] Documentação persistente.

## Evidências

- `dotnet test`: 5 testes unitários e 1 integração aprovados com PostgreSQL 18/Testcontainers.
- `npm run lint`, `npm run build` e `npm test -- --watch=false`: aprovados.
- `docker compose config` local e produção: aprovados.
- Imagens backend/frontend: construídas com sucesso.
- PostgreSQL, backend e frontend: healthy; Nginx ativo.
- Migration executada em banco vazio e smoke HTTP completo aprovado.
- Banco final recriado sem usuário para preservar a experiência de primeiro acesso.
