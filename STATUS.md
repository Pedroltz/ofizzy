# Status do projeto

Atualizado em: 2026-09-02

## Estado

Fases 1, 2 e 3 concluídas e validadas.
- Módulos de Clientes e Veículos totalmente integrados e corrigidos.
- Módulo de Ordens de Serviço completo: abertura, listagem, visualização detalhada, edição (em aberto/em andamento), transições de estado com confirmação e persistência total no PostgreSQL.
- Stack Docker Compose (PostgreSQL, Backend, Frontend e Nginx) validada com smoke tests e persistência após reinício na porta 8080.

## Próximo marco

Fase 4 — Impressão e Envio da OS (ordem de serviço impressa/PDF amigável e envio de resumo via WhatsApp).

## Atenções

- PrimeNG 22 não será utilizado por causa do seu modelo de licença. A decisão vigente está em `docs/adr/0005-primeng-21-mit.md`.
- O build do frontend passa, com aviso não bloqueante de bundle inicial: 659,50 kB para um orçamento de 500 kB. Revisar esse orçamento ou otimizar dependências antes do primeiro deploy público.
- O deploy remoto depende da configuração dos secrets e do servidor descrita em `docs/DEPLOYMENT.md`.
- O histórico de migrations no volume local foi reconciliado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders` aplicadas com sucesso).

