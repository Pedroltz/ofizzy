# Histórico de implementação

## 2026-09-01 — Fase 1 concluída

- Criados repositório, solução, API, frontend, documentos e estrutura de deploy.
- Implementados setup, login, refresh rotation, logout, CSRF, rate limit e migrations.
- Implementado shell responsivo com PrimeNG 22.1 e Angular 22.1.
- Backend: 5 testes unitários e 1 teste de integração aprovados com PostgreSQL 18 real.
- Frontend: lint aprovado, build de produção aprovado e 1 teste Vitest aprovado.
- Infra: configurações Compose local/produção validadas, imagens construídas e os quatro serviços iniciados saudáveis.
- Smoke test via Nginx: SPA, setup protegido por CSRF e sessão autenticada aprovados.
- O volume usado no smoke test foi removido e recriado; o ambiente final está limpo e retorna `required: true` no setup.

Este arquivo é append-only: correções futuras ganham uma nova entrada.

## 2026-09-01 — Fase 2 em andamento

- Implementados no backend os módulos Customers, Vehicles, Services e Parts, com DTOs, FluentValidation, busca, arquivamento lógico, índices e a migration AddCatalogs.
- Implementadas páginas integradas para os quatro cadastros. A tentativa de migração para Angular Material foi abandonada antes de ser concluída.
- Dependências alinhadas para Angular 21, PrimeNG 21 e TypeScript 5.9 conforme ADR 0005. Build de produção do frontend aprovado; há aviso de orçamento inicial de bundle (652,92 kB contra 500 kB).
- Aceite final: 9 testes unitários e 2 testes de integração aprovados com PostgreSQL real/Testcontainers; lint, build e teste do frontend aprovados.
- Corrigido o fluxo CSRF pós-autenticação: setup/login confirmam a sessão via GET autenticado antes do primeiro POST protegido, gerando token vinculado ao usuário.
- A validação final da Fase 2 ainda não foi executada; não há aceite desta fase.

## 2026-09-02 — Fase 3 iniciada

- Implementado módulo de ordens de serviço com DTOs, validação, número sequencial gerado pelo PostgreSQL, snapshots de cliente/veículo/itens, serviços, peças e totais calculados no backend.
- Implementadas transições `Open → InProgress → Completed`, cancelamento antes da conclusão e imutabilidade após conclusão/cancelamento.
- Criadas migration `AddWorkOrders`, listagem e abertura de OS na rota Angular `/ordens`, integradas aos cadastros reais.
- Evidências: backend build sem avisos; 10 testes unitários e 3 de integração aprovados em PostgreSQL real; frontend lint, 1 teste e build aprovados. O build mantém o aviso conhecido de bundle inicial (654,62 kB contra 500 kB).
- Compose validado sintaticamente. A aplicação da migration pelo serviço `migrate` encontrou tabelas existentes sem histórico reconhecido, inclusive na tentativa isolada; os dados locais foram preservados. A causa do comportamento do ambiente Compose e o smoke pelo Nginx permanecem pendentes, embora a mesma cadeia de migrations tenha passado nos testes com PostgreSQL 18/Testcontainers.

## 2026-09-02 — Documentação reconciliada

- Roadmap atualizado para registrar a conclusão da Fase 2 e o progresso parcial da Fase 3.
- Changelog alinhado à stack vigente Angular 21 + PrimeNG 21 e às entregas de catálogos e ordens de serviço.
- Modelo de dados documentado com as entidades e invariantes já implementadas para OS.
## 2026-09-02 — Fase 3 concluída e validada

- Reconciliado o histórico de migrations no PostgreSQL local (`InitialIdentity` inserido em `__EFMigrationsHistory`) e aplicadas com sucesso as migrações `AddCatalogs` e `AddWorkOrders`. Todas as tabelas e a sequence `work_order_number_seq` estão ativas e persistidas.
- Corrigido o interceptor de erros da API no frontend (`api-error.interceptor.ts`) para extrair e exibir mensagens específicas de `ValidationProblemDetails` e mensagens limpas de `ConflictException`.
- Sanitizados os serviços de API (`catalog-api.service.ts` e `work-order-api.service.ts`) para tratamento seguro de parâmetros opcionais de query.
- Aperfeiçoada a tela de Clientes (`customers.page`): validação de documento (CPF 11 dígitos ou CNPJ 14 dígitos), formatação visual do documento e proteção de iniciais no avatar.
- Aperfeiçoada a tela de Veículos (`vehicles.page`): correção de busca no autocomplete de cliente, tratamento seguro de seleção por objeto, uppercase automático na placa e confirmação de arquivamento.
- Implementadas a visualização detalhada e a edição completa de Ordens de Serviço (`work-orders.page`):
  - Modal de detalhes exibindo cliente, veículo, KM, diagnóstico, observações, tabelas de serviços e peças com subtotais e total geral da OS.
  - Modal de criação e edição com reatividade cliente-veículos e aviso quando o cliente não possui veículos.
  - Diálogos de confirmação para finalização (`Completed`) e cancelamento (`Cancelled`).
