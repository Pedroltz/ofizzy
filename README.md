# Sport Pneus

Sistema simples e rápido para gestão de uma pequena oficina, centrado na Ordem de Serviço.

## Executar

1. Copie `.env.example` para `.env` e substitua as senhas.
2. Construa e execute a migration: `docker compose --profile tools run --rm --build migrate`.
3. Inicie: `docker compose up --build -d`.
4. Abra `http://localhost:8080` e conclua o primeiro acesso.

Para desenvolvimento: `docker compose -f compose.yaml -f compose.dev.yaml up --build`.

Consulte [STATUS.md](STATUS.md) para o estado atual e [ROADMAP.md](ROADMAP.md) para as próximas fases.
