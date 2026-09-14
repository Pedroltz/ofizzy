# Referência de seed fiscal antigo

`LegacyFiscalSeed.cs.reference` preserva o utilitário manual anterior, sem compilação ou execução na suíte. Não execute contra o banco de trabalho: ele contém destino fixo e altera configurações, perfis e snapshots de OS existentes em etapas.

O fluxo reproduzível de teste usa dados fictícios criados pela API nos testes de integração e na stack isolada de smoke. O banco atual só será zerado em uma operação explícita antes da homologação; este incremento não realiza reset nem migra dados simulados para uso oficial.
