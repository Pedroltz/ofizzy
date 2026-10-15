# Infraestrutura local e instalação manual

A raiz mantém `compose.yaml` como entrada padrão. Configurações adicionais ficam em `infra/compose/` e arquivos do Nginx em `infra/nginx/`. Não há workflow de deploy, publicação automática de imagens ou acesso SSH pelo GitHub Actions.

Execute os comandos abaixo **na raiz do repositório**. O `.env` pertence à raiz; nunca o versione.

| Cenário | Comando |
| --- | --- |
| Aplicação completa pelo Nginx | `docker compose up -d --wait` após build e migration |
| PostgreSQL para API/Angular no host | `docker compose -f infra/compose/local.yaml up -d` |
| Configuração com hot reload em containers | `docker compose -f compose.yaml -f infra/compose/development.yaml config --quiet` |
| Instalação manual com imagens e HTTPS | `docker compose -f compose.yaml -f infra/compose/production.yaml config --quiet` |

Os dois últimos arquivos são overrides: sempre informe `compose.yaml` primeiro para que os caminhos de build e volumes sejam resolvidos na raiz. A variante local usa configuração fixa de desenvolvimento e não exige `.env`. O override de desenvolvimento é mantido como configuração auxiliar; o fluxo recomendado com hot reload é API/Angular no host, descrito no [guia de início](../docs/GETTING-STARTED.md).

O PostgreSQL local mantém o volume nomeado `ofizzy_postgres_data`, preservando compatibilidade com os dados existentes. A porta continua restrita a localhost. Não exclua volumes para reorganizar arquivos.

O override de produção exige imagens construídas/publicadas manualmente, domínio e certificados válidos. Ele não é executado pela release. Veja [instalação](../docs/INSTALLATION.md) e [backup](../docs/BACKUP-RESTORE.md).
