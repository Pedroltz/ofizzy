# Fase 8 — Documentos fiscais de serviços e produtos

## Editor de OS pesquisável e compacto — 21/09/2026

- O formulário de OS usa autocomplete para cliente e veículo em lugar dos seletores
  nativos. A pesquisa de cliente cobre nome, CPF/CNPJ e telefone; a de veículo cobre
  placa, marca e modelo dentro do cliente escolhido. O backend continua recebendo IDs
  e aplicando regras/cálculos ao salvar.
- A identificação foi condensada em duas faixas e o diálogo ganhou área útil no
  desktop, com grade responsiva em tablet/mobile. O fluxo suporta muitas linhas por
  meio de rolagem interna somente quando necessário; não há overflow horizontal.
- Evidências: lint, 69 testes, build e 40 E2E responsivos aprovados, incluindo busca
  e seleção de cliente/veículo no modal mobile.

## Revisão dos PDFs fiscais — 21/09/2026

- DANFE/DANFSe foram revisados para usar um padrão único em homologação e produção.
  A marca-d’água diagonal `HOMOLOGAÇÃO` é a única diferença visual e é lida de
  `tpAmb` no XML autorizado. Não há faixa de simulação nem marca do sistema no
  documento auxiliar fiscal.
- O DANFSe foi ajustado à NT SE/CGNFS-e nº 008/2026, incluindo título `DANFSe v2.0`,
  subtítulo, identificação de município/gerador/ambiente, dados NFS-e/DPS, QR Code e
  chave. Os demais blocos preservam as informações autorizadas de emitente/prestador,
  tomador/destinatário, itens, tributação, totais, protocolo e situação.
- Evidências: renderização e inspeção visual de exemplos DANFE/DANFSe, 74 testes
  unitários e build Release do backend, lint, 69 testes, build e 40 E2E do frontend
  aprovados; Compose validado com variáveis fictícias. Aderência externa ao
  autorizador continua requisito de homologação oficial.

## Catálogo pesquisável na OS — 21/09/2026

- Corrigida a paginação usada ao abrir o editor: serviços e peças eram buscados na
  página 100 e não apareciam no catálogo. O editor passa a buscar a página inicial
  com até 100 registros.
- A inclusão de item usa autocomplete PrimeNG por nome, com normalização de acentos e
  capitalização, e por código de peça. A seleção envia a ID do catálogo ao fluxo de
  OS já existente, mantendo preços e regras críticos no backend.
- Evidências: 72 testes unitários e build Release do backend; lint, 69 testes
  unitários, build do frontend e 40 E2E responsivos em desktop, mobile e tablet
  aprovados; 11 skips são cenários não aplicáveis ao viewport.

## Correção de pesquisa de clientes — 21/09/2026

- A pesquisa de clientes agora encontra contatos mesmo quando o usuário digita
  telefone com espaços, parênteses ou hífen. Números existentes foram normalizados
  pela migration `20260921100000_NormalizeCustomerContactNumbers`; a UI preserva a
  formatação para leitura.

## Remoção do painel e revisão local de cadastros — 21/09/2026

- O painel e o endpoint de prontidão para homologação foram removidos a pedido do
  usuário. Dados fiscais, A1 e classificação por item continuam nas Configurações.
- A revisão de leitura do banco local encontrou cobertura completa de perfis fiscais
  e destinatários para as duas organizações de desenvolvimento. Os identificadores e
  A1 são de teste e não podem substituir os dados reais para homologação externa.

## Migração PL_010f v1.04 — 21/09/2026

- O catálogo passou a selecionar `NF-e PL_010f v1.04` para novas emissões e os cinco
  XSDs oficiais foram instalados em pasta dedicada, com teste de integridade. XML de
  NF-e do cenário já suportado foi assinado e validado contra o novo pacote.
- O pacote RTC de eventos v1.40 foi versionado separadamente. Ele não contém os
  envelopes de cancelamento ou inutilização; esses fluxos conservam seus schemas
  oficiais atuais até que haja evento aplicável ao domínio suportado.
- A prontidão local de schema deixou de bloquear NF-e, sem liberar produção, ignorar
  credenciamento ou inventar classificação IBS/CBS. Esses requisitos continuam
  externos e condicionais por operação.
- Evidências: 73 testes unitários, 10 de integração e build Release do backend;
  lint, 69 testes e build do frontend; 24 E2E da plataforma em desktop/mobile/tablet.
  Não há migration neste incremento. Compose foi validado apenas com variáveis
  fictícias, sem iniciar serviços ou transmitir documento fiscal.

