# Fiscal — operação e continuidade

Estado: fundação e fluxos locais implementados; emissão oficial ainda não homologada.
Escopo e pendências autoritativas: [fase 8](phases/PHASE-08-FISCAL.md).

## Configuração do servidor

Usar secrets/variáveis de ambiente, nunca arquivos versionados:

- `Fiscal__ActiveKeyId`: identificador da chave ativa, por exemplo `v1`.
- `Fiscal__Keys__v1`: 32 bytes aleatórios codificados em Base64.
- `Fiscal__ProductionEnabled`: ausente/false por padrão.
- `Fiscal__HomologatedTenants__0`: UUID autorizado, somente após homologação documentada.

Preservar chaves antigas enquanto existirem certificados cifrados por elas. Backups do
banco sem as respectivas chaves não permitem recuperar os certificados. A senha de
upload não é persistida; o PKCS#12 exportado fica cifrado e é aberto apenas em memória.
No Compose, injetar essas variáveis no serviço backend por override privado ou solução
de secrets do ambiente. A stack de smoke não usa certificado real nem transmite notas.

O Owner/Admin configura a empresa e os perfis do catálogo em Configurações → Fiscal.
Na OS finalizada, membros preenchem a preparação e solicitam emissão. A API valida
antes de reservar/transmitir e separa NF-e/NFS-e. Cancelamento exige Owner/Admin.
Downloads só existem após XML autorizado persistido. Resultado inconclusivo não é
tratado como autorização nem libera edição da preparação.

## Rotas

| Rota | Operação |
| --- | --- |
| `/api/fiscal/settings` | GET/PUT administrativo |
| `/api/fiscal/certificate` | POST multipart `file`, `password` administrativo |
| `/api/parts/{id}/fiscal` | GET; PUT administrativo do perfil |
| `/api/services/{id}/fiscal` | GET; PUT administrativo do perfil |
| `/api/work-orders/{id}/fiscal` | GET/PUT preparação |
| `/api/work-orders/{id}/fiscal/issue` | POST emissão |
| `/api/work-orders/{id}/fiscal/download?includePdf=true` | GET ZIP dos autorizados no ambiente atual |
| `/api/fiscal/documents/{id}` | GET estado |
| `/api/fiscal/documents/{id}/sync` | POST consulta |
| `/api/fiscal/documents/{id}/cancel` | POST administrativo, `{reason}` |
| `/api/fiscal/documents/{id}/xml` e `/pdf` | GET documentos |
| `/api/fiscal/nfe/inutilizations` | GET/POST administrativo |
| `/api/fiscal/nfe/inutilizations/{id}/sync` | POST administrativo para recuperar o protocolo |

Migrations: `AddFiscalFoundation`, `CompleteFiscalInutilization` e `AddFiscalInutilizationLease`. O índice de documento
ativo exclui os estados Cancelled=6 e Inutilized=7. Não excluir documentos ou eventos
históricos. Escrever sempre por contexto validado, filtros e FKs compostas por TenantId.

## Schemas e fontes

Arquivos oficiais são distribuídos junto à API em `BusinessCore/Fiscal/Schemas`.
O resolver admite somente arquivos desse diretório; XMLs recebidos proíbem DTD.
O schema W3C de assinatura da NFS-e contém DOCTYPE legado: seu carregamento ignora
essa declaração sem resolver recursos externos.

