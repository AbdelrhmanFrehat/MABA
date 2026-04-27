import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { Material, MaterialColor, FilamentSpool } from '../../../shared/models/printing.model';
import { TranslateService } from '@ngx-translate/core';

interface MaterialRow extends Material {
    expanded: boolean;
    spools: FilamentSpool[];
    loadingSpools: boolean;
    savingSpoolIds: Set<string>;
}

@Component({
    selector: 'app-printing-inventory',
    standalone: true,
    imports: [
        CommonModule, FormsModule,
        ButtonModule, TagModule, ToastModule, TooltipModule,
        DialogModule, InputTextModule, InputNumberModule, SelectModule, CheckboxModule
    ],
    providers: [MessageService],
    template: `
    <p-toast />

    <div class="pi-page">
        <!-- Toolbar -->
        <div class="toolbar">
            <div class="toolbar-left">
                <h1>Filament Inventory</h1>
                <span class="sub">Materials &amp; Spools</span>
            </div>
            <div class="toolbar-right">
                <input pInputText [(ngModel)]="search" placeholder="Search materials…" class="search-input" (input)="filterRows()" />
                <p-button label="Add Spool" icon="pi pi-plus" size="small" (click)="openSpoolDialog(null)" />
            </div>
        </div>

        <!-- Materials table -->
        <div class="mat-table">
            <div class="mat-head">
                <div class="c-exp"></div>
                <div class="c-name">Material</div>
                <div class="c-num">Spools</div>
                <div class="c-num">Total Remaining</div>
                <div class="c-price">Price/g</div>
                <div class="c-status">Status</div>
                <div class="c-actions">Actions</div>
            </div>

            @if (loading()) {
                <div class="empty-row"><i class="pi pi-spin pi-spinner"></i> Loading…</div>
            } @else if (visibleRows().length === 0) {
                <div class="empty-row">No materials found.</div>
            } @else {
                @for (row of visibleRows(); track row.id) {
                    <!-- Material row -->
                    <div class="mat-row" [class.expanded]="row.expanded">
                        <div class="c-exp">
                            <button class="exp-btn" (click)="toggleExpand(row)" [pTooltip]="row.expanded ? 'Collapse' : 'Show spools'">
                                <i [class]="row.expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"></i>
                            </button>
                        </div>
                        <div class="c-name">
                            <span class="mat-name">{{ row.nameEn }}</span>
                            <span class="mat-name-ar" *ngIf="row.nameAr">{{ row.nameAr }}</span>
                        </div>
                        <div class="c-num">{{ activeSpoolCount(row) }}</div>
                        <div class="c-num" [class.stock-low]="totalRemaining(row) < 100">
                            {{ totalRemaining(row) | number }}g
                        </div>
                        <div class="c-price">₪{{ row.pricePerGram | number:'1.3-3' }}/g</div>
                        <div class="c-status">
                            <p-tag [value]="row.isActive ? 'Active' : 'Inactive'"
                                   [severity]="row.isActive ? 'success' : 'secondary'" />
                        </div>
                        <div class="c-actions">
                            <p-button icon="pi pi-plus" [text]="true" size="small" severity="secondary"
                                (click)="openSpoolDialog(row)" pTooltip="Add spool" />
                        </div>
                    </div>

                    <!-- Spools (expanded) -->
                    @if (row.expanded) {
                        <div class="spools-section">
                            @if (row.loadingSpools) {
                                <div class="spool-empty"><i class="pi pi-spin pi-spinner"></i></div>
                            } @else if (row.spools.length === 0) {
                                <div class="spool-empty">
                                    No spools. <a class="link" (click)="openSpoolDialog(row)">Add first spool →</a>
                                </div>
                            } @else {
                                <div class="spool-table">
                                    <div class="srow shead">
                                        <div class="sc-color">Color</div>
                                        <div class="sc-rem">Remaining</div>
                                        <div class="sc-init">Initial</div>
                                        <div class="sc-pct">Fill</div>
                                        <div class="sc-status">Status</div>
                                        <div class="sc-date">Added</div>
                                        <div class="sc-act">Actions</div>
                                    </div>
                                    @for (s of row.spools; track s.id) {
                                        <div class="srow sbody" [class.inactive]="!s.isActive">
                                            <div class="sc-color">
                                                <span class="color-dot" [style.background]="s.colorHexCode || '#ccc'"></span>
                                                <span>{{ s.colorNameEn || s.name || '—' }}</span>
                                            </div>
                                            <div class="sc-rem">
                                                <span [class]="remainClass(s)">{{ s.remainingWeightGrams }}g</span>
                                            </div>
                                            <div class="sc-init">{{ s.initialWeightGrams }}g</div>
                                            <div class="sc-pct">
                                                <div class="fill-bar">
                                                    <div class="fill-inner" [style.width.%]="fillPct(s)" [class]="fillClass(s)"></div>
                                                </div>
                                                <span class="fill-label">{{ fillPct(s) }}%</span>
                                            </div>
                                            <div class="sc-status">
                                                <p-tag [value]="s.isActive ? 'Active' : 'Inactive'"
                                                       [severity]="s.isActive ? 'success' : 'secondary'" />
                                            </div>
                                            <div class="sc-date">{{ s.createdAt | date:'MMM d, y' }}</div>
                                            <div class="sc-act">
                                                <p-button icon="pi pi-pencil" [text]="true" size="small" severity="secondary"
                                                    (click)="openEditSpoolDialog(s)" pTooltip="Edit" />
                                                <p-button icon="pi pi-trash" [text]="true" size="small" severity="danger"
                                                    (click)="deleteSpool(row, s)" pTooltip="Deactivate" />
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
                        </div>
                    }
                }
            }
        </div>
    </div>

    <!-- ── Add Spool Dialog ── -->
    <p-dialog [(visible)]="spoolDlg.visible" [modal]="true" [style]="{width:'520px'}">
        <ng-template pTemplate="header">
            <div class="dlg-header">
                <span class="dlg-label">Printing</span>
                <span class="dlg-title">Add Spool</span>
            </div>
        </ng-template>
        <div class="dlg-form">

            <!-- Material -->
            <div class="ff">
                <label>Material *</label>
                <p-select [(ngModel)]="spoolDlg.materialId"
                    [options]="materialOptions" optionLabel="label" optionValue="value"
                    placeholder="Select material…" class="w-full"
                    (onChange)="onMaterialChange()">
                </p-select>
            </div>

            <!-- Color -->
            <div class="ff">
                <label>Color *</label>
                <p-select [(ngModel)]="spoolDlg.colorId"
                    [options]="colorOptionsForDialog" optionLabel="label" optionValue="value"
                    [showClear]="true"
                    placeholder="Select existing color…" class="w-full">
                </p-select>
                <div class="or-row">
                    <div class="or-line"></div>
                    <span class="or-text">or add new color</span>
                    <div class="or-line"></div>
                </div>
            </div>

            <!-- Inline new color -->
            <div class="new-color-box">
                <div class="form-row-3">
                    <div class="ff">
                        <label>Name (EN)</label>
                        <input pInputText [(ngModel)]="spoolDlg.newColorNameEn" class="w-full" placeholder="Black" />
                    </div>
                    <div class="ff">
                        <label>Name (AR)</label>
                        <input pInputText [(ngModel)]="spoolDlg.newColorNameAr" class="w-full" placeholder="أسود" />
                    </div>
                    <div class="ff">
                        <label>Hex</label>
                        <div class="hex-row">
                            <input type="color" class="color-picker" [(ngModel)]="spoolDlg.newColorHex" />
                            <input pInputText class="hex-input" [(ngModel)]="spoolDlg.newColorHex" maxlength="7" />
                        </div>
                    </div>
                </div>
                <small class="color-note">Leave Name blank to use an existing color above.</small>
            </div>

            <!-- Grams -->
            <div class="form-row-2">
                <div class="ff">
                    <label>Initial Weight (g) *</label>
                    <input type="number" class="p-inputtext w-full" [(ngModel)]="spoolDlg.initialGrams" min="1" />
                </div>
                <div class="ff">
                    <label>Label (optional)</label>
                    <input pInputText [(ngModel)]="spoolDlg.name" class="w-full" placeholder="Spool #1" />
                </div>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="Cancel" [text]="true" (click)="spoolDlg.visible = false" />
            <p-button label="Add Spool" [loading]="spoolDlg.saving" (click)="saveSpool()" />
        </ng-template>
    </p-dialog>

    <!-- ── Edit Spool Dialog ── -->
    <p-dialog [(visible)]="editDlg.visible" [modal]="true" [style]="{width:'420px'}">
        <ng-template pTemplate="header">
            <div class="dlg-header">
                <span class="dlg-label">Printing</span>
                <span class="dlg-title">Edit Spool</span>
            </div>
        </ng-template>
        <div class="dlg-form">
            <div class="ff">
                <label>Remaining (g)</label>
                <input type="number" class="p-inputtext w-full" [(ngModel)]="editDlg.remaining" min="0" [max]="editDlg.initial" />
                <small class="color-note">Initial: {{ editDlg.initial }}g</small>
            </div>
            <div class="ff">
                <label>Label (optional)</label>
                <input pInputText [(ngModel)]="editDlg.name" class="w-full" />
            </div>
            <div class="ff">
                <label class="check-label">
                    <p-checkbox [(ngModel)]="editDlg.isActive" [binary]="true"></p-checkbox>
                    Active
                </label>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="Cancel" [text]="true" (click)="editDlg.visible = false" />
            <p-button label="Save" [loading]="editDlg.saving" (click)="saveEditSpool()" />
        </ng-template>
    </p-dialog>
    `,
    styles: [`
        .pi-page { padding: 1rem 1.5rem; }

        /* Toolbar */
        .toolbar { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
        .toolbar-left { display: flex; align-items: baseline; gap: 0.75rem; }
        .toolbar-left h1 { margin: 0; font-size: 1.35rem; font-weight: 700; }
        .sub { font-size: 0.82rem; color: #9ca3af; }
        .toolbar-right { display: flex; align-items: center; gap: 0.5rem; }
        .search-input { height: 32px; padding: 0 0.7rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.85rem; width: 220px; }

        /* Materials table */
        .mat-table { border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden; }
        .mat-head {
            display: grid; grid-template-columns: 2rem 1fr 5rem 9rem 7rem 7rem 6rem;
            padding: 0.5rem 0.85rem; background: #f9fafb; border-bottom: 1px solid #e5e7eb;
            font-size: 0.72rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #6b7280;
        }
        .mat-row {
            display: grid; grid-template-columns: 2rem 1fr 5rem 9rem 7rem 7rem 6rem;
            padding: 0.6rem 0.85rem; border-bottom: 1px solid #f3f4f6;
            align-items: center; background: #fff; transition: background 0.1s;
        }
        .mat-row:hover { background: #fafbff; }
        .mat-row.expanded { background: #f8f9ff; border-bottom-color: transparent; }
        .empty-row { padding: 2.5rem; text-align: center; color: #9ca3af; }

        .exp-btn { background: none; border: none; cursor: pointer; color: #6b7280; padding: 0.2rem; border-radius: 4px; display: flex; align-items: center; font-size: 0.75rem; }
        .exp-btn:hover { background: #f3f4f6; }
        .mat-name { font-size: 0.9rem; font-weight: 600; color: #111827; display: block; }
        .mat-name-ar { font-size: 0.75rem; color: #9ca3af; direction: rtl; }
        .stock-low { color: #ef4444; font-weight: 700; }
        .c-actions { display: flex; }

        /* Spools section */
        .spools-section { background: #f4f6ff; border-bottom: 1px solid #e5e7eb; padding: 0.5rem 0 0.5rem 2.5rem; }
        .spool-empty { padding: 0.75rem; font-size: 0.85rem; color: #9ca3af; }
        .spool-table { }
        .srow { display: grid; grid-template-columns: 1fr 7rem 7rem 8rem 7rem 8rem 7rem; gap: 0; padding: 0.4rem 0.75rem; align-items: center; font-size: 0.83rem; }
        .srow.shead { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #9ca3af; border-bottom: 1px solid #e9ecef; padding-bottom: 0.35rem; }
        .srow.sbody { border-bottom: 1px solid #eceef8; }
        .srow.sbody:hover { background: #eef0ff; }
        .srow.inactive { opacity: 0.5; }
        .sc-act { display: flex; gap: 0; }

        .color-dot { width: 12px; height: 12px; border-radius: 50%; display: inline-block; margin-right: 0.4rem; border: 1px solid rgba(0,0,0,0.1); flex-shrink: 0; }
        .sc-color { display: flex; align-items: center; }

        /* Fill bar */
        .fill-bar { width: 50px; height: 6px; background: #e5e7eb; border-radius: 3px; overflow: hidden; display: inline-block; margin-right: 0.35rem; }
        .fill-inner { height: 100%; border-radius: 3px; transition: width 0.3s; }
        .fill-inner.fill-good { background: #10b981; }
        .fill-inner.fill-warn { background: #f59e0b; }
        .fill-inner.fill-low { background: #ef4444; }
        .fill-label { font-size: 0.75rem; color: #6b7280; }
        .sc-pct { display: flex; align-items: center; }

        .remain-good { color: #059669; font-weight: 600; }
        .remain-warn { color: #d97706; font-weight: 600; }
        .remain-low { color: #dc2626; font-weight: 700; }

        /* Dialogs */
        .dlg-form { display: flex; flex-direction: column; gap: 0.85rem; }
        .ff { display: flex; flex-direction: column; gap: 0.3rem; }
        .ff label { font-size: 0.8rem; font-weight: 600; color: #374151; }
        .form-row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .form-row-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.6rem; }
        .w-full { width: 100%; box-sizing: border-box; }
        .new-color-box { border: 1px dashed #c7d2fe; border-radius: 8px; padding: 0.75rem; background: #f8f9ff; }
        .or-row { display: flex; align-items: center; gap: 0.5rem; margin: 0.5rem 0 0; }
        .or-line { flex: 1; height: 1px; background: #e5e7eb; }
        .or-text { font-size: 0.72rem; color: #9ca3af; white-space: nowrap; }
        .color-note { font-size: 0.72rem; color: #9ca3af; margin-top: 0.25rem; }
        .hex-row { display: flex; align-items: center; gap: 0.4rem; }
        .color-picker { width: 32px; height: 32px; padding: 0; border: 1px solid #d1d5db; border-radius: 4px; cursor: pointer; }
        .hex-input { flex: 1; }
        .check-label { display: flex; align-items: center; gap: 0.4rem; font-size: 0.85rem; cursor: pointer; }
        .link { color: #667eea; cursor: pointer; text-decoration: underline; }
    `]
})
export class PrintingInventoryComponent implements OnInit {
    private api = inject(PrintingApiService);
    private msg = inject(MessageService);
    private translate = inject(TranslateService);

