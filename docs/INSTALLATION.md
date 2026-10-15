# Instalação e atualização manual

O Ofizzy é instalado manualmente com Docker Compose. Não há workflow de deploy ou publicação automática de imagens. Pushes em `main`, tags e releases não acessam servidores nem aplicam migrations remotamente.

O ambiente de destino mantém seu próprio `.env`, certificados em `/etc/letsencrypt`, `compose.yaml` e a pasta `infra/`. Credenciais SSH e GHCR não são requisitos do CI. Caso utilize um registry privado manualmente, autentique o Docker fora do repositório.

Na raiz do checkout, configure `.env`, construa as imagens, execute a migration e inicie os serviços:

```bash
docker compose build
docker compose --profile tools run --rm migrate
docker compose up -d --wait
```

Para usar imagens prontas e HTTPS, configure `BACKEND_IMAGE`, `FRONTEND_IMAGE`, `APP_DOMAIN` e certificados válidos, então utilize `docker compose -f compose.yaml -f infra/compose/production.yaml` nos comandos de instalação. Informe sempre o Compose principal primeiro. A primeira exposição externa exige preparação manual do host, DNS e TLS.

Na primeira migração de uma instalação anterior, abra uma janela de manutenção, confira banco/usuário/volumes e siga [backup e restauração](BACKUP-RESTORE.md). Confirme smoke e persistência antes de reabrir o acesso. Instalações existentes não devem ter volumes removidos ao reorganizar arquivos.

O Nginx deve permanecer como origem única. Os arquivos em `infra/nginx/` habilitam gzip, keep-alive para o proxy da API e cache imutável de um ano somente para assets versionados por hash; HTML não deve receber cache imutável.

Após atualizar, valide login, dashboard, criação/edição de OS, download de PDF e ausência de overflow em um viewport móvel. Migrations devem ser executadas antes da troca definitiva dos containers.

## Instalação única e provisionamento SaaS

Código → CI (backend/frontend/E2E) → build ou imagens manuais → servidor → migration →
containers/healthchecks → todos os tenants utilizam a mesma versão.

Venda → PlatformAdmin → POST /api/platform/tenants → credencial privada → login →
onboarding → tenant operacional. Este segundo fluxo não faz deploy, não cria
containers e não exige outro PostgreSQL. Origem compartilhada, por exemplo
app.ofizzy.com.br; subdomínios/domínios customizados não são necessários agora.

### Primeira instalação vazia

1. Execute migration e inicie a plataforma; healthchecks não dependem de onboarding.
2. Mantenha acesso privado (firewall/túnel); habilite temporariamente
   `PLATFORM_BOOTSTRAP_ENABLED=true` no ambiente do backend e recrie o serviço.
3. Abra `/setup` e crie o administrador global. Nenhum tenant é criado nesse passo.
4. Desabilite a variável e recrie o backend antes de expor acesso público.
5. Em `/plataforma`, provisione a empresa e disponibilize a credencial inicial
   privadamente. O Owner configura dados em `/onboarding` e confirma a conclusão.

### Atualização single-tenant → SaaS

Faça backup verificado antes da migration e interrompa writers da versão antiga.
A migration altera constraints/índices e não deve rodar concorrente à API antiga.
Mais de uma Company legada aborta com segurança: mapear dados explicitamente.
Não existe downgrade destrutivo automático; rollback usa backup pré-migration e
imagem anterior. Não restaure backup single-tenant sobre tenants novos.

Dados, IDs, snapshots, hashes de senha e números da OS são preservados. Usuários
legados recebem vínculo Owner no tenant inicial, não privilégios de plataforma.
Selecione explicitamente um operador autorizado e execute após migration:

```bash
docker compose run --rm backend --grant-platform-admin <UUID-do-operador-existente>
```

O comando persiste IsPlatformAdmin e PlatformAdminGrantedAt e registra UserId no log.
Não é endpoint público. Login antigo sem claim tenant pode exigir nova seleção/login;
refresh legado é associado ao tenant inicial durante migration.

Para a primeira transição use janela de manutenção; deploys posteriores continuam
centralizados. Nginx, certificados, volumes e secrets permanecem únicos da plataforma.

## Publicação do módulo fiscal — 13/09/2026

As três migrations fiscais são AddFiscalFoundation, CompleteFiscalInutilization e AddFiscalInutilizationLease; a stack isolada validada totaliza dez migrations. Conferir histórico do ambiente de destino e backup antes da aplicação. A publicação da API precisa incluir BusinessCore/Fiscal/Schemas.

Configurar as chaves externas por secrets ou override privado do Compose, conforme [FISCAL.md](FISCAL.md); não assumir que variáveis do host são repassadas automaticamente. Manter ProductionEnabled=false até homologação e liberação do tenant. Smoke fiscal em 18082 usa HTTP/Development apenas em ambiente isolado; produção requer HTTPS.

Imagens, migrations e restart foram validados localmente; isso não registra deploy remoto nem homologação. Antes da liberação, cumprir [próximos passos](NEXT-STEPS.md) e [restauração fiscal](BACKUP-RESTORE.md).

## Incremento fiscal/financeiro — 27/09/2026

Aplicar `20260926213104_AddFiscalOrigin` e `20260926213917_AddManualPayments` pelo runner antes da versão nova. Fazer backup conforme runbook. Origem legada sem evidência positiva permanece `Unknown` e bloqueia operações pelo gateway; reconciliar evidência oficial antes de liberar qualquer registro. Não preencher `Official` indiscriminadamente. Não foi criada uma API de reclassificação de origem neste incremento.

Manter os três gates de produção restritos e simulador desligado na homologação oficial. O teste local usa porta 18089, dados/segredos fictícios e stack própria `ofizzy-plan-accept`; não altera a stack operacional. Configurações de simulação e chaves fictícias permanecem em `/tmp`, fora do Git.
