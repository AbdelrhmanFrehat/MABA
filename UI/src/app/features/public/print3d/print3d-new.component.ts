import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FileUploadModule } from 'primeng/fileupload';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { StepsModule } from 'primeng/steps';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Material, Print3dMaterial, Print3dProfile, Print3dEstimate, CreatePrint3dRequestRequest, PrintQualityProfile, MaterialColor } from '../../../shared/models/printing.model';

@Component({
    selector: 'app-print3d-new',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        TranslateModule,
        FileUploadModule,
        CardModule,
        ButtonModule,
        SelectModule,
        TextareaModule,
        ToastModule,
        ProgressSpinnerModule,
        StepsModule,
        DividerModule,
        DialogModule,
        TooltipModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="print3d-new-page" [dir]="languageService.direction">
            <!-- Header Section -->
            <section class="page-header">
                <div class="header-bg"></div>
                <div class="header-pattern"></div>
                <div class="header-content">
                    <div class="breadcrumb">
                        <a routerLink="/3d-print" class="breadcrumb-link">
                            <i class="pi pi-print"></i>
                            {{ languageService.language === 'ar' ? 'الطباعة ثلاثية الأبعاد' : '3D Printing' }}
                        </a>
                        <i class="pi pi-chevron-right"></i>
                        <span>{{ languageService.language === 'ar' ? 'طلب جديد' : 'New Request' }}</span>
                    </div>
                    <h1 class="page-title">
                        {{ languageService.language === 'ar' ? 'إنشاء طلب طباعة جديد' : 'Create New Print Request' }}
                    </h1>
                    <p class="page-description">
                        {{ languageService.language === 'ar' 
                            ? 'ارفع تصميمك واختر إعداداتك للحصول على طباعة احترافية' 
                            : 'Upload your design and choose your settings for a professional print' }}
                    </p>
                </div>
            </section>

            <!-- Progress Steps -->
            <section class="progress-section">
                <div class="container">
                    <div class="progress-steps">
                        <div class="progress-step" [class.active]="currentStep >= 1" [class.completed]="currentStep > 1">
                            <div class="step-circle">
                                <i class="pi" [class.pi-check]="currentStep > 1" [class.pi-upload]="currentStep <= 1"></i>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'رفع الملف' : 'Upload File' }}</span>
                        </div>
                        <div class="progress-line" [class.active]="currentStep > 1"></div>
                        <div class="progress-step" [class.active]="currentStep >= 2" [class.completed]="currentStep > 2">
                            <div class="step-circle">
                                <i class="pi" [class.pi-check]="currentStep > 2" [class.pi-cog]="currentStep <= 2"></i>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'الإعدادات' : 'Settings' }}</span>
                        </div>
                        <div class="progress-line" [class.active]="currentStep > 2"></div>
                        <div class="progress-step" [class.active]="currentStep >= 3">
                            <div class="step-circle">
                                <i class="pi pi-send"></i>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'إرسال' : 'Submit' }}</span>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Main Form -->
            <section class="form-section">
                <div class="container">
                    <form [formGroup]="requestForm" (ngSubmit)="onSubmit()">
                        <div class="form-grid">
                            <!-- Left Column - Form -->
                            <div class="form-column">
                                <!-- File Upload Card -->
                                <div class="form-card" [class.completed]="selectedFile">
                                    <div class="card-header">
                                        <div class="card-icon">
                                            <i class="pi pi-upload"></i>
                                        </div>
                                        <div class="card-title-group">
                                            <h3 class="card-title">{{ languageService.language === 'ar' ? 'رفع التصميم' : 'Upload Design' }}</h3>
                                            <span class="card-badge required">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                        </div>
                                    </div>
                                    <div class="card-body">
                                        <div class="upload-zone" [class.has-file]="selectedFile" (drop)="onDrop($event)" (dragover)="onDragOver($event)">
                                            <div class="upload-content" *ngIf="!selectedFile">
                                                <div class="upload-icon">
                                                    <i class="pi pi-cloud-upload"></i>
                                                </div>
                                                <p class="upload-text">
                                                    {{ languageService.language === 'ar' ? 'اسحب الملف هنا أو' : 'Drag your file here or' }}
                                                </p>
                                                <p-fileUpload 
                                                    #fileUpload
                                                    mode="basic"
                                                    name="file" 
                                                    [multiple]="false"
                                                    accept=".stl"
                                                    [maxFileSize]="100000000"
                                                    [auto]="false"
                                                    [chooseLabel]="languageService.language === 'ar' ? 'اختر ملف' : 'Choose File'"
                                                    (onSelect)="onFileSelect($event)"
                                                    styleClass="upload-button">
                                                </p-fileUpload>
                                                <p class="upload-formats">
                                                    <i class="pi pi-info-circle"></i>
                                                    {{ languageService.language === 'ar' ? 'الصيغ المدعومة: STL' : 'Supported: STL only' }}
                                                </p>
                                            </div>
                                            <div class="file-preview" *ngIf="selectedFile">
                                                <div class="file-icon">
                                                    <i class="pi pi-file-3d"></i>
                                                </div>
                                                <div class="file-info">
                                                    <span class="file-name">{{ selectedFile.name }}</span>
                                                    <span class="file-size">{{ formatFileSize(selectedFile.size) }}</span>
                                                </div>
                                                <button type="button" class="file-remove" (click)="onFileRemove()">
                                                    <i class="pi pi-times"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Material Selection Card -->
                                <div class="form-card" [class.completed]="selectedMaterial">
                                    <div class="card-header">
                                        <div class="card-icon">
                                            <i class="pi pi-box"></i>
                                        </div>
                                        <div class="card-title-group">
                                            <h3 class="card-title">{{ languageService.language === 'ar' ? 'اختر المادة' : 'Select Material' }}</h3>
                                            <span class="card-badge required">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                        </div>
                                    </div>
                                    <div class="card-body">
                                        <div class="material-grid">
                                            <div 
                                                class="material-option" 
                                                *ngFor="let option of materialOptions"
                                                [class.selected]="requestForm.get('materialId')?.value === option.id"
                                                (click)="selectMaterial(option)">
                                                <div class="material-color" [style.background]="option.material.color || 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'"></div>
                                                <div class="material-info">
                                                    <span class="material-name">{{ option.name }}</span>
                                                    <span class="material-type" *ngIf="option.material.color">{{ option.material.color }}</span>
                                                </div>
                                                <div class="material-price">{{ formatCurrency(option.material.pricePerGram) }}/g</div>
                                                <div class="material-check" *ngIf="requestForm.get('materialId')?.value === option.id">
                                                    <i class="pi pi-check"></i>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Color Selection Card (only shown if material has colors) -->
                                <div class="form-card" *ngIf="selectedMaterial && availableColors.length > 0" [class.completed]="selectedColor">
                                    <div class="card-header">
                                        <div class="card-icon">
                                            <i class="pi pi-palette"></i>
                                        </div>
                                        <div class="card-title-group">
                                            <h3 class="card-title">{{ languageService.language === 'ar' ? 'اختر اللون' : 'Select Color' }}</h3>
                                            <span class="card-badge required">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                        </div>
                                    </div>
                                    <div class="card-body">
                                        <div class="color-loading" *ngIf="loadingColors">
                                            <p-progressSpinner strokeWidth="3" [style]="{width: '30px', height: '30px'}"></p-progressSpinner>
                                            <span>{{ languageService.language === 'ar' ? 'جاري تحميل الألوان...' : 'Loading colors...' }}</span>
                                        </div>
                                        <div class="color-grid" *ngIf="!loadingColors">
                                            <div 
                                                class="color-option" 
                                                *ngFor="let color of availableColors"
                                                [class.selected]="selectedColor?.id === color.id"
                                                (click)="selectColor(color)"
                                                [pTooltip]="getColorName(color)"
                                                tooltipPosition="top">
                                                <div class="color-swatch" [style.background-color]="color.hexCode"></div>
                                                <span class="color-name">{{ getColorName(color) }}</span>
                                                <div class="color-check" *ngIf="selectedColor?.id === color.id">
                                                    <i class="pi pi-check"></i>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Profile Selection Card -->
                                <div class="form-card" [class.completed]="selectedProfile">
                                    <div class="card-header">
                                        <div class="card-icon">
                                            <i class="pi pi-sliders-h"></i>
                                        </div>
                                        <div class="card-title-group">
                                            <h3 class="card-title">{{ languageService.language === 'ar' ? 'جودة الطباعة' : 'Print Quality' }}</h3>
                                            <span class="card-badge required">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                        </div>
                                    </div>
                                    <div class="card-body">
                                        <div class="profile-grid">
                                            <div 
                                                class="profile-option" 
                                                *ngFor="let option of profileOptions"
                                                [class.selected]="selectedProfile?.id === option.id"
                                                (click)="selectProfile(option)"
                                                tabindex="0"
                                                (keydown.enter)="selectProfile(option)"
                                                (keydown.space)="selectProfile(option); $event.preventDefault()">
                                                <div class="profile-header">
                                                    <span class="profile-name">{{ option.name }}</span>
                                                    <span class="profile-multiplier">{{ option.profile.priceMultiplier }}x</span>
                                                </div>
                                                <div class="profile-specs">
                                                    <div class="spec">
                                                        <span class="spec-label">{{ languageService.language === 'ar' ? 'الطبقة' : 'Layer' }}</span>
                                                        <span class="spec-value">{{ option.profile.layerHeightMm }}mm</span>
                                                    </div>
                                                    <div class="spec">
                                                        <span class="spec-label">{{ languageService.language === 'ar' ? 'السرعة' : 'Speed' }}</span>
                                                        <span class="spec-value">{{ option.profile.speedCategory }}</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Comments Card -->
                                <div class="form-card">
                                    <div class="card-header">
                                        <div class="card-icon">
                                            <i class="pi pi-comment"></i>
                                        </div>
                                        <div class="card-title-group">
                                            <h3 class="card-title">{{ languageService.language === 'ar' ? 'ملاحظات إضافية' : 'Additional Notes' }}</h3>
                                            <span class="card-badge optional">{{ languageService.language === 'ar' ? 'اختياري' : 'Optional' }}</span>
                                        </div>
                                    </div>
                                    <div class="card-body">
                                        <textarea
                                            pInputTextarea
                                            formControlName="comments"
                                            [rows]="4"
                                            class="comments-textarea"
                                            [placeholder]="languageService.language === 'ar' ? 'أي تعليمات خاصة أو ملاحظات...' : 'Any special instructions or notes...'">
                                        </textarea>
                                    </div>
                                </div>
                            </div>

                            <!-- Right Column - Summary -->
                            <div class="summary-column">
                                <div class="summary-card" [class.sticky]="true">
                                    <div class="summary-header">
                                        <h3>{{ languageService.language === 'ar' ? 'ملخص الطلب' : 'Order Summary' }}</h3>
                                    </div>
                                    
                                    <div class="summary-body">
                                        <!-- File Summary -->
                                        <div class="summary-item" [class.empty]="!selectedFile">
                                            <div class="summary-icon">
                                                <i class="pi pi-file"></i>
                                            </div>
                                            <div class="summary-info">
                                                <span class="summary-label">{{ languageService.language === 'ar' ? 'الملف' : 'File' }}</span>
                                                <span class="summary-value">{{ selectedFile?.name || (languageService.language === 'ar' ? 'لم يتم اختياره' : 'Not selected') }}</span>
                                            </div>
                                            <i class="pi" [class.pi-check-circle]="selectedFile" [class.pi-circle]="!selectedFile" [class.text-primary-500]="selectedFile"></i>
                                        </div>

                                        <!-- Material Summary -->
                                        <div class="summary-item" [class.empty]="!selectedMaterial">
                                            <div class="summary-icon">
                                                <i class="pi pi-box"></i>
                                            </div>
                                            <div class="summary-info">
                                                <span class="summary-label">{{ languageService.language === 'ar' ? 'المادة' : 'Material' }}</span>
                                                <span class="summary-value">{{ getMaterialName() || (languageService.language === 'ar' ? 'لم يتم اختياره' : 'Not selected') }}</span>
                                            </div>
                                            <i class="pi" [class.pi-check-circle]="selectedMaterial" [class.pi-circle]="!selectedMaterial" [class.text-primary-500]="selectedMaterial"></i>
                                        </div>

                                        <!-- Color Summary (only if material has colors) -->
                                        <div class="summary-item" [class.empty]="!selectedColor" *ngIf="availableColors.length > 0">
                                            <div class="summary-icon">
                                                <div class="summary-color-swatch" [style.background-color]="selectedColor?.hexCode || '#e9ecef'"></div>
                                            </div>
                                            <div class="summary-info">
                                                <span class="summary-label">{{ languageService.language === 'ar' ? 'اللون' : 'Color' }}</span>
                                                <span class="summary-value">{{ getSelectedColorName() || (languageService.language === 'ar' ? 'لم يتم اختياره' : 'Not selected') }}</span>
                                            </div>
                                            <i class="pi" [class.pi-check-circle]="selectedColor" [class.pi-circle]="!selectedColor" [class.text-primary-500]="selectedColor"></i>
                                        </div>

                                        <!-- Profile Summary -->
                                        <div class="summary-item">
                                            <div class="summary-icon">
                                                <i class="pi pi-sliders-h"></i>
                                            </div>
                                            <div class="summary-info">
                                                <span class="summary-label">{{ languageService.language === 'ar' ? 'الجودة' : 'Quality' }}</span>
                                                <span class="summary-value">{{ getProfileName() || (languageService.language === 'ar' ? 'الافتراضي' : 'Default') }}</span>
                                            </div>
                                            <i class="pi pi-check-circle text-primary-500"></i>
                                        </div>

                                        <div class="summary-divider"></div>

                                        <!-- Estimate Section -->
                                        <div class="estimate-section" *ngIf="estimate">
                                            <div class="estimate-row">
                                                <span>{{ languageService.language === 'ar' ? 'كمية المادة' : 'Material Amount' }}</span>
                                                <span>{{ estimate.materialGrams }}g</span>
                                            </div>
                                            <div class="estimate-row">
                                                <span>{{ languageService.language === 'ar' ? 'الوقت المتوقع' : 'Estimated Time' }}</span>
                                                <span>{{ estimate.estimatedTimeHours }} {{ languageService.language === 'ar' ? 'ساعة' : 'hours' }}</span>
                                            </div>
                                            <div class="estimate-total">
                                                <span>{{ languageService.language === 'ar' ? 'التكلفة التقديرية' : 'Estimated Cost' }}</span>
                                                <span class="total-price">{{ formatCurrency(estimate.estimatedPrice) }}</span>
                                            </div>
                                        </div>

                                        <div class="estimate-placeholder" *ngIf="!estimate && !estimating">
                                            <i class="pi pi-calculator"></i>
                                            <p>{{ languageService.language === 'ar' ? 'اختر الملف والمادة لحساب التكلفة' : 'Select file and material to calculate cost' }}</p>
                                        </div>

                                        <div class="estimate-loading" *ngIf="estimating">
                                            <p-progressSpinner strokeWidth="3" [style]="{width: '40px', height: '40px'}"></p-progressSpinner>
                                            <p>{{ languageService.language === 'ar' ? 'جاري الحساب...' : 'Calculating...' }}</p>
                                        </div>
                                    </div>

                                    <div class="summary-actions">
                                        <p-button 
                                            [label]="languageService.language === 'ar' ? 'إلغاء' : 'Cancel'"
                                            [outlined]="true"
                                            type="button"
                                            (click)="onCancel()"
                                            styleClass="cancel-btn w-full">
                                        </p-button>
                                        <p-button 
                                            [label]="languageService.language === 'ar' ? 'إرسال الطلب' : 'Submit Request'"
                                            icon="pi pi-send"
                                            iconPos="right"
                                            type="submit"
                                            [disabled]="!canSubmit()"
                                            [loading]="submitting"
                                            styleClass="submit-btn w-full">
                                        </p-button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </section>

            <!-- Success Dialog -->
            <p-dialog 
                [(visible)]="showSuccessDialog" 
                [modal]="true" 
                [closable]="true"
                [draggable]="false"
                [resizable]="false"
                styleClass="success-dialog"
                [style]="{ width: '420px', maxWidth: '90vw' }">
                <ng-template pTemplate="header">
                    <div class="success-header">
                        <i class="pi pi-check-circle"></i>
                        <span>{{ languageService.language === 'ar' ? 'تم إرسال الطلب بنجاح' : 'Request Submitted Successfully' }}</span>
                    </div>
                </ng-template>
                <div class="success-content">
                    <p class="success-message">{{ languageService.language === 'ar' 
                        ? 'تم استلام طلبك وسنتواصل معك قريباً.' 
                        : 'Your request has been received. We will contact you soon.' }}</p>
                    <div class="reference-box" *ngIf="submittedReferenceNumber">
                        <span class="reference-label">{{ languageService.language === 'ar' ? 'الرقم المرجعي' : 'Reference Number' }}</span>
                        <span class="reference-number">{{ submittedReferenceNumber }}</span>
                    </div>
                    <p class="success-note">{{ languageService.language === 'ar' 
                        ? 'يرجى الاحتفاظ بهذا الرقم للمتابعة.' 
                        : 'Please save this number for tracking.' }}</p>
                </div>
                <ng-template pTemplate="footer">
                    <p-button 
                        [label]="languageService.language === 'ar' ? 'عرض طلباتي' : 'View My Requests'" 
                        (onClick)="goToRequests()"
                        styleClass="success-close-btn w-full">
                    </p-button>
                </ng-template>
            </p-dialog>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
        }

        .print3d-new-page {
            min-height: 100vh;
            background: #f8f9fa;
        }

        /* ============ HEADER SECTION ============ */
        .page-header {
            position: relative;
            padding: 4rem 2rem 3rem;
            overflow: hidden;
        }

        .header-bg {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .header-pattern {
            position: absolute;
            inset: 0;
            background-image: 
                radial-gradient(circle at 20% 50%, rgba(102, 126, 234, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 80% 50%, rgba(118, 75, 162, 0.1) 0%, transparent 50%);
            z-index: 1;
        }

        .header-content {
            position: relative;
            z-index: 2;
            max-width: 1200px;
            margin: 0 auto;
        }

        .breadcrumb {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-bottom: 1.5rem;
            font-size: 0.9rem;
        }

        .breadcrumb-link {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: rgba(255,255,255,0.7);
            text-decoration: none;
            transition: color 0.3s ease;
        }

        .breadcrumb-link:hover {
            color: white;
        }

        .breadcrumb .pi-chevron-right {
            color: rgba(255,255,255,0.4);
            font-size: 0.75rem;
        }

        .breadcrumb span {
            color: white;
        }

        .page-title {
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 700;
            color: white;
            margin-bottom: 0.75rem;
        }

        .page-description {
            font-size: 1.1rem;
            color: rgba(255,255,255,0.75);
            margin: 0;
        }

        /* ============ PROGRESS SECTION ============ */
        .progress-section {
            background: white;
            padding: 2rem;
            border-bottom: 1px solid #e9ecef;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        .progress-steps {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0;
        }

        .progress-step {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
        }

        .step-circle {
            width: 50px;
            height: 50px;
            border-radius: 50%;
            background: #e9ecef;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.25rem;
            color: #6c757d;
            transition: all 0.3s ease;
        }

        .progress-step.active .step-circle {
            background: var(--gradient-primary);
            color: white;
            box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
        }

        .progress-step.completed .step-circle {
            background: #28a745;
            color: white;
        }

        .step-label {
            font-size: 0.875rem;
            color: #6c757d;
            font-weight: 500;
        }

        .progress-step.active .step-label {
            color: var(--color-primary);
            font-weight: 600;
        }

        .progress-line {
            width: 100px;
            height: 4px;
            background: #e9ecef;
            margin: 0 1rem;
            margin-bottom: 1.5rem;
            border-radius: 2px;
            transition: background 0.3s ease;
        }

        .progress-line.active {
            background: var(--gradient-primary);
        }

        /* ============ FORM SECTION ============ */
        .form-section {
            padding: 3rem 2rem;
        }

        .form-grid {
            display: grid;
            grid-template-columns: 1fr 380px;
            gap: 2rem;
            align-items: start;
        }

        .form-column {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        /* ============ FORM CARDS ============ */
        .form-card {
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            border: 2px solid transparent;
            transition: all 0.3s ease;
        }

        .form-card:hover {
            box-shadow: 0 8px 30px rgba(0,0,0,0.12);
        }

        .form-card.completed {
            border-color: var(--p-primary-color, #667eea);
        }

        .card-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1.25rem 1.5rem;
            background: #f8f9fa;
            border-bottom: 1px solid #e9ecef;
        }

        .card-icon {
            width: 45px;
            height: 45px;
            background: var(--gradient-primary);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.25rem;
        }

        .card-title-group {
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .card-title {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a1a2e;
            margin: 0;
        }

        .card-badge {
            padding: 0.25rem 0.75rem;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .card-badge.required {
            background: rgba(220, 53, 69, 0.1);
            color: #dc3545;
        }

        .card-badge.optional {
            background: rgba(108, 117, 125, 0.1);
            color: #6c757d;
        }

        .card-body {
            padding: 1.5rem;
        }

        /* ============ UPLOAD ZONE ============ */
        .upload-zone {
            border: 2px dashed #dee2e6;
            border-radius: 12px;
            padding: 3rem 2rem;
            text-align: center;
            transition: all 0.3s ease;
            background: #fafbfc;
        }

        .upload-zone:hover,
        .upload-zone.dragover {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
        }

        .upload-zone.has-file {
            border-style: solid;
            border-color: var(--p-primary-color, #667eea);
            background: rgba(102, 126, 234, 0.05);
            padding: 1.5rem;
        }

        .upload-icon {
            font-size: 3.5rem;
            color: var(--color-primary);
            margin-bottom: 1rem;
        }

        .upload-text {
            color: #6c757d;
            margin-bottom: 1rem;
        }

        :host ::ng-deep .upload-button .p-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
            padding: 0.75rem 2rem !important;
        }

        .upload-formats {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            margin-top: 1rem;
            font-size: 0.85rem;
            color: #6c757d;
        }

        .file-preview {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .file-icon {
            width: 50px;
            height: 50px;
            background: var(--gradient-primary);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.5rem;
        }

        .file-info {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: flex-start;
        }

        .file-name {
            font-weight: 600;
            color: #1a1a2e;
        }

        .file-size {
            font-size: 0.85rem;
            color: #6c757d;
        }

        .file-remove {
            width: 36px;
            height: 36px;
            border: none;
            background: #f8f9fa;
            border-radius: 50%;
            cursor: pointer;
            color: #dc3545;
            transition: all 0.3s ease;
        }

        .file-remove:hover {
            background: #dc3545;
            color: white;
        }

        /* ============ MATERIAL GRID ============ */
        .material-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
            gap: 1rem;
        }

        .material-option {
            position: relative;
            padding: 1rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .material-option:hover {
            border-color: var(--color-primary);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.15);
        }

        .material-option.selected {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
        }

        .material-color {
            width: 100%;
            height: 8px;
            border-radius: 4px;
        }

        .material-info {
            display: flex;
            flex-direction: column;
        }

        .material-name {
            font-weight: 600;
            color: #1a1a2e;
        }

        .material-type {
            font-size: 0.8rem;
            color: #6c757d;
        }

        .material-price {
            font-weight: 700;
            color: var(--color-primary);
        }

        .material-check {
            position: absolute;
            top: 0.75rem;
            right: 0.75rem;
            width: 24px;
            height: 24px;
            background: var(--color-primary);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 0.75rem;
        }

        [dir="rtl"] .material-check {
            right: auto;
            left: 0.75rem;
        }

        /* ============ PROFILE GRID ============ */
        .profile-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
            gap: 1rem;
        }

        .profile-option {
            padding: 1.25rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .profile-option:hover {
            border-color: var(--color-primary);
        }

        .profile-option.selected {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
        }

        .profile-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .profile-name {
            font-weight: 600;
            color: #1a1a2e;
        }

        .profile-multiplier {
            padding: 0.25rem 0.5rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .profile-specs {
            display: flex;
            gap: 1rem;
        }

        .spec {
            display: flex;
            flex-direction: column;
        }

        .spec-label {
            font-size: 0.75rem;
            color: #6c757d;
        }

        .spec-value {
            font-weight: 600;
            color: #1a1a2e;
        }

        /* ============ COMMENTS ============ */
        .comments-textarea {
            width: 100%;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            padding: 1rem;
            resize: vertical;
            transition: border-color 0.3s ease;
        }

        .comments-textarea:focus {
            border-color: var(--color-primary);
            outline: none;
        }

        /* ============ SUMMARY COLUMN ============ */
        .summary-column {
            position: relative;
        }

        .summary-card {
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }

        .summary-card.sticky {
            position: sticky;
            top: 2rem;
        }

        .summary-header {
            padding: 1.5rem;
            background: var(--gradient-dark);
        }

        .summary-header h3 {
            color: white;
            margin: 0;
            font-size: 1.25rem;
        }

        .summary-body {
            padding: 1.5rem;
        }

        .summary-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem 0;
            border-bottom: 1px solid #e9ecef;
        }

        .summary-item:last-child {
            border-bottom: none;
        }

        .summary-item.empty .summary-value {
            color: #adb5bd;
        }

        .summary-icon {
            width: 40px;
            height: 40px;
            background: #f8f9fa;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--color-primary);
        }

        .summary-info {
            flex: 1;
            display: flex;
            flex-direction: column;
        }

        .summary-label {
            font-size: 0.8rem;
            color: #6c757d;
        }

        .summary-value {
            font-weight: 600;
            color: #1a1a2e;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 180px;
        }

        .summary-divider {
            height: 1px;
            background: #e9ecef;
            margin: 1rem 0;
        }

        /* ============ ESTIMATE ============ */
        .estimate-section {
            background: #f8f9fa;
            border-radius: 12px;
            padding: 1.25rem;
        }

        .estimate-row {
            display: flex;
            justify-content: space-between;
            padding: 0.5rem 0;
            font-size: 0.9rem;
            color: #6c757d;
        }

        .estimate-total {
            display: flex;
            justify-content: space-between;
            padding-top: 1rem;
            margin-top: 0.5rem;
            border-top: 2px solid #e9ecef;
            font-weight: 600;
            color: #1a1a2e;
        }

        .total-price {
            font-size: 1.5rem;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .estimate-placeholder,
        .estimate-loading {
            text-align: center;
            padding: 2rem;
            color: #6c757d;
        }

        .estimate-placeholder i,
        .estimate-loading i {
            font-size: 2.5rem;
            margin-bottom: 0.75rem;
            color: #dee2e6;
        }

        .estimate-loading {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 1rem;
        }

        /* ============ ACTIONS ============ */
        .summary-actions {
            padding: 1.5rem;
            border-top: 1px solid #e9ecef;
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        :host ::ng-deep .cancel-btn {
            border-radius: 50px !important;
        }

        :host ::ng-deep .submit-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
        }

        :host ::ng-deep .submit-btn:disabled {
            opacity: 0.6;
        }

        /* ============ COLOR GRID ============ */
        .color-loading {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.75rem;
            padding: 2rem;
            color: #6c757d;
        }

        .color-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
            gap: 0.75rem;
        }

        .color-option {
            position: relative;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 1rem 0.75rem;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
        }

        .color-option:hover {
            border-color: var(--color-primary);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.15);
            transform: translateY(-2px);
        }

        .color-option.selected {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.15);
        }

        .color-swatch {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            border: 2px solid rgba(0,0,0,0.1);
            margin-bottom: 0.5rem;
            box-shadow: 0 2px 8px rgba(0,0,0,0.15);
        }

        .color-name {
            font-size: 0.8rem;
            font-weight: 500;
            color: #1a1a2e;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 100%;
        }

        .color-check {
            position: absolute;
            top: 0.5rem;
            right: 0.5rem;
            width: 20px;
            height: 20px;
            background: var(--color-primary);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 0.65rem;
        }

        [dir="rtl"] .color-check {
            right: auto;
            left: 0.5rem;
        }

        .summary-color-swatch {
            width: 40px;
            height: 40px;
            border-radius: 10px;
            border: 2px solid rgba(0,0,0,0.1);
        }

        /* ============ SUCCESS DIALOG ============ */
        :host ::ng-deep .success-dialog .p-dialog-header {
            padding: 1.25rem 1.5rem;
            background: var(--gradient-primary);
            border-radius: 12px 12px 0 0;
        }

        .success-header {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .success-header i {
            font-size: 1.5rem;
            color: white;
        }

        .success-header span {
            font-size: 1.1rem;
            font-weight: 700;
            color: white;
        }

        :host ::ng-deep .success-dialog .p-dialog-header-close {
            color: white !important;
            background: rgba(255,255,255,0.2) !important;
            border-radius: 8px !important;
        }

        .success-content {
            padding: 1.5rem 0;
            text-align: center;
        }

        .success-message {
            font-size: 1rem;
            color: #6c757d;
            margin: 0 0 1.5rem;
        }

        .reference-box {
            display: flex;
            flex-direction: column;
            gap: 0.35rem;
            padding: 1.25rem;
            background: rgba(102, 126, 234, 0.08);
            border-radius: 12px;
            margin-bottom: 1rem;
        }

        .reference-label {
            font-size: 0.75rem;
            color: #6c757d;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .reference-number {
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--color-primary);
            font-family: 'Courier New', monospace;
        }

        .success-note {
            font-size: 0.85rem;
            color: #6c757d;
            margin: 0;
        }

        :host ::ng-deep .success-dialog .p-dialog-footer {
            padding: 1rem 1.5rem 1.5rem;
            border-top: none;
        }

        :host ::ng-deep .success-close-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 992px) {
            .form-grid {
                grid-template-columns: 1fr;
            }

            .summary-card.sticky {
                position: relative;
                top: 0;
            }

            .progress-line {
                width: 60px;
            }
        }

        @media (max-width: 768px) {
            .page-header {
                padding: 3rem 1.5rem 2rem;
            }

            .form-section {
                padding: 2rem 1rem;
            }

            .material-grid,
            .profile-grid {
                grid-template-columns: 1fr;
            }

            .progress-steps {
                flex-wrap: wrap;
            }

            .progress-line {
                display: none;
            }
        }
    `]
})
export class Print3dNewComponent implements OnInit {
    requestForm: FormGroup;
    materials: Material[] = [];
    profiles: PrintQualityProfile[] = [];
    materialOptions: any[] = [];
    profileOptions: any[] = [];
    selectedFile: File | null = null;
    selectedMaterial: Material | null = null;
    selectedProfile: PrintQualityProfile | null = null;
    selectedColor: MaterialColor | null = null;
    availableColors: MaterialColor[] = [];
    loadingColors = false;
    estimate: Print3dEstimate | null = null;
    estimating = false;
    submitting = false;
    currentStep = 1;
    showSuccessDialog = false;
    submittedReferenceNumber = '';

    private formBuilder = inject(FormBuilder);
    private printingApiService = inject(PrintingApiService);
    private authService = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.requestForm = this.formBuilder.group({
            materialId: ['', Validators.required],
            profileId: [''],
            comments: ['']
        });
    }

    ngOnInit() {
        this.loadMaterials();
        this.loadProfiles();
        this.prefillDesignFromQuery();
    }

    private prefillDesignFromQuery() {
        const designId = this.route.snapshot.queryParamMap.get('designId');
        if (!designId) return;

        this.printingApiService.getDesignDetail(designId).subscribe({
            next: (design) => {
                const primaryFile = (design.files || []).find(f => f.isPrimary) || design.files?.[0];
                if (!primaryFile?.fileUrl) return;
                this.loadFileFromUrl(primaryFile.fileUrl, primaryFile.fileName || 'design.stl');
            }
        });
    }

    private loadFileFromUrl(rawUrl: string, fileName: string) {
        const fileUrl = rawUrl.startsWith('http')
            ? rawUrl
            : `${(environment.apiUrl || '').replace('/api/v1', '')}${rawUrl.startsWith('/') ? rawUrl : '/' + rawUrl}`;

        fetch(fileUrl)
            .then(r => {
                if (!r.ok) throw new Error('Failed to fetch');
                return r.blob();
            })
            .then((blob) => {
                const inferredName = fileName || this.extractFileNameFromUrl(fileUrl) || 'design.stl';
                const file = new File([blob], inferredName, { type: blob.type || 'application/octet-stream' });
                if (!this.isValidFile(file)) {
                    this.showError('print3d.upload.invalidFile');
                    return;
                }
                this.selectedFile = file;
                this.updateProgress();
                this.tryGetEstimate();
            })
            .catch(() => {
                this.showError('messages.loadError');
            });
    }

    private extractFileNameFromUrl(url: string): string {
        const clean = url.split('?')[0].split('#')[0];
        const last = clean.substring(clean.lastIndexOf('/') + 1);
        return decodeURIComponent(last || 'design.stl');
    }

    loadMaterials() {
        this.printingApiService.getMaterials().subscribe({
            next: (materials) => {
                this.materials = materials.filter(m => m.isActive);
                const lang = this.languageService.language;
                this.materialOptions = this.materials.map(m => ({
                    id: m.id,
                    name: lang === 'ar' && m.nameAr ? m.nameAr : m.nameEn,
                    material: m
                }));
            }
        });
    }

    loadProfiles() {
        this.printingApiService.getProfiles().subscribe({
            next: (profiles) => {
                this.profiles = profiles.filter(p => p.isActive);
                const lang = this.languageService.language;
                this.profileOptions = this.profiles.map(p => ({
                    id: p.id,
                    name: lang === 'ar' && p.nameAr ? p.nameAr : p.nameEn,
                    profile: p
                }));
            }
        });
    }

    onDrop(event: DragEvent) {
        event.preventDefault();
        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            const file = files[0];
            if (this.isValidFile(file)) {
                this.selectedFile = file;
                this.updateProgress();
                this.tryGetEstimate();
            }
        }
    }

    onDragOver(event: DragEvent) {
        event.preventDefault();
    }

    isValidFile(file: File): boolean {
        const fileName = file.name.toLowerCase();
        if (!fileName.endsWith('.stl')) {
            return false;
        }
        const maxSize = 100 * 1024 * 1024; // 100 MB
        if (file.size > maxSize) {
            return false;
        }
        return true;
    }

    onFileSelect(event: any) {
        this.selectedFile = event.files[0];
        this.updateProgress();
        this.tryGetEstimate();
    }

    onFileRemove() {
        this.selectedFile = null;
        this.estimate = null;
        this.updateProgress();
    }

    selectMaterial(option: any) {
        this.requestForm.patchValue({ materialId: option.id });
        this.selectedMaterial = option.material;
        this.selectedColor = null;
        this.loadMaterialColors(option.id);
        this.updateProgress();
    }

    loadMaterialColors(materialId: string) {
        this.loadingColors = true;
        this.availableColors = [];
        this.printingApiService.getMaterialColors(materialId).subscribe({
            next: (colors) => {
                this.availableColors = colors.filter(c => c.isActive);
                this.loadingColors = false;
            },
            error: () => {
                this.loadingColors = false;
                this.availableColors = [];
            }
        });
    }

    selectColor(color: MaterialColor) {
        if (this.selectedColor?.id === color.id) {
            this.selectedColor = null;
        } else {
            this.selectedColor = color;
        }
    }

    getColorName(color: MaterialColor): string {
        return this.languageService.language === 'ar' && color.nameAr ? color.nameAr : color.nameEn;
    }

    getSelectedColorName(): string {
        if (!this.selectedColor) return '';
        return this.getColorName(this.selectedColor);
    }

    selectProfile(option: any) {
        if (this.selectedProfile?.id === option.id) {
            this.selectedProfile = null;
            this.requestForm.patchValue({ profileId: '' });
        } else {
            this.selectedProfile = option.profile;
            this.requestForm.patchValue({ profileId: option.id });
        }
    }

    onMaterialChange() {
        const materialId = this.requestForm.get('materialId')?.value;
        this.selectedMaterial = this.materials.find(m => m.id === materialId) || null;
        this.updateProgress();
        this.tryGetEstimate();
    }

    onProfileChange() {
        const profileId = this.requestForm.get('profileId')?.value;
        this.selectedProfile = this.profiles.find(p => p.id === profileId) || null;
        this.tryGetEstimate();
    }

    updateProgress() {
        if (this.selectedFile && this.selectedMaterial) {
            this.currentStep = 3;
        } else if (this.selectedFile) {
            this.currentStep = 2;
        } else {
            this.currentStep = 1;
        }
    }

    getMaterialName(): string {
        if (!this.selectedMaterial) return '';
        return this.languageService.language === 'ar' && this.selectedMaterial.nameAr 
            ? this.selectedMaterial.nameAr 
            : this.selectedMaterial.nameEn;
    }

    getProfileName(): string {
        if (!this.selectedProfile) return '';
        return this.languageService.language === 'ar' && this.selectedProfile.nameAr 
            ? this.selectedProfile.nameAr 
            : this.selectedProfile.nameEn;
    }

    canGetEstimate(): boolean {
        return !!(this.selectedFile && this.selectedMaterial);
    }

    tryGetEstimate() {
        if (this.canGetEstimate()) {
            this.getEstimate();
        }
    }

    getEstimate() {
        if (!this.canGetEstimate()) return;

        this.estimating = true;
        const tempRequest: CreatePrint3dRequestRequest = {
            materialId: this.selectedMaterial!.id,
            profileId: this.selectedProfile?.id,
            designFile: this.selectedFile!
        };
        
        // Create request which handles design creation internally
        this.printingApiService.createRequest(tempRequest).subscribe({
            next: (request) => {
                this.printingApiService.getEstimate(request.id).subscribe({
                    next: (estimate) => {
                        this.estimate = estimate;
                        this.estimating = false;
                    },
                    error: () => {
                        this.estimating = false;
                        this.showError('print3d.estimateError');
                    }
                });
            },
            error: () => {
                this.estimating = false;
                this.showError('print3d.estimateError');
            }
        });
    }

    canSubmit(): boolean {
        if (!this.selectedFile) return false;
        if (!this.selectedMaterial) return false;
        if (!this.selectedProfile) return false;
        if (this.availableColors.length > 0 && !this.selectedColor) return false;
        if (this.submitting) return false;
        return true;
    }

    onSubmit() {
        if (!this.canSubmit()) return;
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url || '/3d-print/new' } });
            return;
        }

        this.submitting = true;
        const request: CreatePrint3dRequestRequest = {
            materialId: this.selectedMaterial!.id,
            materialColorId: this.selectedColor?.id,
            profileId: this.selectedProfile?.id,
            comments: this.requestForm.get('comments')?.value || undefined,
            designFile: this.selectedFile!
        };

        this.printingApiService.createRequest(request).subscribe({
            next: (createdRequest) => {
                this.submitting = false;
                this.submittedReferenceNumber = createdRequest.referenceNumber;
                this.showSuccessDialog = true;
            },
            error: () => {
                this.submitting = false;
                this.showError('messages.saveError');
            }
        });
    }

    goToRequests() {
        this.showSuccessDialog = false;
        this.router.navigate(['/account/designs']);
    }

    onCancel() {
        this.router.navigate(['/3d-print']);
    }

    showError(messageKey: string) {
        this.messageService.add({
            severity: 'error',
            summary: this.translateService.instant('messages.error'),
            detail: this.translateService.instant(messageKey)
        });
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }
}