    loading = signal(false);
    allRows = signal<MaterialRow[]>([]);
    visibleRows = signal<MaterialRow[]>([]);
    allSpools = signal<FilamentSpool[]>([]);

    search = '';
    materialOptions: { label: string; value: string }[] = [];
    colorOptionsForDialog: { label: string; value: string }[] = [];

    spoolDlg = {
        visible: false, saving: false,
        materialId: '' as string,
        colorId: '' as string | null,
        newColorNameEn: '', newColorNameAr: '', newColorHex: '#CCCCCC',
        name: '', initialGrams: 1000
    };

    editDlg = {
        visible: false, saving: false,
        spoolId: '', remaining: 0, initial: 0, name: '', isActive: true
    };

    ngOnInit() { this.load(); }

    load() {
        this.loading.set(true);
        this.api.getAllMaterials().subscribe({
            next: (materials) => {
                this.materialOptions = materials.map(m => ({ label: m.nameEn, value: m.id }));
                this.api.getFilamentSpools().subscribe({
                    next: (spools) => {
                        this.allSpools.set(spools);
                        const rows: MaterialRow[] = materials.map(m => ({
                            ...m,
                            expanded: this.allRows().find(r => r.id === m.id)?.expanded ?? false,
                            spools: spools.filter(s => s.materialId === m.id),
                            loadingSpools: false,
                            savingSpoolIds: new Set()
                        }));
                        this.allRows.set(rows);
                        this.filterRows();
                        this.loading.set(false);
                    },
                    error: () => this.loading.set(false)
                });
            },
            error: () => this.loading.set(false)
        });
    }

