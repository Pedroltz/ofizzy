import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { FiscalValue } from './fiscal-api.service';

export interface FiscalField {
  key: string;
  label: string;
  type?: 'number' | 'currency' | 'date' | 'password';
  options?: { label: string; value: FiscalValue }[];
  colSpan?: 1 | 2;
  required?: boolean;
  pattern?: RegExp;
  patternMessage?: string;
  maxLength?: number;
}

export function fiscalForm(fields: FiscalField[], values: Record<string, FiscalValue> = {}) {
  return new FormGroup(
    Object.fromEntries(
      fields.map((f) => {
        const validators: ValidatorFn[] = [];
        if (f.required) {
          validators.push(Validators.required);
        }
        if (f.pattern) {
          validators.push(Validators.pattern(f.pattern));
        }
        if (f.maxLength) {
          validators.push(Validators.maxLength(f.maxLength));
        }
        return [
          f.key,
          new FormControl<FiscalValue>(
            values[f.key] ?? (f.type === 'number' || f.type === 'currency' ? null : ''),
            validators,
          ),
        ];
      }),
    ),
  );
}

export const addressFields: FiscalField[] = [
  { key: 'street', label: 'Logradouro / Endereço', colSpan: 2, required: true, maxLength: 60 },
  { key: 'number', label: 'Número', required: true, maxLength: 60 },
  { key: 'district', label: 'Bairro', required: true, maxLength: 60 },
  { key: 'city', label: 'Município', required: true, maxLength: 60 },
  { key: 'state', label: 'UF', required: true, maxLength: 2 },
  {
    key: 'postalCode',
    label: 'CEP (somente números)',
    required: true,
    pattern: /^\d{8}$/,
    patternMessage: 'CEP deve conter 8 dígitos numéricos',
  },
  {
    key: 'cityCode',
    label: 'Código IBGE do município',
    required: true,
    pattern: /^\d{7}$/,
    patternMessage: 'Código IBGE deve conter 7 dígitos numéricos',
  },
];

export const productFields: FiscalField[] = [
  {
    key: 'ncm',
    label: 'NCM',
    required: true,
    pattern: /^\d{8}$/,
    patternMessage: 'NCM deve conter 8 dígitos numéricos',
  },
  {
    key: 'cest',
    label: 'CEST (quando aplicável)',
    pattern: /^\d{7}$/,
    patternMessage: 'CEST deve conter 7 dígitos numéricos',
  },
  {
    key: 'origin',
    label: 'Código da origem',
    required: true,
    pattern: /^[0-8]$/,
    patternMessage: 'Origem deve ser de 0 a 8',
  },
  { key: 'unit', label: 'Unidade', required: true, maxLength: 6 },
  { key: 'gtin', label: 'GTIN ou SEM GTIN', required: true },
  { key: 'cfop', label: 'CFOP', required: true },
  { key: 'csosn', label: 'CSOSN', required: true },
  { key: 'pisCst', label: 'CST PIS', required: true },
  { key: 'cofinsCst', label: 'CST COFINS', required: true },
  { key: 'retainedStBase', label: 'Base ST anterior por unidade', type: 'currency' },
  { key: 'retainedStAmount', label: 'ICMS ST anterior por unidade', type: 'currency' },
  { key: 'substituteAmount', label: 'ICMS substituto por unidade', type: 'currency' },
  { key: 'stRate', label: 'Alíquota ST (%)', type: 'number' },
];

export const serviceFields: FiscalField[] = [
  {
    key: 'nationalCode',
    label: 'Código de tributação nacional',
    required: true,
    pattern: /^\d{6}$/,
    patternMessage: 'Código deve conter 6 dígitos numéricos',
  },
  {
    key: 'municipalCode',
    label: 'Código municipal (quando aplicável)',
    pattern: /^\d{3}$/,
    patternMessage: 'Código municipal deve conter 3 dígitos numéricos',
  },
  {
    key: 'nbs',
    label: 'NBS (quando aplicável)',
    pattern: /^\d{9}$/,
    patternMessage: 'NBS deve conter 9 dígitos numéricos',
  },
  {
    key: 'approximateTaxRate',
    label: 'Percentual aproximado de tributos do Simples',
    type: 'number',
  },
];

