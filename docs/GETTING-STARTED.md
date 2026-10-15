# Executar e desenvolver

Execute os comandos a partir da raiz, salvo indicação contrária. Para conhecer o produto, consulte o [README](../README.md).

## Aplicação completa pelo Nginx

Pré-requisito: Docker com Compose.

```bash
cp .env.example .env
# Substitua senhas e gere uma chave JWT aleatória de pelo menos 32 bytes.
docker compose build
docker compose --profile tools run --rm migrate
docker compose up -d --wait
```

No PowerShell, use `Copy-Item .env.example .env`. A origem padrão é `http://localhost:8080`.

Em banco vazio, habilite temporariamente `PLATFORM_BOOTSTRAP_ENABLED=true` em acesso privado e recrie o backend. Crie o operador em `/setup`, desabilite a variável e recrie o backend antes de expor a aplicação. Em `/plataforma`, provisione empresas; cada Owner conclui seu onboarding. Consulte [deploy e migração legada](INSTALLATION.md) antes de atualizar dados existentes.

## Desenvolvimento com hot reload

Requisitos: .NET SDK 10, Node.js 24 LTS/npm e Docker. Somente o PostgreSQL roda no Docker; API e Angular executam em terminais separados.

```bash
# Banco persistente em localhost:5432
docker compose -f infra/compose/local.yaml up -d
```

```bash
# API em http://localhost:5154, com migrations automáticas
dotnet run --project src/backend/Ofizzy.Api --launch-profile local
```

```bash
# Angular em http://localhost:4200
cd src/frontend/ofizzy-web
npm ci
npm start
```

Acesse `http://localhost:4200/setup` no banco vazio para criar o operador, provisione a empresa em `/plataforma` e conclua seu onboarding. O perfil `local` habilita bootstrap de desenvolvimento e conecta a `127.0.0.1:5432`, banco/usuário `ofizzy`. O proxy Angular encaminha `/api` e `/health` à API na porta 5154.

Encerre API/Angular com Ctrl+C. Pare o banco com `docker compose -f infra/compose/local.yaml stop`. Os dados persistem no volume; não use `down --volumes` para preservá-los. A autenticação `trust` do Compose local é exclusiva de máquina confiável, com porta restrita a localhost; não a utilize em produção.

A API gera uma chave JWT efêmera; reiniciar exige novo login. User Secrets existentes têm precedência sobre a conexão do perfil local. Se houver uma configuração antiga, remova-a somente após conferir que não é mais necessária:

```bash
dotnet user-secrets remove "ConnectionStrings:Postgres" --project src/backend/Ofizzy.Api
```

Para uma chave estável, configure `Jwt:SigningKey` por User Secrets sem versioná-la. Os perfis `http`/`https` continuam disponíveis; o deploy usa o Compose completo.

## Validação

```bash
dotnet build Ofizzy.slnx --configuration Release
dotnet test Ofizzy.slnx --configuration Release --no-build
```

Testes de integração usam PostgreSQL descartável e exigem Docker disponível.

Se User Secrets de desenvolvimento sobrescreverem a conexão do Testcontainers no Windows, execute os testes com um perfil vazio exclusivo do processo, sem remover suas credenciais:

```powershell
New-Item -ItemType Directory -Force test-results/empty-profile | Out-Null
dotnet test Ofizzy.slnx --configuration Release --no-build --environment "APPDATA=$PWD/test-results/empty-profile"
```

```bash
cd src/frontend/ofizzy-web
npm run lint
npm test -- --watch=false
npx playwright install chromium
npm run e2e
npm run build
```

O E2E determinístico inicia Angular em `http://127.0.0.1:4300`, intercepta as APIs com fixtures e executa desktop, Pixel 7 e tablet. Ele não comprova persistência no banco. Node 24 é a versão usada no CI e nas imagens.

## Aceite com API real

Execute somente em stack e banco descartáveis, vazios, com bootstrap explicitamente habilitado. No diretório do frontend:

```bash
npm run e2e -- --config playwright.live.config.ts
# Depois de reiniciar a stack de teste (sintaxe bash):
OFIZZY_VERIFY_RESTART=1 npm run e2e -- --config playwright.live.config.ts
```

No PowerShell, defina `$env:OFIZZY_VERIFY_RESTART='1'` antes da segunda execução e remova a variável ao terminar. A origem padrão é `http://127.0.0.1:18081`, alterável por `OFIZZY_E2E_BASE_URL`. As suítes usam credenciais fictícias e nunca devem apontar para produção.

Veja [Testes](TESTING.md), [Status](STATUS.md), [Próximos passos](NEXT-STEPS.md) e [continuidade técnica](../AI-HANDOFF.md). Para atualizar as capturas, siga o [guia de assets](assets/README.md).
