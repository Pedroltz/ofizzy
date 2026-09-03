# Handoff para IA

Atualizado em: 2026-09-02

## Leia primeiro

- Produto e limites: `PROJECT.md`.
- Estado atual e próximo passo: `STATUS.md`.
- Decisões técnicas: `docs/adr/`.
- Contratos HTTP: `docs/API.md`.
- Evidências cronológicas: `docs/IMPLEMENTATION-LOG.md`.

## Baseline

- Branch ativa: `develop`; `main` aponta para a última base estável.
- Backend: .NET 10, ASP.NET Core controllers, EF Core/Npgsql, PostgreSQL 18.
- Frontend: Angular 21 standalone, PrimeNG 21 (MIT), PrimeIcons e Tailwind 4. Consulte a ADR 0005 antes de alterar dependências.
- Infra: `compose.yaml`, Nginx como origem única e migrations via serviço `migrate`.
- Sessão: JWT/refresh em cookies HttpOnly, antiforgery `XSRF-TOKEN`/`X-XSRF-TOKEN`.

## Mapa do código

- `src/backend/SportPneus.Api/Modules`: módulos de negócio.
- `src/backend/SportPneus.Api/Infrastructure`: EF, migrations e erros.
- `src/frontend/sport-pneus-web/src/app/features`: páginas lazy-loaded.
- `src/frontend/sport-pneus-web/src/app/core`: sessão, HTTP e clientes de API.
- `docs/phases`: escopo e aceite por fase.

## Comandos sem toolchain no host

Use os containers oficiais descritos no README. Testes de integração precisam do socket Docker. Para execução normal, copie `.env.example` para `.env`, gere secrets e use Compose.

Comandos frontend vigentes: `npm run lint`, `npm test` e `npm run build`. O builder Angular 21 atual não aceita `npm test -- --run` nem necessita `--watch=false` em execução sem TTY.

A execução direta da API usa .NET User Secrets no perfil `Development`. O `UserSecretsId` está no projeto da API; nunca copie os valores locais para `appsettings*.json`.

## Estado de implementação

Fases 1, 2, 3 e 4 concluídas e aprovadas.
- `Modules/Customers`, `Modules/Vehicles`, `Modules/Services`, `Modules/Parts`, `Modules/WorkOrders` e `Modules/Company` 100% implementados e integrados com o frontend Angular.
- Rotas `/clientes`, `/veiculos`, `/ordens` e `/configuracoes` funcionais com validações visuais inline, formatação de documentos, autocomplete robusto e confirmações de transição/arquivamento.
- Configurações da oficina totalmente gerenciáveis na aba "Dados da Oficina" em `/configuracoes`.
- Impressão A4 minimalista profissional (`.wo-print-sheet`) baseada em texto e linhas divisórias, sem fundos que gastem tinta, acionada com um clique em "Imprimir Ficha".
- Geração de PDF oficial via backend com QuestPDF (download direto via botão "Baixar PDF" na OS ou endpoint `/api/work-orders/{id}/pdf`).
- Histórico de migrations atualizado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders`, `AddWorkshopSettings`).
- Aceite Compose/Nginx realizado com sucesso na porta 8080, validando fluxo completo e persistência após reinício dos contêineres.

## Cuidados conhecidos

- PostgreSQL 18 monta o volume em `/var/lib/postgresql`, não em `/var/lib/postgresql/data`.
- Não atualizar PrimeNG para a major 22. A linha 21 foi fixada para manter a licença MIT (ADR 0005).
- O deploy real exige secrets e servidor externos e não pode ser validado apenas localmente.
- Ao atualizar linhas de uma entidade dependente (como serviços e peças da OS), adicione as novas entidades explicitamente aos `DbSets` do EF Core para evitar `DbUpdateConcurrencyException`.
