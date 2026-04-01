import { Component, OnInit, inject, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { Print3dDesign, DesignFile } from '../../../shared/models/printing.model';
import { LanguageService } from '../../../shared/services/language.service';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-account-designs',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        CardModule,
        ButtonModule,
        TagModule,
        ToastModule,
        DialogModule,
        TooltipModule,
        ProgressSpinnerModule,
        ConfirmDialogModule,
        TranslateModule
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />
        
        <div class="designs-page">
            <!-- Header Section -->
            <div class="designs-header">
                <div class="header-content">
                    <div class="header-text">
                        <h1 class="page-title">
                            <i class="pi pi-box"></i>
                            {{ 'account.designs.title' | translate }}
                        </h1>
                        <p class="page-subtitle">{{ 'account.designs.subtitle' | translate }}</p>
                    </div>
                    <p-button 
                        [label]="'account.designs.uploadNew' | translate" 
                        icon="pi pi-cloud-upload"
                        styleClass="upload-btn"
                        [routerLink]="['/3d-print/new']">
                    </p-button>
                </div>
                
                <!-- Stats Bar -->
                <div class="stats-bar" *ngIf="!loading && designs.length > 0">
                    <div class="stat-item">
                        <span class="stat-value">{{ designs.length }}</span>
                        <span class="stat-label">{{ 'account.designs.totalDesigns' | translate }}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-value">{{ getTotalFiles() }}</span>
                        <span class="stat-label">{{ 'account.designs.totalFiles' | translate }}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-value">{{ formatFileSize(getTotalSize()) }}</span>
                        <span class="stat-label">{{ 'account.designs.totalSize' | translate }}</span>
                    </div>
                </div>
            </div>

            <!-- Loading State -->
            <div *ngIf="loading" class="loading-container">
                <div class="loading-content">
                    <p-progressSpinner strokeWidth="3" animationDuration="1s"></p-progressSpinner>
                    <p class="loading-text">{{ 'account.designs.loading' | translate }}</p>
                </div>
            </div>

            <!-- Empty State -->
            <div *ngIf="!loading && designs.length === 0" class="empty-state">
                <div class="empty-content">
                    <div class="empty-icon">
                        <i class="pi pi-box"></i>
                    </div>
                    <h2>{{ 'account.designs.noDesigns' | translate }}</h2>
                    <p>{{ 'account.designs.noDesignsDescription' | translate }}</p>
                    <p-button 
                        [label]="'account.designs.uploadFirst' | translate" 
                        icon="pi pi-cloud-upload"
                        styleClass="upload-btn-large"
                        [routerLink]="['/3d-print/new']">
                    </p-button>
                </div>
            </div>

            <!-- Designs Grid -->
            <div *ngIf="!loading && designs.length > 0" class="designs-grid">
                <div *ngFor="let design of designs" class="design-card" (click)="openPreview(design)">
                    <!-- Card Preview Area -->
                    <div class="card-preview">
                        <div class="preview-content" *ngIf="design.files && design.files.length > 0">
                            <!-- 3D File Icon with animation -->
                            <div class="file-icon-3d">
                                <div class="cube">
                                    <div class="face front"></div>
                                    <div class="face back"></div>
                                    <div class="face right"></div>
                                    <div class="face left"></div>
                                    <div class="face top"></div>
                                    <div class="face bottom"></div>
                                </div>
                            </div>
                            <div class="file-format-badge">
                                {{ getFileFormat(design) }}
                            </div>
                        </div>
                        <div class="preview-overlay">
                            <i class="pi pi-eye"></i>
                            <span>{{ 'account.designs.preview' | translate }}</span>
                        </div>
                    </div>

                    <!-- Card Content -->
                    <div class="card-content">
                        <div class="design-title">{{ design.title }}</div>
                        <div class="design-meta">
                            <div class="meta-item">
                                <i class="pi pi-file"></i>
                                <span>{{ getPrimaryFileName(design) }}</span>
                            </div>
                            <div class="meta-item">
                                <i class="pi pi-database"></i>
                                <span>{{ formatFileSize(getDesignSize(design)) }}</span>
                            </div>
                            <div class="meta-item">
                                <i class="pi pi-calendar"></i>
                                <span>{{ formatDate(design.createdAt) }}</span>
                            </div>
                        </div>
                        
                        <!-- Tags -->
                        <div class="design-tags" *ngIf="design.files && design.files.length > 0">
                            <p-tag 
                                [value]="design.files.length + ' ' + ('account.designs.files' | translate)" 
                                severity="info"
                                [rounded]="true">
                            </p-tag>
                            <p-tag 
                                *ngIf="design.isPublic"
                                value="Public" 
                                severity="secondary"
                                [rounded]="true"
                                styleClass="public-tag">
                            </p-tag>
                        </div>
                    </div>

                    <!-- Card Actions -->
                    <div class="card-actions">
                        <p-button 
                            icon="pi pi-download"
                            [rounded]="true"
                            [outlined]="true"
                            severity="info"
                            styleClass="download-btn"
                            pTooltip="{{ 'account.designs.download' | translate }}"
                            (click)="downloadDesign(design, $event)">
                        </p-button>
                        <p-button 
                            icon="pi pi-print"
                            [rounded]="true"
                            severity="primary"
                            pTooltip="{{ 'account.designs.useForPrint' | translate }}"
                            [routerLink]="['/3d-print/new']" 
                            [queryParams]="{designId: design.id}"
                            (click)="$event.stopPropagation()">
                        </p-button>
                        <p-button 
                            icon="pi pi-trash"
                            [rounded]="true"
                            [outlined]="true"
                            severity="danger"
                            pTooltip="{{ 'account.designs.delete' | translate }}"
                            (click)="confirmDelete(design, $event)">
                        </p-button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Preview Dialog -->
        <p-dialog 
            [(visible)]="previewVisible" 
            [modal]="true"
            [closable]="true"
            [dismissableMask]="true"
            [style]="{width: '90vw', maxWidth: '1000px'}"
            [contentStyle]="{'padding': '0'}"
            styleClass="preview-dialog">
            
            <ng-template pTemplate="header">
                <div class="preview-header">
                    <i class="pi pi-box"></i>
                    <span>{{ selectedDesign?.title }}</span>
                </div>
            </ng-template>

            <div class="preview-container" *ngIf="selectedDesign">
                <!-- 3D Viewer Area -->
                <div class="viewer-area">
                    <div class="viewer-placeholder">
                        <div class="rotating-cube">
                            <div class="cube large">
                                <div class="face front"></div>
                                <div class="face back"></div>
                                <div class="face right"></div>
                                <div class="face left"></div>
                                <div class="face top"></div>
                                <div class="face bottom"></div>
                            </div>
                        </div>
                        <p class="viewer-text">{{ 'account.designs.3dPreview' | translate }}</p>
                        <p class="viewer-subtext">{{ getPrimaryFileName(selectedDesign) }}</p>
                    </div>
                </div>

                <!-- Design Details -->
                <div class="design-details">
                    <h3>{{ 'account.designs.details' | translate }}</h3>
                    
                    <div class="detail-row">
                        <span class="detail-label">{{ 'account.designs.title' | translate }}</span>
                        <span class="detail-value">{{ selectedDesign.title }}</span>
                    </div>
                    
                    <div class="detail-row" *ngIf="selectedDesign.notes">
                        <span class="detail-label">{{ 'account.designs.notes' | translate }}</span>
                        <span class="detail-value">{{ selectedDesign.notes }}</span>
                    </div>
                    
                    <div class="detail-row">
                        <span class="detail-label">{{ 'account.designs.created' | translate }}</span>
                        <span class="detail-value">{{ formatDateTime(selectedDesign.createdAt) }}</span>
                    </div>

                    <h4 class="files-title">{{ 'account.designs.filesIncluded' | translate }}</h4>
                    <div class="files-list">
                        <div *ngFor="let file of selectedDesign.files" class="file-item">
                            <div class="file-icon">
                                <i class="pi pi-file"></i>
                            </div>
                            <div class="file-info">
                                <span class="file-name">{{ file.fileName }}</span>
                                <span class="file-meta">
                                    {{ file.format || 'Unknown' }} • {{ formatFileSize(file.fileSizeBytes) }}
                                </span>
                            </div>
                            <p-button 
                                icon="pi pi-download"
                                [rounded]="true"
                                [text]="true"
                                (click)="downloadFile(file)">
                            </p-button>
                        </div>
                    </div>
                </div>
            </div>

            <ng-template pTemplate="footer">
                <div class="preview-footer">
                    <p-button 
                        [label]="'common.close' | translate" 
                        [outlined]="true"
                        (click)="previewVisible = false">
                    </p-button>
                    <p-button 
                        [label]="'account.designs.useForPrint' | translate" 
                        icon="pi pi-print"
                        [routerLink]="['/3d-print/new']" 
                        [queryParams]="{designId: selectedDesign?.id}"
                        (click)="previewVisible = false">
                    </p-button>
                </div>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        :host {
            --maba-primary: #667eea;
            --maba-secondary: #764ba2;
        }

        .designs-page {
            min-height: 100vh;
            background: linear-gradient(135deg, #f8f9ff 0%, #f3f4ff 100%);
            padding-bottom: 3rem;
        }

        /* Header Styles */
        .designs-header {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%);
            padding: 2rem;
            margin-bottom: 2rem;
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.2);
        }

        .header-content {
            max-width: 1400px;
            margin: 0 auto;
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .header-text {
            color: white;
        }

        .page-title {
            font-size: 2rem;
            font-weight: 700;
            margin: 0 0 0.5rem 0;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .page-title i {
            font-size: 1.75rem;
        }

        .page-subtitle {
            margin: 0;
            opacity: 0.9;
            font-size: 1.1rem;
        }

        :host ::ng-deep .upload-btn {
            background: white !important;
            color: var(--maba-primary) !important;
            border: none !important;
            font-weight: 600;
            padding: 0.75rem 1.5rem;
            border-radius: 10px !important;
        }

        :host ::ng-deep .upload-btn:hover {
            background: rgba(255,255,255,0.9) !important;
            transform: translateY(-2px);
        }

        /* Stats Bar */
        .stats-bar {
            max-width: 1400px;
            margin: 1.5rem auto 0;
            display: flex;
            gap: 2rem;
            flex-wrap: wrap;
        }

        .stat-item {
            display: flex;
            flex-direction: column;
            color: white;
        }

        .stat-value {
            font-size: 1.5rem;
            font-weight: 700;
        }

        .stat-label {
            font-size: 0.85rem;
            opacity: 0.8;
        }

        /* Loading State */
        .loading-container {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 400px;
        }

        .loading-content {
            text-align: center;
        }

        .loading-text {
            margin-top: 1rem;
            color: var(--text-color-secondary);
        }

        /* Empty State */
        .empty-state {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 400px;
            padding: 2rem;
        }

        .empty-content {
            text-align: center;
            max-width: 400px;
        }

        .empty-icon {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea22 0%, #764ba222 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 1.5rem;
        }

        .empty-icon i {
            font-size: 3rem;
            color: #667eea;
        }

        .empty-content h2 {
            margin: 0 0 0.5rem 0;
            color: var(--text-color);
        }

        .empty-content p {
            color: var(--text-color-secondary);
            margin-bottom: 1.5rem;
        }

        :host ::ng-deep .upload-btn-large {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%) !important;
            border: none !important;
            padding: 1rem 2rem;
            font-size: 1.1rem;
            border-radius: 12px !important;
        }

        /* Designs Grid */
        .designs-grid {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 1rem;
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
            gap: 1.5rem;
        }

        /* Design Card */
        .design-card {
            background: var(--surface-card);
            border-radius: 16px;
            overflow: hidden;
            border: 1px solid rgba(102, 126, 234, 0.12);
            box-shadow: 0 4px 20px rgba(102, 126, 234, 0.08);
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .design-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 12px 40px rgba(102, 126, 234, 0.16);
        }

        /* Card Preview */
        .card-preview {
            height: 200px;
            background: linear-gradient(135deg, #111a3d 0%, #1b2d6b 100%);
            position: relative;
            display: flex;
            align-items: center;
            justify-content: center;
            overflow: hidden;
        }

        .preview-content {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 1rem;
        }

        /* 3D Cube Animation */
        .file-icon-3d {
            perspective: 200px;
        }

        .cube {
            width: 60px;
            height: 60px;
            position: relative;
            transform-style: preserve-3d;
            animation: rotateCube 8s infinite linear;
        }

        .cube.large {
            width: 100px;
            height: 100px;
        }

        .cube .face {
            position: absolute;
            width: 100%;
            height: 100%;
            border: 2px solid rgba(102, 126, 234, 0.6);
            background: rgba(102, 126, 234, 0.1);
        }

        .cube .front { transform: translateZ(30px); }
        .cube .back { transform: translateZ(-30px) rotateY(180deg); }
        .cube .right { transform: translateX(30px) rotateY(90deg); }
        .cube .left { transform: translateX(-30px) rotateY(-90deg); }
        .cube .top { transform: translateY(-30px) rotateX(90deg); }
        .cube .bottom { transform: translateY(30px) rotateX(-90deg); }

        .cube.large .front { transform: translateZ(50px); }
        .cube.large .back { transform: translateZ(-50px) rotateY(180deg); }
        .cube.large .right { transform: translateX(50px) rotateY(90deg); }
        .cube.large .left { transform: translateX(-50px) rotateY(-90deg); }
        .cube.large .top { transform: translateY(-50px) rotateX(90deg); }
        .cube.large .bottom { transform: translateY(50px) rotateX(-90deg); }

        @keyframes rotateCube {
            from { transform: rotateX(-20deg) rotateY(0deg); }
            to { transform: rotateX(-20deg) rotateY(360deg); }
        }

        .file-format-badge {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%);
            color: white;
            padding: 0.35rem 1rem;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .preview-overlay {
            position: absolute;
            inset: 0;
            background: rgba(0,0,0,0.7);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            color: white;
            opacity: 0;
            transition: opacity 0.3s ease;
        }

        .design-card:hover .preview-overlay {
            opacity: 1;
        }

        .preview-overlay i {
            font-size: 2rem;
        }

        /* Card Content */
        .card-content {
            padding: 1.25rem;
        }

        .design-title {
            font-size: 1.2rem;
            font-weight: 600;
            margin-bottom: 0.75rem;
            color: var(--text-color);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .design-meta {
            display: flex;
            flex-direction: column;
            gap: 0.4rem;
            margin-bottom: 1rem;
        }

        .meta-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: var(--text-color-secondary);
            font-size: 0.9rem;
        }

        .meta-item i {
            width: 16px;
            color: var(--maba-primary);
        }

        .meta-item span {
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .design-tags {
            display: flex;
            gap: 0.5rem;
            flex-wrap: wrap;
        }

        :host ::ng-deep .public-tag {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%) !important;
            color: white !important;
        }

        /* Card Actions */
        .card-actions {
            padding: 1rem 1.25rem;
            border-top: 1px solid var(--surface-border);
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
        }
        :host ::ng-deep .card-actions .download-btn.p-button.p-button-outlined {
            border-color: var(--maba-primary) !important;
            color: var(--maba-primary) !important;
            background: #fff !important;
        }
        :host ::ng-deep .card-actions .download-btn.p-button.p-button-outlined:hover {
            border-color: var(--maba-secondary) !important;
            color: var(--maba-secondary) !important;
            background: #f8faff !important;
        }

        /* Preview Dialog */
        :host ::ng-deep .preview-dialog .p-dialog-header {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%);
            color: white;
            padding: 1rem 1.5rem;
        }

        .preview-header {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            font-size: 1.2rem;
            font-weight: 600;
        }

        .preview-container {
            display: grid;
            grid-template-columns: 1fr 350px;
            min-height: 500px;
        }

        @media (max-width: 768px) {
            .preview-container {
                grid-template-columns: 1fr;
            }
        }

        .viewer-area {
            background: linear-gradient(135deg, #111a3d 0%, #1b2d6b 100%);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .viewer-placeholder {
            text-align: center;
            color: white;
        }

        .rotating-cube {
            perspective: 400px;
            margin-bottom: 1.5rem;
        }

        .viewer-text {
            font-size: 1.2rem;
            margin: 0 0 0.5rem 0;
        }

        .viewer-subtext {
            opacity: 0.7;
            margin: 0;
        }

        .design-details {
            padding: 1.5rem;
            background: var(--surface-ground);
            overflow-y: auto;
        }

        .design-details h3 {
            margin: 0 0 1.5rem 0;
            color: var(--text-color);
            font-size: 1.1rem;
        }

        .detail-row {
            margin-bottom: 1rem;
        }

        .detail-label {
            display: block;
            font-size: 0.85rem;
            color: var(--text-color-secondary);
            margin-bottom: 0.25rem;
        }

        .detail-value {
            color: var(--text-color);
            font-weight: 500;
        }

        .files-title {
            margin: 1.5rem 0 1rem 0;
            color: var(--text-color);
            font-size: 1rem;
        }

        .files-list {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .file-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem;
            background: var(--surface-card);
            border-radius: 8px;
        }

        .file-icon {
            width: 40px;
            height: 40px;
            border-radius: 8px;
            background: linear-gradient(135deg, #667eea22 0%, #764ba222 100%);
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .file-icon i {
            color: #667eea;
        }

        .file-info {
            flex: 1;
            min-width: 0;
        }

        .file-name {
            display: block;
            font-weight: 500;
            color: var(--text-color);
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .file-meta {
            font-size: 0.85rem;
            color: var(--text-color-secondary);
        }

        .preview-footer {
            display: flex;
            justify-content: flex-end;
            gap: 0.75rem;
        }

        /* Confirm dialog theme alignment */
        :host ::ng-deep .p-confirmdialog {
            border-radius: 14px;
            overflow: hidden;
        }
        :host ::ng-deep .p-confirmdialog .p-dialog-header {
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%);
            color: #fff;
        }
        :host ::ng-deep .p-confirmdialog .p-confirm-dialog-icon {
            color: var(--maba-primary);
        }
        :host ::ng-deep .p-confirmdialog .confirm-no-btn {
            border-color: var(--maba-primary) !important;
            color: var(--maba-primary) !important;
            background: #fff !important;
        }
        :host ::ng-deep .p-confirmdialog .confirm-yes-btn {
            border: none !important;
            color: #fff !important;
            background: linear-gradient(135deg, var(--maba-primary) 0%, var(--maba-secondary) 100%) !important;
        }
        :host ::ng-deep .p-confirmdialog .confirm-yes-btn:hover {
            filter: brightness(0.97);
        }

        /* Responsive */
        @media (max-width: 640px) {
            .header-content {
                flex-direction: column;
                text-align: center;
            }

            .stats-bar {
                justify-content: center;
            }

            .designs-grid {
                grid-template-columns: 1fr;
            }

            .page-title {
                font-size: 1.5rem;
            }
        }
    `]
})
export class AccountDesignsComponent implements OnInit {
    designs: Print3dDesign[] = [];
    loading = false;
    previewVisible = false;
    selectedDesign: Print3dDesign | null = null;

    private printingApiService = inject(PrintingApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    private languageService = inject(LanguageService);

    ngOnInit() {
        this.loadDesigns();
    }

    loadDesigns() {
        this.loading = true;
        this.printingApiService.getDesigns().subscribe({
            next: (designs) => {
                this.designs = designs;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.loadError')
                });
            }
        });
    }

    openPreview(design: Print3dDesign) {
        this.selectedDesign = design;
        this.previewVisible = true;
    }

    downloadDesign(design: Print3dDesign, event: Event) {
        event.stopPropagation();
        if (design.files && design.files.length > 0) {
            const primaryFile = design.files.find(f => f.isPrimary) || design.files[0];
            this.downloadFile(primaryFile);
        }
    }

    downloadFile(file: DesignFile) {
        const fileUrl = file.fileUrl.startsWith('http') ? file.fileUrl : environment.apiUrl?.replace('/api/v1', '') + file.fileUrl;
        window.open(fileUrl, '_blank');
    }

    confirmDelete(design: Print3dDesign, event: Event) {
        event.stopPropagation();
        this.confirmationService.confirm({
            message: this.translateService.instant('account.designs.deleteConfirm'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            rejectLabel: this.translateService.instant('common.no'),
            acceptLabel: this.translateService.instant('common.yes'),
            rejectButtonStyleClass: 'confirm-no-btn',
            acceptButtonStyleClass: 'confirm-yes-btn',
            accept: () => {
                this.printingApiService.deleteDesign(design.id).subscribe({
                    next: () => {
                        this.designs = this.designs.filter(d => d.id !== design.id);
                        if (this.selectedDesign?.id === design.id) {
                            this.previewVisible = false;
                            this.selectedDesign = null;
                        }
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('messages.deleteSuccess')
                        });
                    },
                    error: (error) => {
                        const detail =
                            error?.error?.message ||
                            error?.error ||
                            this.translateService.instant('messages.deleteError');
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail
                        });
                    }
                });
            }
        });
    }

    getPrimaryFileName(design: Print3dDesign): string {
        if (!design.files || design.files.length === 0) return 'No file';
        const primary = design.files.find(f => f.isPrimary) || design.files[0];
        return primary.fileName;
    }

    getFileFormat(design: Print3dDesign): string {
        if (!design.files || design.files.length === 0) return '3D';
        const primary = design.files.find(f => f.isPrimary) || design.files[0];
        return primary.format || primary.fileName.split('.').pop()?.toUpperCase() || '3D';
    }

    getDesignSize(design: Print3dDesign): number {
        if (!design.files) return 0;
        return design.files.reduce((sum, f) => sum + f.fileSizeBytes, 0);
    }

    getTotalFiles(): number {
        return this.designs.reduce((sum, d) => sum + (d.files?.length || 0), 0);
    }

    getTotalSize(): number {
        return this.designs.reduce((sum, d) => sum + this.getDesignSize(d), 0);
    }

    formatFileSize(bytes: number): string {
        if (!bytes || bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    formatDate(date: string): string {
        if (!date) return '';
        return new Date(date).toLocaleDateString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US');
    }

    formatDateTime(date: string): string {
        if (!date) return '';
        return new Date(date).toLocaleString(this.languageService.language === 'ar' ? 'ar-SA' : 'en-US');
    }
}
