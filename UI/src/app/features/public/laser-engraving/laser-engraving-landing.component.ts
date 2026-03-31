import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { MessageModule } from 'primeng/message';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DividerModule } from 'primeng/divider';
import { FormsModule } from '@angular/forms';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { SliderModule } from 'primeng/slider';
import { LanguageService } from '../../../shared/services/language.service';
import { CatalogApiService } from '../../../shared/services/catalog-api.service';
import { ItemsApiService } from '../../../shared/services/items-api.service';
import { LaserApiService } from '../../../shared/services/laser-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import { Category } from '../../../shared/models/catalog.model';
import { Item } from '../../../shared/models/item.model';
import { LaserMaterial, LaserServiceRequestResult } from '../../../shared/models/laser.model';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-laser-engraving-landing',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, CardModule, ProgressSpinnerModule, ChipModule, TooltipModule, MessageModule, SelectButtonModule, DividerModule, FormsModule, FileUploadModule, InputTextModule, TextareaModule, DialogModule, InputNumberModule, SliderModule],
    template: `
        <div class="laser-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-inner">
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-bolt"></i>
                        {{ 'laserEngraving.hero.badge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'laserEngraving.hero.title' | translate }}</h1>
                    <p class="hero-description">{{ 'laserEngraving.hero.description' | translate }}</p>
                    <div class="hero-cta">
                        <p-button
                            [label]="'laserEngraving.hero.cta' | translate"
                            icon="pi pi-arrow-right"
                            iconPos="right"
                            (onClick)="scrollToNextSection()"
                            styleClass="hero-button-primary">
                        </p-button>
                        <p-button
                            [label]="'laserEngraving.hero.customProjects' | translate"
                            icon="pi pi-envelope"
                                iconPos="left"
                            routerLink="/contact"
                            [outlined]="true"
                            styleClass="hero-button-secondary">
                        </p-button>
                        </div>
                    </div>
                    <div class="laser-machine-wrap">
                        <div class="engraver-box">
                            <div class="frame-top"></div>
                            <div class="frame-left"></div>
                            <div class="frame-right"></div>
                            <div class="frame-back"></div>
                            <div class="engraver-bed">
                                <div class="bed-surface"></div>
                            </div>
                            <div class="x-rail">
                                <div class="laser-carriage">
                                    <div class="laser-housing"></div>
                                    <div class="laser-lens"></div>
                                    <div class="laser-beam"></div>
                                </div>
                            </div>
                            <div class="control-panel">
                                <div class="panel-screen"></div>
                                <div class="led led-on"></div>
                            </div>
                        </div>
                    </div>
                </div>
                <button type="button" class="hero-scroll-indicator" (click)="scrollToNextSection()" [attr.aria-label]="languageService.language === 'ar' ? 'انتقل للقسم التالي' : 'Scroll to next section'">
                    <i class="pi pi-chevron-down"></i>
                </button>
            </section>

            <!-- Materials Selection Section - Industrial 2-Column Layout -->
            <section id="next-section" class="materials-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'laserEngraving.materials.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'laserEngraving.materials.title' | translate }}</h2>
                        <p class="section-description">{{ 'laserEngraving.materials.description' | translate }}</p>
                    </div>

                    <!-- Metal Cutting Warning -->
                    <div class="metal-warning" *ngIf="showMetalWarning()">
                        <p-message severity="warn" [text]="'laserEngraving.materials.metalWarning' | translate"></p-message>
                    </div>

                    <!-- Main Content: 2-Column Layout -->
                    <div class="materials-layout">
                        <!-- Left: Materials Grid -->
                        <div class="materials-main">
                            <!-- Service Mode Toggle -->
                            <div class="mode-toggle-bar">
                                <span class="mode-label">{{ 'laserEngraving.details.operationMode' | translate }}:</span>
                                <p-selectButton
                                    [options]="serviceModeOptions"
                                    [(ngModel)]="selectedServiceMode"
                                    (onChange)="onServiceModeChange()"
                                    styleClass="service-mode-buttons">
                                </p-selectButton>
                            </div>

                            <!-- Loading State -->
                            <div *ngIf="materialsLoading()" class="loading-container">
                                <p-progressSpinner strokeWidth="3" [style]="{ width: '40px', height: '40px' }"></p-progressSpinner>
                                <p>{{ 'laserEngraving.materials.loading' | translate }}</p>
                            </div>

                            <!-- Empty State -->
                            <div *ngIf="!materialsLoading() && filteredMaterials().length === 0 && materialsLoaded()" class="empty-state-grid">
                                <div class="empty-icon"><i class="pi pi-inbox"></i></div>
                                <p class="empty-title">{{ 'laserEngraving.materials.emptyTitle' | translate }}</p>
                                <p class="empty-subtitle">{{ 'laserEngraving.materials.emptySubtitle' | translate }}</p>
                                <p-button
                                    [label]="'laserEngraving.cta.contactUs' | translate"
                                    icon="pi pi-envelope"
                                    routerLink="/contact"
                                    styleClass="contact-btn-empty">
                                </p-button>
                            </div>

                            <!-- Materials Grid -->
                            <div class="materials-grid" *ngIf="!materialsLoading() && filteredMaterials().length > 0">
                                @for (material of filteredMaterials(); track material.id) {
                                    <div
                                        class="material-card"
                                        [class.selected]="selectedMaterial()?.id === material.id"
                                        [class.metal]="material.isMetal"
                                        (click)="selectMaterial(material)"
                                        tabindex="0"
                                        (keydown.enter)="selectMaterial(material)"
                                        (keydown.space)="selectMaterial(material); $event.preventDefault()">
                                        <div class="card-content">
                                            <div class="material-icon">
                                                <i [class]="getMaterialIcon(material)"></i>
                                            </div>
                                            <div class="material-info">
                                                <h4 class="material-name">{{ getMaterialName(material) }}</h4>
                                                <span class="material-type-badge" [class]="'type-' + material.type">
                                                    {{ getTypeBadgeLabel(material.type) }}
                                                </span>
                                            </div>
                                            <div class="material-meta">
                                                <span class="thickness-tag">
                                                    <i class="pi pi-arrows-v"></i>
                                                    {{ getMaterialThickness(material) || ('laserEngraving.materials.thicknessNA' | translate) }}
                                                </span>
                                            </div>
                                        </div>
                                        <div class="card-selected-indicator" *ngIf="selectedMaterial()?.id === material.id">
                                            <i class="pi pi-check"></i>
                                        </div>
                                    </div>
                                }
                            </div>
                        </div>

                        <!-- Right: Details Panel -->
                        <aside class="materials-sidebar">
                            <div class="details-panel" [class.has-selection]="selectedMaterial()">
                                <h3 class="panel-title">{{ 'laserEngraving.details.title' | translate }}</h3>

                                <!-- No Selection State -->
                                <div class="no-selection" *ngIf="!selectedMaterial()">
                                    <div class="no-selection-icon"><i class="pi pi-hand-point-left"></i></div>
                                    <p>{{ 'laserEngraving.details.selectMaterial' | translate }}</p>
                                </div>

                                <!-- Selected Material Details -->
                                <div class="selected-details" *ngIf="selectedMaterial()">
                                    <div class="detail-header">
                                        <span class="detail-name">{{ getMaterialName(selectedMaterial()!) }}</span>
                                        <span class="detail-type-badge" [class]="'type-' + selectedMaterial()!.type">
                                            {{ getTypeBadgeLabel(selectedMaterial()!.type) }}
                                        </span>
                                    </div>

                                    <div class="detail-row" *ngIf="getMaterialThickness(selectedMaterial()!)">
                                        <span class="detail-label">{{ 'laserEngraving.details.recommendedThickness' | translate }}</span>
                                        <span class="detail-value">{{ getMaterialThickness(selectedMaterial()!) }}</span>
                                    </div>

                                    <p class="detail-disclaimer" *ngIf="getMaterialThickness(selectedMaterial()!)">
                                        {{ 'laserEngraving.details.thicknessDisclaimer' | translate }}
                                    </p>

                                    <div class="detail-notes" *ngIf="getMaterialNotes(selectedMaterial()!)">
                                        <span class="detail-label">{{ 'laserEngraving.details.notes' | translate }}</span>
                                        <p class="notes-text">{{ getMaterialNotes(selectedMaterial()!) }}</p>
                                    </div>

                                    <!-- Image Upload Section -->
                                    <div class="upload-section">
                                        <label class="upload-label">{{ 'laserEngraving.upload.title' | translate }}</label>
                                        <p class="upload-hint">{{ 'laserEngraving.upload.hint' | translate }}</p>
                                        
                                        <div 
                                            class="upload-dropzone"
                                            [class.has-file]="selectedFile()"
                                            [class.drag-over]="isDragOver()"
                                            (dragover)="onDragOver($event)"
                                            (dragleave)="onDragLeave($event)"
                                            (drop)="onDrop($event)"
                                            (click)="fileInput.click()">
                                            
                                            <input 
                                                #fileInput
                                                type="file"
                                                accept="image/*,.ai,.eps,.pdf,.svg"
                                                (change)="onFileSelected($event)"
                                                style="display: none;">
                                            
                                            <div *ngIf="!selectedFile()" class="dropzone-empty">
                                                <i class="pi pi-cloud-upload"></i>
                                                <span>{{ 'laserEngraving.upload.dropzone' | translate }}</span>
                                                <span class="dropzone-formats">{{ 'laserEngraving.upload.formats' | translate }}</span>
                                            </div>
                                            
                                            <div *ngIf="selectedFile()" class="dropzone-preview">
                                                <img *ngIf="filePreview()" [src]="filePreview()" alt="Preview" class="preview-image">
                                                <div *ngIf="!filePreview()" class="file-icon">
                                                    <i class="pi pi-file"></i>
                                                </div>
                                                <div class="file-info">
                                                    <span class="file-name">{{ selectedFile()!.name }}</span>
                                                    <span class="file-size">{{ formatFileSize(selectedFile()!.size) }}</span>
                                                </div>
                                                <button type="button" class="remove-file" (click)="removeFile($event)">
                                                    <i class="pi pi-times"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Size Selection Section -->
                                    <div class="size-section">
                                        <label class="size-label">{{ 'laserEngraving.size.title' | translate }}</label>
                                        <p class="size-hint">{{ 'laserEngraving.size.hint' | translate }}</p>
                                        
                                        <div class="size-workspace">
                                            <div class="workspace-visual">
                                                <div class="workspace-grid">
                                                    <div 
                                                        class="size-preview" 
                                                        [style.width.%]="(projectWidth / 40) * 100"
                                                        [style.height.%]="(projectHeight / 40) * 100">
                                                        <span class="size-label-inner" *ngIf="projectWidth > 5 && projectHeight > 5">
                                                            {{ projectWidth }} × {{ projectHeight }} cm
                                                        </span>
                                                    </div>
                                                </div>
                                                <div class="workspace-label">{{ 'laserEngraving.size.workspaceLabel' | translate }}</div>
                                            </div>
                                            
                                            <div class="size-inputs">
                                                <div class="size-input-group">
                                                    <label>{{ 'laserEngraving.size.width' | translate }}</label>
                                                    <div class="input-with-unit">
                                                        <p-inputNumber 
                                                            [(ngModel)]="projectWidth" 
                                                            [min]="1" 
                                                            [max]="40"
                                                            [showButtons]="true"
                                                            buttonLayout="horizontal"
                                                            incrementButtonIcon="pi pi-plus"
                                                            decrementButtonIcon="pi pi-minus"
                                                            suffix=" cm"
                                                            styleClass="size-number-input">
                                                        </p-inputNumber>
                                                    </div>
                                                </div>
                                                <div class="size-input-group">
                                                    <label>{{ 'laserEngraving.size.height' | translate }}</label>
                                                    <div class="input-with-unit">
                                                        <p-inputNumber 
                                                            [(ngModel)]="projectHeight" 
                                                            [min]="1" 
                                                            [max]="40"
                                                            [showButtons]="true"
                                                            buttonLayout="horizontal"
                                                            incrementButtonIcon="pi pi-plus"
                                                            decrementButtonIcon="pi pi-minus"
                                                            suffix=" cm"
                                                            styleClass="size-number-input">
                                                        </p-inputNumber>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        
                                        <div class="size-presets">
                                            <span class="presets-label">{{ 'laserEngraving.size.presets' | translate }}</span>
                                            <div class="preset-buttons">
                                                <button type="button" class="preset-btn" (click)="setSize(10, 10)" [class.active]="projectWidth === 10 && projectHeight === 10">10×10</button>
                                                <button type="button" class="preset-btn" (click)="setSize(15, 15)" [class.active]="projectWidth === 15 && projectHeight === 15">15×15</button>
                                                <button type="button" class="preset-btn" (click)="setSize(20, 20)" [class.active]="projectWidth === 20 && projectHeight === 20">20×20</button>
                                                <button type="button" class="preset-btn" (click)="setSize(30, 20)" [class.active]="projectWidth === 30 && projectHeight === 20">30×20</button>
                                                <button type="button" class="preset-btn" (click)="setSize(40, 40)" [class.active]="projectWidth === 40 && projectHeight === 40">{{ 'laserEngraving.size.maxSize' | translate }}</button>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Optional Customer Info -->
                                    <div class="customer-info-section">
                                        <label class="info-label">{{ 'laserEngraving.customer.contactInfo' | translate }}</label>
                                        <p class="info-hint">{{ 'laserEngraving.customer.optional' | translate }}</p>
                                        
                                        <div class="info-fields">
                                            <input 
                                                type="text" 
                                                pInputText 
                                                [(ngModel)]="customerName"
                                                [placeholder]="'laserEngraving.customer.name' | translate"
                                                class="info-input">
                                            <input 
                                                type="email" 
                                                pInputText 
                                                [(ngModel)]="customerEmail"
                                                [placeholder]="'laserEngraving.customer.email' | translate"
                                                class="info-input">
                                            <input 
                                                type="tel" 
                                                pInputText 
                                                [(ngModel)]="customerPhone"
                                                [placeholder]="'laserEngraving.customer.phone' | translate"
                                                class="info-input">
                                            <textarea 
                                                pInputTextarea 
                                                [(ngModel)]="customerNotes"
                                                [placeholder]="'laserEngraving.customer.notes' | translate"
                                                rows="2"
                                                class="info-textarea">
                                            </textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="panel-cta">
                                    <p-button
                                        [label]="'laserEngraving.cta.requestService' | translate"
                                        icon="pi pi-send"
                                        iconPos="right"
                                        [disabled]="!canSubmitRequest()"
                                        [loading]="isSubmitting()"
                                        (onClick)="submitRequest()"
                                        styleClass="cta-primary w-full">
                                    </p-button>
                                    <p class="cta-disclaimer">{{ 'laserEngraving.cta.quotationDisclaimer' | translate }}</p>
                                    <p-button
                                        [label]="'laserEngraving.cta.contactUs' | translate"
                                        icon="pi pi-envelope"
                                        routerLink="/contact"
                                        [outlined]="true"
                                        styleClass="cta-secondary w-full">
                                    </p-button>
                                </div>
                            </div>
                        </aside>
                    </div>
                </div>
            </section>

            <!-- Divider -->
            <div class="section-divider">
                <p-divider></p-divider>
            </div>

            <!-- Catalog Items Section -->
            <section class="items-section">
                <div class="container">
                    <div class="section-header section-header-secondary">
                        <span class="section-badge secondary">{{ 'laserEngraving.section.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'laserEngraving.section.title' | translate }}</h2>
                        <p class="section-description">{{ 'laserEngraving.section.description' | translate }}</p>
                    </div>

                    <div *ngIf="loading" class="loading-container">
                        <p-progressSpinner strokeWidth="3" [style]="{ width: '40px', height: '40px' }"></p-progressSpinner>
                    </div>

                    <div class="items-grid" *ngIf="!loading && items.length > 0">
                        <a *ngFor="let item of items; let i = index" [routerLink]="['/item', item.id]" class="item-card" [style.animation-delay]="(i * 0.05) + 's'">
                            <div class="item-image-wrapper">
                                <img [src]="getItemImage(item)" [alt]="getItemName(item)" onerror="this.src='assets/img/defult.png'" />
                            </div>
                            <div class="item-content">
                                <h3 class="item-name">{{ getItemName(item) }}</h3>
                                <div class="item-price">{{ formatCurrency(item.price) }}</div>
                                <p-button [label]="'laserEngraving.viewDetails' | translate" styleClass="item-button w-full"></p-button>
                            </div>
                        </a>
                    </div>

                    <!-- Catalog Empty State -->
                    <div *ngIf="!loading && items.length === 0" class="empty-state-catalog">
                        <i class="pi pi-inbox"></i>
                        <p class="empty-title">{{ 'laserEngraving.catalog.emptyTitle' | translate }}</p>
                        <p class="empty-subtitle">{{ 'laserEngraving.catalog.emptySubtitle' | translate }}</p>
                        <p-button
                            [label]="'laserEngraving.cta.requestService' | translate"
                            icon="pi pi-send"
                            routerLink="/contact"
                            styleClass="catalog-empty-btn">
                        </p-button>
                    </div>

                    <div *ngIf="!loading && items.length > 0" class="section-cta">
                        <p-button
                            [label]="'laserEngraving.browseAll' | translate"
                            icon="pi pi-th-large"
                            iconPos="left"
                            routerLink="/catalog/category/laser-engraving"
                            [outlined]="true"
                            styleClass="cta-button">
                        </p-button>
                    </div>
                </div>
            </section>

            <!-- CTA Section -->
            <section class="cta-section">
                <div class="cta-bg"></div>
                <div class="container">
                    <div class="cta-content">
                        <h2 class="cta-title">{{ 'laserEngraving.cta.title' | translate }}</h2>
                        <p class="cta-description">{{ 'laserEngraving.cta.description' | translate }}</p>
                        <p-button
                            [label]="'menu.contact' | translate"
                            icon="pi pi-envelope"
                            routerLink="/contact"
                            styleClass="cta-btn-primary">
                        </p-button>
                    </div>
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
                [style]="{ width: '400px', maxWidth: '90vw' }">
                <ng-template pTemplate="header">
                    <div class="success-header">
                        <i class="pi pi-check-circle"></i>
                        <span>{{ 'laserEngraving.success.title' | translate }}</span>
                    </div>
                </ng-template>
                <div class="success-content">
                    <p class="success-message">{{ 'laserEngraving.success.message' | translate }}</p>
                    <div class="reference-box" *ngIf="submissionResult()">
                        <span class="reference-label">{{ 'laserEngraving.success.referenceNumber' | translate }}</span>
                        <span class="reference-number">{{ submissionResult()!.referenceNumber }}</span>
                    </div>
                    <p class="success-note">{{ 'laserEngraving.success.note' | translate }}</p>
                </div>
                <ng-template pTemplate="footer">
                    <p-button 
                        [label]="'laserEngraving.success.close' | translate" 
                        (onClick)="closeSuccessDialog()"
                        styleClass="success-close-btn">
                    </p-button>
                </ng-template>
            </p-dialog>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-hero: linear-gradient(135deg, #1e204a 0%, #2a2a5e 40%, #3d2a5e 70%, #4a204a 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-primary-hover: #5a72d9;
            --surface-card: #ffffff;
            --surface-ground: #f8fafc;
            --text-primary: #1a1a2e;
            --text-secondary: #6b7280;
            --border-color: #e5e7eb;
        }

        .laser-page { width: 100%; overflow-x: hidden; background: var(--surface-ground); }
        .container { max-width: 1280px; margin: 0 auto; padding: 0 1.5rem; }

        /* Hero Section */
        .hero-section {
            position: relative;
            min-height: 90vh;
            display: flex;
            align-items: center;
            padding: 4rem 1.5rem 3rem;
            overflow: hidden;
        }
        .hero-bg-gradient {
            position: absolute; inset: 0;
            background: var(--gradient-hero);
            z-index: 0;
        }
        .hero-pattern {
            position: absolute; inset: 0;
            background-image: radial-gradient(circle at 20% 30%, rgba(102, 126, 234, 0.12) 0%, transparent 45%),
                radial-gradient(circle at 80% 70%, rgba(118, 75, 162, 0.15) 0%, transparent 45%);
            z-index: 1;
        }
        .hero-inner {
            position: relative;
            z-index: 10;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 3rem;
            align-items: center;
            max-width: 1200px;
            margin: 0 auto;
            width: 100%;
        }
        .hero-content { text-align: left; max-width: 540px; }
        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            background: rgba(255,255,255,0.12);
            border-radius: 50px;
            font-size: 0.875rem;
            font-weight: 600;
            color: white;
            margin-bottom: 1.5rem;
        }
        .hero-title {
            font-size: clamp(2rem, 4vw, 3rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1rem;
            line-height: 1.2;
        }
        .hero-description {
            font-size: clamp(0.95rem, 1.15vw, 1.1rem);
            color: rgba(255,255,255,0.92);
            line-height: 1.7;
            margin-bottom: 1.5rem;
        }
        .hero-cta { display: flex; flex-wrap: wrap; gap: 1rem; }

        /* Laser Machine Animation */
        .laser-machine-wrap {
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 340px;
            pointer-events: none;
            animation: laserFloat 6s ease-in-out infinite;
        }
        @keyframes laserFloat {
            0%, 100% { transform: translateY(0); }
            50% { transform: translateY(-8px); }
        }
        .engraver-box { position: relative; width: 200px; height: 220px; }
        .frame-top {
            position: absolute; top: 0; left: 10px; right: 10px; height: 14px;
            background: linear-gradient(90deg, #2d3748 0%, #4a5568 50%, #2d3748 100%);
            border-radius: 4px 4px 0 0;
        }
        .frame-left, .frame-right {
            position: absolute; top: 0; width: 14px; height: 200px;
            background: linear-gradient(180deg, #4a5568 0%, #2d3748 100%);
            border-radius: 4px;
        }
        .frame-left { left: 0; }
        .frame-right { right: 0; }
        .frame-back {
            position: absolute; top: 14px; left: 14px; right: 14px; height: 160px;
            background: linear-gradient(180deg, #1a1a2e 0%, #0c1445 100%);
            border: 2px solid #2d3748;
            border-top: none;
        }
        .engraver-bed {
            position: absolute; bottom: 28px; left: 24px; right: 24px; height: 90px;
            background: linear-gradient(180deg, #4a5568 0%, #2d3748 100%);
            border-radius: 4px;
            overflow: hidden;
        }
        .bed-surface {
            position: absolute; inset: 4px;
            background: linear-gradient(135deg, #718096 0%, #a0aec0 100%);
            border-radius: 2px;
        }
        .x-rail {
            position: absolute; top: 64px; left: 20px; right: 20px; height: 10px;
            background: linear-gradient(180deg, #718096 0%, #4a5568 100%);
            border-radius: 5px;
            z-index: 10;
        }
        .laser-carriage {
            position: absolute; top: -6px; left: 20px;
            animation: carriageMove 5s ease-in-out infinite;
        }
        @keyframes carriageMove {
            0%, 100% { left: 20px; }
            25% { left: 70px; }
            50% { left: 120px; }
            75% { left: 70px; }
        }
        .laser-housing {
            width: 28px; height: 22px;
            background: linear-gradient(135deg, #2d3748 0%, #1a202c 100%);
            border-radius: 5px;
        }
        .laser-lens {
            position: absolute; bottom: -7px; left: 50%; transform: translateX(-50%);
            width: 10px; height: 10px;
            background: radial-gradient(circle at 30% 30%, #a0aec0, #4a5568);
            border-radius: 50%;
            box-shadow: 0 0 8px rgba(102, 126, 234, 0.6);
        }
        .laser-beam {
            position: absolute; top: 100%; left: 50%; transform: translateX(-50%);
            width: 3px; height: 60px;
            background: linear-gradient(180deg, rgba(102, 126, 234, 0.95) 0%, rgba(118, 75, 162, 0.8) 40%, transparent 100%);
            border-radius: 2px;
            box-shadow: 0 0 18px rgba(102, 126, 234, 0.9);
            animation: beamPulse 0.6s ease-in-out infinite alternate;
        }
        @keyframes beamPulse {
            0% { opacity: 0.9; }
            100% { opacity: 1; box-shadow: 0 0 24px rgba(102, 126, 234, 1); }
        }
        .control-panel {
            position: absolute; bottom: 32px; right: 10px;
            width: 32px; height: 24px;
            background: linear-gradient(180deg, #2d3748 0%, #1a202c 100%);
            border-radius: 4px;
            border: 1px solid #4a5568;
        }
        .panel-screen {
            position: absolute; top: 3px; left: 3px; right: 3px; height: 9px;
            background: #1a202c;
            border-radius: 2px;
        }
        .led {
            position: absolute; bottom: 3px; right: 6px;
            width: 4px; height: 4px;
            border-radius: 50%;
            background: #48bb78;
        }
        .led-on { box-shadow: 0 0 6px #48bb78; animation: ledBlink 2s ease-in-out infinite; }
        @keyframes ledBlink { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }
        [dir="rtl"] .laser-machine-wrap { direction: ltr; }

        /* Hero Buttons */
        :host ::ng-deep .hero-button-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            padding: 0.75rem 1.25rem !important;
            font-weight: 600 !important;
            border-radius: 10px !important;
        }
        :host ::ng-deep .hero-button-secondary {
            background: transparent !important;
            border: 2px solid rgba(255,255,255,0.8) !important;
            color: white !important;
            padding: 0.75rem 1.25rem !important;
            font-weight: 600 !important;
            border-radius: 10px !important;
        }
        :host ::ng-deep .hero-button-secondary:hover {
            background: rgba(255,255,255,0.1) !important;
        }
        .hero-scroll-indicator {
            position: absolute; bottom: 1.5rem; left: 50%;
            transform: translateX(-50%);
            z-index: 10;
            color: rgba(255,255,255,0.6);
            font-size: 1.5rem;
            cursor: pointer;
            background: none;
            border: none;
            padding: 0.5rem;
            animation: bounce 2s infinite;
        }
        @keyframes bounce {
            0%, 20%, 50%, 80%, 100% { transform: translateX(-50%) translateY(0); }
            40% { transform: translateX(-50%) translateY(-12px); }
            60% { transform: translateX(-50%) translateY(-6px); }
        }

        /* Materials Section */
        .materials-section {
            padding: 3rem 0 2.5rem;
            background: var(--surface-ground);
        }
        .section-header { text-align: center; margin-bottom: 1.5rem; }
        .section-badge {
            display: inline-block;
            padding: 0.4rem 1rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 50px;
            font-size: 0.8rem;
            font-weight: 600;
            margin-bottom: 0.6rem;
        }
        .section-badge.secondary {
            background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
        }
        .section-title {
            font-size: clamp(1.5rem, 3vw, 2rem);
            font-weight: 700;
            color: var(--text-primary);
            margin-bottom: 0.4rem;
        }
        .section-description {
            color: var(--text-secondary);
            max-width: 500px;
            margin: 0 auto;
            font-size: 0.95rem;
        }
        .section-header-secondary { margin-bottom: 1.5rem; }

        .metal-warning { display: flex; justify-content: center; margin-bottom: 1rem; }

        /* 2-Column Layout */
        .materials-layout {
            display: grid;
            grid-template-columns: 1fr 320px;
            gap: 1.5rem;
            align-items: start;
        }
        .materials-main { min-width: 0; }

        /* Mode Toggle Bar */
        .mode-toggle-bar {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 0.75rem 1rem;
            background: var(--surface-card);
            border: 1px solid var(--border-color);
            border-radius: 10px;
            margin-bottom: 1rem;
        }
        .mode-label { font-weight: 600; color: var(--text-secondary); font-size: 0.875rem; }
        :host ::ng-deep .service-mode-buttons .p-button {
            padding: 0.5rem 1.25rem !important;
            font-weight: 600;
            font-size: 0.875rem;
        }

        /* Loading */
        .loading-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 3rem;
            gap: 0.75rem;
            background: var(--surface-card);
            border-radius: 12px;
            border: 1px solid var(--border-color);
        }
        .loading-container p { color: var(--text-secondary); margin: 0; }

        /* Empty State Grid */
        .empty-state-grid {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 2.5rem 2rem;
            background: var(--surface-card);
            border-radius: 12px;
            border: 1px solid var(--border-color);
            text-align: center;
        }
        .empty-icon { font-size: 2.5rem; color: #d1d5db; margin-bottom: 0.75rem; }
        .empty-title { font-size: 1rem; font-weight: 600; color: var(--text-primary); margin: 0 0 0.25rem; }
        .empty-subtitle { font-size: 0.875rem; color: var(--text-secondary); margin: 0 0 1rem; }
        :host ::ng-deep .contact-btn-empty {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
        }

        /* Materials Grid - 3/2/1 columns */
        .materials-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 0.875rem;
        }

        /* Material Card */
        .material-card {
            position: relative;
            display: flex;
            flex-direction: column;
            padding: 1rem;
            background: var(--surface-card);
            border: 2px solid var(--border-color);
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.2s ease;
        }
        .material-card:hover {
            border-color: var(--color-primary);
            box-shadow: 0 4px 16px rgba(102, 126, 234, 0.12);
            transform: translateY(-2px);
        }
        .material-card:focus {
            outline: 2px solid var(--color-primary);
            outline-offset: 2px;
        }
        .material-card.selected {
            border-color: var(--color-primary);
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.06) 0%, rgba(118, 75, 162, 0.06) 100%);
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.15);
        }
        .material-card.metal {
            border-color: #fbbf24;
            background: rgba(251, 191, 36, 0.04);
        }
        .card-content { display: flex; flex-direction: column; gap: 0.6rem; }
        .material-icon {
            width: 40px; height: 40px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: var(--gradient-primary);
            border-radius: 8px;
            flex-shrink: 0;
        }
        .material-icon i { font-size: 1.1rem; color: white; }
        .material-card.metal .material-icon {
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
        }
        .material-info { flex: 1; min-width: 0; }
        .material-name {
            font-size: 0.95rem;
            font-weight: 700;
            color: var(--text-primary);
            margin: 0 0 0.35rem;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }
        .material-type-badge, .detail-type-badge {
            display: inline-flex;
            align-items: center;
            padding: 0.2rem 0.5rem;
            font-size: 0.65rem;
            font-weight: 600;
            border-radius: 4px;
            text-transform: uppercase;
        }
        .material-type-badge.type-both, .detail-type-badge.type-both {
            background: rgba(102, 126, 234, 0.15);
            color: #667eea;
        }
        .material-type-badge.type-cut, .detail-type-badge.type-cut {
            background: rgba(239, 68, 68, 0.15);
            color: #ef4444;
        }
        .material-type-badge.type-engrave, .detail-type-badge.type-engrave {
            background: rgba(102, 126, 234, 0.15);
            color: #667eea;
        }
        .material-meta { display: flex; align-items: center; }
        .thickness-tag {
            display: inline-flex;
            align-items: center;
            gap: 0.25rem;
            font-size: 0.75rem;
            color: var(--text-secondary);
            padding: 0.2rem 0.5rem;
            background: rgba(107, 114, 128, 0.08);
            border-radius: 4px;
        }
        .thickness-tag i { font-size: 0.7rem; }
        .card-selected-indicator {
            position: absolute;
            top: 0.5rem;
            right: 0.5rem;
            width: 22px;
            height: 22px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: var(--color-primary);
            border-radius: 50%;
        }
        .card-selected-indicator i { font-size: 0.7rem; color: white; }
        [dir="rtl"] .card-selected-indicator { right: auto; left: 0.5rem; }

        /* Sidebar / Details Panel */
        .materials-sidebar { position: sticky; top: 1.5rem; }
        .details-panel {
            background: var(--surface-card);
            border: 1px solid var(--border-color);
            border-radius: 12px;
            padding: 1.25rem;
        }
        .details-panel.has-selection { border-color: var(--color-primary); }
        .panel-title {
            font-size: 0.9rem;
            font-weight: 700;
            color: var(--text-primary);
            margin: 0 0 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 1px solid var(--border-color);
        }
        .no-selection {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 2rem 1rem;
            text-align: center;
        }
        .no-selection-icon {
            font-size: 2rem;
            color: #d1d5db;
            margin-bottom: 0.75rem;
        }
        [dir="rtl"] .no-selection-icon i { transform: scaleX(-1); }
        .no-selection p { color: var(--text-secondary); font-size: 0.875rem; margin: 0; }
        .selected-details { margin-bottom: 1.25rem; }
        .detail-header {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }
        .detail-name {
            font-size: 1.15rem;
            font-weight: 700;
            color: var(--text-primary);
        }
        .detail-row {
            display: flex;
            flex-wrap: wrap;
            gap: 0.4rem;
            margin-bottom: 0.5rem;
        }
        .detail-label {
            font-size: 0.8rem;
            color: var(--text-secondary);
            font-weight: 500;
        }
        .detail-value {
            font-size: 0.9rem;
            font-weight: 600;
            color: var(--color-primary);
        }
        .detail-disclaimer {
            font-size: 0.75rem;
            color: #9ca3af;
            font-style: italic;
            margin: 0 0 0.75rem;
        }
        .detail-notes { margin-top: 0.75rem; padding-top: 0.75rem; border-top: 1px solid var(--border-color); }
        .notes-text {
            font-size: 0.85rem;
            color: var(--text-secondary);
            line-height: 1.5;
            margin: 0.35rem 0 0;
        }
        .panel-cta {
            margin-top: 1.25rem;
            padding-top: 1rem;
            border-top: 1px solid var(--border-color);
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        :host ::ng-deep .cta-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
            padding: 0.75rem 1rem !important;
        }
        .cta-disclaimer {
            font-size: 0.7rem;
            color: #9ca3af;
            text-align: center;
            margin: 0.25rem 0;
            line-height: 1.4;
        }
        :host ::ng-deep .cta-secondary {
            border-color: var(--border-color) !important;
            color: var(--text-secondary) !important;
            font-weight: 500 !important;
            border-radius: 8px !important;
        }
        :host ::ng-deep .cta-contact {
            background: transparent !important;
            border: 2px solid var(--color-primary) !important;
            color: var(--color-primary) !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
        }
        :host ::ng-deep .cta-contact:hover {
            background: var(--gradient-primary) !important;
            border-color: transparent !important;
            color: white !important;
        }

        /* Section Divider */
        .section-divider {
            max-width: 1280px;
            margin: 0 auto;
            padding: 0 1.5rem;
        }
        :host ::ng-deep .section-divider .p-divider { margin: 0; }

        /* Catalog Items Section */
        .items-section { padding: 2rem 0 2.5rem; background: var(--surface-ground); }
        .items-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
            gap: 1.25rem;
        }
        .item-card {
            display: block;
            background: var(--surface-card);
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 2px 12px rgba(0,0,0,0.06);
            transition: transform 0.3s, box-shadow 0.3s;
            text-decoration: none;
            color: inherit;
        }
        .item-card:hover { transform: translateY(-3px); box-shadow: 0 8px 24px rgba(0,0,0,0.1); }
        .item-image-wrapper { aspect-ratio: 1; overflow: hidden; background: #f3f4f6; }
        .item-image-wrapper img { width: 100%; height: 100%; object-fit: cover; }
        .item-content { padding: 1rem; }
        .item-name { font-size: 0.95rem; font-weight: 700; color: var(--text-primary); margin-bottom: 0.4rem; }
        .item-price { font-size: 1.1rem; font-weight: 700; color: var(--color-primary); margin-bottom: 0.75rem; }
        :host ::ng-deep .item-button { padding: 0.4rem 0.75rem !important; font-size: 0.85rem !important; }

        /* Catalog Empty State */
        .empty-state-catalog {
            text-align: center;
            padding: 2.5rem 2rem;
            background: var(--surface-card);
            border-radius: 12px;
            border: 1px solid var(--border-color);
        }
        .empty-state-catalog i { font-size: 3rem; color: #d1d5db; margin-bottom: 0.75rem; }
        .empty-state-catalog .empty-title { font-size: 1rem; font-weight: 600; color: var(--text-primary); margin: 0 0 0.25rem; }
        .empty-state-catalog .empty-subtitle { font-size: 0.875rem; color: var(--text-secondary); margin: 0 0 1rem; }
        :host ::ng-deep .catalog-empty-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            border-radius: 8px !important;
            padding: 0.65rem 1.25rem !important;
        }
        .section-cta { text-align: center; margin-top: 1.5rem; }
        :host ::ng-deep .cta-button { padding: 0.65rem 1.25rem !important; }

        /* CTA Section */
        .cta-section { position: relative; padding: 2.5rem 2rem; overflow: hidden; }
        .cta-bg { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .cta-content { position: relative; z-index: 10; text-align: center; max-width: 550px; margin: 0 auto; }
        .cta-title { font-size: clamp(1.25rem, 2.5vw, 1.75rem); font-weight: 700; color: white; margin-bottom: 0.5rem; }
        .cta-description { color: rgba(255,255,255,0.9); margin-bottom: 1.25rem; font-size: 0.95rem; }
        :host ::ng-deep .cta-btn-primary {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            padding: 0.65rem 1.25rem !important;
            font-weight: 600 !important;
            border-radius: 10px !important;
        }

        /* Responsive */
        @media (max-width: 1024px) {
            .materials-layout { grid-template-columns: 1fr 280px; }
            .materials-grid { grid-template-columns: repeat(2, 1fr); }
        }
        @media (max-width: 900px) {
            .hero-inner { grid-template-columns: 1fr; text-align: center; }
            .hero-content { text-align: center; max-width: none; }
            .hero-cta { justify-content: center; }
            .laser-machine-wrap { min-height: 260px; order: -1; }
            .engraver-box { transform: scale(0.85); }
        }
        @media (max-width: 768px) {
            .materials-layout {
                grid-template-columns: 1fr;
            }
            .materials-sidebar {
                position: static;
                order: -1;
            }
            .details-panel { margin-bottom: 1rem; }
            .materials-grid { grid-template-columns: repeat(2, 1fr); }
        }
        @media (max-width: 540px) {
            .materials-grid { grid-template-columns: 1fr; }
            .mode-toggle-bar { flex-direction: column; gap: 0.5rem; align-items: flex-start; }
            .hero-section { padding: 3rem 1rem 2rem; min-height: 80vh; }
            .engraver-box { transform: scale(0.7); }
        }

        /* RTL */
        [dir="rtl"] .detail-header,
        [dir="rtl"] .detail-row {
            flex-direction: row-reverse;
            justify-content: flex-start;
        }
        [dir="rtl"] .mode-toggle-bar { flex-direction: row-reverse; }

        /* Upload Section */
        .upload-section {
            margin-top: 1rem;
            padding-top: 1rem;
            border-top: 1px solid var(--border-color);
        }
        .upload-label {
            display: block;
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--text-primary);
            margin-bottom: 0.25rem;
        }
        .upload-hint {
            font-size: 0.75rem;
            color: var(--text-secondary);
            margin: 0 0 0.75rem;
        }
        .upload-dropzone {
            border: 2px dashed var(--border-color);
            border-radius: 10px;
            padding: 1.25rem;
            text-align: center;
            cursor: pointer;
            transition: all 0.2s ease;
            background: rgba(102, 126, 234, 0.02);
        }
        .upload-dropzone:hover, .upload-dropzone.drag-over {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.06);
        }
        .upload-dropzone.has-file {
            border-style: solid;
            border-color: var(--color-primary);
            padding: 0.75rem;
        }
        .dropzone-empty {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
        }
        .dropzone-empty i {
            font-size: 2rem;
            color: var(--color-primary);
            opacity: 0.7;
        }
        .dropzone-empty span {
            font-size: 0.85rem;
            color: var(--text-secondary);
        }
        .dropzone-formats {
            font-size: 0.7rem !important;
            color: #9ca3af !important;
        }
        .dropzone-preview {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            text-align: left;
        }
        .preview-image {
            width: 50px;
            height: 50px;
            object-fit: cover;
            border-radius: 6px;
            border: 1px solid var(--border-color);
        }
        .file-icon {
            width: 50px;
            height: 50px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 6px;
        }
        .file-icon i { font-size: 1.5rem; color: var(--color-primary); }
        .file-info {
            flex: 1;
            min-width: 0;
            display: flex;
            flex-direction: column;
        }
        .file-name {
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--text-primary);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }
        .file-size {
            font-size: 0.7rem;
            color: var(--text-secondary);
        }
        .remove-file {
            width: 28px;
            height: 28px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: rgba(239, 68, 68, 0.1);
            border: none;
            border-radius: 50%;
            cursor: pointer;
            transition: background 0.2s;
        }
        .remove-file:hover { background: rgba(239, 68, 68, 0.2); }
        .remove-file i { font-size: 0.75rem; color: #ef4444; }
        [dir="rtl"] .dropzone-preview { text-align: right; }

        /* Size Selection Section */
        .size-section {
            margin-top: 1rem;
            padding-top: 1rem;
            border-top: 1px solid var(--border-color);
        }
        .size-label {
            display: block;
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--text-primary);
            margin-bottom: 0.25rem;
        }
        .size-hint {
            font-size: 0.7rem;
            color: var(--text-secondary);
            margin: 0 0 0.75rem;
        }
        .size-workspace {
            display: flex;
            gap: 1rem;
            align-items: flex-start;
        }
        .workspace-visual {
            flex: 0 0 120px;
        }
        .workspace-grid {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, #1e1e2e 0%, #2d2d44 100%);
            border: 2px solid var(--color-primary);
            border-radius: 8px;
            position: relative;
            display: flex;
            align-items: flex-end;
            justify-content: flex-start;
            padding: 4px;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
        }
        .size-preview {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.6) 0%, rgba(118, 75, 226, 0.6) 100%);
            border: 1px solid rgba(255, 255, 255, 0.3);
            border-radius: 4px;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
            min-width: 10%;
            min-height: 10%;
        }
        .size-label-inner {
            font-size: 0.55rem;
            color: #fff;
            font-weight: 600;
            text-shadow: 0 1px 2px rgba(0,0,0,0.5);
            white-space: nowrap;
        }
        .workspace-label {
            text-align: center;
            font-size: 0.65rem;
            color: var(--text-secondary);
            margin-top: 0.35rem;
        }
        .size-inputs {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        .size-input-group {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }
        .size-input-group label {
            font-size: 0.7rem;
            color: var(--text-secondary);
            font-weight: 500;
        }
        :host ::ng-deep .size-number-input {
            width: 100%;
        }
        :host ::ng-deep .size-number-input .p-inputnumber-input {
            width: 100%;
            text-align: center;
            font-size: 0.85rem;
            padding: 0.4rem;
        }
        :host ::ng-deep .size-number-input .p-inputnumber-button {
            width: 28px;
        }
        .size-presets {
            margin-top: 0.75rem;
        }
        .presets-label {
            display: block;
            font-size: 0.7rem;
            color: var(--text-secondary);
            margin-bottom: 0.35rem;
        }
        .preset-buttons {
            display: flex;
            flex-wrap: wrap;
            gap: 0.35rem;
        }
        .preset-btn {
            padding: 0.3rem 0.5rem;
            font-size: 0.7rem;
            font-weight: 500;
            background: var(--surface-ground);
            border: 1px solid var(--border-color);
            border-radius: 4px;
            cursor: pointer;
            transition: all 0.2s;
            color: var(--text-primary);
        }
        .preset-btn:hover {
            background: rgba(102, 126, 234, 0.1);
            border-color: var(--color-primary);
        }
        .preset-btn.active {
            background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-secondary) 100%);
            border-color: transparent;
            color: #fff;
        }
        [dir="rtl"] .workspace-grid {
            justify-content: flex-end;
        }

        /* Customer Info Section */
        .customer-info-section {
            margin-top: 1rem;
            padding-top: 1rem;
            border-top: 1px solid var(--border-color);
        }
        .info-label {
            display: block;
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--text-primary);
            margin-bottom: 0.25rem;
        }
        .info-hint {
            font-size: 0.7rem;
            color: var(--text-secondary);
            margin: 0 0 0.75rem;
        }
        .info-fields {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }
        .info-input, .info-textarea {
            width: 100%;
            font-size: 0.85rem !important;
        }
        :host ::ng-deep .info-input.p-inputtext,
        :host ::ng-deep .info-textarea.p-inputtextarea {
            padding: 0.6rem 0.75rem;
            border-radius: 6px;
        }

        /* Success Dialog - theme purple/blue (no green) */
        :host ::ng-deep .success-dialog .p-dialog-header {
            padding: 1rem 1.25rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.12) 0%, rgba(118, 75, 162, 0.12) 100%);
            border-bottom: 1px solid rgba(102, 126, 234, 0.25);
        }
        .success-header {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }
        .success-header i {
            font-size: 1.5rem;
            color: #667eea;
        }
        .success-header span {
            font-size: 1.1rem;
            font-weight: 700;
            color: var(--text-primary);
        }
        .success-content {
            padding: 0.5rem 0;
            text-align: center;
        }
        .success-message {
            font-size: 0.95rem;
            color: var(--text-secondary);
            margin: 0 0 1rem;
        }
        .reference-box {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            padding: 1rem;
            background: rgba(102, 126, 234, 0.08);
            border-radius: 8px;
            margin-bottom: 1rem;
        }
        .reference-label {
            font-size: 0.75rem;
            color: var(--text-secondary);
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        .reference-number {
            font-size: 1.25rem;
            font-weight: 700;
            color: var(--color-primary);
            font-family: monospace;
        }
        .success-note {
            font-size: 0.8rem;
            color: var(--text-secondary);
            margin: 0;
        }
        :host ::ng-deep .success-dialog .p-dialog-footer {
            padding: 1rem 1.25rem;
            border-top: 1px solid var(--border-color);
        }
        :host ::ng-deep .success-close-btn {
            background: var(--gradient-primary) !important;
            border: none !important;
            color: white !important;
            font-weight: 600 !important;
            width: 100%;
        }
    `]
})
export class LaserEngravingLandingComponent implements OnInit {
    languageService = inject(LanguageService);
    private catalogApi = inject(CatalogApiService);
    private itemsApi = inject(ItemsApiService);
    private translate = inject(TranslateService);
    laserApi = inject(LaserApiService);
    private authService = inject(AuthService);
    private router = inject(Router);