- Corrigida a substituição de linhas de serviços e peças no backend (`WorkOrdersController.cs`): substituição do tracking implícito por adição explícita aos `DbSets`, solucionando a exceção `DbUpdateConcurrencyException` no endpoint `PUT /api/work-orders/{id}`.
- Tratamento limpo de mensagens de exceção de conflito em `GlobalExceptionHandler.cs`.
- Ampliada a cobertura de testes de integração em `WorkOrderFlowTests.cs` validando edição (`PUT`) de OS aberta antes das transições de estado.
- Evidências completas de aceitação:
  - Backend: 10 testes unitários e 3 testes de integração aprovados em PostgreSQL real (Testcontainers).
  - Frontend: lint aprovado com zero avisos, 1 teste Vitest aprovado e build de produção aprovado.
  - Compose & Nginx: stack completa rodando na porta 8080 com contêineres saudáveis (`postgres`, `backend`, `frontend`, `nginx`).
  - Smoke test e persistência: fluxo completo (login, listagem, criação, edição e finalização de OS) validado via Nginx e persistido com sucesso após reinício da stack (`docker compose restart`).

## 2026-09-02 — Execução local simplificada

- Habilitado .NET User Secrets na API para carregar a chave JWT e a conexão PostgreSQL no perfil `Development`.
- Mantida a autenticação também no desenvolvimento, evitando divergência de cookies, CSRF e sessão em relação à produção.
- Nenhuma credencial foi adicionada ao repositório.

## 2026-09-02 — Redesign visual profissional e resiliência de inicialização

- **Redesign completo do Frontend (UI/UX)**:
  - Eliminação total de emojis e caracteres especiais em todas as páginas e componentes (`app-shell`, `login`, `setup`, `dashboard`, `customers`, `vehicles`, `work-orders`, `settings`).
  - Adoção de ícones vetoriais padronizados com PrimeIcons (`pi pi-car`, `pi pi-wrench`, `pi pi-users`, `pi pi-file-edit`, `pi pi-sliders-h`, `pi pi-sign-out`, `pi pi-clock`, etc.).
  - Paleta visual refinada: fundo neutro ardósia (`slate-50`), cartões brancos com sombras sutis de elevação, sidebar corporativa escura (`slate-900`), badges semânticas com bordas finas.
  - Estilização automotiva autêntica para placas de veículos no padrão Mercosul/Brasil (fonte mono, contorno sutil, uppercase).
  - Dashboard executivo reorganizado: acesso rápido às principais ações, cards explicativos de operação e resumo em 4 etapas do ciclo de atendimento da oficina.
  - Modal de detalhes da OS aprimorado com cabeçalhos de seção, alinhamento numérico tabular e caixa de totalização destacada.
- **Resiliência na inicialização do backend (`Program.cs`)**:
  - Implementado carregamento automático com fallback em cascata para `Jwt:SigningKey` e conexão do PostgreSQL: verificação explícita de UserSecrets e leitura contextual do `.env` raiz caso os valores não estejam presentes na configuração, sem mutação global de variáveis de processo durante testes.
  - Testes unitários (10/10) e de integração (3/3) aprovados com Testcontainers.
  - Lint e build de produção do frontend aprovados.
  - Imagens Docker reconstruídas e stack Nginx validada na porta 8080.

## 2026-09-02 — Reformulação dos Modais de OS para Painéis Deslizantes (Slide-over Drawers)

- Substituição dos modais tradicionais centrais (`p-dialog`) por painéis laterais deslizantes (`p-drawer`) na direita da tela com backdrop suave:
  - **Drawer de Detalhes da OS**: cabeçalho com número da OS formatado e status em badge, metadados de abertura e conclusão, cards divididos para Cliente e Veículo, bloco técnico de Queixa e Diagnóstico da oficina com destaque em verde, tabelas de serviços e peças com subtotais e resumo financeiro em destaque (`Total Geral`). Rodapé fixo com ações contextuais de transição de status e edição.
  - **Drawer de Criação/Edição de OS**: formulário vertical fluido com cliente, veículo dinâmico, quilometragem, queixa, diagnóstico e seletores ágeis de catálogo para serviços e peças, com atualização instantânea de subtotais e total geral no rodapé.
