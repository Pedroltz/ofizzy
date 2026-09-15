# Status do projeto

Atualizado em 13/09/2026. Branch de trabalho: `develop`.

A fase ativa é a [fase 8 — Fiscal](phases/PHASE-08-FISCAL.md). A implementação local inclui emissão, consulta, cancelamento, downloads e recuperação administrativa de inutilização. **Não há homologação externa nem liberação de emissão em produção.**

| Frente | Estado |
| --- | --- |
| Fases 1–4: fundação, cadastros, OS e impressão | Concluídas localmente |
| Fase 7: SaaS e isolamento por organização | Aceite local concluído; arquitetura vigente |
| Fase 8: serviços e produtos fiscais | Fluxos locais implementados; revisão fiscal e homologação pendentes |
| Fase 5: financeiro | Não iniciada; após a prioridade fiscal |
| Fase 6: acabamento | Parcial; backup/restauração e demais critérios continuam pendentes |
| PWA, estoque, outras verticais e cobrança SaaS | Backlog, sem implementação nesta entrega |

## Entrega fiscal atual

Piloto confirmado: **Igaraçu do Tietê/SP, IBGE 3520004, Simples Nacional**. Integração direta com NFS-e Nacional para serviços e NF-e SP modelo 55 para peças/pneus, sem intermediário pago. A OS mista gera documentos separados e mostra autorização parcial.

Configurações, perfis, preparação, certificado A1 cifrado, sequências, documentos e eventos persistem por tenant. A interface usa PrimeNG 21 nas configurações e na OS concluída. Inutilização tem histórico, confirmação, recuperação com XML original, validação do protocolo e lease persistido de dois minutos. Cancelamentos e inutilizações passam por XSD antes do envio.

Documentos auxiliares finalizados conforme manuais regulamentares: DANFE oficial com canhoto destacável de recebimento, chave de 44 dígitos formatada em blocos, código de barras Code 128C, grade padrão SEFAZ e cláusulas obrigatórias do Simples Nacional (`infAdic`/`infCpl`). DANFSe oficial com QR Code do padrão nacional, identificação da DPS de origem, competência e detalhamento de ISSQN e tributos federais aproximados. Ambiente de desenvolvimento local conta com `DevSimulatedFiscalGateway` e download de certificado A1 autoassinado para testes manuais no navegador sem emissor pago.

## Última validação da implementação — 15/09/2026

| Verificação | Resultado registrado |
| --- | --- |
| Backend Release | Build sem avisos/erros; 56 unitários e 10 integrações aprovados (100%) |
| Frontend | Lint aprovado (0 erros/avisos), 40 unitários Vitest e build de produção aprovados |
| E2E determinístico | 21 testes de plataforma aprovados em Desktop, Tablet e Mobile; 58 testes gerais aprovados (11 skips) |
| Responsividade | 1440/768/320 px; sem overflow e controles com altura mínima >= 44 px |
| Layout de impressão | DANFE e DANFSe validados sem conflitos de constraint no QuestPDF |
| PostgreSQL | 11 migrations no banco; migration AddTenantFiscalProductionRelease aplicada com sucesso |
| Liberação de Produção | Controle visual em `/plataforma` permitindo liberação de produção fiscal pelo operador master |

Os testes de integração substituem o gateway externo; o smoke real sem A1 valida persistência e bloqueios, sem transmissão fiscal. O ambiente de desenvolvimento dispõe de simulação local para validação de ponta a ponta na interface. Essas evidências não equivalem a autorização de órgão fiscal. O build frontend mantém aviso de bundle inicial de 780,45 kB para budget de 500 kB.

## Próxima entrega

Completar cenários adicionais de resiliência e preparar homologação com cadastro, credenciamento e A1 do piloto. Produção exige habilitação explícita no servidor e tenant homologado.

A sequência, dependências e critérios de aceite estão em [NEXT-STEPS.md](NEXT-STEPS.md). Operação e fontes: [FISCAL.md](FISCAL.md). Resultados anteriores estão no [log](IMPLEMENTATION-LOG.md) e no [status histórico](archive/STATUS-ATE-2026-09-13.md); seus números e prioridades são datados.

## Revisão independente — 13/09/2026

Revisão das alterações recentes registrada em [relatório de revisão](REVIEW-2026-09-13.md). Foram encontrados problemas na separação persistente de simulação, resposta fictícia de NFS-e, leitura de campos do DANFSe, cobertura dos testes e disponibilidade/permissões do certificado de desenvolvimento. As declarações anteriores de conformidade integral dos PDFs não constituem aceite comprovado e precisam da correção/validação descrita no relatório. Atualização de schemas/NTs e homologação externa permanecem pendentes.

Nesta revisão: backend build sem avisos/erros, 49 unitários aprovados/1 skip e 9 integrações; frontend lint/39 unitários/build e 52 E2E/11 skips. Não houve alteração de código, transmissão fiscal, reconstrução Compose ou novo smoke/restart. Check EF de modelo não executou por ausência de dotnet-ef no PATH.