    loading = true;
    items: Item[] = [];
    private categoryId: string | null = null;

    selectedServiceMode: 'cut' | 'engrave' = 'engrave';
    serviceModeOptions: { label: string; value: string }[] = [];

    selectedMaterial = signal<LaserMaterial | null>(null);
    materialsLoading = computed(() => this.laserApi.isLoading());
    materialsLoaded = computed(() => this.laserApi.isLoaded());

    selectedFile = signal<File | null>(null);
    filePreview = signal<string | null>(null);
    isDragOver = signal<boolean>(false);
    isSubmitting = signal<boolean>(false);
    submissionResult = signal<LaserServiceRequestResult | null>(null);
    showSuccessDialog = false;

    projectWidth = 20;
    projectHeight = 20;

    customerName = '';
    customerEmail = '';
    customerPhone = '';
    customerNotes = '';

    filteredMaterials = computed(() => {
        const materials = this.laserApi.materials();
        if (this.selectedServiceMode === 'cut') {
            return materials.filter(m => m.isActive && !m.isMetal && (m.type === 'cut' || m.type === 'both'));
        }
        return materials.filter(m => m.isActive && (m.type === 'engrave' || m.type === 'both'));
    });

    constructor() {
        effect(() => {
            const lang = this.languageService.languageSignal();
            this.updateServiceModeOptions();
        });
    }

