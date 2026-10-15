// Real UI + API only. Fixed loopback origin; never use a shared database.
const { createRequire } = require('node:module');
const { resolve } = require('node:path');
const { mkdirSync, readFileSync, writeFileSync } = require('node:fs');
const { randomUUID } = require('node:crypto');
const assert = require('node:assert/strict');
const frontendRequire = createRequire(resolve(__dirname, '../src/frontend/ofizzy-web/package.json'));
const { chromium, expect } = frontendRequire('@playwright/test');
const baseURL = 'http://127.0.0.1:18091';
const email = 'showcase@example.test';
const password = process.env.OFIZZY_DEMO_PASSWORD;
const statePath = resolve(__dirname, '../test-results/github-demo/state.json');
const assets = resolve(__dirname, '../docs/assets/screenshots');
const verify = process.argv.includes('--verify-restart');
const existing = verify || process.argv.includes('--capture-existing');
const capture = !verify && !process.argv.includes('--smoke');

async function main() {
  assert(password?.length >= 12, 'Set OFIZZY_DEMO_PASSWORD (12+ characters) for this isolated fixture.');
  mkdirSync(assets, { recursive: true });
  mkdirSync(resolve(__dirname, '../test-results/github-demo'), { recursive: true });
  const browser = await chromium.launch();
  try {
    const context = await browser.newContext({ baseURL, viewport: { width: 1920, height: 1080 }, colorScheme: 'light' });
    async function api(method, url, data) {
      await context.request.get('/api/setup/status');
      const token = (await context.cookies()).find(x => x.name === 'XSRF-TOKEN')?.value ?? '';
      const response = await context.request.fetch(url, { method, data, headers: { 'X-XSRF-TOKEN': decodeURIComponent(token) } });
      assert(response.ok(), `${method} ${url}: ${response.status()} ${await response.text()}`);
      return response.status() === 204 ? null : response.json();
    }
    let state;
    if (existing) {
      state = JSON.parse(readFileSync(statePath, 'utf8'));
      await api('POST', '/api/auth/login', { email, password });
      await api('POST', '/api/auth/tenant', { tenantId: state.tenantId });
    } else {
      assert((await api('GET', '/api/setup/status')).required, 'Seed requires an EMPTY isolated database. Use --verify-restart for existing fixtures.');
      await api('POST', '/api/setup', { companyName: 'Ofizzy Demo', adminName: 'Equipe demonstração', email, password });
      await api('POST', '/api/auth/login', { email, password });
      const tenant = await api('POST', '/api/platform/tenants', {
        name: 'Oficina Horizonte · Demo', slug: 'github-showcase', vertical: 'Automotive',
        adminName: 'Equipe demonstração', email, modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'],
      });
      await api('POST', '/api/auth/tenant', { tenantId: tenant.id });
      await api('POST', '/api/tenant/onboarding/complete', {});
      const services = [];
      for (const [name, defaultPrice] of [['Revisão preventiva', 280], ['Alinhamento e balanceamento', 160], ['Troca de óleo e filtros', 120], ['Diagnóstico eletrônico', 180]]) {
        services.push(await api('POST', '/api/services', { name, defaultPrice }));
      }
      const parts = [];
      for (const [name, code, salePrice] of [['Óleo sintético 5W30 · 1 L', 'OL-530', 58], ['Filtro de óleo', 'FL-010', 48], ['Pastilhas de freio dianteiras', 'FR-020', 185]]) {
        parts.push(await api('POST', '/api/parts', { name, code, salePrice, costPrice: salePrice / 2 }));
      }
      const names = ['Ana Martins · Demo', 'Bruno Costa · Demo', 'Carla Oliveira · Demo', 'Diego Santos · Demo', 'Elisa Lima · Demo', 'Felipe Rocha · Demo', 'Gabriela Alves · Demo', 'Hugo Pereira · Demo'];
      const cars = [['Toyota', 'Corolla', 'DEM1A01'], ['Honda', 'Civic', 'DEM1A02'], ['Volkswagen', 'Polo', 'DEM1A03'], ['Chevrolet', 'Onix', 'DEM1A04'], ['Fiat', 'Argo', 'DEM1A05'], ['Hyundai', 'HB20', 'DEM1A06'], ['Renault', 'Duster', 'DEM1A07'], ['Nissan', 'Kicks', 'DEM1A08']];
      const orders = [];
      for (let i = 0; i < names.length; i++) {
        const customer = await api('POST', '/api/customers', { name: names[i], email: `cliente${i + 1}@example.test`, notes: 'Cadastro fictício para apresentação do projeto.' });
        const [brand, model, plate] = cars[i];
        const vehicle = await api('POST', '/api/vehicles', { customerId: customer.id, brand, model, plate, year: 2023, color: 'Prata', mileage: 32000 + i * 4500 });
        const service = services[i % services.length];
        const order = await api('POST', '/api/work-orders', {
          customerId: customer.id, vehicleId: vehicle.id, mileage: vehicle.mileage,
          complaint: i === 0 ? 'Revisão preventiva e verificação do sistema de freios.' : 'Inspeção periódica e manutenção preventiva.',
          diagnosis: 'Inspeção realizada. Substituição dos itens de desgaste e revisão dos pontos de segurança.',
          notes: 'Dados fictícios · ambiente de demonstração.',
          services: [{ catalogId: service.id, description: service.name, quantity: 1, unitPrice: service.defaultPrice }, { catalogId: services[1].id, description: services[1].name, quantity: 1, unitPrice: 160 }],
          parts: [{ catalogId: parts[0].id, description: parts[0].name, code: parts[0].code, quantity: 4, unitPrice: 58 }, { catalogId: parts[1].id, description: parts[1].name, code: parts[1].code, quantity: 1, unitPrice: 48 }],
        });
        if (i % 3 !== 0) await api('PATCH', `/api/work-orders/${order.id}/status`, { status: 'InProgress' });
        if (i % 3 === 2) await api('PATCH', `/api/work-orders/${order.id}/status`, { status: 'Completed' });
        orders.push(order);
      }
      const completed = orders[2];
      const payment = await api('POST', `/api/work-orders/${completed.id}/payments`, { requestId: randomUUID(), amount: completed.total, method: 'Pix', receivedAt: new Date().toISOString() });
      await api('POST', `/api/payments/${payment.id}/settlements`, { requestId: randomUUID(), amount: completed.total - 100, fees: 0, segregatedTax: null, settledAt: new Date().toISOString() });
      state = { tenantId: tenant.id, orders: orders.map(x => ({ id: x.id, number: x.number, total: x.total })), paymentId: payment.id };
      writeFileSync(statePath, JSON.stringify(state, null, 2)); // IDs only; never sessions or credentials.
    }
    const summary = await api('GET', '/api/dashboard/summary');
    assert.equal(summary.totalCustomers, 8);
    assert.equal(summary.totalVehicles, 8);
    assert.equal(summary.totalActiveOrders, 6);
    assert.equal(summary.totalCompletedOrders, 2);
    const payments = await api('GET', `/api/work-orders/${state.orders[2].id}/payments`);
    assert.equal(payments.payments[0].id, state.paymentId);
    assert.equal(payments.balance, 100);
    const pdf = await context.request.get(`/api/work-orders/${state.orders[0].id}/pdf`);
    assert(pdf.ok());
    assert.equal((await pdf.body()).subarray(0, 4).toString(), '%PDF');
    const page = await context.newPage();
    const failedRequests = [];
    page.on('response', r => { if (r.url().includes('/api/') && r.status() >= 400) failedRequests.push(`${r.status()} ${new URL(r.url()).pathname}`); });
    async function ready() {
      await page.waitForLoadState('networkidle');
      await page.evaluate(() => document.fonts.ready);
      await expect(page.locator('app-loading-state')).toHaveCount(0);
      await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(page.viewportSize().width);
    }
    async function screenshot(name, locator = page) {
      await ready();
      if (capture) await locator.screenshot({ path: resolve(assets, name), animations: 'disabled' });
    }
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'Visão Geral da Oficina', exact: true })).toBeVisible();
    await screenshot('dashboard.png');
    await page.goto('/ordens');
    await expect(page.getByText('Ana Martins · Demo', { exact: true }).first()).toBeVisible();
    await screenshot('work-orders.png');
    await page.setViewportSize({ width: 1920, height: 1280 });
    await page.getByRole('row', { name: 'Abrir detalhes da OS #0001', exact: true }).getByRole('button', { name: 'Editar', exact: true }).click();
    await expect(page.getByRole('heading', { name: /Editar OS/ })).toBeVisible();
    await screenshot('work-order-editor.png', page.getByRole('dialog'));
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/ordens');
    await page.getByRole('row', { name: 'Abrir detalhes da OS #0003', exact: true }).click();
    const region = page.getByRole('region', { name: 'Recebimentos da OS' });
    await expect(region.getByText(/Saldo da OS/)).toBeVisible();
    await screenshot('payments.png', region);
    const measurements = [];
    for (const width of [1440, 768, 320, 390]) {
      await page.setViewportSize({ width, height: width < 640 ? 844 : 1000 });
      await page.goto('/ordens');
      await expect(page.getByText('Ana Martins · Demo', { exact: true }).first()).toBeVisible();
      await ready();
      const controls = page.locator('main button:visible, main input:visible, main .p-select:visible, main a:visible');
      const compactCount = () => controls.evaluateAll(elements => elements.filter(e => e.getBoundingClientRect().height < 43.5 && e.type !== 'checkbox').length);
      measurements.push({ width, scrollWidth: await page.evaluate(() => document.documentElement.scrollWidth), controlsBelow44px: await compactCount() });
      if (width === 390) {
        await page.goto('/');
        await expect(page.getByRole('heading', { name: 'Visão Geral da Oficina', exact: true })).toBeVisible();
        await screenshot('mobile.png');
      }
    }
    assert.deepEqual(failedRequests, []);
    writeFileSync(resolve(__dirname, `../test-results/github-demo/${verify ? 'restart' : 'capture'}-evidence.json`), JSON.stringify({ baseURL, summary, balance: payments.balance, measurements, failedRequests, verifiedAt: new Date().toISOString() }, null, 2));
    console.log(`${verify ? 'Restart verified' : capture ? 'Five screenshots captured' : 'Smoke verified'}: 8 customers, 8 vehicles, 8 orders, PDF, payments, responsive layout; no API errors.`);
    await context.close();
  } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
