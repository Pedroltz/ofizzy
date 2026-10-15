# Diretrizes de UX

- Priorizar a ação principal e reduzir decisões por tela.
- Desktop com sidebar; celular com Drawer e áreas de toque adequadas.
- A navegação deve ser sóbria: sem gradientes ou glow, com identidade tipográfica, agrupamento por tarefa e estado ativo perceptível por cor, borda e `aria-current`.
- PrimeNG para controles; Tailwind/CSS para layout e espaçamento.
- Toast para feedback, confirmação apenas em ações destrutivas e Skeleton em listas.
- Não mostrar menu ou botão que não possua fluxo real.
- Manter os tokens visuais centralizados e preservar os temas claro, escuro e sistema já implementados.
- Breakpoints oficiais: celular até 640 px, tablet de 641 a 900 px e desktop acima de 900 px.
- No celular, listagens operacionais usam cards; tabelas e o alternador de visualização ficam restritos ao desktop/tablet.
- Criação e edição de OS ocupam a tela móvel (`100dvh`), com conteúdo rolável, ações/total acessíveis e respeito a `safe-area`.
- Linhas de serviços e peças usam cards empilhados no celular e grade no desktop; não depender de rolagem horizontal para editar valores.
- Campos móveis usam no mínimo 16 px e controles interativos devem ter área mínima de 44 × 44 px.
- Toda rota deve funcionar a partir de 320 px sem overflow horizontal; validar também textos longos, estados vazios, carregamento e erro.

## Fluxo fiscal — 13/09/2026

Na OS concluída, concentrar preparação, emissão e download, com campos obrigatórios e erros compreensíveis. Explicar autorização parcial por documento e manter consulta/recuperação para resultado inconclusivo. Download exige XML autorizado. Não exibir uma tentativa como nota emitida.

Ações administrativas de cancelamento/inutilização exigem confirmação com identificação/intervalo. Preservar histórico e campos após erro. Verificar desktop, tablet e celular, sem overflow e com controles de 44 px. [Próximos incrementos](NEXT-STEPS.md).
