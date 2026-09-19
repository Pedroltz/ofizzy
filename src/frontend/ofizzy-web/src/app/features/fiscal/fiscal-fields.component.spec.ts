import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { By } from '@angular/platform-browser';
import { FiscalFieldsComponent, FiscalField, fiscalForm } from './fiscal-fields.component';

@Component({
  standalone: true,
  imports: [FiscalFieldsComponent],
  template: ` <app-fiscal-fields [fields]="testFields" [form]="testForm" prefix="test-" /> `,
})
class TestHostComponent {
  testFields: FiscalField[] = [
    {
      key: 'ncm',
      label: 'NCM',
      required: true,
      pattern: /^\d{8}$/,
      patternMessage: 'NCM deve conter 8 dígitos numéricos',
    },
    {
      key: 'origin',
      label: 'Origem',
      required: true,
      options: [
        { label: '0 - Nacional', value: '0' },
        { label: '1 - Estrangeira', value: '1' },
      ],
    },
    { key: 'rate', label: 'Alíquota', type: 'number' },
    { key: 'amount', label: 'Valor', type: 'currency', required: true },
  ];
  testForm = fiscalForm(this.testFields, { ncm: '', origin: '0', rate: null, amount: null });
}

describe('FiscalFieldsComponent - Validation & Red Highlight', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve exibir indicador de obrigatoriedade (*) para campos obrigatórios', () => {
    const requiredMarks = fixture.debugElement.queryAll(By.css('.required-mark'));
    // ncm, origin e amount são required
    expect(requiredMarks.length).toBe(3);
  });

  it('deve renderizar campo do tipo currency com p-inputnumber e validar valor monetário', () => {
    const amountControl = host.testForm.get('amount');
    expect(amountControl?.value).toBeNull();

    const inputNumber = fixture.debugElement.query(By.css('#test-amount'));
    expect(inputNumber).not.toBeNull();

    amountControl?.setValue(150.75);
    fixture.detectChanges();
    expect(amountControl?.valid).toBe(true);
    expect(amountControl?.value).toBe(150.75);

    amountControl?.setValue(null);
    amountControl?.markAsTouched();
    fixture.detectChanges();
    expect(amountControl?.invalid).toBe(true);

    const amountField = fixture.debugElement.queryAll(By.css('.fiscal-field.has-error'));
    expect(amountField.length).toBeGreaterThan(0);
  });

  it('não deve exibir .has-error inicialmente se o formulário não foi tocado', () => {
    const errorFields = fixture.debugElement.queryAll(By.css('.fiscal-field.has-error'));
    expect(errorFields.length).toBe(0);
  });

  it('deve marcar o campo com .has-error e exibir mensagem explicativa quando o controle for tocado e inválido', () => {
    const ncmControl = host.testForm.get('ncm');
    ncmControl?.setValue('123'); // inválido (menos de 8 dígitos)
    ncmControl?.markAsTouched();
    fixture.detectChanges();

    const ncmField = fixture.debugElement.query(By.css('.fiscal-field.has-error'));
    expect(ncmField).not.toBeNull();

    const errorMsg = fixture.debugElement.query(By.css('.fiscal-error-msg'));
    expect(errorMsg).not.toBeNull();
    expect(errorMsg.nativeElement.textContent).toContain('NCM deve conter 8 dígitos numéricos');
  });

  it('deve remover a classe .has-error assim que um valor válido for digitado', () => {
    const ncmControl = host.testForm.get('ncm');
    ncmControl?.setValue('123');
    ncmControl?.markAsTouched();
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.fiscal-field.has-error'))).not.toBeNull();

    ncmControl?.setValue('40111000'); // válido (8 dígitos)
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.fiscal-field.has-error'))).toBeNull();
    expect(fixture.debugElement.query(By.css('.fiscal-error-msg'))).toBeNull();
  });

  it('deve exibir mensagem de campo obrigatório quando o valor estiver vazio e for tocado', () => {
    const ncmControl = host.testForm.get('ncm');
    ncmControl?.setValue('');
    ncmControl?.markAsTouched();
    fixture.detectChanges();

    const errorMsg = fixture.debugElement.query(By.css('.fiscal-error-msg'));
    expect(errorMsg).not.toBeNull();
    expect(errorMsg.nativeElement.textContent).toContain('Campo obrigatório');
  });

  it('deve exibir mensagem de erro customizado quando setErrors com custom for definido', () => {
    const ncmControl = host.testForm.get('ncm');
    ncmControl?.setErrors({ custom: 'NCM não cadastrado na tabela TIPI' });
    ncmControl?.markAsTouched();
    fixture.detectChanges();

    const errorMsg = fixture.debugElement.query(By.css('.fiscal-error-msg'));
    expect(errorMsg).not.toBeNull();
    expect(errorMsg.nativeElement.textContent).toContain('NCM não cadastrado na tabela TIPI');
  });
});
