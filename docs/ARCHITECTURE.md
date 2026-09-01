# Arquitetura

O Sport Pneus é um monólito modular: uma API, um frontend e um PostgreSQL. A API usa módulos por negócio dentro de um único assembly e um `ApplicationDbContext`. Cada módulo possui seus DTOs, validações, regras e endpoints; não há repositories genéricos, CQRS ou barramento interno.

O Nginx oferece uma origem única: `/api` segue para ASP.NET Core e as demais rotas para Angular. O frontend é standalone, lazy-loaded e usa services/signals sem store global externo.

Dependências entre módulos devem ser explícitas e apontar para contratos, nunca para controllers. Regras e cálculos críticos pertencem ao backend.