    ngOnInit() {
        this.updateServiceModeOptions();
        this.laserApi.loadMaterialsIfNeeded().subscribe();

        this.catalogApi.getAllCategories(true, true).subscribe({
            next: (categories: Category[]) => {
                const laserCategory = categories.find(c => c.slug === 'laser-engraving');
                this.categoryId = laserCategory?.id ?? null;
                if (this.categoryId) {
                    this.itemsApi.getAllItems({ categoryId: this.categoryId, pageSize: 100 }).subscribe({
                        next: (items) => { this.items = items; this.loading = false; },
                        error: () => { this.loading = false; }
                    });
                } else {
                    this.loading = false;
                }
            },
            error: () => { this.loading = false; }
        });
    }

    private updateServiceModeOptions(): void {
        const lang = this.languageService.language;
        this.serviceModeOptions = [
            { label: lang === 'ar' ? 'نقش' : 'Engrave', value: 'engrave' },
            { label: lang === 'ar' ? 'قطع' : 'Cut', value: 'cut' }
        ];
    }

    onServiceModeChange(): void {
        if (this.selectedMaterial()) {
            const current = this.selectedMaterial()!;
            const stillValid = this.filteredMaterials().find(m => m.id === current.id);
            if (!stillValid) {
                this.selectedMaterial.set(null);
            }
        }
    }

