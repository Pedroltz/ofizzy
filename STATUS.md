# Status do projeto

Atualizado em: 2026-09-03

## Estado

Fases 1, 2, 3 e 4 concluídas e validadas.
- Módulos de Clientes e Veículos totalmente integrados e corrigidos.
- Módulo de Ordens de Serviço completo: abertura, listagem, visualização detalhada, edição (em aberto/em andamento), transições de estado com confirmação e persistência total no PostgreSQL.
- Módulo de Configurações da Oficina (`/configuracoes` -> Dados da Oficina): persistência no PostgreSQL de Razão Social, Nome Fantasia, CNPJ, Telefones, WhatsApp, Endereço e Termo de Garantia da OS.
- Impressão A4 minimalista e profissional: modelo limpo baseado em texto e linhas divisórias (`.wo-print-sheet`), sem fundos que gastem tinta, com cabeçalho, dados do cliente/veículo, apontamentos, tabelas de serviços/peças, totais e termo com campos de assinatura.
- Geração de PDF no backend com QuestPDF: endpoint `GET /api/work-orders/{id}/pdf` emitindo arquivo `OS-0001.pdf` diagramado em A4 com suporte a múltiplas páginas.
- Stack Docker Compose (PostgreSQL, Backend, Frontend e Nginx) 100% saudável e testada com persistência após reinício na porta 8080.

## Próximo marco

Fase 5 — Financeiro e Pagamentos (registro de pagamentos, formas de pagamento, parcelas, caixa e fechamento financeiro).

## Atenções

- PrimeNG 22 não será utilizado por causa do seu modelo de licença. A decisão vigente está em `docs/adr/0005-primeng-21-mit.md`.
- O build do frontend passa, com aviso não bloqueante de bundle inicial: ~694 kB para um orçamento de 500 kB. Revisar esse orçamento ou otimizar dependências antes do primeiro deploy público.
- O deploy remoto depende da configuração dos secrets e do servidor descrita em `docs/DEPLOYMENT.md`.
- O histórico de migrations no volume local foi reconciliado e atualizado (`InitialIdentity`, `AddCatalogs`, `AddWorkOrders`, `AddWorkshopSettings` aplicadas com sucesso).

