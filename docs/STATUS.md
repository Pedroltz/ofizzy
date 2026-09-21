# Status do projeto

## Migração do XML NF-e para PL_010f v1.04 — 21/09/2026

- Os cinco XSDs oficiais do `PL_010f_v1.04` foram instalados lado a lado em
  `BusinessCore/Fiscal/Schemas/Nfe010f`, com SHA-256 do ZIP e hashes individuais
  protegidos por teste. Novas NF-e selecionam explicitamente esse pacote; o acervo
  010c não foi alterado nem reinterpretado.
- O XML atualmente suportado (revenda interna do Simples Nacional) foi assinado e
  validado contra o 010f. O algoritmo XMLDSig permanece SHA-1 porque essa é a
  definição do pacote oficial e dos envelopes de evento em uso.
- A distribuição oficial de eventos RTC v1.40 foi instalada em diretório separado.
  Ela não substitui cancelamento `110111` nem inutilização 4.00; ambos continuam nos
  schemas oficiais específicos. Nenhum evento RTC sem cenário fiscal aprovado foi
  habilitado.
- O bloqueio técnico de leiaute NF-e no painel de prontidão foi removido. Permanecem
  obrigatórias as confirmações externas de credenciamento e a classificação RTC por
  operação junto à contabilidade; produção continua desligada.
- Evidências: 73 testes unitários e 10 de integração do backend aprovados; build
  Release sem avisos; lint, 69 testes e build do frontend aprovados; 24 E2E da
  plataforma aprovados em desktop, mobile e tablet. `docker compose config` foi
  validado com variáveis fictícias; nenhuma migration nova foi gerada nem aplicada.

## Regras fiscais com vigência por item — 20/09/2026

- Produtos e serviços agora podem ter revisões de classificação com uma data de início
  de vigência. A preparação da OS seleciona a revisão mais recente aplicável à data de
  emissão; XMLs e documentos já emitidos mantêm o snapshot original.
- A migration `20260920180236_AddFiscalProfileEffectiveDate` preserva os perfis
  existentes com vigência histórica em 01/01/2000 e substitui a unicidade de um perfil
  por item por unicidade de item + vigência. A tela de Configurações Fiscais permite
  criar a próxima revisão sem sobrescrever a regra corrente.
- Isto torna os valores configuráveis por tenant e data, mas não ativa tags RTC nem
  PL_010f sem modelo/XML/schema homologados.
- Evidências: script EF idempotente conferido; backend 72/72, lint, 69/69 testes e
  build frontend aprovados. Migration e smoke Nginx passaram em Compose descartável
  na porta 18087; stack e volume removidos ao final.

## Painel de prontidão para homologação — 20/09/2026

- Adicionado `GET /api/fiscal/homologation-readiness` e o painel **Prontidão para
  homologação** nas Configurações Fiscais. Ele confere dados fiscais, ambiente,
  proteção do A1, validade registrada do certificado e os pacotes de schemas ativos.
- O painel separa explicitamente pendências externas (credenciamento, IE/IM/série e
  classificações da contabilidade) de bloqueios técnicos. Ele bloqueia NF-e habilitada
  enquanto o leiaute ativo for PL_010c, sem confundir a preparação local com aprovação
  do órgão fiscal; NFS-e Nacional 1.01 segue marcada como pacote conferido.
- Evidências: 72 testes unitários backend, lint frontend, 69 testes frontend e build
  aprovados. A versão atual passou migration e smoke pelo Nginx em Compose descartável
  na porta 18085; a stack e o volume foram removidos. Produção não foi habilitada nem
  houve chamada a autorizadores.

## Auditoria regulatória fiscal e segurança de assinatura — 19/09/2026

- Produzida matriz de fontes oficiais em `docs/fiscal/FISCAL-REGULATORY-AUDIT-2026.md`.
  Ela documenta a defasagem do baseline NF-e 010c frente ao 010e vigente, RTC,
  CNPJ alfanumérico, IBS/CBS, NFS-e, eventos, inutilização e endpoints SP.
- Conferido o pacote oficial NFS-e produtivo `v1.01-20260209`: os dez XSDs locais são
  idênticos ao pacote (SHA-256 do ZIP registrado). O pacote de produção restrita RTC
  20260727 é diferente e não foi ativado sem modelagem e validação.
- Adicionado teste de integridade de assinatura: alterar XML após assinar faz a
  verificação falhar. A auditoria identificou que os schemas NF-e/eventos atuais
  fixam SHA-1; não foi feita troca incompatível para SHA-256 sem pacote sucessor.
- Criados `HOMOLOGATION-RUNBOOK.md` e `PRODUCTION-READINESS.md`. Não houve chamada
  a autorizadores, alteração de `Fiscal:ProductionEnabled`, allow-list ou
  `FiscalProductionReleased`.