    selectMaterial(material: LaserMaterial): void {
        if (this.selectedMaterial()?.id === material.id) {
            this.selectedMaterial.set(null);
        } else {
            this.selectedMaterial.set(material);
        }
    }

    showMetalWarning(): boolean {
        return this.selectedServiceMode === 'cut';
    }

    getMaterialName(material: LaserMaterial): string {
        return this.laserApi.getLocalizedName(material, this.languageService.language);
    }

    getMaterialNotes(material: LaserMaterial): string {
        return this.laserApi.getLocalizedNotes(material, this.languageService.language) || '';
    }

    getMaterialThickness(material: LaserMaterial): string | null {
        return this.laserApi.getThicknessRange(material);
    }

    getMaterialIcon(material: LaserMaterial): string {
        if (material.isMetal) return 'pi pi-box';
        const nameEn = material.nameEn.toLowerCase();
        if (nameEn.includes('acrylic')) return 'pi pi-stop';
        if (nameEn.includes('wood') || nameEn.includes('plywood') || nameEn.includes('mdf')) return 'pi pi-table';
        if (nameEn.includes('leather')) return 'pi pi-wallet';
        if (nameEn.includes('glass')) return 'pi pi-th-large';
        if (nameEn.includes('fabric') || nameEn.includes('cloth')) return 'pi pi-ticket';
        if (nameEn.includes('cardboard') || nameEn.includes('paper')) return 'pi pi-file';
        return 'pi pi-circle';
    }

