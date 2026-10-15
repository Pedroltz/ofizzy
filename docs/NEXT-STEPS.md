# Próximos passos — homologação fiscal

Atualizado em **27/09/2026**. Este é o plano canônico para levar a Fase 8 da validação local à **homologação externa real** do piloto de Igaraçu do Tietê/SP, Simples Nacional.

## Estado atual

- O gateway real existe para NF-e SEFAZ-SP e NFS-e Nacional.
- Em `Development`, `Fiscal:SimulateGateway=true` seleciona `DevSimulatedFiscalGateway`; testes e smokes locais não provam autorização pelos órgãos fiscais.
- Novas NF-e usam `PL_010f_v1.04`, mas o domínio/XML ainda não cobre de ponta a ponta os grupos RTC de IBS/CBS.
- CNPJ alfanumérico já funciona no cadastro/busca/contratos e comparação SAN; cadeia fiscal do emitente e tomador NFS-e permanece bloqueada até schemas compatíveis.
- O cronograma oficial da RTC coloca os documentos fiscais do Simples Nacional em obrigatoriedade em **01/01/2027**.
- Split Payment já possui documentação técnica oficial. Para o Ofizzy, o impacto imediato é financeiro e de conciliação; integração direta só entra se o produto assumir papel previsto no arranjo.
- H1 implementado: gates cumulativos, endpoints atualizados e origem fiscal persistida com bloqueio de incompatibilidade. Não houve homologação externa.

## H1 — bloqueios técnicos corrigidos localmente

1. Corrigir `FiscalReleaseGate` para exigir cumulativamente `Fiscal:ProductionEnabled=true`, tenant em `Fiscal:HomologatedTenants` e `Tenant.FiscalProductionReleased=true`.
2. Atualizar o catálogo de endpoints NFS-e/ADN de produção conforme a documentação oficial vigente.
3. Impedir que evidência do simulador seja confundida com autorização externa.
4. Cobrir combinações de gate e seleção de endpoint com testes.

**Aceite:** produção não pode ser liberada por uma única trava; gateway real e simulador ficam inequivocamente separados.

## H2 — adequar RTC e CNPJ alfanumérico

1. Campos editáveis por vigência e rascunhos já disponíveis nas configurações. Confirmar com a contabilidade os cenários suportados, `CST`, `cClassTrib`, alíquotas, reduções, diferimentos e regras aplicáveis ao piloto.
2. Domínio/cálculo/snapshot implementados para tributação integral `000/000001`, base 100%, sem ST anterior. Ampliar somente com cenários/fixtures definidos; não inferir por NCM.
3. Fixtures NF-e/DPS com IBS/CBS passam nos XSDs instalados; completar pacote NFS-e compatível, resposta autorizada e PDF antes de remover o bloqueio de transmissão RTC oficial.
4. Concluir CNPJ alfanumérico no emitente, chave, eventos/inutilização, DPS/NFS-e e consultas com pacotes oficiais compatíveis; cadastro já preserva letras e DV.
5. Manter documentos históricos ligados ao pacote de schema usado na emissão.

**Aceite:** fixtures do perfil piloto validam no pacote oficial correto e cenários não suportados são recusados claramente.

## H3 — resistência a falhas e documentos auxiliares

Exercitar rejeição/correção, timeout seguido de consulta, concorrência de numeração, reinício com lease, falha de persistência após resposta, cancelamento, inutilização e recuperação. DANFE/DANFSe devem ser confrontados com o XML autorizado.

**Aceite:** sem duplicidade, reutilização indevida de número ou perda de rastreabilidade.

## H4 — preparar credenciais reais

Validar com oficina e contabilidade CNPJ, IE/IM, endereço, regime, credenciamento, A1, série exclusiva, numeração inicial e classificações fiscais. Segredos nunca entram no Git ou em logs.

## H5 — homologação externa

Executar com simulador desligado: NF-e normal; NF-e ST se aplicável; NFS-e; OS mista; rejeição/correção; timeout/consulta; cancelamento NF-e/NFS-e; inutilização/recuperação NF-e; XML/PDF/ZIP; persistência após reinício.

**Aceite:** evidências sanitizadas do autorizador, protocolos coerentes e aceite da contabilidade.

## H6 — piloto produtivo controlado

Somente após H1-H5 e a checklist de produção: liberar globalmente, permitir apenas o tenant homologado, registrar responsável/série/evidências e concluir a adequação RTC antes de **01/01/2027**.

## Split Payment

Tratar inicialmente como frente de pagamentos/conciliação. Incremento manual implementado: recebimento, vínculos opcionais DF-e oficiais produtivos, liquidação parcial, segregação nula ou informada e estorno administrativo. Falta redistribuição/conciliação após estorno, referências externas, painel financeiro e importação de provedores. Não acoplar o fiscal a um PSP específico sem requisito real.

## Encerramento de cada incremento

Executar os checks de `AGENTS.md` e `docs/TESTING.md` proporcionais ao risco e registrar evidências reais em `STATUS.md`, `AI-HANDOFF.md`, `IMPLEMENTATION-LOG.md` e `phases/PHASE-08-FISCAL.md`.
