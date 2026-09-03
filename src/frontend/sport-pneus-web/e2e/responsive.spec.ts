import { expect, Page, test } from '@playwright/test';

const customer = {
  id: 'customer-1',
  name: 'Cliente Mobile',
  document: '12345678901',
  phone: '11999999999',
  whatsApp: null,
  email: 'cliente@example.com',
  address: 'Rua de Teste, 10',
  notes: null,
  isActive: true,
  createdAt: '2026-09-01T12:00:00Z',
};
const vehicle = {
  id: 'vehicle-1',
  customerId: customer.id,
  customerName: customer.name,
  plate: 'ABC1D23',
  brand: 'Fiat',
  model: 'Strada',
  year: 2024,
  color: 'Branca',
  mileage: 1000,
  chassis: null,
  notes: null,
  isActive: true,
  createdAt: '2026-09-01T12:00:00Z',
};
const service = {
  id: 'service-1',
  name: 'Alinhamento',
  description: null,
  defaultPrice: 120,
  isActive: true,
};
const part = {
  id: 'part-1',
  name: 'Pneu 175/70 R14',
  code: 'PN-01',
  costPrice: 200,
  salePrice: 300,
  isActive: true,
};
const order = {
  id: 'order-1',
  number: 1,
  customerId: customer.id,
  customerName: customer.name,
  customerDocument: customer.document,
  customerPhone: customer.phone,
  vehicleId: vehicle.id,
  vehiclePlate: vehicle.plate,
  vehicleDescription: 'Fiat Strada',
  mileage: 1000,
  complaint: 'Ruído',
  diagnosis: 'Alinhamento',
  notes: null,
  status: 'Open',
  servicesTotal: 120,
  partsTotal: 300,
  total: 420,
  createdAt: '2026-09-01T12:00:00Z',
  completedAt: null,
  services: [
    {
      id: 'line-1',
      catalogId: service.id,
      description: service.name,
      quantity: 1,
      unitPrice: 120,
      total: 120,
    },
  ],
  parts: [
    {
      id: 'line-2',
      catalogId: part.id,
      description: part.name,
      code: part.code,
      quantity: 1,
      unitPrice: 300,
      total: 300,
    },
  ],
};

async function mockApi(page: Page): Promise<void> {
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    let body: unknown = {};
    if (path === '/api/setup/status') body = { required: false };
    else if (path === '/api/auth/me' || path === '/api/auth/refresh')
      body = { id: 'user-1', name: 'Usuário Teste', email: 'teste@example.com' };
    else if (path === '/api/dashboard/summary')
      body = {
        totalCustomers: 1,
        totalVehicles: 1,
        totalActiveOrders: 1,
        totalCompletedOrders: 0,
        activeOrders: [order],
      };
    else if (path === '/api/customers')
      body = { items: [customer], total: 1, page: 1, pageSize: 12 };
    else if (path === '/api/vehicles') body = { items: [vehicle], total: 1, page: 1, pageSize: 12 };
    else if (path === '/api/services')
      body = { items: [service], total: 1, page: 1, pageSize: 100 };
    else if (path === '/api/parts') body = { items: [part], total: 1, page: 1, pageSize: 100 };
    else if (path === '/api/work-orders/order-1') body = order;
    else if (path === '/api/work-orders')
      body = { items: [order], total: 1, page: 1, pageSize: 12 };
    else if (path === '/api/company')
      body = {
        name: 'Sport Pneus',
        legalName: null,
        cnpj: null,
        phone: null,
        whatsApp: null,
        address: null,
        warrantyTerms: null,
      };
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(body),
    });
  });
}

async function expectNoHorizontalOverflow(page: Page): Promise<void> {
  const sizes = await page.evaluate(() => ({
    viewport: document.documentElement.clientWidth,
    content: document.documentElement.scrollWidth,
  }));
  expect(sizes.content).toBeLessThanOrEqual(sizes.viewport);
}

for (const route of ['/', '/clientes', '/veiculos', '/ordens', '/configuracoes']) {
  test(`${route} não cria rolagem horizontal`, async ({ page }) => {
    await mockApi(page);
    await page.goto(route);
    await expect(page.locator('main')).toBeVisible();
    await expectNoHorizontalOverflow(page);
  });
}

test('login permanece utilizável em todos os viewports', async ({ page }) => {
  await mockApi(page);
  await page.route('**/api/auth/{me,refresh}', (route) =>
    route.fulfill({ status: 401, body: '{}' }),
  );
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'Acessar o sistema' })).toBeVisible();
  await expectNoHorizontalOverflow(page);
});

test('configuração inicial permanece utilizável em todos os viewports', async ({ page }) => {
  await mockApi(page);
  await page.route('**/api/setup/status', (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: '{"required":true}' }),
  );
  await page.goto('/setup');
  await expect(
    page.getByRole('heading', { name: 'Configuração inicial da oficina.' }),
  ).toBeVisible();
  await expectNoHorizontalOverflow(page);
});

test('listagens usam cards e ocultam o seletor no celular', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'mobile');
  await mockApi(page);
  await page.goto('/clientes');
  await expect(page.locator('article.catalog-card').first()).toBeVisible();
  await expect(page.locator('.view-mode-toggle')).toBeHidden();
});

test('formulário de OS vira tela cheia e edita itens em cards no celular', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name !== 'mobile');
  await mockApi(page);
  await page.goto('/ordens');
  await page.getByRole('button', { name: 'Nova Ordem' }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  const box = await dialog.boundingBox();
  // PrimeNG centers the dialog using a transform, which may round a few CSS pixels.
  expect(box?.width).toBeGreaterThanOrEqual(page.viewportSize()!.width * 0.98);
  await page.getByRole('button', { name: 'Novo Serviço Avulso' }).click();
  await expect(page.locator('app-work-order-lines-editor .line-card')).toBeVisible();
});
