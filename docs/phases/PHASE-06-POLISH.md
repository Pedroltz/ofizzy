# Fase 6 — Robustez

Histórico do veículo, busca global, filtros, responsividade final, acessibilidade, E2E, logs e backup/restauração. Aceite: fluxos críticos passam em desktop/celular e uma restauração real é comprovada.

## Correção visual incremental — 2026-09-04

- [x] Modo lista de Ordens de Serviço com espaçamento lateral, larguras de coluna e alinhamento de ações revisados.
- [x] Modo bloco preservado e nenhuma rolagem horizontal adicional introduzida no layout responsivo.
- [x] Status da lista de OS reposicionado antes de Cliente/Veículo e dimensionado para textos longos.
- [x] Badge de status protegido contra corte visual com `nowrap` e overflow visível.
- [x] Status fixado na primeira coluna da lista, antes de Cliente/Veículo.
- [x] Ordem final da lista definida como OS, Cliente/Veículo, Status, Entrada, Total e Ações.
- [x] Tabela de OS da visão geral da Oficina alinhada com espaçamento lateral e status protegido contra corte.
- [x] Lista de OS abre detalhes pela linha inteira e agrupa ações contextuais em controle compacto.
- [x] Listas de Clientes e Veículos usam linha clicável, ações compactas e mantêm a tabela sem rolagem horizontal em telas médias.
- [x] Cards de Clientes e Veículos refatorados com ações coesas e ícones visíveis nos temas claro e escuro.
- [x] Ações de arquivamento usam glifo disponível no PrimeIcons instalado.
- [x] Impressão de OS longas preserva a seção de termos e linhas de assinatura em quebra de página segura.
- [x] Impressão de OS curtas não gera página vazia adicional.
- [x] Dashboard sem atalho duplicado de Nova OS; criação centralizada no cabeçalho global.
- [x] Seletor de tema centralizado na barra lateral e no drawer móvel, sem duplicação nos cabeçalhos.
- [x] Documentação consolidada em `docs/`, com referências internas atualizadas; `README.md` e arquivos de IA permanecem na raiz.

## Entrega antecipada — responsividade e E2E (2026-09-03)

- [x] Todas as rotas atuais sem overflow horizontal em desktop, celular e tablet.
- [x] Listagens em cards no celular, preservando tabela/cards no desktop.
- [x] Criação e edição de OS em tela cheia móvel, com linhas de serviços e peças em cards.
- [x] Áreas de toque de 44 px, inputs de 16 px e suporte a `safe-area` nos fluxos críticos.
- [x] Playwright configurado com projetos desktop, Pixel 7 e tablet: 23 cenários aprovados.
- [ ] Histórico do veículo, busca global, filtros adicionais, logs e backup/restauração permanecem pendentes na Fase 6.

## Performance de navegação e marca — 2026-09-04

- [x] Preload de rotas lazy habilitado.
- [x] Cache de sessão com TTL, deduplicação, invalidação seletiva e limpeza no logout.
- [x] Retorno às telas sem loading bloqueante quando existe snapshot.
- [x] Catálogos e veículos do editor de OS reutilizados entre aberturas.
- [x] Marca pública alterada para Ofizzy no frontend, impressão/PDF, defaults e documentação.
- [x] Cobertura unitária do cache e E2E de navegação repetida adicionadas.
- [x] Frontend lint/test/build/E2E e backend build/test aprovados.
- [ ] Smoke completo via Compose pendente apenas da conclusão do primeiro download da imagem SDK .NET; frontend Docker compilado e PostgreSQL preservado.
- [x] Dashboard dá largura total à lista em notebooks, compacta colunas auxiliares e alterna para cards pela largura real disponível, sem clipping ou overflow.
- [x] Listagem de OS compacta colunas no notebook e usa cards obrigatórios até 900 px, preservando Cliente/Veículo e ações sem overflow.
- [x] Código revisado sem estado morto ou regra CSS duplicada nos pontos alterados; validações essenciais preservadas.

## Continuidade consolidada — 13/09/2026

Acabamento continua parcial. Evidências antigas nesta fase são datadas; backup/restauração fiscal, observabilidade e otimização do bundle continuam trabalhos futuros.

O estado geral está em [STATUS](../STATUS.md); a ordem, dependências e critérios futuros estão em [NEXT-STEPS](../NEXT-STEPS.md). Resultados anteriores neste documento preservam a data e o escopo originais.
