import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig, DialogService } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrintingApiService } from '../../../../shared/services/printing-api.service';
import { MaterialColor, Material } from '../../../../shared/models/printing.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-material-colors-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        CheckboxModule,
        InputNumberModule,
        ToastModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="colors-dialog">
            <!-- Color list -->
            <div class="colors-list">
                @if (loading) {
                    <div class="loading-state">
                        <i class="pi pi-spin pi-spinner text-2xl"></i>
                    </div>
                } @else if (colors.length === 0) {
                    <div class="empty-state">
                        <i class="pi pi-palette text-3xl text-gray-400"></i>
                        <p>No colors added yet</p>
                    </div>
                } @else {
                    <div class="color-rows">
                        @for (color of colors; track color.id) {
                            @if (editingId === color.id) {
                                <!-- Inline edit row -->
                                <div class="color-row editing" [formGroup]="editForm!">
                                    <span class="color-preview" [style.background]="editForm!.get('hexCode')?.value || '#cccccc'"></span>
                                    <input type="color" class="color-picker" formControlName="hexCode" />
                                    <input pInputText formControlName="hexCode" class="hex-input" maxlength="7" placeholder="#RRGGBB" />
                                    <input pInputText formControlName="nameEn" class="name-input" placeholder="Name EN" />
                                    <input pInputText formControlName="nameAr" class="name-input" placeholder="Name AR" dir="rtl" />
                                    <p-inputNumber formControlName="sortOrder" [min]="0" [max]="999" [useGrouping]="false" styleClass="sort-input" />
                                    <p-checkbox formControlName="isActive" [binary]="true" [pTooltip]="'Active'" />
                                    <p-button icon="pi pi-check" [rounded]="true" [text]="true" severity="success" [loading]="saving" (onClick)="saveEdit(color)" />
                                    <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="secondary" (onClick)="cancelEdit()" />
                                </div>
                            } @else {
                                <!-- Display row -->
                                <div class="color-row" [class.inactive]="!color.isActive">
                                    <span class="color-preview" [style.background]="color.hexCode"></span>
                                    <span class="hex-code">{{ color.hexCode }}</span>
                                    <span class="name-en">{{ color.nameEn }}</span>
                                    <span class="name-ar" dir="rtl">{{ color.nameAr }}</span>
                                    <span class="sort">{{ color.sortOrder }}</span>
                                    <span class="status-badge" [class.active]="color.isActive">{{ color.isActive ? 'Active' : 'Inactive' }}</span>
                                    <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="primary" (onClick)="startEdit(color)" />
                                    <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" (onClick)="confirmDelete(color)" />
                                </div>
                            }
                        }
                    </div>
                }
            </div>

            <!-- Add new color form -->
            @if (showAddForm) {
                <div class="add-form" [formGroup]="addForm">
                    <h4 class="add-title">New Color</h4>
                    <div class="add-row">
                        <span class="color-preview" [style.background]="addForm.get('hexCode')?.value || '#cccccc'"></span>
                        <input type="color" class="color-picker" formControlName="hexCode" />
                        <input pInputText formControlName="hexCode" class="hex-input" maxlength="7" placeholder="#RRGGBB" />
                        <input pInputText formControlName="nameEn" class="name-input" placeholder="Name EN *" />
                        <input pInputText formControlName="nameAr" class="name-input" placeholder="Name AR *" dir="rtl" />
                        <p-inputNumber formControlName="sortOrder" [min]="0" [max]="999" [useGrouping]="false" styleClass="sort-input" />
                        <p-checkbox formControlName="isActive" [binary]="true" [pTooltip]="'Active'" />
                        <p-button icon="pi pi-check" [rounded]="true" [text]="true" severity="success" [loading]="saving" [disabled]="addForm.invalid || saving" (onClick)="saveAdd()" />
                        <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="secondary" (onClick)="showAddForm = false" />
                    </div>
                </div>
            }

            <div class="dialog-footer">
                <p-button icon="pi pi-plus" label="Add Color" [outlined]="true" (onClick)="openAddForm()" [disabled]="showAddForm" />
                <p-button label="Close" (onClick)="close()" />
            </div>
        </div>
    `,
    styles: [`
        .colors-dialog {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-height: 200px;
        }

        .loading-state, .empty-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 3rem;
            gap: 0.5rem;
            color: var(--text-color-secondary);
        }

        .color-rows {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .color-row {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 0.75rem;
            border-radius: var(--border-radius);
            background: var(--surface-section);
            border: 1px solid var(--surface-border);
            flex-wrap: wrap;
        }

        .color-row.inactive {
            opacity: 0.55;
        }

        .color-row.editing {
            background: var(--surface-hover);
        }

        .color-preview {
            width: 28px;
            height: 28px;
            border-radius: 50%;
            flex-shrink: 0;
            border: 2px solid var(--surface-border);
        }

        .color-picker {
            width: 36px;
            height: 28px;
            padding: 0;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            flex-shrink: 0;
        }

        .hex-input {
            width: 90px;
            flex-shrink: 0;
        }

        .name-input {
            flex: 1;
            min-width: 100px;
        }

        :host ::ng-deep .sort-input {
            width: 70px;
        }

        .sort {
            width: 40px;
            text-align: center;
            font-size: 0.85rem;
            color: var(--text-color-secondary);
        }

        .hex-code {
            font-family: monospace;
            font-size: 0.85rem;
            width: 70px;
            color: var(--text-color-secondary);
        }

        .name-en {
            flex: 1;
            min-width: 80px;
        }

        .name-ar {
            flex: 1;
            min-width: 80px;
            text-align: right;
        }

        .status-badge {
            font-size: 0.75rem;
            padding: 2px 8px;
            border-radius: 12px;
            background: var(--red-100);
            color: var(--red-700);
        }

        .status-badge.active {
            background: var(--green-100);
            color: var(--green-700);
        }

        .add-form {
            border: 1px dashed var(--primary-color);
            border-radius: var(--border-radius);
            padding: 0.75rem;
        }

        .add-title {
            margin: 0 0 0.5rem;
            font-size: 0.9rem;
            font-weight: 600;
        }

        .add-row {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            flex-wrap: wrap;
        }

        .dialog-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-top: 0.5rem;
            border-top: 1px solid var(--surface-border);
        }
    `]
})
export class MaterialColorsDialogComponent implements OnInit {
    colors: MaterialColor[] = [];
    loading = false;
    saving = false;
    showAddForm = false;
    editingId: string | null = null;

    addForm!: FormGroup;
    editForm: FormGroup | null = null;

    private material!: Material;
    private ref = inject(DynamicDialogRef);
    private config = inject(DynamicDialogConfig);
    private api = inject(PrintingApiService);
    private fb = inject(FormBuilder);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private dialogService = inject(DialogService);

    ngOnInit() {
        this.material = this.config.data?.material as Material;
        this.addForm = this.buildColorForm();
        this.loadColors();
    }

    private buildColorForm(color?: MaterialColor): FormGroup {
        return this.fb.group({
            nameEn: [color?.nameEn ?? '', [Validators.required, Validators.maxLength(100)]],
            nameAr: [color?.nameAr ?? '', [Validators.required, Validators.maxLength(100)]],
            hexCode: [color?.hexCode ?? '#cccccc', [Validators.required, Validators.pattern(/^#[0-9A-Fa-f]{6}$/)]],
            isActive: [color?.isActive ?? true],
            sortOrder: [color?.sortOrder ?? 0]
        });
    }

    loadColors() {
        this.loading = true;
        this.api.getAllMaterialColors(this.material.id).subscribe({
            next: (colors) => {
                this.colors = colors;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load colors', life: 4000 });
            }
        });
    }

    openAddForm() {
        this.addForm = this.buildColorForm();
        this.showAddForm = true;
        this.cancelEdit();
    }

    saveAdd() {
        if (this.addForm.invalid || this.saving) return;
        this.saving = true;
        const v = this.addForm.value;
        this.api.createMaterialColor(this.material.id, {
            nameEn: v.nameEn.trim(),
            nameAr: v.nameAr.trim(),
            hexCode: v.hexCode,
            isActive: v.isActive,
            sortOrder: v.sortOrder ?? 0
        }).subscribe({
            next: () => {
                this.saving = false;
                this.showAddForm = false;
                this.loadColors();
                this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Color added', life: 3000 });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save color', life: 4000 });
            }
        });
    }

    startEdit(color: MaterialColor) {
        this.showAddForm = false;
        this.editingId = color.id;
        this.editForm = this.buildColorForm(color);
    }

    cancelEdit() {
        this.editingId = null;
        this.editForm = null;
    }

    saveEdit(original: MaterialColor) {
        if (!this.editForm || this.editForm.invalid || this.saving) return;
        this.saving = true;
        const v = this.editForm.value;
        this.api.updateMaterialColor(this.material.id, original.id, {
            id: original.id,
            nameEn: v.nameEn.trim(),
            nameAr: v.nameAr.trim(),
            hexCode: v.hexCode,
            isActive: v.isActive,
            sortOrder: v.sortOrder ?? 0
        }).subscribe({
            next: () => {
                this.saving = false;
                this.cancelEdit();
                this.loadColors();
                this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Color updated', life: 3000 });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update color', life: 4000 });
            }
        });
    }

    confirmDelete(color: MaterialColor) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translate.instant('common.confirm'),
            width: '420px',
            closable: true,
            dismissableMask: true,
            data: {
                title: this.translate.instant('common.confirm'),
                message: `Delete color "${color.nameEn}"?`
            }
        });
        ref?.onClose.subscribe((confirmed: boolean) => {
            if (confirmed) this.deleteColor(color);
        });
    }

    private deleteColor(color: MaterialColor) {
        this.api.deleteMaterialColor(this.material.id, color.id).subscribe({
            next: () => {
                this.loadColors();
                this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Color deleted', life: 3000 });
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete color', life: 4000 });
            }
        });
    }

    close() {
        this.ref.close();
    }
}
