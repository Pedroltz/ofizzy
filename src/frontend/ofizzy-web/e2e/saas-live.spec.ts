import { expect, test, Page, BrowserContext } from '@playwright/test';
import { writeFileSync, readFileSync } from 'node:fs';
const password = 'SaasSmoke2026'; // Fictitious isolated acceptance database only.
const baseURL = process.env['OFIZZY_E2E_BASE_URL'] ?? 'http://127.0.0.1:18081';
async function api(context: BrowserContext, method: string, url: string, data?: unknown) {
  await context.request.get('/api/setup/status');
  const xsrf = (await context.cookies()).find(x => x.name === 'XSRF-TOKEN')?.value ?? '';
  return context.request.fetch(url, { method, data, headers: { 'X-XSRF-TOKEN': decodeURIComponent(xsrf) } });
}
async function json(context: BrowserContext, method: string, url: string, data?: unknown) {
  const response = await api(context, method, url, data);
  expect(response.ok(), `${method} ${url}: ${response.status()} ${await response.text()}`).toBeTruthy();
  return response.status() === 204 ? null : response.json();
}
async function layout(page: Page, prefix: string) {
  for (const width of [1440, 768, 320]) {
    await page.setViewportSize({ width, height: 1000 });
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    const small = await page.locator('main button:visible, main input:visible, main .p-select:visible, main a:visible').evaluateAll(elements => elements.filter(e => {
      if ((e as HTMLInputElement).type === 'checkbox') return false; // Its associated label is the touch target.
      return e.getBoundingClientRect().height < 43.5;
    }).map(e => e.outerHTML.slice(0, 120)));
    expect(small).toEqual([]);
    await page.screenshot({ path: `/tmp/ofizzy-live-results/${prefix}-${width}.png`, fullPage: true });
  }
  await page.setViewportSize({ width: 1440, height: 900 });
}
async function seed(context: BrowserContext, name: string, plate: string) {
  const customer = await json(context, 'POST', '/api/customers', { name, document: '12345678909' });
  const vehicle = await json(context, 'POST', '/api/vehicles', { customerId: customer.id, plate, model: 'Modelo smoke' });
  const body = { customerId: customer.id, vehicleId: vehicle.id, complaint: 'Inspeção', services: [{ description: 'Revisão', quantity: 1, unitPrice: 100 }], parts: [] };
  const order = await json(context, 'POST', '/api/work-orders', body);
  expect(order.number).toBe(1);
  const updated = await json(context, 'PUT', '/api/work-orders/' + order.id, { ...body, diagnosis: 'Diagnóstico persistido' });
  expect(updated.diagnosis).toBe('Diagnóstico persistido');
  const pdf = await context.request.get('/api/work-orders/' + order.id + '/pdf');
  expect(pdf.status()).toBe(200); expect((await pdf.body()).subarray(0, 4).toString()).toBe('%PDF');
  return { customer, vehicle, order };
}

