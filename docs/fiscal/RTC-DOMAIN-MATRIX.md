# Matriz de domínio RTC — NF-e e NFS-e

Atualizada em **27/09/2026**.

## Baseline regulatório

- Documentos fiscais do **Simples Nacional** entram na obrigatoriedade RTC em **01/01/2027**.
- NFS-e Nacional recebeu tratamento de CNPJ alfanumérico em produção em 10/08/2026 e mantém documentação de IBS/CBS.
- Novas NF-e do Ofizzy usam `PL_010f_v1.04`.
- Split Payment possui documentação pública; o Ofizzy não é presumido como PSP.

## Matriz

| Tema | Estado no Ofizzy | Decisão |
| --- | --- | --- |
| `CST` / `cClassTrib` | editável por perfil/vigência, inclusive rascunhos | valores definidos pela contabilidade |
| `gIBSCBS` | NF-e integral local, fixture XSD aprovada | envio oficial bloqueado |
| bases/alíquotas/totais IBS/CBS | backend/snapshot para 000/000001 e base 100% | ampliar com cenários e fixtures |
| redução/diferimento | ausente | modelar como cenário explícito |
| monofásico/crédito/estorno | fora do piloto | não criar flags genéricas |
| Imposto Seletivo | fora do piloto | manter bloqueado |
| CNPJ alfanumérico | cadastro/busca/contratos/SAN | concluir emitente, eventos e NFS-e |
| NFS-e IBS/CBS | classificação DPS e cálculo agregado local | concluir pacote/resposta/PDF, envio oficial bloqueado |
| Split Payment | financeiro manual persistido | conciliação/PSP automático pendentes |

## Regras

1. Não inferir tributação por NCM.
2. Não emitir tags vazias apenas para passar no XSD.
3. Toda regra tributária deve ter vigência.
4. O snapshot fiscal deve preservar o que foi usado na emissão.
5. Cenário novo exige fixture, XML esperado e validação no pacote oficial.
6. Cenário não suportado deve falhar de forma explícita.

Rascunhos incompletos/fora do cenário integral são salvos, mas não emitidos. Alíquotas vazias não equivalem a zero; ST anterior permanece bloqueado para RTC. PDF RTC indisponível até adequação.
