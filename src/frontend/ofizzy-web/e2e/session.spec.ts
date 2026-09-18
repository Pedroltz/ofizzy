import { expect, test } from '@playwright/test';

for (const refreshStatus of [200, 401, 503]) {
  test(`sessão: renovação retorna ${refreshStatus}`, async ({ page }) => {
    const user = { id: 'operator', name: 'Operador', email: 'operator@example.test', isPlatformAdmin: true, tenant: null };
    const tenant = { id: 'alpha', name: 'Empresa Alpha', slug: 'alpha', status: 'Pending', vertical: 'Automotive', modules: ['Customers'], fiscalProductionReleased: false };
    let expired = false;
    let refreshCalls = 0;
    let meCalls = 0;
    await page.route('**/api/**', route => {
      const path = new URL(route.request().url()).pathname;
      if (path === '/api/setup/status') return route.fulfill({ json: { required: false } });
      // A surviving access cookie must not bounce the login guard back to the app.
      if (path === '/api/auth/me') { meCalls++; return route.fulfill({ json: user }); }
      if (path === '/api/auth/refresh') {
        refreshCalls++;
        if (refreshStatus === 200) expired = false;
        return route.fulfill({ status: refreshStatus, json: refreshStatus === 200 ? user : { title: 'Falha de teste' } });
      }
      if (expired) return route.fulfill({ status: 401, json: {} });
      return route.fulfill({ json: path === '/api/platform/tenants' ? [tenant] : {} });
    });
    await page.goto('/plataforma');
    await page.getByRole('button', { name: /Gerenciar/ }).first().click();
    expired = true;
    await page.getByRole('button', { name: 'Salvar alterações', exact: true }).click();
    await expect.poll(() => refreshCalls).toBe(1);
    if (refreshStatus === 401) {
      await expect(page).toHaveURL(/\/login$/);
      await expect(page.locator('[formControlName="email"]')).toBeVisible();
      expect(meCalls).toBe(1);
    } else {
      if (refreshStatus === 200) await expect(page.getByRole('dialog')).toBeHidden();
      else await expect(page.getByText('Falha de teste', { exact: true })).toBeVisible();
      await expect(page).toHaveURL(/\/plataforma$/);
      await expect(page.getByText('Sessão expirada', { exact: true })).toHaveCount(0);
    }
  });
}
