import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { MediaApiService } from '../../../shared/services/media-api.service';
import {
    StorageInventoryService, StorageParentDto, StorageVariantDto,
    UpsertStorageParentRequest, UpsertStorageVariantRequest, STORAGE_CATEGORIES
} from '../../../shared/services/storage-inventory.service';
import { environment } from '../../../../environments/environment';

interface ParentRow extends StorageParentDto {
    expanded: boolean;
    loadingVariants: boolean;
    variants: StorageVariantDto[];
    savingVariantIds: Set<string>;
}

@Component({
    selector: 'app-storage-inventory',
    standalone: true,
    imports: [
        CommonModule, FormsModule,
        ButtonModule, InputTextModule, InputNumberModule, SelectModule,
        TagModule, ToastModule, TooltipModule, DialogModule, CheckboxModule
    ],
    providers: [MessageService],
    template: `
    <p-toast />

    <!-- ── Toolbar ── -->
    <div class="inv-page">
        <div class="toolbar">
            <div class="toolbar-left">
                <h1>Storage Inventory</h1>
                <span class="item-count">{{ rows().length }} items</span>
            </div>
            <div class="toolbar-right">
                <div class="search-box">
                    <i class="pi pi-search"></i>
                    <input pInputText [(ngModel)]="search" placeholder="Search items, SKU, value, package…" (input)="onSearch()" />
                </div>
                <select class="filter-select" [(ngModel)]="categoryFilter" (change)="load()">
                    <option value="">All Categories</option>
                    @for (c of categories; track c) {
                        <option [value]="c">{{ c }}</option>
                    }
                </select>
                <select class="filter-select" [(ngModel)]="shopFilter" (change)="load()">
                    <option value="">All</option>
                    <option value="true">Published</option>
                    <option value="false">Unpublished</option>
                </select>
                <p-button label="Add Item" icon="pi pi-plus" size="small" (click)="openParentDialog(null)" />
            </div>
        </div>

        <!-- ── Items table ── -->
        <div class="items-table">
            <!-- Header -->
            <div class="table-head">
                <div class="col-expand"></div>
                <div class="col-image"></div>
                <div class="col-name">Item</div>
                <div class="col-cat">Category</div>
                <div class="col-num">Variants</div>
                <div class="col-num">Total Qty</div>
                <div class="col-status">Shop</div>
                <div class="col-actions">Actions</div>
            </div>

            @if (loading()) {
                <div class="empty-row"><i class="pi pi-spin pi-spinner"></i> Loading…</div>
            } @else if (rows().length === 0) {
                <div class="empty-row">No storage items found. <a (click)="openParentDialog(null)" class="link">Add your first item →</a></div>
            } @else {
                @for (row of rows(); track row.id) {
                    <!-- Parent row -->
                    <div class="parent-row" [class.expanded]="row.expanded">
                        <div class="col-expand">
                            <button class="expand-btn" (click)="toggleExpand(row)" [pTooltip]="row.expanded ? 'Collapse' : 'Expand variants'">
                                <i [class]="row.expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"></i>
                            </button>
                        </div>
                        <div class="col-image">
                            <img *ngIf="row.imageUrl" [src]="resolveUrl(row.imageUrl)" alt="" class="item-thumb" />
                            <div *ngIf="!row.imageUrl" class="thumb-placeholder"><i class="pi pi-box"></i></div>
                        </div>
                        <div class="col-name">
                            <span class="item-name">{{ row.name }}</span>
                            <span class="item-mfr" *ngIf="row.manufacturer">{{ row.manufacturer }}</span>
                        </div>
                        <div class="col-cat"><span class="cat-badge">{{ row.category }}</span></div>
                        <div class="col-num">{{ row.variantCount }}</div>
                        <div class="col-num" [class.low-stock]="row.totalQuantity < 10">{{ row.totalQuantity }}</div>
                        <div class="col-status">
                            <p-tag
                                [value]="row.isPublishedToShop ? 'Published' : 'Draft'"
                                [severity]="row.isPublishedToShop ? 'success' : 'secondary'"
                            />
                        </div>
                        <div class="col-actions">
                            <button class="act-btn" (click)="openVariantDialog(row, null)" title="Add variant"><i class="pi pi-plus"></i></button>
                            <button class="act-btn" (click)="openParentDialog(row)" title="Edit item"><i class="pi pi-pencil"></i></button>
                            <button class="act-btn" [class.act-btn-warn]="row.isPublishedToShop" (click)="toggleParentShop(row)" [title]="row.isPublishedToShop ? 'Unpublish' : 'Publish to shop'">
                                <i [class]="row.isPublishedToShop ? 'pi pi-eye-slash' : 'pi pi-shopping-bag'"></i>
                            </button>
                            <button class="act-btn act-btn-danger" (click)="deleteParent(row)" title="Delete item"><i class="pi pi-trash"></i></button>
                        </div>
                    </div>

                    <!-- Variants table (expanded) -->
                    @if (row.expanded) {
                        <div class="variants-container">
                            @if (row.loadingVariants) {
                                <div class="variants-loading"><i class="pi pi-spin pi-spinner"></i></div>
                            } @else if (row.variants.length === 0) {
                                <div class="variants-empty">
                                    No variants yet.
                                    <a (click)="openVariantDialog(row, null)" class="link">Add first variant →</a>
                                </div>
                            } @else {
                                <div class="variants-table">
                                    <div class="vrow vhead">
                                        <div class="vc-label">Variant</div>
                                        <div class="vc-sku">SKU</div>
                                        <div class="vc-pkg">Package</div>
                                        <div class="vc-val">Value</div>
                                        <div class="vc-qty">Qty</div>
                                        <div class="vc-shop">Shop</div>
                                        <div class="vc-act">Actions</div>
                                    </div>
                                    @for (v of row.variants; track v.id) {
                                        <div class="vrow vbody" [class.inactive]="!v.isActive">
                                            <div class="vc-label">
                                                <span>{{ v.variantLabel || '—' }}</span>
                                                <span class="v-specs" *ngIf="v.tolerance">{{ v.tolerance }}</span>
                                            </div>
                                            <div class="vc-sku mono">{{ v.sku }}</div>
                                            <div class="vc-pkg">{{ v.package || '—' }}</div>
                                            <div class="vc-val">
                                                {{ v.value || '' }}{{ v.valueUnit || '' }}
                                                <span *ngIf="!v.value && !v.valueUnit">—</span>
                                            </div>
                                            <div class="vc-qty">
                                                <div class="qty-ctrl">
                                                    <button class="qty-btn" (click)="adjustQty(row, v, -1)" [disabled]="row.savingVariantIds.has(v.id)">−</button>
                                                    <input type="number" class="qty-input" [ngModel]="v.quantity"
                                                        (ngModelChange)="v.quantity = $event"
                                                        (blur)="saveQty(row, v)" [disabled]="row.savingVariantIds.has(v.id)" />
                                                    <button class="qty-btn" (click)="adjustQty(row, v, 1)" [disabled]="row.savingVariantIds.has(v.id)">+</button>
                                                </div>
                                            </div>
                                            <div class="vc-shop">
                                                <p-tag [value]="v.isPublishedToShop ? '✓' : '–'" [severity]="v.isPublishedToShop ? 'success' : 'secondary'" />
                                            </div>
                                            <div class="vc-act">
                                                <button class="act-btn" (click)="openVariantDialog(row, v)" title="Edit"><i class="pi pi-pencil"></i></button>
                                                <button class="act-btn" [class.act-btn-warn]="v.isPublishedToShop" (click)="toggleVariantShop(row, v)" [title]="v.isPublishedToShop ? 'Unpublish' : 'Publish'">
                                                    <i [class]="v.isPublishedToShop ? 'pi pi-eye-slash' : 'pi pi-shopping-bag'"></i>
                                                </button>
                                                <button class="act-btn act-btn-danger" (click)="deleteVariant(row, v)" title="Delete"><i class="pi pi-trash"></i></button>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }

                            <!-- Quick add variant inline form -->
                            <div class="quick-add-form">
                                <span class="qa-label">Quick add:</span>
                                <input pInputText class="qa-input" [(ngModel)]="quick.value" placeholder="Value (e.g. 10)"
                                    (ngModelChange)="autoSku(row)" />
                                <input pInputText class="qa-input qa-sm" [(ngModel)]="quick.valueUnit" placeholder="Unit (kΩ)"
                                    (ngModelChange)="autoSku(row)" />
                                <input pInputText class="qa-input qa-sm" [(ngModel)]="quick.package" placeholder="Package"
                                    (ngModelChange)="autoSku(row)" />
                                <input pInputText class="qa-input qa-sm" [(ngModel)]="quick.tolerance" placeholder="±%" />
                                <input pInputText class="qa-input" [(ngModel)]="quick.sku" placeholder="SKU (auto)" title="Auto-generated — edit to override" />
                                <input type="number" class="qa-input qa-num" [(ngModel)]="quick.quantity" placeholder="Qty" min="0" />
                                <p-button label="Add" size="small" [loading]="quickAdding" (click)="quickAddVariant(row)" />
                            </div>
                        </div>
                    }
                }
            }
        </div>
    </div>

    <!-- ── Parent Dialog ── -->
    <p-dialog [(visible)]="parentDialog.visible" [modal]="true" [style]="{width:'560px'}">
        <ng-template pTemplate="header">
            <div class="dlg-header">
                <span class="dlg-label">Storage</span>
                <span class="dlg-title">{{ parentDialog.isEdit ? 'Edit Item' : 'New Item' }}</span>
            </div>
        </ng-template>
        <div class="dlg-form">
            <div class="form-row-2">
                <div class="ff"><label>Name *</label><input pInputText [(ngModel)]="parentDialog.data.name" class="w-full" /></div>
                <div class="ff"><label>Category</label>
                    <select class="filter-select w-full" [(ngModel)]="parentDialog.data.category">
                        @for (c of categories; track c) { <option [value]="c">{{ c }}</option> }
                    </select>
                </div>
            </div>
            <div class="ff"><label>Manufacturer / Brand</label><input pInputText [(ngModel)]="parentDialog.data.manufacturer" class="w-full" /></div>
            <div class="ff"><label>Description</label><textarea pInputText [(ngModel)]="parentDialog.data.description" rows="2" class="w-full"></textarea></div>
            <div class="ff">
                <label>Image</label>
                <input type="file" #pImgInput accept="image/*" class="hidden-input" (change)="onParentImageSelected($event)" />
                <div class="upload-row">
                    <p-button [label]="parentDialog.data.imageUrl ? 'Replace' : 'Upload'" icon="pi pi-image" [outlined]="true" size="small" [loading]="uploadingParentImg" (click)="pImgInput.click()" />
                    <button *ngIf="parentDialog.data.imageUrl" type="button" class="clear-btn" (click)="parentDialog.data.imageUrl = ''">×</button>
                </div>
                <img *ngIf="parentDialog.data.imageUrl" [src]="resolveUrl(parentDialog.data.imageUrl)" class="dlg-img-preview" />
            </div>
            <div class="ff"><label>Datasheet URL (optional)</label><input pInputText [(ngModel)]="parentDialog.data.datasheetUrl" class="w-full" placeholder="https://…" /></div>
            <div class="form-row-checks">
                <label class="check-label"><p-checkbox [(ngModel)]="parentDialog.data.isActive" [binary]="true" inputId="pActive"></p-checkbox> Active</label>
                <label class="check-label"><p-checkbox [(ngModel)]="parentDialog.data.isPublishedToShop" [binary]="true" inputId="pShop"></p-checkbox> Published to Shop</label>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="Cancel" [text]="true" (click)="parentDialog.visible = false" />
            <p-button label="Save" [loading]="parentDialog.saving" (click)="saveParent()" />
        </ng-template>
    </p-dialog>

    <!-- ── Variant Dialog ── -->
    <p-dialog [(visible)]="variantDialog.visible" [modal]="true" [style]="{width:'600px'}">
        <ng-template pTemplate="header">
            <div class="dlg-header">
                <span class="dlg-label">{{ variantDialog.parentName }}</span>
                <span class="dlg-title">{{ variantDialog.isEdit ? 'Edit Variant' : 'Add Variant' }}</span>
            </div>
        </ng-template>
        <div class="dlg-form">
            <div class="form-row-2">
                <div class="ff"><label>Value</label><input pInputText [(ngModel)]="variantDialog.data.value" class="w-full" placeholder="10, 100, 47" /></div>
                <div class="ff"><label>Unit</label><input pInputText [(ngModel)]="variantDialog.data.valueUnit" class="w-full" placeholder="kΩ, nF, µH, V" /></div>
            </div>
            <div class="form-row-2">
                <div class="ff"><label>Package / Footprint</label><input pInputText [(ngModel)]="variantDialog.data.package" class="w-full" placeholder="0805, TO-220, DIP-8" /></div>
                <div class="ff"><label>Tolerance</label><input pInputText [(ngModel)]="variantDialog.data.tolerance" class="w-full" placeholder="±5%, ±1%" /></div>
            </div>
            <div class="form-row-3">
                <div class="ff"><label>Voltage Rating</label><input pInputText [(ngModel)]="variantDialog.data.voltageRating" class="w-full" placeholder="50V" /></div>
                <div class="ff"><label>Current Rating</label><input pInputText [(ngModel)]="variantDialog.data.currentRating" class="w-full" placeholder="1A" /></div>
                <div class="ff"><label>Power Rating</label><input pInputText [(ngModel)]="variantDialog.data.powerRating" class="w-full" placeholder="0.25W" /></div>
            </div>
            <div class="form-row-2">
                <div class="ff"><label>SKU *</label><input pInputText [(ngModel)]="variantDialog.data.sku" class="w-full" placeholder="R-10K-0805" /></div>
                <div class="ff"><label>Quantity</label><input type="number" class="p-inputtext w-full" [(ngModel)]="variantDialog.data.quantity" min="0" /></div>
            </div>
            <div class="ff"><label>Label (auto-generated if blank)</label><input pInputText [(ngModel)]="variantDialog.data.variantLabel" class="w-full" placeholder="10kΩ 0805 ±5%" /></div>
            <div class="ff"><label>Notes</label><textarea pInputText [(ngModel)]="variantDialog.data.notes" rows="2" class="w-full"></textarea></div>
            <div class="ff"><label>Datasheet URL</label><input pInputText [(ngModel)]="variantDialog.data.datasheetUrl" class="w-full" /></div>
            <div class="form-row-checks">
                <label class="check-label"><p-checkbox [(ngModel)]="variantDialog.data.isActive" [binary]="true"></p-checkbox> Active</label>
                <label class="check-label"><p-checkbox [(ngModel)]="variantDialog.data.isPublishedToShop" [binary]="true"></p-checkbox> Published to Shop</label>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="Cancel" [text]="true" (click)="variantDialog.visible = false" />
            <p-button label="Save" [loading]="variantDialog.saving" (click)="saveVariant()" />
        </ng-template>
    </p-dialog>
    `,
    styles: [`
        .inv-page { padding: 1rem 1.5rem; }

        /* ── Toolbar ── */
        .toolbar { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
        .toolbar-left { display: flex; align-items: baseline; gap: 0.75rem; }
        .toolbar-left h1 { margin: 0; font-size: 1.35rem; font-weight: 700; }
        .item-count { font-size: 0.82rem; color: #9ca3af; }
        .toolbar-right { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
        .search-box { position: relative; }
        .search-box i { position: absolute; left: 0.65rem; top: 50%; transform: translateY(-50%); color: #9ca3af; font-size: 0.8rem; }
        .search-box input { height: 32px; padding: 0 0.6rem 0 2rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.85rem; width: 240px; }
        .filter-select { height: 32px; padding: 0 0.5rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.82rem; background: #fff; color: #374151; cursor: pointer; }

        /* ── Items table ── */
        .items-table { border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden; }
        .table-head {
            display: grid;
            grid-template-columns: 2rem 4.5rem 1fr 12rem 6rem 7rem 7rem 10rem;
            padding: 0.55rem 0.85rem;
            background: #f9fafb;
            border-bottom: 1px solid #e5e7eb;
            font-size: 0.71rem; font-weight: 700; text-transform: uppercase;
            letter-spacing: 0.06em; color: #6b7280;
            white-space: nowrap;
        }
        .parent-row {
            display: grid;
            grid-template-columns: 2rem 4.5rem 1fr 12rem 6rem 7rem 7rem 10rem;
            padding: 0.65rem 0.85rem;
            border-bottom: 1px solid #f3f4f6;
            align-items: center;
            background: #fff;
            transition: background 0.1s;
        }
        .parent-row:hover { background: #fafbff; }
        .parent-row.expanded { background: #f8f9ff; border-bottom-color: transparent; }
        .empty-row { padding: 2.5rem; text-align: center; color: #9ca3af; font-size: 0.9rem; }

        .col-expand { display: flex; align-items: center; }
        .col-image { display: flex; align-items: center; }
        .col-name { display: flex; flex-direction: column; gap: 0.1rem; overflow: hidden; }
        .col-cat { }
        .col-num { font-size: 0.875rem; color: #374151; font-weight: 500; }
        .col-num.low-stock { color: #ef4444; font-weight: 700; }
        .col-status { }
        .col-actions { display: flex; gap: 0.1rem; align-items: center; }
        .act-btn {
            width: 28px; height: 28px;
            display: inline-flex; align-items: center; justify-content: center;
            background: transparent; border: none; border-radius: 5px;
            color: #6b7280; cursor: pointer; padding: 0;
            transition: background 0.12s, color 0.12s;
            font-size: 0.82rem;
        }
        .act-btn:hover { background: #f3f4f6; color: #374151; }
        .act-btn-warn { color: #d97706; }
        .act-btn-warn:hover { background: #fef3c7; color: #b45309; }
        .act-btn-danger { color: #ef4444; }
        .act-btn-danger:hover { background: #fee2e2; color: #dc2626; }
        .vc-act { display: flex; gap: 0; }

        .expand-btn { background: none; border: none; color: #6b7280; cursor: pointer; padding: 0.25rem; border-radius: 4px; display: flex; align-items: center; font-size: 0.75rem; }
        .expand-btn:hover { background: #f3f4f6; color: #374151; }

        .item-thumb { width: 44px; height: 38px; object-fit: cover; border-radius: 6px; border: 1px solid #e5e7eb; }
        .thumb-placeholder { width: 44px; height: 38px; background: #f3f4f6; border-radius: 6px; display: flex; align-items: center; justify-content: center; color: #d1d5db; font-size: 1rem; }
        .item-name { font-size: 0.88rem; font-weight: 600; color: #111827; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .item-mfr { font-size: 0.75rem; color: #9ca3af; }
        .cat-badge { font-size: 0.72rem; background: #f3f4f6; color: #374151; padding: 0.15rem 0.45rem; border-radius: 3px; }

        /* ── Variants ── */
        .variants-container { background: #f8f9ff; border-bottom: 1px solid #e5e7eb; padding: 0 0 0.5rem 2.5rem; }
        .variants-loading, .variants-empty { padding: 0.75rem 1rem; font-size: 0.85rem; color: #9ca3af; }
        .variants-table { }
        .vrow { display: grid; grid-template-columns: 1fr 8rem 6rem 6rem 9rem 5rem 8rem; gap: 0; align-items: center; padding: 0.45rem 0.75rem; font-size: 0.83rem; }
        .vrow.vhead { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #9ca3af; padding-bottom: 0.35rem; border-bottom: 1px solid #e9ecef; }
        .vrow.vbody { border-bottom: 1px solid #f0f1f5; }
        .vrow.vbody:hover { background: #f0f2ff; }
        .vrow.inactive { opacity: 0.5; }
        .vc-label { display: flex; flex-direction: column; }
        .v-specs { font-size: 0.72rem; color: #9ca3af; }
        .mono { font-family: monospace; font-size: 0.78rem; color: #374151; }
        .vc-act { display: flex; gap: 0; }

        /* Quantity controls */
        .qty-ctrl { display: flex; align-items: center; gap: 2px; }
        .qty-btn { width: 22px; height: 22px; border: 1px solid #d1d5db; background: #fff; cursor: pointer; border-radius: 3px; font-size: 0.9rem; display: flex; align-items: center; justify-content: center; color: #374151; }
        .qty-btn:hover:not(:disabled) { background: #f3f4f6; }
        .qty-btn:disabled { opacity: 0.4; cursor: not-allowed; }
        .qty-input { width: 46px; height: 22px; text-align: center; border: 1px solid #d1d5db; border-radius: 3px; font-size: 0.82rem; padding: 0; }
        .qty-input::-webkit-inner-spin-button { display: none; }

        /* Quick add */
        .quick-add-form { display: flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; padding: 0.6rem 0.75rem; border-top: 1px dashed #e0e7ff; margin-top: 0.25rem; }
        .qa-label { font-size: 0.72rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #9ca3af; white-space: nowrap; }
        .qa-input { height: 28px; padding: 0 0.5rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.82rem; width: 110px; }
        .qa-input.qa-sm { width: 80px; }
        .qa-input.qa-num { width: 65px; }

        /* ── Dialogs ── */
        .dlg-form { display: flex; flex-direction: column; gap: 0.85rem; padding: 0.25rem 0; }
        .ff { display: flex; flex-direction: column; gap: 0.3rem; }
        .ff label { font-size: 0.8rem; font-weight: 600; color: #374151; }
        .form-row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .form-row-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem; }
        .form-row-checks { display: flex; gap: 1.5rem; align-items: center; }
        .check-label { display: flex; align-items: center; gap: 0.4rem; font-size: 0.85rem; cursor: pointer; }
        textarea { resize: vertical; min-height: 52px; }
        .w-full { width: 100%; box-sizing: border-box; }
        .hidden-input { display: none; }
        .upload-row { display: flex; align-items: center; gap: 0.4rem; }
        .clear-btn { background: none; border: none; color: #9ca3af; cursor: pointer; font-size: 1.1rem; padding: 0 0.2rem; }
        .clear-btn:hover { color: #ef4444; }
        .dlg-img-preview { width: 100%; max-height: 100px; object-fit: cover; border-radius: 6px; border: 1px solid #e5e7eb; margin-top: 0.4rem; }
        .link { color: #667eea; cursor: pointer; text-decoration: underline; }
    `]
})
export class StorageInventoryComponent implements OnInit {
    private svc = inject(StorageInventoryService);
    private mediaApi = inject(MediaApiService);
    private msgService = inject(MessageService);

