# ADR 0001 — Baseline tecnológico

Status: parcialmente substituída pela ADR 0005 — 2026-09-01

Usar .NET 10 LTS porque .NET 9 está próximo do fim de suporte. Usar Angular 22.1 e PrimeNG 22.1. O PrimeNG 22 migrou os presets para `@primeuix/themes`, pois `@primeng/themes` ainda possui major 21; o lockfile registra a combinação validada.

A decisão sobre .NET 10 permanece vigente. A decisão frontend acima é apenas histórica: a stack atual é Angular 21 + PrimeNG 21 (MIT), conforme ADR 0005.
