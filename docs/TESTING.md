# Testes

- Backend: `dotnet build SportPneus.slnx --configuration Release` e `dotnet test SportPneus.slnx --configuration Release`.
- Frontend: `npm run lint`, `npm test`, `npm run e2e` e `npm run build`. Em execução sem TTY, o runner Vitest encerra após uma passagem; não use `--run`, opção não reconhecida pelo builder Angular atual.
- Instale o navegador E2E uma vez com `npx playwright install chromium`. O Playwright inicia o servidor em `127.0.0.1:4300` e usa mocks de API determinísticos, sem alterar o banco da oficina.
- Integração usa PostgreSQL real via Testcontainers e requer Docker.
- Antes de concluir uma fase, validar o fluxo completo pelo Nginx e registrar comandos/resultados em `IMPLEMENTATION-LOG.md`.

## Baseline atual

- Backend: 14 testes unitários e 3 testes de integração aprovados.
- Frontend: 22 testes Vitest aprovados em 4 arquivos de teste.
- E2E responsivo: 23 cenários aprovados e 4 ignorados por não se aplicarem ao viewport, em projetos desktop, Pixel 7 e tablet.
- Build frontend aprovado com aviso não bloqueante: bundle inicial de aproximadamente 736 kB para orçamento de 500 kB.
