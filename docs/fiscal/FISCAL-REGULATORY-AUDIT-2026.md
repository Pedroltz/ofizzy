# Auditoria regulatória fiscal — atualização 27/09/2026

Esta é a fotografia regulatória usada para planejar a homologação do Ofizzy. Não substitui homologação externa nem orientação da contabilidade.

## Fontes oficiais verificadas

| Tema | Situação relevante |
| --- | --- |
| Cronograma RTC | documentos fiscais do Simples Nacional: obrigatoriedade em 01/01/2027 |
| NFS-e produção | CNPJ alfanumérico em produção desde 10/08/2026; evoluções IBS/CBS publicadas |
| NFS-e técnica | XSD 1.01 e anexos IBS/CBS seguem como referência nacional |
| Split Payment | Manual/Swagger autorizados; Ato Técnico Conjunto nº 5 publicado em 23/09/2026 |
| NF-e | projeto usa `PL_010f_v1.04`; revalidar NTs/pacotes antes de homologar |

## URLs oficiais

- RTC: <https://www.gov.br/receitafederal/pt-br/acesso-a-informacao/acoes-e-programas/programas-e-atividades/reforma-tributaria-do-consumo/orientacoes-da-reforma-tributaria>
- NFS-e atualizações: <https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/atualizacoes-e-implantacoes>
- NFS-e documentação: <https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual>
- NFS-e RTC: <https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/rtc>
- Atos técnicos conjuntos: <https://www.gov.br/receitafederal/pt-br/acesso-a-informacao/acoes-e-programas/programas-e-atividades/reforma-tributaria-do-consumo/legislacao/atos-tecnicos-conjuntos/>
- NFS-e e Simples (prazo nacional): <https://www.gov.br/receitafederal/pt-br/assuntos/noticias/2026/agosto/simples-nacional-nfs-e-nacional-sera-obrigatoria-para-me-e-epp-a-partir-de-1o-de-novembro-de-2026>
- Comunicado de Igaraçu do Tietê: <https://www.igaracudotiete.sp.gov.br/portal/noticias/0/3/1594/atencao-contribuintes-e-empreendedores-de-igaracu-do-tiete>
- Normas RFB: <https://normas.receita.fazenda.gov.br/>

## Impacto no Ofizzy

- NF-e 010f está selecionado, mas há domínio/snapshot e XML local integral; faltam cenários/pacote/PDF e homologação.
- NFS-e evoluiu para CNPJ alfanumérico e IBS/CBS; o domínio do Ofizzy precisa acompanhar as regras aplicáveis ao Simples.
- CNPJ alfanumérico já preserva letras em cadastro/contratos/busca/SAN; cadeia fiscal restante está bloqueada, não declarar suporte completo.
- Split Payment exige primeiro conciliação entre documento e liquidação; API direta não é pressuposto do emissor fiscal.

## Achados de implementação

1. `Development` usa gateway simulado por padrão.
2. Não há evidência de homologação externa.
3. `FiscalReleaseGate` corrigido: três travas cumulativas, com testes.
4. ADN produtivo e prefixo API da Sefin de homologação atualizados segundo catálogo oficial; uso externo ainda não comprovado.
5. RTC configurável/local integral implementado, mas envio oficial/PDF bloqueados até finalizar adequação.
6. CNPJ alfanumérico está incompleto.

Revalidar esta auditoria antes de cada rodada oficial de homologação e sempre que NF-e, NFS-e, RFB ou CGIBS publicar nova NT, schema, ato ou cronograma.
