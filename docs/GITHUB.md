# Organização do GitHub

O repositório público apresenta o produto em português, usando a marca existente e [capturas reproduzíveis](assets/README.md). A disponibilidade pública não concede licença de código aberto; consulte [LICENSE](../LICENSE).

## Branches e verificações

- `develop` é a branch padrão e permite o fluxo direto de desenvolvimento.
- `main` é reservada a releases: PR obrigatório, checks `backend`, `frontend` e `compose` aprovados com a branch atualizada, conversas resolvidas e proteção aplicada também a administradores. Não exige aprovação de outra pessoa.
- Force push e exclusão de `main` ficam bloqueados. Merge commits e squash são permitidos; rebase merge fica desativado. Branches não são excluídas automaticamente para preservar `develop`.
- O CI executa em pushes de `develop` e PRs, e continua reutilizável pelo deploy. Validações obsoletas são canceladas; deploys são serializados sem interrupção de um deploy em andamento.
- Conteúdo tem permissão somente de leitura por padrão; publicação no GHCR recebe escrita em packages apenas no job responsável.

Os nomes dos checks foram conferidos na execução aprovada `34226277097`. Alterar nomes exige atualizar a proteção de `main`. O deploy continua em push de `main` ou disparo manual; esta entrega não publica release nem executa deploy.

## Colaboração e segurança

Formulários de bug e melhoria e o template de PR ficam em `.github/`. As entradas `CONTRIBUTING.md` e `SECURITY.md` apontam às referências técnicas, com relato privado de vulnerabilidades.

Configurações remotas: secret scanning, push protection, alertas de dependências, correções automáticas de segurança e relato privado habilitados. Correções automáticas significam PRs propostos pelo Dependabot; nenhum merge automático foi configurado.

O Dependabot verifica npm, os três projetos NuGet e GitHub Actions semanalmente, às segundas-feiras, às 09:00 de São Paulo. PRs de versão usam `develop` e agrupam minor/patch. Major permanece separado. O GitHub lê a configuração da branch padrão; PRs de segurança podem seguir o comportamento próprio do serviço.

Não há homepage ou link de demo inventado. A descrição e os topics refletem a implementação atual.

## Reaplicar e conferir

Com `gh` autenticado como administrador de `Pedroltz/ofizzy`, execute da raiz:

```powershell
./scripts/configure-github.ps1
```

O script exige permissão administrativa, aplica as opções acima e grava evidência local em `test-results/github-demo/github-settings-evidence.json`. Não grava tokens no repositório nem muda a conta global do CLI. Se a conta ativa não for administradora, selecione explicitamente a credencial correta na sessão antes de executá-lo.

Depois de mudanças, confira as configurações no GitHub, o formulário de issues, a renderização dos temas do README e uma execução do CI em `develop`. Proteção de `main` e habilitação de scanners não constituem uma auditoria integral de segurança nem homologação fiscal.
