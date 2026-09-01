# Modelo de dados

## Fundação

- `Company`: configuração única da oficina.
- `User`: administrador, email normalizado único e hash de senha.
- `RefreshToken`: somente hash SHA-256, família, expiração, revogação e substituição.

## Convenções futuras

UUID v7 para chaves; `timestamptz` em UTC; `numeric(14,2)` para dinheiro; número da OS por sequence. Cadastros serão arquivados e documentos históricos não serão apagados. Itens de OS guardam snapshots de descrição e preço.
