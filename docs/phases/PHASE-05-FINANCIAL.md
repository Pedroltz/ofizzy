# Fase 5 — Financeiro

Estado em **27/09/2026**: primeiro incremento manual implementado para preparar conciliação; a Fase 8 permanece prioridade e sem homologação externa.

## Implementado

- recebimentos na OS concluída, com valor bruto, forma, data, autor e idempotência;
- liquidações parciais, taxas e segregação tributária opcional (nulo significa desconhecida, não zero);
- estorno administrativo parcial com justificativa e vínculo à liquidação original;
- histórico preservado, sem edição/exclusão dos movimentos;
- vínculos opcionais com documentos oficiais autorizados em produção da mesma OS; simulados/homologação não entram na conciliação fiscal produtiva;
- backend valida saldos, referências, datas e permissões; isolamento/FKs e transação serializável no PostgreSQL;
- interface real na OS, com erro compreensível e preservação da requisição em repetição após falha.

Recebimento sem vínculo fiscal exige conciliação. Cancelamento de nota não estorna dinheiro. Estorno ou mudança fiscal sinaliza revisão do vínculo; redistribuição ainda não implementada. O saldo da OS usa bruto liquidado menos estornado; líquido só é mostrado quando segregação foi informada.

## Pendências

Painel a receber/hoje/mês, referências do banco/provedor, importação/conciliação, redistribuição de alocações e integração automática com Split Payment/PSP. Estes registros manuais não comprovam execução de Split Payment.

Migration: `20260926213917_AddManualPayments`, aplicada no banco descartável de aceite; aplicar nos demais ambientes antes do uso. Evidências locais em [STATUS](../STATUS.md), [TESTING](../TESTING.md) e [log](../IMPLEMENTATION-LOG.md).