- Validação executada: 58 testes unitários backend aprovados, incluindo 28 testes
  fiscais (integridade dos XSDs NFS-e e adulteração de assinatura), e 69 testes
  frontend aprovados. O smoke fiscal pelo Nginx passou antes e depois do restart:
  configuração/preparação persistidas, emissão simulada XML/PDF e cancelamento NFS-e.
  O ambiente foi `Development` isolado, com dados fictícios; não é homologação externa.
  Produção continua pendente.

## Pacotes NF-e 010e/010f — obtenção e comparação oficial — 20/09/2026

- Com o cookie exigido pelo Portal, foram obtidos diretamente os ZIPs 010e e 010f e
  conferidos seus SHA-256. O Portal passou a indicar `PL_010f_v1.04` como atual e o
  010e como histórico; os dois diferem do baseline 010c e entre si em XSDs RTC/IBS-CBS.
  Nenhum XSD ativo foi alterado: a migração parte do 010f e exige adaptação completa.

## Defesa do documento auxiliar e A1 em uso — 20/09/2026

- DANFE e DANFSe agora derivam o ambiente do XML autorizado e recusam XML de espécie
  diferente da persistida. O DANFSe não usa mais fallback de CNPJ ou IM do snapshot.
- A abertura do A1 revalida validade, RSA privado, key usage quando informado e CNPJ
  da configuração fiscal antes de emissão, cancelamento ou inutilização. Isso mantém
  a criptografia AES-GCM existente e evita o uso de certificado substituído/expirado.
- Validação local: 64 testes unitários backend aprovados; build Release sem avisos ou
  erros e `git diff --check` limpo. Nenhum endpoint externo ou gate de produção mudou.

## CNPJ alfanumérico preparado para migração 010f — 20/09/2026

- Implementado o cálculo oficial de DV do CNPJ alfanumérico em capacidade isolada,
  com o exemplo da Receita Federal e casos inválidos cobertos. Os validadores, XML e
  chave de acesso ativos permanecem numéricos sob o pacote NF-e 010c.
- Validação local: 69 testes unitários backend aprovados; build Release sem avisos ou
  erros e `git diff --check` limpo. Produção permanece bloqueada.

## Consulta oficial de eventos NFS-e — 20/09/2026

- A recuperação de uma NFS-e passou a consultar o histórico completo no ADN pela rota
  oficial de produção restrita `GET /NFSe/{ChaveAcesso}/Eventos`, sem supor um código
  de cancelamento na URL. A rota ADN de produção foi bloqueada até ser obtida fonte
  oficial específica; a emissão produtiva não foi liberada.
- Validação local: 70 testes unitários backend aprovados; build Release sem avisos ou
  erros e `git diff --check` limpo.

## Matriz de domínio RTC — 20/09/2026

- Documentada a aplicabilidade de CNPJ alfanumérico, IBS/CBS, monofásico, crédito,
  estorno, IS e NFS-e para o perfil piloto. Campos condicionais dependem de
  classificação e cálculos fornecidos pela contabilidade; não serão inferidos por NCM
  nem emitidos vazios.

## DANFE derivado do XML autorizado — 20/09/2026

- A geração de DANFE passou a obter chave de acesso e série exclusivamente do XML
  autorizado. Um `AccessKey` ou uma série divergente no registro local não consegue
  alterar o PDF; ausência desses campos no XML bloqueia a geração com mensagem clara.
- Teste visual específico aprovado: 6 testes fiscais de PDF. A suíte unitária completa
  passou com 58 testes e o build Release terminou sem avisos ou erros. Nenhuma emissão
  externa ou configuração de produção foi alterada.

## Refatoração abrangente de legibilidade, enriquecimento fiscal e dados de teste — 19/09/2026

- **Refatoração de Legibilidade, Organização e Padronização de Código**:
  - Eliminação de múltiplas instruções em linha única em camadas críticas do backend (`SetupController`, `SessionService`, `PlatformController`, `TenantProvisioningService`, `OnboardingController`, `TenantContextMiddleware`, `TenantAccessFilter`, `PlatformAuthorizationHandler`, `GlobalExceptionHandler` e `Program.cs`).
  - Records e DTOs reformatados verticalmente com indentação canônica de 4 espaços (`VehicleContracts.cs`, `PlatformController.cs`, etc.).
  - FluentValidation: quebra individualizada por regra (`.NotEmpty()`, `.MaximumLength()`, `.Matches()`) em validadores de plataforma, multitenancy e veículos.
  - Testes E2E (`session.spec.ts`): eliminação de condicionais aglutinadas em linha única e padronização semântica de asserções.
  - Zero alterações em contratos públicos, regras de negócio ou banco de dados.
