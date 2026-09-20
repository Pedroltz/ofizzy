# Auditoria regulatória fiscal — 2026-09-19

## Escopo e método

Esta matriz é uma fotografia de fontes **oficiais** consultadas em 19/09/2026. Ela
não é homologação externa e não autoriza produção. A validação XSD é somente uma
barreira estrutural: regras de negócio, credenciamento, cadastro e respostas do
autorizador continuam mandatórios.

O piloto permanece Igaraçu do Tietê/SP, Simples Nacional. Nenhum endpoint oficial
foi chamado e `Fiscal:ProductionEnabled` continua com valor padrão `false`.

## Matriz de versões e impacto

| Tema | Fonte oficial, versão e data | Impacto no Ofizzy / arquivos afetados | Decisão |
| --- | --- | --- | --- |
| NF-e, schemas em produção | [Portal NF-e — Esquemas XML](https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D): `PL_010f_v1.04`, publicado 31/08/2026 para NT 2025.002 v1.50 e NT 2026.007 v1.00. O `010e_v1.02` de 10/07/2026 foi movido para versões anteriores. | O repositório contém o baseline `010c`/4.00 e eventos legados em `BusinessCore/Fiscal/Schemas/Nfe` e `NfeEvents`; `FiscalXml.cs`, `FiscalGateway.cs`, `FiscalPdf.cs` e testes dependem dele. | Alteração necessária: partir do 010f, com origem e hash registrados, sem sobrescrever o baseline; adaptar modelo/XML antes de torná-lo ativo. |
| NF-e, CNPJ alfanumérico | [NT 2026.004 v1.01](https://www.nfe.fazenda.gov.br/portal/informe.aspx?AspxAutoDetectCookieSupport=1&Informe=aBsArQGfNqU%3D&ehCTG=false), publicada 08/06/2026; package `010d_v1.03` publicado 10/07/2026. [Receita Federal — DV](https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/documentos-tecnicos/cnpj), manual publicado 05/11/2024. | `FiscalValidation.cs`, modelos e geração de chave aceitam apenas CNPJ numérico; certificados e chaves de acesso precisam de estratégia explícita de versão. | Alteração necessária, mas não aceitar caracteres alfanuméricos parcialmente: implementar algoritmo oficial, tipos e testes junto do schema aplicável. |
| Reforma Tributária NF-e (IBS/CBS) | [NT 2025.002 v1.50](https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx/listaConteudo.aspx?AspxAutoDetectCookieSupport=1&tipoConteudo=04BIflQt1aY%3D), publicada 03/06/2026; o portal lista a v1.40 de leiaute e schema. | Não há domínio de IBS/CBS no snapshot fiscal. Afeta `FiscalModels.cs`, `FiscalValidation.cs`, `FiscalXml.cs`, tela fiscal, fixtures e schemas. | Necessária análise de aplicabilidade por operação e contador. Não inserir tags vazias nem declarar cálculo automático. |
| DANFE simplificado tipo 2 | [NT 2026.003 v1.00](https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx/listaConteudo.aspx?AspxAutoDetectCookieSupport=1&tipoConteudo=04BIflQt1aY%3D), publicada 25/05/2026. | `FiscalPdf.cs` produz DANFE convencional; o piloto não declarou operação abrangida pelo tipo 2. | Sem alteração funcional agora; registrar como condicional e revisar quando o perfil a exigir. |
| Assinatura/provedor NF-e | [NT 2026.001 v1.02a](https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx/listaConteudo.aspx?AspxAutoDetectCookieSupport=1&tipoConteudo=04BIflQt1aY%3D), publicada 23/06/2026. | `FiscalCertificateVault.cs` e `FiscalXml.cs` usam A1/XMLDSIG local. | Não substituir A1/XMLDSIG sem analisar a aplicabilidade do PAA ao emissor piloto; revisar SHA-256, canonicalização e correlação de certificado em testes. |
| Serviços NF-e SP | [SEFAZ-SP — URLs Web Services](https://portal.fazenda.sp.gov.br/servicos/nfe/Paginas/URL-WEBSERVICES.aspx/url_webservices.asp), consultada 19/09/2026: Autorização, retorno, consulta, evento e inutilização 4.00 para homologação e produção. | `FiscalGateway.cs` contém seleção de endpoint. | Revisão necessária: centralizar catálogo de endpoints por ambiente/autorizador, manter produção bloqueada e testar somente composição/localmente. |
| NFS-e Nacional, produção | [Portal NFS-e — Documentação atual](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual), atualizado 15/08/2026: `NFSe-ESQUEMAS_XSD-v1.01-20260209`, Anexo I DPS v1.01-20260209, eventos v1.01-20260122, NBS v1.01-20260122 e IndOp IBS/CBS v1.01. | `Schemas/Nfse` já nomeia 1.01, mas a procedência/pacote não está registrada e faltam anexos de domínio. Afeta `FiscalXml.cs`, `FiscalGateway.cs`, `FiscalPdf.cs` e testes. | Obter e conferir hash/conteúdo do ZIP oficial; atualizar somente arquivos que divirjam. Sem habilitar grupos RTC na produção sem regra de aplicabilidade. |
| NFS-e Nacional, produção restrita | [Portal NFS-e — produção restrita](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/producao-restrita), atualizado 28/07/2026: `NFSe-ESQUEMAS_XSD-PRODREST-v1.01-20260727`, layout com grupos IBS/CBS. | O mesmo namespace/versão nominal não prova equivalência ao pacote produtivo. | Alteração necessária: armazenar pacote restrito como referência separada e usar somente no ambiente correspondente. |
| IBS/CBS, PIS/COFINS e arredondamento NFS-e | [NT SE/CGNFS-e 007](https://www.gov.br/nfse/pt-br/noticias/publicada-nota-tecnica-se-cgnfs-e-no-007-com-atualizacoes-e-esclarecimentos), publicada 07/02/2026; informa IBS/CBS, IndOp, PIS/COFINS, arredondamento e tolerância, disponíveis em produção e produção restrita desde 09/02/2026. | `FiscalModels.cs`, validação, XML DPS e preparação não representam estes grupos. | Necessária análise modelada: origem, condição, cálculo e validação aprovados pela contabilidade antes de domínio/frontend/XML. |
| Simples Nacional | [Portal NFS-e — documentação atual](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual) e leiautes vigentes; perfil piloto também exige confirmação contábil. | `FiscalXml.cs` já inclui informações complementares; perfis de produto/serviço e regras de tributos devem ser confrontados com os anexos vigentes. | Revisar e cobrir por fixtures; não presumir que o Simples dispensa novos grupos RTC. |
| Cancelamento e eventos | NFS-e: Anexo II PEDREGEVT/EVT v1.01-20260122 na [documentação atual](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual). NF-e: serviço Recepção de Evento 4.00 na [SEFAZ-SP](https://portal.fazenda.sp.gov.br/servicos/nfe/Paginas/URL-WEBSERVICES.aspx/url_webservices.asp). | `FiscalEmissionService.cs`, `FiscalGateway.cs`, `FiscalXml.cs`, `FiscalInutilizationsController.cs` e schemas de eventos. | Conferir contra os pacotes oficiais baixados; preservar XML original, identificação/protocolo e estado pendente em qualquer incerteza. |
| Consulta de eventos NFS-e | [Manual de APIs ADN](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual/manual-contribuintes-apis-adn-sistema-nacional-nfse.pdf), v1.0 de 12/02/2026: `GET /NFSe/{ChaveAcesso}/Eventos`; Swagger de produção restrita em `adn.producaorestrita.nfse.gov.br/contribuintes/docs/index.html`. | A consulta pós-timeout em `FiscalGateway.cs` precisa obter o histórico completo de eventos no ADN, e não inferir cancelamento por rota Sefin/código fixo. | Implementada a rota oficial de produção restrita e consulta do histórico completo. O endpoint ADN de produção fica bloqueado até fonte oficial direta; os gates de produção permanecem cumulativos. |
| Inutilização NF-e | [SEFAZ-SP — NfeInutilizacao 4.00](https://portal.fazenda.sp.gov.br/servicos/nfe/Paginas/URL-WEBSERVICES.aspx/url_webservices.asp). | `FiscalInutilizationsController.cs`, `FiscalEmissionService.cs`, `inutNFe_v4.00.xsd`. | Manter lease, faixa reservada, consulta/recuperação e confirmação por campos estruturados. Verificar schema no pacote `010e`. |
| DANFE/DANFSe | NF-e: [NT 2026.003](https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx/listaConteudo.aspx?AspxAutoDetectCookieSupport=1&tipoConteudo=04BIflQt1aY%3D); NFS-e: manuais e anexos na [documentação atual](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual). | `FiscalPdf.cs` ainda é centralizado. | Revisão necessária. O PDF fiscal só pode ser derivado de `AuthorizedXml`; testes devem extrair e confrontar chave, protocolo, emitente/tomador, totais e situação. |

## Regras de implementação decorrentes

1. O schema ativo deve ser identificado por pacote, versão, data, URL e hash no
   repositório. Pacotes de produção e produção restrita não são intercambiáveis.
2. A atualização de schema será incremental e acompanhada da adaptação do XML, da
   validação de domínio e de fixtures; XML XSD-válido não será marcado como autorizado.
3. `Fiscal:ProductionEnabled`, a allow-list `Fiscal:HomologatedTenants` e
   `Tenant.FiscalProductionReleased` permanecem controles cumulativos. Nenhum deles
   será alterado por esta auditoria.
4. Timeouts, HTTP inválido, resposta de outra chave/digest e processamento inconclusivo
   permanecem em estado pendente; a consulta precede qualquer reenvio.
5. IBS/CBS, CNPJ alfanumérico e mudanças de PIS/COFINS serão modelados apenas com
   regra de preenchimento, origem de dado e validação explicitamente determinadas.

## Próximas verificações técnicas autorizadas

- Baixar os pacotes diretamente dos portais oficiais para diretório temporário,
  registrar SHA-256 e comparar a árvore XSD com o baseline atual.
- Criar uma camada versionada de seleção de schemas e testes que validem XML contra o
  pacote correto.
- Auditar assinatura, gateways e PDFs sem transmitir documentos nem alterar liberações
  de produção.

## Evidência de pacote adicionada após a auditoria

O ZIP NFS-e produtivo foi baixado diretamente do Portal Nacional em 19/09/2026 e
comparado byte a byte: todos os dez XSDs ativos correspondem a `Schemas/1.01`.
O teste `Nfse_production_schema_package_matches_official_20260209_release` fixa os
SHA-256 individuais para impedir alteração silenciosa. Em 20/09/2026, a nova consulta
ao Portal NF-e com o cookie exigido pelo próprio site permitiu baixar diretamente
`PL_010e_v1.02` (SHA-256
`d44ae5aa6a0d1cabf6235d2d2d47b75be5dd87bc6b90a7ec3dcec99c3d41bda1`). A consulta
também revelou que ele foi substituído pelo `PL_010f_v1.04`, publicado em 31/08/2026
(SHA-256 `b8589490a58a09a993a80e6ac4d7ed10f20892061ecfc56719337098d4b95998`).

Os cinco XSDs do 010e e do 010f foram comparados ao baseline local. O 010e altera
estruturas RTC/IBS-CBS e CNPJ alfanumérico; o 010f altera novamente
`DFeTiposBasicos_v1.00.xsd` e `leiauteNFe_v4.00.xsd`. Os ZIPs não incluem os schemas
de inutilização locais. Nenhum XSD ativo foi trocado: a migração deve partir do 010f
e permanece condicionada à adaptação conjunta de modelo, XML, eventos/inutilização,
assinatura e testes.
