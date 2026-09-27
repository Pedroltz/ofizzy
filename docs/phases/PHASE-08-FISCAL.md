# Fase 8 — Documentos fiscais de serviços e produtos

Estado em **27/09/2026**: **implementação local avançada, ainda sem homologação externa**. Piloto: Igaraçu do Tietê/SP, Simples Nacional, IBGE 3520004.

## Objetivo

Emitir documentos fiscais oficiais a partir da OS concluída: NFS-e para serviços e NF-e modelo 55 para produtos. OS mista mantém estados independentes por documento.

## Implementado

- configuração fiscal por tenant e perfis por vigência;
- preparação fiscal persistida;
- A1 protegido com AES-GCM e revalidado antes do uso;
- sequência, snapshots e eventos persistidos;
- geração, assinatura e validação XSD de NF-e e DPS/NFS-e;
- gateway direto NF-e SEFAZ-SP/SVRS e NFS-e Nacional;
- consulta antes de reenvio após timeout;
- cancelamento e inutilização com recuperação;
- isolamento por tenant;
- XML/PDF/ZIP condicionados à autorização;
- DANFE/DANFSe derivados do XML autorizado;
- gates produtivos cumulativos, origem persistida e endpoints NFS-e corrigidos;
- IBS/CBS editável por vigência, rascunhos salvos e cálculo/snapshot/XML local integral `000/000001`;
- cadastro/busca/contratos/SAN com CNPJ alfanumérico, sem declarar cadeia fiscal completa;
- NF-e novas em `PL_010f_v1.04`; NFS-e Nacional 1.01 versionada no projeto.

## Limite das evidências locais

Em `Development`, `Fiscal:SimulateGateway=true` seleciona `DevSimulatedFiscalGateway`. Builds, testes e smokes locais provam comportamento da aplicação e persistência, mas **não provam autorização por SEFAZ, Sefin Nacional ou município**.

## Bloqueios antes da produção

- RTC oficial permanece bloqueado: completar pacote/resposta/PDF e cenários adicionais antes de transmitir IBS/CBS.
- Emitente alfanumérico e tomador NFS-e permanecem bloqueados até adequar todas as cadeias oficiais.
- Faltam credenciais reais, credenciamento, série exclusiva, classificações aprovadas pela contabilidade e execução nos ambientes oficiais.

O cronograma oficial coloca os documentos fiscais do **Simples Nacional** na RTC em **01/01/2027**. É possível homologar o fluxo 2026 antes desse marco, mas a continuidade para 2027 depende da adequação RTC.

## Critério de homologação

A fase só pode ser marcada como homologada após evidência sanitizada de NF-e/NFS-e autorizadas, OS mista, rejeição/correção, timeout/consulta, cancelamentos, inutilização/recuperação, XML/PDF/ZIP coerentes, persistência após reinício e aceite contábil.

Ver [HOMOLOGATION-RUNBOOK.md](../fiscal/HOMOLOGATION-RUNBOOK.md) e [NEXT-STEPS.md](../NEXT-STEPS.md).

## Reforma Tributária e Split Payment

RTC pertence ao domínio fiscal e deve usar regras por vigência e snapshots. Split Payment pertence principalmente à liquidação financeira; preparar correlação documento/pagamento/segregação/estorno antes de qualquer integração direta.

## Incremento local de 27/09/2026

O usuário preferiu manter classificações e alíquotas editáveis em Configurações Fiscais. A estrutura não depende de valores aprovados previamente para ser desenvolvida; emissão exige dados completos/cenário suportado. Valores zero não são inferidos a partir de campo vazio. Em 01/01/2027, perfil Simples sem IBS/CBS bloqueia o fluxo antigo. Fixture XSD não libera transmissão oficial RTC nem PDF incompleto.

Origem `Unknown`/`Simulation`/`Official` persiste em documentos e inutilizações; migration marca somente evidências positivas de simulação. Gateway incompatível não pode transmitir ou recuperar o documento. Legados desconhecidos exigem reconciliação, sem backfill presumido como oficial.

Financeiro manual da OS prepara vínculo documento/liquidação/segregação/estorno, sem executar Split Payment automático. Migrations `AddFiscalOrigin` e `AddManualPayments` aplicadas em banco descartável. Resultados locais e limitações em [STATUS](../STATUS.md) e [log](../IMPLEMENTATION-LOG.md); homologação externa permanece pendente.
