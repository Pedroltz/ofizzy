# Backup e restauração

A automação será entregue na Fase 6. Até lá, produção deve executar `pg_dump` antes de migrations e guardar o arquivo fora do servidor. A conclusão da Fase 6 exige restauração comprovada em banco temporário; possuir somente um arquivo de backup não satisfaz o critério.

Status atual: procedimento automatizado e evidência de restauração ainda pendentes. Não marcar a Fase 6 como concluída até existir rotina agendada, política de retenção, armazenamento externo e teste documentado de restauração em uma instância isolada.
