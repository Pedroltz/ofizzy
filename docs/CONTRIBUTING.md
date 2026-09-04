# Contribuição

- Base: `develop`; produção: `main`; trabalho: `feature/<nome>`.
- Commits Conventional Commits: `feat:`, `fix:`, `refactor:`, `chore:` e `docs:`.
- Antes de abrir PR, execute `dotnet test SportPneus.slnx`, `npm run lint`, `npm test`, `npm run e2e` e `npm run build`.
- O Playwright requer `npx playwright install chromium` na primeira execução.
- Alterações responsivas devem ser validadas em desktop, celular e tablet, sem overflow horizontal.
- Não versionar `.env`, tokens, senhas, certificados ou dados reais de clientes.
- Atualize `STATUS.md`, `IMPLEMENTATION-LOG.md` e o documento da fase com as evidências reais da validação.
