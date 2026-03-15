import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { SoftwareApiService } from '../../../core/services/software-api.service';
import { SoftwareProduct, SoftwareRelease, SoftwareFile } from '../../../shared/models/software.model';

@Component({
    selector: 'app-software-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        ButtonModule,
        TabsModule,
        TableModule,
        TagModule,
        DialogModule,
        CheckboxModule,
        SkeletonModule,
        TooltipModule,
        DividerModule,
        AccordionModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="software-detail" *ngIf="product">
            <!-- Header Section -->
            <section class="header-section">
                <div class="header-container">
                    <a routerLink="/software" class="back-link">
                        <i class="pi pi-arrow-left"></i>
                        {{ 'software.backToLibrary' | translate }}
                    </a>
                    <div class="product-header">
                        <div class="product-icon">
                            <i [class]="product.iconKey || 'pi pi-box'"></i>
                        </div>
                        <div class="product-info">
                            <div class="product-meta">
                                <span class="category" *ngIf="product.category">{{ product.category }}</span>
                            </div>
                            <h1>{{ isRtl ? product.nameAr : product.nameEn }}</h1>
                            <p>{{ isRtl ? product.summaryAr : product.summaryEn }}</p>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Main Content -->
            <section class="content-section">
                <div class="content-container">
                    <p-tabs [value]="activeTab">
                        <p-tablist>
                            <p-tab value="downloads">{{ 'software.downloads' | translate }}</p-tab>
                            <p-tab value="releasenotes">{{ 'software.releaseNotes' | translate }}</p-tab>
                            <p-tab value="requirements">{{ 'software.requirements' | translate }}</p-tab>
                            <p-tab value="license">{{ 'software.license' | translate }}</p-tab>
                        </p-tablist>
                        <p-tabpanels>
                            <!-- Downloads Tab -->
                            <p-tabpanel value="downloads">
                                <div class="releases-list">
                                    <p-accordion [value]="['0']" [multiple]="true">
                                        <p-accordion-panel *ngFor="let release of product.releases; let i = index" [value]="i.toString()">
                                            <p-accordion-header>
                                                <div class="release-header">
                                                    <span class="version">v{{ release.version }}</span>
                                                    <p-tag
                                                        [value]="release.status"
                                                        [severity]="getStatusSeverity(release.status)"
                                                        [rounded]="true"
                                                    ></p-tag>
                                                    <span class="release-date">{{ formatDate(release.releaseDate) }}</span>
                                                </div>
                                            </p-accordion-header>
                                            <p-accordion-content>
                                                <div class="files-table">
                                                    <p-table [value]="release.files || []" [tableStyle]="{'min-width': '50rem'}">
                                                        <ng-template pTemplate="header">
                                                            <tr>
                                                                <th>{{ 'software.fileName' | translate }}</th>
                                                                <th>{{ 'software.platform' | translate }}</th>
                                                                <th>{{ 'software.architecture' | translate }}</th>
                                                                <th>{{ 'software.type' | translate }}</th>
                                                                <th>{{ 'software.size' | translate }}</th>
                                                                <th>{{ 'software.sha256' | translate }}</th>
                                                                <th></th>
                                                            </tr>
                                                        </ng-template>
                                                        <ng-template pTemplate="body" let-file>
                                                            <tr>
                                                                <td>
                                                                    <span class="file-name">{{ file.fileName }}</span>
                                                                </td>
                                                                <td>
                                                                    <span class="os-badge" [class]="file.os.toLowerCase()">
                                                                        <i [class]="getOsIcon(file.os)"></i>
                                                                        {{ file.os }}
                                                                    </span>
                                                                </td>
                                                                <td>{{ file.arch }}</td>
                                                                <td>
                                                                    <p-tag [value]="file.fileType" severity="secondary"></p-tag>
                                                                </td>
                                                                <td>{{ formatFileSize(file.fileSizeBytes) }}</td>
                                                                <td>
                                                                    <span class="sha256" [pTooltip]="file.sha256">
                                                                        {{ file.sha256?.substring(0, 12) }}...
                                                                    </span>
                                                                </td>
                                                                <td>
                                                                    <p-button
                                                                        icon="pi pi-download"
                                                                        [label]="'software.download' | translate"
                                                                        (onClick)="initiateDownload(file)"
                                                                        severity="primary"
                                                                    ></p-button>
                                                                </td>
                                                            </tr>
                                                        </ng-template>
                                                        <ng-template pTemplate="emptymessage">
                                                            <tr>
                                                                <td colspan="7" class="text-center">{{ 'software.noFiles' | translate }}</td>
                                                            </tr>
                                                        </ng-template>
                                                    </p-table>
                                                </div>
                                            </p-accordion-content>
                                        </p-accordion-panel>
                                    </p-accordion>
                                </div>
                            </p-tabpanel>

                            <!-- Release Notes Tab -->
                            <p-tabpanel value="releasenotes">
                                <div class="release-notes" *ngFor="let release of product.releases">
                                    <div class="release-notes-header">
                                        <h3>v{{ release.version }}</h3>
                                        <p-tag
                                            [value]="release.status"
                                            [severity]="getStatusSeverity(release.status)"
                                            [rounded]="true"
                                        ></p-tag>
                                        <span class="date">{{ formatDate(release.releaseDate) }}</span>
                                    </div>
                                    <div class="changelog-content">
                                        <pre>{{ isRtl ? release.changelogAr : release.changelogEn }}</pre>
                                    </div>
                                    <p-divider></p-divider>
                                </div>
                            </p-tabpanel>

                            <!-- Requirements Tab -->
                            <p-tabpanel value="requirements">
                                <div class="requirements" *ngFor="let release of product.releases">
                                    <h3>v{{ release.version }} {{ 'software.systemRequirements' | translate }}</h3>
                                    <div class="requirements-content">
                                        <pre>{{ isRtl ? release.requirementsAr : release.requirementsEn }}</pre>
                                    </div>
                                    <p-divider></p-divider>
                                </div>
                            </p-tabpanel>

                            <!-- License Tab -->
                            <p-tabpanel value="license">
                                <div class="license-content">
                                    <pre>{{ isRtl ? product.licenseAr : product.licenseEn }}</pre>
                                </div>
                            </p-tabpanel>
                        </p-tabpanels>
                    </p-tabs>
                </div>
            </section>
        </div>

        <!-- Loading State -->
        <div class="loading-state" *ngIf="loading">
            <p-skeleton width="100%" height="200px"></p-skeleton>
            <p-skeleton width="100%" height="400px" styleClass="mt-4"></p-skeleton>
        </div>

        <!-- License Agreement Dialog -->
        <p-dialog
            [(visible)]="showLicenseDialog"
            [header]="'software.licenseAgreement' | translate"
            [modal]="true"
            [style]="{width: '500px'}"
        >
            <div class="license-dialog-content">
                <p>{{ 'software.licenseAgreementText' | translate }}</p>
                <div class="license-checkbox">
                    <p-checkbox
                        [(ngModel)]="licenseAccepted"
                        [binary]="true"
                        inputId="licenseAccept"
                    ></p-checkbox>
                    <label for="licenseAccept">{{ 'software.acceptLicense' | translate }}</label>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button
                    [label]="'common.cancel' | translate"
                    severity="secondary"
                    (onClick)="showLicenseDialog = false"
                ></p-button>
                <p-button
                    [label]="'software.downloadNow' | translate"
                    [disabled]="!licenseAccepted"
                    (onClick)="confirmDownload()"
                    severity="primary"
                ></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        :host {
            --gradient-hero: linear-gradient(145deg, #0c1445 0%, #1a1a2e 40%, #16213e 70%, #0f3460 100%);
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
        }

        .software-detail {
            min-height: 100vh;
            background: #f8fafc;
        }

        .header-section {
            background: var(--gradient-hero);
            padding: 3rem 2rem;
            color: white;
            position: relative;
            overflow: hidden;
        }

        .header-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: 
                radial-gradient(circle at 20% 80%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 80% 20%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            pointer-events: none;
        }

        .header-container {
            max-width: 1200px;
            margin: 0 auto;
            position: relative;
            z-index: 1;
        }

        .back-link {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            color: rgba(165, 180, 252, 0.9);
            text-decoration: none;
            font-size: 0.9rem;
            margin-bottom: 1.5rem;
            transition: color 0.2s ease;
        }

        .back-link:hover {
            color: #a5b4fc;
        }

        .product-header {
            display: flex;
            gap: 1.5rem;
            align-items: flex-start;
        }

        .product-icon {
            width: 80px;
            height: 80px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.3) 0%, rgba(118, 75, 162, 0.3) 100%);
            border: 1px solid rgba(102, 126, 234, 0.4);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .product-icon i {
            font-size: 2.5rem;
            color: #a5b4fc;
        }

        .product-info {
            flex: 1;
        }

        .product-meta {
            margin-bottom: 0.5rem;
        }

        .category {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.3) 0%, rgba(118, 75, 162, 0.3) 100%);
            border: 1px solid rgba(102, 126, 234, 0.3);
            padding: 0.25rem 0.75rem;
            border-radius: 12px;
            font-size: 0.8rem;
            color: #a5b4fc;
        }

        .product-info h1 {
            font-size: 2rem;
            font-weight: 700;
            margin: 0 0 0.5rem 0;
            background: linear-gradient(135deg, #ffffff 0%, #a5b4fc 50%, #667eea 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .product-info p {
            font-size: 1rem;
            color: rgba(255, 255, 255, 0.8);
            margin: 0;
        }

        .content-section {
            max-width: 1200px;
            margin: 0 auto;
            padding: 2rem;
        }

        .content-container {
            background: white;
            border-radius: 12px;
            border: 1px solid #e5e7eb;
            overflow: hidden;
        }

        :host ::ng-deep .p-tabpanels {
            padding: 1.5rem;
        }

        .release-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            width: 100%;
        }

        .version {
            font-weight: 600;
            font-size: 1rem;
        }

        .release-date {
            color: #6b7280;
            font-size: 0.85rem;
            margin-inline-start: auto;
        }

        .files-table {
            margin-top: 1rem;
        }

        .file-name {
            font-weight: 500;
            color: #374151;
        }

        .os-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.25rem 0.75rem;
            border-radius: 6px;
            font-size: 0.85rem;
        }

        .os-badge.windows {
            background: #0078d411;
            color: #0078d4;
        }

        .os-badge.macos {
            background: #00000011;
            color: #000;
        }

        .os-badge.linux {
            background: #dd480011;
            color: #dd4800;
        }

        /* Status tag (Stable, Beta, etc.) – theme purple instead of green */
        :host ::ng-deep .p-tag.p-tag-success,
        :host ::ng-deep .p-tag[data-pc-severity="success"],
        :host ::ng-deep [data-p-severity="success"].p-tag {
            background: #667eea !important;
            background-color: #667eea !important;
            color: white !important;
        }

        .sha256 {
            font-family: monospace;
            font-size: 0.8rem;
            color: #6b7280;
            cursor: pointer;
        }

        .release-notes-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .release-notes-header h3 {
            margin: 0;
            font-size: 1.25rem;
        }

        .release-notes-header .date {
            color: #6b7280;
            margin-inline-start: auto;
        }

        .changelog-content,
        .requirements-content,
        .license-content {
            background: #f8fafc;
            padding: 1.5rem;
            border-radius: 8px;
            border: 1px solid #e5e7eb;
        }

        .changelog-content pre,
        .requirements-content pre,
        .license-content pre {
            margin: 0;
            white-space: pre-wrap;
            font-family: inherit;
            line-height: 1.6;
        }

        .requirements h3 {
            margin-top: 0;
        }

        .loading-state {
            padding: 2rem;
        }

        .license-dialog-content {
            padding: 1rem 0;
        }

        .license-checkbox {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-top: 1.5rem;
            padding: 1rem;
            background: #f8fafc;
            border-radius: 8px;
        }

        .license-checkbox label {
            cursor: pointer;
        }

        @media (max-width: 768px) {
            .product-header {
                flex-direction: column;
                text-align: center;
            }

            .product-icon {
                margin: 0 auto;
            }

            .product-info h1 {
                font-size: 1.5rem;
            }

            .release-header {
                flex-wrap: wrap;
            }

            :host ::ng-deep .p-table .p-datatable-thead > tr > th,
            :host ::ng-deep .p-table .p-datatable-tbody > tr > td {
                padding: 0.5rem;
                font-size: 0.85rem;
            }
        }
    `]
})
export class SoftwareDetailComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly softwareApi = inject(SoftwareApiService);
    private readonly translate = inject(TranslateService);
    private readonly messageService = inject(MessageService);

    product: SoftwareProduct | null = null;
    loading = true;
    showLicenseDialog = false;
    licenseAccepted = false;
    selectedFile: SoftwareFile | null = null;
    activeTab = 'downloads';

    get isRtl(): boolean {
        return this.translate.currentLang === 'ar';
    }

    ngOnInit() {
        this.route.params.subscribe(params => {
            const slug = params['slug'];
            if (slug) {
                this.loadProduct(slug);
            }
        });
    }

    loadProduct(slug: string) {
        this.loading = true;
        this.softwareApi.getProductBySlug(slug).subscribe({
            next: (product) => {
                this.product = product;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    initiateDownload(file: SoftwareFile) {
        this.selectedFile = file;
        this.licenseAccepted = false;
        this.showLicenseDialog = true;
    }

    confirmDownload() {
        if (!this.selectedFile || !this.licenseAccepted) return;

        this.showLicenseDialog = false;

        this.softwareApi.downloadFile(this.selectedFile.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = this.selectedFile!.fileName;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);

                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('software.downloadStarted'),
                    detail: this.selectedFile!.fileName
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: this.translate.instant('software.downloadFailed')
                });
            }
        });
    }

    getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
        switch (status?.toLowerCase()) {
            case 'stable': return 'success';
            case 'beta': return 'warn';
            case 'alpha': return 'info';
            case 'deprecated': return 'danger';
            default: return 'secondary';
        }
    }

    getOsIcon(os: string): string {
        switch (os?.toLowerCase()) {
            case 'windows': return 'pi pi-microsoft';
            case 'macos': return 'pi pi-apple';
            case 'linux': return 'pi pi-server';
            default: return 'pi pi-desktop';
        }
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString(this.isRtl ? 'ar-SA' : 'en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    formatFileSize(bytes: number): string {
        if (bytes >= 1073741824) {
            return (bytes / 1073741824).toFixed(1) + ' GB';
        }
        if (bytes >= 1048576) {
            return (bytes / 1048576).toFixed(1) + ' MB';
        }
        if (bytes >= 1024) {
            return (bytes / 1024).toFixed(1) + ' KB';
        }
        return bytes + ' B';
    }
}
