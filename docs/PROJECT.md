# Projeto Ofizzy

## Visão

Plataforma SaaS multi-tenant para gestão de empresas prestadoras de serviços,
com módulos e verticais especializadas (estratégia detalhada em [PRODUCT-STRATEGY.md](PRODUCT-STRATEGY.md)). Automotive é a primeira vertical:
Cliente → Veículo → Ordem de Serviço → Diagnóstico → Serviços/Peças → Finalização
→ Impressão. A prioridade atual é concluir a emissão fiscal da fase 8; um primeiro incremento financeiro manual prepara a conciliação.

Tenant é a organização cliente do Ofizzy. Customer é o cliente atendido por essa
organização. Uma pessoa pode participar de várias organizações por TenantUser.

## Princípios

- Monólito modular, uma API, um PostgreSQL compartilhado e um frontend principal.
- Isolamento por TenantId e autorização no backend, inclusive acesso por IDs.
- Dados históricos e snapshots preservados; cadastros são arquivados.
- Regras e cálculos críticos no backend; DTOs e validação em todos os contratos.
- PrimeNG 21, tokens visuais e operação responsiva desde 320 px.
- Fluxos reais com persistência; nenhum tenant novo exige deploy ou container.

## Escopo atual

Identidade, tenants, vínculos e papéis, configurações por tenant, módulos,
provisionamento administrativo, onboarding, clientes, Automotive/veículos,
catálogos, OS, impressão HTML/PDF e dashboard. Administração global é separada
da administração da organização. Novos usuários recebem credencial inicial pelo
operador em canal privado; não há envio automático de convite nesta fase.

## Evolução futura

Outras verticais, convites por e-mail, SaaS Subscriptions, autosserviço e cobrança
SaaS podem reutilizar o provisionamento. Não implementados agora: pagamentos SaaS,
estoque, fornecedores, agenda, WhatsApp, bancos dedicados,
domínios customizados ou outros frontends. Não haverá microserviços sem necessidade
concreta. Financeiro operacional é distinto da assinatura SaaS.

## Desenvolvimento em andamento — fiscal

Documentos fiscais foram antecipados por solicitação do usuário. A [fase 8](phases/PHASE-08-FISCAL.md) registra implementação e pendências; ainda não há homologação externa ou liberação de produção.

## Prioridades consolidadas — 27/09/2026

Fiscal usa integração direta, sem intermediário pago, para o piloto Igaraçu do Tietê/SP, Simples Nacional. Novas NF-e usam `PL_010f_v1.04`, mas ainda faltam adequação RTC/CNPJ alfanumérico de ponta a ponta, cenários/PDF RTC e homologação externa. A migração municipal para NFS-e Nacional já está em curso e o marco RTC do Simples Nacional é 01/01/2027. [Sequência e critérios](NEXT-STEPS.md).

Gates cumulativos e origem fiscal persistida foram implementados. IBS/CBS fica editável nas configurações por vigência, inclusive rascunhos. Recebimentos/liquidações/estornos manuais da OS persistem no PostgreSQL; isso não executa Split Payment automático.
