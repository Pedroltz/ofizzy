# Plano de documentos fiscais para contabilidade

Revisão consolidada em 13/09/2026. Este plano substitui a proposta inicial de XML próprio, preservada no [arquivo histórico](docs/archive/PLANO-XML-ORIGINAL.md).

## Objetivo e decisões aceitas

Na OS concluída, o mecânico preenche os campos obrigatórios, solicita emissão e baixa os XMLs oficiais para a contabilidade. Serviços geram NFS-e; pneus e peças geram NF-e modelo 55. Uma OS mista pode ter autorização parcial e deve mostrar o estado de cada documento.

Piloto: Igaraçu do Tietê/SP, Simples Nacional. Integração direta NFS-e Nacional e NF-e SP, sem intermediário pago. A1 e infraestrutura continuam necessários. XML próprio não substitui nota fiscal e emissão manual por portal não atende ao fluxo pretendido.

## Integração com o projeto

Reutilizar monólito modular, `ApplicationDbContext`, isolamento por TenantId, DTOs e regras no backend. `BusinessCore/Fiscal` concentra emissão; `features/fiscal` integra PrimeNG 21 às configurações e detalhes da OS, sem criar navegação paralela. Preservar snapshots e documentos históricos.

## Implementado localmente

- Configuração fiscal, perfis de serviços/produtos, preparação da OS e proteção do A1.
- Numeração por tenant/tipo/ambiente/série, geração, assinatura e validação XSD.
- Emissão/consulta/cancelamento, XML/PDF e ZIP dos autorizados, estado parcial e recuperação de tentativas inconclusivas.
- Inutilização administrativa: confirmação, histórico, recuperação do XML original, protocolo correlacionado e lease persistido.
- Permissões, isolamento, testes locais e persistência pelo Nginx após reinício.

## Limites e conclusão pendente

A implementação não está homologada para produção. Schemas/NTs vigentes e DANFE/DANFSe precisam revisão, seguida de homologação externa. Produtos restringem-se ao perfil inicial de revenda interna 5102/102 ou 5405/500; serviços têm classificação única, sem retenções/deduções. Não há venda avulsa, estoque, interestaduais, contingência ou cartões neste incremento. Dados de pagamento da NF-e não implementam financeiro.

O plano continua viável como desenvolvimento direto com escopo restrito; liberação depende de conformidade e aceite externo, não apenas dos testes locais. Sem tarifa de intermediário não significa custo total garantido zero.

Próximos passos, dependências e critérios de aceite: [NEXT-STEPS.md](docs/NEXT-STEPS.md). Estado e evidências: [STATUS.md](docs/STATUS.md). Operação/fontes: [FISCAL.md](docs/FISCAL.md). Fase ativa: [PHASE-08-FISCAL.md](docs/phases/PHASE-08-FISCAL.md).
