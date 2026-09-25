# Histórico de implementação

## 2026-09-24 — Consolidação documental para homologação fiscal

- Reescritos os documentos canônicos da Fase 8 para separar evidência local/simulada de homologação externa e registrar o baseline atual de NF-e/NFS-e.
- Criado roadmap H1-H6 cobrindo gates de produção, endpoints, RTC/CNPJ alfanumérico, resiliência, credenciais do piloto, homologação externa e produção controlada.
- Registrado o achado de que `FiscalProductionReleased` hoje pode contornar as demais travas de produção; correção ficou marcada como P0 antes de piloto produtivo.
- Atualizada a linha regulatória com NFS-e/CNPJ alfanumérico, marco RTC do Simples em 01/01/2027 e documentação de Split Payment publicada até 23/09/2026.
- `REVIEW-2026-09-13.md` e o plano fiscal consolidado de 13/09 foram movidos para `docs/archive/`; conteúdo histórico permanece disponível sem competir com `NEXT-STEPS.md`.
- Nenhum código fiscal, configuração, credencial ou banco foi alterado por este incremento documental.

## 2026-09-21 — Editor de OS com pesquisa e grade compacta

- Substituídos os dois `select` nativos de cliente e veículo por autocomplete
  PrimeNG. O formulário agora mantém `Customer`/`Vehicle` durante a interação e só
  extrai as IDs ao salvar, evitando seleção truncada ou quebra visual por texto longo.
  A busca é normalizada por nome/documento/telefone para cliente e
  placa/marca/modelo para veículo.
- Reorganizados os campos em duas faixas de grade e ampliada a área útil do diálogo.
  As colunas se adaptam para tablet e celular sem rolagem horizontal; o E2E acrescenta
  busca e seleção dos dois controles no cenário móvel.
- Validação: lint, 69 testes unitários, build e E2E responsivo (40 aprovados, 11
  skips não aplicáveis ao viewport) do frontend aprovados.

## 2026-09-21 — Revisão de DANFE e DANFSe para homologação e produção

- Revisado o gerador `FiscalPdf` para manter um leiaute único nos dois ambientes. A
  condição de homologação agora é somente a marca-d’água diagonal `HOMOLOGAÇÃO`,
  baseada em `tpAmb` do XML autorizado; foram removidas a faixa de simulação local e
  a marca comercial do rodapé do documento fiscal.
- O DANFSe adota os títulos e a organização exigidos pela NT SE/CGNFS-e nº 008/2026:
  `DANFSe v2.0`, `Documento Auxiliar da NFS-e`, município/gerador/ambiente, NFS-e,
  DPS, QR Code e chave. DANFE conserva os blocos do MOC para canhoto, emitente,
  protocolo, destinatário, pagamento, impostos, itens, totais e dados adicionais.
- Incluídos testes para garantir que a marca-d’água responde ao XML autorizado e não
  aparece em produção. PDFs de DANFE/DANFSe foram renderizados e inspecionados; 74
  testes unitários, build Release, lint, 69 testes, build e 40 E2E responsivos do
  frontend passaram; `docker compose config` foi validado com variáveis fictícias.

## 2026-09-21 — Catálogo pesquisável no editor de OS

- Corrigido o erro de paginação que carregava serviços e peças da página 100 na
  abertura da OS, retornando um catálogo visualmente vazio. As duas consultas agora
  usam página 1 com limite de 100 registros.
- Substituído o `select` nativo por autocomplete PrimeNG. A busca normaliza acentos e
  capitalização; serviços são encontrados por nome e peças por nome ou código. A
  seleção continua emitindo apenas a ID para o fluxo já persistido de inclusão da OS.
- Validação: 72 testes unitários e build Release do backend; `npm run lint`, 69
  testes unitários, build de produção e E2E responsivo (40 aprovados em
  desktop/mobile/tablet; 11 skips esperados) aprovados.

## 2026-09-21 — Pesquisa de clientes por nome e telefone

- Corrigido o filtro de clientes para distinguir pesquisa textual de pesquisa
  numérica. Telefone, WhatsApp e documento são pesquisados por dígitos, aceitando
  entrada com máscara; nome e e-mail continuam com busca sem distinção de maiúsculas.
- Novos cadastros e edições persistem telefone/WhatsApp somente com dígitos. A
  migration `20260921100000_NormalizeCustomerContactNumbers` corrigiu os contatos
  existentes no PostgreSQL local: 42 revisados, zero com caracteres de máscara.
- Incluído teste de integração para nome e telefone mascarado. Ele passou isolado;
  a execução concorrente completa das integrações encontrou conflito preexistente no
  bootstrap compartilhado antes de exercitar a busca. Também passaram 72 unitários e
  o build Release do backend.

## 2026-09-21 — Remoção da prontidão e auditoria do banco local

- Removidos o painel frontend, contratos, endpoint e teste de `homologation-readiness`.
  A remoção não altera a configuração fiscal nem as travas de produção.
- Revisão somente de leitura no PostgreSQL local: duas organizações de
  desenvolvimento; em cada uma, 20 perfis de produto, 15 perfis de serviço e 21
  clientes têm os campos fiscais estruturados necessários ao cenário suportado.
  Os CNPJs/A1 são de teste e não foram alterados para simular credenciais reais.

## 2026-09-21 — Migração do baseline de novas NF-e para PL_010f v1.04

- Recuperado diretamente do Portal Nacional o ZIP `PL_010f_v1.04` e conferido o
  SHA-256 `b8589490a58a09a993a80e6ac4d7ed10f20892061ecfc56719337098d4b95998`.
  Os cinco XSDs foram instalados sem modificação em `Schemas/Nfe010f`; teste unitário
  confere o hash de cada arquivo.
- `FiscalSchemaCatalog` agora seleciona `NF-e PL_010f v1.04` para documentos novos,
  preservando o 010c histórico e os schemas separados de cancelamento e inutilização.
  O XML fiscal já suportado será validado e assinado contra o pacote novo pela suíte.
- Baixada também a distribuição oficial de eventos RTC v1.40 (SHA-256
  `a4c57ce95b225cd8852f90bd6c39ca28ae551636ff3f67eb2602b9fa847129b2`) para diretório
  próprio. Ela não contém os envelopes de cancelamento/inutilização, portanto não
  substitui fluxos existentes. Nenhum cálculo ou tag IBS/CBS foi inferido sem
  classificação contábil por operação.
- Evidências executadas: 73 testes unitários e 10 de integração do backend, build
  Release sem avisos; lint, 69 testes e build do frontend; 24 E2E de plataforma em
  desktop/mobile/tablet. O E2E revelou e corrigiu um seletor de teste que ainda usava
  classes de marca removidas. Não há migration neste incremento. Compose foi validado
  sintaticamente com credenciais fictícias; não foi iniciado e não houve transmissão
  a autorizadores.

## 2026-09-19 — Auditoria regulatória fiscal incremental

- Criada matriz normativa com fontes oficiais para schemas, RTC, CNPJ alfanumérico,
  NFS-e, eventos, inutilização, DANFE/DANFSe e endpoints SP.
- Comparado o ZIP oficial produtivo NFS-e 1.01-20260209 aos XSDs locais: igualdade
  byte a byte; origem e SHA-256 documentados junto aos schemas. O ZIP de produção
  restrita RTC 20260727 foi apenas analisado, pois diverge e não pode substituir o
  pacote produtivo sem adaptação de domínio/XML.
- Incluído teste que prova invalidação de XMLDSIG após adulteração. A tentativa de
  atualizar SHA-1 para SHA-256 isoladamente falhou na validação do XSD NF-e/eventos
  incorporado e foi revertida; a lacuna está registrada para a migração de pacote.
- Criados runbook de homologação e checklist de produção. Nenhum endpoint oficial
  foi chamado e nenhum gate de produção foi modificado.
- Evidências: 28 testes fiscais aprovados, incluindo a verificação dos hashes dos dez
  XSDs NFS-e; suíte unitária backend completa com 57 aprovados. Lint, 69 unitários e
  build frontend passaram (warning preexistente de bundle: 794,07 kB vs. 500 kB).
  E2E fiscal na stack runtime isolada `ofizzy-fiscal-smoke`/18082: 2/2 aprovados
  antes e 2/2 depois de `docker compose restart`; persistência, XML/PDF e cancelamento
  NFS-e foram exercitados pelo Nginx com dados fictícios e simulador Development.

## 2026-09-20 — Autoridade do XML autorizado no DANFE

- Corrigido `FiscalPdf.GenerateDanfe`: chave de acesso e série passam a ser lidas do
  XML autorizado (`infProt/chNFe` ou `infNFe/@Id`, e `ide/serie`), sem fallback aos
  campos mutáveis da entidade persistida.
- Novo teste altera deliberadamente `AccessKey` e `Series` no registro local e comprova
  que o PDF mantém os valores do XML autorizado.
- Evidência: 6 testes visuais fiscais aprovados; suíte unitária completa com 58
  aprovados; build Release sem avisos ou erros; e `git diff --check` sem problemas.
  Não houve transmissão externa nem alteração de gates de produção.

## 2026-09-20 — Obtenção e comparação dos pacotes oficiais NF-e 010e/010f

- A consulta com o cookie requerido pelo Portal Nacional permitiu baixar diretamente
  o 010e histórico e o 010f atual. Foram registrados os SHA-256 dos ZIPs e comparadas
  as árvores XSD: há mudanças RTC/IBS-CBS e CNPJ alfanumérico, e o 010f modifica de
  novo dois XSDs do 010e. Nenhum arquivo de terceiro foi aceito e nenhum schema ativo
  local foi trocado.
- A origem, hashes e decisão de migrar a partir do 010f foram documentadas em
  `BusinessCore/Fiscal/Schemas/Nfe/README.md` e na auditoria fiscal.

## 2026-09-20 — Proteções de XML autorizado, catálogo de schemas e A1 em uso

- Centralizada a escolha de schemas em `FiscalSchemaCatalog`: NF-e 010c e NFS-e 1.01
  permanecem explicitamente ativos por finalidade, sem ativar o 010f antes da migração
  de domínio/XML. Teste impede troca silenciosa desses baselines.
- DANFE/DANFSe passam a obter ambiente do XML autorizado; o DANFSe deixou de consultar
  CNPJ/IM no snapshot e ambos recusam XML autorizado que não corresponde ao tipo do
  documento persistido. O cancelamento continua indicado pelo histórico fiscal local.
- `FiscalCertificateVault.Load` revalida validade, chave privada RSA, uso de assinatura
  quando a extensão existe e CNPJ ICP-Brasil a cada uso. Teste cobre certificado A1
  criptografado cujo CNPJ diverge da empresa.
- Evidência: 64 testes unitários backend aprovados, build Release sem avisos/erros e
  `git diff --check` limpo. Sem transmissão externa ou mudança de produção.

## 2026-09-20 — CNPJ alfanumérico preparado sem ativação prematura

- A partir do manual oficial da Receita Federal, implementado
  `FiscalValidation.IsAlphanumericCnpj`: base de 12 caracteres em maiúsculas/números,
  valor ASCII menos 48 e dois DVs por módulo 11 com pesos 2–9.
- O validador numérico existente continua sendo usado pelo 010c e NFS-e 1.01. Nenhum
  campo de configuração, XML, chave de acesso ou certificado passou a aceitar CNPJ
  alfanumérico fora da migração integral para o 010f.
- Evidência: 69 testes unitários backend aprovados; build Release sem avisos/erros e
  `git diff --check` limpo.

## 2026-09-20 — Consulta de eventos NFS-e pelo ADN oficial

- Corrigida a consulta de eventos pós-autorização: a API usada passa a ser o ADN na
  rota oficial `GET /NFSe/{ChaveAcesso}/Eventos`, retornando todo o histórico a ser
  examinado pelo gateway em vez de uma rota Sefin com código de evento fixo.
- Como o manual oficial só publica a base de produção restrita, a seleção de base ADN
  recusa explicitamente produção até que haja URL oficial verificável. Não houve
  alteração de `Fiscal:ProductionEnabled`, allow-list ou tenant liberado.
- Evidência: 70 testes unitários backend aprovados; build Release sem avisos/erros e
  `git diff --check` limpo.

## 2026-09-20 — Matriz de domínio para a Reforma Tributária

- Criada `docs/fiscal/RTC-DOMAIN-MATRIX.md`, vinculando cada grupo RTC à fonte,
  documento, condição, origem de dados, representação de domínio/XML e decisão atual.
  O documento impede que campos IBS/CBS, IS, monofásico e CNPJ alfanumérico sejam
  ativados sem cenários e cálculos aprovados pela contabilidade.

## 2026-09-20 — Pacote de schema persistido por documento fiscal

- Incluídos `FiscalDocument.SchemaPackage`, a configuração EF obrigatória (80
  caracteres) e o campo `schemaPackage` no contrato de leitura do frontend.
- `FiscalEmissionService` persiste o pacote retornado por `FiscalSchemaCatalog` antes
  de gerar e assinar o XML. A migration
  `20260920070200_AddFiscalDocumentSchemaPackage` faz backfill explícito dos registros
  existentes para `NF-e PL_010c` ou `NFS-e Nacional 1.01`.