- NFS-e: [pacote 1.01 de 09/02/2026](https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual/nfse-esquemas_xsd-v1-01-20260209.zip).
- NF-e: [pacote 010c/NT2022.002v1.30 da SVRS](https://dfe-portal.svrs.rs.gov.br/NFE/DownloadArquivoEstatico/?sistema=NFE&tipoArquivo=2&nomeArquivo=PL_010c_NT2022_002v1.30.zip).
  Este é o baseline incorporado, **não o pacote mais recente**. Atualização/revalidação
  das NTs vigentes permanece impeditivo de liberação em produção.
- [Serviços oficiais NF-e por autorizador](https://dfe-portal.svrs.rs.gov.br/Nfe/Servicos).

## Evidências locais em 12/09/2026

- Backend Release: build sem avisos/erros; 34 testes unitários aprovados, incluindo
  assinatura/XSD de NF-e e DPS, proteção criptográfica/tenant, CPF/CNPJ, arredondamento,
  ST, bloqueio de produção, evento de outra chave e geração de PDF.
- PostgreSQL 18: 9 integrações aprovadas na suíte completa, incluindo preparação fiscal persistida,
  acesso cruzado negado, perfis de acesso e aplicação das migrations.
- Emissão mista com gateway oficial substituído apenas no teste: timeout de NFS-e,
  consulta antes do reenvio com o mesmo XML, NF-e enviada uma única vez e ZIP de dois
  documentos. HTTP, assinatura/XSD e PostgreSQL reais; nenhuma autorização externa.
- E2E completo: 49 aprovados, 11 ignorados por viewport.
- Frontend: lint, 39 unitários e build aprovados. Bundle inicial 780,45 kB mantém
  aviso de budget 500 kB. Teste fiscal determinístico aprovado em desktop/tablet/320 px.
- Compose isolado `ofizzy-fiscal-smoke`, porta 18082: imagens construídas, nove migrations
  aplicadas em banco vazio. Smoke real sem mocks salvou configuração e preparação via
  Nginx/PostgreSQL e confirmou os dados após reinício dos quatro containers.
- Smoke real verificou configuração em 1440/768/320 px sem overflow e input com 44 px.
  Tentativa sem A1 retornou 400 e download não autorizado retornou 409.
- Não ocorreu emissão, cancelamento ou inutilização em qualquer órgão fiscal. Não há
  evidência de certificado real, homologação externa ou produção nesta sessão.

Comandos reproduzíveis:

```sh
dotnet build Ofizzy.slnx --configuration Release
dotnet test src/backend/Ofizzy.UnitTests --configuration Release
dotnet test src/backend/Ofizzy.IntegrationTests --configuration Release
# Em src/frontend/ofizzy-web:
npm run lint
NODE_OPTIONS=--no-experimental-webstorage npm test -- --watch=false
npm run build
npm run e2e -- --workers=1
npm run e2e -- --config playwright.fiscal.config.ts
# Após reiniciar somente a stack isolada:
OFIZZY_VERIFY_RESTART=1 npm run e2e -- --config playwright.fiscal.config.ts
```

O teste real exige banco isolado e bootstrap habilitado. Credenciais fictícias do teste
não devem ser usadas em produção. HTTP local usa Development; produção exige HTTPS.
Os testes reais têm trace desligado e persistem somente IDs em `/tmp/ofizzy-fiscal-live.json`.

## Recuperação de inutilização — 13/09/2026

Em Configurações → Fiscal → Histórico e inutilização NF-e, o administrador pode
solicitar uma faixa de até 100 números reservados/rejeitados e recuperar pedidos
pendentes. A confirmação mostra o intervalo antes de transmitir. Rejeições explícitas
permitem nova solicitação; resultados inconclusivos mantêm a reserva.

A recuperação reenvia exatamente o XML assinado original, sem reservar novos números.
Retorno 102 ou 563 só conclui o pedido com protocolo e CNPJ/UF/ambiente/ano/modelo/série/
intervalo correspondentes. Não extrair protocolo de texto livre. Resposta divergente
ou sem protocolo conserva o estado Pending. Timeout/falha após envio também conserva
Pending. A lease expira em dois minutos após queda; consultas simultâneas recebem 409.

Schemas de eventos são isolados dos schemas da NF-e para evitar colisão entre tipos:
- [Pacote oficial de cancelamento SVRS](https://dfe-portal.svrs.rs.gov.br/NFE/DownloadArquivoEstatico/?sistema=NFE&tipoArquivo=2&nomeArquivo=Evento_Canc_PL_v1.01_NT_2018_004.zip), pasta `NfeEvents`.
- [Schema oficial de inutilização](https://dfe-portal.svrs.rs.gov.br/Schemas/PRNFE/inutNFe_v4.00.xsd) e [leiaute](https://dfe-portal.svrs.rs.gov.br/Schemas/PRNFE/leiauteInutNFe_v4.00.xsd).
- Retorno de protocolo em duplicidade documentado na [NT 2015.002](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=FACIWTc5Dso%3D).

Evidências desta continuação: build Release sem warnings; 40 unitários backend e
9 integrações, incluindo timeout de inutilização, XML idêntico no reenvio, lease
concorrente, protocolo 563 e preservação do histórico. Frontend lint,39 unitários,
build,52 E2E aprovados/11 skips. XSD de cancelamento NF-e/NFS-e aprovado; faixa
incompatível não é reconhecida como inutilizada. Nenhuma transmissão a órgão fiscal.
A migration aditiva `AddFiscalInutilizationLease` foi aplicada no banco isolado existente.

Aceite final de 13/09/2026: smoke real pelo Nginx aprovado antes e após reinício
dos quatro containers da stack isolada; configuração/preparação preservadas e
faixa não reservada rejeitada sem transmissão. Teste adicional confirmou 404 ao
tentar recuperar pedido de outro tenant e histórico vazio para essa organização.

## Simulação de desenvolvimento local — 13/09/2026

Para possibilitar o teste completo de emissão, geração de DANFE/DANFSe em PDF e
cancelamento antes da aquisição do certificado ICP-Brasil pago, foram implementados:
- `DevSimulatedFiscalGateway`: gateway simulado ativado exclusivamente quando
  `builder.Environment.IsDevelopment()` é verdadeiro E `"Fiscal:SimulateGateway": true`.
  Simula retornos autorizados no formato oficial da SEFAZ (nfeProc com protNFe 135260000000001)
  e Sefin Nacional sem efetuar chamadas externas de rede.
- `FiscalDevController`: rota `GET /api/fiscal/dev/certificate` exclusiva de `Development`
  que gera um certificado PKCS#12 (.pfx) autoassinado com chave RSA 2048 bits e extensão
  ICP-Brasil `2.16.76.1.3.3` preenchida com o CNPJ cadastrado, permitindo testar o fluxo
  real de upload e proteção AES-GCM em `Configurações > Fiscal`.
- Transição para homologação/produção: em `Production`, o endpoint retorna `404` e o gateway
  simulado é fisicamente substituído por `NationalFiscalGateway`.

## Estado consolidado e próxima entrega — 13/09/2026

A recuperação administrativa de inutilização, as regras do Simples Nacional, os documentos
oficiais DANFE/DANFSe com QuestPDF e o gateway simulado de desenvolvimento estão concluídos localmente.
O aceite mais recente: 48 unitários backend, 9 integrações com PostgreSQL 18 real, 39 unitários frontend,
52 E2E e 11 skips, dez migrations e smoke após restart.

A próxima etapa concentra a coleta de credenciais da oficina piloto para homologação externa
oficial nos webservices da SEFAZ-SP e do Sefin Nacional. A sequência com dependências e
critérios está em [NEXT-STEPS.md](NEXT-STEPS.md).
