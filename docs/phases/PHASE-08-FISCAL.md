# Fase 8 — Documentos fiscais de serviços e produtos

Estado em **24/09/2026**: **implementação local avançada, ainda sem homologação externa**. Piloto: Igaraçu do Tietê/SP, Simples Nacional, IBGE 3520004.

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
- NF-e novas em `PL_010f_v1.04`; NFS-e Nacional 1.01 versionada no projeto.

## Limite das evidências locais

Em `Development`, `Fiscal:SimulateGateway=true` seleciona `DevSimulatedFiscalGateway`. Builds, testes e smokes locais provam comportamento da aplicação e persistência, mas **não provam autorização por SEFAZ, Sefin Nacional ou município**.

## Bloqueios antes da produção

- `FiscalReleaseGate` permite `Tenant.FiscalProductionReleased=true` antes de verificar `Fiscal:ProductionEnabled` e `Fiscal:HomologatedTenants`.
- O código ainda bloqueia `NfseAdnBase(Production)`.
- O domínio/XML ainda não representa de ponta a ponta `CST`, `cClassTrib`, `gIBSCBS` e totais IBS/CBS.
- O cálculo de DV de CNPJ alfanumérico existe isoladamente.
- Faltam credenciais reais, credenciamento, série exclusiva, classificações aprovadas pela contabilidade e execução nos ambientes oficiais.

O cronograma oficial coloca os documentos fiscais do **Simples Nacional** na RTC em **01/01/2027**. É possível homologar o fluxo 2026 antes desse marco, mas a continuidade para 2027 depende da adequação RTC.

## Critério de homologação

A fase só pode ser marcada como homologada após evidência sanitizada de NF-e/NFS-e autorizadas, OS mista, rejeição/correção, timeout/consulta, cancelamentos, inutilização/recuperação, XML/PDF/ZIP coerentes, persistência após reinício e aceite contábil.

Ver [HOMOLOGATION-RUNBOOK.md](../fiscal/HOMOLOGATION-RUNBOOK.md) e [NEXT-STEPS.md](../NEXT-STEPS.md).

## Reforma Tributária e Split Payment

RTC pertence ao domínio fiscal e deve usar regras por vigência e snapshots. Split Payment pertence principalmente à liquidação financeira; preparar correlação documento/pagamento/segregação/estorno antes de qualquer integração direta.
