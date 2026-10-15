# ADR 0003 — Sessões do navegador

Status: aceito — 2026-08-31

Access JWT de 15 minutos e refresh token de 30 dias ficam em cookies HttpOnly/SameSite Strict. Mutations exigem antiforgery token. Refresh tokens são aleatórios, armazenados como hash e rotacionados; reutilização revoga a família.

## Contexto de continuidade — 13/09/2026

Esta ADR preserva a decisão e o contexto de sua data. A implementação atual mantém monólito modular, SaaS por tenant e PrimeNG 21 conforme as decisões posteriores aplicáveis. A integração fiscal direta está registrada na [ADR 0007](0007-direct-fiscal-integration.md). Estado e próximas entregas: [STATUS](../STATUS.md) e [NEXT-STEPS](../NEXT-STEPS.md).
