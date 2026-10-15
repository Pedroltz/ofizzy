# Frontend Ofizzy

Aplicação Angular 21 standalone com PrimeNG 21, PrimeIcons e Tailwind CSS 4. A [ADR 0005](adr/0005-primeng-21-mit.md) registra a escolha da linha MIT e impede atualização automática para PrimeNG 22.

## Desenvolvimento

Com as dependências instaladas, execute a partir da raiz do repositório:

```bash
cd src/frontend/ofizzy-web
npm start
```

O servidor abre em `http://localhost:4200` e encaminha `/api` para `http://localhost:5154` por meio de `proxy.conf.json`.

## Validação

```bash
npm run lint
npm test
npx playwright install chromium
npm run e2e
npm run build
```

O Playwright valida login, setup e todas as rotas autenticadas em desktop, Pixel 7 e tablet usando APIs interceptadas. O servidor de teste utiliza `http://127.0.0.1:4300` e não altera o banco local.

Em ambientes sem Node no host, use a imagem oficial Node 24 conforme os comandos registrados na [documentação principal](../README.md). O build possui aviso não bloqueante: bundle inicial de 780,45 kB no aceite de 13/09/2026 para orçamento de 500 kB.

## Interface fiscal — 13/09/2026

`features/fiscal` integra configurações e detalhes da OS finalizada com serviços HTTP tipados e persistência no backend. Preservar PrimeNG 21, tokens e permissões. Mostrar documentos separados na OS mista, erros por campo, resultados inconclusivos e autorização parcial. A tela administrativa permite confirmar intervalos, ver histórico e recuperar inutilização sem perder os campos em erro.

Smoke fiscal real usa playwright.fiscal.config.ts, Nginx na porta 18082 e PostgreSQL isolado; a suíte responsiva usa API interceptada. A verificação do novo formulário cobriu 1440/768/320 px. [Testes](TESTING.md) e [próximos passos](NEXT-STEPS.md).

## Configuração RTC e recebimentos — 27/09/2026

Configurações Fiscais expõem campos IBS/CBS nos perfis por vigência. Formulários mantêm zero distinto de vazio e aceitam rascunhos; a API valida a classificação antes de emitir. A OS concluída inclui `features/payments/work-order-payments.component.ts`, com HTTP real, recebimento, liquidação parcial e estorno administrativo. Troca de OS/tenant descarta respostas antigas; repetição de escrita após erro preserva chave/corpo/data enquanto os dados não mudarem. Segregação desconhecida não exibe líquido presumido. A API continua autoritativa para permissões e saldos.

A suíte `playwright.payments.config.ts` cobre configurações RTC e financeiro pelo Nginx, sem interceptar APIs; inclui 1440/768/320 px, sem overflow e controles >=44 px. Ela só deve usar banco descartável com dados fictícios.
