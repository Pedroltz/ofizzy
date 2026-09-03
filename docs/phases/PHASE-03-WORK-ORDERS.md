# Fase 3 — Ordens de Serviço

Criação rápida, número sequencial, diagnóstico, snapshots, serviços, peças, totais e transições de estado. Aceite: abrir OS em poucos segundos, salvar/finalizar e validar todos os cálculos no backend.

## Estado em 2026-09-02

Concluída e validada. Abertura, listagem, visualização detalhada, edição de ordens abertas/em andamento, transições de estado, cálculo e consistência no backend validadas com sucesso via Nginx na porta 8080 com persistência comprovada após reinício.

## Entregue

- [x] Número sequencial por sequence do PostgreSQL (`work_order_number_seq`).
- [x] Vínculo com cliente e veículo ativos, com snapshots históricos de cliente e veículo.
- [x] Diagnóstico, queixa, observações e quilometragem.
- [x] Serviços e peças com quantidade, preço unitário e snapshots de catálogo.
- [x] Subtotais e total calculados exclusivamente pelo backend.
- [x] Estados `Open`, `InProgress`, `Completed` e `Cancelled`, com transições validadas.
- [x] Imutabilidade de OS finalizada ou cancelada (retornando HTTP 409 Conflito).
- [x] Busca, listagem, abertura, edição e visualização detalhada de OS pela interface Angular.
- [x] Diálogos de confirmação para ações destrutivas ou de fechamento.
- [x] Migration `AddWorkOrders` aplicada e reconciliada no PostgreSQL local e validada em Testcontainers.
- [x] Validação ponta a ponta via origem única do Nginx com persistência após `docker compose restart`.

## Pendente para o aceite

Nenhuma pendência técnica. Todos os itens de aceite foram atendidos e comprovados com testes automatizados e smoke test real.

## Evidências atuais

- Backend: build sem avisos; 10 testes unitários e 3 testes de integração aprovados em PostgreSQL real (Testcontainers).
- Frontend: lint com 0 erros/avisos, 1 teste Vitest aprovado e build de produção aprovado (`dist/sport-pneus-web`).
- Compose & Nginx: stack completa em execução saudável na porta 8080 (`postgres`, `backend`, `frontend`, `nginx`).
- Persistência e Smoke: criação, edição, avanço de status para `InProgress` e `Completed` validados com sucesso via Nginx em `http://localhost:8080` e dados persistidos no PostgreSQL após reinício do Docker Compose.

