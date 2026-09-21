# Eventos RTC da NF-e

Os XSDs deste diretório são a distribuição oficial de eventos RTC da NT 2025.002
v1.40, publicada em 27/07/2026.

- Origem: [Portal Nacional da NF-e](https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D).
- SHA-256 do ZIP: `a4c57ce95b225cd8852f90bd6c39ca28ae551636ff3f67eb2602b9fa847129b2`.

Eles são mantidos lado a lado porque tratam eventos RTC específicos e não substituem
o envelope oficial de cancelamento `110111` já utilizado pelo Ofizzy. Um novo fluxo
de evento só pode selecioná-los depois que seu cenário, payload e validação forem
implementados e aprovados pela contabilidade.