    private readonly MEDIA_TYPE_IMAGE = '00000000-0000-0000-0000-000000000001';

    loading = signal(false);
    rows = signal<ParentRow[]>([]);

    search = '';
    categoryFilter = '';
    shopFilter = '';

    categories = STORAGE_CATEGORIES;

    uploadingParentImg = false;
    quickAdding = false;
    quick = this.emptyQuick();

    parentDialog = {
        visible: false, isEdit: false, saving: false,
        editId: '' as string,
        data: this.emptyParentData()
    };

    variantDialog = {
        visible: false, isEdit: false, saving: false,
        parentId: '', parentName: '', editId: '',
        data: this.emptyVariantData()
    };

    private searchTimer: ReturnType<typeof setTimeout> | null = null;

    ngOnInit() { this.load(); }

    load() {
        this.loading.set(true);
        this.svc.getAll({
            search: this.search || undefined,
            category: this.categoryFilter || undefined,
            publishedToShop: this.shopFilter === '' ? undefined : this.shopFilter === 'true'
        }).subscribe({
            next: (list) => {
                const current = this.rows();
                this.rows.set(list.map(p => {
                    const existing = current.find(r => r.id === p.id);
                    return {
                        ...p,
                        expanded: existing?.expanded ?? false,
                        loadingVariants: false,
                        variants: existing?.variants ?? [],
                        savingVariantIds: existing?.savingVariantIds ?? new Set()
                    } as ParentRow;
                }));
                this.loading.set(false);
            },
            error: () => { this.loading.set(false); this.toast('error', 'Failed to load storage items.'); }
        });
    }

