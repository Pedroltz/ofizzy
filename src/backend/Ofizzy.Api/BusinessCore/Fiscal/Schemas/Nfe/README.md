# Referência dos schemas NF-e

O schema ativo nesta pasta ainda é o baseline `010c`/4.00. Ele não deve ser
sobrescrito por um pacote mais novo sem a correspondente adaptação de domínio, XML,
validação, assinatura, eventos, inutilização e fixtures.

Em 20/09/2026, os pacotes foram obtidos diretamente do Portal Nacional da NF-e com
o cookie `AspxAutoDetectCookieSupport=1` exigido pelo próprio portal:

- Histórico `PL_010e_v1.02`, publicado em 10/07/2026 para NT 2025.002 v1.40,
  NT 2026.002 v1.00 e NT 2026.003 v1.00:
  [download oficial](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=0XLIO6BXjX0%3D)
  e SHA-256 `d44ae5aa6a0d1cabf6235d2d2d47b75be5dd87bc6b90a7ec3dcec99c3d41bda1`.
- Atual `PL_010f_v1.04`, publicado em 31/08/2026 para NT 2025.002 v1.50 e
  NT 2026.007 v1.00:
  [download oficial](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=8ITFuBLltXs%3D)
  e SHA-256 `b8589490a58a09a993a80e6ac4d7ed10f20892061ecfc56719337098d4b95998`.

O 010e contém cinco XSDs de NF-e; o 010f mantém essa árvore e altera
`DFeTiposBasicos_v1.00.xsd` e `leiauteNFe_v4.00.xsd`. O baseline local também tem
schemas de inutilização que não fazem parte desses ZIPs e precisam de fonte oficial
compatível própria. A migração deve partir do 010f, não do 010e já superado.

O algoritmo do CNPJ alfanumérico foi implementado de forma isolada em
`FiscalValidation.IsAlphanumericCnpj`, conforme o manual oficial da Receita Federal.
Ele ainda não é usado pelos validadores, XML ou chave de acesso ativos: esses pontos
continuam numéricos enquanto o pacote 010c estiver selecionado.
