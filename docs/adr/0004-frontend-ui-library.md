# ADR 0004 — Angular Material como biblioteca de componentes

Data: 2026-09-01  
Status: substituída

## Contexto

O projeto foi iniciado com PrimeNG 22. A linha 22 passou a ter licença comercial; o último ramo comunitário sob MIT é o 21. Manter uma versão anterior bloquearia a evolução natural junto ao Angular 22 e criaria uma dependência deliberadamente defasada.

## Decisão

Esta decisão foi substituída pela ADR 0005 antes de a migração ser concluída.

Angular Material é mantido junto ao ecossistema Angular e distribuído sob licença MIT. A migração preservará Angular standalone, Reactive Forms, o tema próprio e os fluxos de UX já definidos.

## Consequências

- Os componentes PrimeNG existentes serão migrados progressivamente para equivalentes Mat.
- O design continuará próprio; não será adotada a aparência padrão de Material sem adaptação.
- Ícones passarão a usar Material Symbols, removendo primeicons.
- A Fase 2 só será marcada como concluída após a migração e os testes do frontend.

## Referências

- [Licença do Angular Components (MIT)](https://github.com/angular/components/blob/main/LICENSE)
- [Licença e histórico do PrimeNG](https://github.com/primefaces/primeng/blob/master/LICENSE.md)

## Contexto de continuidade — 13/09/2026

Esta ADR preserva a decisão e o contexto de sua data. A implementação atual mantém monólito modular, SaaS por tenant e PrimeNG 21 conforme as decisões posteriores aplicáveis. A integração fiscal direta está registrada na [ADR 0007](0007-direct-fiscal-integration.md). Estado e próximas entregas: [STATUS](../STATUS.md) e [NEXT-STEPS](../NEXT-STEPS.md).
