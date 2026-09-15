import { expect, test, Page } from '@playwright/test';
const modules = ['Customers', 'WorkOrders', 'Catalog', 'Automotive'];
const tenant = { id: 'alpha', name: 'Empresa Alpha', slug: 'alpha', status: 'Pending', vertical: 'Automotive', onboardingCompletedAt: null, modules, fiscalProductionReleased: false, fiscalProductionReleasedAt: null };
const operator = { id: 'operator', name: 'Operador', email: 'operator@example.test', isPlatformAdmin: true, tenant: null };
async function base(page: Page, user: unknown = operator) {
  if (test.info().project.name === 'mobile') await page.setViewportSize({ width: 320, height: 800 });
  await page.route('**/api/**', route => {
    const path = new URL(route.request().url()).pathname;
    return route.fulfill({ json: path === '/api/auth/me' ? user : path === '/api/setup/status' ? { required: false } : [] });
  });
}
async function layout(page: Page, name: string) {
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
  const small = await page.locator('main button:visible, main input:not([type=checkbox]):visible, .saas-dialog button:visible, .saas-dialog input:not([type=checkbox]):visible').evaluateAll(nodes => nodes.filter(n => n.getBoundingClientRect().height < 43.5).map(n => n.outerHTML.slice(0, 100)));
  expect(small).toEqual([]);
  await page.screenshot({ path: `/tmp/ofizzy-admin-${name}-${test.info().project.name}.png`, fullPage: true, animations: 'disabled' });
}
async function createForm(page: Page) {
  await page.getByRole('button', { name: 'Nova empresa', exact: true }).first().click();
  await expect(page.getByRole('dialog', { name: 'Nova empresa', exact: true })).toBeVisible();
  await page.getByLabel('Nome da empresa', { exact: true }).fill('Empresa Alpha');
  await page.getByLabel('Identificador', { exact: true }).fill('Alpha');
  await page.getByLabel('Nome do administrador', { exact: true }).fill('Administrador Teste');
  await page.getByLabel('E-mail do administrador', { exact: true }).fill('admin@example.test');
  await page.getByLabel('Senha inicial', { exact: true }).fill('Test-password-123');
}

test('criação valida campos, módulos, erros da API e normaliza identificador', async ({ page }) => {
  await base(page); let posted: Record<string, unknown> | undefined; let reject = true; let created = false;
  await page.route('**/api/platform/tenants', route => {
    if (route.request().method() === 'POST') {
      posted = route.request().postDataJSON();
      if (reject) return route.fulfill({ status: 409, json: { title: 'Identificador já utilizado.' } });
      created = true; return route.fulfill({ status: 201, json: tenant });
    }
    return route.fulfill({ json: created ? [tenant] : [] });
  });
  await page.goto('/plataforma');
  await layout(page, 'empty');
  await createForm(page);
  await page.getByLabel('Nome da empresa', { exact: true }).fill('');
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  await expect(page.getByText('Informe o nome da empresa.')).toBeVisible(); expect(posted).toBeUndefined();
  await page.getByLabel('Nome da empresa', { exact: true }).fill('Empresa Alpha');
  await page.getByLabel('Senha inicial', { exact: true }).fill('only-lowercase');
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  await expect(page.getByText('Use entre 10 e 200 caracteres, com maiúscula, minúscula e número.')).toBeVisible();
  await page.getByLabel('Senha inicial', { exact: true }).fill('Test-password-123');
  await page.getByRole('checkbox', { name: 'Clientes', exact: true }).uncheck();
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  await expect(page.getByText('Automotive exige Clientes. Ordens de serviço exige Clientes, Catálogo e Automotive.')).toBeVisible();
  await page.getByRole('checkbox', { name: 'Clientes', exact: true }).check();
  for (const scheme of ['light', 'dark'] as const) { await page.emulateMedia({ colorScheme: scheme }); await expect(page.locator('html')).toHaveAttribute('data-theme', scheme); await layout(page, 'create-' + scheme); }
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  await expect(page.getByText('Identificador já utilizado.')).toBeVisible();
  await expect(page.getByLabel('Nome da empresa', { exact: true })).toHaveValue('Empresa Alpha');
  reject = false;
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  await expect(page.getByRole('dialog', { name: 'Nova empresa', exact: true })).toBeHidden();
  expect(posted?.['slug']).toBe('alpha');
  await expect(page.getByText('Empresa Alpha', { exact: true })).toBeVisible();
  await layout(page, 'list');
  await page.getByRole('searchbox').fill('inexistente');
  await expect(page.getByText('Nenhuma empresa encontrada')).toBeVisible();
});

