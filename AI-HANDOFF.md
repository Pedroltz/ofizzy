# Continuidade do Ofizzy

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

- **Enquadramento Perfeito de Tela (Fit to Screen / No Extra Scroll)**:
  - Container principal (`.app-main-area`): configurado com `height: 100vh; height: 100dvh; max-height: 100dvh; overflow: hidden;` no desktop, eliminando a rolagem global da janela do navegador.
  - Área de conteúdo (`.app-content`): configurada com `flex: 1 1 auto; min-height: 0; overflow-y: auto; display: flex; flex-direction: column;`. O cabeçalho e menu lateral permanecem estáticos, com rolagem confinada apenas se estritamente necessário.
  - Estrutura de páginas (`.page-container`): definida com `flex: 1 1 auto; min-height: 0; display: flex; flex-direction: column;` e paddings verticais otimizados (`clamp(1rem, 2vw, 1.5rem)`), integrando cabeçalho, filtros e tabela para caber em telas 1080p, 768p e notebooks sem rolagem desnecessária.
  - Wrapper da tabela (`app-data-table-wrapper` / `.data-table-wrapper`): configurado como flex container `flex: 1 1 auto; min-height: 0; display: flex; flex-direction: column;`, com área de rolagem interna (`.data-table-wrapper__scroll`) suportando overflow vertical e horizontal sem transbordar a página externa.
  - Cabeçalho de tabela fixo (`.data-table th`): `position: sticky; top: 0; z-index: 2;` garantindo que os nomes das colunas permaneçam sempre à vista caso a tabela precise rolar internamente.
  - Barra de ferramentas de dados (`app-data-toolbar`): margem inferior reduzida e `flex-shrink: 0`.
  - Tabelas de dados (`.data-table` / `.work-orders-list-table`): compactação de padding vertical de células (`0.5rem 0.75rem`), aumentando a densidade visual e alinhando perfeitamente os itens com a paginação na mesma tela.
  - Paginador PrimeNG (`.p-paginator`): padding compacto (`0.35rem 0.6rem`), botões de página reduzidos para `2rem` e `flex-shrink: 0`, fixando-o na base da tela sem exigir scroll para visualização.
  - Grids de cartões (`.catalog-grid`, `.order-grid`): configurados com `flex: 1 1 auto; min-height: 0; overflow-y: auto;` e gaps/paddings otimizados (`1rem` e `var(--space-3)`).
  - Painel / Dashboard (`.dashboard-page-container`): espaçamentos e gaps ajustados para `var(--space-3)` e `var(--space-4)`.
  - Responsividade móvel preservada: em telas <= 900px, a estrutura adapta-se dinamicamente para `height: auto` e `max-height: none` com toques mínimos >= 44px e sem estouro horizontal.
- **Validação**: Frontend lint (0 erros/avisos), 69/69 testes unitários aprovados em 12 arquivos, build de produção aprovado; Backend com 56 unitários + 10 testes de integração aprovados (100%).


## Robustez do catálogo de serviços e peças com busca, paginação e visualizações (tabela/cards) — 18/09/2026

- **Busca, Paginação e Toolbar no Catálogo de Ajustes**:
  - `CatalogApiService` e `SessionDataCacheService` atualizados com parâmetros de paginação (`page`, `pageSize`) e busca textual (`q`), consumindo os endpoints paginados do backend.
  - Abas "Mão de Obra e Serviços" e "Peças e Insumos" em `SettingsPage`:
    - Adicionado `app-search-field` com debounce de 250ms para busca rápida em tempo real.
    - Adicionado `app-data-toolbar` com contador de itens e alternância de layout (Tabela / Grid de Cards).
    - Adicionado `p-paginator` PrimeNG com seletor de linhas por página (`[12, 24, 48]`).
    - Adicionados componentes compartilhados `app-empty-state` e `app-loading-state` para transições visuais consistentes.
- **Modos de Exibição Responsivos (Tabela / Grid de Cards)**:
  - Tabela completa PrimeNG para desktop com ações diretas (edição, perfil fiscal e arquivamento).
  - Cards detalhados com alvos de toque >= 44px para telas menores e tablets sem scroll horizontal indesejado.
- **Padronização de Indentação e Organização do Código**:
  - Backend (.NET): `.editorconfig` na raiz padronizando 4 espaços para C#, quebras de linha e remoção de instruções comprimidas. Executado `dotnet format whitespace`.
  - Frontend (Angular): scripts `"format"` e `"format:check"` adicionados ao `package.json` e execução completa do Prettier e ESLint.