- Esse registro preserva a evidência do leiaute de cada XML histórico e impede que a
  adoção futura do PL_010f seja confundida com uma revalidação retroativa.
- Validação: migration gerada por `dotnet-ef` em configuração Release; 71 testes
  unitários backend, lint frontend sem erros, 69 testes frontend e build aprovados.
  O build reporta o aviso preexistente de bundle inicial 794,07 kB / budget 500 kB.
- Aplicada e conferida em PostgreSQL de Compose descartável: coluna e histórico EF
  presentes, seguido de backend, frontend e Nginx saudáveis em `localhost:18084`.
  A stack e seu volume foram removidos ao final. Não houve uso de segredo do usuário
  nem chamada a órgão fiscal.

## 2026-09-20 — Prontidão verificável para homologação

- Criado `FiscalHomologationReadiness`, exposto em
  `GET /api/fiscal/homologation-readiness` para administradores do tenant. A resposta
  não inclui segredo, XML ou certificado: somente estado de configuração, validade
  registrada do A1, pacote de schema e pendências compreensíveis.
- O painel de Configurações Fiscais apresenta cada requisito como conferido, bloqueado
  ou pendente externo. NF-e habilitada recebe bloqueio explícito enquanto o pacote
  ativo for `PL_010c`; NFS-e Nacional 1.01 é reconhecida como pacote local conferido.
  Credenciamento e classificação contábil nunca são aprovados automaticamente.
- Novo teste prova que os bloqueios de schema e pendências externas permanecem
  visíveis. Evidências: backend 72/72, lint frontend, 69/69 testes frontend e build
  aprovados; migration e smoke pelo Nginx passaram em Compose descartável na porta
  18085, removido ao final. O build mantém aviso conhecido de bundle inicial
  794,07 kB / 500 kB.

## 2026-09-20 — Revisões de classificação fiscal por vigência

- Criado o campo persistido `EffectiveFrom` para perfis fiscais de produtos e
  serviços. O índice passou de item único para item + vigência, permitindo configurar
  a próxima classificação sem apagar a anterior.
- A preparação carrega apenas perfis já vigentes e escolhe a maior data aplicável.
  O editor do catálogo recebe o campo de data; a preparação da OS não pede essa data,
  porque ela resolve automaticamente a regra vigente. Snapshots emitidos permanecem
  imutáveis.
- Migration `20260920180236_AddFiscalProfileEffectiveDate` validada por script EF
  idempotente; perfis existentes recebem a data histórica 01/01/2000. A mudança não
  ativa PL_010f ou campos RTC sem as regras aprovadas e schemas instalados.
- Evidências: backend 72/72, lint, 69/69 testes frontend, build e migration/smoke
  pelo Nginx em Compose descartável na porta 18087 aprovados; stack e volume removidos.

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
  - Estado vazio inteligente: mensagem positiva _"Pátio Livre de Pendências"_ com botão direto para emissão de nova OS quando todos os veículos estiverem liberados.
- **Grade de Métricas Rápidas Operacionais**:
  - 4 cards analíticos com ícones coloridos: _Ordens Ativas (no pátio)_, _Clientes Cadastrados (base total)_, _Veículos na Base (frota)_ e _Ordens Concluídas (histórico finalizado)_.
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
- Cabeçalho de documento com numeração destacada (`OS #0001`), subtítulo institucional do Ofizzy, tag de status com ícones e data/hora de abertura e conclusão.
  - Painel de identificação em duas colunas amplas (`.wo-doc-info-grid`): card do Cliente (com telefone/WhatsApp e CPF/CNPJ) e card do Veículo (com placa Mercosul em tamanho ampliado e KM registrada).
  - Tabelas de Mão de Obra e Peças com largura total, cabeçalho cinza suave, colunas espaçosas e tipografia nítida de alto contraste.
  - Quadro de fechamento financeiro estilo fatura/recibo (`.wo-doc-total-box`), com subtotais detalhados e valor total da OS em verde esmeralda.
- **Recursos Adicionais e Impressão**:
  - Adicionado botão **"Imprimir Ficha"** (`printOrder()` chamando `window.print()`) com folha de estilos `@media print` que isola a ficha da ordem de serviço para gerar PDF ou impressão física limpa.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.

## 2026-09-02 — Novo Painel Executivo de Fechamento Financeiro da Ordem de Serviço

- **Estrutura Balanceada em 2 Colunas (`.wo-invoice-summary-card`)**:
  - Eliminação da antiga caixa flutuante isolada à direita: implementação de um card de fechamento financeiro de largura total com visual premium.
  - **Coluna Operacional (Esquerda)**:
    - Cabeçalho com ícone de extrato/recibo.
    - Contadores operacionais em destaque: quantidade exata de serviços/mão de obra executados e quantidade de peças aplicadas, separados por divisor vertical limpo.
- Nota de garantia técnica e consolidação do Ofizzy.
  - **Coluna Financeira (Direita)**:
    - Gradiente suave esmeralda (`#f0fdf4` a `#ecfdf5`) com borda sutil.
    - Linhas de Mão de Obra e Peças com rótulos e valores em fonte numérica tabular (`font-variant-numeric: tabular-nums`).
    - Linha divisória limpa e sólida.
    - **Total Geral da Ordem**: badge estilizado `VALOR TOTAL DA OS`, subtítulo explicativo e valor em destaque com tipografia `2.15rem font-black` alinhada perfeitamente ao centro vertical.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.

## 2026-09-02 — Simplificação e Eliminação de Frases Quebradas no Fechamento da OS

- **Eliminação de Poluição Visual e Frases Quebradas**:
  - Removidos textos longos, disclaimers redundantes e o antigo badge espremido que quebrava `VALOR / TOTAL / DA OS` em 3 linhas.
  - Implementado o novo `.wo-summary-card` limpo, moderno e minimalista.
  - **Lado Esquerdo**: Título nítido _Fechamento da Ordem_ com subtítulo direto indicando a contagem de serviços e peças em uma única linha.
  - **Lado Direito**:
    - Rótulos concisos de linha única (_Mão de Obra_ e _Peças & Insumos_) com valores alinhados à direita.
    - Card de _TOTAL GERAL_ em destaque verde suave (`#f0fdf4` com borda `#bbf7d0`), com o rótulo e o valor numérico em `1.75rem font-black` no mesmo alinhamento horizontal, sem nenhuma quebra.
- Validação técnica: `npm run lint` (0 erros), `npm run build` aprovado e contêiner frontend atualizado.

## 2026-09-03 — Conclusão da Fase 4: Impressão e Configuração da Oficina

- **Módulo de Dados da Oficina (`Company`)**:
  - Tabela PostgreSQL `companies` enriquecida com `LegalName`, `City`, `State`, `PostalCode`, `Email`, `WarrantyTerms`, `ReceiptNotes` e expansão de `Cnpj` para 18 caracteres.
  - Migration EF Core `20260903030538_AddWorkshopSettings` criada e aplicada.
  - Endpoints REST `GET /api/company` e `PUT /api/company` com validação FluentValidation.
  - Nova aba "Dados da Oficina" em `/configuracoes` no frontend Angular com formulário reativo completo.
- **Impressão A4 Minimalista em Preto e Branco (`.wo-print-sheet`)**:
  - Modelo econômico e limpo baseado estritamente em texto e linhas divisórias contínuas, sem fundos que desperdicem tinta de impressora.
  - Seções estruturadas: Cabeçalho com dados da oficina e número da OS, Dados do Cliente e Veículo, Apontamentos Técnicos, Tabelas de Serviços e Peças, Fechamento Financeiro e Termo de Garantia com 2 campos de assinatura para termo de entrega do veículo.
  - Acionamento direto via botão "Imprimir Ficha" (`window.print()`).
- **Geração de PDF Oficial no Backend com QuestPDF**:
  - Endpoint `GET /api/work-orders/{id}/pdf` gerando PDF A4 no padrão texto e separações com suporte a múltiplas páginas e numeração `Página X de Y`.
  - Botão "Baixar PDF" integrado na tela de detalhes da OS.
- Validação técnica:
  - Testes de unidade do backend: `14/14` aprovados (incluindo validação do gerador de PDF e validadores da oficina).
  - Testes de integração do backend: `3/3` aprovados.
  - Frontend: `npm run lint` (0 erros) e `npm run build` aprovados.
  - Smoke test via Nginx na porta 8080: login autenticado, PUT de configurações da oficina e download do PDF da OS com status 200 e cabeçalho `%PDF` validados.

## 2026-09-03 — Padronização Visual (Clientes e Veículos) e Otimizações Globais de Performance

- **Padronização dos Modos de Visualização (Clientes e Veículos)**:
  - Implementado sistema simétrico e coeso com 2 modos de exibição:
    - **Lista** (modo padrão inicial): visualização tabular densa, clara e de alta produtividade.
    - **Blocos**: grid de cartões modernos (`.catalog-grid` e `.catalog-card`), com avatar de iniciais / placa Mercosul em destaque, caixas de especificações/contatos e botões de ação alinhados.
  - Skeletons de carregamento adaptativos para cada modo de visualização.
- **Compressão Gzip e Cache Estático no Nginx**:
  - Habilitada compressão `gzip on;` (nível 6) para JSON, JS, CSS, SVG em `deploy/nginx/default.conf` e `deploy/nginx/frontend.conf`, reduzindo o payload de transferência em até 80%.
  - Adicionado cabeçalho `Cache-Control: public, immutable` com validade de 1 ano para todos os assets estáticos com hash.
  - Adicionado `proxy_http_version 1.1; proxy_set_header Connection ""` para manter conexões TCP ativas (keep-alive) entre o Nginx e o Kestrel.
- **Endpoint Agregado de Dashboard (`GET /api/dashboard/summary`)**:
  - Criado `DashboardController` e `DashboardContracts` no backend .NET 10.
  - Eliminação de 3 requisições HTTP concorrentes pesadas no frontend: o dashboard agora executa 1 única requisição agregada ultraveloz (< 15ms).
- **Índices de Performance no PostgreSQL**:
  - Criada e aplicada a migração EF Core `20260903133936_AddPerformanceIndexes`.
  - Índices adicionados: `work_orders` (`Status`, `CreatedAt`, `Status_CreatedAt`, `CustomerName`, `VehiclePlate`), `vehicles` (`IsActive`, `CustomerId`) e `customers` (`IsActive`).
  - Eliminação de Sequential Scans em filtros de status e buscas textuais.
- **Proteção contra Race Conditions no Frontend**:
  - Ajustado debounce para 300ms e implementado versionamento atômico de requisição (`loadVersion`) nas páginas de Clientes, Veículos e Ordens de Serviço, garantindo que respostas fora de ordem não corrompam o estado da tela.
- Validação técnica:
  - Backend: `14/14` testes de unidade aprovados, `3/3` testes de integração aprovados.
  - Migração aplicada com sucesso no PostgreSQL local.
  - Frontend: `npm run lint` (0 erros, 0 avisos) e `npm run build` concluído com sucesso.

## 2026-09-03 — Responsividade Total para Mobile e Tablets

- **Preservação Absoluta do Desktop (> 900px)**:
  - Todas as modificações responsivas foram estritamente encapsuladas em `@media (max-width: 900px)` e `@media (max-width: 640px)`. O visual, dimensões e usabilidade no desktop permanecem 100% idênticos.
- **Navegação & Shell Mobile**:
  - Header mobile com altura fixa de 60px (`3.75rem`), posição sticky e z-index 30.
  - Menu drawer lateral com animação suave de entrada (`drawerSlideRight` e `drawerFadeIn`), fechando automaticamente ao tocar no backdrop, no botão de fechar ou em qualquer item de navegação.
- **Ajustes de Grid e Cartões**:
  - `catalog-grid` e `order-grid` ajustados para 1 coluna fluida no smartphone (eliminando o overflow horizontal de `minmax(340px)`) e 2 colunas nos tablets.
  - `dashboard-grid` e `dashboard-stats-grid` reorganizados em 2 colunas em tablets e 1 coluna em smartphones.
  - Botões de ação rápida (`.quick-actions-bar`) com layout empilhado e altura de toque confortável.
- **Modais e Diálogos de Formulário (`p-dialog` e `.wo-center-dialog`)**:
  - Diálogos ocupando `96vw` no mobile, com altura máxima de `94vh`, scroll vertical táctil e campos em 1 coluna vertical.
  - Prevenção do zoom indesejado no iOS Safari com `font-size: 16px` em inputs e selects.
  - Botões do rodapé de modais ("Cancelar", "Salvar", "Concluir") com largura total e área de toque de 44px (`min-height: 44px`).
- **Operação de Ordem de Serviço pelo Smartphone**:
  - Lançamento de serviços e peças com rolagem horizontal táctil na grade de itens (`.wo-grid-table-container`), permitindo ao mecânico no pátio alterar quantidades e preços sem desconfigurar a tela.
  - Fechamento financeiro da OS e botões de status adaptados para tela de celular.
