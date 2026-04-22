import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DividerModule } from 'primeng/divider';
import { BadgeModule } from 'primeng/badge';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CarouselModule } from 'primeng/carousel';
import { FileUploadModule } from 'primeng/fileupload';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { LanguageService } from '../../../shared/services/language.service';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { Material, Print3dMaterial, Print3dProfile, PrintQualityProfile } from '../../../shared/models/printing.model';
import { LearnMoreDialogComponent } from '../shared/learn-more-dialog/learn-more-dialog.component';
import { Design3dViewerComponent } from '../../../shared/components/design-3d-viewer/design-3d-viewer.component';

@Component({
    selector: 'app-print3d-landing',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        ButtonModule,
        CardModule,
        TagModule,
        ProgressSpinnerModule,
        DividerModule,
        BadgeModule,
        InputNumberModule,
        SelectModule,
        CarouselModule,
        FileUploadModule,
        TextareaModule,
        ToastModule,
        LearnMoreDialogComponent,
        Design3dViewerComponent
    ],
    providers: [MessageService],
    template: `
        <div class="print3d-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-floating-shapes">
                    <div class="shape shape-1"></div>
                    <div class="shape shape-2"></div>
                    <div class="shape shape-3"></div>
                    <div class="shape shape-cube"></div>
                </div>
                <div class="hero-content">
                    <span class="hero-badge animate-fade-in">
                        <i class="pi pi-cog"></i>
                        {{ languageService.language === 'ar' ? 'التصنيع الإضافي' : 'Additive Manufacturing' }}
                    </span>
                    <h1 class="hero-title animate-fade-in-delay">
                        {{ languageService.language === 'ar' ? 'الطباعة الصناعية ثلاثية الأبعاد' : 'Industrial 3D Printing' }}
                    </h1>
                    <p class="hero-description animate-fade-in-delay-2">
                        {{ languageService.language === 'ar'
                            ? 'تصنيع إضافي احترافي للنماذج الهندسية والأجزاء الوظيفية ومكونات الإنتاج.'
                            : 'Professional additive manufacturing for engineering prototypes, functional parts, and production components.' }}
                    </p>
                    <div class="hero-cta animate-fade-in-delay-3">
                        <p-button
                            [label]="languageService.language === 'ar' ? 'إرسال طلب هندسي' : 'Submit Engineering Request'"
                            icon="pi pi-send"
                            iconPos="right"
                            routerLink="/3d-print/new"
                            styleClass="hero-button-primary">
                        </p-button>
                        <p-button
                            [label]="'home.learnMore.learnMore' | translate"
                            icon="pi pi-play"
                            [outlined]="true"
                            (onClick)="showLearnMoreDialog = true"
                            styleClass="hero-button-secondary">
                        </p-button>
                        <p-button
                            [label]="'print3d.customPrintContact' | translate"
                            icon="pi pi-envelope"
                            iconPos="right"
                            routerLink="/contact"
                            [outlined]="true"
                            styleClass="hero-button-secondary">
                        </p-button>
                    </div>
                </div>
                <!-- 3D Printer Animation - Box Style -->
                <div class="printer-animation">
                    <div class="printer-box">
                        <!-- Printer Frame -->
                        <div class="frame-top"></div>
                        <div class="frame-left"></div>
                        <div class="frame-right"></div>
                        <div class="frame-back"></div>

                        <!-- X-Axis Rail -->
                        <div class="x-rail">
                            <!-- Print Head Carriage -->
                            <div class="carriage">
                                <div class="carriage-body"></div>
                                <div class="hotend-block"></div>
                                <div class="heatsink"></div>
                                <div class="nozzle">
                                    <div class="nozzle-tip"></div>
                                    <div class="molten-plastic"></div>
                                </div>
                                <div class="filament-in"></div>
                            </div>
                        </div>

                        <!-- Z Rods -->
                        <div class="z-rod z-rod-left"></div>
                        <div class="z-rod z-rod-right"></div>

                        <!-- Build Plate -->
                        <div class="bed-assembly">
                            <div class="heated-bed">
                                <div class="bed-surface"></div>
                            </div>
                        </div>

                        <!-- Drawing Canvas -->
                        <div class="print-canvas">
                            <svg class="draw-svg" viewBox="0 0 100 100">
                                <path class="draw-path" d="M15,85 L15,35 L50,10 L85,35 L85,85 L50,65 Z" />
                            </svg>
                        </div>

                        <!-- Control Box -->
                        <div class="control-box">
                            <div class="screen"></div>
                            <div class="led-purple"></div>
                            <div class="led-blue"></div>
                        </div>

                        <!-- Spool Holder -->
                        <div class="spool-holder">
                            <div class="spool">
                                <div class="spool-inner"></div>
                            </div>
                        </div>

                        <!-- Animated Filament Line from Spool to Nozzle -->
                        <svg class="filament-svg" viewBox="0 0 220 240">
                            <defs>
                                <linearGradient id="filamentGrad" x1="0%" y1="0%" x2="100%" y2="100%">
                                    <stop offset="0%" style="stop-color:#4facfe;stop-opacity:1" />
                                    <stop offset="50%" style="stop-color:#667eea;stop-opacity:1" />
                                    <stop offset="100%" style="stop-color:#4facfe;stop-opacity:1" />
                                </linearGradient>
                                <!-- Animated gradient for flow effect -->
                                <linearGradient id="flowGrad" x1="0%" y1="0%" x2="100%" y2="0%">
                                    <stop offset="0%" style="stop-color:#4facfe;stop-opacity:0.3">
                                        <animate attributeName="offset" values="0;1;0" dur="1s" repeatCount="indefinite"/>
                                    </stop>
                                    <stop offset="50%" style="stop-color:#00f2fe;stop-opacity:1">
                                        <animate attributeName="offset" values="0.5;1.5;0.5" dur="1s" repeatCount="indefinite"/>
                                    </stop>
                                    <stop offset="100%" style="stop-color:#4facfe;stop-opacity:0.3">
                                        <animate attributeName="offset" values="1;2;1" dur="1s" repeatCount="indefinite"/>
                                    </stop>
                                </linearGradient>
                            </defs>
                            <!-- Spool center: ~176, 24. Filament-in positions follow carriage -->
                            <!-- Path keyframes synced with nozzle movement -->
                            <path class="filament-wire" fill="none" stroke="url(#filamentGrad)" stroke-width="3" stroke-linecap="round">
                                <animate
                                    attributeName="d"
                                    dur="6s"
                                    repeatCount="indefinite"
                                    calcMode="linear"
                                    values="
                                        M176,24 C140,24 80,50 58,100;
                                        M176,24 C140,24 70,30 58,55;
                                        M176,24 C150,24 130,25 110,33;
                                        M176,24 C170,30 165,40 163,55;
                                        M176,24 C175,50 170,80 163,100;
                                        M176,24 C160,40 130,60 110,82;
                                        M176,24 C140,24 80,50 58,100
                                    "
                                    keyTimes="0;0.19;0.35;0.51;0.70;0.85;1"
                                />
                            </path>
                            <!-- Glow effect layer -->
                            <path class="filament-glow" fill="none" stroke="#4facfe" stroke-width="6" stroke-linecap="round" opacity="0.3">
                                <animate
                                    attributeName="d"
                                    dur="6s"
                                    repeatCount="indefinite"
                                    calcMode="linear"
                                    values="
                                        M176,24 C140,24 80,50 58,100;
                                        M176,24 C140,24 70,30 58,55;
                                        M176,24 C150,24 130,25 110,33;
                                        M176,24 C170,30 165,40 163,55;
                                        M176,24 C175,50 170,80 163,100;
                                        M176,24 C160,40 130,60 110,82;
                                        M176,24 C140,24 80,50 58,100
                                    "
                                    keyTimes="0;0.19;0.35;0.51;0.70;0.85;1"
                                />
                            </path>
                        </svg>
                    </div>
                </div>

                <button type="button" class="hero-scroll-indicator" (click)="scrollToNextSection()" [attr.aria-label]="languageService.language === 'ar' ? 'انتقل للقسم التالي' : 'Scroll to next section'">
                    <i class="pi pi-chevron-down"></i>
                </button>
            </section>

            <!-- Statistics Section -->
            <section id="next-section" class="statistics-section">
                <div class="container">
                    <div class="stats-grid">
                        <div class="stat-card" *ngFor="let stat of statistics; let i = index" [style.animation-delay]="(i * 0.1) + 's'">
                            <div class="stat-icon-wrapper">
                                <i [class]="stat.icon"></i>
                            </div>
                            <div class="stat-info">
                                <div class="stat-value">{{ stat.value }}+</div>
                                <div class="stat-label">{{ stat.label }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Industrial Capabilities Section -->
            <section class="capabilities-section">
                <div class="container">
                    <div class="section-header compact">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'القدرات التصنيعية' : 'Manufacturing Capabilities' }}</span>
                        <h2 class="section-title">
                            {{ languageService.language === 'ar' ? 'قدرات الطباعة الصناعية ثلاثية الأبعاد' : 'Industrial 3D Printing Capabilities' }}
                        </h2>
                    </div>
                    <div class="capabilities-grid">
                        <div class="capability-item" *ngFor="let cap of capabilities">
                            <i class="pi pi-check-circle"></i>
                            <span>{{ cap }}</span>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Engineering Workflow -->
            <section class="process-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'سير العمل الهندسي' : 'Engineering Workflow' }}</span>
                        <h2 class="section-title">
                            {{ languageService.language === 'ar' ? 'سير العمل الهندسي' : 'Engineering Workflow' }}
                        </h2>
                    </div>
                    <div class="process-timeline">
                        <div class="timeline-connector"></div>
                        <div class="process-step" *ngFor="let step of processSteps; let i = index" [style.animation-delay]="(i * 0.15) + 's'">
                            <div class="step-number-wrapper">
                                <span class="step-number">{{ i + 1 }}</span>
                                <div class="step-pulse"></div>
                            </div>
                            <div class="step-content">
                                <div class="step-icon-wrapper">
                                    <i [class]="step.icon"></i>
                                </div>
                                <h3 class="step-title">{{ step.title }}</h3>
                                <p class="step-description">{{ step.description }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Unified Configuration Workflow -->
            <section class="workflow-section" id="workflow-section">
                <div class="container">
                    <!-- Workflow Header -->
                    <div class="workflow-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'إعداد الطباعة' : 'Print Configuration' }}</span>
                        <h2 class="section-title">{{ languageService.language === 'ar' ? 'إعداد طلب الطباعة' : 'Configure Your Print Request' }}</h2>
                        <p class="section-subtitle">{{ languageService.language === 'ar' ? 'اتبع الخطوات لإعداد طلب الطباعة ثلاثية الأبعاد' : 'Follow the steps to configure your 3D print request' }}</p>
                    </div>

                    <!-- Step Progress Indicator -->
                    <div class="workflow-progress">
                        <div class="progress-step" [class.completed]="selectedMaterialId" [class.active]="!selectedMaterialId">
                            <div class="step-number">
                                <i class="pi pi-check" *ngIf="selectedMaterialId"></i>
                                <span *ngIf="!selectedMaterialId">1</span>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'المادة' : 'Material' }}</span>
                        </div>
                        <div class="progress-connector" [class.completed]="selectedMaterialId"></div>
                        <div class="progress-step" [class.completed]="selectedMaterialId && (selectedColorId || !materialHasColors)" [class.active]="selectedMaterialId && !selectedColorId && materialHasColors">
                            <div class="step-number">
                                <i class="pi pi-check" *ngIf="selectedMaterialId && (selectedColorId || !materialHasColors)"></i>
                                <span *ngIf="!(selectedMaterialId && (selectedColorId || !materialHasColors))">2</span>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'اللون' : 'Color' }}</span>
                        </div>
                        <div class="progress-connector" [class.completed]="selectedMaterialId && (selectedColorId || !materialHasColors)"></div>
                        <div class="progress-step" [class.completed]="selectedProfileId" [class.active]="selectedMaterialId && (selectedColorId || !materialHasColors) && !selectedProfileId">
                            <div class="step-number">
                                <i class="pi pi-check" *ngIf="selectedProfileId"></i>
                                <span *ngIf="!selectedProfileId">3</span>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'الجودة' : 'Profile' }}</span>
                        </div>
                        <div class="progress-connector" [class.completed]="selectedProfileId"></div>
                        <div class="progress-step" [class.completed]="uploadedFile" [class.active]="selectedProfileId && !uploadedFile">
                            <div class="step-number">
                                <i class="pi pi-check" *ngIf="uploadedFile"></i>
                                <span *ngIf="!uploadedFile">4</span>
                            </div>
                            <span class="step-label">{{ languageService.language === 'ar' ? 'الملف' : 'Upload' }}</span>
                        </div>
                    </div>

                    <!-- Unified Workflow Container -->
                    <div class="workflow-container">
                        <!-- Step 1: Material Selection -->
                        <div class="workflow-step" id="step-material">
                            <div class="step-header">
                                <div class="step-indicator">
                                    <span class="step-num">1</span>
                                </div>
                                <div class="step-title-group">
                                    <h3 class="step-title">{{ languageService.language === 'ar' ? 'اختر المادة' : 'Select Material' }}</h3>
                                    <p class="step-desc">{{ languageService.language === 'ar' ? 'اختر المادة المناسبة لمتطلبات تطبيقك' : 'Choose the material that fits your application requirements' }}</p>
                                </div>
                                <div class="step-status" *ngIf="selectedMaterialId">
                                    <i class="pi pi-check-circle"></i>
                                    <span>{{ getSelectedMaterialName() }}</span>
                                </div>
                            </div>

                            <div *ngIf="loading" class="loading-container">
                                <div class="loading-spinner">
                                    <p-progressSpinner strokeWidth="3"></p-progressSpinner>
                                    <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                                </div>
                            </div>

                            <div class="materials-grid" *ngIf="!loading && filteredMaterials.length > 0">
                                <div class="material-card" 
                                     *ngFor="let material of filteredMaterials; let i = index" 
                                     [style.animation-delay]="(i * 0.03) + 's'"
                                     [class.selected]="selectedMaterialId === material.id"
                                     (click)="selectMaterial(material.id)">
                                    <div class="material-color-strip" [style.background]="material.availableColors?.[0]?.hexCode || 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'"></div>
                                    <div class="material-content">
                                        <div class="material-header">
                                            <h3 class="material-name">{{ getMaterialName(material) }}</h3>
                                            <span class="material-price-compact">{{ formatCurrency(material.pricePerGram) }}/{{ languageService.language === 'ar' ? 'جم' : 'g' }}</span>
                                        </div>
                                        <p class="material-use-case">{{ getMaterialUseCase(material) }}</p>
                                        <div class="material-specs">
                                            <div class="spec-item" *ngIf="material.availableColors?.length">
                                                <i class="pi pi-palette"></i>
                                                <span>{{ material.availableColors.length }} {{ languageService.language === 'ar' ? 'ألوان' : 'colors' }}</span>
                                            </div>
                                            <div class="spec-item">
                                                <i class="pi pi-verified"></i>
                                                <span>{{ languageService.language === 'ar' ? 'مادة هندسية' : 'Engineering Grade' }}</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div *ngIf="!loading && filteredMaterials.length === 0" class="empty-state">
                                <i class="pi pi-inbox"></i>
                                <p>{{ languageService.language === 'ar' ? 'لا توجد مواد متاحة حالياً' : 'No materials available' }}</p>
                            </div>
                        </div>

                        <!-- Step Divider -->
                        <div class="step-divider"></div>

                        <!-- Step 2: Color Selection -->
                        <div class="workflow-step" id="step-color">
                            <div class="step-header">
                                <div class="step-indicator">
                                    <span class="step-num">2</span>
                                </div>
                                <div class="step-title-group">
                                    <h3 class="step-title">{{ languageService.language === 'ar' ? 'اختر اللون' : 'Select Color' }}</h3>
                                    <p class="step-desc">{{ languageService.language === 'ar' ? 'اختر اللون المفضل للمادة المختارة' : 'Choose your preferred color for the selected material' }}</p>
                                </div>
                                <div class="step-status" *ngIf="selectedColorId">
                                    <span class="color-dot-status" [style.background]="getSelectedColorHex()"></span>
                                    <span>{{ getSelectedColorName() }}</span>
                                </div>
                            </div>

                            <div class="color-selection-content">
                                <!-- Hint when no material selected -->
                                <div class="select-material-hint" *ngIf="!selectedMaterialId">
                                    <i class="pi pi-arrow-up"></i>
                                    <p>{{ languageService.language === 'ar' 
                                        ? 'اختر مادة أولاً لرؤية الألوان المتاحة' 
                                        : 'Select a material first to see available colors' }}</p>
                                </div>
                                
                                <!-- Loading skeleton -->
                                <div class="colors-skeleton" *ngIf="selectedMaterialId && colorsLoading">
                                    <div class="skeleton-swatch" *ngFor="let i of [1,2,3,4,5,6]">
                                        <div class="skeleton-circle"></div>
                                        <div class="skeleton-text"></div>
                                    </div>
                                </div>
                                
                                <!-- Colors grid -->
                                <div class="colors-grid" *ngIf="selectedMaterialId && !colorsLoading && materialHasColors && availableColors.length > 0">
                                    <div class="color-swatch" 
                                         *ngFor="let color of availableColors"
                                         [class.selected]="selectedColorId === color.id"
                                         [style.--swatch-color]="color.hexCode"
                                         (click)="selectColor(color.id)"
                                         [title]="languageService.language === 'ar' ? color.nameAr : color.nameEn">
                                        <div class="swatch-circle" [style.background]="color.hexCode">
                                            <i class="pi pi-check swatch-check" *ngIf="selectedColorId === color.id"></i>
                                        </div>
                                        <span class="swatch-name">{{ languageService.language === 'ar' ? color.nameAr : color.nameEn }}</span>
                                    </div>
                                </div>

                                <!-- No colors available for selected material -->
                                <div class="no-colors-message" *ngIf="selectedMaterialId && !colorsLoading && (!materialHasColors || availableColors.length === 0)">
                                    <i class="pi pi-info-circle"></i>
                                    <p>{{ languageService.language === 'ar' 
                                        ? 'لا توجد ألوان متاحة لهذه المادة. سيتم تحديد اللون أثناء المراجعة الهندسية.' 
                                        : 'No colors available for this material. Color will be determined during engineering review.' }}</p>
                                </div>
                            </div>
                        </div>

                        <!-- Step Divider -->
                        <div class="step-divider"></div>

                        <!-- Step 3: Production Profile -->
                        <div class="workflow-step" id="step-profile">
                            <div class="step-header">
                                <div class="step-indicator">
                                    <span class="step-num">3</span>
                                </div>
                                <div class="step-title-group">
                                    <h3 class="step-title">{{ languageService.language === 'ar' ? 'اختر جودة الطباعة' : 'Select Print Quality' }}</h3>
                                    <p class="step-desc">{{ languageService.language === 'ar' ? 'اختر ملف الجودة المناسب لمشروعك' : 'Choose the quality profile that suits your project' }}</p>
                                </div>
                                <div class="step-status" *ngIf="selectedProfileId">
                                    <i class="pi pi-check-circle"></i>
                                    <span>{{ getSelectedProfileName() }}</span>
                                </div>
                            </div>

                            <div class="profiles-grid" *ngIf="!loading && displayProfiles.length > 0">
                                <div class="profile-card" 
                                     *ngFor="let profile of displayProfiles; let i = index"
                                     [class.selected]="selectedProfileId === profile.id"
                                     (click)="selectProfile(profile.id)">
                                    <div class="profile-icon">
                                        <i [class]="getProfileIcon(profile)"></i>
                                    </div>
                                    <h3 class="profile-name">{{ getProfileDisplayName(profile) }}</h3>
                                    <p class="profile-description">{{ getProfileDescription(profile) }}</p>
                                    <div class="profile-specs">
                                        <div class="spec-row">
                                            <span class="spec-label">
                                                <i class="pi pi-sliders-h"></i>
                                                {{ languageService.language === 'ar' ? 'ارتفاع الطبقة' : 'Layer Height' }}
                                            </span>
                                            <span class="spec-value">{{ profile.layerHeightMm }}mm</span>
                                        </div>
                                        <div class="spec-row">
                                            <span class="spec-label">
                                                <i class="pi pi-bolt"></i>
                                                {{ languageService.language === 'ar' ? 'السرعة' : 'Speed' }}
                                            </span>
                                            <span class="spec-value">{{ profile.speedCategory }}</span>
                                        </div>
                                        <div class="spec-row">
                                            <span class="spec-label">
                                                <i class="pi pi-percentage"></i>
                                                {{ languageService.language === 'ar' ? 'معامل السعر' : 'Price Factor' }}
                                            </span>
                                            <span class="spec-value">{{ profile.priceMultiplier }}x</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Step Divider -->
                        <div class="step-divider"></div>

                        <!-- Step 4: Upload STL -->
                        <div class="workflow-step" id="step-upload">
                            <div class="step-header">
                                <div class="step-indicator">
                                    <span class="step-num">4</span>
                                </div>
                                <div class="step-title-group">
                                    <h3 class="step-title">{{ languageService.language === 'ar' ? 'ارفع ملف STL' : 'Upload STL File' }}</h3>
                                    <p class="step-desc">{{ languageService.language === 'ar' ? 'ارفع نموذجك ثلاثي الأبعاد للطباعة' : 'Upload your 3D model for production' }}</p>
                                </div>
                                <div class="step-status" *ngIf="uploadedFile">
                                    <i class="pi pi-check-circle"></i>
                                    <span>{{ uploadedFile.name }}</span>
                                </div>
                            </div>

                            <div class="upload-area">
                                <div class="upload-card" 
                                     [class.has-file]="uploadedFile"
                                     [class.drag-over]="isDragOver"
                                     (dragover)="onDragOver($event)"
                                     (dragleave)="onDragLeave($event)"
                                     (drop)="onDrop($event)">
                                    
                                    <div class="upload-content" *ngIf="!uploadedFile">
                                        <div class="upload-icon">
                                            <i class="pi pi-cloud-upload"></i>
                                        </div>
                                        <h3 class="upload-title">
                                            {{ languageService.language === 'ar' ? 'اسحب ملف STL هنا' : 'Drag & Drop STL File Here' }}
                                        </h3>
                                        <p class="upload-hint">
                                            {{ languageService.language === 'ar' ? 'أو انقر للاختيار' : 'or click to browse' }}
                                        </p>
                                        <input 
                                            type="file" 
                                            #fileInput
                                            accept=".stl"
                                            (change)="onFileSelected($event)"
                                            class="file-input-hidden">
                                        <p-button
                                            [label]="languageService.language === 'ar' ? 'اختر ملف' : 'Browse Files'"
                                            icon="pi pi-folder-open"
                                            [outlined]="true"
                                            (onClick)="fileInput.click()"
                                            styleClass="upload-browse-btn">
                                        </p-button>
                                        <p class="upload-formats">
                                            {{ languageService.language === 'ar' ? 'ملفات STL فقط (الحد الأقصى 50MB)' : 'STL files only (max 50MB)' }}
                                        </p>
                                    </div>

                                    <div class="uploaded-file" *ngIf="uploadedFile">
                                        <div class="file-icon">
                                            <i class="pi pi-file"></i>
                                        </div>
                                        <div class="file-info">
                                            <span class="file-name">{{ uploadedFile.name }}</span>
                                            <span class="file-size">{{ formatFileSize(uploadedFile.size) }}</span>
                                        </div>
                                        <button type="button" class="review-file-btn" (click)="openReview()">
                                            <i class="pi pi-eye"></i>
                                            {{ languageService.language === 'ar' ? 'معاينة' : 'Review' }}
                                        </button>
                                        <button type="button" class="remove-file-btn" (click)="removeFile()">
                                            <i class="pi pi-times"></i>
                                        </button>
                                    </div>

                                    <p class="upload-error" *ngIf="fileError">
                                        <i class="pi pi-exclamation-triangle"></i>
                                        {{ fileError }}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Submit Request Section -->
            <section class="submit-section" id="submit-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ languageService.language === 'ar' ? 'إرسال الطلب' : 'Submit Request' }}</span>
                        <h2 class="section-title">
                            {{ languageService.language === 'ar' ? 'طلب الطباعة' : 'Print Request' }}
                        </h2>
                    </div>

                    <div class="submit-wrapper">
                        <div class="submit-card">
                            <!-- Request Summary -->
                            <div class="request-summary">
                                <h3 class="summary-title">
                                    <i class="pi pi-list-check"></i>
                                    {{ languageService.language === 'ar' ? 'ملخص الطلب' : 'Request Summary' }}
                                </h3>
                                
                                <div class="summary-checklist">
                                    <div class="checklist-item" [class.completed]="selectedMaterialId">
                                        <i [class]="selectedMaterialId ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
                                        <span class="checklist-label">{{ languageService.language === 'ar' ? 'المادة' : 'Material' }}</span>
                                        <span class="checklist-value" *ngIf="selectedMaterialId">{{ getSelectedMaterialName() }}</span>
                                        <span class="checklist-pending" *ngIf="!selectedMaterialId">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                    </div>
                                    
                                    <div class="checklist-item" *ngIf="selectedMaterialId && materialHasColors" [class.completed]="selectedColorId">
                                        <i [class]="selectedColorId ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
                                        <span class="checklist-label">{{ languageService.language === 'ar' ? 'اللون' : 'Color' }}</span>
                                        <span class="checklist-value color-value" *ngIf="selectedColorId">
                                            <span class="color-dot" [style.background]="getSelectedColorHex()"></span>
                                            {{ getSelectedColorName() }}
                                        </span>
                                        <span class="checklist-pending" *ngIf="!selectedColorId">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                    </div>
                                    
                                    <div class="checklist-item" *ngIf="selectedMaterialId && !materialHasColors" [class.completed]="true">
                                        <i class="pi pi-check-circle"></i>
                                        <span class="checklist-label">{{ languageService.language === 'ar' ? 'اللون' : 'Color' }}</span>
                                        <span class="checklist-value optional-value">{{ languageService.language === 'ar' ? 'يُحدد لاحقاً' : 'To be determined' }}</span>
                                    </div>
                                    
                                    <div class="checklist-item" [class.completed]="selectedProfileId">
                                        <i [class]="selectedProfileId ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
                                        <span class="checklist-label">{{ languageService.language === 'ar' ? 'ملف الجودة' : 'Quality Profile' }}</span>
                                        <span class="checklist-value" *ngIf="selectedProfileId">{{ getSelectedProfileName() }}</span>
                                        <span class="checklist-pending" *ngIf="!selectedProfileId">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                    </div>
                                    
                                    <div class="checklist-item" [class.completed]="uploadedFile">
                                        <i [class]="uploadedFile ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
                                        <span class="checklist-label">{{ languageService.language === 'ar' ? 'ملف STL' : 'STL File' }}</span>
                                        <span class="checklist-value" *ngIf="uploadedFile">{{ uploadedFile.name }}</span>
                                        <span class="checklist-pending" *ngIf="!uploadedFile">{{ languageService.language === 'ar' ? 'مطلوب' : 'Required' }}</span>
                                    </div>
                                </div>
                            </div>

                            <!-- Comments -->
                            <div class="comments-section">
                                <label class="comments-label">
                                    <i class="pi pi-comment"></i>
                                    {{ languageService.language === 'ar' ? 'ملاحظات هندسية (اختياري)' : 'Engineering Notes (Optional)' }}
                                </label>
                                <textarea
                                    pTextarea
                                    [(ngModel)]="requestComments"
                                    [placeholder]="languageService.language === 'ar' ? 'المواصفات التقنية، التفاوتات، أو المتطلبات الخاصة...' : 'Technical specifications, tolerances, or special requirements...'"
                                    [rows]="4"
                                    class="comments-textarea">
                                </textarea>
                            </div>

                            <!-- Disclaimer -->
                            <p class="submit-disclaimer">
                                <i class="pi pi-info-circle"></i>
                                {{ languageService.language === 'ar' 
                                    ? 'سيتم تأكيد العرض النهائي بعد المراجعة الهندسية.'
                                    : 'Final quotation will be confirmed after engineering review.' }}
                            </p>

                            <!-- Submit Button -->
                            <p-button
                                [label]="submitting 
                                    ? (languageService.language === 'ar' ? 'جاري الإرسال...' : 'Submitting...')
                                    : (languageService.language === 'ar' ? 'إرسال طلب الطباعة' : 'Submit Print Request')"
                                icon="pi pi-send"
                                iconPos="right"
                                [disabled]="!canSubmit() || submitting"
                                [loading]="submitting"
                                (onClick)="submitRequest()"
                                styleClass="submit-btn w-full">
                            </p-button>
                        </div>

                        <!-- Success Message -->
                        <div class="success-card" *ngIf="requestSubmitted">
                            <div class="success-icon">
                                <i class="pi pi-check-circle"></i>
                            </div>
                            <h3 class="success-title">
                                {{ languageService.language === 'ar' ? 'تم إرسال الطلب!' : 'Request Submitted!' }}
                            </h3>
                            <p class="success-message">
                                {{ languageService.language === 'ar' 
                                    ? 'تم استلام طلب الطباعة بنجاح. سيتواصل معك فريقنا قريباً.'
                                    : 'Your print request has been received. Our team will contact you shortly.' }}
                            </p>
                            <div class="reference-number" *ngIf="submittedRequestId">
                                <span class="ref-label">{{ languageService.language === 'ar' ? 'رقم المرجع' : 'Reference Number' }}</span>
                                <span class="ref-value">{{ submittedRequestId }}</span>
                            </div>
                            <p-button
                                [label]="languageService.language === 'ar' ? 'طلب جديد' : 'New Request'"
                                icon="pi pi-plus"
                                [outlined]="true"
                                (onClick)="resetForm()"
                                styleClass="new-request-btn">
                            </p-button>
                        </div>
                    </div>
                </div>
            </section>
            
            <p-toast></p-toast>

            <!-- Features Section -->
            <section class="features-section">
                <div class="container">
                    <div class="features-grid">
                        <div class="feature-card" *ngFor="let feature of features">
                            <div class="feature-icon">
                                <i [class]="feature.icon"></i>
                            </div>
                            <h3 class="feature-title">{{ feature.title }}</h3>
                            <p class="feature-description">{{ feature.description }}</p>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Final CTA Section -->
            <section class="cta-section">
                <div class="cta-bg"></div>
                <div class="cta-particles">
                    <div class="particle" *ngFor="let i of [1,2,3,4,5,6,7,8]"></div>
                </div>
                <div class="container">
                    <div class="cta-content">
                        <h2 class="cta-title">
                            {{ languageService.language === 'ar'
                                ? 'جاهز لبدء مشروعك الهندسي؟'
                                : 'Ready to Start Your Engineering Project?' }}
                        </h2>
                        <p class="cta-description">
                            {{ languageService.language === 'ar'
                                ? 'قدّم متطلباتك التقنية للتصنيع الإضافي الاحترافي'
                                : 'Submit your technical requirements for professional additive manufacturing' }}
                        </p>
                        <div class="cta-buttons">
                            <p-button
                                [label]="languageService.language === 'ar' ? 'إرسال طلب' : 'Submit Request'"
                                icon="pi pi-send"
                                routerLink="/3d-print/new"
                                styleClass="cta-btn-primary">
                            </p-button>
                            <p-button
                                [label]="languageService.language === 'ar' ? 'استشارة هندسية' : 'Engineering Consultation'"
                                icon="pi pi-comments"
                                [outlined]="true"
                                routerLink="/contact"
                                styleClass="cta-btn-secondary">
                            </p-button>
                        </div>
                    </div>
                </div>
            </section>
            <app-learn-more-dialog [(visible)]="showLearnMoreDialog"></app-learn-more-dialog>

            <!-- 3D Review Overlay -->
            @if (showReviewPanel && reviewModelUrl) {
                <div class="review-overlay" (click)="closeReview()">
                    <div class="review-panel" (click)="$event.stopPropagation()">
                        <div class="review-header">
                            <div class="review-meta">
                                @if (reviewColorHex) {
                                    <span class="review-swatch" [style.background]="reviewColorHex"></span>
                                }
                                <span class="review-label">{{ getSelectedMaterialName() }}</span>
                                @if (reviewColorName) {
                                    <span class="review-color-name">— {{ reviewColorName }}</span>
                                }
                            </div>
                            <button class="review-close-btn" (click)="closeReview()">
                                <i class="pi pi-times"></i>
                            </button>
                        </div>
                        <div class="review-viewer-wrap">
                            <app-design-3d-viewer
                                [modelUrl]="reviewModelUrl"
                                format="STL"
                                [modelColor]="reviewColorHex"
                                background="#f0f2f5">
                            </app-design-3d-viewer>
                        </div>
                    </div>
                </div>
            }
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-secondary: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-accent: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --color-accent: #00f2fe;
            --shadow-lg: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .print3d-page {
            width: 100%;
            overflow-x: hidden;
            background: #fafbfc;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 6rem 2rem;
            overflow: hidden;
        }

        .hero-bg-gradient {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%),
                linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px),
                linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px);
            background-size: 100% 100%, 100% 100%, 60px 60px, 60px 60px;
            animation: patternMove 30s linear infinite;
            z-index: 1;
        }

        @keyframes patternMove {
            0% { background-position: 0 0, 0 0, 0 0, 0 0; }
            100% { background-position: 0 0, 0 0, 60px 60px, 60px 60px; }
        }

        .hero-floating-shapes {
            position: absolute;
            inset: 0;
            z-index: 2;
            pointer-events: none;
        }

        .shape {
            position: absolute;
            border-radius: 50%;
            filter: blur(60px);
            opacity: 0.5;
        }

        .shape-1 {
            width: 400px;
            height: 400px;
            background: var(--gradient-primary);
            top: -100px;
            right: -100px;
            animation: float 8s ease-in-out infinite;
        }

        .shape-2 {
            width: 300px;
            height: 300px;
            background: var(--gradient-secondary);
            bottom: -50px;
            left: -50px;
            animation: float 6s ease-in-out infinite reverse;
        }

        .shape-3 {
            width: 200px;
            height: 200px;
            background: var(--gradient-accent);
            top: 50%;
            left: 10%;
            animation: float 10s ease-in-out infinite 2s;
        }

        .shape-cube {
            width: 150px;
            height: 150px;
            background: rgba(255,255,255,0.1);
            border-radius: 20px;
            top: 20%;
            right: 15%;
            transform: rotate(45deg);
            animation: rotateCube 15s linear infinite;
            filter: blur(0);
        }

        @keyframes float {
            0%, 100% { transform: translateY(0) scale(1); }
            50% { transform: translateY(-30px) scale(1.05); }
        }

        @keyframes rotateCube {
            0% { transform: rotate(45deg); }
            100% { transform: rotate(405deg); }
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
            max-width: 900px;
            margin: 0 auto;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: rgba(255,255,255,0.1);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
            border-radius: 50px;
            color: white;
            font-size: 0.875rem;
            font-weight: 500;
            margin-bottom: 2rem;
        }

        .hero-badge i {
            color: #ffd700;
        }

        .hero-title {
            font-size: clamp(2.5rem, 6vw, 4.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1.5rem;
            line-height: 1.1;
            text-shadow: 0 4px 30px rgba(0,0,0,0.3);
        }

        .hero-description {
            font-size: clamp(1rem, 2vw, 1.35rem);
            color: rgba(255,255,255,0.85);
            margin-bottom: 3rem;
            line-height: 1.7;
            max-width: 700px;
            margin-left: auto;
            margin-right: auto;
        }

        .hero-cta {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }

        :host ::ng-deep .hero-button-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 2.5rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: var(--shadow-glow) !important;
            transition: all 0.3s ease !important;
        }

        :host ::ng-deep .hero-button-primary:hover {
            transform: translateY(-3px) !important;
            box-shadow: 0 0 60px rgba(102, 126, 234, 0.6) !important;
        }

        :host ::ng-deep .hero-button-secondary {
            border-color: rgba(255,255,255,0.4) !important;
            color: white !important;
            padding: 1rem 2rem !important;
            font-size: 1.1rem !important;
            border-radius: 50px !important;
            background: rgba(255,255,255,0.05) !important;
            backdrop-filter: blur(10px) !important;
        }

        :host ::ng-deep .hero-button-secondary:hover {
            background: rgba(255,255,255,0.15) !important;
            border-color: rgba(255,255,255,0.6) !important;
        }

        .hero-scroll-indicator {
            position: absolute;
            bottom: 2rem;
            left: 50%;
            transform: translateX(-50%);
            z-index: 10;
            color: rgba(255,255,255,0.6);
            font-size: 1.5rem;
            animation: bounce 2s infinite;
            cursor: pointer;
            background: none;
            border: none;
            padding: 0.5rem;
        }

        @keyframes bounce {
            0%, 20%, 50%, 80%, 100% { transform: translateX(-50%) translateY(0); }
            40% { transform: translateX(-50%) translateY(-15px); }
            60% { transform: translateX(-50%) translateY(-7px); }
        }

        /* ============ 3D PRINTER ANIMATION - BOX STYLE ============ */
        .printer-animation {
            position: absolute;
            bottom: 60px;
            right: 6%;
            z-index: 5;
            pointer-events: none;
        }

        [dir="rtl"] .printer-animation {
            right: auto;
            left: 6%;
        }

        .printer-box {
            position: relative;
            width: 220px;
            height: 240px;
            animation: printerFloat 6s ease-in-out infinite;
        }

        @keyframes printerFloat {
            0%, 100% { transform: translateY(0); }
            50% { transform: translateY(-6px); }
        }

        /* Frame Structure */
        .frame-top {
            position: absolute;
            top: 0;
            left: 10px;
            right: 10px;
            height: 14px;
            background: linear-gradient(90deg, #2d3748 0%, #4a5568 50%, #2d3748 100%);
            border-radius: 4px 4px 0 0;
            box-shadow: 0 -3px 15px rgba(0,0,0,0.3);
        }

        .frame-left, .frame-right {
            position: absolute;
            top: 0;
            width: 14px;
            height: 220px;
            background: linear-gradient(180deg, #4a5568 0%, #2d3748 100%);
            border-radius: 4px;
        }

        .frame-left { left: 0; }
        .frame-right { right: 0; }

        .frame-back {
            position: absolute;
            top: 14px;
            left: 14px;
            right: 14px;
            height: 180px;
            background: linear-gradient(180deg, #1a1a2e 0%, #0c1445 100%);
            border: 2px solid #2d3748;
            border-top: none;
        }

        /* X-Axis Rail - synced with drawing path */
        .x-rail {
            position: absolute;
            top: 75px;
            left: 20px;
            right: 20px;
            height: 12px;
            background: linear-gradient(180deg, #718096 0%, #4a5568 100%);
            border-radius: 6px;
            box-shadow: 0 3px 10px rgba(0,0,0,0.3);
            animation: xRailSync 6s linear infinite;
            z-index: 10;
        }

        /* Path: M15,85 L15,35 L50,10 L85,35 L85,85 L50,65 Z */
        /* Nozzle tip Y = x-rail top + 58px */
        /* Canvas: top=115, height=90 → SVG Y maps to screenY = 115 + (Y/100)*90 */
        /* x-rail top = screenY - 58 */
        @keyframes xRailSync {
            0% { top: 133px; }      /* Y=85 → screenY=191.5 → rail=133 */
            19% { top: 88px; }      /* Y=35 → screenY=146.5 → rail=88 */
            35% { top: 66px; }      /* Y=10 → screenY=124 → rail=66 */
            51% { top: 88px; }      /* Y=35 → screenY=146.5 → rail=88 */
            70% { top: 133px; }     /* Y=85 → screenY=191.5 → rail=133 */
            85% { top: 115px; }     /* Y=65 → screenY=173.5 → rail=115 */
            100% { top: 133px; }    /* back to start */
        }

        /* Carriage (Print Head Assembly) - synced with drawing path */
        .carriage {
            position: absolute;
            top: -8px;
            left: 20px;
            animation: carriageSync 6s linear infinite;
        }

        /* Nozzle center X = x-rail left(20) + carriage left + 18 */
        /* Canvas: left=35, width=150 → SVG X maps to screenX = 35 + (X/100)*150 */
        /* carriage left = screenX - 38 */
        @keyframes carriageSync {
            0% { left: 20px; }      /* X=15 → screenX=57.5 → carriage=20 */
            19% { left: 20px; }     /* X=15 → still left */
            35% { left: 72px; }     /* X=50 → screenX=110 → carriage=72 */
            51% { left: 125px; }    /* X=85 → screenX=162.5 → carriage=125 */
            70% { left: 125px; }    /* X=85 → still right */
            85% { left: 72px; }     /* X=50 → back center */
            100% { left: 20px; }    /* back to start */
        }

        .carriage-body {
            width: 36px;
            height: 28px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 6px;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.5);
        }

        .hotend-block {
            position: absolute;
            top: 28px;
            left: 50%;
            transform: translateX(-50%);
            width: 20px;
            height: 16px;
            background: linear-gradient(180deg, #e53e3e 0%, #c53030 100%);
            border-radius: 3px;
            box-shadow: 0 0 12px rgba(229, 62, 62, 0.6);
        }

        .heatsink {
            position: absolute;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            width: 26px;
            height: 10px;
            background: repeating-linear-gradient(
                0deg,
                #a0aec0 0px,
                #a0aec0 2px,
                #718096 2px,
                #718096 4px
            );
            border-radius: 2px;
        }

        /* NOZZLE - Main visible part */
        .nozzle {
            position: absolute;
            top: 44px;
            left: 50%;
            transform: translateX(-50%);
            width: 14px;
            height: 20px;
            background: linear-gradient(180deg, #c53030 0%, #9b2c2c 50%, #742a2a 100%);
            clip-path: polygon(0 0, 100% 0, 70% 100%, 30% 100%);
        }

        .nozzle-tip {
            position: absolute;
            bottom: -6px;
            left: 50%;
            transform: translateX(-50%);
            width: 8px;
            height: 8px;
            background: radial-gradient(circle, #ffd700 30%, #b7791f 100%);
            border-radius: 50%;
            box-shadow:
                0 0 8px #ffd700,
                0 0 16px #ffd700,
                0 0 24px #ff8c00;
            animation: nozzleGlow 0.4s ease-in-out infinite alternate;
        }

        @keyframes nozzleGlow {
            0% {
                box-shadow: 0 0 8px #ffd700, 0 0 16px #ffd700, 0 0 24px #ff8c00;
                transform: translateX(-50%) scale(1);
            }
            100% {
                box-shadow: 0 0 12px #ffd700, 0 0 24px #ffd700, 0 0 36px #ff8c00;
                transform: translateX(-50%) scale(1.1);
            }
        }

        /* Molten Plastic coming from nozzle */
        .molten-plastic {
            position: absolute;
            bottom: -25px;
            left: 50%;
            transform: translateX(-50%);
            width: 4px;
            height: 18px;
            background: linear-gradient(180deg,
                #ff6b6b 0%,
                #4facfe 40%,
                #4facfe 100%);
            border-radius: 0 0 2px 2px;
            animation: meltFlow 0.15s linear infinite;
            box-shadow: 0 0 6px #4facfe, 0 0 10px rgba(79, 172, 254, 0.5);
        }

        @keyframes meltFlow {
            0% {
                height: 16px;
                opacity: 1;
            }
            100% {
                height: 20px;
                opacity: 0.9;
            }
        }

        .filament-in {
            position: absolute;
            top: -25px;
            left: 50%;
            transform: translateX(-50%);
            width: 3px;
            height: 28px;
            background: linear-gradient(180deg,
                #4facfe 0%,
                #667eea 25%,
                #4facfe 50%,
                #667eea 75%,
                #4facfe 100%);
            background-size: 100% 14px;
            border-radius: 2px;
            animation: filamentFeed 0.3s linear infinite;
        }

        @keyframes filamentFeed {
            0% { background-position: 0 0; }
            100% { background-position: 0 14px; }
        }

        /* Z Rods */
        .z-rod {
            position: absolute;
            top: 14px;
            width: 6px;
            height: 195px;
            background: linear-gradient(90deg, #a0aec0 0%, #718096 50%, #a0aec0 100%);
            border-radius: 3px;
        }

        .z-rod-left { left: 22px; }
        .z-rod-right { right: 22px; }

        /* Build Plate / Bed */
        .bed-assembly {
            position: absolute;
            top: 195px;
            left: 30px;
            right: 30px;
            height: 20px;
        }

        .heated-bed {
            width: 100%;
            height: 100%;
            background: linear-gradient(135deg, #2d3748 0%, #1a202c 100%);
            border-radius: 4px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.4);
            border: 2px solid #4a5568;
        }

        .bed-surface {
            position: absolute;
            inset: 3px;
            background:
                linear-gradient(90deg, rgba(79, 172, 254, 0.15) 1px, transparent 1px),
                linear-gradient(rgba(79, 172, 254, 0.15) 1px, transparent 1px),
                linear-gradient(135deg, #1a1a2e 0%, #0c1445 100%);
            background-size: 10px 10px, 10px 10px, 100% 100%;
            border-radius: 3px;
            animation: bedHeat 3s ease-in-out infinite;
        }

        @keyframes bedHeat {
            0%, 100% {
                box-shadow: inset 0 0 15px rgba(255, 107, 107, 0.2);
            }
            50% {
                box-shadow: inset 0 0 25px rgba(255, 107, 107, 0.4);
            }
        }

        /* Print Canvas - SVG Drawing - behind the nozzle (z-index: 1) */
        .print-canvas {
            position: absolute;
            top: 115px;
            left: 35px;
            width: 150px;
            height: 90px;
            pointer-events: none;
            z-index: 1;
        }

        .draw-svg {
            width: 100%;
            height: 100%;
            overflow: visible;
        }

        .draw-path {
            fill: none;
            stroke: #4facfe;
            stroke-width: 5;
            stroke-linecap: round;
            stroke-linejoin: round;
            stroke-dasharray: 266;
            stroke-dashoffset: 266;
            animation: drawShape 6s linear infinite;
            filter: drop-shadow(0 0 4px #4facfe) drop-shadow(0 0 8px #667eea);
        }

        /* Drawing appears BEHIND the nozzle - delayed by ~3% */
        /* Nozzle keyframes: 0, 19, 35, 51, 70, 85, 100 */
        /* Path keyframes:   0, 22, 38, 54, 73, 88, 100 (shifted +3%) */
        @keyframes drawShape {
            0% { stroke-dashoffset: 266; opacity: 1; }
            3% { stroke-dashoffset: 266; opacity: 1; }
            22% { stroke-dashoffset: 216; opacity: 1; }
            38% { stroke-dashoffset: 173; opacity: 1; }
            54% { stroke-dashoffset: 130; opacity: 1; }
            73% { stroke-dashoffset: 80; opacity: 1; }
            88% { stroke-dashoffset: 40; opacity: 1; }
            97% { stroke-dashoffset: 0; opacity: 1; }
            100% { stroke-dashoffset: 0; opacity: 0.3; }
        }

        /* Control Box */
        .control-box {
            position: absolute;
            top: 215px;
            left: 10px;
            right: 10px;
            height: 25px;
            background: linear-gradient(180deg, #2d3748 0%, #1a202c 100%);
            border-radius: 0 0 8px 8px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.4);
        }

        .screen {
            position: absolute;
            top: 5px;
            left: 10px;
            width: 50px;
            height: 20px;
            background: linear-gradient(135deg, #0c1445 0%, #1a1a2e 100%);
            border-radius: 3px;
            border: 1px solid #4a5568;
            overflow: hidden;
        }

        .screen::after {
            content: '';
            position: absolute;
            inset: 2px;
            background: repeating-linear-gradient(
                0deg,
                transparent 0px,
                transparent 2px,
                rgba(79, 172, 254, 0.3) 2px,
                rgba(79, 172, 254, 0.3) 4px
            );
            animation: screenFlicker 2s steps(3) infinite;
        }

        @keyframes screenFlicker {
            0%, 100% { opacity: 0.8; }
            50% { opacity: 1; }
        }

        .led-purple, .led-blue {
            position: absolute;
            top: 10px;
            width: 6px;
            height: 6px;
            border-radius: 50%;
        }

        .led-purple {
            right: 25px;
            background: #667eea;
            box-shadow: 0 0 8px #48bb78;
            animation: ledPulse 1s ease-in-out infinite;
        }

        .led-blue {
            right: 10px;
            background: #4facfe;
            box-shadow: 0 0 8px #4facfe;
            animation: ledPulse 1.5s ease-in-out infinite 0.5s;
        }

        @keyframes ledPulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.4; }
        }

        /* Spool Holder */
        .spool-holder {
            position: absolute;
            top: 5px;
            right: 25px;
            z-index: 6;
        }

        [dir="rtl"] .spool-holder {
            right: auto;
            left: 25px;
        }

        .spool {
            width: 38px;
            height: 38px;
            background: conic-gradient(
                from 0deg,
                #4facfe 0deg,
                #667eea 90deg,
                #764ba2 180deg,
                #667eea 270deg,
                #4facfe 360deg
            );
            border-radius: 50%;
            animation: spoolSpin 2s linear infinite;
            box-shadow: 0 0 15px rgba(102, 126, 234, 0.4);
        }

        @keyframes spoolSpin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .spool-inner {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 12px;
            height: 12px;
            background: #1a202c;
            border-radius: 50%;
            border: 2px solid #4a5568;
        }

        /* Filament SVG from spool to nozzle */
        .filament-svg {
            position: absolute;
            top: 0;
            left: 0;
            width: 220px;
            height: 240px;
            pointer-events: none;
            z-index: 5;
            overflow: visible;
        }

        .filament-wire {
            filter: drop-shadow(0 0 4px #4facfe);
        }

        .filament-glow {
            filter: blur(3px);
        }

        /* Responsive */
        @media (max-width: 1200px) {
            .printer-animation {
                transform: scale(0.8);
                bottom: 50px;
                right: 3%;
            }
            [dir="rtl"] .printer-animation {
                right: auto;
                left: 3%;
            }
        }

        @media (max-width: 992px) {
            .printer-animation {
                transform: scale(0.65);
                bottom: 40px;
            }
        }

        @media (max-width: 768px) {
            .printer-animation {
                display: none;
            }
        }

        /* Animations */
        .animate-fade-in { animation: fadeIn 0.8s ease-out forwards; }
        .animate-fade-in-delay { animation: fadeIn 0.8s ease-out 0.2s forwards; opacity: 0; }
        .animate-fade-in-delay-2 { animation: fadeIn 0.8s ease-out 0.4s forwards; opacity: 0; }
        .animate-fade-in-delay-3 { animation: fadeIn 0.8s ease-out 0.6s forwards; opacity: 0; }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(30px); }
            to { opacity: 1; transform: translateY(0); }
        }

        /* ============ STATISTICS SECTION ============ */
        .statistics-section {
            padding: 5rem 2rem;
            background: linear-gradient(180deg, #0c1445 0%, #1a1a2e 100%);
            margin-top: -1px;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
        }

        .stat-card {
            display: flex;
            align-items: center;
            gap: 1.25rem;
            background: rgba(255,255,255,0.05);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.1);
            padding: 1.75rem;
            border-radius: 20px;
            animation: fadeIn 0.6s ease-out backwards;
            transition: all 0.3s ease;
        }

        .stat-card:hover {
            transform: translateY(-5px);
            background: rgba(255,255,255,0.08);
            border-color: rgba(102, 126, 234, 0.5);
        }

        .stat-icon-wrapper {
            width: 60px;
            height: 60px;
            background: var(--gradient-primary);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            color: white;
            flex-shrink: 0;
        }

        .stat-info {
            flex: 1;
        }

        .stat-value {
            font-size: 2rem;
            font-weight: 800;
            color: white;
            line-height: 1;
            margin-bottom: 0.5rem;
        }

        .stat-label {
            font-size: 0.9rem;
            color: rgba(255,255,255,0.6);
        }

        /* ============ SECTION HEADER ============ */
        .section-header {
            text-align: center;
            margin-bottom: 4rem;
        }

        .section-badge {
            display: inline-block;
            padding: 0.5rem 1.25rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.875rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 1rem;
        }

        .section-title {
            font-size: clamp(1.75rem, 4vw, 2.75rem);
            font-weight: 700;
            color: #1a1a2e;
            line-height: 1.2;
        }

        .section-subtitle {
            color: #64748b;
            font-size: 1rem;
            margin-top: 0.5rem;
            max-width: 600px;
            margin-left: auto;
            margin-right: auto;
        }

        .section-header.compact {
            margin-bottom: 2rem;
        }

        /* ============ CAPABILITIES SECTION ============ */
        .capabilities-section {
            padding: 4rem 2rem;
            background: linear-gradient(135deg, #f8fafc 0%, #eef2ff 100%);
        }

        .capabilities-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 1rem 2rem;
            max-width: 900px;
            margin: 0 auto;
        }

        .capability-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem 1rem;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
            transition: all 0.2s ease;
        }

        .capability-item:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
        }

        .capability-item i {
            color: var(--color-primary);
            font-size: 1.1rem;
        }

        .capability-item span {
            color: #334155;
            font-weight: 500;
            font-size: 0.95rem;
        }

        @media (max-width: 768px) {
            .capabilities-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 480px) {
            .capabilities-grid {
                grid-template-columns: 1fr;
            }
        }

        /* ============ PROCESS SECTION ============ */
        .process-section {
            padding: 5rem 2rem;
            background: white;
        }

        .process-timeline {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
            position: relative;
        }

        .timeline-connector {
            position: absolute;
            top: 25px;
            left: 12.5%;
            right: 12.5%;
            height: 4px;
            background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-secondary) 100%);
            border-radius: 2px;
            z-index: 0;
        }

        .timeline-connector::before {
            content: '';
            position: absolute;
            inset: 0;
            background: inherit;
            filter: blur(8px);
            opacity: 0.5;
        }

        .process-step {
            text-align: center;
            position: relative;
            z-index: 1;
            animation: stepFadeIn 0.6s ease-out backwards;
        }

        @keyframes stepFadeIn {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .step-number-wrapper {
            margin-bottom: 1.5rem;
            position: relative;
            display: inline-block;
        }

        .step-number {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 50px;
            height: 50px;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50%;
            font-size: 1.25rem;
            font-weight: 700;
            box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
            position: relative;
            z-index: 2;
        }

        .step-pulse {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 50px;
            height: 50px;
            background: var(--gradient-primary);
            border-radius: 50%;
            animation: pulse 2s ease-out infinite;
            z-index: 1;
        }

        @keyframes pulse {
            0% {
                transform: translate(-50%, -50%) scale(1);
                opacity: 0.5;
            }
            100% {
                transform: translate(-50%, -50%) scale(1.8);
                opacity: 0;
            }
        }

        .step-content {
            background: white;
            padding: 2rem;
            border-radius: 20px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            border: 2px solid transparent;
        }

        .step-content:hover {
            border-color: var(--color-primary);
            transform: translateY(-8px);
            box-shadow: 0 15px 40px rgba(102, 126, 234, 0.2);
        }

        .step-icon-wrapper {
            width: 70px;
            height: 70px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
            border-radius: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.25rem;
            font-size: 2rem;
            color: var(--color-primary);
            transition: all 0.3s ease;
        }

        .step-content:hover .step-icon-wrapper {
            background: var(--gradient-primary);
            color: white;
            transform: scale(1.1) rotate(5deg);
        }

        .step-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .step-description {
            color: #6c757d;
            line-height: 1.6;
            font-size: 0.95rem;
            margin: 0;
        }


        /* ============ UNIFIED WORKFLOW SECTION ============ */
        .workflow-section {
            padding: 3rem 2rem 4rem;
            background: #f8fafc;
        }

        .workflow-header {
            text-align: center;
            margin-bottom: 2rem;
        }

        .workflow-header .section-title {
            color: #1e293b;
            margin-bottom: 0.5rem;
        }

        .workflow-header .section-subtitle {
            color: #64748b;
            font-size: 1rem;
        }

        /* Step Progress Indicator */
        .workflow-progress {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0;
            margin-bottom: 2rem;
            padding: 1.25rem 2rem;
            background: #fff;
            border-radius: 12px;
            border: 1px solid #e2e8f0;
            max-width: 700px;
            margin-left: auto;
            margin-right: auto;
        }

        .progress-step {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
            flex-shrink: 0;
        }

        .progress-step .step-number {
            width: 32px;
            height: 32px;
            border-radius: 50%;
            background: #e2e8f0;
            color: #64748b;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 0.85rem;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .progress-step.active .step-number {
            background: var(--gradient-primary);
            color: white;
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.2);
        }

        .progress-step.completed .step-number {
            background: #667eea;
            color: white;
        }

        .progress-step .step-label {
            font-size: 0.75rem;
            color: #94a3b8;
            font-weight: 500;
            white-space: nowrap;
        }

        .progress-step.active .step-label,
        .progress-step.completed .step-label {
            color: #1e293b;
        }

        .progress-connector {
            width: 40px;
            height: 2px;
            background: #e2e8f0;
            margin: 0 0.5rem;
            margin-bottom: 1.5rem;
            transition: background 0.3s ease;
        }

        .progress-connector.completed {
            background: #667eea;
        }

        /* Workflow Container */
        .workflow-container {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            overflow: hidden;
        }

        /* Workflow Steps */
        .workflow-step {
            padding: 1.5rem 2rem;
        }

        .step-header {
            display: flex;
            align-items: flex-start;
            gap: 1rem;
            margin-bottom: 1.25rem;
        }

        .step-indicator {
            flex-shrink: 0;
        }

        .step-indicator .step-num {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 28px;
            height: 28px;
            border-radius: 8px;
            background: var(--gradient-primary);
            color: white;
            font-size: 0.85rem;
            font-weight: 700;
        }

        .step-title-group {
            flex: 1;
        }

        .step-title {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1e293b;
            margin: 0 0 0.25rem 0;
        }

        .step-desc {
            font-size: 0.875rem;
            color: #64748b;
            margin: 0;
        }

        .step-status {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 0.75rem;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 6px;
            color: #667eea;
            font-size: 0.8rem;
            font-weight: 500;
            flex-shrink: 0;
        }

        .step-status i {
            font-size: 0.85rem;
        }

        .step-status .color-dot-status {
            width: 12px;
            height: 12px;
            border-radius: 4px;
            border: 2px solid rgba(0,0,0,0.1);
        }

        /* Step Divider */
        .step-divider {
            height: 1px;
            background: linear-gradient(90deg, transparent 0%, #e2e8f0 20%, #e2e8f0 80%, transparent 100%);
            margin: 0 2rem;
        }

        /* Color Selection Content */
        .color-selection-content {
            min-height: 60px;
        }

        .materials-filter {
            display: flex;
            justify-content: center;
            gap: 0.75rem;
            margin-bottom: 3rem;
            flex-wrap: wrap;
        }

        .filter-btn {
            padding: 0.75rem 1.5rem;
            border: 2px solid #e9ecef;
            background: white;
            border-radius: 50px;
            font-size: 0.95rem;
            font-weight: 500;
            color: #495057;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .filter-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .filter-btn.active {
            background: var(--gradient-primary);
            border-color: transparent;
            color: white;
        }

        .materials-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 1rem;
        }

        .material-card {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            overflow: hidden;
            animation: fadeIn 0.4s ease-out backwards;
            transition: all 0.25s ease;
            position: relative;
        }

        .material-card::before {
            content: '';
            position: absolute;
            left: 0;
            top: 0;
            bottom: 0;
            width: 4px;
            background: linear-gradient(180deg, var(--color-primary), var(--color-secondary));
            opacity: 0;
            transition: opacity 0.25s ease;
        }

        .material-card:hover {
            border-color: var(--color-primary);
            box-shadow: 0 4px 16px rgba(102, 126, 234, 0.15);
            transform: translateY(-2px);
        }

        .material-card:hover::before {
            opacity: 1;
        }

        .material-card.selected {
            border-color: var(--color-primary);
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.04) 0%, rgba(118, 75, 162, 0.04) 100%);
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.12), 0 4px 16px rgba(102, 126, 234, 0.2);
        }

        .material-card.selected::before {
            opacity: 1;
        }

        .material-color-strip {
            height: 4px;
        }

        .material-content {
            padding: 1.25rem 1.5rem;
        }

        .material-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 0.5rem;
        }

        .material-name {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1e293b;
            margin: 0;
        }

        .material-price-compact {
            font-size: 0.85rem;
            font-weight: 700;
            color: var(--color-primary);
            background: rgba(102, 126, 234, 0.1);
            padding: 0.25rem 0.6rem;
            border-radius: 6px;
            white-space: nowrap;
        }

        .material-use-case {
            color: #64748b;
            font-size: 0.875rem;
            line-height: 1.5;
            margin: 0 0 0.875rem 0;
        }

        .material-type-badge {
            padding: 0.3rem 0.75rem;
            background: rgba(102, 126, 234, 0.08);
            color: var(--color-primary);
            border-radius: 6px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .material-specs {
            display: flex;
            gap: 0.875rem;
            flex-wrap: wrap;
            padding-top: 0.75rem;
            border-top: 1px solid #f1f5f9;
        }

        .spec-item {
            display: inline-flex;
            align-items: center;
            gap: 0.4rem;
            color: #64748b;
            font-size: 0.8rem;
            background: #f8fafc;
            padding: 0.3rem 0.6rem;
            border-radius: 6px;
        }

        .spec-item i {
            color: var(--color-primary);
            font-size: 0.75rem;
        }

        .material-price-section {
            padding-top: 1rem;
            border-top: 1px solid #e9ecef;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .price-label {
            color: #6c757d;
            font-size: 0.9rem;
        }

        .price-value {
            font-size: 1.25rem;
            font-weight: 700;
            color: var(--color-primary);
        }

        /* ============ COLOR SELECTION ============ */

        .color-section-title {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 1rem;
            font-weight: 600;
            color: #1e293b;
            margin: 0;
        }

        .color-section-title i {
            color: var(--color-primary);
            font-size: 1rem;
        }

        .colors-grid {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            justify-content: flex-start;
        }

        .color-swatch {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.35rem;
            cursor: pointer;
            transition: all 0.2s ease;
            padding: 0.5rem;
            border-radius: 8px;
            background: #f8fafc;
            border: 1px solid transparent;
        }

        .color-swatch:hover {
            background: #f1f5f9;
            border-color: #e2e8f0;
        }

        .swatch-circle {
            width: 32px;
            height: 32px;
            border-radius: 8px;
            border: 2px solid #e2e8f0;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s ease;
            box-shadow: 0 1px 3px rgba(0,0,0,0.08);
            position: relative;
        }

        .color-swatch:hover .swatch-circle {
            border-color: var(--color-primary);
            box-shadow: 0 2px 8px rgba(102, 126, 234, 0.25);
        }

        .color-swatch.selected {
            background: rgba(102, 126, 234, 0.08);
            border-color: var(--color-primary);
        }

        .color-swatch.selected .swatch-circle {
            border-color: var(--color-primary);
            border-width: 3px;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.15);
        }

        .swatch-check {
            color: white;
            font-size: 0.8rem;
            font-weight: bold;
            text-shadow: 0 1px 2px rgba(0,0,0,0.4);
        }

        .swatch-name {
            font-size: 0.7rem;
            color: #64748b;
            font-weight: 500;
            text-align: center;
            max-width: 56px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .color-swatch.selected .swatch-name {
            color: var(--color-primary);
            font-weight: 600;
        }

        .color-section-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 1px solid #f1f5f9;
        }

        .color-count {
            font-size: 0.8rem;
            color: #64748b;
            background: #f1f5f9;
            padding: 0.2rem 0.6rem;
            border-radius: 6px;
            font-weight: 500;
        }

        .select-material-hint {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem 1.25rem;
            background: #f8fafc;
            border-radius: 8px;
            border: 1px dashed #cbd5e1;
        }

        .select-material-hint i {
            color: #94a3b8;
            font-size: 1.25rem;
        }

        .select-material-hint p {
            margin: 0;
            color: #64748b;
            font-size: 0.9rem;
        }

        .colors-skeleton {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
        }

        .skeleton-swatch {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.35rem;
            padding: 0.5rem;
            background: #f8fafc;
            border-radius: 8px;
        }

        .skeleton-circle {
            width: 32px;
            height: 32px;
            border-radius: 8px;
            background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%);
            background-size: 200% 100%;
            animation: skeleton-shimmer 1.5s infinite;
        }

        .skeleton-text {
            width: 36px;
            height: 10px;
            border-radius: 4px;
            background: linear-gradient(90deg, #e2e8f0 25%, #f1f5f9 50%, #e2e8f0 75%);
            background-size: 200% 100%;
            animation: skeleton-shimmer 1.5s infinite;
        }

        @keyframes skeleton-shimmer {
            0% { background-position: 200% 0; }
            100% { background-position: -200% 0; }
        }

        .no-colors-message {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.875rem 1.25rem;
            background: #f8fafc;
            border-radius: 8px;
            border-left: 3px solid var(--color-primary);
            }

        .no-colors-message i {
            color: var(--color-primary);
            font-size: 1.1rem;
        }

        .no-colors-message p {
            margin: 0;
            color: #64748b;
            font-size: 0.875rem;
        }

        /* Summary color styling */
        .color-value {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .color-dot {
            width: 14px;
            height: 14px;
            border-radius: 50%;
            border: 2px solid #e9ecef;
            flex-shrink: 0;
        }

        .optional-value {
            color: #6c757d;
            font-style: italic;
        }

        /* ============ PROFILES SECTION ============ */
        .profiles-section {
            padding: 6rem 2rem;
            background: white;
        }

        .profiles-section .section-title {
            color: #1a1a2e;
        }

        .profiles-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 2rem;
            align-items: stretch;
        }

        .profile-card {
            background: #f8f9fa;
            border-radius: 24px;
            padding: 2.5rem 2rem;
            text-align: center;
            position: relative;
            border: 2px solid transparent;
            transition: all 0.3s ease;
        }

        .profile-card:hover {
            transform: translateY(-5px);
            background: white;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
        }

        .profile-card.selected {
            border-color: #667eea;
            background: white;
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.15), 0 20px 40px rgba(102, 126, 234, 0.12);
            transform: translateY(-4px);
        }

        .profile-card.selected .profile-name {
            color: #667eea;
        }

        .profile-card.featured.selected {
            border-color: rgba(255,255,255,0.6);
            box-shadow: 0 0 0 4px rgba(255,255,255,0.2), 0 20px 40px rgba(0,0,0,0.2);
        }

        .profile-card.featured {
            background: var(--gradient-dark);
            border-color: transparent;
            transform: scale(1.05);
        }

        .profile-card.featured:hover {
            transform: scale(1.08) translateY(-5px);
        }

        .profile-card.featured .profile-name,
        .profile-card.featured .spec-label,
        .profile-card.featured .spec-value {
            color: white;
        }

        .profile-card.featured .profile-icon {
            background: rgba(255,255,255,0.1);
            color: white;
        }

        .profile-badge {
            position: absolute;
            top: -12px;
            left: 50%;
            transform: translateX(-50%);
            display: inline-flex;
            align-items: center;
            gap: 0.4rem;
            padding: 0.5rem 1rem;
            background: linear-gradient(135deg, #ffd700 0%, #ffaa00 100%);
            color: white;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 700;
            text-transform: uppercase;
            white-space: nowrap;
        }

        .profile-icon {
            width: 80px;
            height: 80px;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
            font-size: 2.5rem;
            color: var(--color-primary);
        }

        .profile-name {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.5rem;
        }

        .profile-specs {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-bottom: 2rem;
        }

        .spec-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0.75rem 0;
            border-bottom: 1px solid rgba(0,0,0,0.05);
        }

        .profile-card.featured .spec-row {
            border-color: rgba(255,255,255,0.1);
        }

        .spec-label {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #6c757d;
            font-size: 0.9rem;
        }

        .spec-label i {
            color: var(--color-primary);
        }

        .profile-card.featured .spec-label i {
            color: var(--color-accent);
        }

        .spec-value {
            font-weight: 700;
            color: #1a1a2e;
        }

        .spec-value.positive {
            color: var(--color-primary);
        }

        :host ::ng-deep .profile-select-btn {
            border-radius: 50px !important;
            font-weight: 600 !important;
        }

        .profile-card.featured :host ::ng-deep .profile-select-btn {
            background: white !important;
            color: var(--color-primary) !important;
        }

        /* ============ CALCULATOR SECTION ============ */
        .calculator-section {
            padding: 6rem 2rem;
            background: linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%);
        }

        .calculator-wrapper {
            max-width: 700px;
            margin: 0 auto;
        }

        .calculator-card {
            background: white;
            border-radius: 24px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.1);
        }

        .calculator-form {
            padding: 2.5rem;
            display: flex;
            flex-direction: column;
            gap: 1.75rem;
        }

        .form-group label {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-weight: 600;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .form-group label i {
            color: var(--color-primary);
        }

        :host ::ng-deep .calc-input {
            border-radius: 12px !important;
        }

        .calculator-result {
            background: var(--gradient-dark);
            padding: 2.5rem;
            text-align: center;
            opacity: 0;
            transform: translateY(20px);
            transition: all 0.4s ease;
        }

        .calculator-result.visible {
            opacity: 1;
            transform: translateY(0);
        }

        .result-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            color: rgba(255,255,255,0.7);
            margin-bottom: 1rem;
        }

        .result-price {
            font-size: 3.5rem;
            font-weight: 800;
            color: white;
            margin-bottom: 1.5rem;
            text-shadow: 0 4px 20px rgba(0,0,0,0.3);
        }

        .result-breakdown {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            margin-bottom: 2rem;
        }

        .breakdown-item {
            display: flex;
            justify-content: space-between;
            color: rgba(255,255,255,0.7);
            font-size: 0.95rem;
            padding: 0 2rem;
        }

        /* Calculator Summary Panel */
        .calculator-summary {
            background: var(--gradient-dark);
            padding: 2rem;
            opacity: 0;
            transform: translateY(20px);
            transition: all 0.4s ease;
        }

        .calculator-summary.visible {
            opacity: 1;
            transform: translateY(0);
        }

        .summary-header {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            color: rgba(255,255,255,0.9);
            font-weight: 600;
            margin-bottom: 1.5rem;
        }

        .summary-grid {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }

        .summary-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            background: rgba(255,255,255,0.05);
            border-radius: 10px;
            border: 1px solid rgba(255,255,255,0.1);
        }

        .summary-item.highlight {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%);
            border-color: rgba(102, 126, 234, 0.3);
        }

        .summary-item i {
            color: var(--color-accent);
            font-size: 1.25rem;
            width: 40px;
            height: 40px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: rgba(255,255,255,0.1);
            border-radius: 50%;
        }

        .summary-data {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            text-align: left;
        }

        [dir="rtl"] .summary-data {
            text-align: right;
        }

        .summary-label {
            color: rgba(255,255,255,0.6);
            font-size: 0.8rem;
        }

        .summary-value {
            color: white;
            font-size: 1.1rem;
            font-weight: 600;
        }

        .summary-value.price {
            font-size: 1.5rem;
            font-weight: 800;
            background: var(--gradient-accent);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .calculator-disclaimer {
            color: rgba(255,255,255,0.5);
            font-size: 0.8rem;
            text-align: center;
            margin-bottom: 1.5rem;
            font-style: italic;
        }

        :host ::ng-deep .result-cta {
            background: var(--gradient-primary) !important;
            border: none !important;
            border-radius: 50px !important;
            font-weight: 600 !important;
            padding: 1rem 2rem !important;
        }

        /* ============ FEATURES SECTION ============ */
        .features-section {
            padding: 6rem 2rem;
            background: white;
        }

        .features-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 2rem;
        }

        .feature-card {
            text-align: center;
            padding: 2rem 1.5rem;
            border-radius: 20px;
            transition: all 0.3s ease;
        }

        .feature-card:hover {
            background: #f8f9fa;
            transform: translateY(-5px);
        }

        .feature-icon {
            width: 80px;
            height: 80px;
            background: var(--gradient-primary);
            border-radius: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
            font-size: 2rem;
            color: white;
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
        }

        .feature-title {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .feature-description {
            color: #6c757d;
            line-height: 1.6;
            font-size: 0.95rem;
        }

        /* ============ CTA SECTION ============ */
        .cta-section {
            position: relative;
            padding: 8rem 2rem;
            overflow: hidden;
        }

        .cta-bg {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .cta-particles {
            position: absolute;
            inset: 0;
            z-index: 1;
        }

        .particle {
            position: absolute;
            width: 10px;
            height: 10px;
            background: rgba(255,255,255,0.1);
            border-radius: 50%;
            animation: particleFloat 15s infinite;
        }

        .particle:nth-child(1) { left: 10%; top: 20%; animation-delay: 0s; }
        .particle:nth-child(2) { left: 20%; top: 80%; animation-delay: 2s; }
        .particle:nth-child(3) { left: 60%; top: 10%; animation-delay: 4s; }
        .particle:nth-child(4) { left: 80%; top: 50%; animation-delay: 6s; }
        .particle:nth-child(5) { left: 40%; top: 60%; animation-delay: 8s; }
        .particle:nth-child(6) { left: 90%; top: 30%; animation-delay: 10s; }
        .particle:nth-child(7) { left: 30%; top: 40%; animation-delay: 12s; }
        .particle:nth-child(8) { left: 70%; top: 90%; animation-delay: 14s; }

        @keyframes particleFloat {
            0%, 100% { transform: translateY(0) scale(1); opacity: 0.3; }
            50% { transform: translateY(-100px) scale(1.5); opacity: 0.8; }
        }

        .cta-content {
            position: relative;
            z-index: 10;
            text-align: center;
            max-width: 700px;
            margin: 0 auto;
        }

        .cta-title {
            font-size: clamp(2rem, 5vw, 3.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1.5rem;
            line-height: 1.2;
        }

        .cta-description {
            font-size: 1.25rem;
            color: rgba(255,255,255,0.8);
            margin-bottom: 2.5rem;
            line-height: 1.7;
        }

        .cta-buttons {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }

        :host ::ng-deep .cta-btn-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1.25rem 2.5rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 50px !important;
            box-shadow: var(--shadow-glow) !important;
        }

        :host ::ng-deep .cta-btn-secondary {
            border-color: rgba(255,255,255,0.4) !important;
            color: white !important;
            padding: 1.25rem 2rem !important;
            font-size: 1.1rem !important;
            border-radius: 50px !important;
            background: transparent !important;
        }

        :host ::ng-deep .cta-btn-secondary:hover {
            background: rgba(255,255,255,0.1) !important;
        }

        /* ============ UTILITIES ============ */
        .loading-container {
            display: flex;
            justify-content: center;
            padding: 3rem;
        }

        .loading-spinner {
            text-align: center;
            color: #64748b;
        }

        .loading-spinner p {
            margin-top: 0.75rem;
            font-size: 0.9rem;
        }

        .empty-state {
            text-align: center;
            padding: 2rem;
            color: #64748b;
            background: #f8fafc;
            border: 1px dashed #cbd5e1;
            border-radius: 12px;
        }

        .empty-state i {
            font-size: 2rem;
            margin-bottom: 0.5rem;
            color: #94a3b8;
        }

        .empty-state p {
            margin: 0;
            font-size: 0.9rem;
        }

        /* ============ SELECTABLE CARDS ============ */
        .selection-indicator {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.4rem;
            margin-top: 0.875rem;
            padding: 0.5rem 0.75rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 6px;
            font-weight: 600;
            font-size: 0.8rem;
        }

        .selection-indicator i {
            font-size: 0.75rem;
        }

        /* ============ UPLOAD SECTION ============ */
        /* Upload Area (within workflow) */
        .upload-area {
            max-width: 100%;
        }

        .upload-card {
            background: #f8fafc;
            border-radius: 12px;
            border: 2px dashed #cbd5e1;
            padding: 2rem;
            text-align: center;
            transition: all 0.3s ease;
            position: relative;
        }

        .upload-card.drag-over {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
        }

        .upload-card.has-file {
            border-style: solid;
            border-color: var(--color-primary);
        }

        .upload-content {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 1rem;
        }

        .upload-icon {
            width: 56px;
            height: 56px;
            border-radius: 12px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .upload-icon i {
            font-size: 1.75rem;
            color: var(--color-primary);
        }

        .upload-title {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1e293b;
            margin: 0;
        }

        .upload-hint {
            color: #6c757d;
            margin: 0;
        }

        .file-input-hidden {
            display: none;
        }

        :host ::ng-deep .upload-browse-btn {
            margin-top: 0.5rem;
        }

        :host ::ng-deep .upload-browse-btn.p-button-outlined {
            border-color: var(--color-primary) !important;
            color: var(--color-primary) !important;
            background: transparent !important;
        }

        :host ::ng-deep .upload-browse-btn.p-button-outlined:hover {
            background: var(--color-primary) !important;
            color: white !important;
            border-color: var(--color-primary) !important;
        }

        .upload-formats {
            color: #94a3b8;
            font-size: 0.8rem;
            margin: 0;
        }

        .uploaded-file {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1.5rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%);
            border-radius: 12px;
        }

        .file-icon {
            width: 50px;
            height: 50px;
            border-radius: 10px;
            background: var(--gradient-primary);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .file-icon i {
            color: white;
            font-size: 1.5rem;
        }

        .file-info {
            flex: 1;
            text-align: left;
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }

        [dir="rtl"] .file-info {
            text-align: right;
        }

        .file-name {
            font-weight: 600;
            color: #1a1a2e;
            word-break: break-all;
        }

        .file-size {
            color: #64748b;
            font-size: 0.85rem;
        }

        .remove-file-btn {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            border: none;
            background: #fee2e2;
            color: #dc2626;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s ease;
        }

        .remove-file-btn:hover {
            background: #fecaca;
        }

        .review-file-btn {
            display: flex;
            align-items: center;
            gap: 0.4rem;
            padding: 0 0.9rem;
            height: 36px;
            border-radius: 18px;
            border: 1.5px solid #667eea;
            background: transparent;
            color: #667eea;
            font-size: 0.85rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s ease;
            white-space: nowrap;
        }

        .review-file-btn:hover {
            background: #667eea;
            color: white;
        }

        /* Review overlay */
        .review-overlay {
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.75);
            backdrop-filter: blur(4px);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
        }

        .review-panel {
            width: 100%;
            max-width: 860px;
            height: min(80vh, 680px);
            background: #0f1737;
            border-radius: 20px;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            box-shadow: 0 32px 80px rgba(0,0,0,0.5);
        }

        .review-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 0.85rem 1.25rem;
            background: rgba(255,255,255,0.06);
            border-bottom: 1px solid rgba(255,255,255,0.08);
            flex-shrink: 0;
        }

        .review-meta {
            display: flex;
            align-items: center;
            gap: 0.6rem;
        }

        .review-swatch {
            width: 22px;
            height: 22px;
            border-radius: 50%;
            border: 2px solid rgba(255,255,255,0.3);
            flex-shrink: 0;
        }

        .review-label {
            color: #e2e8f0;
            font-weight: 700;
            font-size: 0.95rem;
        }

        .review-color-name {
            color: #94a3b8;
            font-size: 0.9rem;
        }

        .review-close-btn {
            width: 34px;
            height: 34px;
            border-radius: 50%;
            border: none;
            background: rgba(255,255,255,0.1);
            color: #e2e8f0;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: background 0.2s;
            font-size: 0.9rem;
        }

        .review-close-btn:hover {
            background: rgba(255,255,255,0.2);
        }

        .review-viewer-wrap {
            flex: 1;
            min-height: 0;
        }

        .upload-error {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            margin-top: 1rem;
            padding: 0.75rem 1rem;
            background: #fef2f2;
            color: #dc2626;
            border-radius: 8px;
            font-size: 0.9rem;
        }

        /* ============ SUBMIT SECTION ============ */
        .submit-section {
            padding: 5rem 2rem;
            background: white;
        }

        .submit-wrapper {
            max-width: 700px;
            margin: 0 auto;
        }

        .submit-card {
            background: #f8fafc;
            border-radius: 20px;
            padding: 2.5rem;
            border: 1px solid #e2e8f0;
        }

        .request-summary {
            margin-bottom: 2rem;
        }

        .summary-title {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a1a2e;
            margin: 0 0 1.5rem 0;
        }

        .summary-title i {
            color: var(--color-primary);
        }

        .summary-checklist {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .checklist-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem;
            background: white;
            border-radius: 10px;
            border: 1px solid #e2e8f0;
        }

        .checklist-item.completed {
            border-color: #667eea;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(102, 126, 234, 0.02) 100%);
        }

        .checklist-item i {
            font-size: 1.25rem;
            color: #d1d5db;
        }

        .checklist-item.completed i {
            color: #667eea;
        }

        .checklist-label {
            font-weight: 500;
            color: #475569;
            min-width: 100px;
        }

        .checklist-value {
            flex: 1;
            color: #1a1a2e;
            font-weight: 600;
            text-align: right;
        }

        [dir="rtl"] .checklist-value {
            text-align: left;
        }

        .checklist-pending {
            flex: 1;
            color: #94a3b8;
            font-style: italic;
            text-align: right;
        }

        [dir="rtl"] .checklist-pending {
            text-align: left;
        }

        .comments-section {
            margin-bottom: 1.5rem;
        }

        .comments-label {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-weight: 500;
            color: #475569;
            margin-bottom: 0.75rem;
        }

        .comments-label i {
            color: var(--color-primary);
        }

        .comments-textarea {
            width: 100%;
            padding: 1rem;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            font-family: inherit;
            font-size: 0.95rem;
            resize: vertical;
            transition: border-color 0.2s ease;
        }

        .comments-textarea:focus {
            outline: none;
            border-color: var(--color-primary);
        }

        .submit-disclaimer {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #64748b;
            font-size: 0.85rem;
            margin-bottom: 1.5rem;
            padding: 0.75rem 1rem;
            background: rgba(102, 126, 234, 0.05);
            border-radius: 8px;
        }

        .submit-disclaimer i {
            color: var(--color-primary);
        }

        :host ::ng-deep .submit-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 2rem !important;
            font-size: 1.1rem !important;
            font-weight: 600 !important;
            border-radius: 12px !important;
            transition: all 0.3s ease !important;
        }

        :host ::ng-deep .submit-btn:not(:disabled):hover {
            transform: translateY(-2px) !important;
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4) !important;
        }

        :host ::ng-deep .submit-btn:disabled {
            opacity: 0.6 !important;
            cursor: not-allowed !important;
        }

        .success-card {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(102, 126, 234, 0.05) 100%);
            border: 1px solid #667eea;
            border-radius: 20px;
            padding: 3rem;
            text-align: center;
            margin-top: 2rem;
        }

        .success-icon {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .success-icon i {
            color: white;
            font-size: 2.5rem;
        }

        .success-title {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0 0 0.75rem 0;
        }

        .success-message {
            color: #64748b;
            margin: 0 0 1.5rem 0;
        }

        .reference-number {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            padding: 1rem;
            background: white;
            border-radius: 10px;
            margin-bottom: 1.5rem;
        }

        .ref-label {
            color: #64748b;
            font-size: 0.85rem;
        }

        .ref-value {
            font-family: monospace;
            font-size: 1.1rem;
            font-weight: 600;
            color: var(--color-primary);
        }

        :host ::ng-deep .new-request-btn {
            border-color: var(--color-primary) !important;
            color: var(--color-primary) !important;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 1200px) {
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .process-timeline {
                grid-template-columns: repeat(2, 1fr);
            }
            .timeline-connector {
                display: none;
            }
            .profiles-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .profile-card.featured {
                transform: scale(1);
            }
            .features-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .hero-section {
                padding: 4rem 1.5rem;
                min-height: 90vh;
            }
            .stats-grid,
            .process-timeline,
            .profiles-grid,
            .features-grid {
                grid-template-columns: 1fr;
            }
            .hero-cta {
                flex-direction: column;
            }
            .materials-grid {
                grid-template-columns: 1fr;
            }
            .stat-card {
                flex-direction: column;
                text-align: center;
            }
            .section-header {
                margin-bottom: 2rem;
            }
            .cta-buttons {
                flex-direction: column;
            }
            
            /* Workflow responsive */
            .workflow-section {
                padding: 2rem 1rem;
            }
            .workflow-progress {
                flex-wrap: wrap;
                gap: 0.5rem;
                padding: 1rem;
            }
            .progress-connector {
                width: 20px;
                margin: 0 0.25rem;
            }
            .progress-step .step-label {
                font-size: 0.65rem;
            }
            .progress-step .step-number {
                width: 28px;
                height: 28px;
                font-size: 0.75rem;
            }
            .workflow-container {
                border-radius: 12px;
            }
            .workflow-step {
                padding: 1.25rem 1rem;
            }
            .step-header {
                flex-wrap: wrap;
            }
            .step-status {
                width: 100%;
                margin-top: 0.5rem;
                justify-content: flex-start;
            }
            .step-divider {
                margin: 0 1rem;
            }
            .upload-card {
                padding: 1.5rem;
            }
        }
    `]
})
export class Print3dLandingComponent implements OnInit {
    public languageService = inject(LanguageService);
    private printingApiService = inject(PrintingApiService);
    private messageService = inject(MessageService);
    showLearnMoreDialog = false;

