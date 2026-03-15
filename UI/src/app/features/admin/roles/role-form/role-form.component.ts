import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RolesApiService } from '../../../../shared/services/roles-api.service';
import { Role, Permission, CreateRoleRequest, UpdateRoleRequest } from '../../../../shared/models/role.model';

@Component({
    selector: 'app-role-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        MultiSelectModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <form [formGroup]="roleForm" (ngSubmit)="onSubmit()">
            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <div class="form-grid">
                <div class="form-field form-field-full">
                    <label for="name">
                        {{ 'admin.roles.name' | translate }} <span class="text-red-500">*</span>
                    </label>
                    <input 
                        pInputText 
                        id="name" 
                        formControlName="name"
                        [placeholder]="'admin.roles.namePlaceholder' | translate"
                        class="w-full"
                        [class.ng-invalid]="roleForm.get('name')?.invalid && roleForm.get('name')?.touched"
                        maxlength="100"
                    />
                    @if (roleForm.get('name')?.invalid && roleForm.get('name')?.touched) {
                        <small class="p-error">
                            {{ getErrorMessage('name') }}
                        </small>
                    }
                </div>

                <div class="form-field form-field-full">
                    <label for="description">
                        {{ 'admin.roles.description' | translate }}
                    </label>
                    <textarea 
                        id="description" 
                        formControlName="description"
                        [placeholder]="'admin.roles.descriptionPlaceholder' | translate"
                        class="w-full p-inputtext"
                        rows="3"
                    ></textarea>
                </div>

                <div class="form-field form-field-full">
                    <label for="permissions">
                        {{ 'admin.roles.permissions' | translate }}
                    </label>
                    <p-multiSelect
                        id="permissions"
                        formControlName="permissionIds"
                        [options]="permissions"
                        optionLabel="name"
                        optionValue="id"
                        [placeholder]="'admin.roles.selectPermissions' | translate"
                        [filter]="true"
                        [showClear]="true"
                        display="chip"
                        class="w-full"
                    ></p-multiSelect>
                </div>
            </div>

            <div class="form-actions">
                <p-button 
                    [label]="'common.cancel' | translate" 
                    [outlined]="true"
                    (click)="ref.close(false)"
                    [disabled]="saving"
                    styleClass="w-full md:w-auto"
                ></p-button>
                <p-button 
                    [label]="'common.save' | translate" 
                    type="submit"
                    [loading]="saving"
                    [disabled]="roleForm.invalid || saving"
                    styleClass="w-full md:w-auto"
                ></p-button>
            </div>
        </form>
    `,
    styles: [`
        .form-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        .form-field {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-field label {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-color);
        }

        .form-field-full {
            grid-column: 1 / -1;
        }

        .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
            margin-top: 1.5rem;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .form-grid {
                gap: 0.75rem;
            }

            .form-actions {
                flex-direction: column;
            }

            .form-actions p-button {
                width: 100%;
            }
        }
    `]
})
export class RoleFormComponent implements OnInit {
    roleForm!: FormGroup;
    permissions: Permission[] = [];
    loading = false;
    saving = false;
    errorMessage = '';
    isEditMode = false;
    roleId: string | null = null;

    private fb = inject(FormBuilder);
    private rolesApiService = inject(RolesApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    constructor(
        public ref: DynamicDialogRef,
        public config: DynamicDialogConfig
    ) {
        this.roleForm = this.fb.group({
            name: ['', [Validators.required, Validators.maxLength(100)]],
            description: [''],
            permissionIds: [[]]
        });
    }

    ngOnInit() {
        this.loadPermissions();

        if (this.config.data?.role) {
            const role: Role = this.config.data.role;
            this.isEditMode = true;
            this.roleId = role.id;
            this.roleForm.patchValue({
                name: role.name,
                description: role.description || '',
                permissionIds: role.permissions?.map(p => p.id) || []
            });
        }
    }

    loadPermissions() {
        this.loading = true;
        this.rolesApiService.getAllPermissions().subscribe({
            next: (permissions) => {
                this.permissions = permissions;
                this.loading = false;
            },
            error: (error) => {
                this.errorMessage = this.translateService.instant('messages.errorLoadingPermissions');
                this.loading = false;
            }
        });
    }

    onSubmit() {
        if (this.roleForm.invalid) {
            this.markFormGroupTouched(this.roleForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';

        const formValue = this.roleForm.value;

        if (this.isEditMode && this.roleId) {
            const updateRequest: UpdateRoleRequest = {
                name: formValue.name,
                description: formValue.description || undefined,
                permissionIds: formValue.permissionIds || []
            };

            this.rolesApiService.updateRole(this.roleId, updateRequest).subscribe({
                next: (role) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.roleUpdatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(role);
                },
                error: (error) => {
                    this.handleError(error);
                }
            });
        } else {
            const createRequest: CreateRoleRequest = {
                name: formValue.name,
                description: formValue.description || undefined,
                permissionIds: formValue.permissionIds || []
            };

            this.rolesApiService.createRole(createRequest).subscribe({
                next: (role) => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translateService.instant('messages.success'),
                        detail: this.translateService.instant('messages.roleCreatedSuccessfully'),
                        life: 3000
                    });
                    this.ref.close(role);
                },
                error: (error) => {
                    this.handleError(error);
                }
            });
        }
    }

    handleError(error: any) {
        this.saving = false;
        if (error.error?.errors) {
            this.handleValidationErrors(error.error.errors);
        } else {
            this.errorMessage = error.error?.message || this.translateService.instant('messages.errorSavingRole');
            this.messageService.add({
                severity: 'error',
                summary: this.translateService.instant('messages.error'),
                detail: this.errorMessage,
                life: 5000
            });
        }
    }

    handleValidationErrors(errors: Record<string, string[]>) {
        Object.keys(errors).forEach(key => {
            const control = this.roleForm.get(key);
            if (control) {
                control.setErrors({ serverError: errors[key][0] });
            }
        });
    }

    getErrorMessage(fieldName: string): string {
        const control = this.roleForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', { max: control.errors?.['maxlength'].requiredLength });
        }
        if (control?.hasError('serverError')) {
            return control.errors?.['serverError'];
        }
        return '';
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            control?.markAsTouched();
        });
    }
}