- **Validação**: Frontend lint (0 erros/avisos), 69 testes unitários Vitest aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários e 10 testes de integração aprovados (100%).


## Endereço estruturado do cliente com busca ViaCEP e hidratação automática na emissão fiscal — 18/09/2026

- **Endereço Estruturado e Dados Fiscais no Cliente**:
  - Entidade `Customer` expandida com: `PostalCode` (CEP 8 dígitos), `Street` (Logradouro), `Number` (Número), `District` (Bairro), `City` (Cidade), `State` (UF 2 letras), `CityCode` (Código IBGE 7 dígitos) e `StateRegistration` (Inscrição Estadual).
  - Migration EF Core criada (`20260918165000_AddCustomerFiscalAddress`) e aplicada no PostgreSQL (`ofizzy.customers`).
  - Validações FluentValidation em `CustomerContracts.cs`: regras para CEP, UF e Código IBGE. O campo de texto livre `Address` é preservado e formatado automaticamente quando omitido.
- **Frontend e Consulta Pública ViaCEP**:
  - `CustomersPage` atualizado com o card `Localização & Dados Fiscais` responsivo e toques táteis >= 44px.
  - Busca de CEP automatizada: consulta pública no ViaCEP com preenchimento transparente de Logradouro, Bairro, Cidade, UF e Código IBGE municipal.
- **Automação Completa na Emissão Fiscal da Ordem de Serviço**:
  - `FiscalPreparationService`: ao carregar/preparar a OS para NFS-e ou NF-e, se os campos de endereço do destinatário estiverem vazios, preenche automaticamente a partir do cliente (`Customer`), incluindo `recipient.Address`, `StateRegistration` e o indicador de IE (`RecipientIeIndicator: "1"` se PJ com IE, `"9"` se não contribuinte), poupando tempo e evitando falhas na SEFAZ.
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
  - Implementado tipo `currency` no formulário dinâmico fiscal (`FiscalField` e `FiscalFieldsComponent`), configurando `p-inputnumber` com `mode="currency"`, `currency="BRL"`, `locale="pt-BR"`, `placeholder="R$ 0,00"` e altura de 44 px, alinhado aos formulários de OS e Catálogo.
  - Campos convertidos para `type: 'currency'`: `paymentAmount` (Valor do pagamento da NF-e) e valores unitários de ST (`retainedStBase`, `retainedStAmount`, `substituteAmount`).
- **Automação Inteligente a partir dos Itens da OS**:
  - No backend (`FiscalPreparationService`): na carga inicial da preparação ou quando `PaymentAmount` for nulo, calcula automaticamente o total de peças (`partsTotal = order.Parts.Sum(x => x.Quantity * x.UnitPrice)`) e define `PaymentAmount` com base no valor da OS e `PaymentCode: "01"` ("Dinheiro"), ou `0` com `PaymentCode: "90"` ("Sem pagamento") quando não houver peças.
  - No frontend (`WorkOrderFiscalComponent`):
    - Ao abrir a preparação, preenche automaticamente o valor exato dos produtos da OS caso esteja vazio.
    - Sincronização reativa ao alterar o meio de pagamento: ao selecionar "Sem pagamento" (90), zera o valor (`R$ 0,00`); ao retornar para outro meio, preenche com o total de produtos.
    - Banner auxiliar `.fiscal-payment-hint` no card de declaração de pagamento com texto explicativo da regra da SEFAZ e botão `Usar total da NF-e (R$ X,XX)`.
- **Validação**: Frontend lint (0 erros/avisos), 69 unitários Vitest aprovados em 12 arquivos (incluindo novo teste para campo `currency`), build de produção aprovado; Backend com 56 unitários + 10 testes de integração aprovados (100%).

## Atualização dos favicons oficiais do sistema — 18/09/2026

- **Favicons Multi-Resolução e Alta Definição**:
  - Imagem do ícone com engrenagem e checkmark (`ChatGPT Image 17 de set. de 2026, 15_40_02.png`) processada e padronizada para os favicons do sistema em `src/frontend/ofizzy-web/public/`:
    - `favicon.ico`: multi-camada (16×16, 32×32, 48×48, 64×64) para compatibilidade retroativa e navegadores clássicos.
    - `favicon.png`: 512×512 px com transparência alfa e respiro sutil para abas modernas e PWA shortcuts.
    - `apple-touch-icon.png`: 180×180 px para atalhos em dispositivos móveis e iOS.
  - Atualizado `src/frontend/ofizzy-web/src/index.html` com os links dos favicons mantendo os seletores de tema, scripts antiforgery e tipografia Inter.