    loading = true;
    materials: Material[] = [];
    profiles: PrintQualityProfile[] = [];
    filteredMaterials: Material[] = [];

    statistics = [
        { icon: 'pi pi-print', value: 500, label: '' },
        { icon: 'pi pi-box', value: 50, label: '' },
        { icon: 'pi pi-users', value: 200, label: '' },
        { icon: 'pi pi-star', value: 99, label: '' }
    ];

    capabilities: string[] = [];
    processSteps: { icon: string; title: string; description: string }[] = [];
    features: { icon: string; title: string; description: string }[] = [];
    displayProfiles: PrintQualityProfile[] = [];

    // Selection state
    selectedMaterialId: string | null = null;
    selectedColorId: string | null = null;
    selectedProfileId: string | null = null;
    
    // Color selection
    availableColors: import('../../../shared/models/printing.model').MaterialColor[] = [];
    colorsLoading = false;
    materialHasColors = false;

    // File upload
    uploadedFile: File | null = null;
    fileError: string | null = null;
    isDragOver = false;
    maxFileSize = 50 * 1024 * 1024; // 50MB

    // 3D Review
    showReviewPanel = false;
    reviewModelUrl: string | null = null;
    reviewColorHex = '';
    reviewColorName = '';
    private reviewObjectUrl: string | null = null;

