# Production readiness fiscal

Status atual em 19/09/2026: **não liberado**. Esta checklist não muda configuração
nem libera tenant automaticamente.

## Bloqueios obrigatórios

- [ ] Pacotes XSD vigentes foram baixados de fonte oficial, registrados com URL, data
      e hash, instalados/selecionados por ambiente e validados contra fixtures.
- [ ] XML e assinatura foram aceitos em homologação externa para NF-e e NFS-e.
- [ ] O perfil tributário, Simples Nacional, classificações, IBS/CBS e arredondamentos
      foram aprovados pela contabilidade responsável.
- [ ] Consulta, timeout/recuperação, rejeição/correção, cancelamento e inutilização
      foram comprovados em homologação com evidência sanitizada.
- [ ] DANFE e DANFSe derivados do XML autorizado foram conferidos contra os manuais
      aplicáveis e contra a resposta do autorizador.
- [ ] A1 válido, CNPJ correspondente, custódia da chave AES, rotação e restauração
      foram exercitados sem expor senha, PFX ou chave em logs.
- [ ] Isolamento por tenant, idempotência, sequência, lease e download entre tenants
      têm testes verdes e evidência de persistência após reinício.
- [ ] Monitoramento, backup restaurável e procedimento de indisponibilidade foram
      aceitos pelo operador responsável.

## Liberação manual, cumulativa e auditável

Somente após todos os itens acima, um operador autorizado pode, em mudança separada:

1. definir `Fiscal:ProductionEnabled=true` no ambiente de produção;
2. incluir explicitamente o UUID do tenant em `Fiscal:HomologatedTenants`;
3. marcar `Tenant.FiscalProductionReleased` pela administração da plataforma;
4. registrar responsável, data, evidências e série fiscal exclusiva.

Ausência de qualquer condição bloqueia emissão em produção. Esta checklist não é uma
autorização e não deve ser marcada por testes locais ou gateways substituídos.
