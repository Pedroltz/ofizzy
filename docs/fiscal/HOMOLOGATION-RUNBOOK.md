# Runbook de homologação fiscal externa

Status: procedimento para execução futura; **nenhuma etapa deste arquivo foi executada
contra órgão fiscal**. Não usar produção nem registrar segredos em ticket, terminal,
log ou repositório.

## Pré-requisitos e controles

1. Validar com a contabilidade o CNPJ/IE/IM, endereço, regime, série exclusiva,
   classificação de produtos/serviços, regras de IBS/CBS quando aplicáveis e o
   credenciamento do piloto no autorizador.
2. Obter o A1 real por canal protegido. Fazer upload somente pela rota autenticada;
   informar a senha na requisição e confirmar que ela não foi persistida.
3. Configurar o tenant em **Homologação**. `Fiscal:ProductionEnabled` deve continuar
   `false`; não marcar `FiscalProductionReleased` e não incluir o tenant em
   `Fiscal:HomologatedTenants` nesta etapa.
4. Registrar no dossiê externo somente: data/hora, ambiente, versões dos schemas,
   request sanitizado, resposta, `cStat`, mensagem, chave, recibo/protocolo e estado
   local. Nunca registrar senha A1, material da chave AES, JWT, cookies ou PFX.

## Roteiro executável

| Caso | Ação | Evidência/resultado esperado |
| --- | --- | --- |
| NF-e autorizada | Emitir uma OS fictícia de peças dentro do perfil aprovado. | Chave e protocolo coerentes, `AuthorizedXml` persistido, estado `Authorized`; DANFE gerado a partir do XML autorizado. |
| NFS-e autorizada | Emitir uma OS fictícia de serviço conforme o credenciamento nacional/municipal. | Chave/número retornado pelo emissor, XML autorizado persistido e DANFSe derivado dele. |
| Rejeição proposital | Enviar somente um caso previamente acordado com a contabilidade/autorizador. | `cStat` e mensagem registrados; estado `Rejected`, sem tratar como autorização e sem reutilizar identidade indevidamente. |
| Correção | Corrigir a preparação permitida e tentar somente quando a consulta demonstrar que é seguro. | Novo fluxo rastreável; XML e eventos originais preservados. |
| Consulta | Consultar cada documento após envio e antes de qualquer reenvio por timeout. | A chave/digest/protocolo da resposta coincide com o documento local. |
| Timeout | Interromper a conectividade do cliente após o envio, sem fabricar resposta. | Estado `AwaitingConfirmation`; consulta antes de reenvio; não duplicar transmissão. |
| Cancelamento | Cancelar documento autorizado no prazo e motivo permitidos. | Evento/protocolo correspondente persistido; estado `Cancelled`; PDF mostra a situação. |
| Inutilização NF-e | Solicitar faixa real de homologação previamente liberada. | Faixa, CNPJ, modelo, série, ambiente e protocolo conferem; XML original e lease preservados. |
| Download | Baixar XML e PDF apenas após autorização. | ZIP contém o XML autorizado; valores/chave/protocolo do PDF conferem com XML. |

## Encerramento

Anexar evidências sanitizadas ao dossiê de homologação e obter aceite da contabilidade.
Falha, timeout ou resposta inconclusiva bloqueiam a liberação. A liberação de produção
é decisão separada e deve seguir `PRODUCTION-READINESS.md`.
