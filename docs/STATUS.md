# Status do projeto

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

Atualizado em 13/09/2026. Branch de trabalho: `develop`.

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

Configurações, perfis, preparação, certificado A1 cifrado, sequências, documentos e eventos persistem por tenant. A interface usa PrimeNG 21 nas configurações e na OS concluída. Inutilização tem histórico, confirmação, recuperação com XML original, validação do protocolo e lease persistido de dois minutos. Cancelamentos e inutilizações passam por XSD antes do envio.

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