## Auditoria regulatória e preparação de homologação — 19/09/2026

- Criada a [matriz regulatória](../fiscal/FISCAL-REGULATORY-AUDIT-2026.md) somente
  com fontes oficiais. Ela identifica o NF-e `PL_010f_v1.04` como vigente e NTs
  RTC/CNPJ alfanumérico; o baseline local NF-e `010c` segue pendente de migração
  conjunta com XML, domínio e testes.
- Os XSDs NFS-e ativos 1.01 foram confrontados byte a byte com o pacote oficial de
  produção `NFSe-ESQUEMAS_XSD-v1.01-20260209` (SHA-256 registrado em
  `Schemas/Nfse/README.md`). O pacote RTC de produção restrita 20260727 diverge e
  não foi ativado sem regras de IBS/CBS.
- Adicionados runbook de homologação e checklist de produção em `docs/fiscal/`.
  Nenhum deles representa homologação externa ou liberação de produção. O teste
  fiscal agora demonstra que alteração posterior ao XML invalida a assinatura.
- Validação local relacionada: 28 testes fiscais unitários aprovados, incluindo a
  integridade dos dez XSDs NFS-e ativos. A tentativa de
  migrar SHA-1 para SHA-256 isoladamente foi recusada pelo XSD NF-e/eventos atual;
  a troca fica bloqueada até o pacote sucessor oficial e regras de transição serem
  instalados e testados.
- O smoke fiscal runtime isolado (`ofizzy-fiscal-smoke`, porta 18082) passou 2/2
  testes E2E antes e depois do restart: persistência de configuração/preparação,
  emissão simulada de XML/PDF e cancelamento de NFS-e via Nginx. Foram usados dados
  fictícios e `Development`; a evidência não equivale a homologação externa.
- Em 20/09/2026, o DANFE foi reforçado para usar chave e série do XML autorizado,
  nunca campos locais divergentes. Se o XML não contém esses campos, a geração falha
  com erro compreensível em vez de inventar dados.
- Evidência adicional em 20/09/2026: 6 testes visuais de PDF e 58 testes unitários
  backend aprovados; build Release sem avisos/erros e `git diff --check` limpo.
- Em 20/09/2026, nova consulta com o cookie requerido pelo Portal permitiu obter os
  ZIPs NF-e 010e e 010f diretamente da fonte oficial. O 010f é a versão atual; os
  hashes e a comparação foram registrados. A migração continua pendente da adaptação
  conjunta, sem ativar nem substituir schemas prematuramente.
- Em 20/09/2026, a seleção dos schemas ativos passou a ser explícita por pacote e
  finalidade. DANFE/DANFSe passam a derivar ambiente e identidade do XML autorizado;
  o A1 é revalidado em cada uso (validade, RSA, key usage aplicável e CNPJ). Validação:
  64 unitários backend e build Release aprovados sem avisos/erros.
- Em 20/09/2026, foi implementado o cálculo oficial do DV de CNPJ alfanumérico em
  capacidade isolada para o futuro 010f. O fluxo ativo permanece numérico e a migração
  só será ativada junto com XML, chave, schemas e regras fiscais. Validação: 69
  unitários backend e build Release aprovados sem avisos/erros.
- Em 20/09/2026, a consulta de eventos NFS-e foi corrigida para o ADN oficial de
  produção restrita, consultando todo o histórico por chave de acesso. O endpoint ADN
  produtivo permanece bloqueado sem fonte oficial direta. Validação: 70 unitários
  backend e build Release aprovados sem avisos/erros.
- Em 20/09/2026, foi criada a matriz de domínio RTC com fontes, condições e critérios
  de entrada para IBS/CBS, CNPJ alfanumérico, IS e NFS-e. A contabilidade ainda deve
  fornecer classificações e cenários antes da implementação de cálculo/XML.
- Em 20/09/2026, cada `FiscalDocument` passou a preservar o pacote de schema usado
  na emissão. A migration `20260920070200_AddFiscalDocumentSchemaPackage` classifica
  o acervo atual como `NF-e PL_010c` ou `NFS-e Nacional 1.01`, e novas emissões usam
  `FiscalSchemaCatalog`. Esse registro é pré-requisito para não reinterpretar XMLs
  históricos numa futura adoção do PL_010f; não ativa o novo leiaute.
