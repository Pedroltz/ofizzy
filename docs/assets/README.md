# Imagens do repositório

Os logos são cópias dos assets existentes da aplicação. `logo.png` atende ao tema claro e `logo-dark.png` ao escuro, selecionados com `<picture>` no README. Preserve a identidade ao atualizá-los.

As cinco imagens em `screenshots/` vêm da aplicação real pelo Nginx, com API e PostgreSQL. Nomes marcados como Demo, e-mails `example.test`, placas e registros são fictícios. Não há interceptação de API, XML fiscal real, cookies ou credenciais nas imagens.

## Reproduzir as capturas

Requisitos: Docker, Node 24 e dependências do frontend instaladas (`npm ci` e `npx playwright install chromium` no diretório do frontend).

Na raiz, em um PowerShell exclusivo da demonstração:

```powershell
$env:OFIZZY_DEMO_PASSWORD = 'Demo1!' + [guid]::NewGuid().ToString('N')
$demoDatabasePassword = [guid]::NewGuid().ToString('N')
$demoSigningKey = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
@(
  'POSTGRES_DB=ofizzy_github_demo'
  'POSTGRES_USER=ofizzy_demo'
  "POSTGRES_PASSWORD=$demoDatabasePassword"
  "JWT_SIGNING_KEY=$demoSigningKey"
  'APP_PORT=127.0.0.1:18091'
  'ASPNETCORE_ENVIRONMENT=Development'
  'PLATFORM_BOOTSTRAP_ENABLED=true'
  'BACKEND_IMAGE=ofizzy-github-backend:local'
  'FRONTEND_IMAGE=ofizzy-github-frontend:local'
) | Set-Content -Encoding ascii .env.github-demo
docker compose --env-file .env.github-demo -p ofizzy-github-demo build
docker compose --env-file .env.github-demo -p ofizzy-github-demo --profile tools run --rm migrate
docker compose --env-file .env.github-demo -p ofizzy-github-demo up -d --wait
node scripts/capture-github-demo.cjs
```

Para executar o mesmo aceite sem sobrescrever os PNGs versionados, substitua o último comando por `node scripts/capture-github-demo.cjs --smoke`. Use `--verify-restart` depois do reinício para conferir a persistência.

O script exige banco vazio, cria uma empresa fictícia com oito clientes, oito veículos, oito OS e um recebimento com liquidação parcial. Valida respostas da API, PDF, saldo e layout antes de salvar as imagens. A origem é fixa em `127.0.0.1:18091`; a stack não publica PostgreSQL e não usa dados locais existentes.

Para recapturar a mesma fixture, na mesma sessão, use `node scripts/capture-github-demo.cjs --capture-existing`. O script reutiliza somente os IDs salvos em `test-results/github-demo/state.json` e autentica novamente; não salva sessões ou senhas.

## Persistência e revisão

```powershell
docker compose --env-file .env.github-demo -p ofizzy-github-demo restart
docker compose --env-file .env.github-demo -p ofizzy-github-demo up -d --wait
node scripts/capture-github-demo.cjs --verify-restart
```

O modo de verificação confirma os mesmos IDs, oito cadastros de cada tipo, estados das OS, PDF e saldo de R$ 100 após reinício; não sobrescreve imagens. Os relatórios locais ficam em `test-results/github-demo/`, ignorados pelo Git.

Revise visualmente todas as imagens antes de publicá-las. O script exige ausência de overflow na listagem em 1440, 768, 320 e 390 px e registra quantos controles visíveis têm altura abaixo de 44 px. A interface atual possui controles compactos preexistentes; essa medição não declara conformidade integral de alvos de toque. Esta entrega documental não altera o CSS do produto. As capturas desktop usam 1920 × 1080; o editor usa altura de viewport 1280 para mostrar seus itens. Editor e recebimentos são recortes de componentes reais. O celular usa 390 × 844.

Para encerrar preservando a fixture, execute `docker compose --env-file .env.github-demo -p ofizzy-github-demo down`. Para descartar **somente essa demonstração**, use o mesmo comando com `--volumes`. Nunca remova volumes da stack operacional. `.env.github-demo` é ignorado tanto pelo Git quanto pelo contexto Docker.

PNG preserva a nitidez do texto; mantenha cada imagem abaixo de 1 MB e não inclua relatórios de teste ou traces na galeria.