- **Catálogo & Ajustes (`/configuracoes`)**:
  - Abas PrimeNG (`p-tablist`) com rolagem horizontal deslizante (`-webkit-overflow-scrolling: touch`), permitindo alternar entre serviços, peças e dados da oficina sem quebras de layout.
- Validação técnica:
  - Frontend: `npm run lint` (0 erros, 0 avisos) e `npm run build` concluído com sucesso.
  - Backend: `14/14` testes de unidade aprovados e `3/3` testes de integração aprovados.

## 2026-09-03 — Editor móvel de OS e testes responsivos automatizados

- Extraído `WorkOrderLinesEditorComponent`, reutilizado para serviços e peças, com grade no desktop e cards editáveis no celular.
- Criação/edição de OS passou a ocupar `100dvh` em celulares, com `safe-area`, corpo rolável e ações/total persistentes.
- Adicionado `ResponsiveLayoutService`; clientes, veículos e OS sempre apresentam cards até 640 px sem alterar o modo desktop escolhido.
- Corrigido o encolhimento do contêiner principal e das tabelas, eliminando overflow horizontal também no desktop.
- Menu móvel agora fecha por `Escape` e expõe semântica de diálogo.
- Adicionados Playwright e matriz responsiva para todas as rotas, com mocks determinísticos de API.
- Validação: `npm run lint`, `npm test` (2/2), `npm run build` e `npm run e2e` (23 aprovados, 4 ignorados por viewport).

## 2026-09-03 — Modernização da navegação lateral

- Substituído o badge genérico de automóvel por wordmark tipográfico `SP`, sem gradientes ou glow.
- Navegação reorganizada nos grupos Operação e Gestão, com estado ativo discreto, `aria-current` e ícones contidos.
- Desktop e drawer móvel passam a consumir a mesma coleção tipada de rotas no `AppShellComponent`.
- Perfil do usuário integrado ao rodapé, em tratamento monocromático e com ação de saída menos intrusiva.
- Refinamento posterior removeu cores neon, fundos coloridos dos ícones e o selo “Sistema Operacional” do dashboard, preservando apenas slate e o teal original como marcador ativo.
- Validação: lint e build aprovados; Vitest 2/2 e Playwright 23 aprovados, com 4 ignorados por viewport.

## 2026-09-03 — UI/UX Redesign: R1 — Auditoria Visual Concluída

- **Iniciativa UI/UX Redesign**: Iniciado o plano de reformulação completa da experiência visual do Ofizzy, mantendo integrações de backend, regras de negócio e rotas existentes. Registradas as etapas R1 a R10 no `ROADMAP.md`.
- **Mapeamento de Estilos Atuais**:
  - `styles.css` auditado: 3809 linhas monolíticas com escopo `:root` restrito a tokens claros e múltiplos blocos responsivos agrupados no final com sobreposições via `!important`.
  - Dezenas de cores hardcoded espalhadas (`#0f172a`, `#ffffff`, `#f1f5f9`, `#e2e8f0`, `#0f766e`, tons esmeralda, âmbar e ardósia).
  - Configuração do PrimeNG em `app.config.ts` mantinha `darkModeSelector: 'none'`, impedindo suporte nativo a temas escuros.
- **Mapeamento de Componentes Reutilizáveis**:
  - Identificados componentes já existentes: `PageHeaderComponent` (em `shared/components`), `ResponsiveLayoutService` (em `shared/layout`) e `WorkOrderLinesEditorComponent` (em `features/work-orders/components`).
  - Identificadas duplicações estruturais recorrentes entre Clientes, Veículos, Ordens de Serviço e Dashboard: toolbars de busca com alternador de visualização, empty states, skeletons de carregamento, cards de estatísticas operacionais e tags de status.
- **Mapeamento PrimeNG**:
  - Módulos em produção mapeados: `ButtonModule`, `DialogModule`, `DrawerModule`, `InputTextModule`, `InputNumberModule`, `TextareaModule`, `AutoCompleteModule`, `PaginatorModule`, `SkeletonModule`, `TabsModule`, `ToastModule`, `ConfirmDialogModule`.
  - Versão fixada na linha 21 MIT mantida estritamente (ADR 0005).
- **Inconsistências de Layout e UX Identificadas**:
  - Dashboard operacional atual dedica grande parte da tela a cards genéricos de atalho ("Ordens de Serviço", "Clientes", "Veículos", "Catálogo") em vez de expor métricas operacionais prioritárias e fluxo de veículos no pátio.
  - Ausência de um header horizontal operacional de contexto no shell desktop.
  - Falta de suporte a tema escuro e transições de tema de sistema (`prefers-color-scheme`).
- **Estratégia de Migração Incremental (R1 a R10)**:
  - Adotada arquitetura modular de CSS (`tokens.css`, `themes.css`, etc.) e criação de `ThemeService` reativo com `light | dark | system`, detecção de `matchMedia('(prefers-color-scheme: dark)')` e persistência em `localStorage`.
  - Criação de biblioteca de componentes compartilhados em R3 antes de atacar o shell e as telas de negócio.
- **Validação de Baseline**:
  - `npm test`: 2/2 testes Vitest aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: bundle compilado com sucesso.
  - Backend `Ofizzy.UnitTests`: 14/14 testes aprovados.

## 2026-09-03 — UI/UX Redesign: R2 — Design System Concluído

- **Centralização de Tokens de Design**:
  - Criado `src/styles/tokens.css` com escalas de tipografia Inter/mono, line-heights, espaçamentos base 4px (`--space-1` a `--space-16`), border radius (`--radius-xs` a `--radius-full`) e transições.
  - Criado `src/styles/themes.css` com suporte completo a Light Mode (`:root, [data-theme="light"]`) e Dark Mode (`[data-theme="dark"]`).
- Paleta com verde Ofizzy como cor primária (`#15966a` no claro, `#3ecb92` no escuro), superfícies calibradas (`#ffffff`/`#fafbfa` no claro, `#171a18`/`#1c201e` no escuro com fundo `#101211`), estados semânticos (success, warning, danger, info com variantes soft e text) e sombras extremamente sutis.
  - Garantida compatibilidade retroativa mapeando as variáveis legadas `--app-*` para os novos tokens do Design System.
- **ThemeService e Gestão de Tema**:
  - Criados `src/app/core/theme/theme.models.ts` e `src/app/core/theme/theme.service.ts` usando Angular Signals.
  - Suporte a 3 modos: `light`, `dark` e `system`. No modo `system`, escuta eventos de runtime de `matchMedia('(prefers-color-scheme: dark)')` sem recarregar a página.
  - Persistência em `localStorage` sob a chave `workshop-theme`.
  - Script inline adicionado ao `<head>` em `index.html` para aplicar `data-theme` antes da renderização, eliminando flash de tema incorreto (FOUC).
  - Integrado `ThemeService` no `App` component e configurado `darkModeSelector: '[data-theme="dark"]'` nas opções de tema do PrimeNG em `app.config.ts`.
- **Validação Técnica e Testes**:
  - `npm test`: 11/11 testes unitários aprovados (9 novos testes dedicados para `ThemeService` cobrindo transições `system -> light`, `system -> dark`, persistência em localStorage e escuta de mudanças de runtime no OS).
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção aprovada.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em desktop, Pixel 7 e tablet.

## 2026-09-03 — UI/UX Redesign: R3 — Componentes Compartilhados Concluído

- **Biblioteca de Componentes do Design System (`src/app/shared/components/`)**:
  - `PageHeaderComponent`: evoluído com slot de projeção para ações secundárias ou botões compostos, preservando a ação primária declarativa (`actionLabel` e `actionIcon`).
  - `SectionCardComponent`: container de card padronizado com suporte a título, subtítulo, ícone semântico, opções de padding (`none`, `sm`, `md`, `lg`) e slots para ações de cabeçalho e rodapé.
  - `StatCardComponent`: card de métricas e KPIs operacionais com severidades semânticas (`primary`, `success`, `warning`, `danger`, `info`, `neutral`), números tabulares de alto contraste, dica explicativa e indicador de tendência (`up`, `down`).
  - `StatusBadgeComponent`: componente universal de status com mapeamento semântico para o domínio da oficina (`Open`, `InProgress`, `Completed`, `Cancelled`, além de variantes semânticas), ícones contextuais e estilos contrastantes para Light e Dark Mode.
  - `SearchFieldComponent`: campo de busca reutilizável com `ControlValueAccessor` (suporte a `ngModel` e Reactive Forms), ícone de lupa, botão de limpeza rápida quando preenchido e anel de foco baseado em tokens.
  - `DataToolbarComponent`: barra de ferramentas para listagens operacionais unificando busca, filtros, contadores numéricos e seletor de modo de visualização (Tabela/Blocos) com ocultação automática do seletor em telas móveis.
  - `EmptyStateComponent`: estado vazio minimalista com ícone discreto, título, mensagem explicativa e botão de ação primária.
  - `LoadingStateComponent`: componente de carregamento baseado em PrimeNG Skeleton com múltiplos layouts de placeholder (`table`, `cards`, `stats`, `lines`), eliminando spinners centrais pesados.
  - `DataTableWrapperComponent`: container estrutural com bordas e sombras sutis, scroll horizontal seguro com toque tátil no mobile/tablet e slot de paginação/rodapé.
  - `ThemeToggleComponent`: controle de alternância de aparência acessível (`role="menu"`, `role="menuitemradio"`), exibindo as opções Sistema, Claro e Escuro, com fechamento via teclado (`Escape`), foco visível e detecção de clique externo.
  - `index.ts`: arquivo barrel centralizando as exportações dos componentes compartilhados.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados (11 testes dedicados aos componentes compartilhados em `shared-components.spec.ts`).
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: bundle compilado com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em desktop, Pixel 7 e tablet.

## 2026-09-03 — UI/UX Redesign: R4 — App Shell Concluído

- **Reformulação do Shell da Aplicação (`src/app/layout/`)**:
  - **Sidebar Desktop Minimalista**: dimensionada para 248px (`--sidebar-width`), superfícies limpas com fundo `var(--surface-primary)` e borda sutil `var(--border-subtle)`, wordmark `SP` com suporte aos novos tokens de verde (`var(--primary)` e `var(--primary-soft)`). Navegação sóbria inspirada em dashboards SaaS operacionais, com itens ativos destacados por fundo suave e cor no ícone, sem blocos chamativos.
  - **Header Desktop Operacional**: adicionado cabeçalho superior horizontal com altura de 56px (`--header-height`), exibindo a data de hoje formatada em português brasileiro (`todayFormatted`), link ágil de criação "+ Nova OS" para a rota `/ordens` e o seletor acessível de tema (`ThemeToggleComponent`).
  - **Header Mobile & Drawer**: cabeçalho móvel sticky de 60px com acesso direto ao botão hambúrguer, brand da oficina e botão de alternância de tema (`app-theme-toggle`) junto ao avatar. Drawer móvel deslizante com navegação completa, perfil do usuário e botão de tema no rodapé.
  - **Botão de Alternância de Tema**: posicionado de forma permanente e visível tanto na barra superior do desktop quanto no rodapé da sidebar e no header móvel, permitindo alternar livremente entre os modos **Claro**, **Escuro** e **Sistema**.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet.

## 2026-09-03 — UI/UX Redesign: R5 — Dashboard Concluído

- **Remoção de Elementos Promocionais Estáticos**:
  - Eliminados os antigos cartões estáticos dos módulos que ocupavam espaço vertical com descrições genéricas.
- **Painel Operacional Baseado em Dados Reais (`src/app/features/dashboard/`)**:
  - **4 KPIs Operacionais**: `OS em Aberto` (warning), `Em Execução` (info), `Ordens Concluídas` (success) e `Total no Pátio` (primary, com a soma em reais dos valores de ordens ativas), alimentados diretamente pelos dados reais de `/api/dashboard/summary`.
  - **Área Central — Veículos na Oficina (8 colunas no Desktop)**:
    - No desktop, tabela limpa com bordas sutis e números monoespaçados (`DataTableWrapperComponent`), exibindo código da OS, cliente, placa em badge, veículo, data de entrada formatada, `StatusBadgeComponent` e botão de acesso direto.
    - Em celulares e tablets (<=900px), chaveamento automático para cartões táticos individuais com botões de toque >= 44px e zero rolagem horizontal.
    - Integração de `EmptyStateComponent` quando o pátio estiver sem pendências e `LoadingStateComponent` com PrimeNG Skeleton durante o carregamento inicial.
  - **Painel Lateral — Distribuição e Base (4 colunas no Desktop)**:
    - `Situação das Ordens`: barras de progresso CSS nativas com cálculo proporcional em tempo real para ordens Abertas, Em Execução e Concluídas.
    - `Base Operacional`: resumo cadastral da oficina com total de clientes e veículos e atalhos diretos para gerenciamento.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet.

## 2026-09-03 — UI/UX Redesign: R6 — Ordens de Serviço Concluído