- **Validação**: Frontend lint (0 erros), 68 unitários aprovados em 12 arquivos, build de produção aprovado e arquivos presentes na raiz do build (`dist/ofizzy-web/browser/`).

## Troca de organizações na barra superior e expansão do logotipo na barra lateral — 17/09/2026

- **Troca de Organizações no Cabeçalho Superior Minimalista**:
  - Seletor posicionado no cabeçalho superior (`desktop-header__left`) com design minimalista: fundo e bordas transparentes por padrão, sem sombras ou aspecto de botão encorpado, integrado harmoniosamente à barra flutuante.
  - Efeito suave ao passar o mouse (`background: var(--surface-hover)` e cantos arredondados `var(--radius-md)`), chevron simplificado sem caixa de fundo, e nome completo da organização ativa (`Arroba Pneus - Igaraçu do Tietê`, etc.).
  - Popover nativo PrimeNG (`.org-switcher-popover`) preservado integralmente, exibindo a lista de empresas, cargo, checkmark na empresa ativa e link para a plataforma.
- **Logotipo Oficial na Barra Lateral e Alternância por Tema**:
  - Suporte ao logotipo dedicado do tema escuro (`logo-dark.png`) com fundo transparente e proporções 2048 × 682 px exatamente alinhadas à logo clara, sem saltos de tamanho.
  - Alternância instantânea via CSS entre `logo.png` e `logo-dark.png`.
- **Barra Superior Flutuante Minimalista (`desktop-header` e `mobile-header`)**:
  - Barra superior flutuante com margem externa (`var(--space-3) var(--space-6) 0`), bordas levemente arredondadas (`var(--radius-lg)` = 12px), contorno fino perimetral (`1px solid var(--border-subtle)`) e sem sombras pesadas (`box-shadow: none`), proporcionando visual limpo, moderno e plano.
  - Vidro fosco translúcido (`color-mix(in srgb, var(--surface-primary) 92%, transparent)` e `backdrop-filter: blur(12px)`), fixado a 12px do topo com scroll suave.
- **Validação**: Frontend lint (0 erros), 68 unitários aprovados em 12 arquivos, build de produção aprovado; Backend com 56 testes unitários aprovados.

## Validação visual com destaque em vermelho na área fiscal — 17/09/2026

Implementado destaque visual em vermelho para campos obrigatórios ou com erro em todos os formulários fiscais da aplicação (Catálogo de Peças/Serviços, Configurações Fiscais da Empresa e Preparação Fiscal da Ordem de Serviço):
- Centralizado em `FiscalFieldsComponent` e `fiscalForm`: os campos obrigatórios exibem indicador asterisco vermelho `*` no rótulo e, quando inválidos e tocados, recebem a classe `.has-error`, borda vermelha (`var(--danger)`), anel de foco suave avermelhado (`var(--danger-soft)`), rótulo em vermelho e mensagem de erro explicativa abaixo do controle (`<small class="fiscal-error-msg"><i class="pi pi-exclamation-circle"></i> ...</small>`).
- Nos diálogos de Catálogo (`SettingsPage`): `savePart()` e `saveService()` validam NCM, regras de CFOP/CSOSN, ST e código de tributação nacional, marcando os campos com erro e executando `markAllAsTouched()` para que o campo fique vermelho imediatamente junto à notificação Toast.
- Em Configurações Fiscais (`FiscalSettingsComponent`) e Preparação da OS (`WorkOrderFiscalComponent`): validações de formulário, senha de certificado e mapeamento reativo de pendências retornadas pelo backend (`issues`) acendem os campos exatos em vermelho.
- Evidências: `npm run lint` 0 erros/avisos; `npm test` 68 testes unitários aprovados em 12 arquivos (incluindo novos testes em `fiscal-fields.component.spec.ts` e `settings.page.spec.ts`); `npm run build` aprovado; `dotnet test` 56 testes unitários backend aprovados.

## Configuração fiscal integrada no catálogo e dropdown no topo — 17/09/2026

