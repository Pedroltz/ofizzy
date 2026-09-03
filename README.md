# Sport Pneus

Sistema simples e rápido para gestão de uma pequena oficina, centrado na Ordem de Serviço.

## Executar

1. Copie `.env.example` para `.env` e substitua as senhas.
2. Construa e execute a migration: `docker compose --profile tools run --rm --build migrate`.
3. Inicie: `docker compose up --build -d`.
4. Abra `http://localhost:8080` e conclua o primeiro acesso.

## Desenvolvimento local

Para executar somente o PostgreSQL com Docker e manter backend/frontend no host:

```bash
docker compose -f compose.yaml -f compose.local.yaml up -d postgres
```

Defina `ConnectionStrings__Postgres` apontando para `localhost:5432` antes de iniciar a API. O frontend usa `proxy.conf.json` no modo desenvolvimento para encaminhar `/api` para a API local em `http://localhost:5154`.

Para executar a API diretamente com `dotnet run`, configure uma vez os valores locais via .NET User Secrets. O projeto já possui `UserSecretsId`; os segredos são carregados automaticamente no perfil `Development` e nunca devem ser versionados:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<chave-aleatória-com-ao-menos-32-bytes>" --project src/backend/SportPneus.Api
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=sport_pneus;Username=sport_pneus;Password=<senha-local>" --project src/backend/SportPneus.Api
```

Para desenvolvimento: `docker compose -f compose.yaml -f compose.dev.yaml up --build`.

Consulte [STATUS.md](STATUS.md) para o estado atual e [ROADMAP.md](ROADMAP.md) para as próximas fases.