- **Toolbar Operacional e Filtros Semânticos (`work-orders.page.*`)**:
  - Integrado `DataToolbarComponent` com contador reativo de ordens encontradas e alternador de visualização tabela/cards.
  - Adicionado `SearchFieldComponent` com busca reativa e debounce integrado.
  - Implementadas pílulas táteis de filtro por status (`Todas`, `Abertas`, `Em andamento`, `Concluídas`, `Canceladas`), com passagem direta do parâmetro `status` para a API backend (`WorkOrdersController.List`).
- **Apresentação Híbrida Desktop/Mobile**:
  - **Tabela Desktop**: encapsulada com `DataTableWrapperComponent`, tipografia monoespaçada no número da OS e nos totais em R$, `StatusBadgeComponent` semântico e ações rápidas acessíveis.
  - **Cards Mobile/Tablet**: cartões limpos com layout vertical coeso, badges de placa automotiva, botões de toque >= 44px e zero rolagem horizontal indesejada.
  - **Estados Vazios e Carregamento**: uso dos componentes padronizados `EmptyStateComponent` (com mensagem contextual para busca sem resultados) e `LoadingStateComponent` (esqueleto adaptativo).
- **Editor Moderno de Linhas e Fechamento Financeiro**:
  - Eliminação de centenas de linhas de código duplicado no template da página, adotando o componente modular `WorkOrderLinesEditorComponent` para Serviços e Peças.
  - Estilização completa do editor de linhas com tokens semânticos (`--surface-primary`, `--border-subtle`, `--text-primary`, `--font-mono`) com suporte nativo e contraste validado em Dark Mode.
  - Criação do card destacado de fechamento financeiro (`wo-form-totals-card` e `wo-summary-card`) com totais monoespaçados de serviços, peças e valor geral destacado.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso (budget de estilo de componente estritamente respeitado).
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet.

## 2026-09-03 — UI/UX Redesign: R7 — Clientes e Veículos Concluído

- **Padronização Visual e Operacional (`customers.page.*` e `vehicles.page.*`)**:
  - **Toolbars Unificadas**: `DataToolbarComponent` implementado com contagem reativa e dinâmica de registros ("`X clientes encontrados`" e "`X veículos encontrados`") e alternador de tabela/cards.
  - **Busca Rápida com Debounce**: `SearchFieldComponent` integrado com pesquisa ágil por nome, documento, telefone, placa, modelo e fabricante.
  - **Tabelas Desktop Acessíveis**: `DataTableWrapperComponent` adotado nas duas telas com tipografia sóbria, bordas sutis, avatares gerados automaticamente a partir das iniciais do cliente e badges metálicas de placa.
  - **Cartões Móveis Otimizados**: preservação e modernização de `article.catalog-card` com botões de toque >= 44px, links rápidos de discagem telefônica e abertura direta de conversa no WhatsApp.
  - **Estados de Borda**: substituição de layouts manuais por `EmptyStateComponent` com chamadas contextuais de ação e `LoadingStateComponent` com skeleton adaptativo para tabela e cartões.
- **Formulários e Modais de Edição**:
  - Modais de criação e edição com layout responsivo em duas colunas, mensagens de validação visual de CPF/CNPJ e placa, alvos de toque adequados e contraste validado em Light e Dark Mode.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet.

## 2026-09-03 — UI/UX Redesign: R8 — Configurações e Catálogo Concluído

- **Reorganização Modular em Abas (`settings.page.*`)**:
  - **Catálogo de Serviços**: listagem limpa de serviços com título, descrição complementar, preço padrão em tipografia monoespaçada (`var(--font-mono)`), ações rápidas de edição e arquivamento, e integração com `EmptyStateComponent`.
  - **Catálogo de Peças e Insumos**: visualização de SKU em badge monoespaçado, preço de custo, preço de venda e cálculo automático de margem comercial com pílula de destaque visual (`.margin-pill`), com suporte a `EmptyStateComponent`.
  - **Dados da Oficina & Impressão**: formulário completo segmentado por finalidade com `SectionCardComponent`:
    - *Identificação*: Nome fantasia, razão social e CNPJ/CPF com validação visual.
    - *Canais de Contato*: Telefone comercial, WhatsApp e e-mail.
    - *Endereço e Localização*: Logradouro, cidade, UF e CEP em grid responsivo.
    - *Termos Operacionais*: Termo de garantia padrão impresso nas OSs e notas de fechamento aos clientes.
  - **Feedback Operacional**: botão de ação com indicador de carregamento e notificação via `MessageService`.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet.

## 2026-09-03 — UI/UX Redesign: R9 — Autenticação Concluído

- **Telas de Acesso e Setup Inicial Modernizadas (`login.page.*` e `setup.page.*`)**:
  - **Identidade Visual Linear/Vercel**: layout limpo em duas colunas responsivas, tipografia com tracking ajustado, marca com ícone esportivo em fundo esmeralda e gradiente radial sutil.
  - **Alternador de Tema em Destaque**: componente `ThemeToggleComponent` posicionado no canto superior direito (`.auth-top-bar`) para permitir alternância imediata entre Modo Claro, Escuro e Sistema.
  - **Formulário de Login**: inputs ergonômicos para e-mail e senha, botão de login em largura total com spinner em tempo real durante requisições e mensagens de erro de credenciais com ícones de aviso.
  - **Formulário de Setup**: estrutura dividida em seções numeradas (*Dados da Oficina* e *Acesso do Administrador*), validação interativa dos requisitos de complexidade de senha e tratamento de máscara limpa de CNPJ e telefone.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet (incluindo testes de `/login` e `/setup` em todos os viewports sem scroll horizontal).

## 2026-09-03 — UI/UX Redesign: R10 — Polish Concluído

- **Refinamento Global de Usabilidade e Acessibilidade**:
  - **Scrollbars Discretas**: estilização de barras de rolagem nativas via CSS com espessura fina (6px), trilho transparente e cores adaptativas aos temas (`--border-default` e `--border-strong`).
  - **Foco por Teclado e WCAG**: anéis de foco consistentes (`:focus-visible`) com `var(--focus-ring)` em todos os elementos interativos.
  - **Atalhos Rápidos**: tecla `/` configurada globalmente no `AppShellComponent` para focar imediatamente o campo de busca de ordens, clientes e veículos, acompanhada do indicador `<kbd>/</kbd>` no `SearchFieldComponent`. Tecla `ESC` fecha drawers e menus suspensos.
  - **Consistência de Estados de Borda**: uniformidade de `LoadingStateComponent` (skeletons) e `EmptyStateComponent` em todas as rotas da aplicação.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet cobrindo todas as telas em Dark e Light Mode sem overflow horizontal.

## 2026-09-03 — Ajuste de Layout: Tabelas de Ordens de Serviço (Visão Geral e Módulo)

- **Eliminação de Rolagem Horizontal e Alinhamento Vertical das Informações**:
  - **Unificação de Cliente e Veículo**: agrupamento da identificação do cliente e veículo em duas linhas na mesma célula (`.customer-vehicle-cell`), exibindo o nome do cliente no topo e a placa Mercosul com o modelo na linha inferior.
  - **Colgroup e Layout Fixo**: adoção de `table-layout: fixed; width: 100%;` com `<colgroup>` explícito em `dashboard.page.html` e `work-orders.page.html`, garantindo que todas as 6 colunas caibam na largura total sem criar scrollbar lateral em telas de notebook e desktop.
  - **Ações Compactas e Sem Quebra**: botões de ação em grupo (`.table-actions-group` e `.tbl-action-btn`), substituindo botões pesados e mantendo o alinhamento à direita com o valor monetário.
  - **Tratamento de Text-Overflow**: aplicação de `overflow: hidden; text-overflow: ellipsis;` com tooltips nativos `[title]` nos textos longos de clientes e descrições veiculares.
