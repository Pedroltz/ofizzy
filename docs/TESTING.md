# Testes

- Backend: `dotnet build SportPneus.slnx --configuration Release` e `dotnet test SportPneus.slnx --configuration Release`.
- Frontend: `npm run lint`, `npm test` e `npm run build`. Em execução sem TTY, o runner Vitest encerra após uma passagem; não use `--run`, opção não reconhecida pelo builder Angular atual.
- Integração usa PostgreSQL real via Testcontainers e requer Docker.
- Antes de concluir uma fase, validar o fluxo completo pelo Nginx e registrar comandos/resultados em `IMPLEMENTATION-LOG.md`.

## Baseline atual

- Backend: 10 testes unitários e 3 testes de integração aprovados.
- Frontend: 1 teste Vitest aprovado.
- Build frontend aprovado com aviso não bloqueante do orçamento inicial de bundle.
