import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SoftwareApiService } from '../../../core/services/software-api.service';
import { SoftwareProduct, CreateSoftwareProductRequest } from '../../../shared/models/software.model';

@Component({
    selector: 'app-software-products-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TranslateModule,
        TableModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        TextareaModule,
        CheckboxModule,
        InputNumberModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        ToastModule
    ],
    providers: [ConfirmationService, MessageService],
    template: `
        <p-toast></p-toast>
        <p-confirmDialog></p-confirmDialog>

        <div class="software-products-admin">
            <div class="page-header">
                <div class="header-content">
                    <h1>{{ 'admin.software.title' | translate }}</h1>
                    <p>{{ 'admin.software.subtitle' | translate }}</p>
                </div>
                <p-button
                    [label]="'admin.software.addProduct' | translate"
                    icon="pi pi-plus"
                    (onClick)="openCreateDialog()"
                ></p-button>
            </div>

            <div class="products-table">
                <p-table
                    [value]="products"
                    [loading]="loading"
                    [paginator]="true"
                    [rows]="10"
                    [showCurrentPageReport]="true"
                    [rowHover]="true"
                    styleClass="p-datatable-striped"
                >
                    <ng-template pTemplate="header">
                        <tr>
                            <th>{{ 'admin.software.name' | translate }}</th>
                            <th>{{ 'admin.software.slug' | translate }}</th>
                            <th>{{ 'admin.software.category' | translate }}</th>
                            <th>{{ 'admin.software.releases' | translate }}</th>
                            <th>{{ 'admin.software.latestVersion' | translate }}</th>
                            <th>{{ 'admin.software.downloads' | translate }}</th>
                            <th>{{ 'admin.software.status' | translate }}</th>
                            <th>{{ 'common.actions' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-product>
                        <tr>
                            <td>
                                <div class="product-name">
                                    <i [class]="product.iconKey || 'pi pi-box'" class="product-icon"></i>
                                    <span>{{ isRtl ? product.nameAr : product.nameEn }}</span>
                                </div>
                            </td>
                            <td><code>{{ product.slug }}</code></td>
                            <td>{{ product.category || '-' }}</td>
                            <td>{{ product.releasesCount || 0 }}</td>
                            <td>
                                <span *ngIf="product.latestVersion" class="version-badge">
                                    v{{ product.latestVersion }}
                                </span>
                                <span *ngIf="!product.latestVersion">-</span>
                            </td>
                            <td>{{ product.downloadCount || 0 }}</td>
                            <td>
                                <p-tag
                                    [value]="product.isActive ? ('common.active' | translate) : ('common.inactive' | translate)"
                                    [severity]="product.isActive ? 'success' : 'danger'"
                                ></p-tag>
                            </td>
                            <td>
                                <div class="action-buttons">
                                    <p-button
                                        icon="pi pi-list"
                                        [rounded]="true"
                                        [text]="true"
                                        severity="info"
                                        [pTooltip]="'admin.software.manageReleases' | translate"
                                        [routerLink]="['/admin/software', product.id, 'releases']"
                                    ></p-button>
                                    <p-button
                                        icon="pi pi-pencil"
                                        [rounded]="true"
                                        [text]="true"
                                        severity="secondary"
                                        [pTooltip]="'common.edit' | translate"
                                        (onClick)="openEditDialog(product)"
                                    ></p-button>
                                    <p-button
                                        icon="pi pi-trash"
                                        [rounded]="true"
                                        [text]="true"
                                        severity="danger"
                                        [pTooltip]="'common.delete' | translate"
                                        (onClick)="confirmDelete(product)"
                                    ></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="8" class="text-center">
                                {{ 'admin.software.noProducts' | translate }}
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>

        <!-- Create/Edit Dialog -->
        <p-dialog
            [(visible)]="showDialog"
            [modal]="true"
            [style]="{width: '600px'}"
            [draggable]="false"
        >
            <ng-template pTemplate="header">
                <div class="dlg-header">
                    <span class="dlg-label">Software</span>
                    <span class="dlg-title">{{ editMode ? ('admin.software.editProduct' | translate) : ('admin.software.addProduct' | translate) }}</span>
                </div>
            </ng-template>
            <div class="dialog-content" *ngIf="formData">
                <div class="form-grid">
                    <div class="form-field">
                        <label>{{ 'admin.software.slug' | translate }} *</label>
                        <input pInputText [(ngModel)]="formData.slug" [placeholder]="'e.g., my-software'" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.category' | translate }}</label>
                        <input pInputText [(ngModel)]="formData.category" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.nameEn' | translate }} *</label>
                        <input pInputText [(ngModel)]="formData.nameEn" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.nameAr' | translate }} *</label>
                        <input pInputText [(ngModel)]="formData.nameAr" dir="rtl" />
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.summaryEn' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.summaryEn" rows="2"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.summaryAr' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.summaryAr" rows="2" dir="rtl"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.descriptionEn' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.descriptionEn" rows="3"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.descriptionAr' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.descriptionAr" rows="3" dir="rtl"></textarea>
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.iconKey' | translate }}</label>
                        <input pInputText [(ngModel)]="formData.iconKey" placeholder="pi pi-box" />
                    </div>
                    <div class="form-field">
                        <label>{{ 'admin.software.sortOrder' | translate }}</label>
                        <p-inputNumber [(ngModel)]="formData.sortOrder" [min]="0"></p-inputNumber>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.licenseEn' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.licenseEn" rows="3"></textarea>
                    </div>
                    <div class="form-field full-width">
                        <label>{{ 'admin.software.licenseAr' | translate }}</label>
                        <textarea pTextarea [(ngModel)]="formData.licenseAr" rows="3" dir="rtl"></textarea>
                    </div>
                    <div class="form-field checkbox-field">
                        <p-checkbox
                            [(ngModel)]="formData.isActive"
                            [binary]="true"
                            inputId="isActiveCheck"
                        ></p-checkbox>
                        <label for="isActiveCheck">{{ 'admin.software.isActive' | translate }}</label>
                    </div>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button
                    [label]="'common.cancel' | translate"
                    severity="secondary"
                    (onClick)="showDialog = false"
                ></p-button>
                <p-button
                    [label]="'common.save' | translate"
                    (onClick)="saveProduct()"
                    [loading]="saving"
                ></p-button>
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .software-products-admin {
            padding: 1.5rem;
        }

        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 1.5rem;
        }

        .header-content h1 {
            margin: 0 0 0.25rem 0;
            font-size: 1.5rem;
            font-weight: 600;
        }

        .header-content p {
            margin: 0;
            color: #6b7280;
        }

        .products-table {
            background: white;
            border-radius: 8px;
            overflow: hidden;
        }

        .product-name {
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .product-icon {
            width: 32px;
            height: 32px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #667eea11 0%, #764ba211 100%);
            border-radius: 6px;
            color: #667eea;
        }

        code {
            background: #f3f4f6;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
            font-size: 0.85rem;
        }

        .version-badge {
            background: #667eea11;
            color: #667eea;
            padding: 0.25rem 0.5rem;
            border-radius: 4px;
            font-weight: 500;
            font-size: 0.85rem;
        }

        .action-buttons {
            display: flex;
            gap: 0.25rem;
        }

        .dialog-content {
            padding: 1rem 0;
        }

        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
        }

        .form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-field.full-width {
            grid-column: 1 / -1;
        }

        .form-field.checkbox-field {
            flex-direction: row;
            align-items: center;
        }

        .form-field label {
            font-weight: 500;
            font-size: 0.9rem;
            color: #374151;
        }

        .form-field input,
        .form-field textarea {
            width: 100%;
        }
    `]
})
export class SoftwareProductsListComponent implements OnInit {
    private readonly softwareApi = inject(SoftwareApiService);
    private readonly translate = inject(TranslateService);
    private readonly confirmationService = inject(ConfirmationService);
    private readonly messageService = inject(MessageService);

