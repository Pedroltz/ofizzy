import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Part, ServiceItem } from '../../core/api/catalog.models';

@Component({selector:'app-settings-page',imports:[ReactiveFormsModule,ButtonModule,DialogModule,InputNumberModule,InputTextModule,TabsModule,TextareaModule],templateUrl:'./settings.page.html',changeDetection:ChangeDetectionStrategy.OnPush})
export class SettingsPage{
  private readonly api=inject(CatalogApiService);private readonly fb=inject(FormBuilder);private readonly messages=inject(MessageService);private readonly confirmation=inject(ConfirmationService);
  readonly services=signal<ServiceItem[]>([]);readonly parts=signal<Part[]>([]);readonly loading=signal(true);readonly saving=signal(false);readonly serviceDialog=signal(false);readonly partDialog=signal(false);readonly editingService=signal<ServiceItem|null>(null);readonly editingPart=signal<Part|null>(null);
  readonly serviceForm=this.fb.nonNullable.group({name:['',Validators.required],description:[''],defaultPrice:[0,[Validators.required,Validators.min(0)]]});
  readonly partForm=this.fb.nonNullable.group({name:['',Validators.required],code:['',Validators.required],costPrice:[0,[Validators.required,Validators.min(0)]],salePrice:[0,[Validators.required,Validators.min(0)]]});
  constructor(){void this.load();}
  async load():Promise<void>{this.loading.set(true);try{const[services,parts]=await Promise.all([this.api.services(),this.api.parts()]);this.services.set(services.items);this.parts.set(parts.items);}finally{this.loading.set(false);}}
  openService(item?:ServiceItem):void{this.editingService.set(item??null);this.serviceForm.reset({name:item?.name??'',description:item?.description??'',defaultPrice:item?.defaultPrice??0});this.serviceDialog.set(true);}
  openPart(item?:Part):void{this.editingPart.set(item??null);this.partForm.reset({name:item?.name??'',code:item?.code??'',costPrice:item?.costPrice??0,salePrice:item?.salePrice??0});this.partDialog.set(true);}
  async saveService():Promise<void>{if(this.serviceForm.invalid){this.serviceForm.markAllAsTouched();return;}this.saving.set(true);try{const v=this.serviceForm.getRawValue();await this.api.saveService({name:v.name,description:v.description||null,defaultPrice:v.defaultPrice},this.editingService()?.id);this.serviceDialog.set(false);this.messages.add({severity:'success',summary:this.editingService()?'Serviço atualizado':'Serviço criado'});await this.load();}finally{this.saving.set(false);}}
  async savePart():Promise<void>{if(this.partForm.invalid){this.partForm.markAllAsTouched();return;}this.saving.set(true);try{await this.api.savePart(this.partForm.getRawValue(),this.editingPart()?.id);this.partDialog.set(false);this.messages.add({severity:'success',summary:this.editingPart()?'Peça atualizada':'Peça criada'});await this.load();}finally{this.saving.set(false);}}
  archiveService(item:ServiceItem):void{this.confirmation.confirm({header:'Arquivar serviço',message:`Arquivar ${item.name}?`,acceptLabel:'Arquivar',rejectLabel:'Voltar',acceptButtonProps:{severity:'danger'},accept:async()=>{await this.api.archiveService(item.id);await this.load();}});}
  archivePart(item:Part):void{this.confirmation.confirm({header:'Arquivar peça',message:`Arquivar ${item.name}?`,acceptLabel:'Arquivar',rejectLabel:'Voltar',acceptButtonProps:{severity:'danger'},accept:async()=>{await this.api.archivePart(item.id);await this.load();}});}
  money(value:number):string{return value.toLocaleString('pt-BR',{style:'currency',currency:'BRL'});}
}
