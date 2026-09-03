# Frontend Sport Pneus

Aplicação Angular 21 standalone com PrimeNG 21, PrimeIcons e Tailwind CSS 4. A ADR 0005 na raiz do projeto registra a escolha da linha MIT e impede atualização automática para PrimeNG 22.

## Desenvolvimento

Com as dependências instaladas, execute:

```bash
npm start
```

O servidor abre em `http://localhost:4200` e encaminha `/api` para `http://localhost:5154` por meio de `proxy.conf.json`.

## Validação

```bash
npm run lint
npm test
npm run build
```

Em ambientes sem Node no host, use a imagem oficial Node 24 conforme os comandos registrados na documentação principal. O build possui atualmente um aviso não bloqueante de orçamento do bundle inicial.
