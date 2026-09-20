# Matriz de domínio RTC — NF-e 010f e NFS-e Nacional

Atualizada em 20/09/2026 a partir de fontes oficiais. Este documento não autoriza
preenchimento automático, emissão em produção ou ativação de schemas candidatos.

## Fontes verificadas

- [Portal Nacional da NF-e — Esquemas XML](https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D): `PL_010f_v1.04`, publicado em 31/08/2026, NT 2025.002 v1.50 e NT 2026.007 v1.00.
- [Portal Nacional da NF-e — eventos RTC](https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D): pacote da NT 2025.002 v1.40, publicado em 27/07/2026; ZIP conferido SHA-256 `a4c57ce95b225cd8852f90bd6c39ca28ae551636ff3f67eb2602b9fa847129b2`.
- [Receita Federal — cálculo do DV CNPJ alfanumérico](https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/documentos-tecnicos/cnpj), manual atualizado em 05/11/2024.
- [Portal Nacional da NFS-e — documentação atual](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual) e [NT SE/CGNFS-e 007](https://www.gov.br/nfse/pt-br/noticias/publicada-nota-tecnica-se-cgnfs-e-no-007-com-atualizacoes-e-esclarecimentos).

## Decisões por grupo

| Grupo/alteração | Documento e condição | Origem requerida | Domínio e XML propostos | Decisão atual |
| --- | --- | --- | --- | --- |
| CNPJ alfanumérico | NF-e 010f; somente onde o tipo RTC do layout o admitir | Cadastro do emitente/destinatário, certificado e cronograma oficial | Tipo de documento versionado; validador de DV já isolado; chave de acesso e XML apenas na migração 010f | Não ativar no 010c ou NFS-e 1.01, que continuam numéricos. |
| `CST` + `cClassTrib` | NF-e, por item, quando houver tributação IBS/CBS aplicável | Classificação fornecida/aprovada pela contabilidade | Objeto `IbsCbsTaxProfile` específico por perfil de produto, não dezenas de campos soltos em `ProductFiscalData`; `gIBSCBS` na ordem do XSD | Pendente de tabela/classificação e regra por operação. |
| `gIBSCBS` e totais IBS/CBS | NF-e 010f; grupos condicionais conforme classificação tributária | Base, alíquotas, redução, diferimento e responsabilidade definidos pela contabilidade | Cálculo no backend, snapshot fiscal imutável e geração por builder 010f | Não inserir tags vazias ou calcular por NCM sem regra aprovada. |
| Monofásico, crédito presumido, estorno, ZFM/ALC e doação | NF-e 010f; grupos condicionais e mutuamente exclusivos em partes do layout | Regime/operação e documentação contábil | Subtipos específicos de `IbsCbsTaxProfile`, com validação de exclusividade | Fora do perfil inicial de revenda interna; não modelar como flags genéricas. |
| Imposto Seletivo | NF-e 010f, conforme classificação e operação | Contabilidade e enquadramento do item | Subtipo específico, independente de IBS/CBS | Fora do perfil atual; manter ausente até aplicabilidade confirmada. |
| PIS/COFINS e arredondamento NFS-e | NFS-e Nacional, conforme NT 007 e ambiente/layout aplicável | Regime, retenções e regra de arredondamento aprovados | Perfil de serviço versionado e cálculo backend; DPS gerada pelo builder do layout correspondente | O perfil piloto atual não tem retenção/dedução; não alterar sem confirmação. |
| IBS/CBS/IndOp NFS-e | Produção restrita e regras nacionais aplicáveis | Município, regime e contabilidade | Seleção de layout por ambiente e objeto fiscal de serviço | Não ativar o pacote restrito no ambiente produtivo padrão. |

## Critérios de entrada para implementação

1. A contabilidade deve fornecer classificação, cenários, alíquotas, arredondamento,
   responsabilidade e documentos de exemplo para cada operação suportada.
2. Cada cenário precisa de fixture de entrada, snapshot persistido, XML esperado e
   validação contra o pacote oficial correspondente.
3. A versão do schema precisa estar registrada no documento fiscal antes da assinatura.
4. A emissão só selecionará o 010f quando domínio, XML, eventos/inutilização e
   homologação daquele cenário estiverem concluídos; documentos existentes preservam
   o pacote com que foram gerados.
