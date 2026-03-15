import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FaqApiService } from '../../../shared/services/faq-api.service';
import { LanguageService } from '../../../shared/services/language.service';
import { FaqItem, FaqCategory, CreateFaqPayload } from '../../../shared/models/faq.model';

const CATEGORIES: { value: FaqCategory; label: string }[] = [
    { value: 'Print3d', label: 'faq.categories.Print3d' },
    { value: 'Laser', label: 'faq.categories.Laser' },
    { value: 'Cnc', label: 'faq.categories.Cnc' },
    { value: 'Software', label: 'faq.categories.Software' },
    { value: 'OrdersShipping', label: 'faq.categories.OrdersShipping' },
    { value: 'Payments', label: 'faq.categories.Payments' },
    { value: 'Support', label: 'faq.categories.Support' },
    { value: 'General', label: 'faq.categories.General' }
];

@Component({
    selector: 'app-faq-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        InputNumberModule,
        SelectModule,
        CheckboxModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="faq-form-page" [dir]="languageService.direction">
            <div class="page-header">
                <h1>{{ isEdit ? ('admin.faq.editTitle' | translate) : ('admin.faq.createTitle' | translate) }}</h1>
                <p-button [label]="'common.back' | translate" icon="pi pi-arrow-left" [outlined]="true" routerLink="/admin/faq"></p-button>
            </div>

            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                @if (errorMessage) {
                    <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
                }

                <div class="form-grid">
                    <div class="form-field form-field-full">
                        <label>{{ 'admin.faq.category' | translate }} <span class="required">*</span></label>
                        <p-select
                            formControlName="category"
                            [options]="categoryOptions"
                            optionLabel="label"
                            optionValue="value"
                            [placeholder]="'admin.faq.selectCategory' | translate"
                            styleClass="w-full"
                        ></p-select>
                    </div>

                    <div class="form-field form-field-full">
                        <label>{{ 'admin.faq.questionEn' | translate }} <span class="required">*</span></label>
                        <input pInputText formControlName="questionEn" class="w-full" [placeholder]="'admin.faq.questionEnPlaceholder' | translate" />
                        @if (form.get('questionEn')?.invalid && form.get('questionEn')?.touched) {
                            <small class="p-error">{{ 'validation.required' | translate }}</small>
                        }
                    </div>

                    <div class="form-field form-field-full">
                        <label>{{ 'admin.faq.questionAr' | translate }}</label>
                        <input pInputText formControlName="questionAr" class="w-full" [placeholder]="'admin.faq.questionArPlaceholder' | translate" />
                    </div>

                    <div class="form-field form-field-full">
                        <label>{{ 'admin.faq.answerEn' | translate }} <span class="required">*</span></label>
                        <textarea pInputTextarea formControlName="answerEn" rows="5" class="w-full" [placeholder]="'admin.faq.answerEnPlaceholder' | translate"></textarea>
                        @if (form.get('answerEn')?.invalid && form.get('answerEn')?.touched) {
                            <small class="p-error">{{ 'validation.required' | translate }}</small>
                        }
                    </div>

                    <div class="form-field form-field-full">
                        <label>{{ 'admin.faq.answerAr' | translate }}</label>
                        <textarea pInputTextarea formControlName="answerAr" rows="5" class="w-full" [placeholder]="'admin.faq.answerArPlaceholder' | translate"></textarea>
                    </div>

                    <div class="form-field">
                        <label>{{ 'admin.faq.sortOrder' | translate }}</label>
                        <p-inputNumber formControlName="sortOrder" [showButtons]="true" [min]="0" class="w-full"></p-inputNumber>
                    </div>

                    <div class="form-field checkbox-row">
                        <p-checkbox formControlName="isActive" [binary]="true" inputId="isActive"></p-checkbox>
                        <label for="isActive">{{ 'admin.faq.isActive' | translate }}</label>
                    </div>

                    <div class="form-field checkbox-row">
                        <p-checkbox formControlName="isFeatured" [binary]="true" inputId="isFeatured"></p-checkbox>
                        <label for="isFeatured">{{ 'admin.faq.isFeatured' | translate }}</label>
                    </div>
                </div>

                <div class="form-actions">
                    <p-button [label]="'common.cancel' | translate" [outlined]="true" (click)="cancel()" [disabled]="saving"></p-button>
                    <p-button [label]="(saving ? 'common.loading' : 'common.save') | translate" type="submit" [loading]="saving" [disabled]="form.invalid || saving"></p-button>
                </div>
            </form>
        </div>
    `,
    styles: [`
        .faq-form-page { padding: 1rem; max-width: 800px; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; }
        .form-grid { display: flex; flex-wrap: wrap; gap: 1rem; margin-bottom: 1.5rem; }
        .form-field { flex: 1; min-width: 200px; }
        .form-field-full { flex: 1 1 100%; }
        .checkbox-row { display: flex; align-items: center; gap: 0.5rem; }
        .required { color: #ef4444; }
        .form-actions { display: flex; gap: 0.75rem; }
    `]
})
export class FaqFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private faqApi = inject(FaqApiService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    languageService = inject(LanguageService);

    form!: FormGroup;
    isEdit = false;
    id: string | null = null;
    saving = false;
    errorMessage = '';

    categoryOptions = CATEGORIES;

    ngOnInit() {
        this.form = this.fb.group({
            category: ['Print3d', Validators.required],
            questionEn: ['', Validators.required],
            questionAr: [''],
            answerEn: ['', Validators.required],
            answerAr: [''],
            sortOrder: [0],
            isActive: [true],
            isFeatured: [false]
        });

        const id = this.route.snapshot.paramMap.get('id');
        if (id && id !== 'new') {
            this.isEdit = true;
            this.id = id;
            this.loadItem(id);
        }
    }

    loadItem(id: string) {
        this.faqApi.getById(id).subscribe({
            next: (item) => {
                if (item) {
                    const cat = typeof item.category === 'number'
                        ? (['Print3d','Laser','Cnc','Software','OrdersShipping','Payments','Support','General'] as const)[item.category] ?? 'General'
                        : item.category;
                    this.form.patchValue({
                        category: cat,
                        questionEn: item.questionEn,
                        questionAr: item.questionAr || '',
                        answerEn: item.answerEn,
                        answerAr: item.answerAr || '',
                        sortOrder: item.sortOrder,
                        isActive: item.isActive,
                        isFeatured: item.isFeatured
                    });
                }
            }
        });
    }

    onSubmit() {
        if (this.form.invalid || this.saving) return;

        this.saving = true;
        this.errorMessage = '';

        const payload: CreateFaqPayload = {
            category: this.form.value.category,
            questionEn: this.form.value.questionEn,
            questionAr: this.form.value.questionAr || undefined,
            answerEn: this.form.value.answerEn,
            answerAr: this.form.value.answerAr || undefined,
            sortOrder: this.form.value.sortOrder ?? 0,
            isActive: this.form.value.isActive ?? true,
            isFeatured: this.form.value.isFeatured ?? false
        };

        const onSuccess = () => {
            this.messageService.add({
                severity: 'success',
                summary: this.translate.instant('common.success'),
                detail: this.translate.instant(this.isEdit ? 'messages.updateSuccess' : 'messages.createSuccess')
            });
            this.router.navigate(['/admin/faq']);
        };
        const onError = (err: { error?: { message?: string } }) => {
            this.errorMessage = err.error?.message || this.translate.instant('messages.saveError');
            this.saving = false;
        };

        if (this.isEdit && this.id) {
            this.faqApi.update(this.id, payload).subscribe({ next: onSuccess, error: onError });
        } else {
            this.faqApi.create(payload).subscribe({ next: onSuccess, error: onError });
        }
    }

    cancel() {
        this.router.navigate(['/admin/faq']);
    }
}
