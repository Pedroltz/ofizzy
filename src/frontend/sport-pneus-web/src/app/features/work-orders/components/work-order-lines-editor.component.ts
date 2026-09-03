import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Part, ServiceItem } from '../../../core/api/catalog.models';
import { WorkOrderLineRequest } from '../../../core/api/work-order.models';

export type WorkOrderLineKind = 'services' | 'parts';

export interface WorkOrderLineChange {
  index: number;
  field: keyof WorkOrderLineRequest;
  value: string | number | null;
}

@Component({
  selector: 'app-work-order-lines-editor',
  imports: [FormsModule, ButtonModule, InputNumberModule, InputTextModule],
  template: `
    <section class="line-editor">
      <header class="line-editor__header">
        <div class="line-editor__title">
          <i [class]="kind() === 'services' ? 'pi pi-wrench' : 'pi pi-box'"></i>
          <h3>{{ title() }}</h3>
        </div>
        <div class="line-editor__add-actions">
          <p-button
            [label]="manualLabel()"
            icon="pi pi-plus"
            size="small"
            [outlined]="true"
            severity="info"
            type="button"
            (onClick)="addManual.emit()"
          />
          <select
            #picker
            (change)="addCatalog.emit(picker.value); picker.value = ''"
            [attr.aria-label]="catalogLabel()"
          >
            <option value="">+ Do catálogo...</option>
            @for (item of catalog(); track item.id) {
              <option [value]="item.id">{{ item.name }} ({{ money(price(item)) }})</option>
            }
          </select>
        </div>
      </header>

      <div class="line-editor__body">
        @if (lines().length) {
          <div
            class="line-editor__head"
            [class.line-editor__head--parts]="kind() === 'parts'"
            aria-hidden="true"
          >
            <span>Descrição</span>
            @if (kind() === 'parts') {
              <span>Cód / Ref</span>
            }
            <span class="center">Qtd</span>
            <span class="right">Valor unit.</span>
            <span class="right">Subtotal</span>
            <span></span>
          </div>

          <div class="line-editor__rows">
            @for (line of lines(); track $index) {
              <article class="line-card" [class.line-card--parts]="kind() === 'parts'">
                <label class="line-card__description">
                  <span>Descrição</span>
                  <input
                    pInputText
                    [ngModel]="line.description"
                    [ngModelOptions]="{ standalone: true }"
                    [placeholder]="
                      kind() === 'services' ? 'Ex: Alinhamento 3D' : 'Ex: Pneu 175/70 R14'
                    "
                    (ngModelChange)="
                      change.emit({ index: $index, field: 'description', value: $event })
                    "
                  />
                </label>
                @if (kind() === 'parts') {
                  <label class="line-card__code">
                    <span>Cód / Ref</span>
                    <input
                      pInputText
                      [ngModel]="line.code"
                      [ngModelOptions]="{ standalone: true }"
                      placeholder="Ex: SKU-01"
                      (ngModelChange)="change.emit({ index: $index, field: 'code', value: $event })"
                    />
                  </label>
                }
                <label class="line-card__quantity">
                  <span>Quantidade</span>
                  <p-inputnumber
                    [ngModel]="line.quantity"
                    [ngModelOptions]="{ standalone: true }"
                    [min]="0.01"
                    [maxFractionDigits]="2"
                    styleClass="w-full"
                    (ngModelChange)="
                      change.emit({ index: $index, field: 'quantity', value: $event })
                    "
                  />
                </label>
                <label class="line-card__price">
                  <span>Valor unitário</span>
                  <p-inputnumber
                    [ngModel]="line.unitPrice"
                    [ngModelOptions]="{ standalone: true }"
                    mode="currency"
                    currency="BRL"
                    locale="pt-BR"
                    styleClass="w-full"
                    (ngModelChange)="
                      change.emit({ index: $index, field: 'unitPrice', value: $event })
                    "
                  />
                </label>
                <div class="line-card__total">
                  <span>Subtotal</span>
                  <strong>{{ money((line.quantity || 0) * (line.unitPrice || 0)) }}</strong>
                </div>
                <p-button
                  class="line-card__remove"
                  icon="pi pi-trash"
                  [rounded]="true"
                  [text]="true"
                  severity="danger"
                  type="button"
                  (onClick)="remove.emit($index)"
                  [ariaLabel]="'Remover ' + itemName()"
                />
              </article>
            }
          </div>
          <footer class="line-editor__subtotal">
            <span
              >Subtotal {{ shortTitle() }} ({{ lines().length }}
              {{ lines().length === 1 ? 'item' : 'itens' }}):</span
            >
            <strong>{{ money(subtotal()) }}</strong>
          </footer>
        } @else {
          <div class="line-editor__empty">
            <i [class]="kind() === 'services' ? 'pi pi-wrench' : 'pi pi-box'"></i>
            <span
              >Nenhum {{ itemName() }} incluído. Adicione um item avulso ou selecione no
              catálogo.</span
            >
          </div>
        }
      </div>
    </section>
  `,
  styleUrl: './work-order-lines-editor.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderLinesEditorComponent {
  readonly kind = input.required<WorkOrderLineKind>();
  readonly lines = input.required<WorkOrderLineRequest[]>();
  readonly catalog = input.required<(ServiceItem | Part)[]>();
  readonly subtotal = input.required<number>();
  readonly addManual = output<void>();
  readonly addCatalog = output<string>();
  readonly change = output<WorkOrderLineChange>();
  readonly remove = output<number>();

  readonly title = computed(() =>
    this.kind() === 'services' ? 'Serviços e Mão de Obra' : 'Peças e Insumos',
  );
  readonly shortTitle = computed(() => (this.kind() === 'services' ? 'Serviços' : 'Peças'));
  readonly manualLabel = computed(() =>
    this.kind() === 'services' ? 'Novo Serviço Avulso' : 'Nova Peça Avulsa',
  );
  readonly catalogLabel = computed(() => `Adicionar ${this.itemName()} do catálogo`);
  readonly itemName = computed(() => (this.kind() === 'services' ? 'serviço' : 'peça'));

  price(item: ServiceItem | Part): number {
    return 'defaultPrice' in item ? item.defaultPrice : item.salePrice;
  }

  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(
      value || 0,
    );
  }
}
