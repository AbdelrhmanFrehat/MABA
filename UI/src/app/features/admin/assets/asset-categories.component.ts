import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { AssetsService } from '../../../shared/services/assets.service';
import { AssetCategory } from '../../../shared/models/asset.model';

@Component({
    selector: 'app-asset-categories',
    standalone: true,
    imports: [CommonModule, RouterModule, ReactiveFormsModule, ButtonModule, CardModule, InputTextModule, CheckboxModule, ToastModule, ConfirmDialogModule, TableModule],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />

        <div class="p-4">
            <div class="flex justify-between items-start mb-4 gap-3 flex-wrap">
                <h1 class="text-2xl font-bold m-0">Asset Categories</h1>
                <p-button label="Back to Assets" severity="secondary" [outlined]="true" routerLink="/admin/assets"></p-button>
            </div>

            <p-card class="mb-4">
                <form [formGroup]="form" (ngSubmit)="save()">
                    <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
                        <input pInputText formControlName="nameEn" placeholder="Name (English)" class="w-full" />
                        <input pInputText formControlName="nameAr" placeholder="Name (Arabic)" class="w-full" />
                        <input pInputText formControlName="numberingPrefix" placeholder="Prefix (e.g. FRN-)" class="w-full" />
                        <div class="flex items-center gap-2">
                            <p-checkbox formControlName="isActive" [binary]="true" inputId="active"></p-checkbox>
                            <label for="active">Active</label>
                        </div>
                    </div>
                    <div class="flex justify-end gap-2 mt-3">
                        <p-button *ngIf="editingId" label="Cancel" severity="secondary" [outlined]="true" (onClick)="resetForm()"></p-button>
                        <p-button [label]="editingId ? 'Update' : 'Add'" type="submit" [disabled]="form.invalid || saving" [loading]="saving"></p-button>
                    </div>
                </form>
            </p-card>

            <p-card>
                <p-table [value]="rows" [loading]="loading" dataKey="id">
                    <ng-template pTemplate="header">
                        <tr>
                            <th>Name EN</th>
                            <th>Name AR</th>
                            <th>Prefix</th>
                            <th>Active</th>
                            <th style="width:9rem">Actions</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-row>
                        <tr>
                            <td>{{ row.nameEn }}</td>
                            <td>{{ row.nameAr }}</td>
                            <td>{{ row.numberingPrefix }}</td>
                            <td>{{ row.isActive ? 'Yes' : 'No' }}</td>
                            <td>
                                <p-button icon="pi pi-pencil" severity="primary" [text]="true" (onClick)="edit(row)"></p-button>
                                <p-button icon="pi pi-trash" severity="danger" [text]="true" (onClick)="remove(row)"></p-button>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </p-card>
        </div>
    `
})
export class AssetCategoriesComponent implements OnInit {
    rows: AssetCategory[] = [];
    loading = false;
    saving = false;
    editingId: string | null = null;

    form = inject(FormBuilder).group({
        nameEn: ['', Validators.required],
        nameAr: ['', Validators.required],
        numberingPrefix: [''],
        isActive: [true]
    });

    private svc = inject(AssetsService);
    private msg = inject(MessageService);
    private confirm = inject(ConfirmationService);

    ngOnInit() { this.load(); }

    load() {
        this.loading = true;
        this.svc.getCategories().subscribe({
            next: c => { this.rows = c; this.loading = false; },
            error: err => { this.loading = false; this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to load.' }); }
        });
    }

    save() {
        if (this.form.invalid) return;
        const v = this.form.getRawValue();
        const payload = {
            nameEn: v.nameEn || '',
            nameAr: v.nameAr || '',
            numberingPrefix: v.numberingPrefix || undefined,
            isActive: !!v.isActive
        };
        this.saving = true;
        const obs = this.editingId ? this.svc.updateCategory(this.editingId, payload) : this.svc.createCategory(payload);
        obs.subscribe({
            next: () => { this.saving = false; this.msg.add({ severity: 'success', summary: 'Saved', detail: 'Category saved.' }); this.resetForm(); this.load(); },
            error: err => { this.saving = false; this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to save.' }); }
        });
    }

    edit(row: AssetCategory) {
        this.editingId = row.id;
        this.form.patchValue({ nameEn: row.nameEn, nameAr: row.nameAr, numberingPrefix: row.numberingPrefix || '', isActive: row.isActive });
    }

    remove(row: AssetCategory) {
        this.confirm.confirm({
            message: `Delete category "${row.nameEn}"?`,
            accept: () => {
                this.svc.deleteCategory(row.id).subscribe({
                    next: () => { this.msg.add({ severity: 'success', summary: 'Deleted', detail: 'Category removed.' }); this.load(); },
                    error: err => this.msg.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to delete.' })
                });
            }
        });
    }

    resetForm() {
        this.editingId = null;
        this.form.reset({ nameEn: '', nameAr: '', numberingPrefix: '', isActive: true });
    }
}