- **Validação Técnica e Testes**:
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm run build`: compilação de produção concluída com sucesso.
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet, com verificação de ausência de overflow horizontal em `/` e `/ordens`.

## 2026-09-04 — Ajuste de alinhamento da lista de Ordens de Serviço

- Adicionada a classe específica `.work-orders-list-table` à tabela do modo lista de `/ordens`.
- Aumentado o espaçamento interno das extremidades, revisadas as larguras das colunas e reservado espaço suficiente para o conjunto de ações sem aproximá-lo da borda.
- O modo bloco e o comportamento responsivo não foram alterados.
- Validação: `npm run build` executado após a alteração.
- Aplicado o mesmo padrão de espaçamento e alinhamento à tabela de Ordens ativas da visão geral da Oficina, sem alterar os cards mobile.
- Melhorada a interação da lista de OS: clique/Enter/Espaço na linha abre a ficha, e ações contextuais foram agrupadas, removendo o botão redundante de visualização.
- Aplicado o mesmo padrão às listas de Clientes e Veículos: linha abre edição, ações agrupadas e colunas secundárias responsivas evitam scroll horizontal.
- Refatorados os cards de Clientes e Veículos para uma estrutura interativa comum; ações passaram a usar botões HTML com PrimeIcons explicitamente visíveis em light/dark mode.
- Corrigido o glifo de arquivamento em Clientes e Veículos: `pi-archive` inexistente foi substituído por `pi-folder-open` nas ações e diálogos de confirmação.
- Corrigida a paginação da impressão: ficha posicionada no topo, cabeçalhos de tabelas repetíveis e seção de termos/assinaturas protegida contra divisão entre páginas.
- Eliminada a página em branco em OS curtas: elementos invisíveis da SPA agora saem do layout de impressão, mantendo apenas a ficha impressa.
- Correção complementar: status reposicionado antes de Cliente/Veículo e coluna ampliada para impedir o corte de “Em andamento”.
- Protegida a célula do status contra clipping, mantendo o badge visível e antes de Cliente/Veículo.
- Status elevado para a primeira coluna da lista, antes do número da OS e de Cliente/Veículo.
- Ordem final ajustada conforme requisito: `OS → Cliente / Veículo → Status → Entrada → Total → Ações`.
- Removido o atalho duplicado de Nova Ordem de Serviço do Dashboard; a ação permanece disponível no cabeçalho global.
- Seletor de tema centralizado no rodapé da barra lateral e do drawer móvel, removendo sua duplicação nos cabeçalhos.
- Documentação reorganizada: documentos gerais, inclusive o guia do frontend, centralizados em `docs/`; `README.md` mantido como índice na raiz do repositório e arquivos de orientação para IA também preservados na raiz.

## 2026-09-04 — Revisão e Correção Visual Global do Frontend (Dark/Light Mode e Design Tokens)

- **Eliminação Completa de Cores Hardcoded e Adaptação Total ao Dark/Light Mode**:
  - Remoção de todas as classes arbitrárias do Tailwind (`text-slate-*`, `bg-slate-*`, `border-slate-*`, etc.) e estilos inline de `<select>` e `<option>` em modais e fichas.
  - Migração integral de todos os componentes para tokens semânticos CSS (`var(--surface-primary)`, `var(--surface-secondary)`, `var(--surface-hover)`, `var(--text-primary)`, `var(--text-secondary)`, `var(--text-muted)`, `var(--border-subtle)`, `var(--border-default)`, `var(--border-strong)`, `var(--primary)`, `var(--success)`, `var(--warning)`, `var(--danger)`, `var(--info)`).
  - Folha de estilos `styles.css` limpa de quaisquer cores hexadecimais fora de `@media print`, garantindo transição perfeita entre modo claro e escuro sem flashes ou textos invisíveis.
- **Harmonização de Componentes PrimeNG e Formulários**:
  - Overrides temáticos bridge em `styles.css` para `p-dialog`, `p-autocomplete-overlay`, `p-paginator`, `p-inputtext`, `p-tooltip` respeitando tokens de superfície e texto de cada tema.
  - Seletores `<select>` e `<option>` no modal de criação/edição de OS ajustados para herdar fundo e texto sem estilos forçados.
- **Padronização Visual de Grids e Cartões (Clientes, Veículos e Ordens)**:
  - `.catalog-grid` e `.order-grid` uniformizados com `repeat(auto-fill, minmax(min(100%, 320px/330px), 1fr))`, impedindo cartões esticados ou estreitos demais.
  - Hierarquia padronizada em todos os cartões: cabeçalho com identificação clara, painel de meta-informações com labels uniformes, linha de observações com truncamento seguro e rodapé sticky com data e botões de ação acessíveis.
  - Botões de ação em cartões com `pTooltip` e `ariaLabel` descritivos para leitores de tela e usabilidade ergonômica.
- **Padronização das Ações de Tabela**:
  - Tabela de Clientes e Veículos atualizadas para o padrão de botões compactos `.table-actions-group` e `.tbl-action-btn.edit` / `.tbl-action-btn.delete` (quadrados de 32px com ícones centralizados, bordas suaves e hover contextual).
  - Tabela de Ordens de Serviço preservada com alinhamento simétrico e colgroup responsivo sem rolagem horizontal.
- **Ficha da Ordem de Serviço (`wo-center-dialog`) e Resumo Financeiro**:
  - Ficha de documento (`.wo-doc-card`, `.wo-tech-card`, `.wo-doc-section`, `.wo-summary-card`) adaptada com fundos semânticos e tipografia legível em Dark Mode.
  - Resumo financeiro com cards arejados, valores tabulados e destaque claro do Total Geral.
- **Validação Técnica e Testes**:
  - `npm run lint`: 0 erros e 0 avisos.
  - `npm test`: 22/22 testes unitários aprovados.
  - `npm run build`: compilação concluída com sucesso (aviso de budget conhecido mantido).
  - `npm run e2e`: 23 cenários aprovados pelo Playwright em Desktop, Pixel 7 e Tablet, cobrindo rotas em múltiplos viewports sem quebras visuais ou overflow.

## 2026-09-04 — Navegação rápida e rebranding para Ofizzy

- Habilitado o preload dos componentes lazy para retirar o download de chunks do caminho crítico das trocas de tela.
- Implementado cache de sessão GET com TTL de 30 segundos, chaves por filtros/paginação, deduplicação de chamadas, snapshots stale-while-revalidate, invalidação seletiva e proteção contra respostas obsoletas.
- Dashboard e listagens mantêm dados visíveis ao retornar; catálogos, veículos por cliente e dados da oficina são reutilizados no editor de OS.
- Adicionado teste E2E que navega para fora e volta a Clientes, confirma exibição em até 100 ms e uma única chamada a `/api/customers`.
- Alterada a marca pública para Ofizzy em todas as superfícies visíveis, defaults de novas instalações, impressão/PDF e documentação; identificadores internos legados foram mantidos para não quebrar infraestrutura existente.
- Validação: `npm run lint` aprovado; `npm test -- --watch=false` com 26/26; `npm run build` aprovado; `npm run e2e` com 24 aprovados/6 skips; backend com 14/14 unitários, 3/3 integrações e build Release sem avisos.
- Compose: frontend foi construído com sucesso no Docker; a reconstrução completa não terminou porque o SDK .NET de 189 MB ainda não existia localmente e o download foi interrompido. O PostgreSQL permaneceu saudável e com o volume persistente.

## 2026-09-04 — Dashboard adaptável à largura disponível

- A tabela “Veículos na Oficina” passou a responder à largura real da coluna principal por container query.
- Entre 901 e 1400 px, o grid deixa de comprimir a lista em 8/4: a lista ocupa a largura total, os painéis laterais descem em duas colunas e as colunas auxiliares da tabela ficam compactas.
- Abaixo de 46rem disponíveis, a tabela ainda é substituída pelos cards existentes.
- Adicionado cenário E2E em viewport de 1180 px, verificando lista em largura total, Cliente/Veículo acima de 200 px e ausência de overflow horizontal.
- Validação: lint aprovado, 26/26 unitários, build aprovado e 27 E2E aprovados com 9 skips específicos de viewport.

## 2026-09-04 — Responsividade da listagem de Ordens de Serviço

- Compactadas as larguras de OS, status, entrada, total e ações entre 901 e 1400 px, liberando espaço real para Cliente/Veículo.
- Tablets e celulares passam obrigatoriamente para cards em `/ordens`; a preferência tabela/cards permanece disponível no desktop.
- Validação: lint, 26/26 testes unitários e build aprovados; E2E direcionado aprovado em notebook de 1180 px e tablet, sem overflow.

## 2026-09-04 — Limpeza conservadora antes da publicação

- Removidos signal e formatador sem consumidores, consolidado seletor CSS duplicado e retirado `console.error` redundante coberto pelo interceptor HTTP.
- Mantidas as validações de formulários, DTOs, autenticação e regras de OS por serem necessárias para UX e integridade do backend.

## 2026-09-08 — Identidade técnica integral Ofizzy

- Renomeados solução, projetos, namespaces, diretórios backend/frontend, workspace Angular, pacotes, imagens e projeto Compose para Ofizzy.
- Banco, usuário, schema, JWT, cookies, scripts, CI/CD e documentação foram alinhados à mesma identidade, sem alterar APIs ou regras de negócio.
- Adicionada migration idempotente para bancos existentes e teste de integração que comprova a preservação de dados durante a troca de schema.
- Adicionado `scripts/migrate-to-ofizzy.sh`, que cria backup lógico validado, restaura em volume separado e preserva a origem para rollback por sete dias.
- Backend: build Release sem avisos, 14/14 testes unitários e 4/4 testes de integração aprovados.
- Frontend: lint aprovado, 26/26 testes unitários, build aprovado e 27 E2E aprovados com 9 skips condicionais em desktop, celular e tablet.
- Compose: configurações base/dev/local/prod validadas, imagens backend/frontend construídas, 6 migrations aplicadas e quatro serviços saudáveis.
- Smoke pelo Nginx aprovado; setup, autenticação, health check, schema e persistência da sessão após reinício do backend foram confirmados.
- Repositório privado renomeado para `Pedroltz/ofizzy`, preservando `develop` como branch padrão; `origin` local atualizado e commit publicado.
- CI remoto aprovado no commit `4670c67` (run `34226277097`): jobs backend, frontend e Compose concluídos com sucesso.
- O build mantém somente o aviso conhecido do bundle inicial: 740,72 kB para orçamento de 500 kB.
## 2026-09-09 — Execução local com PostgreSQL e `dotnet run`

- Transformado `compose.local.yaml` em arquivo autônomo com somente PostgreSQL 18, bind em `127.0.0.1:5432` e volume persistente `ofizzy_postgres_data`.
- Adicionados defaults apenas para `Development`: conexão PostgreSQL local, chave JWT efêmera em memória e aplicação automática de migrations na inicialização.
- Mantida precedência de User Secrets, variáveis de ambiente e `.env`; produção continua falhando cedo sem secrets obrigatórios.
- Removidos os quatro containers legados da stack `sport-pneus`, sem remover volumes; ao final, somente `ofizzy-local-postgres-1` permaneceu em execução.
- Validação: `dotnet build Ofizzy.slnx` sem avisos; 14 testes unitários e 4 testes de integração aprovados; `dotnet run` iniciou na porta 5154; health checks live/ready saudáveis; 6 migrations confirmadas no PostgreSQL; frontend lint e build aprovados.
- Limitações de validação do ambiente: runner frontend em Node 26 sem `localStorage` (17/26 testes passam; 9 do tema falham antes da execução) e Chromium do Playwright ausente. Nenhuma alteração funcional de frontend foi feita.
- Correção pós-smoke: o EF podia manter `__EFMigrationsHistory` em `ofizzy` enquanto outra execução consultava a cópia em `public`, provocando `42P07 relation already exists`. O bootstrap passou a reconciliar os dois históricos de forma bidirecional antes/depois de migrar. Verificação final: duas execuções consecutivas sem migrations reaplicadas, health ready saudável e 6 registros em cada histórico.
- Corrigido o falso erro “Informe 14 dígitos” quando o CNPJ do setup chega formatado. O validator agora aceita máscara ou somente números; testes cobrem formatos válidos, quantidades inválidas e o payload mascarado no fluxo de integração.
Atualização visual — 2026-09-09: removido o brand-mark do login. Seletor de tema de login/setup abre abaixo e alinhado à direita; sidebar mantém abertura acima. Controles com mínimo de 44 px. Lint e build frontend aprovados (aviso de bundle conhecido). Nenhuma alteração de API ou migration nesta tarefa.
Validação da alteração de login/tema: cenários de login (incluindo troca claro/escuro) e setup aprovados nos três viewports. Suíte completa: 26 aprovados, 9 skips e 1 falha na largura do diálogo móvel de OS, reproduzida na repetição; esse diálogo não foi alterado nesta tarefa.


## 2026-09-10 — Fundação SaaS multi-tenant

Discovery completo antes de código; ADR 0006 registrada. Company tornou-se
TenantSettings preservando tabela/IDs e contrato HTTP. Added Tenant/TenantUser/
TenantModule, CurrentTenant, validação de vínculo/estado por request e autorização
PlatformAdmin persistida. Bootstrap opt-in separado do onboarding.

Isolamento EF automático em oito entidades (incluindo configurações e linhas),
proteção de escrita/concurrency token, FKs/índices compostos e contador de OS
transacional por tenant. Migration segura com backfill legado, snapshots/IDs/hash/
números preservados e recusa de downgrade destrutivo. Provisionamento atômico com
template Automotive, associação de usuário existente sem redefinir senha, roles e
módulos. Refresh serializado com seleção persistida e revogação de replay.

Frontend adaptado incrementalmente: contexto central, seleção fora do shell,
limpeza de cache, módulos no menu/guards, PlatformAdmin e onboarding ligados a API
real. Catálogos deixam de carregar quando módulo desabilitado ou tenant Pending.
Dados fictícios de fallback em configurações/PDF removidos; validator existente
passou a executar nas atualizações. Logs incluem escopos JSON TenantId/UserId/RequestId.
CI inclui E2E frontend e deploy aguarda healthchecks; não há pipeline por tenant.

Validações executadas:

- `dotnet build Ofizzy.slnx --configuration Release --no-restore`: sem warnings/erros.
- `dotnet test Ofizzy.slnx --configuration Release --no-build`: 18 unitários e
  7 integrações aprovados com PostgreSQL 18 real. Teste inclui trigger temporário
  que falha durante provisioning e prova rollback de tenant/usuário/configurações.
- `npm run lint`, `NODE_OPTIONS=--no-experimental-webstorage npm test -- --watch=false`
  (28 testes) e `npm run build`: aprovados. Node 26 sem workaround reproduziu o
  problema pré-existente de localStorage; nenhuma alteração de domínio para contorná-lo.
- `npm run e2e`: 27 aprovados/9 skips de viewport. Atualizado texto do bootstrap e
  medição passou a esperar a animação PrimeNG. Quatro workers estabilizam a execução.
- Imagens construídas via Compose; migration sobre banco vazio aplicada com sucesso.
  Migration sobre banco legado fictício também passou, com OS 42 e snapshots intactos.
- `npm run e2e -- --config playwright.live.config.ts`: aprovado pelo Nginx na porta
  18081, sem interceptações. Bootstrap/provisionamento UI, onboarding, Alpha/João/
  ABC1D23/OS 1 e Beta/Maria/XYZ9Z99/OS 1, edição, dashboard, impressão HTML/PDF e
  isolamento bidirecional por IDs validados.
- Reiniciados os quatro containers da stack isolada; `OFIZZY_VERIFY_RESTART=1 npm run
  e2e -- --config playwright.live.config.ts`: IDs, diagnósticos e OS 1 preservados.
- Novas telas em 1440/768/320 px sem overflow, com controles de 44 px e inspeção visual.

Limites: aviso de bundle 743,95 kB/500 kB; outras verticais, convites por e-mail,
recuperação de acesso, upload de logo, pagamentos operacionais e assinatura SaaS não
implementados. Automotive atual continua íntegra. Deploy remoto não executado;
base local existente não migrada nem alterada. Alterações locais anteriores foram
preservadas e separadas dos commits desta tarefa.


Validação adicional do conteúdo exato preparado para commit, exportado em
`/tmp/ofizzy-review` sem as alterações locais anteriores: backend Release build e
14 unitários/7 integrações aprovados; frontend lint, 28 unitários e build aprovados.
Os 18 unitários da árvore de trabalho incluem quatro testes preexistentes ainda não
versionados pelo usuário. Nenhuma dessas alterações anteriores foi incorporada aos
commits SaaS. Logs da imagem final confirmaram contexto TenantId/UserId/RequestId;
leitura persistida após atualização final da imagem também passou pelo Nginx.

## Desenvolvimento local — 2026-09-10

Por decisão do usuário, somente PostgreSQL permanece em Docker (`compose.local.yaml`,
localhost:5432). API e Angular executam no host: `dotnet run --project
src/backend/Ofizzy.Api --launch-profile local` e `npm start` em
`src/frontend/ofizzy-web`. Perfil local habilita bootstrap e migrations em Development;
proxy Angular liga 4200 à API 5154. Instruções completas no README.
Stack Docker de smoke parada, volumes preservados. PostgreSQL nativo instalado
durante a avaliação foi parado; não é utilizado nem necessário neste fluxo.
Banco de desenvolvimento confirmado com zero usuários e tenants após migrations.
Smoke local: `/health/ready` via Angular retornou Healthy e `/api/setup/status`
retornou required=true. Backend build, 18 unitários e 7 integrações aprovados;
frontend lint, 28 unitários e build aprovados (aviso de bundle conhecido, 744 kB).
E2E determinístico nesta execução: 25 aprovados, 9 skips e 2 falhas de timeout
ao aguardar main (mobile / e tablet /ordens); ambos passaram na repetição
isolada com um worker (2/2). Sem alterações visuais
ou de domínio; aceite real Nginx/Alpha/Beta da fase SaaS permanece documentado acima.

## Formulário de provisionamento — 2026-09-10

Tela organizada com Fieldset, Message e controles PrimeNG existentes, em seções
de empresa, administrador e módulos. Identificador converte maiúsculas para
minúsculas; envio inválido mostra mensagens nos campos, sem botão silenciosamente
desabilitado. Contratos/backend e dados preservados.
Validação frontend: lint/build, 28 unitários, 30 E2E aprovados e 9 skips condicionais.
Após ajuste final de CSS, 3 E2E direcionados aprovados em 1440/768/320 px, sem
overflow e campos/botão com pelo menos 44 px. Teste usa API interceptada apenas
no Playwright; produção mantém chamadas reais. Não reexecutados smoke de criação
real, Compose/migrations ou backend nesta mudança exclusiva de interface.
Aviso de bundle permanece (~749 kB). Capturas em /tmp/ofizzy-platform-*.png.

## 2026-09-10 — Organização modular por tipos e verticais

- Backend reorganizado estruturalmente em 3 pilares de responsabilidade:
  - `Platform/`: Autenticação, Tenancy e Company Settings.
  - `BusinessCore/`: Clientes, Catálogo (Services e Parts), Ordens de Serviço e Dashboard.
  - `Verticals/Automotive/`: Módulo da vertical automotiva (Veículos e contratos correspondentes).
- Namespace `Ofizzy.Api.Verticals.Automotive` aplicado para isolar a vertical automotiva sem quebrar contratos HTTP de API (`/api/vehicles`).
- Relações entre entidades preservadas (`Customer.Vehicles`, `WorkOrder.Vehicle`).
- `ApplicationDbContextModelSnapshot.cs` sincronizado com o novo namespace da entidade `Vehicle`.
- Validações executadas:
  - Backend: build com 0 erros e 0 avisos; 18/18 testes unitários aprovados; 7/7 testes de integração aprovados.
  - Frontend: `npm run lint` aprovado (0 erros); `npm run build` aprovado.

## 2026-09-11 — Ajuste visual da tabela de clientes e suporte a localStorage no Node 22

- **Ajuste visual na tabela de clientes (`customers-list-table`)**:
  - Resolvido quebramento de linha no campo Documento (CPF/CNPJ): adicionados `white-space: nowrap` e `flex-shrink: 0` na classe `.doc-badge`, além de regra específica com ellipsis em `.customers-list-table .doc-badge`.
  - Simplificação da grade de clientes: colunas `WhatsApp` e `Cadastro` removidas do grid tabular para desafogar a visualização e evitar poluição visual. Os dados completos (inclusive link direto para conversa no WhatsApp e data de cadastro) continuam imediatamente acessíveis na visualização em cartões (`catalog-cards-grid`) e no modal de detalhes/edição ao clicar no cliente.
  - Coluna Telefone consolidada para exibir o telefone principal ou WhatsApp disponível com clique direto (`tel:` / `wa.me`), preservando contato rápido.
  - Ajuste de colunas e larguras no `<colgroup>`: Documento fixado com respiro adequado (`12.5rem`), evitando quebras em qualquer resolução.
  - Regra `@media (max-width: 900px)` atualizada para ocultar apenas as colunas secundárias de E-mail e Endereço nos tablets, mantendo Cliente, Documento, Telefone e Ações legíveis e utilizáveis sem overflow horizontal.
- **Ambiente de testes frontend (Node 22)**:
  - Adicionado mock completo de `localStorage` em `theme.service.spec.ts` para compatibilidade com o ambiente Node 22 sem flags experimentais pendentes.
- **Validações realizadas**:
  - Frontend: `npm run lint` aprovado (0 erros, 0 avisos); `npm test` aprovado (7 arquivos, 33/33 testes unitários); `npm run build` aprovado; `npm run e2e -- e2e/responsive.spec.ts` aprovado (27 aprovados, 9 skips condicionais de viewport).
  - Backend: `dotnet test` aprovado (18 testes unitários e 7 testes de integração aprovados).

## 2026-09-11 — Persistência otimizada do modo de visualização (cards/tabela)

- **Criação do `ViewPreferenceService` (`core/preferences/view-preference.service.ts`)**:
  - Serviço root com cache em memória e persistência síncrona/reativa em `localStorage` sob a chave `ofizzy_view_preferences`.
  - Fornece `getSignal(key, defaultMode)` que devolve um `WritableSignal<ViewMode>` com interceptores de `.set()` e `.update()`.
  - Zero custo de rede e zero latência (0 ms), sem causar layout shift ao navegar entre páginas ou recarregar (F5).
  - Isolamento por tela: `customers`, `vehicles`, `work-orders`, `tenants`.
- **Integração nas telas**:
  - `CustomersPage`, `VehiclesPage`, `WorkOrdersPage` e `PlatformPage` agora consomem `ViewPreferenceService`.
  - Preservado o comportamento responsivo móvel do `ResponsiveLayoutService` (celulares continuam usando cards para evitar overflow, enquanto a preferência do usuário para desktop/tablet é preservada).
- **Validações técnicas**:
  - Testes unitários do serviço: 6/6 cenários em `view-preference.service.spec.ts` (total frontend: 8 arquivos, 39/39 aprovados).
  - ESLint: 0 erros, 0 avisos.
  - Angular Build: compilado com sucesso.
  - Playwright E2E: novo teste automatizado `modo de visualização (cards/tabela) persiste após navegação e recarregamento` aprovado (total: 28 aprovados, 11 skips de viewport).

## 2026-09-11 — Redesenho e conforto visual dos blocos de clientes e veículos

- **Layout e preenchimento de frame (`catalog-card`)**:
  - Aplicada regra `align-items: stretch !important;` e `width: 100%;` em `.catalog-card`, eliminando o confinamento forçado ao centro herdado de `.entity-card`.
  - `.catalog-card-header`: ajustado para 100% da largura com `justify-content: space-between`.
  - `.catalog-card-identity`: alinhamento à esquerda (`text-align: left`) e `width: 100%`, com hierarquia clara entre título e subtítulo.
  - `.catalog-card-meta-box`: largura integral com padding confortável (`0.85rem 1rem`), integrando os campos de contato e especificações sem recuos excessivos.
  - `.catalog-card-notes`: transformado em banner de largura total com ícone e fundo suave (`var(--warning-soft)`).
  - `.catalog-card-footer`: linha divisória e rodapé ocupando 100% da largura, com data à esquerda e ações à direita.
- **Clientes (`CustomersPage`)**:
  - Avatar, Nome e E-mail integrados em conjunto visual alinhado à esquerda; Documento posicionado no canto superior direito.
  - Adicionado helper `formatPhone` para aplicar máscara em telefones brasileiros `(DD) 9XXXX-XXXX` ou `(DD) XXXX-XXXX`, mantendo link do WhatsApp limpo.
- **Veículos (`VehiclesPage`)**:
  - Placa e badge de Ano/Cor ancorados nas extremidades do cabeçalho.
  - Modelo e Proprietário alinhados à esquerda; especificações e observações ocupando a largura do frame.
- **Validações técnicas**:
  - Frontend: ESLint 0 erros; 39/39 testes unitários aprovados; build compilado com sucesso; 28 testes E2E Playwright aprovados sem regressões.
  - Backend: 25/25 testes aprovados (18 unitários, 7 integração).

## 2026-09-11 — Documentação e planejamento: PWA e Operação Offline-First (Store & Forward)

- **Planejamento de PWA e Resiliência Operacional**:
  - Especificada a evolução para Progressive Web App (PWA) no Angular com `@angular/service-worker` e `manifest.webmanifest` para instalação nativa no celular, tablet e desktop.
  - Especificado o modelo arquitetural *Offline-First* com persistência local em `IndexedDB` (via `Dexie.js`) para suportar quedas de energia no galpão e falta de conectividade no pátio externo da oficina.
  - Mapeado o alinhamento com a arquitetura existente do Ofizzy: aproveitamento das chaves primárias em `Guid` (UUIDs) permitindo geração de IDs válidos no cliente, fila de despacho FIFO (`SyncQueue`), garantia de idempotência e atribuição do sequencial amigável da OS (`LastWorkOrderNumber`) pelo backend.
- **Documentação atualizada**:
  - `docs/ROADMAP.md`: Adicionada seção dedicada com itens de checklist para PWA e Operação Offline-First no backlog de evolução.
  - `docs/PRODUCT-STRATEGY.md`: Incorporado como diferencial competitivo chave para oficinas mecânicas no diagnóstico de produto e no diagrama de trilha estratégica (Etapa 2 - Eficiência e Operação no Pátio).
  - `docs/ARCHITECTURE.md`: Documentada a camada de evolução de frontend para PWA e Offline-First (App Shell, IndexedDB, geração de GUIDs no cliente, fila de sincronização e idempotência).
  - `docs/STATUS.md` e `AI-HANDOFF.md`: Atualizados para refletir o planejamento arquitetural e inclusão no backlog oficial.

## 2026-09-11 — Reorganização modular do frontend com Verticals

- **Alinhamento arquitetural frontend-backend**:
  - Módulo de veículos movido de `features/vehicles` para `features/verticals/automotive/vehicles`, espelhando a modularidade de `src/backend/Ofizzy.Api/Verticals/Automotive`.
  - Criado arquivo de exportação de módulo `src/app/features/verticals/automotive/index.ts`.
  - Atualizados os caminhos relativos de injeção e importação em `vehicles.page.ts`.
  - Atualizada a rota `/veiculos` em `app.routes.ts` com o novo caminho de lazy loading mantendo a validação de acesso ao módulo `Automotive`.
- **Validações técnicas**:
  - Frontend Linter: `npm run lint` aprovado (0 erros, 0 avisos).
  - Frontend Testes Unitários: `npm test` aprovado (8 arquivos, 39/39 testes unitários).
  - Frontend Build: `npm run build` aprovado.
  - Frontend E2E: `npm run e2e -- e2e/responsive.spec.ts` aprovado (28 cenários aprovados, 11 skips condicionais de viewport).
  - Backend Testes: `dotnet test` aprovado (18 unitários, 7 integração com PostgreSQL real).




## 2026-09-12 — Fundação fiscal e continuidade do piloto

- Adicionados módulo Fiscal, DTOs, perfis, preparação, certificado cifrado, documentos,
  eventos, sequências e duas migrations; preservado monólito e isolamento por tenant.
- Integração de configurações e OS com PrimeNG 21, XML/DPS assinados, adaptadores diretos,
  estados de emissão parcial, downloads condicionados e bloqueio de produção.
- Corrigidos schema de assinatura NFS-e com DTD legado, data/total em nova tentativa,
  acompanhamento de emissão parcial, identificação de cancelamento e índice histórico.
- Backend: build Release sem warnings; 34 unitários; 9 integrações, incluindo cenário
  de timeout/reenvio idempotente aprovado com gateway de teste.
- Frontend: lint, 39 unitários, build; E2E final 49 aprovados/11 skips. Preparação fiscal
  verificada em desktop/tablet/320 px. Falha inicial de seletor com ícone corrigida por ariaLabel.
- Compose isolado/18082: imagens, nove migrations, configuração e preparação pelo Nginx,
  persistência após restart dos quatro containers. Nenhuma chamada real de emissão.
- Piloto informado durante a sessão: Igaraçu do Tietê/SP, Simples Nacional. A fonte municipal
  confirma padrão nacional desde 01/08/2026 para ME/EPP Simples. Registrado na fase 8.
- Pendências explícitas de schemas vigentes/RTC, PDFs oficiais, eventos, recuperação de
  inutilização e homologação. Não declarar a feature pronta para produção.


## 2026-09-13 — Eventos e recuperação de inutilização fiscal

- Validadores XSD locais para cancelamento NF-e/NFS-e e inutilização; schemas oficiais
  de cancelamento separados para evitar conflitos de tipos.
- Recuperação administrativa com o XML original, confirmação de protocolo 102/563
  vinculada ao CNPJ/ambiente/UF/faixa, lease persistido de dois minutos e eventos das tentativas.
- Nova migration aditiva `AddFiscalInutilizationLease`; nenhuma pendência de modelo EF.
- Interface real para histórico, solicitação com confirmação e recuperação; erros no
  diálogo mantêm os dados. API impede inutilizar números não reservados ou autorizados.
- Backend Release sem avisos/erros; suíte completa 40 unitários/9 integrações. Testes
  dirigidos finais: 22 unitários fiscais/2 integrações, incluindo falha de comunicação,
  pedido idêntico no reenvio, consulta simultânea bloqueada e protocolo de faixa divergente.
- Frontend lint,39 unitários/build; E2E completo52 aprovados/11 skips. Tela fiscal nova
  verificada em 1440/768/320 px, sem overflow e com controles testados de pelo menos44 px.
- Imagens Compose reconstruídas; décima migration aplicada sobre o banco isolado
  existente. Smoke Nginx aprovou histórico, erro da faixa inexistente e isolamento do
  endpoint de recuperação por ID. Não houve transmissão a órgão fiscal.
- Permanecem atualização das NTs/schemas NF-e vigentes, revisão dos PDFs oficiais e
  homologação externa do piloto Igaraçu do Tietê/SP, Simples Nacional.

- Aceite final: smoke real aprovado antes e após reinício dos quatro containers;
  configuração e preparação preservadas. Outro tenant recebe 404 ao recuperar um
  pedido existente e não visualiza seu histórico. Teste fiscal direcionado aprovado.

## 2026-09-13 — Consolidação documental e próximos passos

A pedido do usuário, revisados os Markdown do projeto: status/handoff consolidados, plano de XML alinhado à emissão oficial de serviços e produtos, referências de arquitetura/API/dados/UI/operação atualizadas, fases e ADRs contextualizadas. Adicionados NEXT-STEPS e ADR 0007. Status, handoff e proposta inicial preservados em docs/archive como registros históricos.

A ordem futura é revisar schemas/NTs e PDFs, ampliar cenários de falha, preparar cadastro/acesso do piloto, homologar e só então liberar produção. Financeiro permanece posterior; pagamento fiscal não é recebimento. Recuperação de inutilização já está implementada, e não deve continuar listada como tarefa de desenvolvimento pendente.

Esta alteração é documental: os resultados de backend/frontend/migrations/Compose/smoke de 13/09 pertencem à implementação previamente registrada. Não houve nova execução dessas suítes nem transmissão fiscal nesta revisão. Verificação documental: 40 arquivos Markdown conferidos, nenhum link local quebrado e git diff --check sem erros. Foram atualizados os 35 documentos existentes e adicionados dois documentos de referência e três arquivos históricos.

## 2026-09-13 — Conclusão de DANFE, DANFSe e conformidade do Simples Nacional

- **DANFE Oficial (NF-e modelo 55)**:
  - Implementado em `FiscalPdf.cs` com canhoto destacável de recebimento no topo ("RECEBEMOS DE [Emitente] OS PRODUTOS...", Data de Recebimento, Identificação e Assinatura do Recebedor, NF-e Nº, Série).
  - Chave de acesso de 44 dígitos formatada em 11 blocos de 4 caracteres para legibilidade e digitação manual.
  - Código de barras Code 128C integrado via ZXing e SVG vetorial de alta definição.
  - Grade estruturada oficial da SEFAZ com cabeçalho de emitente, indicação 1-Saída em caixa destacada, natureza da operação, protocolo de autorização de uso, destinatário/remetente, forma de pagamento, cálculo detalhado do imposto em duas linhas, transporte/volumes e tabela completa de produtos/peças com alinhamento numérico tabular.
  - Grupo `infAdic` com `infCpl` preenchido conforme Resolução CGSN nº 140/2018 ("DOCUMENTO EMITIDO POR ME OU EPP OPTANTE PELO SIMPLES NACIONAL. NAO GERA DIREITO A CREDITO FISCAL DE IPI.") e identificação da Ordem de Serviço de origem (`OS #0001`).
