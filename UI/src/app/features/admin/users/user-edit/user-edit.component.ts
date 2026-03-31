import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { MultiSelectModule } from 'primeng/multiselect';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UsersApiService, UpdateUserRequest } from '../../../../shared/services/users-api.service';
import { User } from '../../../../shared/models/auth.model';
import { RolesApiService } from '../../../../shared/services/roles-api.service';
import { Role } from '../../../../shared/models/role.model';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'app-user-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        CheckboxModule,
        MultiSelectModule,
        ToastModule,
        MessageModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <div class="mb-4">
                <h1>{{ 'admin.users.editUser' | translate }}</h1>
                <p class="text-surface-600">{{ userId ? ('admin.users.editUserDescription' | translate) : ('admin.users.createUserDescription' | translate) }}</p>
            </div>

            @if (errorMessage) {
                <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-4" />
            }

            <form [formGroup]="userForm" (ngSubmit)="onSubmit()">
                <div class="form-grid">
                    <div class="form-field">
                        <label for="fullName">
                            {{ 'admin.users.fullName' | translate }} <span class="text-red-500">*</span>
                        </label>
                        <input 
                            pInputText 
                            id="fullName" 
                            formControlName="fullName"
                            [placeholder]="'admin.users.fullNamePlaceholder' | translate"
                            class="w-full"
                            [class.ng-invalid]="userForm.get('fullName')?.invalid && userForm.get('fullName')?.touched"
                        />
                        @if (userForm.get('fullName')?.invalid && userForm.get('fullName')?.touched) {
                            <small class="p-error">
                                {{ getErrorMessage('fullName') }}
                            </small>
                        }
                    </div>

                    <div class="form-field">
                        <label for="email">
                            {{ 'admin.users.email' | translate }}
                        </label>
                        <input 
                            pInputText 
                            id="email" 
                            formControlName="email"
                            type="email"
                            [placeholder]="'admin.users.emailPlaceholder' | translate"
                            class="w-full"
                            [readonly]="true"
                            [disabled]="true"
                        />
                        <small class="text-surface-500">{{ 'admin.users.emailReadOnly' | translate }}</small>
                    </div>

                    <div class="form-field">
                        <label for="phone">
                            {{ 'admin.users.phone' | translate }}
                        </label>
                        <input 
                            pInputText 
                            id="phone" 
                            formControlName="phone"
                            [placeholder]="'admin.users.phonePlaceholder' | translate"
                            class="w-full"
                        />
                    </div>

                    <div class="form-field form-field-full">
                        <div class="checkbox-field">
                            <p-checkbox 
                                id="isActive" 
                                formControlName="isActive"
                                binary
                                inputId="isActive"
                            ></p-checkbox>
                            <label for="isActive">
                                {{ 'admin.users.isActive' | translate }}
                            </label>
                        </div>
                    </div>

                    <div class="form-field form-field-full">
                        <label for="roles">
                            {{ 'admin.users.roles' | translate }}
                        </label>
                        <p-multiSelect
                            id="roles"
                            formControlName="roleIds"
                            [options]="availableRoles"
                            optionLabel="name"
                            optionValue="id"
                            [placeholder]="'admin.users.selectRoles' | translate"
                            [filter]="true"
                            [showClear]="true"
                            display="chip"
                            class="w-full"
                        ></p-multiSelect>
                        <small class="text-surface-500">{{ 'admin.users.rolesHelp' | translate }}</small>
                    </div>
                </div>

                <div class="form-actions">
                    <p-button 
                        [label]="'common.cancel' | translate" 
                        [outlined]="true"
                        (click)="onCancel()"
                        [disabled]="saving"
                        styleClass="w-full md:w-auto"
                    ></p-button>
                    <p-button 
                        [label]="'common.save' | translate" 
                        type="submit"
                        [loading]="saving"
                        [disabled]="userForm.invalid || saving"
                        styleClass="w-full md:w-auto"
                    ></p-button>
                </div>
            </form>
        </div>
    `,
    styles: [`
        .card {
            padding: 1rem;
        }

        @media (min-width: 768px) {
            .card {
                padding: 1.5rem;
            }
        }

        .form-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
        }

        @media (min-width: 768px) {
            .form-grid {
                grid-template-columns: repeat(2, 1fr);
            }
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

        .checkbox-field {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .checkbox-field label {
            margin: 0;
            cursor: pointer;
        }

        .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 0.5rem;
            margin-top: 1.5rem;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .card {
                padding: 0.75rem;
            }

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
export class UserEditComponent implements OnInit {
    userId: string | null = null;
    user: User | null = null;
    userForm!: FormGroup;
    saving = false;
    loading = false;
    errorMessage = '';
    availableRoles: Role[] = [];
    private initialRoleIds: string[] = [];

    constructor(
        private fb: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private usersApiService: UsersApiService,
        private rolesApiService: RolesApiService,
        private messageService: MessageService,
        private translateService: TranslateService
    ) {
        this.userForm = this.fb.group({
            fullName: ['', [Validators.required, Validators.maxLength(200)]],
            phone: [''],
            isActive: [true],
            roleIds: [[] as string[]]
        });
    }

    ngOnInit() {
        this.userId = this.route.snapshot.paramMap.get('id');
        
        if (this.userId) {
            this.loadUser();
        }
    }

    loadUser() {
        if (!this.userId) return;

        this.loading = true;
        forkJoin({
            user: this.usersApiService.getUserById(this.userId),
            roles: this.rolesApiService.getAllRoles(),
            userRoles: this.rolesApiService.getUserRoles(this.userId).pipe(catchError(() => of([] as Role[])))
        }).subscribe({
            next: ({ user, roles, userRoles }) => {
                this.user = user;
                this.availableRoles = roles;

                const selectedRoleIds = userRoles.length > 0
                    ? userRoles.map(r => r.id)
                    : roles.filter(r => user.roles.includes(r.name)).map(r => r.id);

                this.initialRoleIds = [...selectedRoleIds];
                this.userForm.patchValue({
                    fullName: user.fullName,
                    phone: user.phone || '',
                    isActive: user.isActive ?? true,
                    roleIds: selectedRoleIds
                });
                this.loading = false;
            },
            error: (error) => {
                this.errorMessage = this.translateService.instant('messages.errorLoadingUser');
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.errorMessage,
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    onSubmit() {
        if (this.userForm.invalid || !this.userId) {
            this.markFormGroupTouched(this.userForm);
            return;
        }

        this.saving = true;
        this.errorMessage = '';

        const updateRequest: UpdateUserRequest = {
            fullName: this.userForm.value.fullName,
            phone: this.userForm.value.phone || undefined
        };

        this.usersApiService.updateUser(this.userId, updateRequest).subscribe({
            next: () => {
                this.syncRolesAfterProfileUpdate();
            },
            error: (error) => {
                this.saving = false;
                if (error.error?.errors) {
                    // Handle validation errors
                    this.handleValidationErrors(error.error.errors);
                } else {
                    this.errorMessage = error.error?.message || this.translateService.instant('messages.errorUpdatingUser');
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translateService.instant('messages.error'),
                        detail: this.errorMessage,
                        life: 5000
                    });
                }
            }
        });
    }

    private syncRolesAfterProfileUpdate() {
        if (!this.userId) {
            this.saving = false;
            return;
        }

        const selectedRoleIds: string[] = this.userForm.value.roleIds || [];
        const rolesToAssign = selectedRoleIds.filter(id => !this.initialRoleIds.includes(id));
        const rolesToRemove = this.initialRoleIds.filter(id => !selectedRoleIds.includes(id));

        if (rolesToAssign.length === 0 && rolesToRemove.length === 0) {
            this.onSaveSuccess();
            return;
        }

        const assignRequests = rolesToAssign.map(roleId =>
            this.rolesApiService.assignRoleToUser(roleId, this.userId as string)
        );
        const removeRequests = rolesToRemove.map(roleId =>
            this.rolesApiService.removeRoleFromUser(roleId, this.userId as string)
        );

        forkJoin([...assignRequests, ...removeRequests]).subscribe({
            next: () => this.onSaveSuccess(),
            error: (error) => {
                this.saving = false;
                this.errorMessage = error.error?.message || this.translateService.instant('admin.users.rolesUpdateError');
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.errorMessage,
                    life: 5000
                });
            }
        });
    }

    private onSaveSuccess() {
        this.messageService.add({
            severity: 'success',
            summary: this.translateService.instant('messages.success'),
            detail: this.translateService.instant('messages.userUpdatedSuccessfully'),
            life: 3000
        });
        this.saving = false;
        setTimeout(() => {
            this.router.navigate(['/admin/users']);
        }, 1000);
    }

    onCancel() {
        this.router.navigate(['/admin/users']);
    }

    getErrorMessage(fieldName: string): string {
        const control = this.userForm.get(fieldName);
        if (control?.hasError('required')) {
            return this.translateService.instant('validation.required');
        }
        if (control?.hasError('maxlength')) {
            return this.translateService.instant('validation.maxLength', { max: control.errors?.['maxlength'].requiredLength });
        }
        if (control?.hasError('email')) {
            return this.translateService.instant('validation.email');
        }
        return '';
    }

    handleValidationErrors(errors: Record<string, string[]>) {
        Object.keys(errors).forEach(key => {
            const control = this.userForm.get(key);
            if (control) {
                control.setErrors({ serverError: errors[key][0] });
            }
        });
    }

    markFormGroupTouched(formGroup: FormGroup) {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            control?.markAsTouched();
        });
    }
}