- **Enriquecimento e Semente de Dados com Validação Fiscal Total**:
  - Cadastrados novos registros reais em ambos os tenants piloto (`Arroba Pneus - Igaraçu do Tietê` e `Arroba Pneus - Barra Bonita`):
    - Clientes PF com CPFs matematicamente válidos pela Receita Federal (módulo 11) e PJ com CNPJs e Inscrições Estaduais válidas.
    - Endereços completos com código IBGE de 7 dígitos (`3520004` e `3505302`) compatíveis com a UF São Paulo (`SP`).
    - Veículos reais com placas válidas (padrão Mercosul), ano, modelo, KM e chassis.
    - Ordens de Serviço concluídas com peças e serviços vinculados aos respectivos perfis fiscais (NCM, CEST, CFOP, CSOSN, alíquotas de ST anterior e Código Nacional de NFS-e `140101`).
    - Preparações fiscais (`fiscal_preparations`) persistidas e validadas, prontas para emissão de NF-e e NFS-e.
- **Validação Automatizada**:
  - Backend: 56 testes unitários aprovados (`dotnet test src/backend/Ofizzy.UnitTests/Ofizzy.UnitTests.csproj`).
  - Frontend: compilação e build de produção aprovados (`npm --prefix src/frontend/ofizzy-web run build`).


## Layout "Fit to Screen" em todas as telas paginadas e dashboard — 18/09/2026

- **Enquadramento Perfeito de Tela (Fit to Screen, Widescreen & Dinâmico)**:
  - Densidade e Capacidade de Linhas por Página (`pageSize = 18`):
    - O número de linhas/itens por página foi expandido de 12 para 18 em todas as telas paginadas (Ordens de Serviço, Clientes, Veículos e Catálogo de Peças/Serviços), preenchendo a tela com dados reais e aproveitando a verticalidade antes de quebrar para a próxima página.
    - O espaçamento natural e compacto das linhas da tabela foi mantido (`padding: 0.5rem 0.75rem`), garantindo visual limpo sem esticar forçadamente a altura dos textos.
    - O paginador mantém sincronia precisa com o índice inicial (`[first]="(page() - 1) * pageSize"`).
  - Container principal (`.app-main-area`): configurado com `height: 100vh; height: 100dvh; max-height: 100dvh; overflow: hidden;` no desktop, contendo o cabeçalho superior e a área de trabalho sem extrapolar a tela.
  - Área de conteúdo (`.app-content`): configurada com `flex: 1 1 auto; min-height: 0; overflow-y: auto; display: flex; flex-direction: column;`, permitindo que apenas o conteúdo interno role suavemente se a resolução for muito baixa, mantendo o cabeçalho sempre visível.
  - Estrutura de páginas (`.page-container`): definida com `flex: 1 1 auto; min-height: 0; display: flex; flex-direction: column;`, garantindo que todo o conjunto de cabeçalho, filtros, tabela e paginação caiba confortavelmente em telas 1080p, 768p e notebooks sem rolagem desnecessária.
  - Wrapper da tabela (`app-data-table-wrapper` / `.data-table-wrapper`): configurado como flex container `flex: 1 1 auto; min-height: 0; display: flex; flex-direction: column;`, com área de rolagem interna (`.data-table-wrapper__scroll`) suportando rolagem vertical e horizontal contida, impedindo scroll na página externa.
  - Cabeçalho de tabela fixo (`.data-table th`): `position: sticky; top: 0; z-index: 2;` mantendo as colunas sempre visíveis caso a tabela precise rolar internamente.
  - Barra de ferramentas de dados (`app-data-toolbar`): espaçamento inferior reduzido para `var(--space-3)` e `flex-shrink: 0`.
  - Tabelas de dados (`.data-table` / `.work-orders-list-table`): compactação de padding vertical de células (`0.5rem 0.75rem`), proporcionando densidade de dados limpa e profissional com mais informações visíveis ao mesmo tempo.
  - Paginador PrimeNG (`.p-paginator`): padding compacto (`0.35rem 0.6rem`), botões de página enxutos (`height: 2rem; min-width: 2rem;`), margem superior reduzida e `flex-shrink: 0`, fixando-o na base da tela sem exigir rolagem para encontrá-lo.
  - Grids de cartões (`.catalog-grid`, `.order-grid`): configurados com `flex: 1 1 auto; min-height: 0; overflow-y: auto;` e gaps/paddings otimizados (`1rem` e `var(--space-3)`).
  - Painel / Dashboard (`.dashboard-page-container`): redução de espaçamentos verticais e gaps dos KPIs e painéis para `var(--space-3)` e `var(--space-4)`.
  - Responsividade móvel preservada: em telas <= 900px, a estrutura flexibiliza automaticamente para `height: auto` e `max-height: none` com toques mínimos >= 44px e sem estouro horizontal.
