import { expect, test } from '@playwright/test';

// Fictitious credentials, exclusively for an isolated database with bootstrap enabled.
const email = 'session@smoke.test';
const password = 'SessionSmoke2026';

test('renova sessão com XSRF real, persiste escrita e permanece no login após expiração', async ({ page, context }) => {
  const status = await context.request.get('/api/setup/status');
  if ((await status.json()).required) {
    await page.goto('/setup');
    await page.getByLabel('Nome completo').fill('Operador sessão');
    await page.getByLabel('E-mail corporativo').fill(email);
    await page.getByLabel('Senha de acesso').fill(password);
    await page.getByRole('button', { name: 'Concluir configuração' }).click();
  } else {
    await page.goto('/login');
    await page.locator('[formControlName="email"]').fill(email);
    await page.locator('[formControlName="password"]').fill(password);
    await page.locator('button[type="submit"]').click();
  }
  await expect(page.getByRole('heading', { name: 'Administração da plataforma' })).toBeVisible();
  if (process.env['OFIZZY_VERIFY_RESTART'] === '1') {
    await expect(page.getByText('Empresa sessão inicial', { exact: true })).toBeVisible();
  }
  const suffix = process.env['OFIZZY_VERIFY_RESTART'] === '1' ? `restart-${Date.now()}` : 'inicial';
  await page.getByRole('button', { name: 'Nova empresa', exact: true }).first().click();
  await page.getByLabel('Nome da empresa', { exact: true }).fill(`Empresa sessão ${suffix}`);
  await page.getByLabel('Identificador', { exact: true }).fill(`session-${suffix}`);
  await page.getByLabel('Nome do administrador', { exact: true }).fill('Administrador sessão');
  await page.getByLabel('E-mail do administrador', { exact: true }).fill(`admin-${suffix}@smoke.test`);
  await page.getByLabel('Senha inicial', { exact: true }).fill(password);

  // Removing only access reproduces cookie expiry without waiting fifteen minutes.
  // Leave the authenticated XSRF token in place to reproduce the original failure.
  const staleXsrf = (await context.cookies()).find(cookie => cookie.name === 'XSRF-TOKEN')!.value;
  await context.clearCookies({ name: 'ofizzy_access' });
  const staleRefresh = await context.request.post('/api/auth/refresh', {
    data: {}, headers: { 'X-XSRF-TOKEN': decodeURIComponent(staleXsrf) },
  });
  expect(staleRefresh.status()).toBe(400);
  const refreshed = page.waitForResponse(response => response.url().endsWith('/api/auth/refresh'));
  await page.getByRole('button', { name: 'Criar empresa', exact: true }).click();
  expect((await refreshed).status()).toBe(200);
  await expect(page.getByRole('dialog')).toBeHidden();
  await expect(page.getByText(`Empresa sessão ${suffix}`, { exact: true })).toBeVisible();
  await expect(page.getByText('Sessão expirada', { exact: true })).toHaveCount(0);

  // No refresh cookie left: the next protected operation must end at login.
  await context.clearCookies({ name: /^ofizzy_(access|refresh)$/ });
  await page.getByRole('button', { name: /Gerenciar/ }).first().click();
  await page.getByRole('button', { name: 'Salvar alterações', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.locator('[formControlName="email"]')).toBeVisible();
  for (const width of [1440, 768, 320]) {
    await page.setViewportSize({ width, height: 900 });
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
  }
  await page.locator('[formControlName="email"]').fill(email);
  await page.locator('[formControlName="password"]').fill(password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/plataforma$/);
});
