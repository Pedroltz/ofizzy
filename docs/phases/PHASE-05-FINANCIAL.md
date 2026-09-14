# Fase 5 — Financeiro

Recebimentos, estornos, status derivado e painel básico. Aceite: impedir valor acima do saldo, manter histórico e calcular a receber/recebido hoje/recebido no mês.

Status em 13/09/2026: **não iniciada; SaaS tem aceite local e a fase 8 fiscal é a prioridade anterior a esta fase**.

A antecipação de responsividade e E2E da Fase 6 não altera este escopo. A implementação financeira deverá reutilizar os padrões móveis atuais, preservar os snapshots da OS e manter cálculos financeiros autoritativos no backend.

Atualização técnica em 2026-09-08: a identidade interna foi padronizada como Ofizzy antes do início funcional desta fase. Nenhum contrato ou requisito financeiro foi antecipado ou alterado.

Evidências da fundação técnica: backend 14 testes unitários e 4 de integração; frontend 26 testes unitários e 27 E2E; build das imagens e smoke pelo Nginx aprovados. A fase continua não iniciada funcionalmente.

Atualização de ambiente em 2026-09-09: o desenvolvimento local foi simplificado para PostgreSQL isolado via `compose.local.yaml` e API via `dotnet run`, com migrations automáticas e defaults restritos ao perfil `Development`. O escopo financeiro permanece não iniciado.

Correção complementar: históricos EF existentes em `public` e `ofizzy` são reconciliados idempotentemente, permitindo reiniciar `dotnet run` sem tentativa de recriar tabelas. Validado com duas inicializações consecutivas, 14 testes unitários e 4 de integração.

Correção de setup: CNPJ opcional passou a aceitar entrada com máscara, mantendo 14 dígitos como regra e persistência normalizada. O escopo financeiro não foi alterado.
Atualização visual — 2026-09-09: removido o brand-mark do login. Seletor de tema de login/setup abre abaixo e alinhado à direita; sidebar mantém abertura acima. Controles com mínimo de 44 px. Lint e build frontend aprovados (aviso de bundle conhecido). Nenhuma alteração de API ou migration nesta tarefa.

Replanejamento em 2026-09-10: futuros pagamentos devem ser tenant-scoped, com FKs compostas para OS e testes de isolamento. Financeiro operacional não é cobrança de assinatura SaaS.

## Continuidade consolidada — 13/09/2026

A informação de pagamento declarada na NF-e não implementa recebimento financeiro. Próximo incremento desta fase exige saldo e estornos no backend, histórico e testes de isolamento por tenant.

O estado geral está em [STATUS](../STATUS.md); a ordem, dependências e critérios futuros estão em [NEXT-STEPS](../NEXT-STEPS.md). Resultados anteriores neste documento preservam a data e o escopo originais.