- **Posicionamento Unificado das Ações Primárias (Nova OS, Novo Cliente, Novo Veículo)**:
  - Os botões de criação ("Nova Ordem", "Novo Cliente", "Novo Veículo") foram movidos do cabeçalho de página (`app-page-header`) para a barra de ferramentas de dados (`app-data-toolbar` no slot `toolbar-actions`).
  - Isso alinha o botão principal diretamente à altura dos filtros, pesquisa e visualizações (tabela/cards), garantindo uma hierarquia visual limpa e ergonômica sem sobrecarregar o topo da página.
- **Exibição Dinâmica de Endereço em Clientes**:
  - Implementado o método formatador `formatAddress(item: Customer)` em `CustomersPage`, compondo e exibindo inteligentemente o endereço tanto na listagem em tabela quanto nos cards:
    - Se o cliente possuir `address` composto preenchido, exibe-o diretamente.
    - Se o cliente possuir os campos fiscais estruturados (`street`, `number`, `district`, `city`, `state`, `postalCode`), formata dinamicamente a linha completa (`Rua, Número, Bairro, Cidade - UF, CEP XXXXX-XXX`), eliminando os travessões vazios (`—`) na lista.
- **Validação e Homologação Fiscal dos Exemplos Locais**:
  - Ajustados os perfis fiscais de serviços (`fiscal_services`): código de tributação nacional padronizado com 6 dígitos (`140101`) e NBS/Código Municipal compatíveis.
  - Ajustados os perfis fiscais de peças e produtos (`fiscal_products`): preenchidos os dados de substituição tributária (ST anterior) retida (`RetainedStBase`, `RetainedStAmount`, `SubstituteAmount`, `StRate`) exigidos para operações com CSOSN 500 / CFOP 5405.
  - Sincronizado certificado A1 de homologação dev (`Oficina Teste Dev`) e configuração de regime Simples Nacional para ambas as organizações cadastradas.
  - Ordens finalizadas agora carregam sem nenhuma pendência impeditiva, permitindo emissão imediata de NF-e e NFS-e pelo gateway simulado de homologação.
- **Validação**: Frontend lint (0 erros/avisos), 69/69 testes unitários aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários + 10 testes de integração aprovados (100%).


## Robustez do catálogo de serviços e peças com busca, paginação e visualizações (tabela/cards) — 18/09/2026

- **Busca e Paginação no Catálogo**:
  - `CatalogApiService` e `SessionDataCacheService` atualizados com suporte a paginação (`page`, `pageSize`) e consulta (`q`), integrando com os endpoints backend já paginados.
  - Implementado `app-search-field` com debounce de 250ms nas abas de "Mão de Obra e Serviços" e "Peças e Insumos" da página de Ajustes (`settings.page`).
  - Implementado `app-data-toolbar` com contador dinâmico de itens, busca integrada e alternância de visualização (`table` vs `cards`).
  - Adicionado componente `p-paginator` PrimeNG com navegação por páginas, `rowsPerPageOptions: [12, 24, 48]` e persistência de estado reativa.
  - Adicionados estados vazios (`app-empty-state`) e de carregamento (`app-loading-state`).
- **Modos de Exibição Responsivos (Tabela / Grid de Cards)**:
  - Visualização em tabela rica com PrimeNG `p-table`, badges de status, formatação monetária e botões de ação rápida (editar, configurar perfil fiscal, arquivar).
  - Visualização em cards responsivos para mobile e tablet com área de toque mínima >= 44px, badges contextuais e botões táteis.
- **Padronização de Indentação e Organização do Código**:
  - Backend (.NET): remoção de linhas comprimidas com múltiplas instruções/ponto-e-vírgula e if inline no backend (`FiscalController.cs`, `WorkOrdersController.cs`), criação do `.editorconfig` oficial na raiz e execução do `dotnet format whitespace`.
  - Frontend (Angular): adição de scripts npm `"format"` e `"format:check"` e formatação via Prettier em todo o projeto web.
- **Validação**: Frontend lint (0 erros/avisos), 69 testes unitários Vitest aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários e 10 testes de integração aprovados (100%).


## Endereço estruturado do cliente com busca ViaCEP e hidratação automática na emissão fiscal — 18/09/2026

