# Continuidade do Ofizzy

Atualizado em **27/09/2026**, branch `develop`. Fase ativa: [Fase 8](docs/phases/PHASE-08-FISCAL.md). O piloto continua sem homologação externa.

## Incremento implementado

- Produção exige as três travas: `Fiscal:ProductionEnabled`, tenant na allow-list e `FiscalProductionReleased`. Lista vazia bloqueia; liberação no banco não contorna o servidor.
- Documentos/inutilizações persistem `Origin` (`Unknown`, `Simulation`, `Official`). Origem desconhecida ou diferente do gateway bloqueia transmissão/consulta/recuperação. Migration classifica somente evidências positivas do simulador, nunca converte registros legados em oficiais por suposição.
- ADN produtivo usa `/contribuintes`; Sefin de homologação usa `/API/SefinNacional`, conforme catálogo oficial. Endpoint correto não comprova acesso externo.
- Configurações Fiscais permitem editar IBS/CBS por item/vigência: CST, cClassTrib, base, três alíquotas e cIndOp para serviços. Foi pedido pelo usuário que os valores fiquem configuráveis, sem aguardar aprovação contábil para desenvolver a estrutura. Rascunhos incompletos podem ser salvos; emissão exige configuração suportada e completa. Zero difere de valor não informado.
- Cenário local inicial: `000/000001`, base 100%, sem ST anterior. Backend calcula/snapshot preserva valores; fixtures assinadas passam nos XSDs instalados. NF-e gera grupos e totais; DPS envia classificação IBS/CBS. **Transmissão RTC oficial bloqueada** até adequar pacote/aceitação/PDF. PDFs RTC não são oferecidos pela UI e o backend recusa geração incompleta. Outros cenários exigem implementação específica.
- Marco do Simples em 01/01/2027 impede continuar silenciosamente com perfil sem IBS/CBS. Não há inferência tributária por NCM.
- CNPJ alfanumérico: cadastro, busca, contratos, normalização/DV e comparação SAN preservam letras. **Não é suporte completo:** emitente alfanumérico e tomador NFS-e continuam bloqueados pelos schemas das cadeias restantes. Não editar XSD oficial manualmente.
- Financeiro manual na OS concluída: recebimento, liquidação parcial, taxas, segregação informada ou desconhecida, estorno administrativo com justificativa e histórico imutável. PostgreSQL/tenant/FKs, transações serializáveis e chave de idempotência protegem as escritas. Vínculo fiscal só admite documentos oficiais autorizados em produção da mesma OS.
- Não existe integração automática com PSP ou Plataforma Pública de Split Payment. Cancelamento fiscal não devolve pagamento automaticamente. Recebimentos sem vínculo, vínculos cancelados e estornos exigem conciliação; painel mensal e redistribuição de vínculos ainda pendentes.

## Migrations

`20260926213104_AddFiscalOrigin` e `20260926213917_AddManualPayments`, aplicadas somente no banco descartável de aceite. Aplicar nos ambientes restantes antes de usar a versão. Modelo EF sem mudanças pendentes.

## Evidências e continuidade

Resultados reais estão em [STATUS](docs/STATUS.md), [log](docs/IMPLEMENTATION-LOG.md) e [TESTING](docs/TESTING.md). Não usar números históricos como nova execução.

Próximo trabalho: concluir pacote NFS-e/alfanumérico, eventos/chaves do emitente alfanumérico, cenários RTC/PDF e resiliência adicional; obter A1/credenciamento/classificações reais e executar [runbook](docs/fiscal/HOMOLOGATION-RUNBOOK.md) com simulador desligado. Nenhuma produção foi habilitada. [Plano e aceite](docs/NEXT-STEPS.md).

Edições preexistentes em `GlobalExceptionHandler.cs` e `theme-toggle.component.ts` foram preservadas; não atribuí-las a este incremento.
