import { expect, test, BrowserContext } from '@playwright/test';
import { readFileSync, writeFileSync } from 'node:fs';
const statePath = '/tmp/ofizzy-fiscal-live.json';
const password = 'FiscalSmoke2026'; // Only the isolated acceptance database.
async function api(context: BrowserContext, method: string, url: string, data?: unknown) {
  await context.request.get('/api/setup/status');
  const token = (await context.cookies()).find(x => x.name === 'XSRF-TOKEN')?.value ?? '';
  return context.request.fetch(url, { method, data, headers: { 'X-XSRF-TOKEN': decodeURIComponent(token) } });
}
async function json(context: BrowserContext, method: string, url: string, data?: unknown) {
  const response = await api(context, method, url, data);
  expect(response.ok(), `${method} ${url}: ${response.status()} ${await response.text()}`).toBeTruthy();
  return response.status() === 204 ? null : response.json();
}
test('configuração e preparação fiscal persistem pelo Nginx', async ({ page }) => {
  const context = page.context();
  const restart = process.env['OFIZZY_VERIFY_RESTART'] === '1';
  let orderId: string;
  let tenantId: string;
  if (restart) {
    ({ orderId, tenantId } = JSON.parse(readFileSync(statePath, 'utf8')));
    await json(context, 'POST', '/api/auth/login', { email: 'fiscal@smoke.test', password });
    await json(context, 'POST', '/api/auth/tenant', { tenantId });
  } else {
    const setup = await json(context, 'GET', '/api/setup/status');
    if (setup.required) await json(context, 'POST', '/api/setup', { companyName: 'Plataforma Fiscal', adminName: 'Operador', email: 'fiscal@smoke.test', password });
    await json(context, 'POST', '/api/auth/login', { email: 'fiscal@smoke.test', password });
    const tenant = await json(context, 'POST', '/api/platform/tenants', { name: 'Oficina Fiscal', slug: `fiscal-${Date.now()}`, vertical: 'Automotive', adminName: 'Operador', email: 'fiscal@smoke.test', modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'] });
    await json(context, 'POST', '/api/auth/tenant', { tenantId: tenant.id });
    await json(context, 'POST', '/api/tenant/onboarding/complete', {});
    const customer = await json(context, 'POST', '/api/customers', { name: 'Cliente Fiscal', document: '12345678909' });
    const vehicle = await json(context, 'POST', '/api/vehicles', { customerId: customer.id, plate: 'FIS1A23', model: 'Teste' });
    const order = await json(context, 'POST', '/api/work-orders', { customerId: customer.id, vehicleId: vehicle.id, services: [{ description: 'Revisão', quantity: 1, unitPrice: 100 }], parts: [{ description: 'Pneu', code: 'PN-01', quantity: 1, unitPrice: 300 }] });
    orderId = order.id; tenantId = tenant.id;
    await json(context, 'PATCH', `/api/work-orders/${orderId}/status`, { status: 'InProgress' });
    await json(context, 'PATCH', `/api/work-orders/${orderId}/status`, { status: 'Completed' });
    await json(context, 'PUT', '/api/fiscal/settings', { cnpj: '11222333000181', legalName: 'Oficina Fiscal', stateRegistration: '110042490114', municipalRegistration: '', regime: 'SimplesNacional', address: { street: 'Rua Teste', number: '10', district: 'Centro', city: 'São Paulo', cityCode: '3550308', state: 'SP', postalCode: '01001000' }, nfeEnabled: true, nfseEnabled: true, environment: 'Homologation', nfeSeries: 1, dpsSeries: 1 });
    writeFileSync(statePath, JSON.stringify({ orderId, tenantId }), { mode: 0o600 });
  }
  const fiscal = await json(context, 'GET', `/api/work-orders/${orderId}/fiscal`);
  expect(fiscal.servicesTotal).toBe(100); expect(fiscal.productsTotal).toBe(300);
  if (restart) expect(fiscal.preparation.name).toBe('Tomador Fiscal Persistido');
  await page.goto('/configuracoes');
  await page.getByRole('tab', { name: 'Fiscal', exact: true }).click();
  await expect(page.locator('#issuer-cnpj')).toHaveValue('11222333000181');
  for (const width of [1440, 768, 320]) {
    await page.setViewportSize({ width, height: 1000 });
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    expect((await page.locator('#issuer-cnpj').boundingBox())!.height).toBeGreaterThanOrEqual(44);
  }
  await page.getByText('Histórico e inutilização NF-e', { exact: true }).click();
  await expect(page.getByText('Nenhum pedido registrado.')).toBeVisible();
  await page.locator('#inut-firstNumber').fill('999999'); await page.locator('#inut-lastNumber').fill('999999');
  await page.locator('#inut-reason').fill('Teste de validação para número não reservado');
  await page.getByRole('button', { name: 'Revisar inutilização', exact: true }).click();
  const confirmation = page.getByRole('dialog', { name: 'Confirmar inutilização', exact: true });
  await confirmation.getByRole('button', { name: 'Confirmar inutilização', exact: true }).click();
  await expect(confirmation.getByText(/O intervalo deve conter somente/)).toBeVisible();
  await confirmation.getByRole('button', { name: 'Voltar', exact: true }).click();
  expect((await api(context, 'POST', '/api/fiscal/nfe/inutilizations/00000000-0000-0000-0000-000000000001/sync')).status()).toBe(404);
  await page.goto('/ordens');
  await page.getByText('Cliente Fiscal', { exact: true }).first().click();
  await page.getByRole('button', { name: /Preparação fiscal/ }).click();
  const dialog = page.getByRole('dialog', { name: 'Preparação fiscal da OS' });
  await dialog.locator('#recipient-name').fill('Tomador Fiscal Persistido');
  await dialog.getByRole('button', { name: 'Salvar preparação' }).click();
  await expect.poll(async () => (await json(context, 'GET', `/api/work-orders/${orderId}/fiscal`)).preparation.name).toBe('Tomador Fiscal Persistido');
  expect((await api(context, 'POST', `/api/work-orders/${orderId}/fiscal/issue`)).status()).toBe(400);
  expect((await api(context, 'GET', `/api/work-orders/${orderId}/fiscal/download`)).status()).toBe(409);
});

const simulationStatePath = '/tmp/ofizzy-fiscal-simulation-live.json';
test('simulação emite XML e PDF e cancela NFS-e pelo Nginx', async ({ page }) => {
  const context = page.context();
  await json(context, 'POST', '/api/auth/login', { email: 'fiscal@smoke.test', password });
  const restart = process.env['OFIZZY_VERIFY_RESTART'] === '1';
  let state: { tenantId: string; orderId: string; documentIds: string[] };
  if (restart) {
    state = JSON.parse(readFileSync(simulationStatePath, 'utf8'));
    await json(context, 'POST', '/api/auth/tenant', { tenantId: state.tenantId });
  } else {
    const tenant = await json(context, 'POST', '/api/platform/tenants', { name: 'Oficina Simulada', slug: `simulada-${Date.now()}`, vertical: 'Automotive', adminName: 'Operador', email: 'fiscal@smoke.test', modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'] });
    await json(context, 'POST', '/api/auth/tenant', { tenantId: tenant.id });
    await json(context, 'POST', '/api/tenant/onboarding/complete', {});
    const address = { street: 'Rua do Emitente', number: '10', district: 'Centro', city: 'Igaraçu do Tietê', cityCode: '3520004', state: 'SP', postalCode: '17350000' };
    await json(context, 'PUT', '/api/fiscal/settings', { cnpj: '11222333000181', legalName: 'Oficina Simulada', stateRegistration: '110042490114', municipalRegistration: '', regime: 'SimplesNacional', address, nfeEnabled: true, nfseEnabled: true, environment: 'Homologation', nfeSeries: 1, dpsSeries: 1 });
    await page.goto('/configuracoes');
    await page.getByRole('tab', { name: 'Fiscal', exact: true }).click();
    const button = page.getByRole('button', { name: 'Baixar certificado A1 de teste (Dev)', exact: true });
    await expect(button).toBeVisible();
    for (const width of [1440, 768, 320]) {
      await page.setViewportSize({ width, height: 1000 });
      await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      expect((await button.boundingBox())!.height).toBeGreaterThanOrEqual(44);
    }
    const download = page.waitForEvent('download');
    await button.click();
    const downloaded = await download;
    await page.locator('#fiscal-file').setInputFiles((await downloaded.path())!);
    await page.locator('#fiscal-password').fill('teste123');
    await page.getByRole('button', { name: 'Cadastrar ou substituir certificado', exact: true }).click();
    await expect(page.getByText(/Validade:/)).toBeVisible();
    const customer = await json(context, 'POST', '/api/customers', { name: 'Tomador Simulado', document: '12345678909' });
    const vehicle = await json(context, 'POST', '/api/vehicles', { customerId: customer.id, plate: 'SIM1A23', model: 'Teste' });
    const order = await json(context, 'POST', '/api/work-orders', { customerId: customer.id, vehicleId: vehicle.id, services: [{ description: 'Revisão', quantity: 1, unitPrice: 100 }], parts: [{ description: 'Pneu', code: 'PN-01', quantity: 1, unitPrice: 300 }] });
    for (const status of ['InProgress', 'Completed']) await json(context, 'PATCH', `/api/work-orders/${order.id}/status`, { status });
    await json(context, 'PUT', `/api/work-orders/${order.id}/fiscal`, {
      name: 'Tomador Simulado', document: '12345678909', address: { ...address, street: 'Rua do Tomador' }, competence: new Date().toISOString().slice(0, 10),
      recipientIeIndicator: '9', paymentCode: '01', paymentAmount: 300,
      products: { [order.parts[0].id]: { ncm: '40111000', origin: '0', unit: 'UN', gtin: 'SEM GTIN', cfop: '5102', csosn: '102', pisCst: '07', cofinsCst: '07' } },
      services: { [order.services[0].id]: { nationalCode: '140101', approximateTaxRate: 6 } },
    });
    await page.goto('/ordens');
    await page.getByText('Tomador Simulado', { exact: true }).first().click();
    const zipDownload = page.waitForEvent('download');
    await page.getByRole('button', { name: /Emitir documentos fiscais/ }).click();
    await expect(page.getByText('Documentos emitidos', { exact: true })).toBeVisible();
    expect((await zipDownload).suggestedFilename()).toMatch(/\.zip$/);
    const result = await json(context, 'GET', `/api/work-orders/${order.id}/fiscal`);
    state = { tenantId: tenant.id, orderId: order.id, documentIds: result.documents.map((d: { id: string }) => d.id) };
    const nfseCard = page.locator('.fiscal-documents article').filter({ hasText: 'NFS-e · Serviços' });
    await nfseCard.getByRole('button', { name: 'Cancelar nota', exact: true }).click();
    await page.locator('#fiscal-cancel-reason').fill('Cancelamento fictício para teste local pelo Nginx');
    await page.getByRole('button', { name: 'Confirmar cancelamento', exact: true }).click();
    await expect(nfseCard.getByText('Cancelado · Homologação', { exact: true })).toBeVisible();
    writeFileSync(simulationStatePath, JSON.stringify(state), { mode: 0o600 });
  }
  const result = await json(context, 'GET', `/api/work-orders/${state.orderId}/fiscal`);
  expect(result.documents.map((d: { id: string }) => d.id).sort()).toEqual([...state.documentIds].sort());
  expect(result.documents.find((d: { kind: string }) => d.kind === 'Nfse').state).toBe('Cancelled');
  expect(result.documents.find((d: { kind: string }) => d.kind === 'Nfe').state).toBe('Authorized');
  for (const document of result.documents) {
    expect((await api(context, 'GET', `/api/fiscal/documents/${document.id}/xml`)).status()).toBe(200);
    const pdf = await api(context, 'GET', `/api/fiscal/documents/${document.id}/pdf`);
    expect(pdf.status()).toBe(200);
    expect((await pdf.body()).subarray(0, 5).toString()).toBe('%PDF-');
    writeFileSync(`/tmp/ofizzy-smoke-${document.kind}.pdf`, await pdf.body());
  }
});
