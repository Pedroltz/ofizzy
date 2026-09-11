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

Somente o PostgreSQL roda no Docker. Backend (.NET SDK 10) e frontend (Node.js 24
LTS/npm) executam na máquina, em terminais separados. A partir da raiz:

```bash
# Banco persistente em localhost:5432
docker compose -f compose.local.yaml up -d
```

```bash
# API em http://localhost:5154, com migrations automáticas
dotnet run --project src/backend/Ofizzy.Api --launch-profile local
```

```bash
# Angular em http://localhost:4200
cd src/frontend/ofizzy-web
npm ci  # instalação/atualização das dependências
npm start
```

Acesse **http://localhost:4200/setup** no banco vazio para criar o operador da
plataforma. Depois, crie a empresa em `/plataforma` e conclua seu onboarding.
O perfil `local` habilita bootstrap em desenvolvimento e conecta ao PostgreSQL
Docker (`127.0.0.1:5432`, banco/usuário `ofizzy`). O proxy Angular encaminha `/api`
e `/health` à API na porta 5154. Nginx não é necessário neste fluxo local.

Encerre API/Angular com Ctrl+C. Para parar o banco:
`docker compose -f compose.local.yaml stop`. Os dados persistem no volume;
não use `down --volumes` se desejar preservá-los. A autenticação `trust` deste
Compose é apenas para desenvolvimento em máquina confiável, com porta restrita
a localhost; não utilize esta configuração em produção.

A API gera uma chave JWT efêmera, portanto reiniciar exige novo login. User
Secrets existentes têm precedência sobre a conexão do perfil local. Para usar
a conexão acima, remova uma configuração antiga, se houver:

```bash
dotnet user-secrets remove "ConnectionStrings:Postgres" --project src/backend/Ofizzy.Api
```

Para uma chave estável, configure `Jwt:SigningKey` por User Secrets sem versionar
a chave. Os perfis anteriores `http`/`https` continuam disponíveis. O deploy
central continua usando o Compose completo; não é necessário iniciá-lo para desenvolver.

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
