# Contribuição

- Trabalhe em `develop`; use `main` somente para releases, conforme AGENTS.md.
- Commits Conventional Commits: `feat:`, `fix:`, `refactor:`, `chore:` e `docs:`.
- Antes de abrir PR, execute `dotnet test Ofizzy.slnx`, `npm run lint`, `npm test`, `npm run e2e` e `npm run build`.
- O Playwright requer `npx playwright install chromium` na primeira execução.
- Alterações responsivas devem ser validadas em desktop, celular e tablet, sem overflow horizontal.
- Não versionar `.env`, tokens, senhas, certificados ou dados reais de clientes.
- Atualize `STATUS.md`, `AI-HANDOFF.md`, `IMPLEMENTATION-LOG.md` e o documento da fase com as evidências reais da validação.

## Continuidade e evidências — 13/09/2026

Ler a sequência obrigatória do AGENTS.md e consultar [NEXT-STEPS.md](NEXT-STEPS.md). Distinguir testes executados no incremento de resultados anteriores; registrar limitações e homologação pendente. Não reclassificar fixture/gateway substituído como emissão oficial. Mudanças fiscais exigem preservar isolamento, snapshots e numeração.
