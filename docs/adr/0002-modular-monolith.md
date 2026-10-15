# ADR 0002 — Monólito modular

Status: aceito — 2026-08-31

Uma API e um banco com módulos por domínio. Esta escolha reduz operação e abstrações, preservando limites de código suficientes para evolução. Microserviços, repositories genéricos, MediatR e event bus ficam fora do MVP.

## Contexto de continuidade — 13/09/2026

Esta ADR preserva a decisão e o contexto de sua data. A implementação atual mantém monólito modular, SaaS por tenant e PrimeNG 21 conforme as decisões posteriores aplicáveis. A integração fiscal direta está registrada na [ADR 0007](0007-direct-fiscal-integration.md). Estado e próximas entregas: [STATUS](../STATUS.md) e [NEXT-STEPS](../NEXT-STEPS.md).
