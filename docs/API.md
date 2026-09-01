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