- Estilos dedicados adicionados em `styles.css` (`.wo-drawer`, `.wo-block`, `.wo-table`, `.wo-grand-total`, `.wo-drawer-footer`).
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e imagem frontend reconstruída.

## 2026-09-02 — Visualização Híbrida de Dados (Tabelas e Cartões Ricos) e Contraste de Textos

- **Correção de Alto Contraste de Cores**:
  - Desativada a comutação automática de tema escuro do PrimeNG com `darkModeSelector: 'none'` em `app.config.ts`, prevenindo que navegadores/sistemas com tema escuro forçassem textos brancos sobre cartões de fundo claro.
  - Aplicada blindagem de estilo em `styles.css` para `select`, `select option`, `.wo-info-card strong`, `.order-card-identity h2` e `.catalog-row h3`, garantindo tom ardósia de máxima legibilidade (`#0f172a`).
- **Modo Híbrido de Listagem (Tabela e Cartões)**:
## 2026-09-02 — Lista em Faixas Amplas (Spacious Full-Width Rows) sem Rolagem Horizontal

- **Nova Apresentação de Lista Ampla para Clientes e Veículos**:
  - Implementada a visualização em faixas horizontais de largura total (`.spacious-list` e `.spacious-row`), eliminando o atrito da rolagem horizontal das tabelas densas e o aperto dos cartões tradicionais.
  - **Clientes**: avatar ampliado com iniciais (`3.2rem`), Nome Completo em destaque (`1.12rem`, peso 750, tom `#0f172a`), badge com CPF/CNPJ, chips confortáveis para Telefone, WhatsApp (verde) e E-mail, endereço com ícone de localização, notas e data de cadastro.
  - **Veículos**: placa no padrão Mercosul ampliada (`.plate-badge.large`), Marca & Modelo em evidência, tag de identificação do Proprietário, pílulas de Ano/Cor, Quilometragem e Chassi, sub-linha para anotações e data de cadastro.
  - Largura de contêiner expandida (`max-width: 86rem`) para aproveitar telas maiores com conforto ergonômico.
## 2026-09-02 — Reestruturação de Ordens de Serviço: Itens Manuais/Avulsos e Edição Direta de Valores

- **Flexibilidade Total no Lançamento de Serviços e Peças**:
  - Implementada a possibilidade de inserir serviços avulsos (`+ Novo Serviço Avulso`) e peças avulsas (`+ Nova Peça Avulsa`) diretamente na criação ou edição da OS, sem obrigatoriedade de pré-cadastro no catálogo.
  - O catálogo de serviços e peças permanece acessível via dropdown ágil (`+ Do Catálogo...`), preenchendo automaticamente os campos como sugestão inicial.
  - **Edição Direta por Linha**: cada linha de serviço ou peça possui campos editáveis para Descrição (texto livre), Código/Referência (para peças), Quantidade (numérico com decimais) e Valor Unitário (moeda BRL com recálculo instantâneo).
  - **Cálculo em Tempo Real**: subtotais individuais por item, subtotal de mão de obra, subtotal de peças e Total Geral da OS recalculados a cada alteração em tempo real.
  - **Validações Consistentes**: impedimento de salvar itens com descrição vazia ou valores negativos; integridade mantida contra regras de imutabilidade de OS finalizadas ou canceladas.
- **Backend**:
  - `WorkOrderLineRequest` estendido com `string? Code = null` e validado via FluentValidation.
  - `WorkOrdersController` grava o código fornecido para peças avulsas ou recupera do catálogo para itens catalogados.
- **Testes e Build**:
  - Testes de integração em `WorkOrderFlowTests` atualizados cobrindo inserção de itens manuais e catalogados com sucesso (todos os 3 testes aprovados).
## 2026-09-02 — Alinhamento e Organização Visual das Ordens de Serviço (CSS Grid & Spacious Rows)

- **Organização do Formulário do Drawer de OS**:
  - Reorganizados os campos gerais: KM e Observações Internas emparelhados em linha de altura simples, Queixa Relatada e Diagnóstico Técnico perfeitamente balanceados em duas colunas simétricas com `rows="2"`.
- **Grade Tabular Estruturada com CSS Grid para Itens da OS**:
  - Substituição de cartões soltos com labels repetidos por `.wo-grid-table` com cabeçalho de colunas unificado no topo da tabela (`Descrição`, `Cód/Ref`, `Qtd`, `Valor Unit.`, `Subtotal`, `Ações`).
  - Cada item compõe uma linha perfeitamente alinhada em colunas fixas com altura padronizada (`align-items: center`), garantindo leitura clara e sem desvios entre linhas.
  - Subtotais de cada linha e subtotais parciais de seção alinhados à direita com destaque visual.
