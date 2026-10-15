import { defineConfig } from '@playwright/test';
export default defineConfig({
  testDir: './e2e', testMatch: 'fiscal-live.spec.ts', workers: 1, timeout: 120_000,
  use: { baseURL: process.env['OFIZZY_E2E_BASE_URL'] ?? 'http://127.0.0.1:18082', viewport: { width: 1440, height: 1000 }, trace: 'off', screenshot: 'only-on-failure' },
  outputDir: '/tmp/ofizzy-fiscal-live-results', reporter: 'list',
});
