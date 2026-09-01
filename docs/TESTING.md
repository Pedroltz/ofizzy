# Testes

- Backend: `dotnet test SportPneus.slnx`.
- Frontend: `npm test -- --watch=false` e `npm run build`.
- Integração usa PostgreSQL real via Testcontainers e requer Docker.
- Antes de concluir uma fase, validar o fluxo completo pelo Nginx e registrar comandos/resultados em `IMPLEMENTATION-LOG.md`.
