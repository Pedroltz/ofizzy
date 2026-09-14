# Backup e restauração

A automação será entregue na Fase 6. Até lá, produção deve executar `pg_dump` antes de migrations e guardar o arquivo fora do servidor. A conclusão da Fase 6 exige restauração comprovada em banco temporário; possuir somente um arquivo de backup não satisfaz o critério.

## Migração técnica para Ofizzy

O script `scripts/migrate-to-ofizzy.sh` executa a migração única para um banco e volume novos, sem modificar o volume de origem. Antes de executá-lo, atualize o `.env` não versionado para `POSTGRES_DB=ofizzy` e `POSTGRES_USER=ofizzy` e garanta que as imagens Ofizzy estejam disponíveis.

```bash
./scripts/migrate-to-ofizzy.sh <projeto-compose-origem> <banco-origem> <usuario-origem> /caminho-seguro/ofizzy-pre-migration.dump
```

O script interrompe os serviços com escrita, gera e valida um dump em formato custom, cria `ofizzy_postgres_data`, restaura sem owners/ACLs, aplica migrations e inicia a nova stack. Valide contagens, login e o fluxo completo antes de liberar tráfego. Preserve o dump e o volume de origem por sete dias; qualquer remoção posterior exige confirmação explícita.

Status atual: procedimento automatizado e evidência de restauração ainda pendentes. Não marcar a Fase 6 como concluída até existir rotina agendada, política de retenção, armazenamento externo e teste documentado de restauração em uma instância isolada.

## Backup fiscal e restauração — 13/09/2026

Incluir tabelas fiscais, documentos, eventos, sequências e certificados cifrados no backup consistente do PostgreSQL. Preservar as chaves externas versionadas em custódia protegida separada; o banco sozinho não recupera o A1.

Restaurar primeiro em ambiente isolado, com emissão em produção desabilitada. Uma restauração antiga pode omitir notas autorizadas posteriormente: conciliar documentos/protocolos e numeração com o órgão fiscal antes de retomar emissão. Nunca presumir que um número ausente do backup está livre. O exercício completo de restauração/rotação continua pendente, com aceite em [NEXT-STEPS.md](NEXT-STEPS.md).
