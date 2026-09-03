# ADR 0005 — PrimeNG 21 como biblioteca UI do MVP

Data: 2026-09-01  
Status: aceito

## Decisão

Usar Angular 21 e PrimeNG 21, ambos na última linha MIT disponível, com Tailwind CSS para layout e responsividade.

## Motivo

O projeto já possui telas e fluxos com PrimeNG. Migrá-los para outra biblioteca criaria retrabalho sem benefício proporcional para o MVP. PrimeNG 22 não será usado por causa da nova licença comercial.

## Limites

- Fixar dependências na linha 21; não atualizar automaticamente para a major 22.
- Acompanhar vulnerabilidades conhecidas, pois esta linha não recebe evolução comunitária.
- Avaliar nova migração de biblioteca antes de uma evolução grande do produto.