    onSearch() {
        if (this.searchTimer) clearTimeout(this.searchTimer);
        this.searchTimer = setTimeout(() => this.load(), 350);
    }

    toggleExpand(row: ParentRow) {
        row.expanded = !row.expanded;
        if (row.expanded && row.variants.length === 0) this.loadVariants(row);
        this.quick = this.emptyQuick();
    }

    loadVariants(row: ParentRow) {
        row.loadingVariants = true;
        this.svc.getById(row.id).subscribe({
            next: (p) => { row.variants = p.variants ?? []; row.loadingVariants = false; },
            error: () => { row.loadingVariants = false; }
        });
    }

    // ── Parent CRUD ────────────────────────────────────────────────────────

    openParentDialog(row: ParentRow | null) {
        this.parentDialog.isEdit = !!row;
        this.parentDialog.editId = row?.id ?? '';
        this.parentDialog.saving = false;
        this.parentDialog.data = row ? {
            name: row.name, description: row.description ?? '',
            category: row.category, manufacturer: row.manufacturer ?? '',
            imageUrl: row.imageUrl ?? '', datasheetUrl: row.datasheetUrl ?? '',
            isPublishedToShop: row.isPublishedToShop, isActive: row.isActive, sortOrder: row.sortOrder
        } : this.emptyParentData();
        this.parentDialog.visible = true;
    }