- Evidências: migration gerada por EF Core em Release, backend 71/71, lint frontend,
  69/69 testes frontend e build aprovados. A migration foi aplicada e conferida em
  PostgreSQL de Compose descartável; backend, frontend e Nginx ficaram saudáveis na
  porta temporária 18084 antes da remoção da stack e do volume.
- Em 20/09/2026, foi incluído o painel de prontidão para homologação, alimentado pelo
  endpoint autenticado `GET /api/fiscal/homologation-readiness`. Ele não libera
  produção e não presume credenciamento: apresenta, separadamente, dados locais
  verificados, pendências externas e o bloqueio de NF-e enquanto PL_010c estiver ativo.
  Validação: backend 72/72, lint frontend, 69/69 testes frontend e build; migration e
  smoke pelo Nginx passaram em Compose descartável na porta 18085, removido ao final.
- Em 20/09/2026, perfis de produtos e serviços passaram a aceitar revisões por data
  de vigência (`EffectiveFrom`). A preparação seleciona a versão vigente e o snapshot
  fiscal conserva a classificação aplicada. A migration
  `20260920180236_AddFiscalProfileEffectiveDate` mantém os dados anteriores em
  01/01/2000. Esse mecanismo torna dados fiscais configuráveis, mas não permite emitir
  tags RTC/PL_010f sem a implementação e validação específicas.
  Evidências: script EF idempotente, backend 72/72, lint, 69/69 testes frontend,
  build e migration/smoke Nginx em Compose descartável na porta 18087 aprovados.

## Endereço estruturado do cliente com busca ViaCEP e hidratação automática na emissão fiscal — 18/09/2026

O cadastro de clientes foi expandido para suportar endereço estruturado e dados fiscais:
- Novos campos no modelo `Customer`: `PostalCode`, `Street`, `Number`, `District`, `City`, `State`, `CityCode` (IBGE 7 dígitos) e `StateRegistration` (Inscrição Estadual).
- Interface de cliente com card dedicado `Localização & Dados Fiscais` e integração com a API pública do ViaCEP, preenchendo automaticamente o logradouro, bairro, cidade, UF e código IBGE do município.
- No `FiscalPreparationService`: a preparação fiscal da OS identifica se o destinatário não possui endereço preenchido e hidrata automaticamente os dados fiscais a partir do cliente (`Customer`), incluindo endereço completo, Inscrição Estadual e indicador de IE (`RecipientIeIndicator`), evitando digitação repetitiva e erros de validação da SEFAZ.

Validação: 56 unitários + 10 integrações backend aprovados; frontend lint aprovado (0 erros/avisos), 69 unitários Vitest aprovados em 12 arquivos, build de produção aprovado.

## Exibição da senha do Certificado A1 Dev e refinamento de validação fiscal — 18/09/2026

Adicionada a exibição sutil da senha padrão do certificado A1 de homologação (`teste123`) em texto discreto logo abaixo do botão de cadastrar/substituir em Configurações Fiscais.
O botão de download de certificado de teste passa a ser renderizado estritamente quando o ambiente fiscal for `Homologação` (`isHomologation()`) e o servidor estiver em modo de desenvolvimento com simulação ativa (`devToolsAvailable`).
As mensagens de validação fiscal de itens de serviço e produtos na Ordem de Serviço agora incluem o nome/descrição do item como prefixo, eliminando mensagens duplicadas anônimas e integrando com o destaque em vermelho do campo no modal via chave camelCase.

Validação: 56 unitários + 10 integrações backend aprovados; frontend lint aprovado (0 erros/avisos), 69 unitários Vitest aprovados em 12 arquivos, build de produção aprovado.

## Formatação monetária e automação do pagamento da NF-e na Preparação Fiscal — 18/09/2026

Adicionado suporte ao tipo `currency` no formulário fiscal (`p-inputnumber mode="currency" currency="BRL" locale="pt-BR"`) padronizando o campo de pagamento declarado da NF-e e os campos de Substituição Tributária (`retainedStBase`, `retainedStAmount`, `substituteAmount`).
Implementada a automação de valores: o backend e o frontend calculam e preenchem automaticamente o valor de pagamento com base no somatório das peças da OS (`partsTotal`), vinculando a "Dinheiro" (01) ou "Sem pagamento" (90 com R$ 0,00), eliminando a digitação manual redundante e prevenindo rejeições SEFAZ de divergência de valor com botão auxiliar de sincronização imediata.

