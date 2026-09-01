# Deploy

O workflow `deploy.yml` publica imagens com a tag do SHA no GHCR e atualiza um servidor Linux por SSH.

Secrets exigidos: `SSH_HOST`, `SSH_PORT`, `SSH_USER`, `SSH_PRIVATE_KEY`, `DEPLOY_PATH`, `GHCR_USERNAME` e `GHCR_TOKEN`. O servidor mantém seu próprio `.env`, certificados em `/etc/letsencrypt` e os arquivos `compose.yaml`, `compose.prod.yaml` e `deploy/`.

O deploy autentica no GHCR, baixa imagens, executa o serviço `migrate`, atualiza containers e verifica `/health/ready`. A primeira publicação exige preparação manual do host e DNS.