- **Endereço Estruturado e Dados Fiscais no Cliente**:
  - Modelo `Customer` expandido com campos fiscais dedicados: CEP (`PostalCode`), Logradouro (`Street`), Número (`Number`), Bairro (`District`), Cidade (`City`), UF (`State`, 2 letras), Código IBGE do Município (`CityCode`, 7 dígitos) e Inscrição Estadual (`StateRegistration`).
  - Migration do Entity Framework Core criada (`20260918165000_AddCustomerFiscalAddress`) e aplicada no PostgreSQL (`ofizzy.customers`).
  - Validações robustas no backend via FluentValidation (`CustomerContracts`): CEP com 8 dígitos, UF com 2 caracteres, Código IBGE com 7 dígitos numéricos e limites adequados de caracteres.
  - O campo de texto livre legado `Address` é automaticamente composto ou mantido como complemento (`Street, Number - District, City - State (PostalCode)`).
- **Interface e Integração com ViaCEP no Frontend**:
  - Diálogo de criação/edição do cliente atualizado com o card visual `Localização & Dados Fiscais` estilizado com cantos arredondados, contrastes suaves e toques táteis >= 44px.
  - Botão de busca e autopreenchimento por CEP conectado diretamente ao serviço público ViaCEP: ao informar o CEP (ou perder o foco), preenche automaticamente Logradouro, Bairro, Cidade, UF e o Código IBGE de 7 dígitos do município.
- **Automação Completa na Emissão Fiscal da Ordem de Serviço**:
  - No `FiscalPreparationService`: ao abrir ou preparar uma Ordem de Serviço para emissão (NFS-e ou NF-e), o sistema consulta o cliente associado (`WorkOrder.CustomerId`). Se os dados do destinatário estiverem em branco ou incompletos, hidrata automaticamente o endereço fiscal estruturado (`recipient.Address`), a Inscrição Estadual (`StateRegistration`) e o indicador de IE (`RecipientIeIndicator: "1"` se PJ com IE, `"9"` se não contribuinte), eliminando digitação duplicada e retrabalho.
- **Validação**: Frontend lint (0 erros/avisos), 69 testes unitários aprovados em 12 arquivos, build de produção concluído com sucesso; Backend com 56 testes unitários e 10 testes de integração aprovados (100%).

## Exibição da senha do Certificado A1 Dev e refinamento de validação fiscal — 18/09/2026

- **Certificado Digital A1 de Teste em Homologação**:
  - Exibição condicional do botão de download do certificado A1 dev: visível exclusivamente quando o ambiente configurado for `Homologação` (`isHomologation()`) e as ferramentas de desenvolvimento estiverem ativas no servidor (`devToolsAvailable`).
  - Indicação discreta da senha padrão (`teste123`) posicionada em texto sutil (`.dev-cert-subtle-hint`) logo abaixo dos botões de ação do certificado.
  - Ao clicar para baixar o arquivo `.pfx`, o sistema também autopreenche a senha no formulário e avisa via notificação Toast.
- **Refinamento na Validação Fiscal e Resolução de Erros Duplicados**:
  - Mensagens de pendência de itens da OS (peças e serviços) agora trazem o nome/descrição do item como prefixo (ex: `[Item]: O código municipal...`), eliminando mensagens de erro anônimas e duplicadas na tela da OS.
  - Corrigido o mapeamento de propriedades nos controles do diálogo de preparação fiscal (`camelCase` matching), permitindo que campos com pendências fiquem destacados em vermelho no formulário.
  - Sanitização de espaços em branco: campos opcionais como código municipal e NBS vazios são gravados e validados como `null`, prevenindo falsos positivos de validação.
  - Limpeza realizada nos registros de teste locais no PostgreSQL para normalizar códigos municipais incorretos do seed de dev.
- **Validação**: Frontend lint (0 erros/avisos), 69 testes unitários aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários e 10 testes de integração aprovados (100%).

## Formatação monetária e automação do pagamento da NF-e na Preparação Fiscal — 18/09/2026

- **Formatação Monetária Padrão BRL**:
  - Adicionado suporte nativo ao tipo `currency` em `FiscalField` e `FiscalFieldsComponent`, renderizando `p-inputnumber` com `mode="currency"`, `currency="BRL"`, `locale="pt-BR"`, `placeholder="R$ 0,00"`, alinhado ao padrão de moeda do restante da aplicação (Ordem de Serviço e Catálogo).
  - O campo `paymentAmount` ("Valor do pagamento da NF-e") e os campos unitários de ST (`retainedStBase`, `retainedStAmount`, `substituteAmount`) foram atualizados para `type: 'currency'`.
