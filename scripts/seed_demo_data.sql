-- Seed Script: Realistic Workshop Demonstration Data
-- Ofizzy - Gestão de Oficinas

BEGIN;

-- Limpeza prévia de registros de demonstração anteriores
DELETE FROM ofizzy.work_order_services WHERE "Id"::text LIKE 'c0000006%';
DELETE FROM ofizzy.work_order_parts WHERE "Id"::text LIKE 'c0000007%';
DELETE FROM ofizzy.work_orders WHERE "Id"::text LIKE 'c0000005%';
DELETE FROM ofizzy.vehicles WHERE "Id"::text LIKE 'c0000004%';
DELETE FROM ofizzy.customers WHERE "Id"::text LIKE 'c0000003%';
DELETE FROM ofizzy.parts WHERE "Id"::text LIKE 'c0000002%';
DELETE FROM ofizzy.services WHERE "Id"::text LIKE 'c0000001%';

-- 1. SERVIÇOS DO CATÁLOGO
INSERT INTO ofizzy.services ("Id", "Name", "Description", "DefaultPrice", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
  ('c0000001-0000-0000-0000-000000000001', 'Alinhamento 3D Dianteiro e Traseiro', 'Leitura a laser e ajuste de convergência/divergência dianteira e traseira', 130.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000002', 'Balanceamento de Rodas (4 Rodas)', 'Equilíbrio dinâmico computadorizado de massa das 4 rodas', 80.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000003', 'Montagem e Troca de Pneus (4 Rodas)', 'Desmontagem, montagem técnica com pasta especial e calibragem com N2', 60.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000004', 'Troca de Pastilhas e Discos de Freio', 'Substituição de pastilhas dianteiras/traseiras e sangria do sistema DOT4', 160.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000005', 'Troca de Óleo e Filtros (Mão de Obra)', 'Drenagem do cárter, substituição do anel de vedação e filtros', 70.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000006', 'Cambagem e Cáster Técnico (por lado)', 'Correção angular com equipamento pneumático de alta precisão', 90.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000007', 'Higienização de Ar-Condicionado e Ozônio', 'Eliminação de fungos e bactérias nos dutos de ventilação e cabine', 120.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000008', 'Revisão de Suspensão e Amortecedores', 'Inspeção de buchas, pivôs, bieletas, batentes e amortecedores', 150.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000009', 'Vulcanização e Reparo de Pneu a Frio', 'Remendo interno tipo cogumelo conforme norma técnica de segurança', 45.00, true, NOW(), NOW()),
  ('c0000001-0000-0000-0000-000000000010', 'Rodízio de Pneus e Inspeção de Desgaste', 'Rodízio em X ou paralelo com medição de sulco em milímetros', 40.00, true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- 2. PEÇAS E INSUMOS DO CATÁLOGO
INSERT INTO ofizzy.parts ("Id", "Name", "Code", "CostPrice", "SalePrice", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
  ('c0000002-0000-0000-0000-000000000001', 'Pneu Pirelli Cinturato P7 205/55 R16', 'PIR-205-55-16', 360.00, 540.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000002', 'Pneu Michelin Primacy 4 225/45 R17', 'MCH-225-45-17', 490.00, 750.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000003', 'Pneu Goodyear EfficientGrip 185/65 R15', 'GDY-185-65-15', 280.00, 420.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000004', 'Pneu Bridgestone Turanza ER300 195/60 R15', 'BRG-195-60-15', 310.00, 460.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000005', 'Pastilha de Freio Dianteira Fras-le Cerâmica', 'FRS-PD1420', 110.00, 185.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000006', 'Par de Discos de Freio Dianteiros Fremax Ventilados', 'FMX-BD4520', 220.00, 360.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000007', 'Óleo Motorcraft 5W30 100% Sintético (1 Litro)', 'MOT-5W30-1L', 32.00, 58.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000008', 'Filtro de Óleo Mann-Filter W712', 'MAN-W712', 25.00, 48.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000009', 'Filtro de Cabine / Ar-Condicionado Tecfil', 'TEC-CAB4410', 28.00, 55.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000010', 'Válvula Bico de Roda TR414 Premium', 'VAL-TR414', 4.00, 15.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000011', 'Fluido de Freio Bosch DOT 4 (500ml)', 'BOS-DOT4-500', 24.00, 45.00, true, NOW(), NOW()),
  ('c0000002-0000-0000-0000-000000000012', 'Amortecedor Dianteiro Monroe OESpectrum (Unidade)', 'MNR-742145', 290.00, 470.00, true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- 3. CLIENTES
INSERT INTO ofizzy.customers ("Id", "Name", "Document", "Phone", "WhatsApp", "Email", "Address", "Notes", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
  ('c0000003-0000-0000-0000-000000000001', 'Renata Vasconcelos de Alencar', '28491823812', '1134218899', '11988223344', 'renata.alencar@uol.com.br', 'Av. Paulista, 1842, Apto 112 - Bela Vista, São Paulo - SP', 'Cliente preferencial. Solicita aviso prévio por WhatsApp antes de aprovar peças.', true, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days'),
  ('c0000003-0000-0000-0000-000000000002', 'Marcos Vinicius Mendonça', '19382746501', '1122894455', '11971239988', 'marcos.mendonca@logistica.com.br', 'Rua Vergueiro, 3050 - Vila Mariana, São Paulo - SP', 'Frotista individual. Paga via PIX CNPJ.', true, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days'),
  ('c0000003-0000-0000-0000-000000000003', 'TranspSilva Logística e Frotas LTDA', '12345678000195', '1138765500', '11995543322', 'frotas@transpsilva.com.br', 'Rodovia dos Imigrantes, km 18, Galpão 4 - Diadema - SP', 'Contrato corporativo. Faturamento para 30 dias com boleto bancário.', true, NOW() - INTERVAL '60 days', NOW() - INTERVAL '60 days'),
  ('c0000003-0000-0000-0000-000000000004', 'Juliana Costa Prado', '38472910488', '1145671122', '11982345678', 'ju.prado@advocacia.com.br', 'Rua Oscar Freire, 920 - Jardins, São Paulo - SP', 'Veículo de uso executivo, sempre realizar higienização ao término.', true, NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
  ('c0000003-0000-0000-0000-000000000005', 'Eduardo Henrique Antunes', '09483726190', '1131092233', '11964551122', 'eduardo.antunes@gmail.com', 'Av. Moema, 450 - Moema, São Paulo - SP', 'Usa pneus de alta performance esportivos.', true, NOW() - INTERVAL '15 days', NOW() - INTERVAL '15 days'),
  ('c0000003-0000-0000-0000-000000000006', 'Camila Nogueira Ribeiro', '47382910544', '1129883344', '11976543210', 'camila.ribeiro@tecnologia.io', 'Rua Gomes de Carvalho, 1507 - Vila Olímpia, São Paulo - SP', 'Agendamentos somente aos sábados pela manhã.', true, NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days'),
  ('c0000003-0000-0000-0000-000000000007', 'Rodrigo Fonseca Bueno', '58291038477', '1144332211', '11981234567', 'rodrigo.bueno@outlook.com', 'Rua Domingos de Morais, 1200 - Vila Mariana, São Paulo - SP', 'Cliente novo por indicação.', true, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'),
  ('c0000003-0000-0000-0000-000000000008', 'Auto Peças e Mecânica Central LTDA', '98765432000188', '1133221100', '11991122334', 'compras@mecanicacentral.com.br', 'Av. Santo Amaro, 2400 - Santo Amaro, São Paulo - SP', 'Parceiro comercial para serviços de alinhamento pesado.', true, NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days')
ON CONFLICT ("Id") DO NOTHING;

-- 4. VEÍCULOS
INSERT INTO ofizzy.vehicles ("Id", "CustomerId", "Plate", "Brand", "Model", "Year", "Color", "Mileage", "Chassis", "Notes", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
  ('c0000004-0000-0000-0000-000000000001', 'c0000003-0000-0000-0000-000000000001', 'BRA2E19', 'Toyota', 'Corolla XEi 2.0 Dynamic Force', 2022, 'Prata Metálico', 38400, '9BRBD48E3N0129845', 'Revisões em dia. Rodas de liga aro 16.', true, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days'),
  ('c0000004-0000-0000-0000-000000000002', 'c0000003-0000-0000-0000-000000000002', 'RTY9H88', 'Honda', 'Civic Touring 1.5 Turbo', 2021, 'Preto Cristal', 46200, '93HFC1670MZ102948', 'Usa pneus 225/45 R17.', true, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days'),
  ('c0000004-0000-0000-0000-000000000003', 'c0000003-0000-0000-0000-000000000003', 'FLT4A12', 'Iveco', 'Daily 35S14 Baú Refrigerado', 2020, 'Branco Geada', 142500, '93Z35S140L8201934', 'Veículo de entrega diária. Revisão quinzenal.', true, NOW() - INTERVAL '60 days', NOW() - INTERVAL '60 days'),
  ('c0000004-0000-0000-0000-000000000004', 'c0000003-0000-0000-0000-000000000003', 'FLT4B34', 'Mercedes-Benz', 'Sprinter 415 CDI Furgão', 2019, 'Prata', 185600, '8AC906633KA918234', 'Troca constante de pastilhas devido à carga.', true, NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days'),
  ('c0000004-0000-0000-0000-000000000005', 'c0000003-0000-0000-0000-000000000004', 'JCP3C45', 'Jeep', 'Compass Limited 1.3 Turbo T270', 2023, 'Cinza Granite', 22100, '988674930PX827102', 'Pneus aro 19 em excelente estado.', true, NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
  ('c0000004-0000-0000-0000-000000000006', 'c0000003-0000-0000-0000-000000000005', 'EHA7D90', 'Volkswagen', 'Golf GTI 2.0 TSI DSG', 2018, 'Vermelho Tornado', 68900, '3VW5T7AU7JM293847', 'Preparação esportiva leve, rodas aro 18.', true, NOW() - INTERVAL '15 days', NOW() - INTERVAL '15 days'),
  ('c0000004-0000-0000-0000-000000000007', 'c0000003-0000-0000-0000-000000000006', 'CNR5F67', 'Chevrolet', 'Onix Premier 1.0 Turbo', 2022, 'Azul Seeker', 31500, '9BGSG48U0NG192834', 'Cliente reclamou de vibração ao atingir 100 km/h.', true, NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days'),
  ('c0000004-0000-0000-0000-000000000008', 'c0000003-0000-0000-0000-000000000007', 'RFB1G23', 'Hyundai', 'Creta Ultimate 2.0 Smartstream', 2023, 'Branco Atlas', 19800, '9BHGN81EBPB092834', 'Primeira revisão de suspensão e rodízio.', true, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'),
  ('c0000004-0000-0000-0000-000000000009', 'c0000003-0000-0000-0000-000000000008', 'BCP8K99', 'Ford', 'Ranger Limited 3.2 4x4 Diesel', 2020, 'Preto Gales', 94300, '8AFTF34E2L8291034', 'Uso misto terra e asfalto. Alinhamento 4x4 necessário.', true, NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days')
ON CONFLICT ("Id") DO NOTHING;

-- 5. ORDENS DE SERVIÇO (DISTRIBUIÇÃO: InProgress = Pátio, Open = Abertas, Completed = Concluídas, Cancelled = Canceladas)

-- OS 101: InProgress (Pátio da Oficina) - Toyota Corolla (Renata)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000001',
  'c0000003-0000-0000-0000-000000000001',
  'c0000004-0000-0000-0000-000000000001',
  'Renata Vasconcelos de Alencar', '28491823812', '11988223344',
  'BRA2E19', 'Toyota Corolla XEi 2.0 Dynamic Force', 38400,
  'Direção puxando para a direita na estrada e ruído leve no freio dianteiro.',
  'Desalinhamento dianteiro de 2° e pastilhas dianteiras no limite de segurança (menos de 2mm).',
  'Veículo no elevador 01. Montagem e alinhamento em execução.',
  'InProgress', NOW() - INTERVAL '4 hours', NOW() - INTERVAL '1 hour', NULL
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000001', 'c0000005-0000-0000-0000-000000000001', 'c0000001-0000-0000-0000-000000000001', 'Alinhamento 3D Dianteiro e Traseiro', 1, 130.00),
  ('c0000006-0000-0000-0000-000000000002', 'c0000005-0000-0000-0000-000000000001', 'c0000001-0000-0000-0000-000000000004', 'Troca de Pastilhas e Discos de Freio', 1, 160.00)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_parts ("Id", "WorkOrderId", "PartId", "Description", "Code", "Quantity", "UnitPrice")
VALUES
  ('c0000007-0000-0000-0000-000000000001', 'c0000005-0000-0000-0000-000000000001', 'c0000002-0000-0000-0000-000000000005', 'Pastilha de Freio Dianteira Fras-le Cerâmica', 'FRS-PD1420', 1, 185.00),
  ('c0000007-0000-0000-0000-000000000002', 'c0000005-0000-0000-0000-000000000001', 'c0000002-0000-0000-0000-000000000011', 'Fluido de Freio Bosch DOT 4 (500ml)', 'BOS-DOT4-500', 1, 45.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 102: InProgress (Pátio da Oficina) - Honda Civic (Marcos Vinicius)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000002',
  'c0000003-0000-0000-0000-000000000002',
  'c0000004-0000-0000-0000-000000000002',
  'Marcos Vinicius Mendonça', '19382746501', '11971239988',
  'RTY9H88', 'Honda Civic Touring 1.5 Turbo', 46200,
  'Troca dos 2 pneus dianteiros desgastados e balanceamento completo.',
  'Pneus dianteiros com desgaste irregular no ombro externo. Traseiros ainda com 5mm.',
  'Veículo na rampa 02 aguardando finalização do balanceamento dinâmico.',
  'InProgress', NOW() - INTERVAL '3 hours', NOW() - INTERVAL '30 minutes', NULL
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000003', 'c0000005-0000-0000-0000-000000000002', 'c0000001-0000-0000-0000-000000000002', 'Balanceamento de Rodas (4 Rodas)', 1, 80.00),
  ('c0000006-0000-0000-0000-000000000004', 'c0000005-0000-0000-0000-000000000002', 'c0000001-0000-0000-0000-000000000003', 'Montagem e Troca de Pneus (4 Rodas)', 1, 60.00),
  ('c0000006-0000-0000-0000-000000000005', 'c0000005-0000-0000-0000-000000000002', 'c0000001-0000-0000-0000-000000000001', 'Alinhamento 3D Dianteiro e Traseiro', 1, 130.00)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_parts ("Id", "WorkOrderId", "PartId", "Description", "Code", "Quantity", "UnitPrice")
VALUES
  ('c0000007-0000-0000-0000-000000000003', 'c0000005-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000002', 'Pneu Michelin Primacy 4 225/45 R17', 'MCH-225-45-17', 2, 750.00),
  ('c0000007-0000-0000-0000-000000000004', 'c0000005-0000-0000-0000-000000000002', 'c0000002-0000-0000-0000-000000000010', 'Válvula Bico de Roda TR414 Premium', 'VAL-TR414', 2, 15.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 103: InProgress (Pátio da Oficina) - Iveco Daily (TranspSilva)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000003',
  'c0000003-0000-0000-0000-000000000003',
  'c0000004-0000-0000-0000-000000000003',
  'TranspSilva Logística e Frotas LTDA', '12345678000195', '1138765500',
  'FLT4A12', 'Iveco Daily 35S14 Baú Refrigerado', 142500,
  'Troca preventiva de óleo de motor e revisão dos freios dianteiros para viagem.',
  'Óleo vencido por quilometragem (passou 2.000 km). Pastilhas com meia vida, liberadas para rodagem.',
  'Veículo no Box Utilitários. Troca de óleo e filtro finalizada.',
  'InProgress', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '15 minutes', NULL
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000006', 'c0000005-0000-0000-0000-000000000003', 'c0000001-0000-0000-0000-000000000005', 'Troca de Óleo e Filtros (Mão de Obra)', 1, 70.00)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_parts ("Id", "WorkOrderId", "PartId", "Description", "Code", "Quantity", "UnitPrice")
VALUES
  ('c0000007-0000-0000-0000-000000000005', 'c0000005-0000-0000-0000-000000000003', 'c0000002-0000-0000-0000-000000000007', 'Óleo Motorcraft 5W30 100% Sintético (1 Litro)', 'MOT-5W30-1L', 7, 58.00),
  ('c0000007-0000-0000-0000-000000000006', 'c0000005-0000-0000-0000-000000000003', 'c0000002-0000-0000-0000-000000000008', 'Filtro de Óleo Mann-Filter W712', 'MAN-W712', 1, 48.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 104: Open (Aguardando Aprovação / Orçamento) - Jeep Compass (Juliana Costa)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000004',
  'c0000003-0000-0000-0000-000000000004',
  'c0000004-0000-0000-0000-000000000005',
  'Juliana Costa Prado', '38472910488', '11982345678',
  'JCP3C45', 'Jeep Compass Limited 1.3 Turbo T270', 22100,
  'Odor desagradável no ar condicionado e barulho ao esterçar em manobras.',
  'Filtro de cabine saturado de poeira e bucha da barra estabilizadora ressecada.',
  'Orçamento enviado para aprovação da cliente via WhatsApp.',
  'Open', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', NULL
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000007', 'c0000005-0000-0000-0000-000000000004', 'c0000001-0000-0000-0000-000000000007', 'Higienização de Ar-Condicionado e Ozônio', 1, 120.00),
  ('c0000006-0000-0000-0000-000000000008', 'c0000005-0000-0000-0000-000000000004', 'c0000001-0000-0000-0000-000000000008', 'Revisão de Suspensão e Amortecedores', 1, 150.00)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_parts ("Id", "WorkOrderId", "PartId", "Description", "Code", "Quantity", "UnitPrice")
VALUES
  ('c0000007-0000-0000-0000-000000000007', 'c0000005-0000-0000-0000-000000000004', 'c0000002-0000-0000-0000-000000000009', 'Filtro de Cabine / Ar-Condicionado Tecfil', 'TEC-CAB4410', 1, 55.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 105: Open (Em Diagnóstico) - Volkswagen Golf GTI (Eduardo Henrique)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000005',
  'c0000003-0000-0000-0000-000000000005',
  'c0000004-0000-0000-0000-000000000006',
  'Eduardo Henrique Antunes', '09483726190', '11964551122',
  'EHA7D90', 'Volkswagen Golf GTI 2.0 TSI DSG', 68900,
  'Pneu traseiro esquerdo perdendo pressão aos poucos (cerca de 5 libras por semana).',
  'Prego pequeno alojado na banda de rodagem, reparável a frio sem perda de índice de carga.',
  'Agendado para reparo hoje às 17h.',
  'Open', NOW() - INTERVAL '5 hours', NOW() - INTERVAL '5 hours', NULL
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000009', 'c0000005-0000-0000-0000-000000000005', 'c0000001-0000-0000-0000-000000000009', 'Vulcanização e Reparo de Pneu a Frio', 1, 45.00),
  ('c0000006-0000-0000-0000-000000000010', 'c0000005-0000-0000-0000-000000000005', 'c0000001-0000-0000-0000-000000000002', 'Balanceamento de Rodas (4 Rodas)', 1, 80.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 106: Completed (Entregue) - Chevrolet Onix (Camila Nogueira)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000006',
  'c0000003-0000-0000-0000-000000000006',
  'c0000004-0000-0000-0000-000000000007',
  'Camila Nogueira Ribeiro', '47382910544', '11976543210',
  'CNR5F67', 'Chevrolet Onix Premier 1.0 Turbo', 31500,
  'Vibração perceptível no volante acima de 90 km/h.',
  'Chumbo de balanceamento da roda dianteira direita soltou.',
  'Serviço concluído com sucesso. Teste de rodagem OK.',
  'Completed', NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000011', 'c0000005-0000-0000-0000-000000000006', 'c0000001-0000-0000-0000-000000000002', 'Balanceamento de Rodas (4 Rodas)', 1, 80.00),
  ('c0000006-0000-0000-0000-000000000012', 'c0000005-0000-0000-0000-000000000006', 'c0000001-0000-0000-0000-000000000001', 'Alinhamento 3D Dianteiro e Traseiro', 1, 130.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 107: Completed (Entregue) - Ford Ranger (Auto Peças Central)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000007',
  'c0000003-0000-0000-0000-000000000008',
  'c0000004-0000-0000-0000-000000000009',
  'Auto Peças e Mecânica Central LTDA', '98765432000188', '11991122334',
  'BCP8K99', 'Ford Ranger Limited 3.2 4x4 Diesel', 94300,
  'Troca dos 2 amortecedores dianteiros com vazamento de óleo.',
  'Amortecedor direito estourado após buraco na rodovia. Batentes danificados.',
  'Amortecedores Monroe substituídos e alinhamento tridimensional realizado.',
  'Completed', NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days'
) ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_services ("Id", "WorkOrderId", "ServiceId", "Description", "Quantity", "UnitPrice")
VALUES
  ('c0000006-0000-0000-0000-000000000013', 'c0000005-0000-0000-0000-000000000007', 'c0000001-0000-0000-0000-000000000008', 'Revisão de Suspensão e Amortecedores', 1, 150.00),
  ('c0000006-0000-0000-0000-000000000014', 'c0000005-0000-0000-0000-000000000007', 'c0000001-0000-0000-0000-000000000001', 'Alinhamento 3D Dianteiro e Traseiro', 1, 130.00)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO ofizzy.work_order_parts ("Id", "WorkOrderId", "PartId", "Description", "Code", "Quantity", "UnitPrice")
VALUES
  ('c0000007-0000-0000-0000-000000000008', 'c0000005-0000-0000-0000-000000000007', 'c0000002-0000-0000-0000-000000000012', 'Amortecedor Dianteiro Monroe OESpectrum (Unidade)', 'MNR-742145', 2, 470.00)
ON CONFLICT ("Id") DO NOTHING;

-- OS 108: Cancelled - Hyundai Creta (Rodrigo Fonseca)
INSERT INTO ofizzy.work_orders (
  "Id", "CustomerId", "VehicleId", "CustomerName", "CustomerDocument", "CustomerPhone",
  "VehiclePlate", "VehicleDescription", "Mileage", "Complaint", "Diagnosis", "Notes",
  "Status", "CreatedAt", "UpdatedAt", "CompletedAt"
) VALUES (
  'c0000005-0000-0000-0000-000000000008',
  'c0000003-0000-0000-0000-000000000007',
  'c0000004-0000-0000-0000-000000000008',
  'Rodrigo Fonseca Bueno', '58291038477', '11981234567',
  'RFB1G23', 'Hyundai Creta Ultimate 2.0 Smartstream', 19800,
  'Cliente solicitou orçamento para 4 pneus novos aro 17.',
  'Pneus atuais ainda com 4.5mm de profundidade (seguros até 1.6mm). Não há necessidade técnica de troca imediata.',
  'Cliente optou por realizar a troca apenas na revisão de 30.000 km.',
  'Cancelled', NOW() - INTERVAL '6 days', NOW() - INTERVAL '6 days', NULL
) ON CONFLICT ("Id") DO NOTHING;

COMMIT;