    products: SoftwareProduct[] = [];
    loading = true;
    showDialog = false;
    editMode = false;
    saving = false;
    formData: CreateSoftwareProductRequest | null = null;
    editingId: string | null = null;

    get isRtl(): boolean {
        return this.translate.currentLang === 'ar';
    }

    ngOnInit() {
        this.loadProducts();
    }

    loadProducts() {
        this.loading = true;
        this.softwareApi.getAllProductsAdmin().subscribe({
            next: (products) => {
                this.products = products;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    openCreateDialog() {
        this.editMode = false;
        this.editingId = null;
        this.formData = {
            slug: '',
            nameEn: '',
            nameAr: '',
            summaryEn: '',
            summaryAr: '',
            descriptionEn: '',
            descriptionAr: '',
            category: '',
            iconKey: 'pi pi-box',
            licenseEn: '',
            licenseAr: '',
            isActive: true,
            sortOrder: 0
        };
        this.showDialog = true;
    }

    openEditDialog(product: SoftwareProduct) {
        this.editMode = true;
        this.editingId = product.id;
        this.formData = {
            slug: product.slug,
            nameEn: product.nameEn,
            nameAr: product.nameAr,
            summaryEn: product.summaryEn || '',
            summaryAr: product.summaryAr || '',
            descriptionEn: product.descriptionEn || '',
            descriptionAr: product.descriptionAr || '',
            category: product.category || '',
            iconKey: product.iconKey || 'pi pi-box',
            licenseEn: product.licenseEn || '',
            licenseAr: product.licenseAr || '',
            isActive: product.isActive,
            sortOrder: product.sortOrder
        };
        this.showDialog = true;
    }

    saveProduct() {
        if (!this.formData || !this.formData.slug || !this.formData.nameEn || !this.formData.nameAr) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translate.instant('common.warning'),
                detail: this.translate.instant('admin.software.fillRequired')
            });
            return;
        }

        this.saving = true;
        const request = this.editMode && this.editingId
            ? this.softwareApi.updateProduct(this.editingId, this.formData)
            : this.softwareApi.createProduct(this.formData);

        request.subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('common.success'),
                    detail: this.translate.instant(this.editMode ? 'admin.software.updated' : 'admin.software.created')
                });
                this.showDialog = false;
                this.saving = false;
                this.loadProducts();
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: err.error || this.translate.instant('common.saveFailed')
                });
                this.saving = false;
            }
        });
    }

    confirmDelete(product: SoftwareProduct) {
        this.confirmationService.confirm({
            message: this.translate.instant('admin.software.deleteConfirm', { name: product.nameEn }),
            header: this.translate.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.softwareApi.deleteProduct(product.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translate.instant('common.success'),
                            detail: this.translate.instant('admin.software.deleted')
                        });
                        this.loadProducts();
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translate.instant('common.error'),
                            detail: this.translate.instant('common.deleteFailed')
                        });
                    }
                });
            }
        });
    }
}
