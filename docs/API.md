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