    // Request submission
    requestComments = '';
    submitting = false;
    requestSubmitted = false;
    submittedRequestId: string | null = null;

    ngOnInit() {
        this.updateLabels();
        this.loadMaterials();
        this.loadProfiles();
    }

    updateLabels() {
        const isAr = this.languageService.language === 'ar';

        this.statistics = [
            { icon: 'pi pi-print', value: 500, label: isAr ? 'قطعة منتجة' : 'Parts Produced' },
            { icon: 'pi pi-box', value: 50, label: isAr ? 'خيار مادة' : 'Material Options' },
            { icon: 'pi pi-users', value: 200, label: isAr ? 'شريك هندسي' : 'Engineering Partners' },
            { icon: 'pi pi-star', value: 99, label: isAr ? '% معدل الجودة' : '% Quality Rate' }
        ];

        this.capabilities = isAr ? [
            'النمذجة الوظيفية',
            'الأغلفة الميكانيكية',
            'المكونات الهيكلية',
            'التجميعات الهندسية',
            'الإنتاج بكميات صغيرة',
            'الأشكال الهندسية المخصصة'
        ] : [
            'Functional Prototyping',
            'Mechanical Enclosures',
            'Structural Components',
            'Engineering Assemblies',
            'Small-Batch Production',
            'Custom Geometries'
        ];

        this.processSteps = [
            {
                icon: 'pi pi-upload',
                title: isAr ? 'رفع النموذج التقني' : 'Upload Technical Model',
                description: isAr ? 'أرسل ملف CAD بصيغة STL أو OBJ أو 3MF' : 'Submit your CAD file in STL, OBJ, or 3MF format'
            },
            {
                icon: 'pi pi-sliders-h',
                title: isAr ? 'اختيار المعايير الهندسية' : 'Select Engineering Parameters',
                description: isAr ? 'اختر نوع المادة ودقة الطبقة ومواصفات الملء' : 'Choose material type, layer resolution, and infill specifications'
            },
            {
                icon: 'pi pi-file-check',
                title: isAr ? 'المراجعة والموافقة الهندسية' : 'Engineering Review & Approval',
                description: isAr ? 'يراجع فريقنا المواصفات ويؤكد معايير الإنتاج' : 'Our team reviews specifications and confirms production parameters'
            },
            {
                icon: 'pi pi-truck',
                title: isAr ? 'الإنتاج والتسليم' : 'Production & Delivery',
                description: isAr ? 'تصنيع دقيق وتسليم بضبط الجودة' : 'Precision manufacturing and quality-controlled delivery'
            }
        ];

        this.features = [
            {
                icon: 'pi pi-verified',
                title: isAr ? 'دقة هندسية' : 'Engineering Precision',
                description: isAr ? 'معدات صناعية بتفاوتات معايرة' : 'Industrial-grade equipment with calibrated tolerances'
            },
            {
                icon: 'pi pi-bolt',
                title: isAr ? 'سرعة التنفيذ' : 'Rapid Turnaround',
                description: isAr ? 'سير عمل إنتاجي محسّن للتسليم الفعال' : 'Optimized production workflow for efficient delivery'
            },
            {
                icon: 'pi pi-chart-line',
                title: isAr ? 'تسعير شفاف' : 'Transparent Pricing',
                description: isAr ? 'تكلفة مبنية على المادة بدون رسوم مخفية' : 'Material-based costing with no hidden fees'
            },
            {
                icon: 'pi pi-cog',
                title: isAr ? 'دعم تقني' : 'Technical Support',
                description: isAr ? 'استشارة هندسية لتحسين المواد والتصميم' : 'Engineering consultation for material and design optimization'
            }
        ];
    }

