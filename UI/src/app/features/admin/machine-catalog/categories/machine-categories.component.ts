import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { MachineCatalogService } from '../../../../shared/services/machine-catalog.service';
import { MachineCategory } from '../../../../shared/models/machine-catalog.model';

@Component({
    selector: 'app-machine-categories',
    standalone: true,
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        ButtonModule, TableModule, DialogModule, InputTextModule,
        TextareaModule, InputNumberModule, CheckboxModule, TagModule,
        ToastModule, TooltipModule
    ],
    providers: [MessageService],
    styles: [`
        .mp-header { border-inline-start: 3px solid var(--tech-primary-blue, #0066FF); padding-inline-start: 1rem; }
        .mp-badge { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.68rem; font-weight: 800; letter-spacing: 0.1em; text-transform: uppercase; color: var(--tech-primary-blue, #0066FF); background: rgba(0,102,255,0.08); border: 1px solid rgba(0,102,255,0.2); border-radius: 4px; padding: 2px 8px; margin-bottom: 0.4rem; }
    `],
    template: `
        <p-toast />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div class="mp-header">
                    <div class="mp-badge"><i class="pi pi-server" style="font-size:0.65rem"></i> Machine Platform</div>
                    <h1 class="text-2xl font-bold m-0">Machine Categories</h1>
                    <p class="text-500 mt-1 mb-0 text-sm">Machine types exposed to the desktop app — CNC, Printer3D, Laser, etc.</p>
                </div>
                <p-button label="New Category" icon="pi pi-plus" (onClick)="openNew()"></p-button>
            </div>

            <p-table [value]="rows" [loading]="loading" stripedRows>
                <ng-template pTemplate="header">
                    <tr>
                        <th>Code</th>
                        <th>Name (EN)</th>
                        <th>Name (AR)</th>
                        <th>Icon Key</th>
                        <th>Sort</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-row>
                    <tr>
                        <td><code class="text-primary font-bold">{{ row.code }}</code></td>
                        <td>{{ row.displayNameEn }}</td>
                        <td>{{ row.displayNameAr }}</td>
                        <td><span class="text-500 text-sm">{{ row.iconKey || '—' }}</span></td>
                        <td>{{ row.sortOrder }}</td>
                        <td>
                            <p-tag [value]="row.isActive ? 'Active' : 'Inactive'"
                                   [severity]="row.isActive ? 'success' : 'danger'"></p-tag>
                        </td>
                        <td>
                            <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="primary"
                                      pTooltip="Edit" (onClick)="openEdit(row)"></p-button>
                            <p-button [icon]="row.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'"
                                      [text]="true" [rounded]="true"
                                      [severity]="row.isActive ? 'warn' : 'success'"
                                      [pTooltip]="row.isActive ? 'Deactivate' : 'Activate'"
                                      (onClick)="toggleActive(row)"></p-button>
                        </td>
                    </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                    <tr><td colspan="7" class="text-center p-5 text-500">No machine categories found.</td></tr>
                </ng-template>
            </p-table>
        </div>

        <!-- Create / Edit Dialog -->
        <p-dialog [(visible)]="showDialog" [modal]="true" [style]="{ width: '520px' }"
                  [header]="editId ? 'Edit Category' : 'New Category'"
                  [draggable]="false" [resizable]="false">
            <form [formGroup]="form" class="flex flex-col gap-3 mt-1">
                <div class="field">
                    <label class="font-semibold block mb-1">Code <span class="text-red-500">*</span></label>
                    <input pInputText formControlName="code" class="w-full" placeholder="e.g. CNC"
                           [attr.disabled]="editId ? true : null" />
                    <small class="text-500">Uppercase. Unique identifier used by the system.</small>
                </div>
                <div class="grid grid-cols-2 gap-3">
                    <div class="field">
                        <label class="font-semibold block mb-1">Name (EN) <span class="text-red-500">*</span></label>
                        <input pInputText formControlName="displayNameEn" class="w-full" />
                    </div>
                    <div class="field">
                        <label class="font-semibold block mb-1">Name (AR) <span class="text-red-500">*</span></label>
                        <input pInputText formControlName="displayNameAr" class="w-full" dir="rtl" />
                    </div>
                </div>
                <div class="field">
                    <label class="font-semibold block mb-1">Description (EN)</label>
                    <textarea pTextarea formControlName="descriptionEn" class="w-full" rows="2"></textarea>
                </div>
                <div class="field">
                    <label class="font-semibold block mb-1">Description (AR)</label>
                    <textarea pTextarea formControlName="descriptionAr" class="w-full" rows="2" dir="rtl"></textarea>
                </div>
                <div class="grid grid-cols-2 gap-3">
                    <div class="field">
                        <label class="font-semibold block mb-1">Icon Key</label>
                        <input pInputText formControlName="iconKey" class="w-full" placeholder="e.g. icon-cnc" />
                    </div>
                    <div class="field">
                        <label class="font-semibold block mb-1">Sort Order</label>
                        <p-inputnumber formControlName="sortOrder" class="w-full" [min]="0"></p-inputnumber>
                    </div>
                </div>
                <div class="field flex items-center gap-2">
                    <p-checkbox formControlName="isActive" [binary]="true" inputId="catActive"></p-checkbox>
                    <label for="catActive" class="cursor-pointer">Active</label>
                </div>
            </form>
            <ng-template pTemplate="footer">
                <p-button label="Cancel" [outlined]="true" (onClick)="showDialog = false"></p-button>
                <p-button label="Save" [loading]="saving" [disabled]="form.invalid" (onClick)="save()"></p-button>
            </ng-template>
        </p-dialog>
    `
})
export class MachineCategoriesComponent implements OnInit {
    private svc = inject(MachineCatalogService);
    private msg = inject(MessageService);
    private fb = inject(FormBuilder);

