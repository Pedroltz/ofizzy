# Matriz de domínio RTC — NF-e e NFS-e

Atualizada em **24/09/2026**.

## Baseline regulatório

- Documentos fiscais do **Simples Nacional** entram na obrigatoriedade RTC em **01/01/2027**.
- NFS-e Nacional recebeu tratamento de CNPJ alfanumérico em produção em 10/08/2026 e mantém documentação de IBS/CBS.
- Novas NF-e do Ofizzy usam `PL_010f_v1.04`.
- Split Payment possui documentação pública; o Ofizzy não é presumido como PSP.

## Matriz

| Tema | Estado no Ofizzy | Decisão |
| --- | --- | --- |
| `CST` / `cClassTrib` | ausente no domínio RTC | implementar por perfil e vigência |
| `gIBSCBS` | ausente | implementar somente para cenários aprovados |
| bases/alíquotas/totais IBS/CBS | ausentes | calcular no backend e preservar snapshot |
| redução/diferimento | ausente | modelar como cenário explícito |
| monofásico/crédito/estorno | fora do piloto | não criar flags genéricas |
| Imposto Seletivo | fora do piloto | manter bloqueado |
| CNPJ alfanumérico | só DV isolado | adaptar cadeia completa |
| NFS-e IBS/CBS | schema parcial, domínio ausente | adequar conforme perfil/cronograma |
| Split Payment | não implementado | preparar conciliação; integrar sob demanda real |

## Regras

1. Não inferir tributação por NCM.
2. Não emitir tags vazias apenas para passar no XSD.
3. Toda regra tributária deve ter vigência.
4. O snapshot fiscal deve preservar o que foi usado na emissão.
5. Cenário novo exige fixture, XML esperado e validação no pacote oficial.
6. Cenário não suportado deve falhar de forma explícita.
