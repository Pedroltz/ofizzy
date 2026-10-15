import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient } from '@angular/common/http';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SettingsPage } from './settings.page';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { FiscalApiService } from '../fiscal/fiscal-api.service';
import { Part, ServiceItem } from '../../core/api/catalog.models';

import { ToastMessageOptions } from 'primeng/api';
import { FiscalValue } from '../fiscal/fiscal-api.service';
import { PartRequest, ServiceRequest } from '../../core/api/catalog.models';

describe('SettingsPage - Fiscal Catalog Integration', () => {
  const mockTenant = signal({
    id: 'tenant-1',
    name: 'Oficina Teste',
    slug: 'teste',
    status: 'Active',
    vertical: 'Automotive',
    role: 'Owner',
    onboardingCompleted: true,
    modules: ['Catalog', 'Customers', 'WorkOrders'],
  });

  const mockParts: Part[] = [
    {
      id: 'part-1',
      name: 'Pneu Aro 15',
      code: 'PN-15',
      costPrice: 200,
      salePrice: 350,
      isActive: true,
    },
  ];

  const mockServices: ServiceItem[] = [
    {
      id: 'srv-1',
      name: 'Alinhamento 3D',
      description: 'Dianteiro',
      defaultPrice: 120,
      isActive: true,
    },
  ];

  let savedPartData: PartRequest | null = null;
  let savedServiceData: ServiceRequest | null = null;
  let savedFiscalProfile: { kind: string; id: string; data: Record<string, FiscalValue> } | null =
    null;
  let messagesAdded: ToastMessageOptions[] = [];

  const mockCatalogApi = {
    peekServices: () => ({ items: mockServices, total: mockServices.length, page: 1, pageSize: 12 }),
    peekParts: () => ({ items: mockParts, total: mockParts.length, page: 1, pageSize: 12 }),
    services: () =>
      Promise.resolve({ items: mockServices, total: mockServices.length, page: 1, pageSize: 12 }),
    parts: () =>
      Promise.resolve({ items: mockParts, total: mockParts.length, page: 1, pageSize: 12 }),
    savePart: (data: PartRequest, id?: string) => {
      savedPartData = data;
      return Promise.resolve({ id: id || 'new-part-id', ...data, isActive: true });
    },
    saveService: (data: ServiceRequest, id?: string) => {
      savedServiceData = data;
      return Promise.resolve({ id: id || 'new-srv-id', ...data, isActive: true });
    },
    archivePart: () => Promise.resolve(),
    archiveService: () => Promise.resolve(),
  };

  const mockFiscalApi = {
    profile: (kind: 'parts' | 'services', id: string) => {
      if (kind === 'parts' && id === 'part-1') {
        return Promise.resolve({
          ncm: '40111000',
          cest: '0100100',
          origin: '0',
          unit: 'UN',
          gtin: 'SEM GTIN',
          cfop: '5102',
          csosn: '102',
          pisCst: '07',
          cofinsCst: '07',
          retainedStBase: null,
          retainedStAmount: null,
          substituteAmount: null,
          stRate: null,
        });
      }
      return Promise.resolve({});
    },
    saveProfile: (kind: 'parts' | 'services', id: string, data: Record<string, FiscalValue>) => {
      savedFiscalProfile = { kind, id, data };
      return Promise.resolve();
    },
    settings: () =>
      Promise.resolve({
        settings: {
          cnpj: '11222333000181',
          legalName: 'Oficina Teste',
          stateRegistration: 'ISENTO',
          municipalRegistration: '',
          regime: 'SimplesNacional',
          address: null,
          nfeEnabled: true,
          nfseEnabled: true,
          environment: 'Homologation',
          nfeSeries: 1,
          dpsSeries: 1,
        },
        certificate: null,
        encryptionConfigured: true,
        productionAllowed: false,
        devToolsAvailable: false,
      }),
  };

  const mockTenantContext = {
    tenant: mockTenant,
    has: (m: string) => m === 'Catalog',
    admin: () => true,
  };

  const mockMessages = {
    add: (msg: ToastMessageOptions) => messagesAdded.push(msg),
  };

  beforeEach(() => {
    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    );
    savedPartData = null;
    savedServiceData = null;
    savedFiscalProfile = null;
    messagesAdded = [];

    TestBed.configureTestingModule({
      imports: [SettingsPage],
      providers: [
        provideAnimationsAsync(),
        provideHttpClient(),
        { provide: TenantContextService, useValue: mockTenantContext },
        { provide: CatalogApiService, useValue: mockCatalogApi },
        { provide: FiscalApiService, useValue: mockFiscalApi },
        { provide: MessageService, useValue: mockMessages },
        ConfirmationService,
      ],
    });
  });

  it('deve inicializar com o slider fiscal desligado ao cadastrar nova peça', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openPart();

    expect(component.partDialog()).toBe(true);
    expect(component.editingPart()).toBeNull();
    expect(component.partFiscalEnabled.value).toBe(false);
    expect(component.partFiscalForm.get('cfop')?.value).toBe('5102');
    expect(component.partFiscalForm.get('csosn')?.value).toBe('102');
    expect(component.partFiscalForm.get('gtin')?.value).toBe('SEM GTIN');
  });

  it('deve carregar os dados fiscais e ativar o slider ao editar peça existente que já possua classificação', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openPart(mockParts[0]);

    expect(component.partDialog()).toBe(true);
    expect(component.editingPart()?.id).toBe('part-1');
    expect(component.partFiscalEnabled.value).toBe(true);
    expect(component.partFiscalForm.get('ncm')?.value).toBe('40111000');
    expect(component.partFiscalForm.get('cfop')?.value).toBe('5102');
  });

  it('deve validar NCM inválido quando o slider fiscal estiver ativado', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openPart();
    component.partForm.patchValue({
      name: 'Peça Teste',
      code: 'PC-1',
      costPrice: 50,
      salePrice: 100,
    });
    component.partFiscalEnabled.setValue(true);
    component.partFiscalForm.patchValue({ ncm: '123' }); // menos de 8 dígitos

    await component.savePart();

    expect(savedPartData).toBeNull();
    expect(savedFiscalProfile).toBeNull();
    expect(component.partFiscalForm.get('ncm')?.invalid).toBe(true);
    expect(component.partFiscalForm.get('ncm')?.touched).toBe(true);
    expect(messagesAdded.some((m) => m.severity === 'error' && m.summary === 'NCM inválido')).toBe(
      true,
    );
  });

  it('deve salvar peça e perfil fiscal juntos quando o slider estiver ativado com dados válidos', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openPart();
    component.partForm.patchValue({
      name: 'Válvula de Pneu',
      code: 'VALV-1',
      costPrice: 5,
      salePrice: 15,
    });
    component.partFiscalEnabled.setValue(true);
    component.partFiscalForm.patchValue({
      ncm: '84818099',
      cfop: '5102',
      csosn: '102',
      unit: 'UN',
      origin: '0',
    });

    await component.savePart();

    expect(savedPartData).not.toBeNull();
    expect(savedPartData?.name).toBe('Válvula de Pneu');
    expect(savedFiscalProfile).not.toBeNull();
    expect(savedFiscalProfile?.kind).toBe('parts');
    expect(savedFiscalProfile?.id).toBe('new-part-id');
    expect(savedFiscalProfile?.data['ncm']).toBe('84818099');
    expect(component.partDialog()).toBe(false);
  });

  it('deve salvar apenas a peça sem chamar perfil fiscal quando o slider estiver desativado', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openPart();
    component.partForm.patchValue({
      name: 'Abraçadeira',
      code: 'ABR-1',
      costPrice: 2,
      salePrice: 6,
    });
    component.partFiscalEnabled.setValue(false);

    await component.savePart();

    expect(savedPartData).not.toBeNull();
    expect(savedFiscalProfile).toBeNull();
    expect(component.partDialog()).toBe(false);
  });

  it('deve validar e salvar serviço com dados fiscais quando o slider de serviços estiver ativado', async () => {
    const fixture = TestBed.createComponent(SettingsPage);
    const component = fixture.componentInstance;

    await component.openService();
    component.serviceForm.patchValue({
      name: 'Troca de Óleo',
      description: 'Troca completa',
      defaultPrice: 80,
    });
    component.serviceFiscalEnabled.setValue(true);
    component.serviceFiscalForm.patchValue({
      nationalCode: '140101',
    });

    await component.saveService();

    expect(savedServiceData).not.toBeNull();
    expect(savedFiscalProfile).not.toBeNull();
    expect(savedFiscalProfile?.kind).toBe('services');
    expect(savedFiscalProfile?.id).toBe('new-srv-id');
    expect(savedFiscalProfile?.data['nationalCode']).toBe('140101');
    expect(component.serviceDialog()).toBe(false);
  });
});
