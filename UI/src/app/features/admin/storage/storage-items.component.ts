import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ItemsApiService, PagedResult } from '../../../shared/services/items-api.service';
import { MediaApiService } from '../../../shared/services/media-api.service';
import { CatalogApiService } from '../../../shared/services/catalog-api.service';
import { Item } from '../../../shared/models/item.model';
import { Category } from '../../../shared/models/catalog.model';
import { environment } from '../../../../environments/environment';

interface StorageItemVm {
    item: Item;
    quantity: number;
    reorderLevel: number;
    imageUrl: string;
    savingQty?: boolean;
}

@Component({
    selector: 'app-storage-items',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        InputTextModule,
        InputNumberModule,
        DialogModule,
        TagModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="storage-page">
            <div class="header">
                <div>
                    <h1>Storage Items</h1>
                    <p>Add simple storage items, update quantities fast, and publish ready items to shop.</p>
                </div>
                <p-button label="Add Storage Item" icon="pi pi-plus" (onClick)="openCreateDialog()" />
            </div>

            <div class="cards-grid">
                <p-card *ngFor="let row of rows">
                    <div class="storage-card">
                        <img [src]="row.imageUrl" alt="storage item" class="thumb" />
                        <div class="meta">
                            <div class="title-row">
                                <h3>{{ row.item.nameEn }}</h3>
                                <p-tag
                                    [value]="row.item.itemStatusKey === 'Active' ? 'In Shop' : 'Storage Only'"
                                    [severity]="row.item.itemStatusKey === 'Active' ? 'success' : 'warn'">
                                </p-tag>
                            </div>
                            <small class="sku">{{ row.item.sku }}</small>

                            <div class="qty-box">
                                <label>Quantity</label>
                                <div class="qty-controls">
                                    <p-button
                                        icon="pi pi-minus"
                                        [outlined]="true"
                                        size="small"
                                        [disabled]="!!row.savingQty || row.quantity <= 0"
                                        (onClick)="quickAdjust(row, -1)" />
                                    <p-inputNumber
                                        [(ngModel)]="row.quantity"
                                        [min]="0"
                                        [maxFractionDigits]="0"
                                        inputStyleClass="qty-input"
                                        [disabled]="!!row.savingQty"
                                        (onBlur)="saveQuantity(row)" />
                                    <p-button
                                        icon="pi pi-plus"
                                        [outlined]="true"
                                        size="small"
                                        [disabled]="!!row.savingQty"
                                        (onClick)="quickAdjust(row, 1)" />
                                </div>
                            </div>

                            <div class="actions">
                                <p-button
                                    label="Edit Details"
                                    icon="pi pi-pencil"
                                    [outlined]="true"
                                    size="small"
                                    (onClick)="openPublishDialog(row, false)" />
                                <p-button
                                    [label]="row.item.itemStatusKey === 'Active' ? 'Remove From Shop' : 'Put In Shop'"
                                    [icon]="row.item.itemStatusKey === 'Active' ? 'pi pi-eye-slash' : 'pi pi-shopping-cart'"
                                    size="small"
                                    (onClick)="togglePublish(row)" />
                                <p-button
                                    label="Delete"
                                    icon="pi pi-trash"
                                    severity="danger"
                                    [outlined]="true"
                                    size="small"
                                    (onClick)="deleteStorageItem(row)" />
                            </div>
                        </div>
                    </div>
                </p-card>
            </div>
        </div>

        <p-dialog header="Add Storage Item" [(visible)]="showCreateDialog" [modal]="true" [style]="{width: '560px'}">
            <div class="dialog-grid">
                <div class="field">
                    <label>Name</label>
                    <input pInputText [(ngModel)]="createModel.name" />
                </div>
                <div class="field">
                    <label>Initial Quantity</label>
                    <p-inputNumber [(ngModel)]="createModel.quantity" [min]="0" [maxFractionDigits]="0" />
                </div>
                <div class="field field-full">
                    <label>Image</label>
                    <input #createImageInput type="file" accept="image/*" class="hidden-file-input" (change)="onCreateFileSelected($event)" />
                    <div class="upload-box" (click)="createImageInput.click()">
                        <i class="pi pi-image"></i>
                        <div class="upload-main">{{ createImageFile ? 'Image selected' : 'Click to choose image' }}</div>
                        <div class="upload-sub">PNG/JPG/WEBP - used as card/shop image</div>
                    </div>
                    <div class="file-meta" *ngIf="createImageFile">
                        <img *ngIf="createImagePreviewUrl" [src]="createImagePreviewUrl" alt="selected preview" class="preview-img" />
                        <div class="file-text">
                            <strong>{{ createImageFile.name }}</strong>
                            <small>{{ formatBytes(createImageFile.size) }}</small>
                        </div>
                        <p-button icon="pi pi-times" severity="secondary" [outlined]="true" size="small" (onClick)="clearCreateImage($event)" />
                    </div>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button label="Cancel" [outlined]="true" (onClick)="showCreateDialog = false" />
                <p-button label="Create" [loading]="creating" (onClick)="createStorageItem()" />
            </ng-template>
        </p-dialog>

        <p-dialog header="Complete Shop Details" [(visible)]="showPublishDialog" [modal]="true" [style]="{width: '620px'}">
            <div class="dialog-grid">
                <div class="field">
                    <label>Category</label>
                    <select class="native-select" [(ngModel)]="publishModel.categoryId">
                        <option [ngValue]="''">Select category</option>
                        <option *ngFor="let c of categories" [ngValue]="c.id">{{ c.nameEn }}</option>
                    </select>
                </div>
                <div class="field">
                    <label>Price</label>
                    <p-inputNumber [(ngModel)]="publishModel.price" [min]="0" [maxFractionDigits]="2" />
                </div>
                <div class="field field-full">
                    <label>Description (English)</label>
                    <input pInputText [(ngModel)]="publishModel.generalDescriptionEn" />
                </div>
                <div class="field field-full">
                    <label>Description (Arabic)</label>
                    <input pInputText [(ngModel)]="publishModel.generalDescriptionAr" />
                </div>
                <div class="field field-full">
                    <label>Replace/Upload Image (optional)</label>
                    <input #publishImageInput type="file" accept="image/*" class="hidden-file-input" (change)="onPublishFileSelected($event)" />
                    <div class="upload-box" (click)="publishImageInput.click()">
                        <i class="pi pi-upload"></i>
                        <div class="upload-main">{{ publishImageFile ? 'New image selected' : 'Click to replace current image' }}</div>
                        <div class="upload-sub">Leave empty to keep existing image</div>
                    </div>
                    <div class="file-meta" *ngIf="publishImageFile">
                        <img *ngIf="publishImagePreviewUrl" [src]="publishImagePreviewUrl" alt="selected preview" class="preview-img" />
                        <div class="file-text">
                            <strong>{{ publishImageFile.name }}</strong>
                            <small>{{ formatBytes(publishImageFile.size) }}</small>
                        </div>
                        <p-button icon="pi pi-times" severity="secondary" [outlined]="true" size="small" (onClick)="clearPublishImage($event)" />
                    </div>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button label="Cancel" [outlined]="true" (onClick)="showPublishDialog = false" />
                <p-button
                    [label]="publishAfterSave ? 'Save & Put In Shop' : 'Save Details'"
                    [loading]="savingPublishDetails"
                    (onClick)="savePublishDetails()" />
            </ng-template>
        </p-dialog>
    `,
    styles: [`
        .storage-page { padding: 1rem; }
        .header { display: flex; justify-content: space-between; gap: 1rem; align-items: center; margin-bottom: 1rem; }
        .header h1 { margin: 0 0 0.25rem; }
        .header p { margin: 0; color: #64748b; }
        .cards-grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
        .storage-card { display: grid; grid-template-columns: 96px 1fr; gap: 1rem; }
        .thumb { width: 96px; height: 96px; border-radius: 10px; object-fit: cover; border: 1px solid #e2e8f0; }
        .meta h3 { margin: 0; font-size: 1rem; }
        .title-row { display: flex; justify-content: space-between; gap: 0.5rem; align-items: center; }
        .sku { color: #64748b; display: block; margin-bottom: 0.7rem; }
        .qty-box { margin-bottom: 0.7rem; }
        .qty-box label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 0.25rem; }
        .qty-controls { display: flex; align-items: center; gap: 0.4rem; }
        .actions { display: flex; flex-wrap: wrap; gap: 0.5rem; }
        .dialog-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.8rem; }
        .field { display: flex; flex-direction: column; gap: 0.35rem; }
        .field label { font-size: 0.85rem; font-weight: 600; }
        .field-full { grid-column: 1 / -1; }
        .native-select { border: 1px solid #cbd5e1; border-radius: 8px; height: 2.5rem; padding: 0 0.6rem; }
        .hidden-file-input { display: none; }
        .upload-box {
            border: 1px dashed #8b95ff;
            border-radius: 10px;
            background: #f8faff;
            padding: 0.85rem;
            cursor: pointer;
            transition: all 0.2s ease;
        }
        .upload-box:hover { background: #f1f4ff; border-color: #6573f3; }
        .upload-box i { color: #6573f3; margin-bottom: 0.4rem; display: inline-block; }
        .upload-main { font-weight: 600; color: #1e293b; }
        .upload-sub { color: #64748b; font-size: 0.82rem; margin-top: 0.1rem; }
        .file-meta {
            display: flex;
            align-items: center;
            gap: 0.6rem;
            margin-top: 0.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            padding: 0.45rem 0.55rem;
            background: #fff;
        }
        .preview-img { width: 42px; height: 42px; border-radius: 8px; object-fit: cover; border: 1px solid #e2e8f0; }
        .file-text { display: flex; flex-direction: column; min-width: 0; flex: 1; }
        .file-text strong { font-size: 0.85rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .file-text small { color: #64748b; }
        @media (max-width: 768px) {
            .header { flex-direction: column; align-items: flex-start; }
            .storage-card { grid-template-columns: 1fr; }
            .thumb { width: 100%; height: 180px; }
            .dialog-grid { grid-template-columns: 1fr; }
        }
    `]
})
export class StorageItemsComponent implements OnInit {
    private readonly storageSkuPrefix = 'STR-';
    private readonly defaultImageMediaTypeId = '00000000-0000-0000-0000-000000000001';

    private itemsApi = inject(ItemsApiService);
    private mediaApi = inject(MediaApiService);
    private catalogApi = inject(CatalogApiService);
    private messageService = inject(MessageService);

    rows: StorageItemVm[] = [];
    categories: Category[] = [];
    statuses: { id: string; key: string }[] = [];

    showCreateDialog = false;
    creating = false;
    createModel = { name: '', quantity: 0 };
    createImageFile: File | null = null;
    createImagePreviewUrl = '';

    showPublishDialog = false;
    publishAfterSave = false;
    savingPublishDetails = false;
    selectedRow: StorageItemVm | null = null;
    publishImageFile: File | null = null;
    publishImagePreviewUrl = '';
    publishModel = {
        categoryId: '',
        price: 0,
        generalDescriptionEn: '',
        generalDescriptionAr: ''
    };

    ngOnInit(): void {
        this.loadDependencies();
    }

    openCreateDialog(): void {
        this.createModel = { name: '', quantity: 0 };
        this.createImageFile = null;
        this.createImagePreviewUrl = '';
        this.showCreateDialog = true;
    }

    private loadDependencies(): void {
        this.itemsApi.getItemStatuses().subscribe({
            next: (statuses) => {
                this.statuses = statuses.map((s) => ({ id: s.id, key: s.key }));
            }
        });
        this.catalogApi.getAllCategories(true).subscribe({
            next: (categories) => {
                this.categories = categories;
            }
        });
        this.loadStorageItems();
    }

    private loadStorageItems(): void {
        this.itemsApi.searchItems(undefined, { pageNumber: 1, pageSize: 300, sortBy: 'createdAt', sortDescending: true }).subscribe({
            next: (response: PagedResult<Item>) => {
                const storageItems = (response.items || []).filter((x) => (x.sku || '').startsWith(this.storageSkuPrefix));
                this.rows = storageItems.map((item) => ({
                    item,
                    quantity: Number(item.inventory?.quantityOnHand ?? 0),
                    reorderLevel: Number(item.inventory?.reorderLevel ?? 0),
                    imageUrl: this.resolveImageUrl(item.primaryImageUrl)
                }));
            }
        });
    }

    onCreateFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0] ?? null;
        this.createImageFile = file;
        this.createImagePreviewUrl = file ? URL.createObjectURL(file) : '';
    }

    onPublishFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0] ?? null;
        this.publishImageFile = file;
        this.publishImagePreviewUrl = file ? URL.createObjectURL(file) : '';
    }

    clearCreateImage(event?: Event): void {
        event?.stopPropagation();
        this.createImageFile = null;
        this.createImagePreviewUrl = '';
    }

    clearPublishImage(event?: Event): void {
        event?.stopPropagation();
        this.publishImageFile = null;
        this.publishImagePreviewUrl = '';
    }

    formatBytes(bytes: number): string {
        if (!bytes) return '0 B';
        const units = ['B', 'KB', 'MB', 'GB'];
        let size = bytes;
        let unitIndex = 0;
        while (size >= 1024 && unitIndex < units.length - 1) {
            size /= 1024;
            unitIndex++;
        }
        return `${size.toFixed(unitIndex === 0 ? 0 : 1)} ${units[unitIndex]}`;
    }

    createStorageItem(): void {
        if (!this.createModel.name.trim()) {
            this.toastError('Name is required.');
            return;
        }

        const draftStatusId = this.getStatusId('Draft');
        if (!draftStatusId) {
            this.toastError('Draft status is missing. Please check item statuses.');
            return;
        }

        this.creating = true;
        const nowCode = Date.now().toString().slice(-8);
        this.itemsApi.createItem({
            nameEn: this.createModel.name.trim(),
            nameAr: this.createModel.name.trim(),
            sku: `${this.storageSkuPrefix}${nowCode}`,
            itemStatusId: draftStatusId,
            price: 0,
            currency: 'USD',
            initialQuantity: Number(this.createModel.quantity ?? 0),
            reorderLevel: 0
        }).subscribe({
            next: (created) => {
                this.uploadAndLinkImageIfAny(created.id, this.createImageFile, () => {
                    this.creating = false;
                    this.showCreateDialog = false;
                    this.toastSuccess('Storage item created.');
                    this.loadStorageItems();
                });
            },
            error: (err) => {
                this.creating = false;
                this.toastError(err?.error?.message || 'Failed to create storage item.');
            }
        });
    }

    quickAdjust(row: StorageItemVm, delta: number): void {
        const newQty = Math.max(0, Number(row.quantity || 0) + delta);
        row.quantity = newQty;
        this.saveQuantity(row);
    }

    saveQuantity(row: StorageItemVm): void {
        row.savingQty = true;
        this.itemsApi.updateInventory(row.item.id, {
            quantityOnHand: Math.max(0, Number(row.quantity || 0)),
            reorderLevel: Number(row.reorderLevel || 0)
        }).subscribe({
            next: () => {
                row.savingQty = false;
            },
            error: () => {
                row.savingQty = false;
                this.toastError('Failed to update quantity.');
                this.loadStorageItems();
            }
        });
    }

    togglePublish(row: StorageItemVm): void {
        const currentlyPublished = row.item.itemStatusKey === 'Active';
        if (currentlyPublished) {
            const draftStatusId = this.getStatusId('Draft');
            if (!draftStatusId) {
                this.toastError('Draft status is missing.');
                return;
            }
            this.updateItemStatus(row, draftStatusId, 'Removed from shop.');
            return;
        }

        const missing = this.getMissingPublishFields(row.item);
        if (missing.length > 0) {
            this.openPublishDialog(row, true);
            this.toastError(`Complete required details before publishing: ${missing.join(', ')}`);
            return;
        }

        this.itemsApi.getItemMedia(row.item.id).subscribe({
            next: (media) => {
                if (!media || media.length === 0) {
                    this.openPublishDialog(row, true);
                    this.toastError('Image is required before publishing.');
                    return;
                }
                const activeStatusId = this.getStatusId('Active');
                if (!activeStatusId) {
                    this.toastError('Active status is missing.');
                    return;
                }
                this.updateItemStatus(row, activeStatusId, 'Item published to shop.');
            },
            error: () => {
                this.openPublishDialog(row, true);
                this.toastError('Image is required before publishing.');
            }
        });
    }

    deleteStorageItem(row: StorageItemVm): void {
        const confirmed = window.confirm(`Delete storage item "${row.item.nameEn}"? This cannot be undone.`);
        if (!confirmed) return;

        this.itemsApi.deleteItem(row.item.id).subscribe({
            next: () => {
                this.toastSuccess('Storage item deleted.');
                this.loadStorageItems();
            },
            error: (err) => {
                this.toastError(err?.error?.message || 'Failed to delete storage item.');
            }
        });
    }

    openPublishDialog(row: StorageItemVm, publishAfterSave: boolean): void {
        this.selectedRow = row;
        this.publishAfterSave = publishAfterSave;
        this.publishImageFile = null;
        this.publishImagePreviewUrl = '';
        this.publishModel = {
            categoryId: row.item.categoryId || '',
            price: Number(row.item.price || 0),
            generalDescriptionEn: row.item.generalDescriptionEn || '',
            generalDescriptionAr: row.item.generalDescriptionAr || ''
        };
        this.showPublishDialog = true;
    }

    savePublishDetails(): void {
        if (!this.selectedRow) return;

        this.savingPublishDetails = true;
        const row = this.selectedRow;
        const updatedPayload = {
            nameEn: row.item.nameEn,
            nameAr: row.item.nameAr,
            sku: row.item.sku,
            itemStatusId: row.item.itemStatusId,
            price: Number(this.publishModel.price || 0),
            currency: row.item.currency || 'USD',
            brandId: row.item.brandId || undefined,
            categoryId: this.publishModel.categoryId || undefined,
            tagIds: row.item.tagIds || [],
            isFeatured: row.item.isFeatured || false,
            isNew: row.item.isNew || false,
            isOnSale: row.item.isOnSale || false,
            generalDescriptionEn: this.publishModel.generalDescriptionEn || undefined,
            generalDescriptionAr: this.publishModel.generalDescriptionAr || undefined
        };

        this.itemsApi.updateItem(row.item.id, updatedPayload).subscribe({
            next: (updated) => {
                this.uploadAndLinkImageIfAny(updated.id, this.publishImageFile, () => {
                    if (!this.publishAfterSave) {
                        this.savingPublishDetails = false;
                        this.showPublishDialog = false;
                        this.toastSuccess('Details saved.');
                        this.loadStorageItems();
                        return;
                    }

                    const missing = this.getMissingPublishFields(updated);
                    if (missing.length > 0) {
                        this.savingPublishDetails = false;
                        this.toastError(`Still missing: ${missing.join(', ')}`);
                        return;
                    }

                    this.itemsApi.getItemMedia(updated.id).subscribe({
                        next: (media) => {
                            if (!media || media.length === 0) {
                                this.savingPublishDetails = false;
                                this.toastError('Image is required before publishing.');
                                return;
                            }
                            const activeStatusId = this.getStatusId('Active');
                            if (!activeStatusId) {
                                this.savingPublishDetails = false;
                                this.toastError('Active status is missing.');
                                return;
                            }
                            this.itemsApi.updateItem(updated.id, {
                                ...updatedPayload,
                                itemStatusId: activeStatusId
                            }).subscribe({
                                next: () => {
                                    this.savingPublishDetails = false;
                                    this.showPublishDialog = false;
                                    this.toastSuccess('Item published to shop.');
                                    this.loadStorageItems();
                                },
                                error: () => {
                                    this.savingPublishDetails = false;
                                    this.toastError('Failed to publish item.');
                                }
                            });
                        },
                        error: () => {
                            this.savingPublishDetails = false;
                            this.toastError('Image is required before publishing.');
                        }
                    });
                });
            },
            error: (err) => {
                this.savingPublishDetails = false;
                this.toastError(err?.error?.message || 'Failed to save details.');
            }
        });
    }

    private updateItemStatus(row: StorageItemVm, statusId: string, successMessage: string): void {
        this.itemsApi.updateItem(row.item.id, {
            nameEn: row.item.nameEn,
            nameAr: row.item.nameAr,
            sku: row.item.sku,
            itemStatusId: statusId,
            price: row.item.price,
            currency: row.item.currency,
            brandId: row.item.brandId || undefined,
            categoryId: row.item.categoryId || undefined,
            tagIds: row.item.tagIds || [],
            isFeatured: row.item.isFeatured || false,
            isNew: row.item.isNew || false,
            isOnSale: row.item.isOnSale || false,
            generalDescriptionEn: row.item.generalDescriptionEn || undefined,
            generalDescriptionAr: row.item.generalDescriptionAr || undefined
        }).subscribe({
            next: () => {
                this.toastSuccess(successMessage);
                this.loadStorageItems();
            },
            error: () => this.toastError('Failed to update publish status.')
        });
    }

    private getMissingPublishFields(item: Item): string[] {
        const missing: string[] = [];
        if (!item.categoryId) missing.push('category');
        if (!item.price || item.price <= 0) missing.push('price');
        if (!item.generalDescriptionEn?.trim()) missing.push('description (EN)');
        if (!item.generalDescriptionAr?.trim()) missing.push('description (AR)');
        return missing;
    }

    private getStatusId(statusKey: string): string | null {
        const found = this.statuses.find((x) => x.key?.toLowerCase() === statusKey.toLowerCase());
        return found?.id || null;
    }

    private uploadAndLinkImageIfAny(itemId: string, file: File | null, done: () => void): void {
        if (!file) {
            done();
            return;
        }

        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaTypeId', this.defaultImageMediaTypeId);

        this.mediaApi.uploadMedia(formData).subscribe({
            next: (asset) => {
                this.itemsApi.linkMediaToItem(itemId, {
                    mediaAssetId: asset.id,
                    isPrimary: true,
                    sortOrder: 0
                }).subscribe({
                    next: () => done(),
                    error: () => done()
                });
            },
            error: () => done()
        });
    }

    private resolveImageUrl(url?: string): string {
        if (!url?.trim()) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        const base = (environment.apiUrl ?? '').replace(/\/api\/v1\/?$/, '');
        return base ? `${base}${url.startsWith('/') ? url : `/${url}`}` : url;
    }

    private toastSuccess(detail: string): void {
        this.messageService.add({ severity: 'success', summary: 'Success', detail, life: 3000 });
    }

    private toastError(detail: string): void {
        this.messageService.add({ severity: 'error', summary: 'Error', detail, life: 4500 });
    }
}