Validação: 56 unitários + 10 integrações backend aprovados; frontend lint aprovado (0 erros/avisos), 69 unitários Vitest aprovados em 12 arquivos, build de produção aprovado.

## Manutenção transversal de sessão — 16/09/2026

Corrigido falso aviso de expiração causado por XSRF vinculado à identidade anterior do cookie de acesso. Renovação sincroniza XSRF, preserva sessões em falhas temporárias e mantém o usuário no login quando o refresh ou a repetição recebe 401. Sem mudanças de regra fiscal, schema ou interface visual.

Validação: backend build/56 unitários/10 integrações; frontend lint/51 unitários/build; 61 E2E existentes e 9 novos aprovados (11 skips existentes). Smoke real pelo Nginx/PostgreSQL aprovado antes e após restart, incluindo escrita persistida e sessão expirada. Migration runner sem pendências. Aviso preexistente de budget frontend: 783,12 kB para 500 kB. Não constitui homologação fiscal nem deploy de produção.

Estado em 2026-09-13: implementação em desenvolvimento, ainda sem homologação fiscal externa.
Esta fase foi antecipada por solicitação do usuário. A fundação SaaS da fase 7 permanece vigente.

## Decisões de produto

O fluxo desejado é emitir e baixar documentos oficiais a partir de uma OS concluída,
com complementos obrigatórios preenchidos previamente. Uma OS mista produz NFS-e
para serviços e NF-e modelo 55 para produtos. Sem intermediário pago; certificado
A1 existente, infraestrutura e obrigações fiscais continuam necessários.
Não há emissão simulada no produto nem XML próprio apresentado como nota fiscal.

