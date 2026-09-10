# Ofizzy

Plataforma SaaS multi-tenant para empresas prestadoras de serviços. Automotive é a primeira vertical, centrada na Ordem de Serviço.

O MVP atual inclui autenticação, clientes, veículos, catálogos, ciclo completo de OS, configuração da oficina, impressão HTML/PDF, dashboard operacional e interface responsiva para desktop, celular e tablet. A fundação SaaS adiciona organizações, usuários com múltiplos vínculos, módulos, provisionamento administrativo e onboarding. Financeiro operacional continua no roadmap.

## Documentação

- [Visão e limites do produto](docs/PROJECT.md)
- [Status atual](docs/STATUS.md)
- [Roadmap](docs/ROADMAP.md)
- [Arquitetura](docs/ARCHITECTURE.md) e [modelo de dados](docs/DATA-MODEL.md)
- [Frontend](docs/FRONTEND.md), [contratos da API](docs/API.md), [testes](docs/TESTING.md) e [deploy](docs/DEPLOYMENT.md)
- [Guia de contribuição](docs/CONTRIBUTING.md), [segurança](docs/SECURITY.md) e [changelog](docs/CHANGELOG.md)
- [Decisões arquiteturais](docs/adr/) e [fases de implementação](docs/phases/)
- Orientações para agentes de IA: [`AGENTS.md`](AGENTS.md) e [`AI-HANDOFF.md`](AI-HANDOFF.md)

## Executar

Execute os comandos abaixo a partir da raiz do repositório.

1. Copie `.env.example` para `.env` e substitua as senhas.
2. Construa as imagens (`docker compose build`) e execute a migration: `docker compose --profile tools run --rm migrate`.
3. Inicie: `docker compose up --build -d`.
4. Para banco vazio, inicialize o operador da plataforma em acesso privado com `PLATFORM_BOOTSTRAP_ENABLED=true`; desabilite após `/setup`.
5. Em `/plataforma`, crie tenants. Cada Owner conclui seu próprio onboarding. Consulte [deploy e migração legada](docs/DEPLOYMENT.md) antes de atualizar dados existentes.

## Desenvolvimento local

Para executar somente o PostgreSQL com Docker e manter backend/frontend no host:

```bash
docker compose -f compose.yaml -f compose.local.yaml up -d postgres
```

Defina `ConnectionStrings__Postgres` apontando para `localhost:5432` antes de iniciar a API. O frontend usa `proxy.conf.json` no modo desenvolvimento para encaminhar `/api` para a API local em `http://localhost:5154`.

Para executar a API diretamente com `dotnet run`, configure uma vez os valores locais via .NET User Secrets. O projeto já possui `UserSecretsId`; os segredos são carregados automaticamente no perfil `Development` e nunca devem ser versionados:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<chave-aleatória-com-ao-menos-32-bytes>" --project src/backend/Ofizzy.Api
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=ofizzy;Username=ofizzy;Password=<senha-local>" --project src/backend/Ofizzy.Api
```

Para desenvolvimento: `docker compose -f compose.yaml -f compose.dev.yaml up --build`.

## Validação

Backend:

```bash
dotnet test Ofizzy.slnx
```

Frontend:

```bash
cd src/frontend/ofizzy-web
npm run lint
npm test
npx playwright install chromium
npm run e2e
npm run build
```

O E2E inicia o Angular em `http://127.0.0.1:4300`, intercepta as APIs com dados determinísticos e executa projetos desktop, Pixel 7 e tablet.

Consulte [docs/STATUS.md](docs/STATUS.md) para o estado atual, [docs/ROADMAP.md](docs/ROADMAP.md) para as próximas fases e [AI-HANDOFF.md](AI-HANDOFF.md) para continuidade técnica.

## SaaS: nova empresa não exige deploy

Uma API, um PostgreSQL e Nginx atendem todos os tenants. PlatformAdmin provisiona
organizações; Owners administram somente suas empresas. Tenant é a empresa cliente
do Ofizzy; Customer é seu cliente operacional. A migração preserva dados legados,
IDs e snapshots. Detalhes na [ADR 0006](docs/adr/0006-saas-multi-tenancy.md).

O aceite com API real está em `playwright.live.config.ts`; execute apenas em banco
descartável vazio com bootstrap explicitamente habilitado:

```bash
npm run e2e -- --config playwright.live.config.ts
# Após reiniciar os containers da stack de teste:
OFIZZY_VERIFY_RESTART=1 npm run e2e -- --config playwright.live.config.ts
```

A origem padrão desse teste é `http://127.0.0.1:18081`, alterável por
`OFIZZY_E2E_BASE_URL`. Usa somente credenciais fictícias e não deve apontar para
produção. Em Node 26, execute unitários com `NODE_OPTIONS=--no-experimental-webstorage
npm test -- --watch=false`; CI e imagem frontend usam Node 24.
