# Fase 4 — Impressão & Configuração da Oficina

Status: **Concluída e Validada (2026-09-03)**

## Escopo Entregue

1. **Configuração da Oficina (`Company`)**:
   - Entidade e tabela PostgreSQL `companies` estendida com Razão Social, CNPJ (até 18 dígitos), Telefones, WhatsApp, E-mail, Endereço completo, Cidade, UF, CEP, Termo de Garantia e Observações finais.
   - Endpoints `GET /api/company` e `PUT /api/company` com validação FluentValidation.
   - Aba **"Dados da Oficina"** em `/configuracoes` no frontend Angular com formulário reativo e feedback instantâneo.

2. **Impressão HTML A4 Minimalista**:
   - Template de impressão limpo, econômico e profissional baseado estritamente em **texto e linhas divisórias** (`.wo-print-sheet`).
   - Sem fundos coloridos, sem gradientes ou blocos que desperdicem tinta.
   - Estrutura completa: Cabeçalho da Oficina + Identificação da OS, Dados do Cliente e Veículo, Queixa/Diagnóstico, Tabelas de Serviços e Peças, Fechamento Financeiro e Termo de Garantia com campos de assinatura para entrega do veículo.
   - Disparado diretamente via botão **"Imprimir Ficha"** (`window.print()`).

3. **Geração de PDF Oficial no Backend (QuestPDF)**:
   - Endpoint `GET /api/work-orders/{id}/pdf` gerando arquivo PDF com o mesmo design minimalista A4 de texto e separações.
   - Suporte a múltiplas páginas com cabeçalho contínuo e numeração de páginas (`Página X de Y`).
   - Botão **"Baixar PDF"** integrado na tela de detalhes da Ordem de Serviço.

## Critérios de Aceite Atendidos

- [x] Documentos utilizam exclusivamente dados persistidos no PostgreSQL.
- [x] Suporte a múltiplas páginas para ordens volumosas.
- [x] Contém todos os campos obrigatórios (oficina, cliente, veículo, itens, valores e assinaturas).
- [x] Testes de unidade e integração aprovados (`14/14` unit tests e `3/3` integration tests).
- [x] Smoke test via Nginx na porta 8080 confirmando geração do PDF (`%PDF`, status 200, 96 KB) e persistência das configurações da oficina.
