# Frontend Ofizzy

Aplicação Angular 21 standalone com PrimeNG 21, PrimeIcons e Tailwind CSS 4. A [ADR 0005](adr/0005-primeng-21-mit.md) registra a escolha da linha MIT e impede atualização automática para PrimeNG 22.

## Desenvolvimento

Com as dependências instaladas, execute a partir da raiz do repositório:

```bash
cd src/frontend/sport-pneus-web
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

Em ambientes sem Node no host, use a imagem oficial Node 24 conforme os comandos registrados na [documentação principal](../README.md). O build possui aviso não bloqueante: bundle inicial de aproximadamente 736 kB para orçamento de 500 kB.