    filterRows() {
        const q = this.search.trim().toLowerCase();
        this.visibleRows.set(q
            ? this.allRows().filter(r => r.nameEn.toLowerCase().includes(q) || r.nameAr?.toLowerCase().includes(q))
            : this.allRows());
    }

    toggleExpand(row: MaterialRow) {
        row.expanded = !row.expanded;
        this.allRows.update(l => [...l]);
    }

    // ── Computed helpers ───────────────────────────────────────────────────

    activeSpoolCount(row: MaterialRow) {
        return row.spools.filter(s => s.isActive).length;
    }

    totalRemaining(row: MaterialRow) {
        return row.spools.filter(s => s.isActive).reduce((sum, s) => sum + s.remainingWeightGrams, 0);
    }

    fillPct(s: FilamentSpool): number {
        if (!s.initialWeightGrams) return 0;
        return Math.round((s.remainingWeightGrams / s.initialWeightGrams) * 100);
    }

    fillClass(s: FilamentSpool): string {
        const p = this.fillPct(s);
        return p > 30 ? 'fill-good' : p > 10 ? 'fill-warn' : 'fill-low';
    }

    remainClass(s: FilamentSpool): string {
        if (s.remainingWeightGrams > 300) return 'remain-good';
        if (s.remainingWeightGrams > 100) return 'remain-warn';
        return 'remain-low';
    }

