import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../shared/services/api.service';
import { LanguageService } from '../../../shared/services/language.service';

interface Address {
    id?: string;
    fullName: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
    phone: string;
    isDefault?: boolean;
}

@Component({
    selector: 'app-account-addresses',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        DialogModule,
        ToastModule,
        ConfirmDialogModule,
        TagModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmDialog />
        <div class="addresses-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-map-marker"></i>
                        {{ languageService.language === 'ar' ? 'عناويني' : 'My Addresses' }}
                    </span>
                    <h1 class="hero-title">{{ 'account.addresses.title' | translate }}</h1>
                </div>
            </section>

            <!-- Main Section -->
            <section class="main-section">
                <div class="container">
                    <!-- Header Actions -->
                    <div class="page-header">
                        <button class="add-btn" (click)="openAddDialog()">
                            <i class="pi pi-plus"></i>
                            {{ 'account.addresses.addNew' | translate }}
                        </button>
                    </div>

                    <!-- Loading -->
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                        </div>
                    </div>

                    <!-- Empty State -->
                    <div *ngIf="!loading && addresses.length === 0" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-map-marker"></i>
                        </div>
                        <h2>{{ 'account.addresses.noAddresses' | translate }}</h2>
                        <p>{{ 'account.addresses.noAddressesDescription' | translate }}</p>
                        <button class="cta-button" (click)="openAddDialog()">
                            <i class="pi pi-plus"></i>
                            {{ 'account.addresses.addNew' | translate }}
                        </button>
                    </div>

                    <!-- Addresses Grid -->
                    <div *ngIf="!loading && addresses.length > 0" class="addresses-grid">
                        <div *ngFor="let address of addresses; let i = index" class="address-card" [style.animation-delay]="(i * 0.1) + 's'">
                            <div class="card-header">
                                <div class="header-info">
                                    <h3>{{ address.fullName }}</h3>
                                    <span *ngIf="address.isDefault" class="default-badge">
                                        <i class="pi pi-check"></i>
                                        {{ 'account.addresses.default' | translate }}
                                    </span>
                                </div>
                                <div class="card-actions">
                                    <button 
                                        class="icon-btn"
                                        (click)="openEditDialog(address)"
                                        [pTooltip]="'common.edit' | translate">
                                        <i class="pi pi-pencil"></i>
                                    </button>
                                    <button 
                                        class="icon-btn danger"
                                        (click)="deleteAddress(address)"
                                        [pTooltip]="'common.delete' | translate">
                                        <i class="pi pi-trash"></i>
                                    </button>
                                </div>
                            </div>
                            
                            <div class="address-content">
                                <p class="address-line">{{ address.addressLine1 }}</p>
                                <p class="address-line" *ngIf="address.addressLine2">{{ address.addressLine2 }}</p>
                                <p class="address-line">{{ address.city }}, {{ address.state }} {{ address.postalCode }}</p>
                                <p class="address-line">{{ address.country }}</p>
                                <p class="phone-line">
                                    <i class="pi pi-phone"></i>
                                    {{ address.phone }}
                                </p>
                            </div>

                            <div class="card-footer" *ngIf="!address.isDefault">
                                <button class="set-default-btn" (click)="setDefaultAddress(address)">
                                    <i class="pi pi-star"></i>
                                    {{ 'account.addresses.setDefault' | translate }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>

        <!-- Address Dialog -->
        <p-dialog 
            [header]="(editingAddress ? 'account.addresses.edit' : 'account.addresses.addNew') | translate"
            [(visible)]="showDialog"
            [modal]="true"
            [closable]="true"
            [dismissableMask]="true"
            [style]="{width: '600px'}"
            [contentStyle]="{'padding': '0'}"
            styleClass="address-dialog"
            (onHide)="closeDialog()">
            <div class="dialog-content">
                <form [formGroup]="addressForm" (ngSubmit)="saveAddress()">
                    <div class="form-grid">
                        <div class="form-group full-width">
                            <label>{{ 'account.addresses.fullName' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-user"></i>
                                <input type="text" pInputText formControlName="fullName" />
                            </div>
                        </div>
                        <div class="form-group full-width">
                            <label>{{ 'account.addresses.addressLine1' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-map-marker"></i>
                                <input type="text" pInputText formControlName="addressLine1" />
                            </div>
                        </div>
                        <div class="form-group full-width">
                            <label>{{ 'account.addresses.addressLine2' | translate }}</label>
                            <div class="input-wrapper">
                                <i class="pi pi-map"></i>
                                <input type="text" pInputText formControlName="addressLine2" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label>{{ 'account.addresses.city' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-building"></i>
                                <input type="text" pInputText formControlName="city" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label>{{ 'account.addresses.state' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-flag"></i>
                                <input type="text" pInputText formControlName="state" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label>{{ 'account.addresses.postalCode' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-hashtag"></i>
                                <input type="text" pInputText formControlName="postalCode" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label>{{ 'account.addresses.country' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-globe"></i>
                                <input type="text" pInputText formControlName="country" />
                            </div>
                        </div>
                        <div class="form-group full-width">
                            <label>{{ 'account.addresses.phone' | translate }} *</label>
                            <div class="input-wrapper">
                                <i class="pi pi-phone"></i>
                                <input type="tel" pInputText formControlName="phone" />
                            </div>
                        </div>
                    </div>
                    <div class="dialog-actions">
                        <button type="button" class="cancel-btn" (click)="closeDialog()">
                            {{ 'common.cancel' | translate }}
                        </button>
                        <button 
                            type="submit" 
                            class="submit-btn"
                            [disabled]="!addressForm.valid || saving">
                            <i *ngIf="saving" class="pi pi-spin pi-spinner"></i>
                            <i *ngIf="!saving" class="pi pi-check"></i>
                            {{ 'common.save' | translate }}
                        </button>
                    </div>
                </form>
            </div>
        </p-dialog>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .addresses-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 1000px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            padding: 4rem 2rem;
            overflow: hidden;
        }

        .hero-bg-gradient {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            z-index: 1;
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: rgba(255,255,255,0.1);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
            border-radius: 50px;
            color: white;
            font-size: 0.875rem;
            margin-bottom: 1.5rem;
        }

        .hero-title {
            font-size: clamp(2rem, 4vw, 3rem);
            font-weight: 800;
            color: white;
        }

        /* ============ MAIN SECTION ============ */
        .main-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .page-header {
            display: flex;
            justify-content: flex-end;
            margin-bottom: 2rem;
        }

        .add-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.875rem 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .add-btn:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        /* ============ LOADING ============ */
        .loading-container {
            text-align: center;
            padding: 4rem 2rem;
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .loading-spinner p {
            margin-top: 1rem;
            color: #6c757d;
        }

        /* ============ EMPTY STATE ============ */
        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .empty-icon {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 2rem;
        }

        .empty-icon i {
            font-size: 3rem;
            color: var(--color-primary);
        }

        .empty-state h2 {
            font-size: 1.75rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .empty-state p {
            color: #6c757d;
            margin-bottom: 2rem;
        }

        .cta-button {
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem 2rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 50px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .cta-button:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        /* ============ ADDRESSES GRID ============ */
        .addresses-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1.5rem;
        }

        .address-card {
            background: white;
            border-radius: 20px;
            padding: 1.5rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            animation: fadeIn 0.5s ease-out backwards;
            transition: all 0.3s;
        }

        .address-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .card-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 1rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .header-info h3 {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .default-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.25rem;
            padding: 0.25rem 0.75rem;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 50px;
            color: #667eea;
            font-size: 0.8rem;
            font-weight: 600;
        }

        .card-actions {
            display: flex;
            gap: 0.5rem;
        }

        .icon-btn {
            width: 36px;
            height: 36px;
            background: #f8f9fa;
            border: none;
            border-radius: 10px;
            color: #6c757d;
            cursor: pointer;
            transition: all 0.3s;
        }

        .icon-btn:hover {
            background: var(--color-primary);
            color: white;
        }

        .icon-btn.danger:hover {
            background: #ef4444;
        }

        .address-content {
            margin-bottom: 1rem;
        }

        .address-line {
            color: #6c757d;
            margin-bottom: 0.25rem;
        }

        .phone-line {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #6c757d;
            margin-top: 0.75rem;
        }

        .phone-line i {
            color: var(--color-primary);
        }

        .card-footer {
            padding-top: 1rem;
            border-top: 1px solid #e9ecef;
        }

        .set-default-btn {
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            padding: 0.75rem 1rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 10px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .set-default-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        /* ============ DIALOG ============ */
        :host ::ng-deep .address-dialog .p-dialog-header {
            background: var(--gradient-primary);
            color: white;
            padding: 1.25rem 1.5rem;
        }

        .dialog-content {
            padding: 2rem;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 1.25rem;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .form-group.full-width {
            grid-column: 1 / -1;
        }

        .form-group label {
            font-weight: 600;
            color: #1a1a2e;
        }

        .input-wrapper {
            position: relative;
            display: flex;
            align-items: center;
        }

        .input-wrapper i {
            position: absolute;
            left: 1rem;
            color: #6c757d;
        }

        .input-wrapper input {
            width: 100%;
            padding: 0.875rem 1rem 0.875rem 2.75rem;
            border: 2px solid #e9ecef;
            border-radius: 10px;
            font-size: 1rem;
            transition: all 0.3s;
        }

        .input-wrapper input:focus {
            outline: none;
            border-color: var(--color-primary);
            box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
        }

        .dialog-actions {
            display: flex;
            justify-content: flex-end;
            gap: 1rem;
            margin-top: 2rem;
            padding-top: 1.5rem;
            border-top: 1px solid #e9ecef;
        }

        .cancel-btn {
            padding: 0.875rem 1.5rem;
            background: transparent;
            border: 2px solid #e9ecef;
            border-radius: 10px;
            color: #6c757d;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .cancel-btn:hover {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }

        .submit-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.875rem 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 10px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }

        .submit-btn:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        .submit-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .addresses-grid {
                grid-template-columns: 1fr;
            }
            .form-grid {
                grid-template-columns: 1fr;
            }
            .form-group.full-width {
                grid-column: 1;
            }
        }
    `]
})
export class AccountAddressesComponent implements OnInit {
    addresses: Address[] = [];
    loading = false;
    showDialog = false;
    editingAddress: Address | null = null;
    saving = false;
    addressForm: FormGroup;

    private formBuilder = inject(FormBuilder);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    constructor() {
        this.addressForm = this.formBuilder.group({
            fullName: ['', Validators.required],
            addressLine1: ['', Validators.required],
            addressLine2: [''],
            city: ['', Validators.required],
            state: ['', Validators.required],
            postalCode: ['', Validators.required],
            country: ['', Validators.required],
            phone: ['', Validators.required]
        });
    }

    ngOnInit() {
        this.loadAddresses();
    }

    loadAddresses() {
        this.loading = true;
        this.apiService.get<Address[]>('/addresses').subscribe({
            next: (addresses) => {
                this.addresses = addresses;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    openAddDialog() {
        this.editingAddress = null;
        this.addressForm.reset();
        this.showDialog = true;
    }

    openEditDialog(address: Address) {
        this.editingAddress = address;
        this.addressForm.patchValue(address);
        this.showDialog = true;
    }

    closeDialog() {
        this.showDialog = false;
        this.editingAddress = null;
        this.addressForm.reset();
    }

    saveAddress() {
        if (!this.addressForm.valid) return;

        this.saving = true;
        const addressData = this.addressForm.value;
        const request = this.editingAddress && this.editingAddress.id
            ? this.apiService.put('/addresses', this.editingAddress.id, addressData)
            : this.apiService.post('/addresses', addressData);

        request.subscribe({
            next: () => {
                this.loadAddresses();
                this.closeDialog();
                this.saving = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('messages.saved')
                });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.saveError')
                });
            }
        });
    }

    deleteAddress(address: Address) {
        if (!address.id) return;
        
        this.confirmationService.confirm({
            message: this.translateService.instant('account.addresses.confirmDelete'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.delete<void>('/addresses', address.id!).subscribe({
                    next: () => {
                        this.loadAddresses();
                        this.messageService.add({
                            severity: 'success',
                            summary: this.translateService.instant('messages.success'),
                            detail: this.translateService.instant('messages.deleted')
                        });
                    },
                    error: () => {
                        this.messageService.add({
                            severity: 'error',
                            summary: this.translateService.instant('messages.error'),
                            detail: this.translateService.instant('messages.deleteError')
                        });
                    }
                });
            }
        });
    }

    setDefaultAddress(address: Address) {
        this.apiService.post<void>(`/addresses/${address.id}/set-default`, {}).subscribe({
            next: () => {
                this.loadAddresses();
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('account.addresses.defaultSet')
                });
            }
        });
    }
}