- **Lista em Faixas Amplas na Página de Ordens de Serviço**:
  - A página principal de Ordens de Serviço adotou o mesmo padrão de faixas horizontais de largura total (`.spacious-list`), com badge da OS, cliente, placa, status e valor total sem quebras incômodas.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.

## 2026-09-02 — Painel Operacional no Dashboard: Ordens Ativas e Métricas em Tempo Real

- **Seção "Pátio da Oficina" no Painel Inicial**:
  - Exibição dinâmica das Ordens de Serviço abertas ou em andamento diretamente na tela inicial (`DashboardPage`), com atualização reativa via APIs de OS e Catálogos.
  - Bloquinhos modernos com indicador pulsante de OS ativas, identificação do cliente, placa Mercosul, modelo do veículo, data/hora de abertura (`createdAt`), valor da OS e link de acesso rápido.
  - Estado vazio inteligente: mensagem positiva *"Pátio Livre de Pendências"* com botão direto para emissão de nova OS quando todos os veículos estiverem liberados.
- **Grade de Métricas Rápidas Operacionais**:
  - 4 cards analíticos com ícones coloridos: *Ordens Ativas (no pátio)*, *Clientes Cadastrados (base total)*, *Veículos na Base (frota)* e *Ordens Concluídas (histórico finalizado)*.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e imagem Docker atualizada.

## 2026-09-02 — Correção de Responsividade e Rolagem no Modal de Ordem de Serviço

- **Fim dos Itens Cortados / Escondidos**:
  - Correção do bug de `min-height: auto` em `.wo-drawer-body` e `.wo-drawer-form`: aplicação de `min-height: 0 !important; flex: 1 1 auto; overflow-y: auto !important;`.
  - Cabeçalho (`p-drawer-header`) e rodapé (`p-drawer-footer`) agora possuem `flex-shrink: 0 !important`, permanecendo perfeitamente fixos (Sticky Header & Sticky Footer) enquanto o corpo rola suavemente mesmo com dezenas de itens.
- **Fim das Sobreposições em Telas Menores**:
  - As tabelas de serviços e peças (`.wo-table` e `.wo-grid-table`) foram envolvidas em contêineres com rolagem horizontal segura (`.wo-table-wrapper` e `.wo-grid-table-container`) e larguras mínimas protegidas (`520px` e `580px`), impedindo o colapso de números, preços e botões.
  - Cartões de Cliente e Veículo quebram responsivamente para 1 coluna em telas menores que 768px.
  - O card de Total Geral (`.wo-grand-total`) e o rodapé com múltiplos botões de ação (`.wo-drawer-footer`) agora quebram linhas ordenadamente (`flex-wrap: wrap; gap: .75rem`), sem transbordar ou empurrar elementos para fora da tela.
  - Largura do modal expandida para até `920px` / `940px` em monitores amplos, proporcionando conforto visual no lançamento de ordens volumosas.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.

## 2026-09-02 — Modal Centralizado em Ficha de Documento e Suporte a Impressão da OS

- **Substituição da Gaveta Lateral por Modal Centralizado Ergonômico**:
  - Migração de `<p-drawer>` lateral para modal centralizado `<p-dialog>` (`.wo-center-dialog`) de largura expandida (`min(1100px, 95vw)`).
  - Experiência visual centralizada, eliminando a sensação de "gaveta espremida no canto direito da tela".
- **Ficha Visual da Ordem de Serviço (Padrão Centro Automotivo)**:
  - Cabeçalho de documento com numeração destacada (`OS #0001`), subtítulo institucional da Sport Pneus, tag de status com ícones e data/hora de abertura e conclusão.
  - Painel de identificação em duas colunas amplas (`.wo-doc-info-grid`): card do Cliente (com telefone/WhatsApp e CPF/CNPJ) e card do Veículo (com placa Mercosul em tamanho ampliado e KM registrada).
  - Tabelas de Mão de Obra e Peças com largura total, cabeçalho cinza suave, colunas espaçosas e tipografia nítida de alto contraste.
  - Quadro de fechamento financeiro estilo fatura/recibo (`.wo-doc-total-box`), com subtotais detalhados e valor total da OS em verde esmeralda.
- **Recursos Adicionais e Impressão**:
  - Adicionado botão **"Imprimir Ficha"** (`printOrder()` chamando `window.print()`) com folha de estilos `@media print` que isola a ficha da ordem de serviço para gerar PDF ou impressão física limpa.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.