- **Automação Inteligente a partir dos Itens da OS**:
  - No backend (`FiscalPreparationService`): na carga inicial da preparação ou caso não haja valor salvo, o sistema calcula automaticamente o total de peças (`partsTotal = order.Parts.Sum(x => x.Quantity * x.UnitPrice)`) e define o valor padrão de `PaymentAmount` e `PaymentCode: "01"` ("Dinheiro") para OS mista/produtos, ou `"90"` ("Sem pagamento") com valor `0` caso não haja produtos.
  - No frontend (`WorkOrderFiscalComponent`):
    - Ao abrir a preparação, preenche automaticamente o valor exato dos produtos da OS caso esteja nulo ou vazio.
    - Sincronização reativa ao alterar o meio de pagamento: ao selecionar "Sem pagamento" (90), o valor é automaticamente zerado (`R$ 0,00`); ao retornar para um meio de pagamento válido, restaura o total de produtos.
    - Card de auxílio com destaque informativo e botão de 1 clique (`Usar total da NF-e (R$ X,XX)`) para sincronização instantânea em caso de divergência.
- **Validação**: Frontend lint (0 erros/avisos), 69 unitários Vitest aprovados em 12 arquivos (incluindo novo teste para campo `currency`), build de produção aprovado e sem estouro de orçamento por componente; Backend com 56 unitários + 10 testes de integração aprovados (100%).

## Atualização dos favicons oficiais do sistema — 18/09/2026

- **Favicons em Alta Resolução e Multi-Resolução**:
  - Processada a imagem oficial fornecida pelo usuário (`ChatGPT Image 17 de set. de 2026, 15_40_02.png`), contendo o símbolo da engrenagem com checkmark em fundo transparente.
  - Gerados e configurados em `src/frontend/ofizzy-web/public/`:
    - `favicon.ico`: arquivo multi-resolução com camadas de 16×16, 32×32, 48×48 e 64×64 px para compatibilidade completa com todos os navegadores desktop legados e modernos.
    - `favicon.png`: ícone moderno em PNG transparente de alta definição (512×512 px) com margem de segurança para evitar cortes circulares/quadrados nas abas.
    - `apple-touch-icon.png`: ícone de atalho móvel em 180×180 px para dispositivos iOS e atalhos na tela de início.
  - Atualizado `index.html` com tags `<link rel="icon">` e `<link rel="apple-touch-icon">`, preservando `<base href="/">`, metatags responsivas, tipografia Inter e script de tema.
- **Validação**: Frontend lint (0 erros/avisos), 68/68 unitários aprovados em 12 arquivos, build de produção concluído com sucesso e assets devidamente distribuídos em `dist/ofizzy-web/browser/`.

## Troca de organizações na barra superior e expansão do logotipo na barra lateral — 17/09/2026

- **Troca de Organizações no Cabeçalho Superior Minimalista**:
  - Botão seletor na barra superior (`desktop-header__left`) simplificado com abordagem minimalista: fundo e bordas transparentes por padrão, sem sombras ou caixa em relevo, integrado diretamente à barra flutuante.
  - Efeito suave ao passar o mouse (`background: var(--surface-hover)` e cantos arredondados `var(--radius-md)`), chevron simplificado sem caixa de fundo, e nome completo da organização ativa (`Arroba Pneus - Igaraçu do Tietê`, etc.).
  - Popover nativo PrimeNG (`.org-switcher-popover`) preservado integralmente para a troca de empresa.
- **Logotipo Oficial na Barra Lateral e Alternância por Tema**:
  - Adicionado suporte ao logotipo dedicado para o tema escuro (`logo-dark.png`) com base na imagem transparente oficial fornecida, calibrado para as dimensões exatas de 2048 × 682 px da logo clara.
  - Alternância fluida via CSS entre `logo.png` (tema claro) e `logo-dark.png` (tema escuro) no desktop e mobile, garantindo visual limpo e sem fundo branco indesejado.
- **Barra Superior Flutuante Minimalista (`desktop-header` e `mobile-header`)**:
  - Barra superior flutuante com margens de respiração (`var(--space-3) var(--space-6) 0`), cantos suavemente arredondados (`var(--radius-lg)` = 12px), contorno fino completo (`1px solid var(--border-subtle)`) e sem sombras pesadas (`box-shadow: none`).
  - Vidro translúcido com desfoque de fundo (`color-mix(in srgb, var(--surface-primary) 92%, transparent); backdrop-filter: blur(12px)`), flutuando fixa a 12px do topo com rolagem fluida de conteúdo por trás.
- **Validação**: Frontend lint (0 erros), 68 unitários aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários aprovados.

## Validação visual com destaque em vermelho na área fiscal — 17/09/2026

