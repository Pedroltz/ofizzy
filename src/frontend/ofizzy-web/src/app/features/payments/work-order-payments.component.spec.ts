import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { WorkOrderPaymentsComponent } from './work-order-payments.component';

describe('WorkOrderPaymentsComponent', () => {
  const summary = { total: 100, registered: 0, settled: 0, reversed: 0, balance: 100, payments: [], documents: [] };
  beforeEach(() => TestBed.configureTestingModule({
    imports: [WorkOrderPaymentsComponent], providers: [provideHttpClient(), provideHttpClientTesting(), provideAnimationsAsync(),
      { provide: TenantContextService, useValue: { tenant: signal({ id: 'alpha' }), admin: () => true } }],
  }));
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('reuses the same idempotency key and date after an inconclusive response', async () => {
    const fixture = TestBed.createComponent(WorkOrderPaymentsComponent);
    fixture.componentRef.setInput('orderId', 'order-1');
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/work-orders/order-1/payments').flush(summary);
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.open('Register'); component.form.controls.amount.setValue(60);
    const first = component.save();
    const request = http.expectOne('/api/work-orders/order-1/payments');
    const body = request.request.body;
    request.flush({ detail: 'Resultado inconclusivo' }, { status: 503, statusText: 'Unavailable' });
    await first;
    expect(component.error()).toBe('Resultado inconclusivo');
    const second = component.save();
    const retry = http.expectOne('/api/work-orders/order-1/payments');
    expect(retry.request.body).toEqual(body);
    retry.flush({ detail: 'Resultado inconclusivo' }, { status: 503, statusText: 'Unavailable' });
    await second;
    fixture.destroy();
  });

  it('discards responses from the previous work order', async () => {
    const fixture = TestBed.createComponent(WorkOrderPaymentsComponent);
    fixture.componentRef.setInput('orderId', 'order-1'); fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    const old = http.expectOne('/api/work-orders/order-1/payments');
    fixture.componentRef.setInput('orderId', 'order-2'); fixture.detectChanges();
    http.expectOne('/api/work-orders/order-2/payments').flush({ ...summary, total: 200 });
    await fixture.whenStable();
    old.flush(summary);
    await fixture.whenStable();
    expect(fixture.componentInstance.data()?.total).toBe(200);
    fixture.destroy();
  });
});