Implementada a configuração fiscal integrada diretamente nos diálogos de cadastro e edição de peças (NF-e) e serviços (NFS-e) em Catálogo e Ajustes (`SettingsPage`), eliminando a necessidade de navegação até a aba Fiscal para vincular NCM, CFOP, CSOSN ou código de tributação:
- Acionada por slider/toggle switch (`p-toggleswitch`) com card informativo `.fiscal-accordion-card` visível para administradores (`tenantContext.admin()`).
- Novos produtos vêm com padrões inteligentes de revenda interna do Simples Nacional (CFOP 5102, CSOSN 102, Origem 0, Unidade UN, GTIN SEM GTIN, PIS/COFINS 07), exigindo apenas digitar o NCM de 8 dígitos.
- Edição de itens com perfil fiscal existente ativa o slider automaticamente com os dados preenchidos.
- Validação prévia de NCM e regras fiscais antes de salvar o produto e o perfil (`PUT /api/parts/{id}/fiscal` e `PUT /api/services/{id}/fiscal`).
- Evidências: `npm run lint` 0 avisos; `npm test` 62 unitários aprovados (11 arquivos, incluindo `settings.page.spec.ts`); `npm run build` aprovado; Playwright 40 testes responsivos aprovados; `dotnet test` 56 unitários + 10 integrações aprovados.

## Seletor de organização em dropdown no topo e tipografia global Inter — 17/09/2026

