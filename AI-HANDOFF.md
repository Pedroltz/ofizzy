# Continuidade do Ofizzy

Atualizado em **15/10/2026**, branch `develop`. Fase ativa: [Fase 8](docs/phases/PHASE-08-FISCAL.md). O piloto continua sem homologação externa.

## GitHub e documentação — 15/10/2026

README visual e cinco capturas com API/PostgreSQL reais e dados fictícios. [Execução](docs/GETTING-STARTED.md), [reprodução das imagens](docs/assets/README.md) e [configuração remota](docs/GITHUB.md) documentadas. Código público com direitos reservados por escolha do usuário; não adicionar MIT por suposição.

`main` exige PR + checks `backend`, `frontend`, `compose`, sem aprovação de outro colaborador. `develop` continua padrão com escrita direta. Recursos de segurança/relato privado habilitados; Dependabot semanal e CI em pushes de `develop`. Não renomear checks sem atualizar a proteção. Nenhum deploy foi disparado.

Validação local: backend 100 unitários + 12 integrações; frontend lint/72 testes/build; E2E 69 passaram inicialmente e quatro timeouts passaram na repetição, 11 skips. Compose/migrations/smoke Nginx e persistência após restart aprovados. Schema hashes protegidos com `-text` no Git, sem mudar XSDs. No Windows, User Secrets interferiam nas integrações; usar perfil APPDATA vazio por processo, conforme o guia, sem apagar secrets.

Limites registrados: bundle 795,40 kB; controles compactos preexistentes abaixo de 44 px nas listagens, inclusive mobile/tablet; nenhuma alteração de CSS nesta entrega. Ausência de overflow conferida em 1440/768/320/390 px. Resultados detalhados em [STATUS](docs/STATUS.md). Evidências temporárias em `test-results/github-demo/` são ignoradas; não versionar sessões, credenciais ou traces.

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
