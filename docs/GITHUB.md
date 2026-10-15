# Organização do GitHub

O repositório público apresenta o produto em português, usando a marca existente e [capturas reproduzíveis](assets/README.md). A disponibilidade pública não concede licença de código aberto; consulte [LICENSE](../LICENSE).

## Branches e verificações

- `develop` é a branch padrão e permite o fluxo direto de desenvolvimento.
- `main` é reservada a releases: PR obrigatório, checks `backend`, `frontend` e `compose` aprovados com a branch atualizada, conversas resolvidas e proteção aplicada também a administradores. Não exige aprovação de outra pessoa.
- Force push e exclusão de `main` ficam bloqueados. Merge commits e squash são permitidos; rebase merge fica desativado. Branches não são excluídas automaticamente para preservar `develop`.
- O CI executa em pushes de `develop`/`main`, PRs e disparos manuais. Validações obsoletas de desenvolvimento/PR são canceladas. Também valida os Compose adicionais em `infra/compose/`.
- Conteúdo tem permissão somente de leitura. O workflow de deploy/SSH e publicação no GHCR foi removido; nenhuma tag ou release instala a aplicação remotamente.

Os checks `backend`, `frontend` e `compose` foram preservados. Alterar nomes exige atualizar a proteção de `main`. Releases são publicadas após integrar `develop` em `main` por PR aprovado no CI. A primeira tag é `1.0.0`; consulte as [notas da versão](releases/1.0.0.md).

## Colaboração e segurança

Formulários de bug e melhoria e o template de PR ficam em `.github/`. As entradas `CONTRIBUTING.md` e `SECURITY.md` apontam às referências técnicas, com relato privado de vulnerabilidades.

Configurações remotas: secret scanning, push protection, alertas de dependências, correções automáticas de segurança e relato privado habilitados. Correções automáticas significam PRs propostos pelo Dependabot; nenhum merge automático foi configurado. Na habilitação em 15/10/2026 foram encontrados 18 alertas preexistentes, incluindo dois críticos; confira a [fila de segurança](https://github.com/Pedroltz/ofizzy/security/dependabot) para o estado atualizado.

O Dependabot verifica npm, os três projetos NuGet e GitHub Actions semanalmente, às segundas-feiras, às 09:00 de São Paulo. PRs de versão usam `develop` e agrupam minor/patch. Major permanece separado. O GitHub lê a configuração da branch padrão; PRs de segurança podem seguir o comportamento próprio do serviço.

Não há homepage ou link de demo inventado. A descrição e os topics refletem a implementação atual.

## Reaplicar e conferir

Com `gh` autenticado como administrador de `Pedroltz/ofizzy`, execute da raiz:

```powershell
./scripts/configure-github.ps1
```

O script exige permissão administrativa, aplica as opções acima e grava evidência local em `test-results/github-demo/github-settings-evidence.json`. Não grava tokens no repositório nem muda a conta global do CLI. Se a conta ativa não for administradora, selecione explicitamente a credencial correta na sessão antes de executá-lo.

Depois de mudanças, confira as configurações no GitHub, o formulário de issues, a renderização dos temas do README e uma execução do CI em `develop`. Proteção de `main` e habilitação de scanners não constituem uma auditoria integral de segurança nem homologação fiscal.
