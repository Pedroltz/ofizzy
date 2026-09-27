# Fiscal — operação e continuidade

Atualizado em **27/09/2026**. Estado: fluxos locais implementados; homologação externa do piloto ainda pendente.

## Ambientes e gateway

`NationalFiscalGateway` contém integração direta NF-e/NFS-e. Em `Development`, `Fiscal:SimulateGateway=true` seleciona `DevSimulatedFiscalGateway`.

Homologação real deve usar ambiente/configuração isolados, simulador desligado e credenciais reais. PFX, senha, chave AES, JWT, cookies e dados fiscais reais nunca devem ser versionados.

## Gates de produção

A implementação exige cumulativamente `Fiscal:ProductionEnabled=true`, tenant em `Fiscal:HomologatedTenants` e `Tenant.FiscalProductionReleased=true`.

A liberação persistida não contorna o servidor. Allow-list vazia bloqueia. `Origin` distingue simulação, oficial e desconhecido; operações com origem incompatível são recusadas. Legados sem evidência positiva permanecem desconhecidos até reconciliação.

## Schemas

- NF-e nova: `PL_010f_v1.04` em `Schemas/Nfe010f`.
- NF-e histórica: pacote persistido no documento; não reinterpretar.
- Eventos RTC NF-e: pacote separado em `Schemas/NfeEventsRtc`.
- NFS-e: pacote nacional 1.01 em `Schemas/Nfse`.

Schema válido não equivale a autorização fiscal.

## Reforma Tributária

O cronograma oficial coloca os documentos fiscais do **Simples Nacional** em obrigatoriedade RTC em **01/01/2027**. Até esse marco, concluir domínio, cálculo, snapshot e XML dos grupos aplicáveis, usando regras fornecidas pela contabilidade.

A NFS-e Nacional colocou tratamento de CNPJ alfanumérico em produção em 10/08/2026. O Ofizzy valida/preserva letras no cadastro, busca, contratos e SAN, mas ainda bloqueia emitente alfanumérico e tomador NFS-e pelas cadeias de schemas remanescentes. Não declarar suporte fiscal completo.

No cronograma nacional, ME/EPP do Simples passam a usar obrigatoriamente o Emissor Nacional em 01/11/2026; Igaraçu do Tietê comunicou migração local dessas empresas desde 01/08/2026.

## NFS-e e ADN

A documentação nacional mantém manuais para emissão e ADN. ADN produtivo: `https://adn.nfse.gov.br/contribuintes`. Sefin homologação: `https://sefin.producaorestrita.nfse.gov.br/API/SefinNacional`; produção: `https://sefin.nfse.gov.br/SefinNacional`. Conferidos no [catálogo oficial](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/apis-prod-restrita-e-producao). Acessibilidade e respostas com A1 real ainda exigem homologação externa.

## Split Payment

A Plataforma Pública possui Manual de Integração/Swagger e recebeu novo ato técnico em 23/09/2026. O primeiro incremento manual preserva recebimentos, vínculos opcionais com DF-e oficiais produtivos, liquidações, taxas, segregação e estornos. Segregação desconhecida não gera líquido presumido; cancelamento fiscal não gera devolução automática. Identificadores externos, redistribuição e integração com provedores permanecem pendentes. Integração direta só entra se houver papel técnico aplicável ao Ofizzy.

## Referências

- [Próximos passos](NEXT-STEPS.md)
- [Auditoria regulatória](fiscal/FISCAL-REGULATORY-AUDIT-2026.md)
- [Runbook de homologação](fiscal/HOMOLOGATION-RUNBOOK.md)
- [Checklist de produção](fiscal/PRODUCTION-READINESS.md)

Não habilitar produção com base apenas em build, XSD, fixture ou gateway simulado.

## IBS/CBS configurável

Em Configurações → Fiscal → Classificação fiscal do catálogo, escolha serviço/produto e vigência. Informe CST IBS/CBS, cClassTrib, base, IBS UF, IBS municipal, CBS e cIndOp quando serviço. Pode salvar rascunho incompleto sem aprovação prévia; use os valores aprovados pela contabilidade para emissão. Zero é explícito; campo vazio permanece desconhecido.

O cálculo local inicial suporta `000/000001`, base 100%, sem ST anterior; outros cenários bloqueiam emissão. Valores calculados no backend entram no snapshot e na prévia da OS. Fixtures assinadas passam nos XSDs instalados, mas transmissão RTC oficial e PDF RTC continuam bloqueados até completar schemas/aceitação/leiaute. A DPS contém classificação IBS/CBS; cálculo local não significa que a Sefin aceitará ou devolverá os mesmos valores. Em 01/01/2027, perfis do Simples sem configuração IBS/CBS bloqueiam o fluxo antigo.

## Rotas

Configurações/certificado: `GET/PUT /api/fiscal/settings`, `POST /api/fiscal/certificate` (Owner/Admin para escritas). Perfis: `GET/PUT /api/parts/{id}/fiscal` e `/api/services/{id}/fiscal` (Owner/Admin para escritas). Preparação: `GET/PUT /api/work-orders/{id}/fiscal`; emissão: `POST /api/work-orders/{id}/fiscal/issue`.

Documento: `GET /api/fiscal/documents/{id}`, `POST .../{id}/sync`, `POST .../{id}/cancel`, `GET .../{id}/xml` e `.../{id}/pdf`. ZIP: `GET /api/work-orders/{id}/fiscal/download`. Inutilização: `GET/POST /api/fiscal/nfe/inutilizations`, `POST .../{id}/sync`. Cancelamento/inutilização são administrativos. Não confundir resposta local com autorização oficial. Downloads dependem de XML autorizado persistido; PDF RTC continua indisponível. [Permissões e contratos](API.md).