test('SaaS real pelo Nginx: provisionamento, onboarding, Alpha/Beta e persistência', async ({ page, browser }) => {
  test.skip(process.env['OFIZZY_VERIFY_RESTART'] === '1');
  await page.goto('/setup');
  await page.getByLabel('Nome completo').fill('Operador smoke');
  await page.getByLabel('E-mail corporativo').fill('platform@smoke.test');
  await page.getByLabel('Senha de acesso').fill(password);
  await page.getByRole('button', { name: 'Concluir configuração' }).click();
  await expect(page.getByRole('heading', { name: 'Administração da plataforma' })).toBeVisible();
  await layout(page, 'platform');
  await page.getByLabel('Nome', { exact: true }).fill('Mecânica Alpha');
  await page.getByLabel('Identificador').fill('alpha');
  await page.getByLabel('Nome do administrador').fill('Proprietário Alpha');
  await page.getByLabel('E-mail do administrador').fill('alpha@smoke.test');
  await page.getByLabel('Senha inicial').fill(password);
  await page.getByRole('button', { name: 'Criar tenant', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Mecânica Alpha' })).toBeVisible();
  const alpha = (await json(page.context(), 'GET', '/api/platform/tenants'))[0];
  const beta = await json(page.context(), 'POST', '/api/platform/tenants', { name: 'Mecânica Beta', slug: 'beta', vertical: 'Automotive', adminName: 'Proprietário Beta', email: 'beta@smoke.test', password, modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'] });
  const aContext = await browser.newContext({ baseURL }); const bContext = await browser.newContext({ baseURL });
  const a = await aContext.newPage(); const b = await bContext.newPage();
  for (const [tab, email, name] of [[a, 'alpha@smoke.test', 'Mecânica Alpha'], [b, 'beta@smoke.test', 'Mecânica Beta']] as const) {
    await tab.goto('/login'); await tab.locator('[formControlName="email"]').fill(email); await tab.locator('[formControlName="password"]').fill(password);
    await tab.locator('button[type="submit"]').click();
    await expect(tab.getByRole('heading', { name: 'Configure sua empresa' })).toBeVisible();
    await expect(tab.locator('[formControlName="name"]')).toHaveValue(name);
    await tab.locator('[formControlName="legalName"]').fill(name + ' Serviços');
    await tab.getByRole('button', { name: 'Salvar Dados da Oficina' }).click();
    if (tab === a) await layout(tab, 'onboarding');
    await tab.getByRole('button', { name: 'Concluir onboarding' }).click();
    await expect(tab).toHaveURL(baseURL + '/');
  }
  const ar = await seed(aContext, 'João', 'ABC1D23'); const br = await seed(bContext, 'Maria', 'XYZ9Z99');
  for (const [context, own, other] of [[aContext, ar, br], [bContext, br, ar]] as const) {
    for (const [route, field] of [['customers', 'customer'], ['vehicles', 'vehicle'], ['work-orders', 'order']] as const) {
      const items = await json(context, 'GET', '/api/' + route);
      expect(items.total).toBe(1); expect(items.items[0].id).toBe(own[field].id);
      expect((await context.request.get('/api/' + route + '/' + other[field].id)).status()).toBe(404);
    }
    expect((await json(context, 'GET', '/api/dashboard/summary')).totalActiveOrders).toBe(1);
  }
  await a.goto('/ordens'); await expect(a.getByText('João', { exact: true }).first()).toBeVisible();
  await a.getByText('João', { exact: true }).first().click();
  await expect(a.getByRole('button', { name: 'Imprimir' })).toBeVisible();
  await a.getByRole('button', { name: 'Imprimir' }).click();
  await a.emulateMedia({ media: 'print' }); await expect(a.locator('.wo-print-sheet')).toBeVisible();
  await a.emulateMedia({ media: 'screen' });
  await a.goto('/organizacoes'); await expect(a.getByRole('heading', { name: 'Suas organizações' })).toBeVisible();
  await layout(a, 'organizations');
  // Persist only nonsecret fixture identifiers, never cookies or session storage.
  writeFileSync('/tmp/ofizzy-saas-live.json', JSON.stringify({ alpha, beta, ar, br }));
  await aContext.close(); await bContext.close();
});

test('os dados dos dois tenants sobrevivem ao reinício', async ({ browser }) => {
  test.skip(process.env['OFIZZY_VERIFY_RESTART'] !== '1');
  const state = JSON.parse(readFileSync('/tmp/ofizzy-saas-live.json', 'utf8'));
  for (const [email, own] of [['alpha@smoke.test', state.ar], ['beta@smoke.test', state.br]]) {
    const context = await browser.newContext({ baseURL });
    await json(context, 'POST', '/api/auth/login', { email, password });
    const order = await json(context, 'GET', '/api/work-orders/' + own.order.id);
    expect(order.number).toBe(1); expect(order.diagnosis).toBe('Diagnóstico persistido');
    expect((await json(context, 'GET', '/api/customers')).items[0].id).toBe(own.customer.id);
    await context.close();
  }
});
