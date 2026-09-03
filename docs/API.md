# API

Prefixo `/api`, JSON camelCase e erros `application/problem+json`.

## Fase 1

- `GET /api/setup/status`
- `POST /api/setup`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /health/live`
- `GET /health/ready`

Requisições mutáveis exigem `X-XSRF-TOKEN`, obtido pelo cookie legível `XSRF-TOKEN`. Tokens de sessão nunca são retornados no corpo.

## Fase 2

- CRUD e arquivamento em `/api/customers`, `/api/vehicles`, `/api/services` e `/api/parts`.

## Fase 3

- `GET /api/work-orders?q=&status=&page=&pageSize=`
- `GET /api/work-orders/{id}`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}` para OS aberta ou em andamento
- `PATCH /api/work-orders/{id}/status` com `Open`, `InProgress`, `Completed` ou `Cancelled`

O número é sequencial no PostgreSQL. Dados do cliente, veículo e itens são preservados como snapshots; subtotais e total são calculados pelo backend. OS finalizada ou cancelada é imutável.

## Fase 4

- `GET /api/company`
- `PUT /api/company`
- `GET /api/work-orders/{id}/pdf`

Os dados da oficina alimentam a impressão e o PDF. O PDF é devolvido como `application/pdf` com nome `OS-NNNN.pdf`.

## Dashboard

- `GET /api/dashboard/summary`

Retorna totais de clientes, veículos, ordens ativas/finalizadas e a lista resumida de ordens ativas em uma única consulta agregada.

## Convenções de listagem

Listagens usam `q`, `page` e `pageSize`; veículos também aceitam `customerId` e ordens aceitam `status`. Respostas paginadas usam `items`, `total`, `page` e `pageSize`.