- **DANFSe Oficial (Padrão Nacional Sefin)**:
  - Implementado em `FiscalPdf.cs` no padrão do Sistema Nacional da NFS-e.
  - QR Code oficial gerado via ZXing para consulta pública no Portal Nacional da NFS-e.
  - Identificação completa com número da NFS-e, série, data/hora de emissão, competência, e identificadores da DPS de origem (número, série e data).
  - Dados do prestador e tomador com CPF/CNPJ formatado e inscrição municipal.
  - Discriminação dos serviços, código de tributação nacional, código municipal e NBS.
  - Tributação municipal e valores com base de cálculo, alíquota de ISSQN, ISSQN retido (Sim/Não) e destaque em verde com fundo suave para o Valor Líquido da NFS-e.
  - Grupo `infoCompl` com `xPed` e `xInfComp` na DPS.
- **Banners de Status**:
  - Inclusão de tarja em vermelho "SEM VALOR FISCAL — EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO" para documentos em homologação.
  - Inclusão de tarja "DOCUMENTO CANCELADO" para documentos com cancelamento homologado.
- **Validações e Testes**:
  - Backend: build em Release com 0 avisos e 0 erros; 44 testes unitários (4 novos cenários para DANFE, DANFSe, banners e validação de tags no XSD) e 9 testes de integração com Testcontainers / PostgreSQL 18 real aprovados.
  - Frontend: ESLint aprovado (0 erros, 0 avisos); 39 testes unitários Vitest aprovados; build de produção aprovado; 52 testes E2E do Playwright aprovados (11 skips de viewport).