Piloto confirmado pelo usuário: **Igaraçu do Tietê/SP, Simples Nacional**.
Código IBGE **3520004**, conforme [IBGE](https://www.ibge.gov.br/cidades-e-estados/sp/igaracu-do-tiete.html).
CNPJs, endereços e certificados dos testes são fictícios; nenhuma configuração real foi alterada.

## Implementado

- Módulo `BusinessCore/Fiscal`, um DbContext, DTOs e validações no backend.
- Configuração por organização, perfis de produtos/serviços e preparação da OS persistidos.
- Certificado A1 protegido com AES-GCM, chave externa versionada e vínculo ao tenant.
- Documentos, sequência por tipo/ambiente/série, snapshots, eventos e controle de concorrência.
- Geração e assinatura de NF-e 4.00 e DPS 1.01, validadas com XSDs locais.
- Adaptadores diretos NFS-e Nacional e NF-e SP/SVRS, consulta e solicitação de cancelamento.
- Tentativa inconclusiva conserva identidade/XML; consulta precede reenvio.
- XML/PDF individual e ZIP condicionado a documento autorizado; emissão parcial visível.
- Configurações e preparação em componentes PrimeNG 21 integrados às páginas existentes.
- Inutilização administrativa de números reservados e rejeitados, histórico na interface,
  recuperação do protocolo com XML original e exclusão mútua por lease persistido.
- Produção condicionada a habilitação no servidor e lista de tenants homologados.

## Pendências de desenvolvimento e homologação

- Validar cadastro/credenciamento e parâmetros municipais do piloto no emissor nacional.
- Atualizar o pacote NF-e de referência para a versão vigente, revisar notas técnicas/RTC
  aplicáveis ao Simples e validar operações reais em homologação. Passar no XSD não
  demonstra aceitação de regras fiscais pela SEFAZ ou prefeitura.
- Homologar cancelamento e inutilização com os órgãos fiscais. Os pedidos já passam
  por XSD; recuperação local preserva os números até confirmar protocolo/faixa/CNPJ.
- Revisar o PDF contra os manuais oficiais completos de DANFE/DANFSe e homologar.
  O teste atual comprova geração do arquivo, não conformidade integral do leiaute.
- Exercitar cancelamento oficial, rejeição seguida de correção, numeração concorrente,
  rotação de chave e falhas de persistência após resposta oficial.
- Definir série exclusiva do Ofizzy antes de ativar um emissor que já usa outro sistema.
  Não há importação de numeração legada nem fluxo de venda avulsa.

Não habilitar produção apenas porque o build e os testes locais passaram.

## Perfil fiscal inicial

Produtos: revenda interna, 5102/102 ou 5405/500, PIS/COFINS sem destaque,
com valores de ST anterior fornecidos pela contabilidade. Não há apuração automática
por NCM, importação de XML de entrada, interestaduais, devoluções, contingência ou cartões.
Serviços: classificação única por NFS-e, sem retenção/deduções; percentual aproximado
informado pela contabilidade. Não generalizar esse perfil para todos os serviços/municípios.

A Prefeitura de Igaraçu do Tietê anunciou a migração obrigatória para o padrão nacional
para ME/EPP do Simples desde **01/08/2026**. O conector nacional é, portanto, a direção
de integração do piloto; a autorização da empresa e suas classificações ainda exigem
homologação. Fonte consultada em 12/09/2026: [comunicado municipal](https://www.igaracudotiete.sp.gov.br/portal/noticias/0/3/1594/atencao-contribuintes-e-empreendedores-de-igaracu-do-tiete).
Não aplicar a esse município o cronograma da capital São Paulo.

## Operação e testes

Veja [FISCAL.md](../FISCAL.md) para configuração, contratos, fontes e evidências.

## Continuidade — 13/09/2026

## Modularização de operação — 21/09/2026

Veículos/Automotive agora é opcional por organização, sem alterar os requisitos fiscais. OS sem veículo são suportadas pelo domínio e pela migration `20260921113000_MakeWorkOrderVehicleOptional`, mas emissão fiscal continua dependente dos módulos e campos exigidos pelo fluxo fiscal. Não interpretar a capacidade de operar OS genérica como autorização de transmissão fiscal.

Cancelamento NF-e/NFS-e e inutilização passam por XSD antes do envio. Recuperação
administrativa em `POST /api/fiscal/nfe/inutilizations/{id}/sync` reaproveita o pedido
assinado e confirma retornos 102/563 somente com protocolo e identificação compatíveis.
Uma falha durante recuperação mantém o intervalo bloqueado. Lease de dois minutos
impede concorrência e permite retomar após queda do processo. Eventos preservam as tentativas.
A tela exige revisão/confirmacão do intervalo e mostra erros sem perder os campos.
Validação: build Release, 48 unitários backend/9 integrações; frontend lint, 39 unitários,
build e 52 E2E/11 skips; novo formulário verificado em 1440/768/320 px.

Aceite final de 13/09/2026: smoke real pelo Nginx aprovado antes e após reinício
dos quatro containers da stack isolada; configuração/preparação preservadas e
faixa não reservada rejeitada sem transmissão. Teste adicional confirmou 404 ao
tentar recuperar pedido de outro tenant e histórico vazio para essa organização.

## Continuidade consolidada — 13/09/2026

Recuperação de inutilização, lease e XSD de eventos estão concluídos localmente.
Também foram concluídos a inclusão das regras/tags obrigatórias do Simples Nacional
(infAdic/infCpl na NF-e e infoCompl/xInfComp na DPS), o redesign regulamentar de
DANFE (canhoto destacável, Code 128C, 44 dígitos formatados, layout SEFAZ) e DANFSe
(cabeçalho Nacional Sefin, QR Code oficial, impostos e valor líquido) em `FiscalPdf.cs`,
além do ambiente de testes locais com `DevSimulatedFiscalGateway` e gerador de certificado A1
fictício exclusivo para a fase de desenvolvimento.
A suíte unitária de backend foi expandida para 48 testes (100% aprovados).

A próxima entrega concentra a homologação externa oficial nos webservices da SEFAZ-SP e Sefin Nacional.
Não habilitar produção antes desse aceite.

O estado geral está em [STATUS](../STATUS.md); a ordem, dependências e critérios futuros estão em [NEXT-STEPS](../NEXT-STEPS.md). Resultados anteriores neste documento preservam a data e o escopo originais.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](../REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.

## Estabilização e validação integral — 13/09/2026

Após a revisão independente e a aplicação das correções pendentes, foram resolvidos os pontos bloqueantes de testes automatizados:
- **QuestPDF DANFE**: corrigida a proporção de aspecto do código de barras Code 128 (altura ajustada para 28 px), eliminando `DocumentLayoutException`.
- **Acessibilidade mobile**: aplicada altura mínima de 44 px nos botões de `fiscal-settings.component.ts`, sanando a falha no teste de responsividade.
- **Validação final da fase**: 55 testes unitários backend, 10 testes de integração, lint e 39 testes unitários frontend, e 58 testes E2E (11 skips) 100% aprovados.