    onParentImageSelected(event: Event) {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;
        this.uploadingParentImg = true;
        const form = new FormData();
        form.append('file', file);
        form.append('mediaTypeId', this.MEDIA_TYPE_IMAGE);
        this.mediaApi.uploadMedia(form).subscribe({
            next: (a) => { this.parentDialog.data.imageUrl = a.fileUrl; this.uploadingParentImg = false; },
            error: () => { this.uploadingParentImg = false; this.toast('error', 'Image upload failed.'); }
        });
    }

    saveParent() {
        if (!this.parentDialog.data.name?.trim()) return this.toast('warn', 'Name is required.');
        this.parentDialog.saving = true;
        const req: UpsertStorageParentRequest = { ...this.parentDialog.data, name: this.parentDialog.data.name.trim() };
        const call = this.parentDialog.isEdit
            ? this.svc.update(this.parentDialog.editId, req)
            : this.svc.create(req);
        call.subscribe({
            next: () => { this.parentDialog.visible = false; this.parentDialog.saving = false; this.load(); },
            error: () => { this.parentDialog.saving = false; this.toast('error', 'Save failed.'); }
        });
    }

    toggleParentShop(row: ParentRow) {
        this.svc.toggleShop(row.id).subscribe({
            next: (updated) => {
                this.rows.update(list => list.map(r => r.id === row.id ? { ...r, isPublishedToShop: updated.isPublishedToShop } : r));
            },
            error: () => this.toast('error', 'Update failed.')
        });
    }