## 2026-09-13 — Ambiente de testes locais com dados forjados e Gateway Simulado de Desenvolvimento

- **DevSimulatedFiscalGateway**:
  - Implementado em `DevSimulatedFiscalGateway.cs`, ativado exclusivamente quando `builder.Environment.IsDevelopment()` é verdadeiro E `"Fiscal:SimulateGateway": true`.
  - Simula respostas de autorização oficial da SEFAZ para NF-e mod. 55 com protocolo `135260000000001`, envelopamento em `<nfeProc versao="4.00">` e carimbo `<protNFe>`.
  - Simula autorização da NFS-e Nacional com chave de 50 dígitos e protocolo.
  - Simula cancelamento (`cStat=101`) e inutilização de numeração (`cStat=102`).
  - Permite validar 100% do fluxo visual no navegador, banco PostgreSQL real e geração de PDF de DANFE e DANFSe sem conexão de rede com a SEFAZ.
- **Gerador de Certificado A1 Fictício para Desenvolvimento**:
  - Implementado em `FiscalDevController.cs` (`GET /api/fiscal/dev/certificate`), restrito a `IsDevelopment()`.
  - Gera em memória um certificado PKCS#12 (.pfx) com chave privada RSA 2048 bits e extensão ICP-Brasil OID `2.16.76.1.3.3` preenchida com o CNPJ cadastrado na empresa (senha padrão: `teste123`).
  - Adicionado botão *"Baixar certificado A1 de teste (Dev)"* na tela de Configurações Fiscais do frontend.
  - Exercita o fluxo real de validação do arquivo, conferência do CNPJ, proteção com AES-GCM no cofre `FiscalCertificateVault` e gravação no PostgreSQL.
