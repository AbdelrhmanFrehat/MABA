import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../shared/services/language.service';
import { AuthService } from '../../../shared/services/auth.service';
import { DesignRequestService } from '../../../shared/services/design-request.service';
import {
    DESIGN_REQUEST_TYPES,
    type DesignRequestType,
    type IntendedUse,
    type ToleranceLevel,
    CreateDesignRequestPayload
} from '../../../shared/models/design-request.model';

const ACCEPTED_EXTENSIONS = '.png,.jpg,.jpeg,.webp,.pdf,.step,.stp,.iges,.igs,.sldprt,.sldasm,.stl,.obj,.dxf,.zip';
const MAX_FILE_SIZE_MB = 50;
const MAX_FILES = 20;

@Component({
    selector: 'app-design-new-request',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        TranslateModule,
        ButtonModule,
        CardModule,
        StepsModule,
        SelectModule,
        InputTextModule,
        TextareaModule,
        CheckboxModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="design-wizard-page" [dir]="languageService.direction">
            <section class="page-header">
                <div class="header-bg"></div>
                <div class="header-pattern"></div>
                <div class="header-content">
                    <div class="breadcrumb">
                        <a routerLink="/design" class="breadcrumb-link">{{ 'design.hero.badge' | translate }}</a>
                        <i class="pi pi-chevron-right"></i>
                        <span>{{ 'design.wizard.newRequest' | translate }}</span>
                    </div>
                    <h1 class="page-title">{{ 'design.wizard.title' | translate }}</h1>
                </div>
            </section>

            @if (submitted()) {
                <div class="container success-wrapper">
                    <div class="success-card">
                        <div class="success-icon"><i class="pi pi-check-circle"></i></div>
                        <h2>{{ 'design.wizard.successTitle' | translate }}</h2>
                        <p>{{ 'design.wizard.successDesc' | translate }}</p>
                        <div class="reference-block">
                            <span class="ref-label">{{ 'design.wizard.referenceNumber' | translate }}</span>
                            <span class="ref-value">{{ referenceNumber() }}</span>
                        </div>
                        <div class="success-actions">
                            <p-button
                                [label]="'design.wizard.backToDesign' | translate"
                                icon="pi pi-arrow-left"
                                iconPos="left"
                                routerLink="/design"
                                styleClass="success-btn">
                            </p-button>
                            @if (authService.authenticated) {
                                <p-button
                                    [label]="'menu.myRequests' | translate"
                                    icon="pi pi-list"
                                    iconPos="left"
                                    routerLink="/account/requests"
                                    [outlined]="true"
                                    styleClass="success-btn-outline">
                                </p-button>
                            }
                        </div>
                    </div>
                </div>
            } @else {
                <div class="container steps-container">
                    <p-steps [model]="stepsItems()" [activeIndex]="currentStep()" [readonly]="true" styleClass="wizard-steps" />
                </div>

                <div class="container form-container">
                    <!-- Step 1: Starting point -->
                    @if (currentStep() === 0) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'design.wizard.step1Title' | translate }}</h2>
                            <div class="type-grid">
                                @for (type of designRequestTypes; track type) {
                                    <button
                                        type="button"
                                        class="type-option"
                                        [class.selected]="selectedType() === type"
                                        (click)="selectType(type)">
                                        <i class="pi" [ngClass]="getTypeIcon(type)"></i>
                                        <span>{{ 'design.startingPoints.' + type + '.title' | translate }}</span>
                                    </button>
                                }
                            </div>
                            <div class="step-actions">
                                <p-button
                                    [label]="'design.wizard.next' | translate"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    (onClick)="nextStep()"
                                    [disabled]="!selectedType()"
                                    styleClass="step-btn">
                                </p-button>
                            </div>
                        </div>
                    }

                    <!-- Step 2: Uploads -->
                    @if (currentStep() === 1) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'design.wizard.step2Title' | translate }}</h2>
                            <p class="step-hint">{{ uploadHint() }}</p>
                            <div class="upload-zone" (click)="fileInput.click()" (dragover)="onDragOver($event)" (drop)="onDrop($event)">
                                <input #fileInput type="file" multiple [accept]="acceptExtensions" (change)="onFilesSelected($event)" hidden />
                                <i class="pi pi-cloud-upload"></i>
                                <span>{{ 'design.wizard.uploadPrompt' | translate }}</span>
                                <small>{{ 'design.wizard.uploadTypes' | translate }} • {{ 'design.wizard.maxSize' | translate: { size: MAX_FILE_SIZE_MB } }}</small>
                            </div>
                            @if (files().length > 0) {
                                <ul class="file-list">
                                    @for (f of files(); track f.name + f.size) {
                                        <li class="file-item">
                                            <i class="pi pi-file"></i>
                                            <span class="file-name">{{ f.name }}</span>
                                            <span class="file-size">{{ formatSize(f.size) }}</span>
                                            <button type="button" class="file-remove" (click)="removeFile(f)" aria-label="Remove">
                                                <i class="pi pi-times"></i>
                                            </button>
                                        </li>
                                    }
                                </ul>
                            }
                            @if (uploadError()) {
                                <p class="upload-error">{{ uploadError() }}</p>
                            }
                            <div class="step-actions">
                                <p-button [label]="'design.wizard.back' | translate" icon="pi pi-arrow-left" iconPos="left" (onClick)="prevStep()" [text]="true" styleClass="step-btn-text" />
                                <p-button
                                    [label]="'design.wizard.next' | translate"
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    (onClick)="nextStep()"
                                    [disabled]="!canProceedFromUploads()"
                                    styleClass="step-btn" />
                            </div>
                        </div>
                    }

                    <!-- Step 3: Requirements -->
                    @if (currentStep() === 2) {
                        <div class="step-card">
                            <form [formGroup]="requirementsForm">
                                <h2 class="step-heading">{{ 'design.wizard.step3Title' | translate }}</h2>
                                <div class="form-row">
                                    <label>{{ 'design.wizard.titleLabel' | translate }} *</label>
                                    <input pInputText formControlName="title" [placeholder]="'design.wizard.titlePlaceholder' | translate" class="w-full" />
                                    @if (requirementsForm.get('title')?.invalid && requirementsForm.get('title')?.touched) {
                                        <small class="field-error">{{ 'validation.required' | translate }}</small>
                                    }
                                </div>
                                <div class="form-row">
                                    <label>{{ 'design.wizard.descriptionLabel' | translate }}</label>
                                    <textarea pTextarea formControlName="description" rows="4" [placeholder]="'design.wizard.descriptionPlaceholder' | translate" class="w-full"></textarea>
                                </div>
                                <div class="form-grid-2">
                                    <div class="form-row">
                                        <label>{{ 'design.wizard.intendedUse' | translate }}</label>
                                        <p-select formControlName="intendedUse" [options]="intendedUseOptions" optionLabel="label" optionValue="value" [placeholder]="'design.wizard.selectIntendedUse' | translate" styleClass="w-full" />
                                    </div>
                                    <div class="form-row">
                                        <label>{{ 'design.wizard.toleranceLevel' | translate }}</label>
                                        <p-select formControlName="toleranceLevel" [options]="toleranceOptions" optionLabel="label" optionValue="value" [placeholder]="'design.wizard.selectTolerance' | translate" styleClass="w-full" />
                                    </div>
                                </div>
                                <div class="form-grid-2">
                                    <div class="form-row">
                                        <label>{{ 'design.wizard.budgetRange' | translate }}</label>
                                        <p-select formControlName="budgetRange" [options]="budgetOptions" optionLabel="label" optionValue="value" [placeholder]="'design.wizard.selectBudget' | translate" styleClass="w-full" />
                                    </div>
                                    <div class="form-row">
                                        <label>{{ 'design.wizard.timeline' | translate }}</label>
                                        <p-select formControlName="timeline" [options]="timelineOptions" optionLabel="label" optionValue="value" [placeholder]="'design.wizard.selectTimeline' | translate" styleClass="w-full" />
                                    </div>
                                </div>
                                <div class="form-row">
                                    <label>{{ 'design.wizard.materialPreference' | translate }}</label>
                                    <input pInputText formControlName="materialPreference" [placeholder]="'design.wizard.materialPlaceholder' | translate" class="w-full" />
                                </div>
                                <div class="form-row">
                                    <label>{{ 'design.wizard.dimensionsNotes' | translate }}</label>
                                    <input pInputText formControlName="dimensionsNotes" [placeholder]="'design.wizard.dimensionsPlaceholder' | translate" class="w-full" />
                                </div>
                                @if (selectedType() === 'PhysicalObject') {
                                    <div class="form-row checkbox-row">
                                        <p-checkbox formControlName="ipOwnershipConfirmed" [binary]="true" inputId="ipConfirm" />
                                        <label for="ipConfirm" class="checkbox-label">{{ 'design.wizard.ipConfirmLabel' | translate }}</label>
                                    </div>
                                    @if (requirementsForm.get('ipOwnershipConfirmed')?.invalid && requirementsForm.get('ipOwnershipConfirmed')?.touched) {
                                        <small class="field-error">{{ 'design.wizard.ipConfirmRequired' | translate }}</small>
                                    }
                                }
                                <div class="step-actions">
                                    <p-button [label]="'design.wizard.back' | translate" icon="pi pi-arrow-left" iconPos="left" (onClick)="prevStep()" [text]="true" styleClass="step-btn-text" />
                                    <p-button
                                        [label]="'design.wizard.next' | translate"
                                        icon="pi pi-arrow-right"
                                        iconPos="right"
                                        (onClick)="nextStep()"
                                        [disabled]="!requirementsForm.valid || (selectedType() === 'PhysicalObject' && !requirementsForm.get('ipOwnershipConfirmed')?.value)"
                                        styleClass="step-btn" />
                                </div>
                            </form>
                        </div>
                    }

                    <!-- Step 4: Review & Submit -->
                    @if (currentStep() === 3) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'design.wizard.step4Title' | translate }}</h2>
                            <div class="review-block">
                                <p><strong>{{ 'design.wizard.reviewType' | translate }}:</strong> {{ selectedType() ? ('design.startingPoints.' + selectedType() + '.title' | translate) : '-' }}</p>
                                <p><strong>{{ 'design.wizard.titleLabel' | translate }}:</strong> {{ requirementsForm.get('title')?.value || '-' }}</p>
                                <p><strong>{{ 'design.wizard.attachmentsCount' | translate }}:</strong> {{ files().length }}</p>
                                @if (files().length > 0) {
                                    <ul class="review-file-list">
                                        @for (f of files(); track f.name) {
                                            <li>{{ f.name }} ({{ formatSize(f.size) }})</li>
                                        }
                                    </ul>
                                }
                            </div>
                            @if (!authService.authenticated) {
                                <p class="login-required-msg">{{ 'design.wizard.loginRequired' | translate }}</p>
                                <p-button
                                    [label]="'nav.auth.signIn' | translate"
                                    icon="pi pi-user"
                                    (onClick)="goToLogin()"
                                    styleClass="step-btn">
                                </p-button>
                            } @else {
                                <div class="step-actions">
                                    <p-button [label]="'design.wizard.back' | translate" icon="pi pi-arrow-left" iconPos="left" (onClick)="prevStep()" [text]="true" styleClass="step-btn-text" />
                                    <p-button
                                        [label]="'design.wizard.submit' | translate"
                                        icon="pi pi-send"
                                        (onClick)="onSubmit()"
                                        [loading]="submitting()"
                                        styleClass="step-btn submit-btn" />
                                </div>
                            }
                        </div>
                    }
                </div>
            }
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
        }
        .design-wizard-page { min-height: 100vh; background: #fafbfc; padding-bottom: 4rem; }
        .container { max-width: 800px; margin: 0 auto; padding: 0 1.5rem; }
        .page-header { position: relative; padding: 3rem 2rem; overflow: hidden; }
        .header-bg { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .header-pattern { position: absolute; inset: 0; background-image: radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%); z-index: 1; }
        .header-content { position: relative; z-index: 10; text-align: center; }
        .breadcrumb { display: flex; align-items: center; justify-content: center; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 0.75rem; font-size: 0.875rem; }
        .breadcrumb-link { color: rgba(255,255,255,0.85); text-decoration: none; }
        .breadcrumb-link:hover { color: white; }
        .breadcrumb i { color: rgba(255,255,255,0.6); }
        .breadcrumb span { color: white; font-weight: 500; }
        .page-title { font-size: clamp(1.5rem, 3vw, 2rem); font-weight: 700; color: white; margin: 0; }

        .steps-container { padding: 1.5rem 1.5rem 0; }
        :host ::ng-deep .wizard-steps .p-steps-item .p-menuitem-link { padding: 0.75rem; }
        :host ::ng-deep .wizard-steps .p-steps-title { font-size: 0.8rem; }

        .form-container { padding: 1.5rem; }
        .step-card { background: white; border-radius: 20px; padding: 2rem; box-shadow: 0 10px 40px rgba(0,0,0,0.08); margin-bottom: 1rem; }
        .step-heading { font-size: 1.25rem; font-weight: 700; color: #1a1a2e; margin: 0 0 1.25rem 0; }
        .step-hint { font-size: 0.9rem; color: #6b7280; margin-bottom: 1rem; }
        .type-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 0.75rem; margin-bottom: 1.5rem; }
        .type-option {
            display: flex; align-items: center; gap: 0.75rem; padding: 1rem; border: 2px solid #e5e7eb; border-radius: 12px; background: #fafbfc;
            cursor: pointer; text-align: left; transition: all 0.2s; font-size: 0.95rem; font-weight: 500; color: #374151;
        }
        .type-option:hover { border-color: var(--color-primary); background: rgba(102, 126, 234, 0.05); }
        .type-option.selected { border-color: var(--color-primary); background: rgba(102, 126, 234, 0.1); color: var(--color-primary); }
        .type-option i { font-size: 1.25rem; color: var(--color-primary); }
        .step-actions { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem; margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid #e5e7eb; }
        :host ::ng-deep .step-btn { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.75rem 1.25rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .step-btn-text { color: var(--color-primary) !important; }
        :host ::ng-deep .submit-btn { padding: 0.875rem 1.5rem !important; }

        .upload-zone {
            border: 2px dashed #d1d5db; border-radius: 12px; padding: 2rem; text-align: center; cursor: pointer; transition: border-color 0.2s; margin-bottom: 1rem;
            display: flex; flex-direction: column; align-items: center; gap: 0.5rem;
        }
        .upload-zone:hover { border-color: var(--color-primary); background: rgba(102, 126, 234, 0.03); }
        .upload-zone i { font-size: 2rem; color: var(--color-primary); }
        .upload-zone span { font-weight: 500; color: #374151; }
        .upload-zone small { color: #9ca3af; font-size: 0.8rem; }
        .file-list { list-style: none; padding: 0; margin: 0 0 1rem 0; }
        .file-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0; border-bottom: 1px solid #f3f4f6; }
        .file-item i.pi-file { color: var(--color-primary); }
        .file-name { flex: 1; font-size: 0.9rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .file-size { font-size: 0.8rem; color: #6b7280; }
        .file-remove { background: none; border: none; color: #9ca3af; cursor: pointer; padding: 0.25rem; }
        .file-remove:hover { color: #ef4444; }
        .upload-error { color: #ef4444; font-size: 0.875rem; margin-top: 0.5rem; }

        .form-row { margin-bottom: 1.25rem; }
        .form-row label { display: block; font-size: 0.875rem; font-weight: 600; color: #374151; margin-bottom: 0.5rem; }
        .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        @media (max-width: 600px) { .form-grid-2 { grid-template-columns: 1fr; } }
        .field-error { color: #ef4444; font-size: 0.75rem; display: block; margin-top: 0.25rem; }
        .checkbox-row { display: flex; align-items: flex-start; gap: 0.75rem; }
        .checkbox-label { margin-bottom: 0 !important; cursor: pointer; font-weight: 500; }

        .review-block { background: #f8fafc; border-radius: 12px; padding: 1.25rem; margin-bottom: 1.5rem; }
        .review-block p { margin: 0.5rem 0; font-size: 0.95rem; color: #374151; }
        .review-file-list { margin: 0.5rem 0 0 1rem; padding: 0; font-size: 0.875rem; color: #6b7280; }
        .login-required-msg { color: #6b7280; margin-bottom: 1rem; font-size: 0.95rem; }

        .success-wrapper { padding: 3rem 1.5rem; }
        .success-card { background: white; border-radius: 20px; padding: 3rem; box-shadow: 0 10px 40px rgba(0,0,0,0.08); text-align: center; }
        .success-icon { width: 80px; height: 80px; margin: 0 auto 1.5rem; background: var(--gradient-primary); border-radius: 50%; display: flex; align-items: center; justify-content: center; }
        .success-icon i { font-size: 2.5rem; color: white; }
        .success-card h2 { font-size: 1.5rem; font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; }
        .success-card > p { color: #6b7280; margin-bottom: 1.5rem; }
        .reference-block { background: #f3f4f6; border-radius: 12px; padding: 1rem; margin-bottom: 1.5rem; }
        .ref-label { display: block; font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem; }
        .ref-value { font-size: 1.5rem; font-weight: 700; color: var(--color-primary); }
        .success-actions { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
        :host ::ng-deep .success-btn { background: var(--gradient-primary) !important; border: none !important; }
        :host ::ng-deep .success-btn-outline { border: 2px solid var(--color-primary) !important; color: var(--color-primary) !important; }
        .w-full { width: 100%; }
        [dir="rtl"] .type-option { text-align: right; }
    `]
})
export class DesignNewRequestComponent implements OnInit {
    languageService = inject(LanguageService);
    authService = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private fb = inject(FormBuilder);
    private designService = inject(DesignRequestService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    readonly MAX_FILE_SIZE_MB = MAX_FILE_SIZE_MB;
    readonly acceptExtensions = ACCEPTED_EXTENSIONS;
    readonly designRequestTypes = DESIGN_REQUEST_TYPES;

    currentStep = signal(0);
    selectedType = signal<DesignRequestType | null>(null);
    files = signal<File[]>([]);
    uploadError = signal<string | null>(null);
    submitting = signal(false);
    submitted = signal(false);
    referenceNumber = signal('');

    get intendedUseOptions(): { label: string; value: IntendedUse }[] {
        return [
            { label: this.translate.instant('design.wizard.intendedUseOpt.prototype'), value: 'prototype' },
            { label: this.translate.instant('design.wizard.intendedUseOpt.final'), value: 'final' },
            { label: this.translate.instant('design.wizard.intendedUseOpt.academic'), value: 'academic' },
            { label: this.translate.instant('design.wizard.intendedUseOpt.industrial'), value: 'industrial' }
        ];
    }
    get toleranceOptions(): { label: string; value: ToleranceLevel }[] {
        return [
            { label: this.translate.instant('design.wizard.toleranceOpt.standard'), value: 'standard' },
            { label: this.translate.instant('design.wizard.toleranceOpt.precise'), value: 'precise' },
            { label: this.translate.instant('design.wizard.toleranceOpt.unknown'), value: 'unknown' }
        ];
    }
    get budgetOptions(): { label: string; value: string }[] {
        return [
            { label: this.translate.instant('design.wizard.budgetOpt.under500'), value: 'under500' },
            { label: this.translate.instant('design.wizard.budgetOpt.500-2000'), value: '500-2000' },
            { label: this.translate.instant('design.wizard.budgetOpt.2000-5000'), value: '2000-5000' },
            { label: this.translate.instant('design.wizard.budgetOpt.5000plus'), value: '5000plus' },
            { label: this.translate.instant('design.wizard.budgetOpt.discuss'), value: 'discuss' }
        ];
    }
    get timelineOptions(): { label: string; value: string }[] {
        return [
            { label: this.translate.instant('design.wizard.timelineOpt.asap'), value: 'asap' },
            { label: this.translate.instant('design.wizard.timelineOpt.1-2weeks'), value: '1-2weeks' },
            { label: this.translate.instant('design.wizard.timelineOpt.1month'), value: '1month' },
            { label: this.translate.instant('design.wizard.timelineOpt.2-3months'), value: '2-3months' },
            { label: this.translate.instant('design.wizard.timelineOpt.flexible'), value: 'flexible' }
        ];
    }

    requirementsForm: FormGroup = this.fb.group({
        title: ['', Validators.required],
        description: [''],
        intendedUse: [null as IntendedUse | null],
        materialPreference: [''],
        dimensionsNotes: [''],
        toleranceLevel: [null as ToleranceLevel | null],
        budgetRange: [null as string | null],
        timeline: [null as string | null],
        ipOwnershipConfirmed: [false]
    });

    stepsItems = computed(() => [
        { label: this.languageService.translate('design.wizard.step1') },
        { label: this.languageService.translate('design.wizard.step2') },
        { label: this.languageService.translate('design.wizard.step3') },
        { label: this.languageService.translate('design.wizard.step4') }
    ]);

    uploadHint = computed(() => {
        const t = this.selectedType();
        if (t === 'PhysicalObject') return this.languageService.translate('design.wizard.uploadHintPhysical');
        if (t === 'BrokenDesign' || t === 'ExistingCAD') return this.languageService.translate('design.wizard.uploadHintRequired');
        return this.languageService.translate('design.wizard.uploadHintOptional');
    });

    ngOnInit() {
        const type = this.route.snapshot.queryParams['type'] as string | undefined;
        if (type && DESIGN_REQUEST_TYPES.includes(type as DesignRequestType)) {
            this.selectedType.set(type as DesignRequestType);
        }
    }

    getTypeIcon(type: DesignRequestType): string {
        const icons: Record<DesignRequestType, string> = {
            IdeaOnly: 'pi-lightbulb',
            BrokenDesign: 'pi-wrench',
            PhysicalObject: 'pi-box',
            ExistingCAD: 'pi-file-edit',
            TechnicalDrawings: 'pi-list',
            ImproveExistingProduct: 'pi-arrow-right-arrow-left'
        };
        return icons[type] || 'pi-file';
    }

    selectType(type: DesignRequestType) {
        this.selectedType.set(type);
        this.uploadError.set(null);
    }

    nextStep() {
        if (this.currentStep() < 3) this.currentStep.update(s => s + 1);
    }

    prevStep() {
        if (this.currentStep() > 0) this.currentStep.update(s => s - 1);
    }

    onFilesSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const list = input.files ? Array.from(input.files) : [];
        this.addFiles(list);
        input.value = '';
    }

    onDragOver(e: DragEvent) {
        e.preventDefault();
        e.stopPropagation();
    }

    onDrop(e: DragEvent) {
        e.preventDefault();
        e.stopPropagation();
        const list = e.dataTransfer?.files ? Array.from(e.dataTransfer.files) : [];
        this.addFiles(list);
    }

    private addFiles(list: File[]) {
        const current = this.files();
        const maxSize = MAX_FILE_SIZE_MB * 1024 * 1024;
        const allowedExt = new Set(ACCEPTED_EXTENSIONS.split(',').map(x => x.trim().toLowerCase()));
        let err: string | null = null;
        const added: File[] = [];
        for (const f of list) {
            if (current.length + added.length >= MAX_FILES) {
                err = `Maximum ${MAX_FILES} files.`;
                break;
            }
            const ext = '.' + (f.name.split('.').pop() || '').toLowerCase();
            if (!allowedExt.has(ext)) continue;
            if (f.size > maxSize) {
                err = `File ${f.name} exceeds ${MAX_FILE_SIZE_MB} MB.`;
                break;
            }
            added.push(f);
        }
        this.uploadError.set(err);
        if (added.length) this.files.set([...current, ...added]);
    }

    removeFile(file: File) {
        this.files.update(list => list.filter(f => f !== file));
        this.uploadError.set(null);
    }

    canProceedFromUploads(): boolean {
        const t = this.selectedType();
        const list = this.files();
        if (t === 'PhysicalObject') return list.length >= 3;
        if (t === 'BrokenDesign' || t === 'ExistingCAD') return list.length >= 1;
        return true;
    }

    formatSize(bytes: number): string {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return (bytes / Math.pow(k, i)).toFixed(1) + ' ' + ['B', 'KB', 'MB', 'GB'][i];
    }

    goToLogin() {
        this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url || '/design/new' } });
    }

    onSubmit() {
        if (!this.authService.authenticated) {
            this.goToLogin();
            return;
        }
        if (this.requirementsForm.invalid) return;
        const t = this.selectedType();
        if (t === 'PhysicalObject' && !this.requirementsForm.get('ipOwnershipConfirmed')?.value) return;

        this.submitting.set(true);
        const payload: CreateDesignRequestPayload = {
            requestType: t!,
            title: this.requirementsForm.get('title')!.value,
            description: this.requirementsForm.get('description')?.value || undefined,
            intendedUse: this.requirementsForm.get('intendedUse')?.value || undefined,
            materialPreference: this.requirementsForm.get('materialPreference')?.value || undefined,
            dimensionsNotes: this.requirementsForm.get('dimensionsNotes')?.value || undefined,
            toleranceLevel: this.requirementsForm.get('toleranceLevel')?.value || undefined,
            budgetRange: this.requirementsForm.get('budgetRange')?.value || undefined,
            timeline: this.requirementsForm.get('timeline')?.value || undefined,
            ipOwnershipConfirmed: t === 'PhysicalObject' ? this.requirementsForm.get('ipOwnershipConfirmed')?.value : undefined,
            attachmentFileNames: this.files().map(f => f.name)
        };

        this.designService.createRequest(payload, this.files()).subscribe({
            next: (res) => {
                this.submitting.set(false);
                this.referenceNumber.set(res.referenceNumber);
                this.submitted.set(true);
            },
            error: () => {
                this.submitting.set(false);
                this.messageService.add({
                    severity: 'error',
                    summary: this.languageService.translate('messages.error'),
                    detail: this.languageService.translate('messages.saveError')
                });
            }
        });
    }
}
