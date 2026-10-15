# API

Prefixo `/api`, JSON camelCase e erros `application/problem+json`.

## Fase 1

- `GET /api/setup/status`
- `POST /api/setup`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /health/live`
- `GET /health/ready`

Requisições mutáveis exigem `X-XSRF-TOKEN`, obtido pelo cookie legível `XSRF-TOKEN`. Tokens de sessão nunca são retornados no corpo.

## Fase 2

- CRUD e arquivamento em `/api/customers`, `/api/vehicles`, `/api/services` e `/api/parts`.

## Fase 3

- `GET /api/work-orders?q=&status=&page=&pageSize=`
- `GET /api/work-orders/{id}`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}` para OS aberta ou em andamento
- `PATCH /api/work-orders/{id}/status` com `Open`, `InProgress`, `Completed` ou `Cancelled`

O número é sequencial por tenant no PostgreSQL. Dados do cliente, veículo e itens são preservados como snapshots; subtotais e total são calculados pelo backend. OS finalizada ou cancelada é imutável.

## Fase 4

- `GET /api/company`
- `PUT /api/company`
- `GET /api/work-orders/{id}/pdf`

Os dados da oficina alimentam a impressão e o PDF. O PDF é devolvido como `application/pdf` com nome `OS-NNNN.pdf`.

## Dashboard

- `GET /api/dashboard/summary`

Retorna totais de clientes, veículos, ordens ativas/finalizadas e a lista resumida de ordens ativas em uma única consulta agregada.

## Convenções de listagem

Listagens usam `q`, `page` e `pageSize`; veículos também aceitam `customerId` e ordens aceitam `status`. Respostas paginadas usam `items`, `total`, `page` e `pageSize`.

## SaaS multi-tenant (ADR 0006)

Todas as rotas operacionais existentes conservam seus DTOs e agora isolam pelo tenant
validado da sessão. TenantId nunca é propriedade aceita em DTO operacional. ID de
recurso de outra organização resulta em 404; referência indisponível resulta em 409.
Falta de vínculo, módulo ou permissão resulta em 403. Sem sessão: 401. Entrada
inválida: 400. Conflitos de índice/FK retornam erro compreensível sem detalhes SQL.

### Identidade e seleção

- `POST /api/auth/login { email, password }`: retorna `{ id, name, email,
  isPlatformAdmin, tenant }`. `tenant` é nulo quando não existe exatamente um vínculo
  disponível. Login com único vínculo seleciona automaticamente.
- `GET /api/auth/me`: mesmo contrato. Tenant contém `id`, `name`, `slug`, `status`,
  `vertical`, `role`, `onboardingCompleted`, `modules`.
- `GET /api/auth/tenants`: organizações disponíveis do usuário, com o mesmo DTO de
  contexto, somente vínculos ativos e estados Active/Pending.
- `POST /api/auth/tenant { tenantId }`: valida vínculo no banco, revoga refresh atual
  e emite contexto selecionado. Retorna o usuário/contexto. O ID é uma solicitação de
  seleção, não autorização. PlatformAdmin também precisa de vínculo para operar dados.
- `POST /api/auth/refresh`: mantém tenant selecionado e revalida vínculo/estado;
  rotação transacional, replay revoga família. Retorna o mesmo DTO de usuário.
- Logout mantém o contrato. Cookies HttpOnly, SameSite e CSRF continuam obrigatórios.
  Faça GET após login/seleção para renovar o cookie XSRF associado à identidade.

### Administração global

Todas exigem usuário ativo com IsPlatformAdmin persistido, revalidado por request.

- `GET /api/platform/tenants`: lista metadados administrativos, sem dados operacionais.
- `GET /api/platform/tenants/{id}`: detalhes de tenant.
- `POST /api/platform/tenants`: provisionamento transacional. Corpo:
  `{ name, slug, vertical: "Automotive", adminName, email, password?, modules? }`.
  Slug minúsculo alfanumérico com hífens, até 80 caracteres. Nome até 160.
  Omissão de modules aplica template Automotive. Array vazio desabilita todos.
  Novo usuário exige senha com 10–200 caracteres, maiúscula/minúscula/número.
  Usuário existente ativo exige password nulo/omitido; senha existente nunca muda.
  Retorna 201 com `{ id, name, slug, status, vertical, onboardingCompletedAt, modules }`.
- `PUT /api/platform/tenants/{id} { status, modules }`: atualiza estado e módulos,
  registra autor/data. Active exige onboarding concluído. Enum inválido/dependência
  inválida resulta em 400; ativação prematura resulta em 409. Não há exclusão física.

Módulos disponíveis: Customers, Catalog, Automotive, WorkOrders. Automotive exige
Customers; a OS atual exige os quatro. Vertical só pode ser definida no provisionamento
nesta fase, pois somente Automotive está implementada.

### Configurações e onboarding

