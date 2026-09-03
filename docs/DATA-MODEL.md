# Modelo de dados

## Fundação

- `Company`: configuração única da oficina.
- `User`: administrador, email normalizado único e hash de senha.
- `RefreshToken`: somente hash SHA-256, família, expiração, revogação e substituição.

## Cadastros

- `Customer`: dados de contato, documento opcional único e arquivamento lógico.
- `Vehicle`: pertence a um cliente, possui placa única e arquivamento lógico.
- `ServiceItem`: serviço de catálogo com preço padrão.
- `Part`: peça de catálogo com código único, custo e preço de venda.

## Ordens de serviço

- `WorkOrder`: número sequencial, cliente/veículo de origem, snapshots históricos, diagnóstico, quilometragem e estado.
- `WorkOrderService`: snapshot da descrição, quantidade e preço unitário; a referência ao catálogo é opcional e usa `SET NULL`.
- `WorkOrderPart`: snapshot da descrição, código, quantidade e preço unitário; a referência ao catálogo é opcional e usa `SET NULL`.
- Estados persistidos: `Open`, `InProgress`, `Completed` e `Cancelled`.
- Subtotais e total não são aceitos do cliente: são calculados a partir de quantidade × preço unitário no backend.

## Convenções

UUID v7 para chaves; `timestamptz` em UTC; `numeric(14,2)` para dinheiro; quantidades com três casas decimais; número da OS por sequence. Cadastros são arquivados e documentos históricos não são apagados.