    deleteParent(row: ParentRow) {
        if (!confirm(`Delete "${row.name}" and all its variants? This cannot be undone.`)) return;
        this.svc.delete(row.id).subscribe({
            next: () => { this.rows.update(l => l.filter(r => r.id !== row.id)); },
            error: () => this.toast('error', 'Delete failed.')
        });
    }

    // ── Variant CRUD ───────────────────────────────────────────────────────

    openVariantDialog(row: ParentRow, v: StorageVariantDto | null) {
        this.variantDialog.isEdit = !!v;
        this.variantDialog.parentId = row.id;
        this.variantDialog.parentName = row.name;
        this.variantDialog.editId = v?.id ?? '';
        this.variantDialog.saving = false;
        this.variantDialog.data = v ? {
            variantLabel: v.variantLabel ?? '', sku: v.sku, quantity: v.quantity, unit: v.unit,
            package: v.package ?? '', value: v.value ?? '', valueUnit: v.valueUnit ?? '',
            tolerance: v.tolerance ?? '', voltageRating: v.voltageRating ?? '',
            currentRating: v.currentRating ?? '', powerRating: v.powerRating ?? '',
            notes: v.notes ?? '', imageUrl: v.imageUrl ?? '', datasheetUrl: v.datasheetUrl ?? '',
            isActive: v.isActive, isPublishedToShop: v.isPublishedToShop
        } : this.emptyVariantData();
        this.variantDialog.visible = true;
    }