@Component({
  selector: 'app-fiscal-fields',
  standalone: true,
  imports: [ReactiveFormsModule, InputTextModule, InputNumberModule, SelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="fiscal-grid" [formGroup]="form()">
    @for (field of fields(); track field.key) {
      <div
        class="fiscal-field"
        [class.col-span-2]="field.colSpan === 2"
        [class.has-error]="isInvalid(field.key)"
      >
        <label [for]="prefix() + field.key" class="fiscal-label">
          <span>{{ field.label }}</span>
          @if (isRequired(field)) {
            <span class="required-mark" aria-hidden="true">*</span>
          }
        </label>
        @if (field.options) {
          <p-select
            [inputId]="prefix() + field.key"
            [formControlName]="field.key"
            [options]="field.options"
            optionLabel="label"
            optionValue="value"
            appendTo="body"
            [invalid]="isInvalid(field.key)"
          />
        } @else if (field.type === 'currency') {
          <p-inputnumber
            [inputId]="prefix() + field.key"
            [formControlName]="field.key"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            [min]="0"
            placeholder="R$ 0,00"
            [invalid]="isInvalid(field.key)"
          />
        } @else if (field.type === 'number') {
          <p-inputnumber
            [inputId]="prefix() + field.key"
            [formControlName]="field.key"
            [minFractionDigits]="0"
            [maxFractionDigits]="4"
            locale="pt-BR"
            [invalid]="isInvalid(field.key)"
          />
        } @else {
          <input
            pInputText
            [id]="prefix() + field.key"
            [type]="field.type || 'text'"
            [formControlName]="field.key"
            autocomplete="off"
            maxlength="255"
            [invalid]="isInvalid(field.key)"
          />
        }
        @if (errorMessage(field.key); as errorMsg) {
          <small class="fiscal-error-msg" role="alert">
            <i class="pi pi-exclamation-circle" aria-hidden="true"></i>
            <span>{{ errorMsg }}</span>
          </small>
        }
      </div>
    }
  </div>`,
  styles: `
    :host {
      display: block;
      min-width: 0;
    }
    .fiscal-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 1.25rem 1rem;
    }
    .fiscal-field {
      display: flex;
      flex-direction: column;
      gap: 0.375rem;
      min-width: 0;
      position: relative;
    }
    .col-span-2 {
      grid-column: span 2 / span 2;
    }
    .fiscal-label {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-secondary);
      line-height: 1.25;
      min-width: 0;
      overflow-wrap: anywhere;
      transition: color var(--transition-fast);
    }
    .required-mark {
      color: var(--danger);
      font-weight: 700;
      line-height: 1;
    }
    .fiscal-field.has-error .fiscal-label {
      color: var(--danger);
    }
    .fiscal-field.has-error input,
    .fiscal-field.has-error :host ::ng-deep .p-select,
    .fiscal-field.has-error :host ::ng-deep .p-inputnumber-input {
      border-color: var(--danger) !important;
    }
    .fiscal-field.has-error input:focus,
    .fiscal-field.has-error :host ::ng-deep .p-select:focus,
    .fiscal-field.has-error :host ::ng-deep .p-inputnumber-input:focus {
      box-shadow: 0 0 0 3px var(--danger-soft) !important;
    }
    .fiscal-error-msg {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.75rem;
      color: var(--danger);
      font-weight: 500;
      line-height: 1.25;
      margin-top: 0.125rem;
    }
    .fiscal-error-msg i {
      font-size: 0.8rem;
      flex-shrink: 0;
    }
    input {
      min-height: 44px;
      width: 100%;
      min-width: 0;
    }
    p-select,
    p-inputnumber {
      width: 100%;
      min-width: 0;
      min-height: 44px;
    }
    :host ::ng-deep .p-inputnumber {
      width: 100%;
    }
    :host ::ng-deep .p-inputnumber-input {
      min-width: 0;
      width: 100%;
      min-height: 44px;
    }
    @media (max-width: 640px) {
      .fiscal-grid {
        grid-template-columns: 1fr;
      }
      .col-span-2 {
        grid-column: auto;
      }
      input,
      :host ::ng-deep input {
        font-size: 16px;
      }
    }
  `,
})
export class FiscalFieldsComponent {
  readonly fields = input.required<FiscalField[]>();
  readonly form = input.required<ReturnType<typeof fiscalForm>>();
  readonly prefix = input('fiscal-');

  isInvalid(key: string): boolean {
    const ctrl = this.form().get(key);
    return !!ctrl && ctrl.invalid && (ctrl.touched || ctrl.dirty);
  }

  isRequired(field: FiscalField): boolean {
    if (field.required) return true;
    const ctrl = this.form().get(field.key);
    return !!ctrl?.hasValidator?.(Validators.required);
  }

  errorMessage(key: string): string | null {
    const ctrl = this.form().get(key);
    if (!ctrl || !ctrl.invalid || !(ctrl.touched || ctrl.dirty)) return null;
    if (ctrl.errors?.['custom']) return String(ctrl.errors['custom']);
    if (ctrl.errors?.['required']) return 'Campo obrigatório';
    if (ctrl.errors?.['pattern']) {
      const fieldDef = this.fields().find((f) => f.key === key);
      return fieldDef?.patternMessage ?? 'Formato inválido';
    }
    if (ctrl.errors?.['maxlength']) {
      return `Máximo de ${ctrl.errors['maxlength'].requiredLength} caracteres`;
    }
    if (ctrl.errors?.['min'] || ctrl.errors?.['max']) return 'Valor fora do limite permitido';
    return 'Valor inválido';
  }
}
