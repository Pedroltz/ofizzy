# Deploy

O workflow `deploy.yml` publica imagens com a tag do SHA no GHCR e atualiza um servidor Linux por SSH.

Secrets exigidos: `SSH_HOST`, `SSH_PORT`, `SSH_USER`, `SSH_PRIVATE_KEY`, `DEPLOY_PATH`, `GHCR_USERNAME` e `GHCR_TOKEN`. O servidor mantém seu próprio `.env`, certificados em `/etc/letsencrypt` e os arquivos `compose.yaml`, `compose.prod.yaml` e `deploy/`.

O deploy autentica no GHCR, baixa imagens, executa o serviço `migrate`, atualiza containers e verifica `/health/ready`. A primeira publicação exige preparação manual do host e DNS.

Na primeira publicação da identidade técnica Ofizzy, não execute o deploy automático sobre a instalação anterior. Abra uma janela de manutenção, atualize o `.env` do servidor para o banco e usuário `ofizzy` e siga `docs/BACKUP-RESTORE.md`. Depois do smoke e da confirmação de persistência, os deploys voltam ao fluxo normal do workflow.

O Nginx deve permanecer como origem única. Os arquivos em `deploy/nginx/` habilitam gzip, keep-alive para o proxy da API e cache imutável de um ano somente para assets versionados por hash; HTML não deve receber cache imutável.

Após atualizar, valide login, dashboard, criação/edição de OS, download de PDF e ausência de overflow em um viewport móvel. Migrations devem ser executadas antes da troca definitiva dos containers.
