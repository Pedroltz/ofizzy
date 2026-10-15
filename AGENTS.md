# Instruções para agentes de IA

Leia nesta ordem antes de alterar o projeto: `docs/PROJECT.md`, `docs/STATUS.md`, `AI-HANDOFF.md`, `docs/ARCHITECTURE.md` e o documento da fase ativa em `docs/phases/`.

## Regras obrigatórias

- Trabalhe na branch `develop`; use `main` somente para releases.
- Preserve o monólito modular e evite repositories genéricos, MediatR, CQRS e abstrações sem uso real.
- Não crie telas simuladas: todo fluxo visual deve consumir uma API e persistir no PostgreSQL.
- Não exponha entidades EF; use DTOs e valide entrada.
- Regras e cálculos críticos pertencem ao backend.
- Use PrimeNG para controles e CSS/Tailwind para layout.
- Cadastros são arquivados; documentos históricos não são apagados.
- Nunca versionar `.env`, senhas, tokens, certificados ou dados reais.
- Antes de concluir: backend build/test, frontend lint/test/E2E/build, migration, Compose e smoke do fluxo.
- Mudanças visuais devem preservar desktop e ser verificadas em celular/tablet sem overflow horizontal ou controles menores que 44 px.
- Atualize `docs/STATUS.md`, `AI-HANDOFF.md`, `docs/IMPLEMENTATION-LOG.md` e o documento da fase com evidências reais.
- Use Conventional Commits pequenos e descritivos.

## Definição de pronto

Código compilando não basta. A entrega precisa funcionar pelo Nginx, persistir após reinício, ter validações e erros compreensíveis, testes proporcionais ao risco e documentação suficiente para outra IA continuar sem redescobrir decisões.

## Referências de continuidade — 13/09/2026

A fase ativa é [PHASE-08-FISCAL.md](docs/phases/PHASE-08-FISCAL.md). Consultar [NEXT-STEPS.md](docs/NEXT-STEPS.md) para dependências e critérios de aceite. As regras obrigatórias acima permanecem vigentes; resultados locais não equivalem a homologação externa.
