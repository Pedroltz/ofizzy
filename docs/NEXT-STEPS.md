# Próximos passos de desenvolvimento

Planejamento consolidado em 13/09/2026. Referência do estado entregue: [STATUS](STATUS.md). A ordem funcional é concluir a [fase fiscal](phases/PHASE-08-FISCAL.md), depois financeiro; acabamento necessário à segurança e operação acompanha a fase ativa.

## 1. Atualizar e validar os leiautes fiscais

**Pode avançar no repositório.** A auditoria de 19/09/2026 está em
`fiscal/FISCAL-REGULATORY-AUDIT-2026.md`: os XSD NFS-e produtivos 1.01 já conferem
byte a byte com o pacote oficial 20260209; o baseline NF-e `010c` continua antigo.
Em 20/09/2026, o Portal serviu os ZIPs oficiais e revelou que o `010e_v1.02` foi
substituído pelo `PL_010f_v1.04`. A referência, origem e hashes estão em
`BusinessCore/Fiscal/Schemas/Nfe/README.md`. Migrar a partir do 010f, revisando
geração/validação e eventos/inutilização; não atualizar somente o XSD sem adaptar o XML.
Usar `fiscal/RTC-DOMAIN-MATRIX.md` como critério de entrada: IBS/CBS e CNPJ
alfanumérico só avançam para domínio/XML após regras e exemplos da contabilidade.

Aceite: fixtures de NF-e normal/ST, DPS, cancelamentos e inutilização validadas; casos incompatíveis recusados com erro compreensível; fontes oficiais e diferenças documentadas. Validar XSD local não encerra homologação.

## 2. Concluir DANFE e DANFSe

**Pode avançar em paralelo à revisão de schemas, com validação final após ela.** Revisar `FiscalPdf` contra os manuais oficiais: campos obrigatórios, chave/protocolo, identificação do ambiente, código de barras/QR, totais, paginação e legibilidade. Distinguir PDF operacional da OS de documento auxiliar fiscal.

Aceite: exemplos fictícios curtos e multipágina inspecionados, conteúdo consistente com XML autorizado, validação dos códigos e revisão do leiaute. Testar somente cabeçalho PDF não basta. Conferência externa entra no passo 5.

## 3. Completar resistência a falhas e operação

**Pode avançar com PostgreSQL e gateways de teste.** Ampliar cenários relevantes: rejeição e correção com preservação da identidade, numeração concorrente, queda com lease ativo/expirado, falha de persistência após resposta oficial, rotação de chave/A1 e recuperação de backup. Revisar retomada de consulta ao reabrir a OS e feedback de erros na interface.

Aceite: nenhuma autorização duplicada, intervalo liberado indevidamente ou acesso entre tenants; XML original preservado na incerteza; retomada auditável após restart. Verificar desktop, tablet e celular com controles de 44 px. Separar evidência de API interceptada, integração real com gateway substituído e smoke pelo Nginx.

## 4. Preparar a oficina piloto

**Depende de dados e acesso externos.** Município e regime já confirmados: Igaraçu do Tietê/SP, Simples Nacional, IBGE 3520004. Validar CNPJ, IE/IM e endereço fiscal, credenciamento nos emissores, A1 válido, classificações e parâmetros fornecidos pela contabilidade. Usar canal protegido para segredos; nunca anexá-los ao repositório ou logs.

Definir série exclusiva de NF-e, numeração inicial e parâmetros de DPS antes de coexistir com outro emissor. O sistema não importa numeração legada. Configurar chaves externas versionadas e restauração segura; produção permanece desabilitada.

Aceite: checklist cadastral validado pela oficina/contabilidade e acesso ao ambiente oficial de homologação disponível. Não depender de contratação de intermediário pago. Certificado, infraestrutura e manutenção continuam dependências; ausência de tarifa de API não garante custo operacional total zero.

## 5. Homologar os fluxos oficiais

**Depende dos passos 1–4 e dos órgãos fiscais.** Executar emissão de serviços, produtos normais e ST admitidos pelo perfil, OS mista, autorização parcial, rejeição/correção, timeout/consulta, cancelamento e inutilização/recuperação. Confirmar aceitação de XML, protocolo e documentos auxiliares junto às fontes oficiais e à contabilidade.

Aceite: evidências sanitizadas, ambiente e versões registrados, protocolos compatíveis, retomada sem duplicidade e persistência pelo Nginx após reinício. Não usar dados de testes como autorização oficial. Pendências externas impedem declarar a fase homologada.

## 6. Liberar piloto controlado

**Somente após homologação comprovada e decisão de liberação.** Configurar `Fiscal:ProductionEnabled` e permitir apenas o tenant homologado. Confirmar HTTPS, backup restaurável, custódia das chaves, monitoramento sem XML/certificado em logs e procedimento para indisponibilidade oficial.

Aceite: liberação registrada, fluxo acompanhado e conciliação de documentos confirmada. Restaurar banco antigo exige reconciliar documentos e sequências com os órgãos fiscais antes de voltar a emitir; não reutilizar números por terem desaparecido do backup.

## Depois da prioridade fiscal

| Frente | Próxima entrega e aceite |
| --- | --- |
| [Fase 5 — Financeiro](phases/PHASE-05-FINANCIAL.md) | Recebimentos/estornos por tenant, saldo no backend e histórico; impedir recebimento acima do saldo. Pagamento informado na NF-e não equivale a lançamento financeiro. |
| [Fase 6 — Acabamento](phases/PHASE-06-POLISH.md) | Exercício de backup/restauração, observabilidade, UX e redução/justificativa do bundle de 780,45 kB. |
| Evolução SaaS | Convites, autosserviço e assinaturas somente com escopo próprio; provisionamento existente permanece base. |
| Backlog | Estoque, venda avulsa, outros perfis fiscais/municípios, PWA e WhatsApp dependem de priorização. Não estão implementados nem são condição automática do piloto atual. |

## Como encerrar cada incremento

Preservar arquitetura e UI atuais; executar os checks previstos em [AGENTS.md](../AGENTS.md) e [TESTING.md](TESTING.md), incluindo migration/Compose/smoke quando aplicáveis à entrega. Registrar comandos, resultados, limitações e data em STATUS, AI-HANDOFF, IMPLEMENTATION-LOG e fase ativa. Não promover resultados anteriores a novos testes executados.
