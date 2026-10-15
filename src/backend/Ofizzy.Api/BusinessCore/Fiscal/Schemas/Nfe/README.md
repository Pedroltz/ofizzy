# Referência dos schemas NF-e

O schema histórico nesta pasta é o baseline `010c`/4.00. Ele não deve ser
sobrescrito: documentos emitidos sob esse baseline devem continuar rastreáveis. O
schema ativo para novas NF-e está em `../Nfe010f`.

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
`DFeTiposBasicos_v1.00.xsd` e `leiauteNFe_v4.00.xsd`. Os cinco arquivos do 010f
estão instalados sem alteração em `../Nfe010f` e são o baseline ativo de novas
emissões. O acervo local também tem schemas de inutilização que não fazem parte
desses ZIPs e mantêm fonte oficial compatível própria.

O algoritmo do CNPJ alfanumérico foi implementado de forma isolada em
`FiscalValidation.IsAlphanumericCnpj`, conforme o manual oficial da Receita Federal.
Ele não é inferido para o perfil piloto numérico: qualquer cenário que o utilize
precisa de classificação, certificado e teste de chave/XML próprios.