    saveVariant() {
        if (!this.variantDialog.data.sku?.trim()) return this.toast('warn', 'SKU is required.');
        this.variantDialog.saving = true;
        const req: UpsertStorageVariantRequest = { ...this.variantDialog.data };
        const row = this.rows().find(r => r.id === this.variantDialog.parentId);
        const call = this.variantDialog.isEdit
            ? this.svc.updateVariant(this.variantDialog.parentId, this.variantDialog.editId, req)
            : this.svc.addVariant(this.variantDialog.parentId, req);
        call.subscribe({
            next: (saved) => {
                this.variantDialog.visible = false;
                this.variantDialog.saving = false;
                if (row) {
                    if (this.variantDialog.isEdit) {
                        row.variants = row.variants.map(v => v.id === saved.id ? saved : v);
                    } else {
                        row.variants = [...row.variants, saved];
                        row.variantCount = row.variants.filter(v => v.isActive).length;
                        row.totalQuantity = row.variants.filter(v => v.isActive).reduce((s, v) => s + v.quantity, 0);
                    }
                    this.rows.update(l => [...l]);
                }
            },
            error: (err) => { this.variantDialog.saving = false; this.toast('error', err?.error?.message || 'Save failed.'); }
        });
    }

    adjustQty(row: ParentRow, v: StorageVariantDto, delta: number) {
        const newQty = Math.max(0, v.quantity + delta);
        v.quantity = newQty;
        this.saveQty(row, v);
    }

