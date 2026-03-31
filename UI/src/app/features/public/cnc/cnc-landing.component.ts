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
import { RadioButtonModule } from 'primeng/radiobutton';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';
import { LanguageService } from '../../../shared/services/language.service';
import { CncApiService } from '../../../shared/services/cnc-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import { CncMaterial, CncOperationType, CncOperationAvailability, CncDepthMode, CncServiceRequestPayload } from '../../../shared/models/cnc.model';

type CncMode = 'routing' | 'pcb';
type RoutingOperation = 'cut' | 'engrave' | 'pocket' | 'drill';
type PcbOperation = 'isolation' | 'pcbDrill' | 'cutout' | 'text';
type PcbSide = 'single' | 'double';
type DesignSourceType = 'production' | 'image';

@Component({
    selector: 'app-cnc-landing',
    standalone: true,
    imports: [
        CommonModule, RouterModule, TranslateModule, ButtonModule, CardModule,
        ProgressSpinnerModule, ChipModule, TooltipModule, MessageModule,
        SelectButtonModule, DividerModule, FormsModule, FileUploadModule,
        InputTextModule, TextareaModule, DialogModule, InputNumberModule,
        RadioButtonModule, ToastModule, SelectModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="cnc-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="hero-pattern"></div>
                <div class="hero-inner">
                    <div class="hero-content">
                        <span class="hero-badge">
                            <i class="pi pi-cog"></i>
                            {{ 'cnc.hero.badge' | translate }}
                        </span>
                        <h1 class="hero-title">{{ 'cnc.hero.title' | translate }}</h1>
                        <p class="hero-description">{{ 'cnc.hero.description' | translate }}</p>
                        
                        <!-- Constraints chips -->
                        <div class="constraints-row">
                            <span class="constraint-chip">
                                <i class="pi pi-arrows-alt"></i>
                                {{ 'cnc.constraints.maxAreaShort' | translate }}
                            </span>
                            <span class="constraint-chip">
                                <i class="pi pi-sort-amount-up"></i>
                                {{ 'cnc.constraints.maxThicknessShort' | translate }}
                            </span>
                            <span class="constraint-chip">
                                <i class="pi pi-clone"></i>
                                {{ 'cnc.constraints.pcbDoubleShort' | translate }}
                            </span>
                        </div>
                        
                        <p class="hero-capability">{{ 'cnc.hero.capability' | translate }}</p>
                        
                        <div class="hero-cta">
                            <p-button
                                [label]="'cnc.hero.cta' | translate"
                                icon="pi pi-arrow-right"
                                iconPos="right"
                                (onClick)="scrollToSection('request-section')"
                                styleClass="hero-button-primary">
                            </p-button>
                            <p-button
                                [label]="'cnc.hero.contact' | translate"
                                icon="pi pi-envelope"
                                iconPos="left"
                                routerLink="/contact"
                                styleClass="hero-button-secondary">
                            </p-button>
                        </div>
                    </div>
                    <div class="cnc-visual-wrap">
                        <div class="cnc-animation-box">
                            <!-- Gantry Rail -->
                            <div class="gantry-bar"></div>
                            
                            <!-- Spindle Unit -->
                            <div class="spindle-unit">
                                <div class="spindle-body"></div>
                                <div class="collet"></div>
                                <div class="bit-holder"></div>
                                <div class="router-bit"></div>
                            </div>
                            
                            <!-- Cutting sparks -->
                            <div class="spark s1"></div>
                            <div class="spark s2"></div>
                            <div class="spark s3"></div>
                            <div class="spark s4"></div>
                            
                            <!-- Work material -->
                            <div class="work-material">
                                <div class="cut-groove"></div>
                            </div>
                        </div>
                    </div>
                </div>
                <button type="button" class="hero-scroll-indicator" (click)="scrollToSection('capabilities-section')">
                    <i class="pi pi-chevron-down"></i>
                </button>
            </section>

            <!-- Capabilities Section -->
            <section id="capabilities-section" class="capabilities-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'cnc.capabilities.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'cnc.capabilities.title' | translate }}</h2>
                    </div>
                    <div class="capabilities-grid">
                        <!-- CNC Routing Card -->
                        <div class="capability-card" [class.active]="selectedMode() === 'routing'" (click)="selectMode('routing')">
                            <div class="capability-icon">
                                <i class="pi pi-th-large"></i>
                            </div>
                            <h3>{{ 'cnc.capabilities.routing.title' | translate }}</h3>
                            <ul class="capability-list">
                                <li>{{ 'cnc.capabilities.routing.item1' | translate }}</li>
                                <li>{{ 'cnc.capabilities.routing.item2' | translate }}</li>
                                <li>{{ 'cnc.capabilities.routing.item3' | translate }}</li>
                                <li>{{ 'cnc.capabilities.routing.item4' | translate }}</li>
                            </ul>
                            <p-button 
                                [label]="languageService.language === 'ar' ? 'اختر هذه الخدمة' : 'Select This Service'"
                                icon="pi pi-check"
                                [outlined]="selectedMode() !== 'routing'"
                                (onClick)="selectModeAndScroll('routing')"
                                styleClass="capability-btn">
                            </p-button>
                        </div>
                        
                        <!-- PCB Milling Card -->
                        <div class="capability-card" [class.active]="selectedMode() === 'pcb'" (click)="selectMode('pcb')">
                            <div class="capability-icon pcb-icon">
                                <i class="pi pi-sitemap"></i>
                            </div>
                            <h3>{{ 'cnc.capabilities.pcb.title' | translate }}</h3>
                            <ul class="capability-list">
                                <li>{{ 'cnc.capabilities.pcb.item1' | translate }}</li>
                                <li>{{ 'cnc.capabilities.pcb.item2' | translate }}</li>
                                <li>{{ 'cnc.capabilities.pcb.item3' | translate }}</li>
                                <li>{{ 'cnc.capabilities.pcb.item4' | translate }}</li>
                            </ul>
                            <p-button 
                                [label]="languageService.language === 'ar' ? 'اختر هذه الخدمة' : 'Select This Service'"
                                icon="pi pi-check"
                                [outlined]="selectedMode() !== 'pcb'"
                                (onClick)="selectModeAndScroll('pcb')"
                                styleClass="capability-btn">
                            </p-button>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Request Form Section -->
            <section id="request-section" class="request-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'cnc.mode.label' | translate }}</span>
                        <h2 class="section-title">{{ languageService.language === 'ar' ? 'تقديم طلب CNC' : 'Submit CNC Request' }}</h2>
                    </div>

                    <!-- Mode Toggle -->
                    <div class="mode-toggle-container">
                        <p-selectButton 
                            [options]="modeOptions" 
                            [(ngModel)]="selectedModeValue"
                            (onChange)="onModeChange($event)"
                            optionLabel="label" 
                            optionValue="value"
                            styleClass="mode-toggle">
                        </p-selectButton>
                    </div>

                    <!-- CNC Routing Context Banner (only in Routing mode) -->
                    <div class="context-banner routing-banner" *ngIf="selectedMode() === 'routing'">
                        <div class="banner-visual">
                            <div class="routing-visual">
                                <div class="routing-bed"></div>
                                <div class="routing-workpiece">
                                    <div class="routing-cutout routing-cutout-1"></div>
                                    <div class="routing-cutout routing-cutout-2"></div>
                                    <div class="routing-pocket"></div>
                                </div>
                                <div class="routing-gantry">
                                    <div class="routing-spindle"></div>
                                </div>
                                <div class="routing-dust"></div>
                            </div>
                        </div>
                        <div class="banner-content">
                            <h4 class="banner-title">{{ 'cnc.routingBanner.title' | translate }}</h4>
                            <p class="banner-desc">{{ 'cnc.routingBanner.desc' | translate }}</p>
                        </div>
                    </div>

                    <!-- PCB Visual Context Banner (only in PCB mode) -->
                    <div class="context-banner pcb-banner" *ngIf="selectedMode() === 'pcb'">
                        <div class="banner-visual">
                            <div class="pcb-visual">
                                <div class="pcb-board">
                                    <div class="pcb-copper-layer"></div>
                                    <div class="pcb-trace-h pcb-trace-h1"></div>
                                    <div class="pcb-trace-h pcb-trace-h2"></div>
                                    <div class="pcb-trace-h pcb-trace-h3"></div>
                                    <div class="pcb-trace-v pcb-trace-v1"></div>
                                    <div class="pcb-trace-v pcb-trace-v2"></div>
                                    <div class="pcb-trace-v pcb-trace-v3"></div>
                                    <div class="pcb-pad pcb-pad-1"></div>
                                    <div class="pcb-pad pcb-pad-2"></div>
                                    <div class="pcb-pad pcb-pad-3"></div>
                                    <div class="pcb-pad pcb-pad-4"></div>
                                    <div class="pcb-pad pcb-pad-5"></div>
                                    <div class="pcb-pad pcb-pad-6"></div>
                                    <div class="pcb-ic">
                                        <div class="pcb-ic-pins-left"></div>
                                        <div class="pcb-ic-pins-right"></div>
                                        <div class="pcb-ic-dot"></div>
                                    </div>
                                    <div class="pcb-smd pcb-smd-1"></div>
                                    <div class="pcb-smd pcb-smd-2"></div>
                                    <div class="pcb-smd pcb-smd-3"></div>
                                </div>
                            </div>
                        </div>
                        <div class="banner-content">
                            <h4 class="banner-title">{{ 'cnc.pcbBanner.title' | translate }}</h4>
                            <p class="banner-desc">{{ 'cnc.pcbBanner.desc' | translate }}</p>
                        </div>
                    </div>

                    <div class="request-form-grid">
                        <!-- Left Column: Configuration -->
                        <div class="config-column">
                            <!-- Material Selection (Routing mode only) -->
                            <div class="form-section" *ngIf="selectedMode() === 'routing'">
                                <h3 class="form-section-title">
                                    <i class="pi pi-box"></i>
                                    {{ 'cnc.material.title' | translate }}
                                </h3>
                                <p class="form-hint">{{ 'cnc.material.hint' | translate }}</p>
                                
                                <p-select
                                    [options]="materialOptions()"
                                    [(ngModel)]="selectedMaterialId"
                                    (onChange)="onMaterialChange($event)"
                                    optionLabel="label"
                                    optionValue="value"
                                    [placeholder]="'cnc.material.placeholder' | translate"
                                    [showClear]="true"
                                    styleClass="material-dropdown w-full">
                                    <ng-template pTemplate="selectedItem" let-item>
                                        <div class="material-option" *ngIf="item">
                                            <span>{{ item.label }}</span>
                                            <span class="thickness-range" *ngIf="item.thicknessRange">{{ item.thicknessRange }}</span>
                                        </div>
                                    </ng-template>
                                    <ng-template pTemplate="item" let-item>
                                        <div class="material-option">
                                            <span>{{ item.label }}</span>
                                            <span class="thickness-range" *ngIf="item.thicknessRange">{{ item.thicknessRange }}</span>
                                        </div>
                                    </ng-template>
                                </p-select>
                                
                                <!-- Material Notes -->
                                <div class="material-notes" *ngIf="selectedMaterial() && getMaterialNotes()">
                                    <i class="pi pi-info-circle"></i>
                                    <span>{{ getMaterialNotes() }}</span>
                                </div>
                            </div>

                            <!-- Operation Type -->
                            <div class="form-section">
                                <h3 class="form-section-title">
                                    <i class="pi pi-sliders-h"></i>
                                    {{ 'cnc.operation.title' | translate }}
                                </h3>
                                
                                <!-- Routing Operations with Rules Engine -->
                                <div class="operations-grid" *ngIf="selectedMode() === 'routing'">
                                    <div 
                                        *ngFor="let op of routingOperations" 
                                        class="operation-card"
                                        [class.selected]="selectedRoutingOperation() === op.value"
                                        [class.disabled]="!isOperationAvailable(op.value)"
                                        [pTooltip]="getOperationTooltip(op.value)"
                                        tooltipPosition="top"
                                        (click)="selectRoutingOperationIfAvailable(op.value)">
                                        <i [class]="op.icon"></i>
                                        <span class="op-label">{{ op.label }}</span>
                                        <span class="op-helper">{{ op.helper }}</span>
                                        <i class="pi pi-ban disabled-icon" *ngIf="!isOperationAvailable(op.value)"></i>
                                    </div>
                                </div>

                                <!-- PCB Operations -->
                                <div class="operations-grid" *ngIf="selectedMode() === 'pcb'">
                                    <div 
                                        *ngFor="let op of pcbOperations" 
                                        class="operation-card"
                                        [class.selected]="selectedPcbOperation() === op.value"
                                        (click)="selectPcbOperation(op.value)">
                                        <i [class]="op.icon"></i>
                                        <span class="op-label">{{ op.label }}</span>
                                        <span class="op-helper">{{ op.helper }}</span>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Depth Mode (Routing mode, Cut/Engrave only) -->
                            <div class="form-section" *ngIf="selectedMode() === 'routing' && showDepthMode()">
                                <h3 class="form-section-title">
                                    <i class="pi pi-sort-amount-down"></i>
                                    {{ 'cnc.depth.title' | translate }}
                                </h3>
                                
                                <div class="depth-mode-toggle">
                                    <div 
                                        class="depth-option" 
                                        [class.selected]="depthMode() === 'through'"
                                        (click)="setDepthMode('through')">
                                        <i class="pi pi-arrow-down"></i>
                                        <span>{{ 'cnc.depth.through' | translate }}</span>
                                    </div>
                                    <div 
                                        class="depth-option" 
                                        [class.selected]="depthMode() === 'custom'"
                                        (click)="setDepthMode('custom')">
                                        <i class="pi pi-pencil"></i>
                                        <span>{{ 'cnc.depth.custom' | translate }}</span>
                                    </div>
                                </div>
                                
                                <!-- Custom Depth Input -->
                                <div class="custom-depth-input" *ngIf="depthMode() === 'custom'">
                                    <label>{{ 'cnc.depth.value' | translate }} (mm)</label>
                                    <p-inputNumber 
                                        [(ngModel)]="customDepth" 
                                        [min]="0.1" 
                                        [max]="getMaxDepth()" 
                                        [step]="0.1"
                                        [showButtons]="true"
                                        (onInput)="validateDepth()">
                                    </p-inputNumber>
                                    <span class="depth-hint">{{ 'cnc.depth.maxHint' | translate: {max: getMaxDepth()} }}</span>
                                    <span class="depth-error" *ngIf="depthError">{{ depthError }}</span>
                                </div>
                            </div>

                            <!-- PCB Material Selection (only for PCB mode) -->
                            <div class="form-section" *ngIf="selectedMode() === 'pcb'">
                                <h3 class="form-section-title">
                                    <i class="pi pi-box"></i>
                                    {{ 'cnc.pcbMaterial.title' | translate }}
                                </h3>
                                <div class="pcb-material-grid">
                                    <div 
                                        *ngFor="let mat of pcbMaterialOptions" 
                                        class="pcb-material-card"
                                        [class.selected]="selectedPcbMaterial() === mat.value"
                                        (click)="selectPcbMaterial(mat.value)">
                                        <div class="material-header">
                                            <span class="material-name">{{ mat.value }}</span>
                                            <i class="pi pi-check-circle selected-icon" *ngIf="selectedPcbMaterial() === mat.value"></i>
                                        </div>
                                        <p class="material-desc">{{ languageService.language === 'ar' ? mat.descriptionAr : mat.descriptionEn }}</p>
                                    </div>
                                </div>
                            </div>

                            <!-- PCB Thickness Selection (only for PCB mode) -->
                            <div class="form-section" *ngIf="selectedMode() === 'pcb'">
                                <h3 class="form-section-title">
                                    <i class="pi pi-sliders-h"></i>
                                    {{ 'cnc.pcbThickness.title' | translate }}
                                </h3>
                                <div class="pcb-thickness-options">
                                    <div 
                                        *ngFor="let th of getAvailablePcbThicknesses()" 
                                        class="thickness-option"
                                        [class.selected]="selectedPcbThickness() === th.value"
                                        (click)="selectPcbThickness(th.value)">
                                        <span class="thickness-value">{{ th.label }}</span>
                                    </div>
                                </div>
                                <p class="pcb-thickness-hint">{{ 'cnc.pcbThickness.hint' | translate }}</p>
                            </div>

                            <!-- PCB Side Selection (only for PCB mode) -->
                            <div class="form-section" *ngIf="selectedMode() === 'pcb'">
                                <h3 class="form-section-title">
                                    <i class="pi pi-clone"></i>
                                    {{ 'cnc.pcbSide.title' | translate }}
                                </h3>
                                <div class="pcb-side-toggle">
                                    <div 
                                        class="side-option" 
                                        [class.selected]="selectedPcbSide() === 'single'"
                                        (click)="selectPcbSide('single')">
                                        <i class="pi pi-stop"></i>
                                        <span>{{ languageService.language === 'ar' ? 'وجه واحد' : 'Single-Sided' }}</span>
                                    </div>
                                    <div 
                                        class="side-option" 
                                        [class.selected]="selectedPcbSide() === 'double'"
                                        (click)="selectPcbSide('double')">
                                        <i class="pi pi-clone"></i>
                                        <span>{{ languageService.language === 'ar' ? 'وجهين' : 'Double-Sided' }}</span>
                                    </div>
                                </div>
                                
                                <!-- Double-Sided Warning Banner -->
                                <div class="double-sided-warning" *ngIf="selectedPcbSide() === 'double'">
                                    <i class="pi pi-info-circle"></i>
                                    <span>{{ 'cnc.pcbSide.doubleWarning' | translate }}</span>
                                </div>
                            </div>

                            <!-- Dimensions (Routing mode only) -->
                            <div class="form-section" *ngIf="selectedMode() === 'routing'">
                                <h3 class="form-section-title">
                                    <i class="pi pi-arrows-alt"></i>
                                    {{ 'cnc.size.title' | translate }}
                                </h3>
                                
                                <div class="size-workspace">
                                    <!-- Visual Preview -->
                                    <div class="workspace-visual">
                                        <div class="workspace-grid">
                                            <div 
                                                class="size-preview" 
                                                [style.width.%]="(width / 400) * 100"
                                                [style.height.%]="(height / 400) * 100">
                                                <span class="size-label-inner" *ngIf="width > 50 && height > 50">
                                                    {{ width }} × {{ height }}
                                                </span>
                                            </div>
                                        </div>
                                        <div class="workspace-label">{{ 'cnc.size.workspaceLabel' | translate }}</div>
                                    </div>
                                    
                                    <!-- Inputs -->
                                    <div class="size-inputs">
                                        <div class="size-input-row">
                                            <div class="size-input-group">
                                                <label>{{ 'cnc.size.width' | translate }} (mm)</label>
                                                <p-inputNumber 
                                                    [(ngModel)]="width" 
                                                    [min]="1" 
                                                    [max]="400" 
                                                    [showButtons]="true"
                                                    buttonLayout="horizontal"
                                                    incrementButtonIcon="pi pi-plus"
                                                    decrementButtonIcon="pi pi-minus"
                                                    styleClass="size-number-input">
                                                </p-inputNumber>
                                            </div>
                                            <div class="size-input-group">
                                                <label>{{ 'cnc.size.height' | translate }} (mm)</label>
                                                <p-inputNumber 
                                                    [(ngModel)]="height" 
                                                    [min]="1" 
                                                    [max]="400" 
                                                    [showButtons]="true"
                                                    buttonLayout="horizontal"
                                                    incrementButtonIcon="pi pi-plus"
                                                    decrementButtonIcon="pi pi-minus"
                                                    styleClass="size-number-input">
                                                </p-inputNumber>
                                            </div>
                                        </div>
                                        <div class="size-input-row">
                                            <div class="size-input-group">
                                                <label>{{ 'cnc.size.thickness' | translate }} (mm)</label>
                                                <p-inputNumber 
                                                    [(ngModel)]="thickness" 
                                                    [min]="getMinThickness()" 
                                                    [max]="getMaxThickness()" 
                                                    [showButtons]="true"
                                                    [step]="0.5"
                                                    buttonLayout="horizontal"
                                                    incrementButtonIcon="pi pi-plus"
                                                    decrementButtonIcon="pi pi-minus"
                                                    (onInput)="onThicknessChange()"
                                                    styleClass="size-number-input">
                                                </p-inputNumber>
                                                <span class="thickness-limit-hint" *ngIf="selectedMaterial()">
                                                    {{ getMinThickness() }} - {{ getMaxThickness() }} mm
                                                </span>
                                            </div>
                                            <div class="size-input-group">
                                                <label>{{ 'cnc.size.quantity' | translate }}</label>
                                                <p-inputNumber 
                                                    [(ngModel)]="quantity" 
                                                    [min]="1" 
                                                    [max]="100" 
                                                    [showButtons]="true"
                                                    buttonLayout="horizontal"
                                                    incrementButtonIcon="pi pi-plus"
                                                    decrementButtonIcon="pi pi-minus"
                                                    styleClass="size-number-input">
                                                </p-inputNumber>
                                            </div>
                                        </div>
                                        
                                        <!-- Quick Size Presets -->
                                        <div class="size-presets-inline">
                                            <span class="presets-label">{{ 'cnc.size.presets' | translate }}</span>
                                            <div class="preset-buttons">
                                                <button type="button" class="preset-btn" (click)="setPreset(100, 100)">100×100</button>
                                                <button type="button" class="preset-btn" (click)="setPreset(200, 200)">200×200</button>
                                                <button type="button" class="preset-btn" (click)="setPreset(300, 300)">300×300</button>
                                                <button type="button" class="preset-btn" (click)="setPreset(400, 400)">{{ 'cnc.size.maxSize' | translate }}</button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Right Column: Upload & Contact -->
                        <div class="upload-column">
                            <!-- File Upload -->
                            <div class="form-section">
                                <h3 class="form-section-title">
                                    <i class="pi pi-upload"></i>
                                    {{ 'cnc.upload.title' | translate }}
                                </h3>

                                <!-- Design Source Toggle -->
                                <div class="design-source-toggle">
                                    <span class="design-source-label">{{ 'cnc.designSource.label' | translate }}</span>
                                    <div class="design-source-options">
                                        <div 
                                            class="source-option" 
                                            [class.selected]="designSourceType() === 'production'"
                                            (click)="setDesignSource('production')">
                                            <i class="pi pi-file-export"></i>
                                            <span>{{ 'cnc.designSource.production' | translate }}</span>
                                        </div>
                                        <div 
                                            class="source-option" 
                                            [class.selected]="designSourceType() === 'image'"
                                            (click)="setDesignSource('image')">
                                            <i class="pi pi-image"></i>
                                            <span>{{ 'cnc.designSource.image' | translate }}</span>
                                        </div>
                                    </div>
                                </div>

                                <p class="form-hint">{{ getUploadHint() }}</p>
                                
                                <div class="upload-zone" 
                                    (dragover)="onDragOver($event)" 
                                    (dragleave)="onDragLeave($event)"
                                    (drop)="onDrop($event)"
                                    [class.dragover]="isDragging"
                                    (click)="fileInput.click()">
                                    <input #fileInput type="file" [accept]="acceptedFormats" (change)="onFileSelect($event)" hidden multiple>
                                    <i class="pi pi-cloud-upload upload-icon"></i>
                                    <span class="upload-text">{{ 'cnc.upload.dropzone' | translate }}</span>
                                    <span class="upload-formats">{{ getUploadFormatsText() }}</span>
                                </div>

                                <!-- Processing Info Banner (only for Image mode) -->
                                <div class="processing-info-banner" *ngIf="designSourceType() === 'image'">
                                    <i class="pi pi-info-circle"></i>
                                    <span>{{ selectedMode() === 'routing' ? ('cnc.designSource.processingRouting' | translate) : ('cnc.designSource.processingPcb' | translate) }}</span>
                                </div>

                                <!-- Uploaded Files List -->
                                <div class="uploaded-files" *ngIf="uploadedFiles.length > 0">
                                    <div class="uploaded-file" *ngFor="let file of uploadedFiles; let i = index">
                                        <i class="pi pi-file"></i>
                                        <span class="file-name">{{ file.name }}</span>
                                        <span class="file-size">{{ formatFileSize(file.size) }}</span>
                                        <button type="button" class="remove-file" (click)="removeFile(i)">
                                            <i class="pi pi-times"></i>
                                        </button>
                                    </div>
                                </div>

                                <!-- Design Notes -->
                                <div class="design-notes">
                                    <label>{{ 'cnc.upload.designNotesLabel' | translate }}</label>
                                    <textarea pInputTextarea [(ngModel)]="designNotes" rows="3" 
                                        [placeholder]="'cnc.upload.designNotesPlaceholder' | translate"></textarea>
                                </div>
                            </div>

                            <!-- Contact Information -->
                            <div class="form-section">
                                <h3 class="form-section-title">
                                    <i class="pi pi-user"></i>
                                    {{ 'cnc.customer.contactInfo' | translate }}
                                </h3>
                                <p class="form-hint">{{ 'cnc.customer.requiredHint' | translate }}</p>

                                <div class="contact-form">
                                    <div class="form-field">
                                        <label>{{ 'cnc.customer.name' | translate }} *</label>
                                        <input pInputText [(ngModel)]="customerName" [placeholder]="'cnc.customer.namePlaceholder' | translate">
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'cnc.customer.email' | translate }} *</label>
                                        <input pInputText type="email" [(ngModel)]="customerEmail" [placeholder]="'cnc.customer.emailPlaceholder' | translate">
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'cnc.customer.phone' | translate }}</label>
                                        <input pInputText type="tel" [(ngModel)]="customerPhone" [placeholder]="'cnc.customer.phonePlaceholder' | translate">
                                    </div>
                                    <div class="form-field full-width">
                                        <label>{{ 'cnc.customer.description' | translate }}</label>
                                        <textarea pInputTextarea [(ngModel)]="projectDescription" rows="4" 
                                            [placeholder]="'cnc.customer.descriptionPlaceholder' | translate"></textarea>
                                    </div>
                                </div>
                            </div>

                            <!-- Submit Button -->
                            <div class="submit-section">
                                <p-button 
                                    [label]="'cnc.cta.submitRequest' | translate"
                                    icon="pi pi-send"
                                    iconPos="right"
                                    [loading]="isSubmitting"
                                    [disabled]="!isFormValid()"
                                    (onClick)="submitRequest()"
                                    styleClass="submit-btn">
                                </p-button>
                                <p class="disclaimer">{{ 'cnc.cta.disclaimer' | translate }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- File Requirements Section -->
            <section class="file-req-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'cnc.fileReq.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'cnc.fileReq.title' | translate }}</h2>
                    </div>
                    <div class="file-req-grid">
                        <div class="file-req-card">
                            <h4>{{ 'cnc.fileReq.routing.title' | translate }}</h4>
                            <ul>
                                <li>{{ 'cnc.fileReq.routing.item1' | translate }}</li>
                                <li>{{ 'cnc.fileReq.routing.item2' | translate }}</li>
                                <li>{{ 'cnc.fileReq.routing.item3' | translate }}</li>
                            </ul>
                        </div>
                        <div class="file-req-card">
                            <h4>{{ 'cnc.fileReq.pcb.title' | translate }}</h4>
                            <ul>
                                <li>{{ 'cnc.fileReq.pcb.item1' | translate }}</li>
                                <li>{{ 'cnc.fileReq.pcb.item2' | translate }}</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </section>

            <!-- CTA Section -->
            <section class="cta-section">
                <div class="container">
                    <div class="cta-content">
                        <h2>{{ 'cnc.cta.sectionTitle' | translate }}</h2>
                        <p>{{ 'cnc.cta.sectionDescription' | translate }}</p>
                        <p-button 
                            [label]="'cnc.cta.contactUs' | translate"
                            icon="pi pi-send"
                            iconPos="right"
                            routerLink="/contact"
                            styleClass="cta-button">
                        </p-button>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        .cnc-page {
            min-height: 100vh;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            min-height: 90vh;
            display: flex;
            align-items: center;
            justify-content: center;
            overflow: hidden;
            padding: 6rem 2rem;
        }

        .hero-bg {
            position: absolute;
            inset: 0;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            z-index: 0;
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image: 
                radial-gradient(circle at 20% 80%, rgba(102, 126, 234, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 80% 20%, rgba(118, 75, 162, 0.1) 0%, transparent 50%);
            z-index: 1;
        }

        .hero-inner {
            position: relative;
            z-index: 2;
            max-width: 1400px;
            margin: 0 auto;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 4rem;
            align-items: center;
        }

        .hero-content {
            color: white;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            background: rgba(102, 126, 234, 0.2);
            border: 1px solid rgba(102, 126, 234, 0.4);
            padding: 0.5rem 1rem;
            border-radius: 50px;
            font-size: 0.9rem;
            margin-bottom: 1.5rem;
            color: #a5b4fc;
        }

        .hero-title {
            font-size: 3rem;
            font-weight: 800;
            line-height: 1.2;
            margin-bottom: 1.5rem;
            background: linear-gradient(135deg, #ffffff 0%, #a5b4fc 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .hero-description {
            font-size: 1.2rem;
            color: #94a3b8;
            line-height: 1.7;
            margin-bottom: 1.5rem;
        }

        .constraints-row {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
            margin-bottom: 1rem;
        }

        .constraint-chip {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            background: rgba(102, 126, 234, 0.15);
            border: 1px solid rgba(118, 75, 162, 0.35);
            color: #c7d2fe;
            padding: 0.4rem 0.8rem;
            border-radius: 50px;
            font-size: 0.85rem;
            font-weight: 500;
        }

        .hero-capability {
            font-size: 0.95rem;
            color: #fbbf24;
            margin-bottom: 2rem;
            padding: 0.75rem 1rem;
            background: rgba(251, 191, 36, 0.1);
            border-radius: 8px;
            border-left: 3px solid #fbbf24;
        }

        .hero-cta {
            display: flex;
            gap: 1rem;
            flex-wrap: wrap;
        }

        :host ::ng-deep .hero-button-primary {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            border: none !important;
            padding: 1rem 2rem !important;
            border-radius: 50px !important;
            font-weight: 600 !important;
        }

        :host ::ng-deep .hero-button-secondary {
            background: transparent !important;
            border: 2px solid #667eea !important;
            color: #a5b4fc !important;
            padding: 1rem 2rem !important;
            border-radius: 50px !important;
        }

        :host ::ng-deep .hero-button-secondary:hover {
            background: rgba(102, 126, 234, 0.2) !important;
        }

        .hero-scroll-indicator {
            position: absolute;
            bottom: 2rem;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(255, 255, 255, 0.1);
            border: 1px solid rgba(255, 255, 255, 0.2);
            color: white;
            width: 48px;
            height: 48px;
            border-radius: 50%;
            cursor: pointer;
            animation: bounce 2s infinite;
            z-index: 10;
        }

        @keyframes bounce {
            0%, 100% { transform: translateX(-50%) translateY(0); }
            50% { transform: translateX(-50%) translateY(10px); }
        }

        /* CNC Visual - Hero Section */
        .cnc-visual-wrap {
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .cnc-animation-box {
            width: 280px;
            height: 280px;
            background: linear-gradient(145deg, #0f1419 0%, #1a1f2e 100%);
            border-radius: 16px;
            border: 1px solid rgba(102, 126, 234, 0.2);
            position: relative;
            overflow: hidden;
            box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
        }

        /* Gantry Rail */
        .gantry-bar {
            position: absolute;
            top: 30px;
            left: 50%;
            transform: translateX(-50%);
            width: 180px;
            height: 16px;
            background: linear-gradient(180deg, #4a5568 0%, #2d3748 100%);
            border-radius: 3px;
            box-shadow: 0 3px 8px rgba(0, 0, 0, 0.4);
        }

        /* Spindle Unit */
        .spindle-unit {
            position: absolute;
            top: 42px;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .spindle-body {
            width: 50px;
            height: 35px;
            background: linear-gradient(180deg, #4a5568 0%, #2d3748 100%);
            border-radius: 4px 4px 0 0;
        }

        .collet {
            width: 28px;
            height: 18px;
            background: linear-gradient(180deg, #4a5568 0%, #374151 100%);
            clip-path: polygon(15% 0%, 85% 0%, 100% 100%, 0% 100%);
        }

        .bit-holder {
            width: 16px;
            height: 12px;
            background: #4a5568;
        }

        .router-bit {
            width: 6px;
            height: 120px;
            background: linear-gradient(90deg, #6b7280 0%, #4b5563 40%, #6b7280 60%, #4b5563 100%);
            border-radius: 0 0 2px 2px;
            position: relative;
        }

        /* Spiral pattern on bit */
        .router-bit::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: repeating-linear-gradient(
                -45deg,
                transparent,
                transparent 4px,
                rgba(0, 0, 0, 0.3) 4px,
                rgba(0, 0, 0, 0.3) 8px
            );
            border-radius: 0 0 2px 2px;
        }

        /* Sparks */
        .spark {
            position: absolute;
            width: 3px;
            height: 20px;
            background: linear-gradient(180deg, #9ca3af 0%, transparent 100%);
            border-radius: 2px;
        }

        .spark.s1 {
            bottom: 65px;
            left: 100px;
            transform: rotate(-45deg);
            animation: sparkFly1 0.6s ease-out infinite;
        }

        .spark.s2 {
            bottom: 72px;
            left: 115px;
            transform: rotate(-30deg);
            height: 15px;
            animation: sparkFly2 0.6s ease-out infinite 0.1s;
        }

        .spark.s3 {
            bottom: 65px;
            right: 100px;
            transform: rotate(45deg);
            animation: sparkFly3 0.6s ease-out infinite 0.05s;
        }

        .spark.s4 {
            bottom: 72px;
            right: 115px;
            transform: rotate(30deg);
            height: 15px;
            animation: sparkFly4 0.6s ease-out infinite 0.15s;
        }

        @keyframes sparkFly1 {
            0% { opacity: 0; transform: rotate(-45deg) translateY(0); }
            20% { opacity: 1; }
            100% { opacity: 0; transform: rotate(-45deg) translateY(-25px) translateX(-15px); }
        }

        @keyframes sparkFly2 {
            0% { opacity: 0; transform: rotate(-30deg) translateY(0); }
            20% { opacity: 1; }
            100% { opacity: 0; transform: rotate(-30deg) translateY(-20px) translateX(-10px); }
        }

        @keyframes sparkFly3 {
            0% { opacity: 0; transform: rotate(45deg) translateY(0); }
            20% { opacity: 1; }
            100% { opacity: 0; transform: rotate(45deg) translateY(-25px) translateX(15px); }
        }

        @keyframes sparkFly4 {
            0% { opacity: 0; transform: rotate(30deg) translateY(0); }
            20% { opacity: 1; }
            100% { opacity: 0; transform: rotate(30deg) translateY(-20px) translateX(10px); }
        }

        /* Work Material */
        .work-material {
            position: absolute;
            bottom: 30px;
            left: 50%;
            transform: translateX(-50%);
            width: 200px;
            height: 28px;
            background: linear-gradient(180deg, #d97706 0%, #b45309 60%, #92400e 100%);
            border-radius: 4px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
        }

        /* Cut groove where bit touches */
        .cut-groove {
            position: absolute;
            top: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 8px;
            height: 12px;
            background: linear-gradient(180deg, #451a03 0%, #78350f 100%);
            border-radius: 0 0 3px 3px;
            box-shadow: inset 0 3px 6px rgba(0, 0, 0, 0.6);
        }

        /* Dust cloud around cutting point */
        .cut-groove::after {
            content: '';
            position: absolute;
            top: -8px;
            left: 50%;
            transform: translateX(-50%);
            width: 40px;
            height: 12px;
            background: radial-gradient(ellipse at center, rgba(180, 83, 9, 0.5) 0%, transparent 70%);
            animation: dustPulse 0.5s ease-in-out infinite;
        }

        @keyframes dustPulse {
            0%, 100% { opacity: 0.5; transform: translateX(-50%) scale(1); }
            50% { opacity: 1; transform: translateX(-50%) scale(1.3); }
        }

        /* ============ CAPABILITIES SECTION ============ */
        .capabilities-section {
            padding: 6rem 2rem;
            background: #f8fafc;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        .section-header {
            text-align: center;
            margin-bottom: 3rem;
        }

        .section-badge {
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 0.5rem 1.5rem;
            border-radius: 50px;
            font-size: 0.85rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 1rem;
        }

        .section-title {
            font-size: 2.5rem;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 1rem;
        }

        .capabilities-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 2rem;
        }

        .capability-card {
            background: white;
            border-radius: 20px;
            padding: 2.5rem;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 2px solid transparent;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .capability-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 12px 40px rgba(0, 0, 0, 0.12);
        }

        .capability-card.active {
            border-color: #667eea;
            box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
        }

        .capability-icon {
            width: 70px;
            height: 70px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1.5rem;
        }

        .capability-icon.pcb-icon {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        .capability-icon i {
            font-size: 2rem;
            color: white;
        }

        .capability-card h3 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 1rem;
        }

        .capability-list {
            list-style: none;
            padding: 0;
            margin: 0 0 1.5rem 0;
        }

        .capability-list li {
            padding: 0.5rem 0;
            color: #475569;
            display: flex;
            align-items: flex-start;
            gap: 0.75rem;
            line-height: 1.5;
        }

        .capability-list li::before {
            content: '✓';
            color: #667eea;
            font-weight: bold;
            flex-shrink: 0;
        }

        :host ::ng-deep .capability-btn {
            width: 100%;
            justify-content: center;
        }

        :host ::ng-deep .capability-btn.p-button {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            border: none !important;
            color: white !important;
        }

        :host ::ng-deep .capability-btn.p-button-outlined {
            background: transparent !important;
            border: 2px solid #667eea !important;
            color: #667eea !important;
        }

        :host ::ng-deep .capability-btn.p-button-outlined:hover {
            background: rgba(102, 126, 234, 0.1) !important;
        }

        /* ============ REQUEST SECTION ============ */
        .request-section {
            padding: 6rem 2rem;
            background: white;
        }

        .mode-toggle-container {
            display: flex;
            justify-content: center;
            margin-bottom: 2rem;
        }

        :host ::ng-deep .mode-toggle {
            border-radius: 50px !important;
            overflow: hidden;
        }

        :host ::ng-deep .mode-toggle .p-button {
            padding: 1rem 2rem !important;
            border-radius: 0 !important;
            background: #e2e8f0 !important;
            border: none !important;
            color: #475569 !important;
        }

        :host ::ng-deep .mode-toggle .p-button.p-highlight {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            color: white !important;
        }

        :host ::ng-deep .mode-toggle .p-button:focus {
            box-shadow: none !important;
        }

        /* Context Banners (shared styles) */
        .context-banner {
            display: flex;
            align-items: center;
            gap: 1.25rem;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            border: 1px solid rgba(102, 126, 234, 0.3);
            border-radius: 16px;
            padding: 1rem 1.5rem;
            margin-bottom: 2rem;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        }

        .banner-visual {
            width: 80px;
            height: 64px;
            flex-shrink: 0;
            border-radius: 10px;
            overflow: hidden;
            position: relative;
        }

        .banner-content {
            flex: 1;
            min-width: 0;
        }

        .banner-title {
            font-size: 1.05rem;
            font-weight: 600;
            color: #ffffff;
            margin: 0 0 0.35rem 0;
            line-height: 1.3;
        }

        .banner-desc {
            font-size: 0.875rem;
            color: #94a3b8;
            margin: 0;
            line-height: 1.5;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
        }

        /* CNC Routing Banner Visual */
        .routing-visual {
            width: 100%;
            height: 100%;
            background: linear-gradient(135deg, #374151 0%, #1f2937 100%);
            border-radius: 10px;
            position: relative;
            overflow: hidden;
        }

        .routing-bed {
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            height: 20px;
            background: linear-gradient(180deg, #4b5563 0%, #374151 100%);
        }

        .routing-workpiece {
            position: absolute;
            bottom: 20px;
            left: 10px;
            right: 10px;
            height: 28px;
            background: linear-gradient(180deg, #d97706 0%, #b45309 100%);
            border-radius: 3px;
        }

        .routing-cutout {
            position: absolute;
            background: #374151;
            border-radius: 2px;
        }

        .routing-cutout-1 {
            width: 12px;
            height: 12px;
            top: 8px;
            left: 8px;
            border-radius: 50%;
        }

        .routing-cutout-2 {
            width: 18px;
            height: 8px;
            top: 10px;
            right: 8px;
            border-radius: 4px;
        }

        .routing-pocket {
            position: absolute;
            width: 20px;
            height: 14px;
            background: rgba(55, 65, 81, 0.6);
            border: 1px solid rgba(55, 65, 81, 0.8);
            border-radius: 3px;
            top: 7px;
            left: 50%;
            transform: translateX(-50%);
        }

        .routing-gantry {
            position: absolute;
            top: 4px;
            left: 50%;
            transform: translateX(-50%);
            width: 24px;
            height: 8px;
            background: linear-gradient(180deg, #9ca3af 0%, #6b7280 100%);
            border-radius: 2px;
            animation: routingMove 2s ease-in-out infinite;
        }

        .routing-spindle {
            position: absolute;
            top: 100%;
            left: 50%;
            transform: translateX(-50%);
            width: 4px;
            height: 16px;
            background: linear-gradient(180deg, #d1d5db 0%, #9ca3af 100%);
            border-radius: 0 0 2px 2px;
        }

        .routing-dust {
            position: absolute;
            top: 28px;
            left: 50%;
            transform: translateX(-50%);
            width: 16px;
            height: 8px;
            background: radial-gradient(ellipse, rgba(217, 119, 6, 0.4) 0%, transparent 70%);
            animation: dustPulse 1s ease-in-out infinite;
        }

        @keyframes routingMove {
            0%, 100% { left: 35%; }
            50% { left: 65%; }
        }

        @keyframes dustPulse {
            0%, 100% { opacity: 0.6; transform: translateX(-50%) scale(1); }
            50% { opacity: 1; transform: translateX(-50%) scale(1.2); }
        }

        /* PCB Banner Visual (improved) */
        .pcb-visual {
            width: 100%;
            height: 100%;
            border-radius: 10px;
            position: relative;
            overflow: hidden;
        }

        .pcb-board {
            width: 100%;
            height: 100%;
            background: linear-gradient(145deg, #065f46 0%, #047857 40%, #059669 100%);
            position: relative;
        }

        .pcb-copper-layer {
            position: absolute;
            inset: 0;
            background: 
                repeating-linear-gradient(0deg, transparent, transparent 3px, rgba(4, 120, 87, 0.3) 3px, rgba(4, 120, 87, 0.3) 4px),
                repeating-linear-gradient(90deg, transparent, transparent 3px, rgba(4, 120, 87, 0.3) 3px, rgba(4, 120, 87, 0.3) 4px);
        }

        .pcb-trace-h {
            position: absolute;
            height: 2px;
            background: linear-gradient(90deg, #c9a227 0%, #dbb42c 50%, #c9a227 100%);
            box-shadow: 0 0 2px rgba(219, 180, 44, 0.5);
        }

        .pcb-trace-h1 { width: 55%; top: 15%; left: 5%; }
        .pcb-trace-h2 { width: 40%; top: 50%; right: 5%; }
        .pcb-trace-h3 { width: 30%; bottom: 20%; left: 15%; }

        .pcb-trace-v {
            position: absolute;
            width: 2px;
            background: linear-gradient(180deg, #c9a227 0%, #dbb42c 50%, #c9a227 100%);
            box-shadow: 0 0 2px rgba(219, 180, 44, 0.5);
        }

        .pcb-trace-v1 { height: 35%; top: 15%; left: 58%; }
        .pcb-trace-v2 { height: 25%; top: 50%; right: 20%; }
        .pcb-trace-v3 { height: 30%; bottom: 20%; left: 43%; }

        .pcb-pad {
            position: absolute;
            width: 6px;
            height: 6px;
            background: radial-gradient(circle, #dbb42c 40%, #a78920 100%);
            border-radius: 50%;
            box-shadow: 0 0 3px rgba(219, 180, 44, 0.6);
        }

        .pcb-pad-1 { top: 12%; left: 3%; }
        .pcb-pad-2 { top: 12%; left: 58%; }
        .pcb-pad-3 { top: 47%; right: 3%; }
        .pcb-pad-4 { top: 47%; right: 20%; }
        .pcb-pad-5 { bottom: 17%; left: 13%; }
        .pcb-pad-6 { bottom: 17%; left: 43%; }

        .pcb-ic {
            position: absolute;
            width: 22px;
            height: 16px;
            background: linear-gradient(180deg, #1f2937 0%, #111827 100%);
            border-radius: 2px;
            top: 50%;
            left: 25%;
            transform: translateY(-50%);
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.4);
        }

        .pcb-ic-pins-left,
        .pcb-ic-pins-right {
            position: absolute;
            top: 2px;
            bottom: 2px;
            width: 3px;
            background: repeating-linear-gradient(
                180deg,
                #9ca3af 0px,
                #9ca3af 2px,
                transparent 2px,
                transparent 4px
            );
        }

        .pcb-ic-pins-left { left: -3px; }
        .pcb-ic-pins-right { right: -3px; }

        .pcb-ic-dot {
            position: absolute;
            top: 3px;
            left: 3px;
            width: 3px;
            height: 3px;
            background: #6b7280;
            border-radius: 50%;
        }

        .pcb-smd {
            position: absolute;
            background: linear-gradient(180deg, #374151 0%, #1f2937 100%);
            border-radius: 1px;
        }

        .pcb-smd-1 {
            width: 8px;
            height: 4px;
            top: 25%;
            right: 15%;
        }

        .pcb-smd-2 {
            width: 6px;
            height: 3px;
            bottom: 35%;
            right: 30%;
        }

        .pcb-smd-3 {
            width: 10px;
            height: 5px;
            bottom: 30%;
            left: 60%;
        }

        .request-form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 3rem;
        }

        .form-section {
            background: #f8fafc;
            border-radius: 16px;
            padding: 1.5rem;
            margin-bottom: 1.5rem;
        }

        .form-section-title {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            font-size: 1.1rem;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 1rem;
        }

        .form-section-title i {
            color: #667eea;
        }

        .form-hint {
            color: #64748b;
            font-size: 0.9rem;
            margin-bottom: 1rem;
        }

        /* Design Source Toggle */
        .design-source-toggle {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-bottom: 1rem;
            flex-wrap: wrap;
        }

        .design-source-label {
            font-size: 0.85rem;
            font-weight: 500;
            color: #475569;
        }

        .design-source-options {
            display: flex;
            gap: 0.5rem;
        }

        .source-option {
            display: flex;
            align-items: center;
            gap: 0.4rem;
            padding: 0.4rem 0.75rem;
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            font-size: 0.8rem;
            color: #475569;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .source-option:hover {
            border-color: #667eea;
            color: #667eea;
        }

        .source-option.selected {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-color: transparent;
            color: white;
        }

        .source-option i {
            font-size: 0.85rem;
        }

        /* Processing Info Banner */
        .processing-info-banner {
            display: flex;
            align-items: flex-start;
            gap: 0.6rem;
            background: rgba(102, 126, 234, 0.08);
            border: 1px solid rgba(102, 126, 234, 0.2);
            border-radius: 8px;
            padding: 0.65rem 0.85rem;
            margin-top: 0.75rem;
            font-size: 0.8rem;
            color: #5b6b9a;
            line-height: 1.4;
        }

        .processing-info-banner i {
            color: #667eea;
            font-size: 0.9rem;
            margin-top: 1px;
            flex-shrink: 0;
        }

        /* Operations Grid */
        .operations-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1rem;
        }

        .operation-card {
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            padding: 1rem;
            cursor: pointer;
            transition: all 0.2s ease;
            text-align: center;
        }

        .operation-card:hover {
            border-color: #667eea;
        }

        .operation-card.selected {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.05);
        }

        .operation-card i {
            font-size: 1.5rem;
            color: #667eea;
            margin-bottom: 0.5rem;
            display: block;
        }

        .op-label {
            display: block;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 0.25rem;
        }

        .op-helper {
            display: block;
            font-size: 0.75rem;
            color: #64748b;
            line-height: 1.4;
        }

        /* Disabled Operation Card */
        .operation-card.disabled {
            opacity: 0.5;
            cursor: not-allowed;
            background: #f8fafc;
            position: relative;
        }

        .operation-card.disabled:hover {
            border-color: #e2e8f0;
        }

        .operation-card .disabled-icon {
            position: absolute;
            top: 0.5rem;
            right: 0.5rem;
            font-size: 0.75rem;
            color: #ef4444;
        }

        /* Material Dropdown */
        .material-dropdown {
            width: 100%;
        }

        :host ::ng-deep .material-dropdown .p-dropdown {
            width: 100%;
        }

        .material-option {
            display: flex;
            justify-content: space-between;
            align-items: center;
            width: 100%;
        }

        .material-option .thickness-range {
            font-size: 0.75rem;
            color: #64748b;
            background: #f1f5f9;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
        }

        .material-notes {
            display: flex;
            align-items: flex-start;
            gap: 0.5rem;
            margin-top: 0.75rem;
            padding: 0.75rem;
            background: rgba(102, 126, 234, 0.08);
            border-radius: 8px;
            font-size: 0.85rem;
            color: #475569;
        }

        .material-notes i {
            color: #667eea;
            flex-shrink: 0;
            margin-top: 0.1rem;
        }

        /* Depth Mode Toggle */
        .depth-mode-toggle {
            display: flex;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .depth-option {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem;
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .depth-option:hover {
            border-color: #667eea;
        }

        .depth-option.selected {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.05);
        }

        .depth-option i {
            font-size: 1.25rem;
            color: #667eea;
        }

        .custom-depth-input {
            background: #f8fafc;
            padding: 1rem;
            border-radius: 8px;
        }

        .custom-depth-input label {
            display: block;
            font-size: 0.85rem;
            color: #475569;
            margin-bottom: 0.5rem;
        }

        .custom-depth-input :host ::ng-deep .p-inputnumber {
            width: 100%;
        }

        .depth-hint {
            display: block;
            margin-top: 0.5rem;
            font-size: 0.75rem;
            color: #64748b;
        }

        .depth-error {
            display: block;
            margin-top: 0.5rem;
            font-size: 0.75rem;
            color: #ef4444;
            font-weight: 500;
        }

        .thickness-limit-hint {
            display: block;
            margin-top: 0.25rem;
            font-size: 0.7rem;
            color: #667eea;
            font-weight: 500;
        }

        /* PCB Material Grid */
        .pcb-material-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1rem;
        }

        .pcb-material-card {
            padding: 1rem;
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .pcb-material-card:hover {
            border-color: #667eea;
            transform: translateY(-2px);
        }

        .pcb-material-card.selected {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.05);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
        }

        .pcb-material-card .material-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 0.5rem;
        }

        .pcb-material-card .material-name {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1e293b;
        }

        .pcb-material-card .selected-icon {
            color: #667eea;
            font-size: 1.2rem;
        }

        .pcb-material-card .material-desc {
            font-size: 0.8rem;
            color: #64748b;
            margin: 0;
            line-height: 1.4;
        }

        /* PCB Thickness Options */
        .pcb-thickness-options {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
        }

        .thickness-option {
            padding: 0.6rem 1.2rem;
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .thickness-option:hover {
            border-color: #667eea;
        }

        .thickness-option.selected {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.1);
        }

        .thickness-option .thickness-value {
            font-size: 0.9rem;
            font-weight: 600;
            color: #1e293b;
        }

        .thickness-option.selected .thickness-value {
            color: #667eea;
        }

        .pcb-thickness-hint {
            margin-top: 0.75rem;
            font-size: 0.75rem;
            color: #64748b;
        }

        /* Double-Sided Warning Banner */
        .double-sided-warning {
            display: flex;
            align-items: flex-start;
            gap: 0.75rem;
            margin-top: 1rem;
            padding: 0.875rem 1rem;
            background: linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(217, 119, 6, 0.08) 100%);
            border: 1px solid rgba(245, 158, 11, 0.3);
            border-radius: 8px;
        }

        .double-sided-warning i {
            color: #d97706;
            font-size: 1rem;
            margin-top: 2px;
        }

        .double-sided-warning span {
            font-size: 0.85rem;
            color: #92400e;
            line-height: 1.5;
        }

        /* PCB Side Toggle */
        .pcb-side-toggle {
            display: flex;
            gap: 1rem;
        }

        .side-option {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
            padding: 1rem;
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .side-option:hover {
            border-color: #667eea;
        }

        .side-option.selected {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.05);
        }

        .side-option i {
            font-size: 1.5rem;
            color: #667eea;
        }

        /* Size Workspace Visual */
        .size-workspace {
            display: flex;
            gap: 1.5rem;
            align-items: flex-start;
        }

        .workspace-visual {
            flex: 0 0 140px;
        }

        .workspace-grid {
            width: 140px;
            height: 140px;
            background: linear-gradient(135deg, #1e1e2e 0%, #2d2d44 100%);
            border: 2px solid #667eea;
            border-radius: 8px;
            position: relative;
            display: flex;
            align-items: flex-end;
            justify-content: flex-start;
            padding: 4px;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
            overflow: hidden;
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
            position: relative;
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
            font-size: 0.7rem;
            color: #64748b;
            margin-top: 0.5rem;
        }

        .size-inputs {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .size-input-row {
            display: flex;
            gap: 1rem;
        }

        .size-input-group {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }

        .size-input-group label {
            font-size: 0.8rem;
            color: #475569;
            font-weight: 500;
        }

        :host ::ng-deep .size-number-input {
            width: 100%;
        }

        :host ::ng-deep .size-number-input .p-inputnumber-input {
            width: 100%;
            text-align: center;
            font-size: 0.9rem;
            padding: 0.5rem;
        }

        :host ::ng-deep .size-number-input .p-inputnumber-button {
            width: 32px;
        }

        .size-presets-inline {
            margin-top: 0.5rem;
        }

        .size-presets-inline .presets-label {
            display: block;
            font-size: 0.75rem;
            color: #64748b;
            margin-bottom: 0.35rem;
        }

        .preset-buttons {
            display: flex;
            flex-wrap: wrap;
            gap: 0.4rem;
        }

        /* RTL Support for Workspace Grid */
        [dir="rtl"] .workspace-grid {
            justify-content: flex-end;
        }

        [dir="rtl"] .size-workspace {
            flex-direction: row-reverse;
        }

        .dimensions-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1rem;
        }

        .dimension-input label {
            display: block;
            font-size: 0.85rem;
            color: #475569;
            margin-bottom: 0.5rem;
        }

        .dimension-input :host ::ng-deep .p-inputnumber {
            width: 100%;
        }

        .size-presets {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-top: 1rem;
            flex-wrap: wrap;
        }

        .presets-label {
            font-size: 0.85rem;
            color: #64748b;
        }

        .preset-btn {
            padding: 0.4rem 0.8rem;
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            font-size: 0.8rem;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .preset-btn:hover {
            border-color: #667eea;
            color: #667eea;
        }

        /* Upload Zone */
        .upload-zone {
            border: 2px dashed #cbd5e1;
            border-radius: 12px;
            padding: 2rem;
            text-align: center;
            cursor: pointer;
            transition: all 0.2s ease;
            background: white;
        }

        .upload-zone:hover,
        .upload-zone.dragover {
            border-color: #667eea;
            background: rgba(102, 126, 234, 0.05);
        }

        .upload-icon {
            font-size: 2.5rem;
            color: #667eea;
            margin-bottom: 1rem;
            display: block;
        }

        .upload-text {
            display: block;
            color: #475569;
            margin-bottom: 0.5rem;
        }

        .upload-formats {
            display: block;
            font-size: 0.85rem;
            color: #94a3b8;
        }

        .uploaded-files {
            margin-top: 1rem;
        }

        .uploaded-file {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem;
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            margin-bottom: 0.5rem;
        }

        .uploaded-file i {
            color: #667eea;
        }

        .file-name {
            flex: 1;
            font-size: 0.9rem;
            color: #1e293b;
        }

        .file-size {
            font-size: 0.8rem;
            color: #94a3b8;
        }

        .remove-file {
            background: none;
            border: none;
            color: #ef4444;
            cursor: pointer;
            padding: 0.25rem;
        }

        .design-notes {
            margin-top: 1rem;
        }

        .design-notes label {
            display: block;
            font-size: 0.85rem;
            color: #475569;
            margin-bottom: 0.5rem;
        }

        .design-notes textarea {
            width: 100%;
        }

        /* Contact Form */
        .contact-form {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1rem;
        }

        .form-field {
            display: flex;
            flex-direction: column;
        }

        .form-field.full-width {
            grid-column: 1 / -1;
        }

        .form-field label {
            font-size: 0.85rem;
            color: #475569;
            margin-bottom: 0.5rem;
        }

        .form-field input,
        .form-field textarea {
            width: 100%;
        }

        /* Submit Section */
        .submit-section {
            text-align: center;
            padding: 2rem 0;
        }

        :host ::ng-deep .submit-btn {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            border: none !important;
            padding: 1rem 3rem !important;
            border-radius: 50px !important;
            font-weight: 600 !important;
            font-size: 1.1rem !important;
        }

        .disclaimer {
            margin-top: 1rem;
            font-size: 0.85rem;
            color: #94a3b8;
        }

        /* ============ FILE REQ SECTION ============ */
        .file-req-section {
            padding: 5rem 2rem;
            background: #f8fafc;
        }

        .file-req-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 2rem;
        }

        .file-req-card {
            background: white;
            border-radius: 16px;
            padding: 2rem;
            border: 1px solid #e2e8f0;
        }

        .file-req-card h4 {
            font-size: 1.2rem;
            font-weight: 600;
            color: #1e293b;
            margin-bottom: 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 2px solid #667eea;
        }

        .file-req-card ul {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .file-req-card li {
            padding: 0.5rem 0;
            color: #475569;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .file-req-card li::before {
            content: '•';
            color: #667eea;
            font-weight: bold;
        }

        /* ============ CTA SECTION ============ */
        .cta-section {
            padding: 5rem 2rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        .cta-content {
            text-align: center;
            color: white;
        }

        .cta-content h2 {
            font-size: 2.5rem;
            font-weight: 700;
            margin-bottom: 1rem;
        }

        .cta-content p {
            font-size: 1.2rem;
            opacity: 0.9;
            margin-bottom: 2rem;
        }

        :host ::ng-deep .cta-button {
            background: white !important;
            color: #667eea !important;
            padding: 1rem 2.5rem !important;
            border-radius: 50px !important;
            font-weight: 600 !important;
            border: none !important;
        }

        :host ::ng-deep .cta-button:hover {
            background: #f1f5f9 !important;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 1024px) {
            .hero-inner {
                grid-template-columns: 1fr;
                text-align: center;
            }

            .hero-cta {
                justify-content: center;
            }

            .constraints-row {
                justify-content: center;
            }

            .cnc-visual-wrap {
                display: none;
            }

            .capabilities-grid {
                grid-template-columns: 1fr;
            }

            .request-form-grid {
                grid-template-columns: 1fr;
            }
        }

        /* RTL Support for Workspace */
        [dir="rtl"] .bed-surface {
            justify-content: flex-end;
        }

        @media (max-width: 768px) {
            .hero-title {
                font-size: 2rem;
            }

            .operations-grid {
                grid-template-columns: 1fr;
            }

            .dimensions-grid {
                grid-template-columns: 1fr;
            }

            .contact-form {
                grid-template-columns: 1fr;
            }

            .file-req-grid {
                grid-template-columns: 1fr;
            }

            .size-workspace {
                flex-direction: column;
            }

            .workspace-visual {
                flex: none;
                align-self: center;
            }

            .size-input-row {
                flex-direction: column;
            }
        }
    `]
})
export class CncLandingComponent implements OnInit {
    languageService = inject(LanguageService);
    translateService = inject(TranslateService);
    messageService = inject(MessageService);
    cncApiService = inject(CncApiService);
    private authService = inject(AuthService);
    private router = inject(Router);

    // Mode
    selectedModeValue: CncMode = 'routing';
    selectedMode = signal<CncMode>('routing');
    
    modeOptions = [
        { label: 'CNC Routing', value: 'routing' },
        { label: 'PCB Milling', value: 'pcb' }
    ];

    // Material Selection
    selectedMaterialId: string | null = null;
    selectedMaterial = signal<CncMaterial | null>(null);
    operationAvailabilities = signal<CncOperationAvailability[]>([]);
    
    materialOptions = computed(() => {
        const materials = this.cncApiService.routingMaterials();
        const lang = this.languageService.language;
        return materials.map(m => ({
            value: m.id,
            label: this.cncApiService.getLocalizedName(m, lang),
            thicknessRange: this.cncApiService.getThicknessRange(m),
            material: m
        }));
    });

    // Operations
    selectedRoutingOperation = signal<RoutingOperation>('cut');
    selectedPcbOperation = signal<PcbOperation>('isolation');
    selectedPcbSide = signal<PcbSide>('single');
    designSourceType = signal<DesignSourceType>('production');

    // PCB Material & Thickness
    selectedPcbMaterial = signal<'FR4' | 'FR1'>('FR4');
    selectedPcbThickness = signal<number>(1.6);

    pcbMaterialOptions = [
        { 
            value: 'FR4' as const, 
            labelEn: 'FR4', 
            labelAr: 'FR4',
            descriptionEn: 'Standard fiberglass PCB. Durable, heat-resistant.',
            descriptionAr: 'لوح فايبرجلاس قياسي. متين ومقاوم للحرارة.',
            minThickness: 1.0,
            maxThickness: 2.0,
            availableThicknesses: [1.0, 1.2, 1.6, 2.0]
        },
        { 
            value: 'FR1' as const, 
            labelEn: 'FR1', 
            labelAr: 'FR1',
            descriptionEn: 'Paper-based phenolic. Cost-effective for prototypes.',
            descriptionAr: 'فينوليك ورقي. اقتصادي للنماذج الأولية.',
            minThickness: 1.0,
            maxThickness: 1.6,
            availableThicknesses: [1.0, 1.2, 1.6]
        }
    ];

    pcbThicknessOptions = [
        { value: 1.0, label: '1.0 mm' },
        { value: 1.2, label: '1.2 mm' },
        { value: 1.6, label: '1.6 mm (Standard)' },
        { value: 2.0, label: '2.0 mm' }
    ];

    routingOperations = [
        { value: 'cut' as RoutingOperation, label: 'Cut', icon: 'pi pi-minus', helper: '' },
        { value: 'engrave' as RoutingOperation, label: 'Engrave', icon: 'pi pi-pencil', helper: '' },
        { value: 'pocket' as RoutingOperation, label: 'Pocket', icon: 'pi pi-stop', helper: '' },
        { value: 'drill' as RoutingOperation, label: 'Drill', icon: 'pi pi-circle', helper: '' }
    ];

    pcbOperations = [
        { value: 'isolation' as PcbOperation, label: 'Isolation Routing', icon: 'pi pi-sitemap', helper: '' },
        { value: 'pcbDrill' as PcbOperation, label: 'PCB Drilling', icon: 'pi pi-circle', helper: '' },
        { value: 'cutout' as PcbOperation, label: 'Board Cutout', icon: 'pi pi-stop', helper: '' },
        { value: 'text' as PcbOperation, label: 'Text/Silkscreen', icon: 'pi pi-align-left', helper: '' }
    ];

    // Depth Mode
    depthMode = signal<CncDepthMode>('through');
    customDepth = 3;
    depthError = '';

    // Dimensions
    width = 100;
    height = 100;
    thickness = 3;
    quantity = 1;

    // Upload
    uploadedFiles: File[] = [];
    isDragging = false;
    designNotes = '';

    // Customer
    customerName = '';
    customerEmail = '';
    customerPhone = '';
    projectDescription = '';

    isSubmitting = false;

    constructor() {
        // Reactive effect: update operations when material or thickness changes
        effect(() => {
            const material = this.selectedMaterial();
            const availabilities = this.cncApiService.getAvailableOperations(material, this.thickness);
            this.operationAvailabilities.set(availabilities);
            
            // Check if current operation is still valid
            const currentOp = this.selectedRoutingOperation();
            const currentAvail = availabilities.find(a => a.operation === currentOp);
            if (currentAvail && !currentAvail.available) {
                // Auto-select first available operation
                const firstAvailable = this.cncApiService.getFirstAvailableOperation(material, this.thickness);
                this.selectedRoutingOperation.set(firstAvailable as RoutingOperation);
                
                this.messageService.add({
                    severity: 'info',
                    summary: this.languageService.language === 'ar' ? 'تحديث' : 'Updated',
                    detail: this.languageService.language === 'ar' 
                        ? 'تم تحديث نوع العملية بناءً على قيود المادة المحددة' 
                        : 'Operation updated based on selected material constraints'
                });
            }
        });
    }

    get acceptedFormats(): string {
        const mode = this.selectedMode();
        const source = this.designSourceType();
        
        if (mode === 'routing') {
            return source === 'production' 
                ? '.svg,.dxf,.pdf' 
                : '.png,.jpg,.jpeg,.webp,.pdf';
        } else {
            return source === 'production' 
                ? '.zip,.gbr,.gtl,.gbl,.drl' 
                : '.png,.jpg,.jpeg,.pdf';
        }
    }

    getUploadHint(): string {
        const mode = this.selectedMode();
        const source = this.designSourceType();
        const isAr = this.languageService.language === 'ar';
        
        if (mode === 'routing') {
            if (source === 'production') {
                return isAr ? 'ارفع ملف التصميم (SVG, DXF, PDF)' : 'Upload your design file (SVG, DXF, PDF)';
            } else {
                return isAr ? 'ارفع صورة أو رسم تخطيطي (PNG, JPG, PDF)' : 'Upload an image or sketch (PNG, JPG, PDF)';
            }
        } else {
            if (source === 'production') {
                return isAr ? 'ارفع ملفات Gerber (.zip أو ملفات فردية)' : 'Upload your Gerber files (.zip or individual files)';
            } else {
                return isAr ? 'ارفع صورة أو مخطط (PNG, JPG, PDF)' : 'Upload an image or schematic (PNG, JPG, PDF)';
            }
        }
    }

    getUploadFormatsText(): string {
        const mode = this.selectedMode();
        const source = this.designSourceType();
        
        if (mode === 'routing') {
            return source === 'production' ? 'SVG, DXF, PDF' : 'PNG, JPG, WEBP, PDF';
        } else {
            return source === 'production' ? 'ZIP (Gerber), GBR, GTL, GBL, DRL' : 'PNG, JPG, PDF';
        }
    }

    setDesignSource(source: DesignSourceType) {
        this.designSourceType.set(source);
        this.uploadedFiles = [];
    }

    ngOnInit() {
        this.updateLabels();
        this.translateService.onLangChange.subscribe(() => {
            this.updateLabels();
        });
        
        // Load materials from API
        this.cncApiService.loadMaterialsIfNeeded().subscribe();
    }

    updateLabels() {
        const isAr = this.languageService.language === 'ar';
        
        this.modeOptions = [
            { label: isAr ? 'توجيه CNC' : 'CNC Routing', value: 'routing' },
            { label: isAr ? 'تفريز PCB' : 'PCB Milling', value: 'pcb' }
        ];

        this.routingOperations = [
            { value: 'cut', label: isAr ? 'قطع' : 'Cut', icon: 'pi pi-minus', helper: isAr ? 'قطع كامل' : 'Through-cut' },
            { value: 'engrave', label: isAr ? 'نقش' : 'Engrave', icon: 'pi pi-pencil', helper: isAr ? 'علامات سطحية' : 'Surface marking' },
            { value: 'pocket', label: isAr ? 'جيب' : 'Pocket', icon: 'pi pi-stop', helper: isAr ? 'إزالة المواد' : 'Material removal' },
            { value: 'drill', label: isAr ? 'حفر' : 'Drill', icon: 'pi pi-circle', helper: isAr ? 'ثقوب' : 'Holes' }
        ];

        this.pcbOperations = [
            { value: 'isolation', label: isAr ? 'عزل المسارات' : 'Isolation Routing', icon: 'pi pi-sitemap', helper: isAr ? 'فصل النحاس' : 'Trace separation' },
            { value: 'pcbDrill', label: isAr ? 'حفر PCB' : 'PCB Drilling', icon: 'pi pi-circle', helper: isAr ? 'ثقوب المكونات' : 'Component holes' },
            { value: 'cutout', label: isAr ? 'قطع اللوحة' : 'Board Cutout', icon: 'pi pi-stop', helper: isAr ? 'حدود اللوحة' : 'Board outline' },
            { value: 'text', label: isAr ? 'نص/سيلك' : 'Text/Silkscreen', icon: 'pi pi-align-left', helper: isAr ? 'طباعة النص' : 'Text milling' }
        ];
    }

    onModeChange(event: any) {
        this.selectedMode.set(event.value);
    }

    selectMode(mode: CncMode) {
        this.selectedModeValue = mode;
        this.selectedMode.set(mode);
    }

    selectModeAndScroll(mode: CncMode) {
        this.selectMode(mode);
        setTimeout(() => this.scrollToSection('request-section'), 100);
    }

    selectRoutingOperation(op: RoutingOperation) {
        this.selectedRoutingOperation.set(op);
        // Reset depth mode when changing operation
        if (op !== 'cut' && op !== 'engrave') {
            this.depthMode.set('through');
        }
    }

    selectRoutingOperationIfAvailable(op: RoutingOperation) {
        if (this.isOperationAvailable(op)) {
            this.selectRoutingOperation(op);
        }
    }
    
    // Material Selection Methods
    onMaterialChange(event: any) {
        const materialId = event.value;
        if (materialId) {
            const option = this.materialOptions().find(m => m.value === materialId);
            if (option) {
                this.selectedMaterial.set(option.material);
                // Clamp thickness to material limits
                const clamped = this.cncApiService.clampThickness(option.material, this.thickness);
                if (clamped !== this.thickness) {
                    this.thickness = clamped;
                    this.messageService.add({
                        severity: 'info',
                        summary: this.languageService.language === 'ar' ? 'تعديل' : 'Adjusted',
                        detail: this.languageService.language === 'ar' 
                            ? `تم تعديل السمك إلى ${clamped} مم بناءً على حدود المادة` 
                            : `Thickness adjusted to ${clamped}mm based on material limits`
                    });
                }
            }
        } else {
            this.selectedMaterial.set(null);
        }
    }
    
    getMaterialNotes(): string | undefined {
        const material = this.selectedMaterial();
        if (!material) return undefined;
        return this.cncApiService.getLocalizedNotes(material, this.languageService.language);
    }
    
    // Operation Availability Methods
    isOperationAvailable(op: RoutingOperation): boolean {
        const availability = this.operationAvailabilities().find(a => a.operation === op);
        return availability?.available ?? true;
    }
    
    getOperationTooltip(op: RoutingOperation): string {
        const availability = this.operationAvailabilities().find(a => a.operation === op);
        if (availability && !availability.available && availability.reason) {
            return availability.reason;
        }
        return '';
    }
    
    // Depth Mode Methods
    showDepthMode(): boolean {
        const op = this.selectedRoutingOperation();
        return op === 'cut' || op === 'engrave';
    }
    
    setDepthMode(mode: CncDepthMode) {
        this.depthMode.set(mode);
        if (mode === 'through') {
            this.customDepth = this.thickness;
            this.depthError = '';
        }
    }
    
    getMaxDepth(): number {
        const material = this.selectedMaterial();
        const op = this.selectedRoutingOperation();
        if (material) {
            return this.cncApiService.getMaxDepthForOperation(material, op as CncOperationType, this.thickness);
        }
        return this.thickness;
    }
    
    validateDepth() {
        const material = this.selectedMaterial();
        const op = this.selectedRoutingOperation() as CncOperationType;
        const validation = this.cncApiService.validateDepth(material, op, this.customDepth, this.thickness);
        
        if (!validation.valid) {
            this.depthError = validation.error || '';
        } else {
            this.depthError = '';
        }
    }
    
    // Thickness Constraint Methods
    getMinThickness(): number {
        const material = this.selectedMaterial();
        if (material && material.minThicknessMm !== null) {
            return material.minThicknessMm;
        }
        return 0.5; // Default min
    }
    
    getMaxThickness(): number {
        const material = this.selectedMaterial();
        if (material && material.maxThicknessMm !== null) {
            return material.maxThicknessMm;
        }
        return 10; // Default max
    }
    
    onThicknessChange() {
        const material = this.selectedMaterial();
        if (material) {
            // Clamp thickness to material limits
            const clamped = this.cncApiService.clampThickness(material, this.thickness);
            if (clamped !== this.thickness) {
                this.thickness = clamped;
            }
        }
        
        // Update depth if in through mode
        if (this.depthMode() === 'through') {
            this.customDepth = this.thickness;
        }
        
        // Revalidate depth if in custom mode
        if (this.depthMode() === 'custom') {
            this.validateDepth();
        }
    }

    selectPcbOperation(op: PcbOperation) {
        this.selectedPcbOperation.set(op);
    }

    selectPcbSide(side: PcbSide) {
        this.selectedPcbSide.set(side);
    }

    selectPcbMaterial(material: 'FR4' | 'FR1') {
        this.selectedPcbMaterial.set(material);
        
        // Check if current thickness is valid for the new material
        const materialOption = this.pcbMaterialOptions.find(m => m.value === material);
        if (materialOption) {
            const currentThickness = this.selectedPcbThickness();
            if (!materialOption.availableThicknesses.includes(currentThickness as any)) {
                // Select the default thickness (1.6mm if available, otherwise the highest available)
                const defaultThickness = materialOption.availableThicknesses.includes(1.6) 
                    ? 1.6 
                    : materialOption.availableThicknesses[materialOption.availableThicknesses.length - 1];
                this.selectedPcbThickness.set(defaultThickness);
                
                this.messageService.add({
                    severity: 'info',
                    summary: this.languageService.language === 'ar' ? 'تم تعديل السُمك' : 'Thickness Adjusted',
                    detail: this.languageService.language === 'ar' 
                        ? `تم اختيار ${defaultThickness} مم بناءً على المادة المحددة.`
                        : `Selected ${defaultThickness}mm based on material constraints.`,
                    life: 3000
                });
            }
        }
    }

    selectPcbThickness(thickness: number) {
        this.selectedPcbThickness.set(thickness);
    }

    getAvailablePcbThicknesses() {
        const material = this.pcbMaterialOptions.find(m => m.value === this.selectedPcbMaterial());
        if (material) {
            return this.pcbThicknessOptions.filter(t => material.availableThicknesses.includes(t.value as any));
        }
        return this.pcbThicknessOptions;
    }

    getPcbDepthForOperation(): number {
        const thickness = this.selectedPcbThickness();
        const operation = this.selectedPcbOperation();
        
        switch (operation) {
            case 'isolation':
                return 0.15; // Fixed shallow depth for isolation routing
            case 'pcbDrill':
            case 'cutout':
                return thickness; // Full board thickness
            case 'text':
                return 0.1; // Shallow engraving for silkscreen
            default:
                return thickness;
        }
    }

    setPreset(w: number, h: number) {
        this.width = w;
        this.height = h;
    }

    scrollToSection(sectionId: string) {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth' });
        }
    }

    // File handling
    onDragOver(event: DragEvent) {
        event.preventDefault();
        this.isDragging = true;
    }

    onDragLeave(event: DragEvent) {
        event.preventDefault();
        this.isDragging = false;
    }

    onDrop(event: DragEvent) {
        event.preventDefault();
        this.isDragging = false;
        const files = event.dataTransfer?.files;
        if (files) {
            this.addFiles(files);
        }
    }

    onFileSelect(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files) {
            this.addFiles(input.files);
        }
    }

    addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            this.uploadedFiles.push(files[i]);
        }
    }

    removeFile(index: number) {
        this.uploadedFiles.splice(index, 1);
    }

    formatFileSize(bytes: number): string {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    isFormValid(): boolean {
        return !!(this.customerName && this.customerEmail && this.width > 0 && this.height > 0);
    }

    async submitRequest() {
        if (!this.authService.authenticated) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url || '/cnc' } });
            return;
        }
        if (!this.isFormValid()) {
            this.messageService.add({
                severity: 'warn',
                summary: this.languageService.language === 'ar' ? 'تحذير' : 'Warning',
                detail: this.languageService.language === 'ar' ? 'يرجى ملء جميع الحقول المطلوبة' : 'Please fill all required fields'
            });
            return;
        }

        this.isSubmitting = true;

        // Simulate API call
        setTimeout(() => {
            this.isSubmitting = false;
            this.messageService.add({
                severity: 'success',
                summary: this.languageService.language === 'ar' ? 'تم الإرسال' : 'Submitted',
                detail: this.languageService.language === 'ar' ? 'تم إرسال طلبك بنجاح. سنتواصل معك قريباً.' : 'Your request has been submitted. We will contact you soon.'
            });

            // Reset form
            this.uploadedFiles = [];
            this.designNotes = '';
            this.customerName = '';
            this.customerEmail = '';
            this.customerPhone = '';
            this.projectDescription = '';
        }, 1500);
    }
}
