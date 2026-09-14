# Segurança

Reporte vulnerabilidades de forma privada ao responsável pela instalação. Não abra issues públicas com credenciais ou dados de clientes.

As sessões usam cookies HttpOnly/SameSite, proteção CSRF e refresh token com hash e rotação. Produção exige HTTPS, chave JWT aleatória com no mínimo 32 bytes e senhas exclusivas. Logs não devem conter senha, token, documento ou payload completo de clientes.

Testes automatizados e screenshots devem usar dados fictícios. O E2E responsivo intercepta as APIs e não deve apontar para uma base de produção nem registrar cookies, tokens ou dados reais nos artefatos do Playwright.

## Segurança multi-tenant

TenantId vem exclusivamente da sessão assinada após validação de usuário, TenantUser,
role e estado no banco. Claims não substituem autorização. Suspensão ou revogação
bloqueiam a próxima request; requests já em execução podem terminar sua transação.
Um PlatformAdmin não recebe bypass das consultas operacionais e um Owner não recebe
privilégios globais. A concessão de PlatformAdmin é persistida com timestamp e registrada
pelo comando administrativo de servidor; não há comparação de e-mail hardcoded.

Bootstrap é opt-in, somente sem usuários. Habilite em rede privada/túnel antes da
exposição pública e desabilite após criar o operador. Não use o primeiro acesso público
como mecanismo de proteção. Usuário novo provisionado recebe senha inicial por canal
privado do operador; e-mail automático e recuperação/troca autônoma são evoluções futuras.

Filtros EF cobrem todos os dados operacionais, SaveChanges protege propriedade e FKs
compostas impedem vínculos cruzados. Tabelas User/Tenant/TenantUser/TenantModule são
controle global, com acesso restrito a código de identidade/tenancy e APIs privilegiadas.
Não há IgnoreQueryFilters no código de produção. SQL bruto novo requer revisão de
isolamento; acesso direto de operador ao PostgreSQL não é isolado por RLS.

Refresh tokens armazenam apenas hash e tenant selecionado, com rotação serializada
por lock transacional. Replay revoga a família. Alteração de tenant revoga refresh
anterior e limpa cache do frontend. Não grave tokens em localStorage, logs ou fixtures.
Logs da request incluem TenantId, UserId e RequestId sem payloads pessoais.

Testes de aceite real usam stack/banco isolados, sem interceptar API. Traces estão
desativados nesse conjunto para não gravar cookies. A suíte visual determinística
continua separada e não é evidência de isolamento de banco.

## Proteção fiscal — 13/09/2026

A1 é armazenado cifrado com AES-GCM, chave externa versionada e vínculo ao tenant/keyId. A senha do upload não é persistida; leitura expõe apenas metadados. Manter chaves anteriores enquanto houver certificados dependentes e não registrar XMLs completos, senhas, tokens ou certificados em logs.

Respostas oficiais exigem correlação de identidade/protocolo; XML recebido proíbe DTD e schemas resolvem somente arquivos locais autorizados. Incerteza não autoriza liberar numeração. Produção exige Fiscal:ProductionEnabled e tenant em Fiscal:HomologatedTenants. [Operação](FISCAL.md), [restauração](BACKUP-RESTORE.md) e [próximos passos](NEXT-STEPS.md).