    // ── Add Spool ──────────────────────────────────────────────────────────

    openSpoolDialog(preselectedRow: MaterialRow | null) {
        this.spoolDlg = {
            visible: true, saving: false,
            materialId: preselectedRow?.id ?? '',
            colorId: null,
            newColorNameEn: '', newColorNameAr: '', newColorHex: '#CCCCCC',
            name: '', initialGrams: 1000
        };
        if (preselectedRow) this.onMaterialChange();
    }

    onMaterialChange() {
        this.spoolDlg.colorId = null;
        this.colorOptionsForDialog = [];
        if (!this.spoolDlg.materialId) return;
        this.api.getAllMaterialColors(this.spoolDlg.materialId).subscribe({
            next: (colors) => {
                this.colorOptionsForDialog = colors.map(c => ({
                    label: `${c.nameEn} (${c.hexCode})`,
                    value: c.id
                }));
            }
        });
    }

    saveSpool() {
        const d = this.spoolDlg;
        if (!d.materialId) return this.toast('warn', 'Select a material.');
        const hasExistingColor = !!d.colorId;
        const hasNewColor = !!d.newColorNameEn?.trim();
        if (!hasExistingColor && !hasNewColor)
            return this.toast('warn', 'Select an existing color or enter a new color name.');
        if (!d.initialGrams || d.initialGrams < 1)
            return this.toast('warn', 'Initial weight must be at least 1g.');

        d.saving = true;
        this.api.createSpoolWithColor({
            materialId: d.materialId,
            materialColorId: hasExistingColor ? d.colorId! : null,
            newColorNameEn: hasNewColor ? d.newColorNameEn.trim() : null,
            newColorNameAr: hasNewColor && d.newColorNameAr ? d.newColorNameAr.trim() : null,
            newColorHexCode: hasNewColor ? d.newColorHex : null,
            name: d.name.trim() || null,
            initialWeightGrams: d.initialGrams
        }).subscribe({
            next: (spool) => {
                d.saving = false;
                d.visible = false;
                this.allRows.update(rows => rows.map(r => {
                    if (r.id !== spool.materialId) return r;
                    return { ...r, spools: [spool, ...r.spools] };
                }));
                this.filterRows();
                this.toast('success', 'Spool added.');
            },
            error: (err) => {
                d.saving = false;
                this.toast('error', err?.error?.message || 'Failed to add spool.');
            }
        });
    }