    getTypeBadgeLabel(type: string): string {
        const lang = this.languageService.language;
        if (type === 'both') return lang === 'ar' ? 'قطع ونقش' : 'Cut & Engrave';
        if (type === 'cut') return lang === 'ar' ? 'قطع فقط' : 'Cut Only';
        return lang === 'ar' ? 'نقش فقط' : 'Engrave Only';
    }

    getItemImage(item: Item): string {
        const raw = item.primaryImageUrl || (item.mediaAssets && (item.mediaAssets as any)[0]?.fileUrl) || '';
        return this.buildImageUrl(raw);
    }

    private buildImageUrl(url: string): string {
        if (!url) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        const baseUrl = (environment.apiUrl || '').replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '');
        const path = url.startsWith('/') ? url : '/' + url;
        return baseUrl ? `${baseUrl}${path}` : url;
    }

    getItemName(item: Item): string {
        return this.languageService.language === 'ar' && item.nameAr ? item.nameAr : item.nameEn;
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', { style: 'currency', currency: 'ILS', minimumFractionDigits: 2 }).format(value);
    }

    scrollToNextSection() {
        document.querySelector('#next-section')?.scrollIntoView({ behavior: 'smooth' });
    }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver.set(true);
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver.set(false);
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver.set(false);
        
        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.handleFile(files[0]);
        }
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.handleFile(input.files[0]);
        }
    }

    private handleFile(file: File): void {
        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml', 'image/bmp', 'application/pdf', 'application/postscript'];
        const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.svg', '.bmp', '.ai', '.eps', '.pdf'];
        
        const extension = '.' + file.name.split('.').pop()?.toLowerCase();
        const isValidType = allowedTypes.includes(file.type) || allowedExtensions.includes(extension);
        
        if (!isValidType) {
            alert(this.languageService.language === 'ar' 
                ? 'نوع الملف غير مدعوم. الأنواع المدعومة: JPG, PNG, GIF, SVG, PDF, AI, EPS'
                : 'Unsupported file type. Supported types: JPG, PNG, GIF, SVG, PDF, AI, EPS');
            return;
        }

        const maxSize = 20 * 1024 * 1024; // 20 MB
        if (file.size > maxSize) {
            alert(this.languageService.language === 'ar'
                ? 'حجم الملف كبير جداً. الحد الأقصى هو 20 ميجابايت'
                : 'File size too large. Maximum size is 20 MB');
            return;
        }

        this.selectedFile.set(file);

        if (file.type.startsWith('image/') && !file.type.includes('svg')) {
            const reader = new FileReader();
            reader.onload = (e) => {
                this.filePreview.set(e.target?.result as string);
            };
            reader.readAsDataURL(file);
        } else {
            this.filePreview.set(null);
        }
    }

    removeFile(event: Event): void {
        event.stopPropagation();
        this.selectedFile.set(null);
        this.filePreview.set(null);
    }

    formatFileSize(bytes: number): string {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    canSubmitRequest(): boolean {
        return !!this.selectedMaterial() && !!this.selectedFile();
    }

    submitRequest(): void {
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url || '/laser-engraving' } });
            return;
        }
        const material = this.selectedMaterial();
        const file = this.selectedFile();

        if (!material || !file) {
            return;
        }

        this.isSubmitting.set(true);

        this.laserApi.submitServiceRequest(
            material.id,
            this.selectedServiceMode,
            file,
            {
                widthCm: this.projectWidth,
                heightCm: this.projectHeight
            },
            {
                name: this.customerName || undefined,
                email: this.customerEmail || undefined,
                phone: this.customerPhone || undefined,
                notes: this.customerNotes || undefined
            }
        ).subscribe({
            next: (result) => {
                this.isSubmitting.set(false);
                this.submissionResult.set(result);
                this.showSuccessDialog = true;
                this.resetForm();
            },
            error: (err) => {
                this.isSubmitting.set(false);
                const message = err.error?.message || err.message || 'An error occurred';
                alert(this.languageService.language === 'ar'
                    ? 'حدث خطأ أثناء إرسال الطلب: ' + message
                    : 'Error submitting request: ' + message);
            }
        });
    }

    setSize(width: number, height: number): void {
        this.projectWidth = width;
        this.projectHeight = height;
    }

    private resetForm(): void {
        this.selectedMaterial.set(null);
        this.selectedFile.set(null);
        this.filePreview.set(null);
        this.projectWidth = 20;
        this.projectHeight = 20;
        this.customerName = '';
        this.customerEmail = '';
        this.customerPhone = '';
        this.customerNotes = '';
    }

    closeSuccessDialog(): void {
        this.showSuccessDialog = false;
        this.submissionResult.set(null);
    }
}
