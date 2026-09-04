# Arquitetura

O Ofizzy é um monólito modular: uma API, um frontend e um PostgreSQL. A API usa módulos por negócio dentro de um único assembly e um `ApplicationDbContext`. Cada módulo possui seus DTOs, validações, regras e endpoints; não há repositories genéricos, CQRS ou barramento interno.

O Nginx oferece uma origem única: `/api` segue para ASP.NET Core e as demais rotas para Angular. O frontend é standalone, lazy-loaded e usa services/signals sem store global externo.

No frontend, páginas em `features/` funcionam como contêineres de estado e chamadas HTTP. Blocos coesos ficam em componentes da própria feature; padrões realmente compartilhados ficam em `shared/`. A responsividade usa CSS nos breakpoints 640/900 px e um serviço baseado em `matchMedia` somente quando o comportamento Angular precisa mudar, como a escolha obrigatória de cards no celular.

O editor de OS mantém uma única fonte de dados na página contêiner e delega a apresentação das linhas ao componente reutilizável de serviços/peças. Cálculos exibidos no navegador são estimativas; o backend continua autoritativo.

Assets estáticos com hash recebem cache imutável e gzip no Nginx. O dashboard usa um endpoint agregado para evitar múltiplas consultas concorrentes no carregamento inicial.

Dependências entre módulos devem ser explícitas e apontar para contratos, nunca para controllers. Regras e cálculos críticos pertencem ao backend.
