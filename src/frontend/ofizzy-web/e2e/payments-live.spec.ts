import { expect, test, BrowserContext, Locator } from '@playwright/test';
import { readFileSync, writeFileSync } from 'node:fs';
const statePath = '/tmp/ofizzy-payments-live.json';
const password = 'FiscalSmoke2026';
async function api(context: BrowserContext, method: string, url: string, data?: unknown) {
  await context.request.get('/api/setup/status');
  const token = (await context.cookies()).find(x => x.name === 'XSRF-TOKEN')?.value ?? '';
  const response = await context.request.fetch(url, { method, data, headers: { 'X-XSRF-TOKEN': decodeURIComponent(token) } });
  expect(response.ok(), `${method} ${url}: ${response.status()} ${await response.text()}`).toBeTruthy();
  return response.status() === 204 ? null : response.json();
}
async function money(input: Locator, value: string) {
  await input.click();
  await input.press('ControlOrMeta+A');
  await input.pressSequentially(value);
  await input.press('Tab');
}
test('recebimento, liquidação e estorno persistem e funcionam em desktop/tablet/mobile', async ({ page }) => {
  const context = page.context();
  const restart = process.env['OFIZZY_VERIFY_RESTART'] === '1';
  let state: { tenantId: string; orderId: string; paymentId: string; serviceId?: string };
  if (restart) {
    state = JSON.parse(readFileSync(statePath, 'utf8'));
    await api(context, 'POST', '/api/auth/login', { email: 'fiscal@smoke.test', password });
    await api(context, 'POST', '/api/auth/tenant', { tenantId: state.tenantId });
  } else {
    const setup = await api(context, 'GET', '/api/setup/status');
    if (setup.required) await api(context, 'POST', '/api/setup', { companyName: 'Payments Accept', adminName: 'Operador', email: 'fiscal@smoke.test', password });
    await api(context, 'POST', '/api/auth/login', { email: 'fiscal@smoke.test', password });
    const tenant = await api(context, 'POST', '/api/platform/tenants', { name: 'Oficina Pagamentos', slug: `payments-${Date.now()}`, vertical: 'Automotive', adminName: 'Operador', email: 'fiscal@smoke.test', modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'] });
    await api(context, 'POST', '/api/auth/tenant', { tenantId: tenant.id });
    await api(context, 'POST', '/api/tenant/onboarding/complete', {});
    const customer = await api(context, 'POST', '/api/customers', { name: 'Cliente Financeiro', document: '12.ABC.345/01de-35' });
    expect(customer.document).toBe('12ABC34501DE35');
    const service = await api(context, 'POST', '/api/services', { name: 'Perfil RTC rascunho', defaultPrice: 100 });
    await page.goto('/configuracoes');
    await page.getByRole('tab', { name: 'Fiscal', exact: true }).click();
    await page.locator('#fiscal-search').fill('Perfil RTC rascunho');
    await page.getByRole('button', { name: /Buscar itens$/ }).click();
    await page.getByRole('combobox', { name: 'Item do catálogo', exact: true }).click();
    await page.getByRole('option', { name: 'Serviço · Perfil RTC rascunho', exact: true }).click();
    await page.locator('#profile-nationalCode').fill('010101');
    await page.locator('#profile-rtcEnabled').click();
    await page.getByRole('option', { name: 'Configurar IBS/CBS', exact: true }).click();
    await page.locator('#profile-rtcCst').fill('200');
    await money(page.locator('#profile-rtcIbsMunicipalRate'), '0');
    const saved = page.waitForResponse(r => r.url().endsWith(`/api/services/${service.id}/fiscal`) && r.request().method() === 'PUT');
    await page.getByRole('button', { name: /Salvar classificação$/ }).click();
    expect((await saved).status()).toBe(204);
    const draft = await api(context, 'GET', `/api/services/${service.id}/fiscal`);
    expect(draft.rtcEnabled).toBe(true); expect(draft.rtcCst).toBe('200');
    expect(draft.rtcIbsMunicipalRate).toBe(0); expect(draft.rtcCbsRate).toBeNull();
    for (const width of [1440, 768, 320]) {
      await page.setViewportSize({ width, height: 1000 });
      await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      expect((await page.locator('#profile-rtcCst').boundingBox())!.height).toBeGreaterThanOrEqual(44);
    }
    const vehicle = await api(context, 'POST', '/api/vehicles', { customerId: customer.id, plate: 'PAY1A23', model: 'Teste' });
    const order = await api(context, 'POST', '/api/work-orders', { customerId: customer.id, vehicleId: vehicle.id, services: [{ description: 'Revisão', quantity: 1, unitPrice: 100 }], parts: [] });
    for (const status of ['InProgress', 'Completed']) await api(context, 'PATCH', `/api/work-orders/${order.id}/status`, { status });
    await page.goto('/ordens');
    await page.getByText('Cliente Financeiro', { exact: true }).first().click();
    await page.getByRole('button', { name: /Registrar recebimento$/ }).click();
    const dialog = page.getByRole('dialog', { name: /Registrar recebimento$/ });
    for (const width of [1440, 768, 320]) {
      await page.setViewportSize({ width, height: 1000 });
      await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      await expect.poll(async () => (await dialog.locator('#payment-amount').boundingBox())!.height).toBeGreaterThanOrEqual(44);
      await expect.poll(async () => (await dialog.getByRole('button', { name: /Salvar registro$/ }).boundingBox())!.height).toBeGreaterThanOrEqual(44);
    }
    await money(dialog.locator('#payment-amount'), '60');
    await dialog.getByRole('button', { name: /Salvar registro$/ }).click();
    await expect(dialog).not.toBeVisible();
    await page.getByRole('button', { name: /Registrar liquidação$/ }).click();
    const settlement = page.getByRole('dialog', { name: /Registrar liquidação$/ });
    await money(settlement.locator('#payment-amount'), '30');
    await money(settlement.locator('#payment-fees'), '1');
    await settlement.getByRole('button', { name: /Salvar registro$/ }).click();
    await expect(settlement).not.toBeVisible();
    await page.getByRole('button', { name: /Registrar estorno$/ }).click();
    const reversal = page.getByRole('dialog', { name: /Registrar estorno$/ });
    await money(reversal.locator('#payment-amount'), '10');
    await reversal.locator('#payment-reason').fill('Estorno manual confirmado no provedor');
    await reversal.getByRole('button', { name: /Salvar registro$/ }).click();
    await expect(reversal).not.toBeVisible();
    const payments = await api(context, 'GET', `/api/work-orders/${order.id}/payments`);
    state = { tenantId: tenant.id, orderId: order.id, paymentId: payments.payments[0].id, serviceId: service.id };
    writeFileSync(statePath, JSON.stringify(state), { mode: 0o600 });
  }
  if (state.serviceId) {
    const draft = await api(context, 'GET', `/api/services/${state.serviceId}/fiscal`);
    expect(draft.rtcEnabled).toBe(true); expect(draft.rtcCst).toBe('200');
    expect(draft.rtcIbsMunicipalRate).toBe(0); expect(draft.rtcCbsRate).toBeNull();
  }
  const payments = await api(context, 'GET', `/api/work-orders/${state.orderId}/payments`);
  expect(payments.balance).toBe(80);
  expect(payments.payments).toHaveLength(1);
  expect(payments.payments[0].id).toBe(state.paymentId);
  expect(payments.payments[0].movements).toHaveLength(2);
  expect(payments.payments[0].movements[0].segregatedTax).toBeNull();
  await page.goto('/ordens');
  await page.getByText('Cliente Financeiro', { exact: true }).first().click();
  const section = page.getByRole('region', { name: 'Recebimentos da OS' });
  await expect(section.getByText('Não informada', { exact: false })).toBeVisible();
  for (const width of [1440, 768, 320]) {
    await page.setViewportSize({ width, height: 1000 });
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    await expect.poll(async () => (await page.getByRole('button', { name: /Registrar recebimento$/ }).boundingBox())!.height).toBeGreaterThanOrEqual(44);
  }
});