test('edição explica ativação e confirma suspensão sem alterar lista antes de salvar', async ({ page }) => {
  await base(page); let saved: unknown; let current = { ...tenant };
  await page.route('**/api/platform/tenants', route => route.fulfill({ json: [current] }));
  await page.route('**/api/platform/tenants/alpha', route => { saved = route.request().postDataJSON(); current = { ...current, ...(saved as object) }; return route.fulfill({ json: current }); });
  await page.goto('/plataforma');
  await page.getByRole('button', { name: /Gerenciar/ }).click();
  await expect(page.getByText('A empresa precisa concluir a configuração inicial antes de ser ativada.', { exact: false })).toBeVisible();
  await page.locator('#tenant-status').click();
  await expect(page.getByRole('option', { name: 'Ativa', exact: true })).toHaveAttribute('data-p-disabled', 'true');
  await page.getByRole('option', { name: 'Suspensa', exact: true }).click();
  await page.getByRole('button', { name: 'Salvar alterações', exact: true }).click();
  await expect(page.getByRole('alertdialog', { name: 'Suspender empresa' })).toBeVisible();
  await page.getByRole('button', { name: 'Voltar', exact: true }).click(); expect(saved).toBeUndefined();
  await page.getByRole('button', { name: 'Salvar alterações', exact: true }).click();
  await page.getByRole('button', { name: 'Confirmar', exact: true }).click();
  await expect(page.getByRole('dialog', { name: 'Gerenciar empresa', exact: true })).toBeHidden();
  expect(saved).toEqual({ status: 'Suspended', modules, fiscalProductionReleased: false });
});

test('edição permite liberar ambiente de produção fiscal para a organização', async ({ page }) => {
  await base(page); let saved: unknown; let current = { ...tenant };
  await page.route('**/api/platform/tenants', route => route.fulfill({ json: [current] }));
  await page.route('**/api/platform/tenants/alpha', route => { saved = route.request().postDataJSON(); current = { ...current, ...(saved as object), fiscalProductionReleased: true, fiscalProductionReleasedAt: '2026-09-15T15:00:00Z' }; return route.fulfill({ json: current }); });
  await page.goto('/plataforma');
  await page.getByRole('button', { name: /Gerenciar/ }).click();
  await page.getByRole('checkbox', { name: /Liberar ambiente de produção fiscal/ }).check();
  await page.getByRole('button', { name: 'Salvar alterações', exact: true }).click();
  await expect(page.getByRole('dialog', { name: 'Gerenciar empresa', exact: true })).toBeHidden();
  expect(saved).toEqual({ status: 'Pending', modules, fiscalProductionReleased: true });
});

test('organizações mostra ausência de vínculo e permite navegar na plataforma', async ({ page }) => {
  await base(page); await page.goto('/organizacoes');
  await expect(page.getByText('Nenhuma empresa vinculada à sua conta')).toBeVisible();
  await layout(page, 'organizations');
  await page.getByRole('button', { name: 'Administrar empresas', exact: true }).click();
  await expect(page).toHaveURL(/\/plataforma$/);
  await expect(page.getByRole('heading', { name: 'Administração da plataforma' })).toBeVisible();
});