    saveQty(row: ParentRow, v: StorageVariantDto) {
        row.savingVariantIds = new Set([...row.savingVariantIds, v.id]);
        this.svc.setQuantity(row.id, v.id, Math.max(0, v.quantity)).subscribe({
            next: (updated) => {
                v.quantity = updated.quantity;
                row.savingVariantIds = new Set([...row.savingVariantIds].filter(id => id !== v.id));
                row.totalQuantity = row.variants.filter(x => x.isActive).reduce((s, x) => s + x.quantity, 0);
                this.rows.update(l => [...l]);
            },
            error: () => { row.savingVariantIds = new Set([...row.savingVariantIds].filter(id => id !== v.id)); }
        });
    }

    toggleVariantShop(row: ParentRow, v: StorageVariantDto) {
        this.svc.toggleVariantShop(row.id, v.id).subscribe({
            next: (updated) => {
                row.variants = row.variants.map(x => x.id === v.id ? updated : x);
                this.rows.update(l => [...l]);
            },
            error: () => this.toast('error', 'Update failed.')
        });
    }

    deleteVariant(row: ParentRow, v: StorageVariantDto) {
        if (!confirm(`Delete variant "${v.variantLabel || v.sku}"?`)) return;
        this.svc.deleteVariant(row.id, v.id).subscribe({
            next: () => {
                row.variants = row.variants.filter(x => x.id !== v.id);
                row.variantCount = row.variants.filter(x => x.isActive).length;
                row.totalQuantity = row.variants.filter(x => x.isActive).reduce((s, x) => s + x.quantity, 0);
                this.rows.update(l => [...l]);
            },
            error: () => this.toast('error', 'Delete failed.')
        });
    }

