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

async function mockApi(page: Page, requestCounts?: Map<string, number>): Promise<void> {
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    requestCounts?.set(path, (requestCounts.get(path) ?? 0) + 1);
    let body: unknown = {};
    if (path === '/api/setup/status') body = { required: false };
    else if (path === '/api/auth/me' || path === '/api/auth/refresh')
      body = { id: 'user-1', name: 'Usuário Teste', email: 'teste@example.com', isPlatformAdmin: false, tenant: { id: 'tenant-1', name: 'Oficina Teste', slug: 'teste', status: 'Active', vertical: 'Automotive', role: 'Owner', onboardingCompleted: true, modules: ['Customers', 'WorkOrders', 'Catalog', 'Automotive'] } };
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
        name: 'Ofizzy',
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

test('retorno para uma listagem reutiliza os dados da sessão', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop');
  const requests = new Map<string, number>();
  await mockApi(page, requests);

  await page.goto('/clientes');
  await expect(page.getByText(customer.name).first()).toBeVisible();
  await page.locator('.desktop-sidebar a[href="/"]').first().click();
  await expect(page.locator('.desktop-sidebar a[href="/clientes"]')).toBeVisible();
  await page.locator('.desktop-sidebar a[href="/clientes"]').click();
  await expect(page.getByText(customer.name).first()).toBeVisible({ timeout: 100 });

  expect(requests.get('/api/customers')).toBe(1);
});

test('modo de visualização (cards/tabela) persiste após navegação e recarregamento', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop');
  await mockApi(page);

  await page.goto('/clientes');
  await expect(page.locator('.customers-list-table')).toBeVisible();

  // Alterna para visualização em blocos
  await page.locator('.data-toolbar__toggle-btn[aria-label="Visualização em Blocos"]').click();
  await expect(page.locator('.catalog-grid')).toBeVisible();
  await expect(page.locator('.customers-list-table')).toHaveCount(0);

  // Navega para outra tela
  await page.locator('.desktop-sidebar a[href="/veiculos"]').click();
  await expect(page.locator('.vehicles-list-table')).toBeVisible();

  // Retorna para clientes - deve manter visualização em blocos
  await page.locator('.desktop-sidebar a[href="/clientes"]').click();
  await expect(page.locator('.catalog-grid')).toBeVisible();
  await expect(page.locator('.customers-list-table')).toHaveCount(0);

  // Recarrega a página - deve continuar em blocos via localStorage
  await page.reload();
  await expect(page.locator('.catalog-grid')).toBeVisible();
  await expect(page.locator('.customers-list-table')).toHaveCount(0);
});

test('dashboard dá largura total à lista antes de comprimir cliente e veículo', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop');
  await page.setViewportSize({ width: 1180, height: 800 });
  await mockApi(page);

  await page.goto('/');

  await expect(page.locator('.dashboard-table-desktop')).toBeVisible();
  const mainBox = await page.locator('.dashboard-main-col').boundingBox();
  const sideBox = await page.locator('.dashboard-side-col').boundingBox();
  const customerColumn = await page.locator('.dashboard-work-orders-table tbody td').nth(1).boundingBox();
  expect(sideBox!.y).toBeGreaterThan(mainBox!.y + mainBox!.height - 1);
  expect(customerColumn!.width).toBeGreaterThan(200);
  await expectNoHorizontalOverflow(page);
});

test('lista de OS preserva cliente e veículo em notebook e usa cards no tablet', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name === 'mobile');
  await mockApi(page);
  await page.goto('/ordens');

  if (testInfo.project.name === 'tablet') {
    await expect(page.locator('.order-grid .order-card').first()).toBeVisible();
  } else {
    await page.setViewportSize({ width: 1180, height: 800 });
    const customerColumn = await page.locator('.work-orders-list-table tbody td').nth(1).boundingBox();
    expect(customerColumn!.width).toBeGreaterThan(200);
  }
  await expectNoHorizontalOverflow(page);
});

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
  await expect(page.getByRole('heading', { name: 'Acesse sua conta Ofizzy' })).toBeVisible();
  await expect(page.locator('.brand-mark')).toHaveCount(0);
  await page.getByRole('button', { name: 'Alternar tema de aparência' }).click();
  await expect(page.getByRole('menu')).toBeInViewport();
  await page.getByRole('menuitemradio', { name: 'Escuro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.getByRole('button', { name: 'Alternar tema de aparência' }).click();
  await page.getByRole('menuitemradio', { name: 'Claro' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
  await expectNoHorizontalOverflow(page);
});

test('configuração inicial permanece utilizável em todos os viewports', async ({ page }) => {
  await mockApi(page);
  await page.route('**/api/setup/status', (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: '{"required":true}' }),
  );
  await page.goto('/setup');
  await expect(
    page.getByRole('heading', { name: 'Inicialização da plataforma.' }),
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
  // Wait for the PrimeNG opening animation before measuring the final layout.
  await expect.poll(async () => (await dialog.boundingBox())?.width ?? 0).toBeGreaterThanOrEqual(page.viewportSize()!.width * 0.98);
  await page.getByRole('button', { name: 'Novo Serviço Avulso' }).click();
  await expect(page.locator('app-work-order-lines-editor .line-card')).toBeVisible();
});