    rows: MachineCategory[] = [];
    loading = false;
    saving = false;
    showDialog = false;
    editId: string | null = null;

    form = this.fb.group({
        code: ['', [Validators.required, Validators.maxLength(100)]],
        displayNameEn: ['', [Validators.required, Validators.maxLength(200)]],
        displayNameAr: ['', [Validators.required, Validators.maxLength(200)]],
        descriptionEn: [''],
        descriptionAr: [''],
        iconKey: [''],
        sortOrder: [0],
        isActive: [true]
    });

    ngOnInit() { this.load(); }

    load() {
        this.loading = true;
        this.svc.getCategories().subscribe({
            next: d => { this.rows = d; this.loading = false; },
            error: () => { this.loading = false; this.err('Failed to load categories.'); }
        });
    }

    openNew() {
        this.editId = null;
        this.form.reset({ code: '', displayNameEn: '', displayNameAr: '', descriptionEn: '', descriptionAr: '', iconKey: '', sortOrder: 0, isActive: true });
        this.form.get('code')!.enable();
        this.showDialog = true;
    }

    openEdit(row: MachineCategory) {
        this.editId = row.id;
        this.form.patchValue({ code: row.code, displayNameEn: row.displayNameEn, displayNameAr: row.displayNameAr, descriptionEn: row.descriptionEn || '', descriptionAr: row.descriptionAr || '', iconKey: row.iconKey || '', sortOrder: row.sortOrder, isActive: row.isActive });
        this.form.get('code')!.disable();
        this.showDialog = true;
    }

    save() {
        if (this.form.invalid) return;
        this.saving = true;
        const v = this.form.getRawValue() as any;
        const obs = this.editId
            ? this.svc.updateCategory(this.editId, v)
            : this.svc.createCategory(v);

        obs.subscribe({
            next: () => { this.saving = false; this.showDialog = false; this.load(); this.msg.add({ severity: 'success', summary: 'Saved', detail: 'Category saved.' }); },
            error: (e: any) => { this.saving = false; this.err(e?.error?.message || 'Save failed.'); }
        });
    }

    toggleActive(row: MachineCategory) {
        this.svc.toggleCategoryActive(row.id).subscribe({
            next: () => { this.load(); this.msg.add({ severity: 'info', summary: 'Updated', detail: `Category ${row.isActive ? 'deactivated' : 'activated'}.` }); },
            error: () => this.err('Failed to update status.')
        });
    }

    private err(msg: string) {
        this.msg.add({ severity: 'error', summary: 'Error', detail: msg });
    }
}