Implementado destaque visual em vermelho para campos obrigatórios ou com erro em todos os formulários fiscais da aplicação (Catálogo de Peças/Serviços, Configurações Fiscais da Empresa e Preparação Fiscal da Ordem de Serviço):
- Centralizado em `FiscalFieldsComponent` e `fiscalForm`: os campos obrigatórios exibem indicador asterisco vermelho `*` no rótulo e, quando inválidos e tocados, recebem a classe `.has-error`, borda vermelha (`var(--danger)`), anel de foco suave avermelhado (`var(--danger-soft)`), rótulo em vermelho e mensagem de erro explicativa abaixo do controle (`<small class="fiscal-error-msg"><i class="pi pi-exclamation-circle"></i> ...</small>`).
- Nos diálogos de Catálogo (`SettingsPage`): `savePart()` e `saveService()` validam NCM, regras de CFOP/CSOSN, ST e código de tributação nacional, marcando os campos com erro e executando `markAllAsTouched()` para que o campo fique vermelho imediatamente junto à notificação Toast.
- Em Configurações Fiscais (`FiscalSettingsComponent`) e Preparação da OS (`WorkOrderFiscalComponent`): validações de formulário, senha de certificado e mapeamento reativo de pendências retornadas pelo backend (`issues`) acendem os campos exatos em vermelho.
- Evidências: `npm run lint` 0 erros/avisos; `npm test` 68 testes unitários aprovados em 12 arquivos (incluindo novos testes em `fiscal-fields.component.spec.ts` e `settings.page.spec.ts`); `npm run build` aprovado; `dotnet test` 56 testes unitários backend aprovados.

## Configuração fiscal integrada no catálogo e dropdown no topo — 17/09/2026

Integrada a classificação fiscal (NCM, CEST, CFOP, CSOSN, CST PIS/COFINS para produtos; código de tributação nacional para serviços) diretamente nos diálogos de cadastro e edição de peças e serviços na página de Catálogo e Ajustes (`SettingsPage`). O recurso é acionado via slider/toggle switch (`p-toggleswitch`) com card explicativo para administradores, preenchimento automático de padrões sugeridos do Simples Nacional (CFOP 5102, CSOSN 102, UN, SEM GTIN, etc.) e ativação automática com dados carregados ao editar itens que já possuam perfil fiscal.

Substituído o item intermediário "Trocar organização" do menu lateral por um dropdown nativo com PrimeNG `p-popover` posicionado diretamente na marca `Ofizzy` (desktop sidebar e mobile header/drawer). Se o usuário possuir apenas 1 organização vinculada, o componente não exibe dropdown nem chevron, atuando como apresentação da empresa ou link simples para o início. Ao possuir 2 ou mais, exibe o chevron indicador e abre o menu suspenso para troca instantânea com feedback visual e toast. O design foi refinado para proporções slim (14.75rem), eliminando pesos visuais excessivos e garantindo alto contraste. Além disso, a tipografia global do sistema foi atualizada para a família `Inter` via Google Fonts e tokens CSS, unificando a legibilidade da barra lateral, controles e componentes.

Validação: backend Release sem erros/avisos (56 unitários + 10 integrações); frontend lint (0 avisos), 62 unitários Vitest aprovados (incluindo 6 novos testes da integração fiscal do catálogo); build de produção concluído; Playwright E2E aprovado em Desktop, Tablet e Mobile sem overflow horizontal.

## Correção de sessão — 16/09/2026

Corrigida a renovação após expirar o cookie de acesso: o frontend atualiza o XSRF antes/depois do refresh e repete escritas com o token atualizado. Somente 401 encerra a sessão; falhas temporárias não exibem falso aviso de expiração. A tela de login não restaura automaticamente uma sessão já invalidada nesta aba, e requisições concorrentes compartilham a renovação e um único aviso.

Validação desta correção: backend Release sem avisos/erros, 56 unitários e 10 integrações; frontend lint, 51 unitários, build; 61 E2E existentes aprovados/11 skips e 9 regressões de sessão aprovadas em desktop/mobile/tablet. Smoke real pelo Nginx na porta 18083 aprovado antes e depois do reinício dos quatro containers, incluindo escrita persistida, reprodução do XSRF antigo com 400, renovação com 200 e permanência no login após refresh inválido. Migration runner confirmou banco atualizado; nenhuma migration nova. Bundle inicial mantém aviso de 783,12 kB / budget 500 kB. Detalhes no log de implementação. Sem publicação em produção.

Atualizado em 20/09/2026. Branch de trabalho: `develop`.

A fase ativa é a [fase 8 — Fiscal](phases/PHASE-08-FISCAL.md). A implementação local inclui emissão, consulta, cancelamento, downloads e recuperação administrativa de inutilização. **Não há homologação externa nem liberação de emissão em produção.**

| Frente | Estado |
| --- | --- |
| Fases 1–4: fundação, cadastros, OS e impressão | Concluídas localmente |
| Fase 7: SaaS e isolamento por organização | Aceite local concluído; arquitetura vigente |
| Fase 8: serviços e produtos fiscais | Fluxos locais implementados; revisão fiscal e homologação pendentes |
| Fase 5: financeiro | Não iniciada; após a prioridade fiscal |
| Fase 6: acabamento | Parcial; backup/restauração e demais critérios continuam pendentes |
| PWA, estoque, outras verticais e cobrança SaaS | Backlog, sem implementação nesta entrega |