test('onboarding salva antes de concluir e permite nova tentativa sem perder dados', async ({ page }) => {
  const user = { ...operator, isPlatformAdmin: false, tenant: { ...tenant, role: 'Owner', onboardingCompleted: false } };
  await base(page, user); const calls: string[] = []; let fail = true;
  await page.route('**/api/company', route => { if (route.request().method() === 'PUT') calls.push('save'); return route.fulfill({ json: { name: 'Empresa Alpha' } }); });
  await page.route('**/api/tenant/onboarding/complete', route => { calls.push('complete'); return route.fulfill({ status: fail ? 409 : 200, json: { title: 'Conclusão temporariamente indisponível.' } }); });
  await page.goto('/onboarding');
  await page.getByLabel('Razão Social', { exact: true }).fill('Alpha Serviços');
  await layout(page, 'onboarding');
  await page.getByRole('button', { name: 'Salvar e concluir configuração' }).click();
  await expect(page.getByText('Conclusão temporariamente indisponível.')).toBeVisible();
  expect(calls).toEqual(['save', 'complete']);
  await expect(page.getByLabel('Razão Social', { exact: true })).toHaveValue('Alpha Serviços');
  fail = false;
  await page.route('**/api/auth/me', route => route.fulfill({ json: { ...user, tenant: { ...user.tenant, status: 'Active', onboardingCompleted: true, modules: ['Customers'] } } }));
  await page.getByRole('button', { name: 'Salvar e concluir configuração' }).click();
  await expect(page).toHaveURL(/\/configuracoes$/); expect(calls).toEqual(['save', 'complete', 'save', 'complete']);
});

test('usuário comum não acessa administração e colaborador não configura empresa', async ({ page }) => {
  await base(page, { ...operator, isPlatformAdmin: false, tenant: { ...tenant, role: 'Member', onboardingCompleted: false } });
  await page.goto('/plataforma'); await expect(page).toHaveURL(/\/organizacoes$/);
  await expect(page.getByRole('button', { name: 'Administrar empresas' })).toHaveCount(0);
  await page.goto('/onboarding');
  await expect(page.getByText('O proprietário ou administrador desta empresa precisa concluir', { exact: false })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Salvar e concluir configuração' })).toHaveCount(0);
});

test('organizações permite tentar novamente e selecionar entre múltiplos vínculos', async ({ page }) => {
  await base(page, { ...operator, isPlatformAdmin: false }); let failed = true; let selected: unknown;
  await page.route('**/api/auth/tenants', route => failed ? route.fulfill({ status: 503, json: { title: 'Serviço indisponível' } }) : route.fulfill({ json: [{ ...tenant, role: 'Owner', onboardingCompleted: false }, { ...tenant, id: 'beta', name: 'Empresa Beta', role: 'Owner', onboardingCompleted: false }] }));
  await page.route('**/api/auth/tenant', route => { selected = route.request().postDataJSON(); return route.fulfill({ json: {} }); });
  await page.goto('/organizacoes');
  await expect(page.getByText('Não foi possível carregar suas empresas', { exact: true })).toBeVisible();
  await expect(page.getByText('Nenhuma empresa vinculada à sua conta')).toHaveCount(0);
  failed = false;
  await page.getByRole('button', { name: 'Tentar novamente', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Empresa Beta' })).toBeVisible();
  await page.route('**/api/auth/me', route => route.fulfill({ json: { ...operator, isPlatformAdmin: false, tenant: { ...tenant, id: 'beta', role: 'Owner', onboardingCompleted: false } } }));
  await page.route('**/api/company', route => route.fulfill({ json: { name: 'Empresa Beta' } }));
  await page.locator('article').filter({ has: page.getByRole('heading', { name: 'Empresa Beta' }) }).getByRole('button').click();
  await expect(page).toHaveURL(/\/onboarding$/); expect(selected).toEqual({ tenantId: 'beta' });
});