- `GET /api/tenant/settings`: configurações do tenant, mesmo DTO de `/api/company`.
- `PUT /api/tenant/settings`: mesmo corpo de PUT /api/company; exige Owner/Admin,
  valida entrada. GET e PUT /api/company permanecem aliases HTTP de compatibilidade,
  não entidades ou serviços duplicados.
- `POST /api/tenant/onboarding/complete {}`: Owner/Admin confirma configuração;
  exige nome operacional preenchido, grava OnboardingCompletedAt explicitamente e
  muda Pending para Active. Idempotente para organização já ativa.
- Pending pode acessar configurações/onboarding, mas não operações normais.
  Suspended/Archived não têm contexto operacional disponível.

### Bootstrap da plataforma

`GET /api/setup/status` só retorna required=true quando Platform:BootstrapEnabled
foi explicitamente habilitado e não há usuários. `POST /api/setup` cria somente o
operador global inicial, nunca uma empresa. O corpo legado SetupRequest permanece
compatível (companyName é mantido por compatibilidade, não provisiona tenant).
Desabilitado por padrão. Use somente por acesso privado durante inicialização.
Em base legada, conceda ao operador selecionado pelo comando documentado de servidor.

### Autorizações operacionais

Members podem operar clientes/catálogos/OS nos módulos habilitados; somente
Owner/Admin alteram configurações/concluem onboarding. Owners não acessam APIs de
plataforma. Dashboard requer Customers/WorkOrders/Automotive; PDFs exigem o módulo
WorkOrders e contexto Automotive atual. Numeração é independente por tenant.

## Fiscal — desenvolvimento

Contratos e permissões em [FISCAL.md](FISCAL.md#rotas). APIs exigem sessão, antiforgery nas mutações e tenant válido. Produção fiscal depende de homologação; a presença das rotas não indica autorização para emissão real.

## Contratos fiscais e recuperação — 13/09/2026

O catálogo completo de rotas está em [FISCAL.md](FISCAL.md). DTOs de preparação, configurações e perfis passam por validação no backend; TenantId não é escolhido pelo cliente. Owner/Admin gerencia configuração, certificado, perfis, cancelamento e inutilização; membros acessam preparação/emissão/download conforme permissões.

`GET /api/fiscal/nfe/inutilizations` retorna até 100 pedidos recentes do tenant; `POST /api/fiscal/nfe/inutilizations/{id}/sync` recupera o protocolo usando o XML original. Concorrência retorna 409; ID de outro tenant retorna 404. Resposta de processamento não significa autorização: a UI deve ler o estado retornado. ZIP contém somente autorizados no ambiente atual, podendo ser parcial. [Pendências](NEXT-STEPS.md).

## Financeiro manual e contratos RTC — 27/09/2026

Todos os endpoints exigem sessão/tenant e Customers, Catalog, WorkOrders. Escritas exigem antiforgery; TenantId/autor não vêm do DTO.

| Rota | Contrato / permissão |
| --- | --- |
| `GET /api/work-orders/{id}/payments` | totais/saldo, recebimentos, movimentos, vínculos e documentos produtivos elegíveis |
| `POST /api/work-orders/{id}/payments` | `requestId`, `amount`, `method`, `receivedAt`, `allocations? [{documentId, amount}]`; OS concluída |
| `POST /api/payments/{id}/settlements` | `requestId`, `amount`, `fees`, `segregatedTax?`, `settledAt`; liquidação parcial |
| `POST /api/payments/{id}/reversals` | `requestId`, `settlementId`, `amount`, `reason`, `reversedAt`; Owner/Admin |

Valores com até duas casas, datas não futuras e saldos validados no backend. Métodos: Cash, Pix, Transfer, Card, Cheque. Alocações opcionais devem somar o recebimento, sem exceder o documento oficial autorizado em produção da mesma OS. Origem Simulation/Unknown e homologação não são elegíveis. IDs de outro tenant retornam 404. Repetição com mesma chave/corpo retorna o mesmo ID; chave reutilizada com dados diferentes retorna 409. Não há rotas de edição/exclusão de movimento. Em conflito concorrente, atualizar dados e reenviar a requisição original; não gerar nova chave enquanto o resultado estiver desconhecido.

`segregatedTax=null` é desconhecido; zero é informação explícita. Saldo da OS = total bruto arredondado a centavos − liquidado + estornado; taxas/segregação não redefinem quitação bruta. Estornos preservam os valores da liquidação original. `requiresReview` sinaliza alteração fiscal/estorno; redistribuição ainda não disponível.

Perfis de produto/serviço aceitam `rtcEnabled`, `rtcCst`, `rtcClassTrib`, `rtcBasePercent`, `rtcIbsUfRate`, `rtcIbsMunicipalRate`, `rtcCbsRate`; serviços incluem `rtcOperationCode`. A vigência continua em `effectiveFrom`. Rascunhos incompletos podem ser salvos, mas não emitidos. Resumo fiscal inclui `productsRtc`/`servicesRtc`, e cada documento expõe `origin` e `canDownloadPdf`. XML disponível não implica PDF RTC disponível ou autorização externa.
