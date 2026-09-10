import { defineConfig } from '@playwright/test';
export default defineConfig({
  testDir: './e2e', testMatch: 'saas-live.spec.ts', workers: 1, timeout: 120_000,
  use: { baseURL: process.env['OFIZZY_E2E_BASE_URL'] ?? 'http://127.0.0.1:18081', viewport: { width: 1440, height: 900 }, trace: 'off', screenshot: 'only-on-failure' },
  outputDir: '/tmp/ofizzy-live-results', reporter: 'list',
});