## Entrega fiscal atual

Piloto confirmado: **Igaraçu do Tietê/SP, IBGE 3520004, Simples Nacional**. Integração direta com NFS-e Nacional para serviços e NF-e SP modelo 55 para peças/pneus, sem intermediário pago. A OS mista gera documentos separados e mostra autorização parcial.

Configurações, perfis, preparação, certificado A1 cifrado, sequências, documentos e eventos persistem por tenant. Cada documento novo também grava o pacote de schemas que o validou (`NF-e PL_010c` ou `NFS-e Nacional 1.01`); a migration identifica documentos legados pelo respectivo leiaute histórico. A interface usa PrimeNG 21 nas configurações e na OS concluída. Inutilização tem histórico, confirmação, recuperação com XML original, validação do protocolo e lease persistido de dois minutos. Cancelamentos e inutilizações passam por XSD antes do envio.

## Rastreabilidade de schemas fiscais — 20/09/2026

- Criada a migration `20260920070200_AddFiscalDocumentSchemaPackage`: registra o pacote de schemas em `fiscal_documents`, com backfill determinístico (`NF-e PL_010c` para NF-e e `NFS-e Nacional 1.01` para NFS-e existentes).
- A API expõe `schemaPackage` no resumo do documento e novas emissões preenchem o valor pelo catálogo central de schemas. Assim, uma migração futura para o PL_010f não reinterpreta XMLs já emitidos sob o baseline 010c.
- Evidências locais: migration gerada pelo EF Core em Release; `dotnet test` backend 71/71; lint frontend sem erros; testes frontend 69/69; build frontend aprovado. O bundle inicial continua em 794,07 kB, acima do budget de 500 kB (aviso preexistente, sem bloqueio de build).
- A migration foi aplicada em Compose isolado com PostgreSQL descartável, verificada no histórico EF e na coluna criada; backend, frontend e Nginx responderam `Healthy` na porta temporária 18084. A stack e o volume foram removidos ao final. Não foram lidas nem alteradas credenciais do usuário e não houve transmissão externa.

Documentos auxiliares finalizados conforme manuais regulamentares: DANFE oficial com canhoto destacável de recebimento, chave de 44 dígitos formatada em blocos, código de barras Code 128C, grade padrão SEFAZ e cláusulas obrigatórias do Simples Nacional (`infAdic`/`infCpl`). DANFSe oficial com QR Code do padrão nacional, identificação da DPS de origem, competência e detalhamento de ISSQN e tributos federais aproximados. Ambiente de desenvolvimento local conta com `DevSimulatedFiscalGateway` e download de certificado A1 autoassinado para testes manuais no navegador sem emissor pago.

## Última validação da implementação — 15/09/2026

| Verificação | Resultado registrado |
| --- | --- |
| Backend Release | Build sem avisos/erros; 56 unitários e 10 integrações aprovados (100%) |
| Frontend | Lint aprovado (0 erros/avisos), 40 unitários Vitest e build de produção aprovados |
| E2E determinístico | 21 testes de plataforma aprovados em Desktop, Tablet e Mobile; 58 testes gerais aprovados (11 skips) |
| Responsividade | 1440/768/320 px; sem overflow e controles com altura mínima >= 44 px |
| Layout de impressão | DANFE e DANFSe validados sem conflitos de constraint no QuestPDF |
| PostgreSQL | 11 migrations no banco; migration AddTenantFiscalProductionRelease aplicada com sucesso |
| Liberação de Produção | Controle visual em `/plataforma` permitindo liberação de produção fiscal pelo operador master |

Os testes de integração substituem o gateway externo; o smoke real sem A1 valida persistência e bloqueios, sem transmissão fiscal. O ambiente de desenvolvimento dispõe de simulação local para validação de ponta a ponta na interface. Essas evidências não equivalem a autorização de órgão fiscal. O build frontend mantém aviso de bundle inicial de 780,45 kB para budget de 500 kB.

## Próxima entrega

Completar cenários adicionais de resiliência e preparar homologação com cadastro, credenciamento e A1 do piloto. Produção exige habilitação explícita no servidor e tenant homologado.

A sequência, dependências e critérios de aceite estão em [NEXT-STEPS.md](NEXT-STEPS.md). Operação e fontes: [FISCAL.md](FISCAL.md). Resultados anteriores estão no [log](IMPLEMENTATION-LOG.md) e no [status histórico](archive/STATUS-ATE-2026-09-13.md); seus números e prioridades são datados.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.
