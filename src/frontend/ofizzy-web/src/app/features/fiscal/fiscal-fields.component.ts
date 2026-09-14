import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { FiscalValue } from './fiscal-api.service';
export interface FiscalField { key: string; label: string; type?: 'number' | 'date' | 'password'; options?: { label: string; value: FiscalValue }[]; colSpan?: 1 | 2; }
export function fiscalForm(fields: FiscalField[], values: Record<string, FiscalValue> = {}) { return new FormGroup(Object.fromEntries(fields.map(f => [f.key, new FormControl<FiscalValue>(values[f.key] ?? (f.type === 'number' ? null : ''))]))); }
export const addressFields: FiscalField[] = [
  { key: 'street', label: 'Logradouro / Endereço', colSpan: 2 }, { key: 'number', label: 'Número' }, { key: 'district', label: 'Bairro' },
  { key: 'city', label: 'Município' }, { key: 'state', label: 'UF' }, { key: 'postalCode', label: 'CEP (somente números)' }, { key: 'cityCode', label: 'Código IBGE do município' }
];
export const productFields: FiscalField[] = [
  { key: 'ncm', label: 'NCM' }, { key: 'cest', label: 'CEST (quando aplicável)' }, { key: 'origin', label: 'Código da origem' },
  { key: 'unit', label: 'Unidade' }, { key: 'gtin', label: 'GTIN ou SEM GTIN' }, { key: 'cfop', label: 'CFOP' }, { key: 'csosn', label: 'CSOSN' },
  { key: 'pisCst', label: 'CST PIS' }, { key: 'cofinsCst', label: 'CST COFINS' },
  { key: 'retainedStBase', label: 'Base ST anterior por unidade', type: 'number' }, { key: 'retainedStAmount', label: 'ICMS ST anterior por unidade', type: 'number' },
  { key: 'substituteAmount', label: 'ICMS substituto por unidade', type: 'number' }, { key: 'stRate', label: 'Alíquota ST (%)', type: 'number' }
];
export const serviceFields: FiscalField[] = [
  { key: 'nationalCode', label: 'Código de tributação nacional' }, { key: 'municipalCode', label: 'Código municipal (quando aplicável)' },
  { key: 'nbs', label: 'NBS (quando aplicável)' }, { key: 'approximateTaxRate', label: 'Percentual aproximado de tributos do Simples', type: 'number' }
];
@Component({
  selector: 'app-fiscal-fields', standalone: true, imports: [ReactiveFormsModule, InputTextModule, InputNumberModule, SelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="fiscal-grid" [formGroup]="form()">@for (field of fields(); track field.key) {
    <div class="fiscal-field" [class.col-span-2]="field.colSpan === 2">
      <label [for]="prefix() + field.key" class="fiscal-label">{{ field.label }}</label>
      @if (field.options) { <p-select [inputId]="prefix() + field.key" [formControlName]="field.key" [options]="field.options" optionLabel="label" optionValue="value" appendTo="body" /> }
      @else if (field.type === 'number') { <p-inputnumber [inputId]="prefix() + field.key" [formControlName]="field.key" [minFractionDigits]="0" [maxFractionDigits]="4" locale="pt-BR" /> }
      @else { <input pInputText [id]="prefix() + field.key" [type]="field.type || 'text'" [formControlName]="field.key" autocomplete="off" maxlength="255" /> }
    </div>
  }</div>`,
  styles: `:host{display:block;min-width:0}.fiscal-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:1.25rem 1rem}.fiscal-field{display:flex;flex-direction:column;gap:0.375rem;min-width:0}.col-span-2{grid-column:span 2 / span 2}.fiscal-label{display:block;font-size:0.75rem;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;color:var(--text-secondary);line-height:1.25;min-width:0;overflow-wrap:anywhere}input{min-height:44px;width:100%;min-width:0}p-select,p-inputnumber{width:100%;min-width:0;min-height:44px}:host ::ng-deep .p-inputnumber{width:100%}:host ::ng-deep .p-inputnumber-input{min-width:0;width:100%;min-height:44px}@media(max-width:640px){.fiscal-grid{grid-template-columns:1fr}.col-span-2{grid-column:auto}input,:host ::ng-deep input{font-size:16px}}`
})
export class FiscalFieldsComponent {
  readonly fields = input.required<FiscalField[]>(); readonly form = input.required<ReturnType<typeof fiscalForm>>(); readonly prefix = input('fiscal-');
}
