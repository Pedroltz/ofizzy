import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PageHeaderComponent } from './page-header.component';
import { SectionCardComponent } from './section-card.component';
import { StatCardComponent } from './stat-card.component';
import { StatusBadgeComponent } from './status-badge.component';
import { SearchFieldComponent } from './search-field.component';
import { DataToolbarComponent } from './data-toolbar.component';
import { EmptyStateComponent } from './empty-state.component';
import { LoadingStateComponent } from './loading-state.component';
import { DataTableWrapperComponent } from './data-table-wrapper.component';
import { ThemeToggleComponent } from './theme-toggle.component';
import { ThemeService } from '../../core/theme/theme.service';

describe('Shared Design System Components', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })));
  });

  describe('PageHeaderComponent', () => {
    let fixture: ComponentFixture<PageHeaderComponent>;

    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [PageHeaderComponent],
      }).compileComponents();

      fixture = TestBed.createComponent(PageHeaderComponent);
    });

    it('should render title and description', () => {
      fixture.componentRef.setInput('title', 'Ordens de Serviço');
      fixture.componentRef.setInput('description', 'Gerencie as ordens');
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('h1')?.textContent).toContain('Ordens de Serviço');
      expect(el.querySelector('.page-description')?.textContent).toContain('Gerencie as ordens');
    });

    it('should emit action on button click when actionLabel is set', () => {
      let clicked = false;
      fixture.componentRef.setInput('title', 'Test');
      fixture.componentRef.setInput('description', 'Test desc');
      fixture.componentRef.setInput('actionLabel', 'Nova OS');
      fixture.componentInstance.action.subscribe(() => {
        clicked = true;
      });
      fixture.detectChanges();

      const btn = fixture.nativeElement.querySelector('button.primary-button');
      btn.click();
      expect(clicked).toBe(true);
    });
  });

  describe('SectionCardComponent', () => {
    it('should render title, subtitle and padding', async () => {
      await TestBed.configureTestingModule({
        imports: [SectionCardComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(SectionCardComponent);
      fixture.componentRef.setInput('title', 'Informações Gerais');
      fixture.componentRef.setInput('subtitle', 'Dados do cliente');
      fixture.componentRef.setInput('padding', 'lg');
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.section-card__title')?.textContent).toContain('Informações Gerais');
      expect(el.querySelector('.section-card__subtitle')?.textContent).toContain('Dados do cliente');
      expect(el.querySelector('.section-card')?.classList.contains('p-lg')).toBe(true);
    });
  });

  describe('StatCardComponent', () => {
    it('should render metric value, label and severity', async () => {
      await TestBed.configureTestingModule({
        imports: [StatCardComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(StatCardComponent);
      fixture.componentRef.setInput('label', 'Ordens Ativas');
      fixture.componentRef.setInput('value', 8);
      fixture.componentRef.setInput('hint', 'No pátio');
      fixture.componentRef.setInput('severity', 'success');
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.stat-card__label')?.textContent).toContain('Ordens Ativas');
      expect(el.querySelector('.stat-card__value')?.textContent).toContain('8');
      expect(el.querySelector('.stat-card__hint')?.textContent).toContain('No pátio');
      expect(el.querySelector('.stat-card')?.classList.contains('severity-success')).toBe(true);
    });
  });

  describe('StatusBadgeComponent', () => {
    it('should map Open and Completed to proper Portuguese labels and classes', async () => {
      await TestBed.configureTestingModule({
        imports: [StatusBadgeComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(StatusBadgeComponent);
      fixture.componentRef.setInput('status', 'Open');
      fixture.detectChanges();

      let el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.status-badge__label')?.textContent).toContain('Aberta');
      expect(el.querySelector('.status-badge')?.classList.contains('variant-open')).toBe(true);

      fixture.componentRef.setInput('status', 'Completed');
      fixture.detectChanges();

      el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.status-badge__label')?.textContent).toContain('Concluída');
      expect(el.querySelector('.status-badge')?.classList.contains('variant-completed')).toBe(true);
    });
  });

  describe('SearchFieldComponent', () => {
    it('should update value and emit searchChange, and clear on button click', async () => {
      await TestBed.configureTestingModule({
        imports: [SearchFieldComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(SearchFieldComponent);
      let emitted = '';
      fixture.componentInstance.searchChange.subscribe((val) => {
        emitted = val;
      });
      fixture.detectChanges();

      const inputEl = fixture.nativeElement.querySelector('input') as HTMLInputElement;
      inputEl.value = 'Honda Civic';
      inputEl.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      expect(fixture.componentInstance.value()).toBe('Honda Civic');
      expect(emitted).toBe('Honda Civic');

      const clearBtn = fixture.nativeElement.querySelector('.search-field__clear') as HTMLButtonElement;
      expect(clearBtn).not.toBeNull();
      clearBtn.click();
      fixture.detectChanges();

      expect(fixture.componentInstance.value()).toBe('');
      expect(emitted).toBe('');
    });
  });

  describe('DataToolbarComponent', () => {
    it('should render counter and toggle view modes', async () => {
      await TestBed.configureTestingModule({
        imports: [DataToolbarComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(DataToolbarComponent);
      fixture.componentRef.setInput('itemCount', 15);
      fixture.componentRef.setInput('itemLabelPlural', 'clientes cadastrados');
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.data-toolbar__counter')?.textContent).toContain('15 clientes cadastrados');

      const buttons = el.querySelectorAll('.data-toolbar__toggle-btn');
      expect(buttons.length).toBe(2);

      // Click blocks button
      (buttons[1] as HTMLButtonElement).click();
      fixture.detectChanges();
      expect(fixture.componentInstance.viewMode()).toBe('cards');
    });
  });

  describe('EmptyStateComponent', () => {
    it('should render empty title, description and handle action', async () => {
      await TestBed.configureTestingModule({
        imports: [EmptyStateComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(EmptyStateComponent);
      fixture.componentRef.setInput('title', 'Nenhum registro');
      fixture.componentRef.setInput('description', 'Cadastre o primeiro item');
      fixture.componentRef.setInput('actionLabel', 'Novo');
      let actionFired = false;
      fixture.componentInstance.action.subscribe(() => {
        actionFired = true;
      });
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.empty-state__title')?.textContent).toContain('Nenhum registro');
      expect(el.querySelector('.empty-state__description')?.textContent).toContain('Cadastre o primeiro item');

      const btn = el.querySelector('button.primary-button') as HTMLButtonElement;
      btn.click();
      expect(actionFired).toBe(true);
    });
  });

  describe('LoadingStateComponent', () => {
    it('should render skeleton containers for table and cards', async () => {
      await TestBed.configureTestingModule({
        imports: [LoadingStateComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(LoadingStateComponent);
      fixture.componentRef.setInput('mode', 'table');
      fixture.componentRef.setInput('count', 3);
      fixture.detectChanges();

      let el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.loading-table')).not.toBeNull();

      fixture.componentRef.setInput('mode', 'cards');
      fixture.detectChanges();

      el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.loading-cards')).not.toBeNull();
    });
  });

  describe('DataTableWrapperComponent', () => {
    it('should render container with scrollable viewport', async () => {
      await TestBed.configureTestingModule({
        imports: [DataTableWrapperComponent],
      }).compileComponents();

      const fixture = TestBed.createComponent(DataTableWrapperComponent);
      fixture.detectChanges();

      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.data-table-wrapper__scroll')).not.toBeNull();
    });
  });

  describe('ThemeToggleComponent', () => {
    it('should open menu and select theme', async () => {
      await TestBed.configureTestingModule({
        imports: [ThemeToggleComponent],
        providers: [ThemeService],
      }).compileComponents();

      const fixture = TestBed.createComponent(ThemeToggleComponent);
      const themeService = TestBed.inject(ThemeService);
      fixture.detectChanges();

      const btn = fixture.nativeElement.querySelector('.theme-toggle__btn') as HTMLButtonElement;
      expect(fixture.componentInstance.isOpen()).toBe(false);

      // Open menu
      btn.click();
      fixture.detectChanges();
      expect(fixture.componentInstance.isOpen()).toBe(true);

      const options = fixture.nativeElement.querySelectorAll('.theme-toggle__option');
      expect(options.length).toBe(3);

      // Select dark theme
      (options[2] as HTMLButtonElement).click();
      fixture.detectChanges();

      expect(themeService.preference()).toBe('dark');
      expect(fixture.componentInstance.isOpen()).toBe(false);
    });

    it('should toggle theme when clicked in switch mode', async () => {
      await TestBed.configureTestingModule({
        imports: [ThemeToggleComponent],
        providers: [ThemeService],
      }).compileComponents();

      const fixture = TestBed.createComponent(ThemeToggleComponent);
      fixture.componentRef.setInput('mode', 'switch');
      const themeService = TestBed.inject(ThemeService);
      fixture.detectChanges();

      const switchBtn = fixture.nativeElement.querySelector('button.theme-switch') as HTMLButtonElement;
      expect(switchBtn).not.toBeNull();

      const initial = themeService.activeTheme();
      switchBtn.click();
      fixture.detectChanges();

      const expected = initial === 'dark' ? 'light' : 'dark';
      expect(themeService.activeTheme()).toBe(expected);
    });
  });
});
