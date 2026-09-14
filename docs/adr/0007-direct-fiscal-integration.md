# ADR 0007 — Integração fiscal direta no monólito

Data: 13/09/2026. Estado: decisão aceita; implementação local em andamento, homologação externa pendente.

## Contexto

O usuário exige emissão oficial e download a partir da OS, incluindo serviços e pneus/peças, sem intermediário pago nesta etapa. Piloto confirmado em Igaraçu do Tietê/SP, Simples Nacional. Exportar XML próprio ou depender da digitação manual no portal não satisfaz esse fluxo.

## Decisão

Implementar BusinessCore/Fiscal no mesmo assembly/DbContext, com DTOs, isolamento por tenant, snapshots e regras no backend. Usar conectores diretos NFS-e Nacional e NF-e SP modelo 55, A1 cifrado com chaves externas versionadas e schemas distribuídos junto à API. Integrar componentes PrimeNG 21 às configurações e à OS concluída.

Tratar cada documento separadamente em OS mista. Preservar XML/identidade em resultado inconclusivo, consultar antes de reenviar e registrar eventos. Recuperação de inutilização usa pedido original e lease persistido. Produção exige habilitação explícita e tenant homologado.

## Consequências

Não há tarifa de intermediário contratada pelo plano, mas certificado, infraestrutura e manutenção continuam necessários. O projeto assume atualização de schemas/NTs, disponibilidade dos órgãos e validação dos PDFs. O perfil inicial é restrito; expansão fiscal, estoque e venda avulsa exigem escopo adicional. Testes locais não demonstram autorização oficial.

Fontes e contratos em [FISCAL](../FISCAL.md); dependências e aceite em [NEXT-STEPS](../NEXT-STEPS.md).