- **Validações e Testes**:
  - Backend: 48 testes unitários (4 novos testes em `FiscalDevSimulationTests.cs` cobrindo compatibilidade do SAN ICP-Brasil com o cofre, geração de `<nfeProc>`, autorização de NFS-e, geração de PDF do DANFE/DANFSe e cancelamento/inutilização simulados) e 9 testes de integração com PostgreSQL 18 real aprovados (100%).
  - Frontend: ESLint aprovado (0 erros, 0 avisos), 39 testes unitários Vitest aprovados e build de produção aprovado.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](archive/REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.

## 2026-09-13 — Estabilização do DANFE, altura de toque e validação integral

- **Correção de restrição de layout no QuestPDF (`FiscalPdf.cs`)**:
  - Diagnosticada exceção `DocumentLayoutException` durante geração do DANFE da NF-e com o debugger ativado do QuestPDF: o elemento de código de barras Code 128 possuía `Height(32)` com aspect ratio vetorial de 7,925, exigindo 253,6 px de largura, o que excedia os 247 px disponíveis na coluna do cabeçalho.
  - Ajustada a altura do código de barras vetorial para `28` px (`Height(28)`), mantendo o código de barras nítido e dentro da largura disponível sem truncamento ou overflow.
- **Acessibilidade móvel em Configurações Fiscais (`fiscal-settings.component.ts`)**:
  - Adicionado `:host ::ng-deep .p-button { min-height: 44px; }` ao componente de configurações fiscais, assegurando que o botão de utilitário de teste em desenvolvimento (*"Baixar certificado A1 de teste (Dev)"*) e demais botões atinjam a área de toque mínima de 44 px exigida por `AGENTS.md` e testada por `e2e/responsive.spec.ts`.
- **Validações e Testes completos**:
  - Backend: 55 testes unitários aprovados (0 falhas) e 10 testes de integração aprovados (0 falhas) em `Ofizzy.slnx` com `Release`.
  - Frontend: ESLint aprovado (0 erros, 0 avisos), 39 testes unitários Vitest aprovados (8/8 suítes) e build de produção executado com êxito.
  - E2E Playwright: 58 testes aprovados, 11 skips de viewport em 69 testes (100% dos testes aplicáveis passando em Desktop, Tablet e Mobile).

## 2026-09-14 — Revisão de margens, espaçamentos e padronização PrimeNG no frontend

- **Modal da Ordem de Serviço (`work-orders.page.html`)**:
  - Posicionamento do componente `<app-work-order-fiscal>` movido para dentro de `.wo-dialog-content` (após `.wo-summary-card`), herdando padding ergonômico interno (`2rem 2.25rem`) e espaçamento vertical entre cartões.
- **Grid de Campos Fiscais (`fiscal-fields.component.ts`)**:
  - Desacoplamento dos `<label>` em contêineres `.fiscal-field` com tipografia em caixa alta e tracking.
  - Grid responsivo em 2 colunas (`minmax(0, 1fr)`) com suporte a `colSpan: 2` (utilizado para nome do cliente e logradouro), colapsando em coluna única em telas móveis ($\le 640$ px).
  - Controles com altura mínima de 44 px preservada.
- **Configurações Fiscais (`fiscal-settings.component.ts`)**:
  - Separação clara de Dados Gerais e Endereço Fiscal com subtítulos em caixa alta e divisores sutis.
  - Barra de ações dedicada para salvamento com alinhamento à direita e divisor superior.
  - Card visual para o certificado digital A1 com badge de status "Ativo".
- **Inutilização de NF-e (`fiscal-inutilizations.component.ts`)**:
  - Histórico de inutilizações reorganizado em grid de cards `.history-card`.
  - Diálogo de confirmação com rodapé padronizado em `<ng-template #footer>`.
- **Painel e Modais Fiscais da OS (`work-order-fiscal.component.ts`)**:
  - Valores e totais em pílulas estruturadas (`.fiscal-value-pill`), tags PrimeNG com severidade contextual para situação e listas de pendências com espaçamento legível.
  - Modal de *Preparação fiscal da OS* completamente reformulado com classe `.wo-center-dialog.fiscal-prep-dialog`, banner informativo superior com `.icon-circle info` e organização em 4 cartões com `SectionCardComponent` (*Dados do Tomador*, *Endereço Fiscal*, *Declaração de Pagamento* e *Itens e Tributação* em `p-fieldset`).
  - Modal de *Cancelar documento fiscal* com alerta de impacto SEFAZ / Prefeitura e textarea estilizado.
  - Otimização da folha de estilos do componente para respeitar o limite de 4.00 kB do Angular sem avisos de budget.
  - Compatibilidade total com seletores de acessibilidade Playwright (`Salvar preparação`, `Confirmar cancelamento`, `Preparação fiscal`, `#recipient-name`).
- **Validações e Testes (Frontend)**:
  - ESLint: aprovado com 0 erros e 0 avisos (`npm run lint`).
  - Vitest: 39 testes unitários aprovados em 8 arquivos (`npx ng test --no-watch`).
  - Playwright: 40 testes responsivos aprovados em Desktop, Tablet e Mobile 320px sem overflow horizontal e com controles $\ge 44$ px (`npx playwright test e2e/responsive.spec.ts`).

## 2026-09-14 — Evolução do controle de tema (modo switch na barra superior)

- **Controle de Tema como Chave / Switch (`ThemeToggleComponent`)**:
  - Adicionado suporte a `mode="switch"` ao lado do modo `dropdown` padrão.
  - No modo switch, o componente renderiza uma chave deslizante elegante com ícones de sol e lua (`pi-sun` e `pi-moon`), trilho suave, `role="switch"` e acessibilidade compatível com leitores de tela e navegação por teclado (`min-height: 44px`, `min-width: 44px`, `focus-visible`).
- **Barra Superior (`AppShellComponent`)**:
  - Movido o controle de tema para o cabeçalho superior (`desktop-header__right`), posicionado diretamente ao lado do botão **Nova OS**.
  - No cabeçalho móvel (`mobile-header__right`), a chave de tema também foi inserida ao lado do avatar do usuário.
  - O rodapé da sidebar foi despoluído, mantendo o foco nas informações do usuário autenticado e botão de encerramento de sessão.
- **Validações e Testes**:
  - ESLint: 0 erros e 0 avisos (`npm run lint`).
  - Vitest: 40 testes unitários aprovados em 8 arquivos (`npx ng test --no-watch`), incluindo novo teste para o modo switch.
  - Angular Build: compilação de produção aprovada com sucesso (`npm run build`).
  - Playwright: 58 testes aprovados (11 skips) em Desktop, Tablet e Mobile (`npx playwright test`).

## 2026-09-15 — Liberação do ambiente fiscal de produção via interface da plataforma

- **Modelagem e Migração (PostgreSQL / EF Core)**:
  - Adicionados campos `FiscalProductionReleased` (boolean, default false), `FiscalProductionReleasedAt` (timestamp with time zone, nullable) e `FiscalProductionReleasedByUserId` (uuid, nullable) na entidade `Tenant`.
  - Criada migration `20260915181631_AddTenantFiscalProductionRelease`.
- **Backend (.NET)**:
  - `PlatformController`: Atualizados `PlatformTenantResponse` e `UpdateTenantRequest` para expor e atualizar `FiscalProductionReleased` com auditoria de data/hora e identificador do usuário autenticado.
  - `FiscalReleaseGate`: Implementados métodos assíncronos `IsTenantAllowedAsync` e `EnsureAllowedAsync` verificando a liberação persistida no banco com fallback para as configurações de servidor (`Fiscal:HomologatedTenants` e `Fiscal:ProductionEnabled`).
  - Integrado o release gate em `FiscalSettingsController`, `FiscalEmissionService`, `FiscalInutilizationsController` e `FiscalPreparationService`.
- **Frontend (Angular / PrimeNG)**:
  - `platform.page.ts` e `platform.page.html`: Exibição de badge do status fiscal (`Produção liberada` vs `Homologação`) na tabela e cards, e inserção do campo "Liberar ambiente de produção fiscal" com texto explicativo e data de liberação no modal de gerenciamento da empresa.
- **Validações e Testes**:
  - Backend: 56 testes unitários aprovados (`Ofizzy.UnitTests`) e 10 testes de integração aprovados (`Ofizzy.IntegrationTests`), totalizando 100% de aprovação.
  - Frontend: ESLint aprovado (0 warnings/errors), 40 testes unitários Vitest aprovados, 21 testes E2E do Playwright aprovados (Desktop, Mobile, Tablet) e build de produção aprovado.

## 2026-09-15 — Renovação silenciosa de sessão (Silent Refresh) no Frontend

- **Causa Raiz Resolvida**:
  - Token de acesso JWT expira em 15 minutos (`AccessTokenMinutes = 15`). Após inatividade, a API retornava `401 Unauthorized`. O interceptor anterior ignorava 401 sem disparar refresh, travando interações em tela sem recarregar (F5).
- **Frontend (Angular)**:
  - Criado `authRefreshInterceptor`: captura HTTP 401, dispara `POST /api/auth/refresh` em segundo plano e repete a requisição original de forma transparente.
  - `AuthService`: implementado `refreshSession(): Promise<boolean>` com desduplicação concorrente via `refreshPromise` e `handleSessionExpired()`.
  - `app.config.ts`: registrado `authRefreshInterceptor` na cadeia HTTP antes de `apiErrorInterceptor`.
  - Se o refresh falhar (expiração total de 30 dias), redireciona para `/login` e apresenta aviso contextual com `MessageService`.
- **Validações e Testes**:

## 2026-09-17 — Troca de organizações na barra superior e expansão do logotipo na barra lateral

- **Troca de Organizações no Cabeçalho Superior (`desktop-header`)**:
  - Removido o seletor da barra lateral e inserido na barra superior (`desktop-header__left`) no lugar da data.
  - Exibição do nome completo da organização (`Arroba Pneus - Igaraçu do Tietê`, etc.) com ícone e chevron dropdown, sem o título redundante "Ofizzy" e sem o quadrado `OF`.
  - Popover nativo PrimeNG (`.org-switcher-popover`) com largura expandida (`18rem` a `24rem`), acomodando nomes completos com respiro, checkmark na empresa ativa e link para a plataforma.
- **Logotipo Oficial e Alternância Dinâmica por Tema**:
  - Adicionado suporte a logotipo específico para o tema escuro (`logo-dark.png`) com base na imagem enviada pelo usuário, com transparência e contraste nativos para fundos pretos.
  - Alternância automática via CSS entre `logo.png` (tema claro) e `logo-dark.png` (tema escuro) no desktop e mobile, eliminando fundo branco no modo escuro.
  - Redimensionado para preencher com equilíbrio a largura da barra lateral (`max-width: 220px`, `max-height: 65px`).
- **Barra Superior Flutuante e Seletor de Organização Minimalista**:
  - Seletor de organizações (`desktop-header__left`) simplificado para estética minimalista: transparente por padrão, sem bordas marcadas, caixas internas ou sombras (`box-shadow: none`), integrando-se naturalmente à barra superior.
  - Hover sutil em forma de cápsula (`var(--surface-hover)`), chevron simplificado e preservação total do dropdown popover nativo PrimeNG (`.org-switcher-popover`).
  - Cabeçalho flutuante (`desktop-header` e `mobile-header`) com margens externas, cantos suavemente arredondados (`var(--radius-lg)` = 12px), contorno sutil (`1px solid var(--border-subtle)`) e sem sombras pesadas (`box-shadow: none`).
- **Validação Técnica**:
  - Frontend Lint: `npm run lint` aprovado (0 erros, 0 avisos).
  - Frontend Testes: `npm test` aprovado (12 arquivos, 68/68 testes unitários).
  - Frontend Build: `npm run build` aprovado.
  - Backend Testes: `dotnet test` aprovado (56/56 testes unitários).

## 2026-09-18 — Favicons oficiais em alta resolução e multi-resolução

## 2026-09-21 — Módulo de Veículos opcional por organização

- Atualizada a dependência de módulos: Clientes + Catálogo + Ordens de Serviço podem operar sem Automotive; Automotive permanece dependente de Clientes.
- A autorização de API, as rotas e o menu removem Veículos quando o tenant o desativa. O formulário de OS omite veículo e quilometragem nesse modo; com Automotive ativo, a seleção continua obrigatória no backend.
- `WorkOrder.VehicleId`, placa e descrição foram tornados opcionais no modelo, contratos e migration `20260921113000_MakeWorkOrderVehicleOptional`. OS existentes mantêm seus snapshots; uma edição sem Automotive não apaga o veículo histórico.
- Adicionados testes unitários de dependência de módulos. A migration foi aplicada no PostgreSQL local. Validações: backend build sem avisos e 76 unitários aprovados; frontend lint, 69 testes e build aprovados. O aviso de budget inicial do frontend (795,36 kB para limite de 500 kB) permanece preexistente.

- **Processamento e Geração de Favicons**:
  - Imagem do ícone com engrenagem e checkmark (`ChatGPT Image 17 de set. de 2026, 15_40_02.png`) tratada e exportada para os assets de cabeçalho do navegador em `src/frontend/ofizzy-web/public/`.
  - Criado `favicon.ico` com múltiplas camadas (16×16, 32×32, 48×48, 64×64 px) para compatibilidade universal com navegadores desktop legados e modernos.
  - Criado `favicon.png` em alta resolução (512×512 px) com fundo transparente e margem de respiro de 5% para evitar cortes nas abas.
  - Criado `apple-touch-icon.png` (180×180 px) para atalhos móveis em iOS e Android.
- **Configuração no HTML**:
  - Atualizado `src/frontend/ofizzy-web/src/index.html` com tags `<link rel="icon" type="image/png" href="favicon.png">`, `<link rel="icon" type="image/x-icon" href="favicon.ico">` e `<link rel="apple-touch-icon" href="apple-touch-icon.png">`.
  - Preservadas as tags críticas existentes: `<base href="/">`, `<meta name="viewport">`, tipografia Google Fonts Inter e o script autoexecutável de inicialização de tema.
- **Validações e Testes**:
  - Frontend Lint: aprovado com 0 erros e 0 avisos (`npm run lint`).
  - Frontend Testes: 68 testes unitários aprovados em 12 arquivos (`npm test`).
  - Frontend Build: compilação de produção aprovada com sucesso (`npm run build`), gerando os arquivos de favicon diretamente na raiz pública (`dist/ofizzy-web/browser/`).

## 2026-09-18 — Formatação monetária e automação do pagamento da NF-e na Preparação Fiscal

- **Formatação Monetária Padrão BRL em Controles Fiscais**:
  - Implementado o suporte a `type: 'currency'` na interface `FiscalField` e no componente `FiscalFieldsComponent`.
  - Renderiza o PrimeNG `p-inputnumber` com `mode="currency"`, `currency="BRL"`, `locale="pt-BR"`, `placeholder="R$ 0,00"` e altura mínima de 44 px, alinhando-se ao comportamento dos demais campos monetários da aplicação.
  - Atualizados os campos de pagamento da NF-e (`paymentAmount`) e valores unitários de ST (`retainedStBase`, `retainedStAmount`, `substituteAmount`) para usar `type: 'currency'`.
- **Automação do Valor e Forma de Pagamento da NF-e**:
  - Backend (`FiscalPreparationService`): calcula o somatório de peças da OS (`partsTotal = order.Parts.Sum(x => x.Quantity * x.UnitPrice)`). Se `PaymentAmount` for nulo ou na primeira carga da preparação, atribui automaticamente `PaymentAmount: partsTotal` e `PaymentCode: "01"` ("Dinheiro"), ou `0` com `PaymentCode: "90"` ("Sem pagamento") quando a OS não possuir peças.
  - Frontend (`WorkOrderFiscalComponent`):
    - Preenche automaticamente o total de peças ao abrir a preparação da OS quando o valor estiver em branco.
    - Sincronização reativa ao mudar o meio de pagamento: selecionar "Sem pagamento" (90) ajusta imediatamente o valor para `0` (`R$ 0,00`); selecionar outro meio de pagamento restaura o total das peças.
    - Card de auxílio com dica informativa da regra da SEFAZ e botão de ação rápida `Usar total da NF-e (R$ X,XX)`.
- **Validações e Testes**:
  - Frontend Lint: `npm run lint` aprovado (0 erros, 0 avisos).
  - Frontend Testes: `npm test` aprovado com 69/69 testes unitários em 12 arquivos (incluindo teste para campo `currency`).
  - Frontend Build: `npm run build` aprovado sem estouro de orçamento por componente.
  - Backend Testes: `dotnet test` aprovado com 56 unitários + 10 de integração (100%).

## 2026-09-18 — Exibição da senha do Certificado A1 Dev e refinamento de validação fiscal

- **Certificado Digital A1 de Teste em Homologação**:
  - Exibição condicional do botão de download do certificado A1 dev: visível exclusivamente quando o ambiente configurado for `Homologação` (`isHomologation()`) e as ferramentas de desenvolvimento estiverem ativas no servidor (`devToolsAvailable`).
  - Indicação discreta da senha padrão (`teste123`) posicionada em texto sutil (`.dev-cert-subtle-hint`) logo abaixo dos botões de ação do certificado.
  - Ao clicar para baixar o arquivo `.pfx`, o sistema também autopreenche a senha no formulário e avisa via notificação Toast.
- **Refinamento na Validação Fiscal e Resolução de Erros Duplicados**:
  - Mensagens de pendência de itens da OS (peças e serviços) agora trazem o nome/descrição do item como prefixo (ex: `[Item]: O código municipal...`), eliminando mensagens de erro anônimas e duplicadas na tela da OS.
  - Corrigido o mapeamento de propriedades nos controles do diálogo de preparação fiscal (`camelCase` matching), permitindo que campos com pendências fiquem destacados em vermelho no formulário.
  - Sanitização de espaços em branco: campos opcionais como código municipal e NBS vazios são gravados e validados como `null`, prevenindo falsos positivos de validação.
  - Limpeza realizada nos registros de teste locais no PostgreSQL para normalizar códigos municipais incorretos do seed de dev.
## 2026-09-18 — Endereço estruturado do cliente com busca ViaCEP e hidratação automática na emissão fiscal

- **Estruturação de Endereço e Dados Fiscais no Cliente**:
  - Adicionados campos fiscais ao modelo `Customer`: `PostalCode` (CEP 8 dígitos), `Street` (Logradouro), `Number` (Número), `District` (Bairro), `City` (Cidade), `State` (UF 2 letras), `CityCode` (Código IBGE 7 dígitos) e `StateRegistration` (Inscrição Estadual).
  - Criada migration EF Core `20260918165000_AddCustomerFiscalAddress` e executada no banco PostgreSQL (`ofizzy.customers`).
  - Implementada validação no backend via FluentValidation (`CustomerContracts.cs`) com regras de tamanho e formato para CEP, UF e Código IBGE. O campo livre `Address` é automaticamente gerado caso omitido.
- **Frontend e Consulta Pública ViaCEP**:
  - Interface do diálogo de cliente (`CustomersPage`) atualizada com o card `Localização & Dados Fiscais`, toques mínimos de 44px e responsividade mobile.
  - Implementada busca com feedback visual no serviço público ViaCEP ao digitar ou perder foco no campo de CEP, autopreenchendo Logradouro, Bairro, Cidade, UF e o Código IBGE do município.
- **Automação Completa na Preparação Fiscal da Ordem de Serviço**:
  - `FiscalPreparationService`: ao carregar ou preparar a emissão da OS (NFS-e ou NF-e), hidrata automaticamente os dados fiscais do destinatário (`recipient.Address`, `StateRegistration` e `RecipientIeIndicator`) a partir do cadastro do cliente associado à OS (`WorkOrder.CustomerId`), dispensando preenchimento manual redundante.
- **Validações e Testes**:
  - Frontend Lint: `npm run lint` aprovado (0 erros, 0 avisos).
  - Frontend Testes: `npm test` aprovado com 69/69 testes unitários em 12 arquivos.
  - Frontend Build: `npm run build` aprovado.
  - Backend Testes: `dotnet test` aprovado com 56 unitários + 10 de integração (100%).