    // ── Edit Spool ─────────────────────────────────────────────────────────

    openEditSpoolDialog(s: FilamentSpool) {
        this.editDlg = {
            visible: true, saving: false,
            spoolId: s.id,
            remaining: s.remainingWeightGrams,
            initial: s.initialWeightGrams,
            name: s.name || '',
            isActive: s.isActive
        };
    }

    saveEditSpool() {
        const d = this.editDlg;
        if (d.remaining < 0) return this.toast('warn', 'Remaining must be ≥ 0.');
        d.saving = true;
        this.api.updateFilamentSpool(d.spoolId, {
            remainingWeightGrams: d.remaining,
            name: d.name.trim() || null,
            isActive: d.isActive
        }).subscribe({
            next: (updated) => {
                d.saving = false;
                d.visible = false;
                this.allRows.update(rows => rows.map(r => ({
                    ...r,
                    spools: r.spools.map(s => s.id === updated.id ? updated : s)
                })));
                this.filterRows();
            },
            error: () => { d.saving = false; this.toast('error', 'Save failed.'); }
        });
    }

    // ── Delete Spool ───────────────────────────────────────────────────────

    deleteSpool(row: MaterialRow, s: FilamentSpool) {
        if (!confirm(`Deactivate spool "${s.colorNameEn || s.name || s.id}"?`)) return;
        this.api.deleteFilamentSpool(s.id).subscribe({
            next: () => {
                this.allRows.update(rows => rows.map(r => {
                    if (r.id !== row.id) return r;
                    return { ...r, spools: r.spools.map(x => x.id === s.id ? { ...x, isActive: false } : x) };
                }));
                this.filterRows();
            },
            error: () => this.toast('error', 'Failed to deactivate spool.')
        });
    }

    private toast(severity: string, detail: string) {
        this.msg.add({ severity, summary: severity === 'error' ? 'Error' : severity === 'warn' ? 'Warning' : 'Done', detail, life: 4000 });
    }
}