    loadMaterials() {
        this.printingApiService.getMaterials().subscribe({
            next: (materials: Material[]) => {
                this.materials = materials.filter((m: Material) => m.isActive);
                this.filteredMaterials = [...this.materials];

                // Update statistics
                this.statistics[1].value = this.materials.length;

                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    loadProfiles() {
        this.printingApiService.getProfiles().subscribe({
            next: (profiles: PrintQualityProfile[]) => {
                this.profiles = profiles.filter((p: PrintQualityProfile) => p.isActive);

                // Filter to 3 display profiles: Draft (0.3mm), Standard (0.2mm), Precision (lowest)
                this.displayProfiles = this.filterDisplayProfiles(this.profiles);
            },
            error: () => { }
        });
    }

    filterDisplayProfiles(profiles: PrintQualityProfile[]): PrintQualityProfile[] {
        const sorted = [...profiles].sort((a, b) => b.layerHeightMm - a.layerHeightMm);
        const displayProfiles: PrintQualityProfile[] = [];
        
        // Find Draft (0.3mm or highest layer height)
        const draft = sorted.find(p => p.layerHeightMm >= 0.28) || sorted[0];
        if (draft) displayProfiles.push(draft);
        
        // Find Standard (0.2mm or middle)
        const standard = sorted.find(p => p.layerHeightMm >= 0.18 && p.layerHeightMm < 0.28) || sorted[Math.floor(sorted.length / 2)];
        if (standard && !displayProfiles.includes(standard)) displayProfiles.push(standard);
        
        // Find Precision (0.1mm or 0.07mm - lowest layer height)
        const precision = sorted.find(p => p.layerHeightMm <= 0.12) || sorted[sorted.length - 1];
        if (precision && !displayProfiles.includes(precision)) displayProfiles.push(precision);
        
        return displayProfiles.slice(0, 3);
    }

    getProfileDisplayName(profile: PrintQualityProfile): string {
        const isAr = this.languageService.language === 'ar';
        const layerHeight = profile.layerHeightMm;
        
        if (layerHeight >= 0.28) {
            return isAr ? `مسودة (${layerHeight}mm)` : `Draft (${layerHeight}mm)`;
        } else if (layerHeight >= 0.18) {
            return isAr ? `قياسي (${layerHeight}mm)` : `Standard (${layerHeight}mm)`;
        } else {
            return isAr ? `دقيق (${layerHeight}mm)` : `Precision (${layerHeight}mm)`;
        }
    }

    getProfileDescription(profile: PrintQualityProfile): string {
        const isAr = this.languageService.language === 'ar';
        const layerHeight = profile.layerHeightMm;
        
        if (layerHeight >= 0.28) {
            return isAr ? 'التحقق من المفهوم والتكرار السريع' : 'Concept validation and fast iteration';
        } else if (layerHeight >= 0.18) {
            return isAr ? 'توازن بين القوة والتفاصيل' : 'Balanced strength and detail';
        } else {
            return isAr ? 'مكونات هندسية عالية التفاصيل' : 'High-detail engineering components';
        }
    }

    getProfileIcon(profile: PrintQualityProfile): string {
        const layerHeight = profile.layerHeightMm;
        if (layerHeight >= 0.28) return 'pi pi-bolt';
        if (layerHeight >= 0.18) return 'pi pi-box';
        return 'pi pi-gem';
    }

    getMaterialName(material: Material): string {
        return this.languageService.language === 'ar' && material.nameAr
            ? material.nameAr
            : material.nameEn;
    }

    getProfileName(profile: PrintQualityProfile): string {
        return this.languageService.language === 'ar' && profile.nameAr
            ? profile.nameAr
            : profile.nameEn;
    }

    getMaterialUseCase(material: Material): string {
        const isAr = this.languageService.language === 'ar';
        const name = material.nameEn?.toUpperCase() || '';
        
        const useCases: { [key: string]: { en: string; ar: string } } = {
            'PLA': { 
                en: 'General prototyping and visual models', 
                ar: 'النمذجة العامة والنماذج البصرية' 
            },
            'PETG': { 
                en: 'Functional parts with improved impact resistance', 
                ar: 'الأجزاء الوظيفية مع مقاومة صدمات محسنة' 
            },
            'ABS': { 
                en: 'Structural enclosures and mechanical components', 
                ar: 'الأغلفة الهيكلية والمكونات الميكانيكية' 
            },
            'TPU': { 
                en: 'Flexible parts and vibration dampening', 
                ar: 'الأجزاء المرنة وتخميد الاهتزازات' 
            },
            'RESIN': { 
                en: 'High-detail precision components', 
                ar: 'مكونات عالية الدقة والتفاصيل' 
            }
        };
        
        for (const key of Object.keys(useCases)) {
            if (name.includes(key)) {
                return isAr ? useCases[key].ar : useCases[key].en;
            }
        }
        
        return isAr ? 'مادة بمواصفات هندسية' : 'Engineering-grade material';
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }

    handleImageError(event: Event) {
        const img = event.target as HTMLImageElement;
        if (img) {
            img.src = 'assets/img/defult.png';
        }
    }

    scrollToNextSection() {
        document.querySelector('#next-section')?.scrollIntoView({ behavior: 'smooth' });
    }

    // Selection methods
    selectMaterial(materialId: string) {
        if (this.selectedMaterialId === materialId) {
            this.selectedMaterialId = null;
            this.selectedColorId = null;
            this.availableColors = [];
            this.materialHasColors = false;
        } else {
            this.selectedMaterialId = materialId;
            this.selectedColorId = null;
            this.loadMaterialColors(materialId);
        }
    }

    loadMaterialColors(materialId: string) {
        this.colorsLoading = true;
        this.availableColors = [];
        this.materialHasColors = false;
        
        this.printingApiService.getMaterialColors(materialId).subscribe({
            next: (colors) => {
                this.availableColors = colors;
                this.materialHasColors = colors.length > 0;
                this.colorsLoading = false;
            },
            error: () => {
                this.availableColors = [];
                this.materialHasColors = false;
                this.colorsLoading = false;
            }
        });
    }

    selectColor(colorId: string) {
        this.selectedColorId = this.selectedColorId === colorId ? null : colorId;
    }

    getSelectedColorName(): string {
        if (!this.selectedColorId) return '';
        const color = this.availableColors.find(c => c.id === this.selectedColorId);
        if (!color) return '';
        return this.languageService.language === 'ar' ? color.nameAr : color.nameEn;
    }

    getSelectedColorHex(): string {
        if (!this.selectedColorId) return '';
        const color = this.availableColors.find(c => c.id === this.selectedColorId);
        return color ? color.hexCode : '';
    }

    selectProfile(profileId: string) {
        this.selectedProfileId = this.selectedProfileId === profileId ? null : profileId;
    }

    getSelectedMaterialName(): string {
        if (!this.selectedMaterialId) return '';
        const material = this.materials.find(m => m.id === this.selectedMaterialId);
        return material ? this.getMaterialName(material) : '';
    }

    getSelectedProfileName(): string {
        if (!this.selectedProfileId) return '';
        const profile = this.displayProfiles.find(p => p.id === this.selectedProfileId);
        return profile ? this.getProfileDisplayName(profile) : '';
    }

    // File upload methods
    onDragOver(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;
    }

    onDrop(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;
        
        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.validateAndSetFile(files[0]);
        }
    }

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.validateAndSetFile(input.files[0]);
            input.value = '';
        }
    }

    validateAndSetFile(file: File) {
        this.fileError = null;
        const isAr = this.languageService.language === 'ar';
        
        const fileName = file.name.toLowerCase();
        if (!fileName.endsWith('.stl')) {
            this.fileError = isAr 
                ? 'يُسمح بملفات STL فقط. يرجى اختيار ملف بامتداد .stl'
                : 'Only STL files are allowed. Please select a file with .stl extension.';
            return;
        }

        if (file.size > this.maxFileSize) {
            this.fileError = isAr 
                ? 'حجم الملف يتجاوز الحد الأقصى (50MB). يرجى اختيار ملف أصغر.'
                : 'File size exceeds maximum limit (50MB). Please select a smaller file.';
            return;
        }

        this.uploadedFile = file;
    }

    removeFile() {
        this.uploadedFile = null;
        this.fileError = null;
        this.showReviewPanel = false;
        if (this.reviewObjectUrl) {
            URL.revokeObjectURL(this.reviewObjectUrl);
            this.reviewObjectUrl = null;
            this.reviewModelUrl = null;
        }
    }

    openReview() {
        if (!this.uploadedFile) return;
        if (this.reviewObjectUrl) URL.revokeObjectURL(this.reviewObjectUrl);
        this.reviewObjectUrl = URL.createObjectURL(this.uploadedFile);
        this.reviewModelUrl = this.reviewObjectUrl;

        const color = this.availableColors.find(c => c.id === this.selectedColorId);
        this.reviewColorHex = color?.hexCode || '';
        this.reviewColorName = color
            ? (this.languageService.language === 'ar' && color.nameAr ? color.nameAr : color.nameEn)
            : '';

        this.showReviewPanel = true;
    }

    closeReview() {
        this.showReviewPanel = false;
    }

    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Request submission
    canSubmit(): boolean {
        const basicRequirements = !!(this.selectedMaterialId && this.selectedProfileId && this.uploadedFile);
        if (!basicRequirements) return false;
        if (this.materialHasColors && !this.selectedColorId) return false;
        return true;
    }

    submitRequest() {
        if (!this.canSubmit() || this.submitting) return;
        
        this.submitting = true;
        const isAr = this.languageService.language === 'ar';

        this.printingApiService.createRequest({
            materialId: this.selectedMaterialId!,
            materialColorId: this.selectedColorId || undefined,
            profileId: this.selectedProfileId!,
            comments: this.requestComments || undefined,
            designFile: this.uploadedFile!
        }).subscribe({
            next: (response) => {
                this.submitting = false;
                this.requestSubmitted = true;
                this.submittedRequestId = response.id;
                
                this.messageService.add({
                    severity: 'success',
                    summary: isAr ? 'تم الإرسال' : 'Submitted',
                    detail: isAr ? 'تم إرسال طلب الطباعة بنجاح' : 'Print request submitted successfully',
                    life: 5000
                });

                document.querySelector('#submit-section')?.scrollIntoView({ behavior: 'smooth' });
            },
            error: (err) => {
                this.submitting = false;
                this.messageService.add({
                    severity: 'error',
                    summary: isAr ? 'خطأ' : 'Error',
                    detail: isAr ? 'فشل إرسال الطلب. يرجى المحاولة مرة أخرى.' : 'Failed to submit request. Please try again.',
                    life: 5000
                });
            }
        });
    }

    resetForm() {
        this.selectedMaterialId = null;
        this.selectedColorId = null;
        this.availableColors = [];
        this.materialHasColors = false;
        this.selectedProfileId = null;
        this.uploadedFile = null;
        this.fileError = null;
        this.requestComments = '';
        this.requestSubmitted = false;
        this.submittedRequestId = null;
        
        document.querySelector('#next-section')?.scrollIntoView({ behavior: 'smooth' });
    }
}
