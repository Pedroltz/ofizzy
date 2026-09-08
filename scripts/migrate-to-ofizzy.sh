#!/usr/bin/env sh
set -eu

if [ "$#" -ne 4 ]; then
  echo "Uso: $0 <projeto-compose-origem> <banco-origem> <usuario-origem> <arquivo-backup>" >&2
  exit 2
fi

source_project="$1"
source_database="$2"
source_user="$3"
backup_file="$4"
target_volume="ofizzy_postgres_data"

if [ -z "$source_project" ] || [ -z "$source_database" ] || [ -z "$source_user" ]; then
  echo "Os identificadores de origem são obrigatórios." >&2
  exit 2
fi

if docker volume inspect "$target_volume" >/dev/null 2>&1; then
  echo "O volume $target_volume já existe. Migração cancelada para não sobrescrever dados." >&2
  exit 1
fi

source_postgres_id="$(docker compose -p "$source_project" ps -q postgres)"
if [ -z "$source_postgres_id" ]; then
  echo "O PostgreSQL de origem não está em execução." >&2
  exit 1
fi

backup_dir="$(dirname "$backup_file")"
mkdir -p "$backup_dir"
backup_dir="$(cd "$backup_dir" && pwd)"
backup_file="$backup_dir/$(basename "$backup_file")"

echo "Interrompendo serviços que podem gravar na base de origem..."
docker compose -p "$source_project" stop backend frontend nginx

echo "Gerando backup consistente..."
docker exec "$source_postgres_id" pg_dump \
  --username "$source_user" \
  --dbname "$source_database" \
  --format custom > "$backup_file"

docker run --rm \
  --volume "$backup_dir:/backup:ro" \
  postgres:18-alpine \
  pg_restore --list "/backup/$(basename "$backup_file")" >/dev/null

echo "Criando o PostgreSQL Ofizzy em volume separado..."
docker compose up --detach --wait postgres

echo "Restaurando os dados sem alterar o volume de origem..."
docker compose exec -T postgres sh -c \
  'pg_restore --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" --no-owner --no-acl' \
  < "$backup_file"

echo "Aplicando migrations e iniciando a nova stack..."
docker compose --profile tools run --rm migrate
docker compose up --detach --wait

echo "Migração concluída. Preserve o backup e o volume de origem até o fim da janela de rollback."