    // ── Quick Add ──────────────────────────────────────────────────────────

    autoSku(row: ParentRow) {
        // Build SKU from: first 3 letters of parent name + value + valueUnit + package
        const prefix = row.name.replace(/[^a-zA-Z0-9]/g, '').slice(0, 3).toUpperCase();
        const val = (this.quick.value || '').replace(/[^a-zA-Z0-9]/g, '');
        const unit = (this.quick.valueUnit || '').replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
        const pkg = (this.quick.package || '').replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
        const parts = [prefix, val + unit, pkg].filter(p => p);
        this.quick.sku = parts.join('-');
    }

    quickAddVariant(row: ParentRow) {
        // Auto-generate SKU if still blank
        if (!this.quick.sku?.trim()) this.autoSku(row);
        if (!this.quick.sku?.trim()) {
            // Last resort: timestamp-based unique SKU
            this.quick.sku = `${row.name.replace(/[^a-zA-Z0-9]/g,'').slice(0,4).toUpperCase()}-${Date.now().toString().slice(-6)}`;
        }
        this.quickAdding = true;
        const req: UpsertStorageVariantRequest = {
            variantLabel: '', sku: this.quick.sku.trim(),
            quantity: this.quick.quantity ?? 0, unit: 'pcs',
            package: this.quick.package || undefined,
            value: this.quick.value || undefined,
            valueUnit: this.quick.valueUnit || undefined,
            tolerance: this.quick.tolerance || undefined,
            isActive: true, isPublishedToShop: false
        };
        this.svc.addVariant(row.id, req).subscribe({
            next: (saved) => {
                row.variants = [...row.variants, saved];
                row.variantCount = row.variants.filter(v => v.isActive).length;
                row.totalQuantity = row.variants.filter(v => v.isActive).reduce((s, v) => s + v.quantity, 0);
                this.rows.update(l => [...l]);
                this.quick = this.emptyQuick();
                this.quickAdding = false;
            },
            error: (err) => { this.quickAdding = false; this.toast('error', err?.error?.message || 'Quick add failed.'); }
        });
    }

    resolveUrl(url: string): string {
        if (!url || url.startsWith('http') || url.startsWith('/assets')) return url;
        const base = (environment.apiUrl ?? '').replace(/\/api\/v1\/?$/, '');
        return base ? `${base}${url.startsWith('/') ? url : '/' + url}` : url;
    }

    private toast(severity: string, detail: string) {
        this.msgService.add({ severity, summary: severity === 'error' ? 'Error' : severity === 'warn' ? 'Warning' : 'Done', detail, life: 4000 });
    }

    private emptyParentData() {
        return { name: '', description: '', category: 'Other', manufacturer: '', imageUrl: '', datasheetUrl: '', isPublishedToShop: false, isActive: true, sortOrder: 0 };
    }

    private emptyVariantData(): UpsertStorageVariantRequest {
        return { variantLabel: '', sku: '', quantity: 0, unit: 'pcs', package: '', value: '', valueUnit: '', tolerance: '', voltageRating: '', currentRating: '', powerRating: '', notes: '', imageUrl: '', datasheetUrl: '', isActive: true, isPublishedToShop: false };
    }

    private emptyQuick() {
        return { value: '', valueUnit: '', package: '', tolerance: '', sku: '', quantity: 0 };
    }
}