Implementado o seletor inteligente de organização diretamente na marca `Ofizzy` superior (desktop sidebar, mobile header e drawer), substituindo o item intermediário "Trocar organização" do menu da barra lateral:
- Se o usuário tem 1 organização (`userTenants.length <= 1`), a marca é apenas link simples para o início, sem chevron e sem acionar menu.
- Se o usuário tem 2 ou mais organizações (`userTenants.length > 1`), exibe chevron indicador e abre o `p-popover` do PrimeNG 21 com a lista de empresas, cargo e checkmark na ativa, permitindo alternar em 1 clique com feedback visual.
- Criado `OrganizationSwitcherComponent` (com layout slim e elegante) e atualizado `TenantContextService` com `userTenants` e `hasMultipleTenants`.
- Padronizada a tipografia global da aplicação para a fonte `Inter` via Google Fonts no [`index.html`](file:///C:/Users/pedro.tunin/Documents/Projetos/Pessoal/ofizzy/src/frontend/ofizzy-web/src/index.html), `--font-sans` em [`tokens.css`](file:///C:/Users/pedro.tunin/Documents/Projetos/Pessoal/ofizzy/src/frontend/ofizzy-web/src/styles/tokens.css) e [`styles.css`](file:///C:/Users/pedro.tunin/Documents/Projetos/Pessoal/ofizzy/src/frontend/ofizzy-web/src/styles.css) (aplicando em `html`, `body`, controles nativos e componentes PrimeNG `.p-component`), unificando a barra lateral ("Gestão", "Operação", "Sistema") e demais interfaces.

## Correção de sessão — 16/09/2026

`AuthService.refreshSession()` busca `/api/setup/status` antes e depois de rotacionar o refresh para sincronizar a identidade do token antiforgery. Não remover essas leituras: o XSRF autenticado anterior passa a ser inválido quando o access cookie expira. O interceptor repete a escrita com o novo header XSRF, encerra a sessão também no segundo 401 e não confunde rede/400/429/5xx com expiração. `restore()` delega a renovação ao interceptor, sem segundo refresh; sessão invalidada não é restaurada nesta aba até login explícito. Não houve alteração no backend ou nas migrations.

Evidências: backend build/56 unitários/10 integrações; frontend lint/51 unitários/build; 61 E2E existentes + 9 novos aprovados, 11 skips existentes; smoke real de sessão pelo Nginx antes/depois de restart, com persistência. Reproduzir com `playwright.session.config.ts` em stack isolada/bootstrap habilitado na porta 18083, nunca em produção. `OFIZZY_VERIFY_RESTART=1` verifica a empresa fictícia inicial persistida. Ver log para comandos e limites; homologação fiscal permanece pendente.

Atualizado em 13/09/2026. Trabalhar em `develop`; `main` é exclusiva de releases. Ler `docs/PROJECT.md`, `docs/STATUS.md`, este arquivo, `docs/ARCHITECTURE.md` e a [fase ativa](docs/phases/PHASE-08-FISCAL.md) antes de alterar código.

## Ponto de retomada

O usuário priorizou emissão oficial de serviços e produtos a partir da OS, sem intermediário pago. Piloto já informado: **Igaraçu do Tietê/SP, Simples Nacional, IBGE 3520004**. Não perguntar novamente município/regime. O certificado A1 é uma dependência de homologação; não foi fornecido nem usado certificado real nos testes.

Fundação SaaS e fases 1–4 têm aceite local. Fiscal está implementado localmente com leiautes regulamentares concluídos (DANFE com canhoto e barcode, DANFSe com QR Code e detalhamento tributário, regras obrigatórias do Simples Nacional em infAdic/infCpl e infoCompl/xInfComp). Homologação externa pendente. Financeiro não começou; pagamentos declarados na NF-e não registram recebimentos.

## Implementação para localizar

- Backend: `src/backend/Ofizzy.Api/BusinessCore/Fiscal/`. Serviços concretos, DTOs e regras; um `ApplicationDbContext`.
- `FiscalEmissionService`: preparação, reserva, assinatura, envio/consulta e cancelamento; mantém identidade e XML em resultado inconclusivo.
- `FiscalGateway`: comunicação oficial direta e correlação de resposta; `DevSimulatedFiscalGateway`: gateway simulado local ativo exclusivamente em `Development` (`Fiscal:SimulateGateway: true`); `FiscalDevController`: gerador de certificado A1 autoassinado para testes locais; `FiscalXml`: assinatura, XSD local, regras do Simples Nacional (`infAdic`/`infCpl`) e DPS (`infoCompl`); `FiscalPdf`: DANFE oficial com canhoto destacável e DANFSe nacional com QR Code; `FiscalCertificateVault`: A1 cifrado com chave externa versionada.
- `FiscalInutilizationsController`: histórico e recuperação de intervalo, XML original, lease de dois minutos, confirmação estrita de protocolo e identificação. Incerteza mantém números reservados.
- Frontend: `src/frontend/ofizzy-web/src/app/features/fiscal/`, integrado às configurações e detalhes da OS finalizada. Manter PrimeNG 21, tokens, desktop e 320/768 px.
- Migrations fiscais: `AddFiscalFoundation`, `CompleteFiscalInutilization`, `AddFiscalInutilizationLease`. Dez migrations no banco de smoke; não remover históricos ou reescrever migrations aplicadas.

## Restrições que devem permanecer

Todas as entidades fiscais usam TenantId, filtros e FKs compostas. Owner/Admin administra configuração, perfis, certificado, cancelamento e inutilização; membros preparam/emitem/baixam conforme permissões. Não expor entidades EF ou certificados nos DTOs/logs.

Uma OS mista tem NF-e e NFS-e independentes. Autorização parcial não autoriza reenviar o documento já autorizado. Resultado inconclusivo exige consulta/recuperação; não liberar numeração nem substituir o XML assinado. Downloads dependem de XML autorizado persistido. Cancelamento preserva histórico.

Produção permanece condicionada à liberação do operador da plataforma em `/plataforma` (`Tenant.FiscalProductionReleased`), com suporte mantido a `Fiscal:ProductionEnabled` e `Fiscal:HomologatedTenants`. O simulador de desenvolvimento nunca opera em ambiente de produção. XSD NF-e incorporado é baseline antigo, não prova conformidade vigente. DANFE e DANFSe atendem aos manuais e foram testados com fixtures autorizadas; homologação oficial permanece obrigatória antes de produção. Não apresentar teste com gateway substituído como homologação oficial.

## Retomar o trabalho

Seguir [Próximos passos](docs/NEXT-STEPS.md). [FISCAL.md](docs/FISCAL.md) contém configuração, contratos e fontes. Não há autorização implícita para transmitir notas reais ou publicar produção sem homologação comprovada.

Desenvolvimento habitual: PostgreSQL via `compose.local.yaml`, API no host (`dotnet run --project src/backend/Ofizzy.Api --launch-profile local`) e Angular (`npm start` em `src/frontend/ofizzy-web`). Proxy 4200 → 5154. Smoke fiscal usa stack isolada `ofizzy-fiscal-smoke`, porta 18082; confirmar estado antes de reutilizar. Não alterar o volume de desenvolvimento para executar smoke.

Último aceite da implementação em 15/09/2026: backend build/56 unitários/10 integrações aprovados (100%); frontend lint/40 unitários/build; 21 E2E de plataforma aprovados em Desktop, Tablet e Mobile; migration AddTenantFiscalProductionRelease adicionada. Comandos e limites em [TESTING.md](docs/TESTING.md).

Preservar alterações de código já presentes na árvore. Registrar novas evidências em STATUS, IMPLEMENTATION-LOG e fase 8. O [handoff anterior](docs/archive/HANDOFF-ATE-2026-09-13.md) foi arquivado para consulta histórica; não define tarefas vigentes.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](docs/REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.
