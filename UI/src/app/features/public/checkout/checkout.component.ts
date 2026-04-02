import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { RadioButtonModule } from 'primeng/radiobutton';
import { StepsModule } from 'primeng/steps';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CartService } from '../../../shared/services/cart.service';
import { AuthService } from '../../../shared/services/auth.service';
import { LanguageService } from '../../../shared/services/language.service';
import { Cart, ShippingAddress, BillingAddress, PaymentMethodType, ShippingMethod } from '../../../shared/models';
import { User } from '../../../shared/models/auth.model';

@Component({
    selector: 'app-checkout',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        SelectModule,
        RadioButtonModule,
        StepsModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <div class="checkout-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-credit-card"></i>
                        {{ languageService.language === 'ar' ? 'إتمام الطلب' : 'Checkout' }}
                    </span>
                    <h1 class="hero-title">{{ 'checkout.title' | translate }}</h1>
                </div>
            </section>

            <!-- Main Content -->
            <section class="content-section">
                <div class="container">
                    <div *ngIf="loading" class="loading-container">
                        <div class="loading-spinner">
                            <i class="pi pi-spin pi-spinner"></i>
                            <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                        </div>
                    </div>

                    <div *ngIf="!loading && !cart" class="empty-state">
                        <div class="empty-icon">
                            <i class="pi pi-shopping-cart"></i>
                        </div>
                        <h2>{{ 'checkout.emptyCart' | translate }}</h2>
                        <p-button 
                            [label]="'checkout.continueShopping' | translate" 
                            [routerLink]="['/catalog']"
                            styleClass="cta-button">
                        </p-button>
                    </div>

                    <div *ngIf="!loading && cart && cart.items.length > 0" class="checkout-grid">
                        <div class="checkout-steps-section">
                            <!-- Steps Indicator -->
                            <div class="steps-wrapper">
                                <div class="step-item" *ngFor="let step of stepsData; let i = index" 
                                     [class.active]="currentStep === i"
                                     [class.completed]="currentStep > i">
                                    <div class="step-number">
                                        <i *ngIf="currentStep > i" class="pi pi-check"></i>
                                        <span *ngIf="currentStep <= i">{{ i + 1 }}</span>
                                    </div>
                                    <span class="step-label">{{ step.label }}</span>
                                </div>
                            </div>

                            <!-- Step Content -->
                            <div class="step-content">
                                <!-- Step 1: Customer Details -->
                                <div *ngIf="currentStep === 0" class="section-card animate-fade-in">
                                    <div class="card-header">
                                        <i class="pi pi-user"></i>
                                        <h2>{{ 'checkout.customerDetails' | translate }}</h2>
                                    </div>
                                    <form [formGroup]="customerForm" class="checkout-form">
                                        <div class="form-row">
                                            <div class="form-group">
                                                <label>{{ 'checkout.fullName' | translate }} *</label>
                                                <input type="text" pInputText formControlName="fullName" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'checkout.email' | translate }} *</label>
                                                <input type="email" pInputText formControlName="email" />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'checkout.phone' | translate }} *</label>
                                            <input type="tel" pInputText formControlName="phone" />
                                        </div>
                                    </form>
                                </div>

                                <!-- Step 2: Shipping Address -->
                                <div *ngIf="currentStep === 1" class="section-card animate-fade-in">
                                    <div class="card-header">
                                        <i class="pi pi-map-marker"></i>
                                        <h2>{{ 'checkout.shippingAddress' | translate }}</h2>
                                    </div>
                                    <form [formGroup]="shippingForm" class="checkout-form">
                                        <div class="form-group">
                                            <label>{{ 'checkout.addressLine1' | translate }} *</label>
                                            <input type="text" pInputText formControlName="addressLine1" />
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'checkout.addressLine2' | translate }}</label>
                                            <input type="text" pInputText formControlName="addressLine2" />
                                        </div>
                                        <div class="form-row">
                                            <div class="form-group">
                                                <label>{{ 'checkout.city' | translate }} *</label>
                                                <input type="text" pInputText formControlName="city" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'checkout.state' | translate }}</label>
                                                <input type="text" pInputText formControlName="state" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'checkout.postalCode' | translate }}</label>
                                                <input type="text" pInputText formControlName="postalCode" />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'checkout.country' | translate }} *</label>
                                            <input type="text" pInputText formControlName="country" />
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'checkout.shippingMethod' | translate }} *</label>
                                            <p-select 
                                                [options]="shippingMethods"
                                                formControlName="shippingMethod"
                                                optionLabel="label"
                                                optionValue="value"
                                                styleClass="w-full">
                                            </p-select>
                                        </div>
                                    </form>
                                </div>

                                <!-- Step 3: Payment Method -->
                                <div *ngIf="currentStep === 2" class="section-card animate-fade-in">
                                    <div class="card-header">
                                        <i class="pi pi-credit-card"></i>
                                        <h2>{{ 'checkout.paymentMethod' | translate }}</h2>
                                    </div>
                                    <form [formGroup]="paymentForm" class="checkout-form">
                                        <div class="payment-options">
                                            <div *ngFor="let method of paymentMethods" 
                                                 class="payment-option"
                                                 [class.selected]="paymentForm.value.paymentMethod === method.value"
                                                 [class.disabled]="method.disabled">
                                                <p-radioButton 
                                                    [inputId]="method.value"
                                                    [value]="method.value"
                                                    [disabled]="method.disabled"
                                                    formControlName="paymentMethod">
                                                </p-radioButton>
                                                <label [for]="method.value">
                                                    <i [class]="method.icon"></i>
                                                    <span class="method-label">{{ method.label }}</span>
                                                    <span *ngIf="method.disabled" class="coming-soon-badge">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                                </label>
                                            </div>
                                        </div>
                                    </form>
                                </div>

                                <!-- Step 4: Review -->
                                <div *ngIf="currentStep === 3" class="section-card animate-fade-in">
                                    <div class="card-header">
                                        <i class="pi pi-check-circle"></i>
                                        <h2>{{ 'checkout.reviewOrder' | translate }}</h2>
                                    </div>
                                    <div class="review-content">
                                        <div class="review-section">
                                            <h3>{{ 'checkout.shippingAddress' | translate }}</h3>
                                            <p>{{ customerForm.value.fullName }}</p>
                                            <p>{{ shippingForm.value.addressLine1 }}</p>
                                            <p *ngIf="shippingForm.value.addressLine2">{{ shippingForm.value.addressLine2 }}</p>
                                            <p>{{ shippingForm.value.city }}, {{ shippingForm.value.state }} {{ shippingForm.value.postalCode }}</p>
                                            <p>{{ shippingForm.value.country }}</p>
                                        </div>
                                        <div class="review-section">
                                            <h3>{{ 'checkout.paymentMethod' | translate }}</h3>
                                            <p>{{ getPaymentMethodLabel(paymentForm.value.paymentMethod) }}</p>
                                        </div>
                                        <div class="review-section">
                                            <h3>{{ 'checkout.orderItems' | translate }}</h3>
                                            <div *ngFor="let item of cart?.items" class="review-item">
                                                <span>{{ getItemName(item) }} x {{ item.quantity }}</span>
                                                <span class="price">{{ formatCurrency(item.subtotal) }}</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Navigation Buttons -->
                            <div class="step-navigation">
                                <p-button 
                                    *ngIf="currentStep > 0"
                                    [label]="'checkout.previous' | translate" 
                                    icon="pi pi-arrow-left"
                                    [outlined]="true"
                                    (click)="previousStep()"
                                    styleClass="nav-btn">
                                </p-button>
                                <div class="spacer"></div>
                                <p-button 
                                    *ngIf="currentStep < 3"
                                    [label]="'checkout.next' | translate" 
                                    icon="pi pi-arrow-right"
                                    iconPos="right"
                                    (click)="nextStep()"
                                    [disabled]="!isCurrentStepValid()"
                                    styleClass="nav-btn primary">
                                </p-button>
                                <p-button 
                                    *ngIf="currentStep === 3"
                                    [label]="'checkout.placeOrder' | translate" 
                                    icon="pi pi-check"
                                    iconPos="right"
                                    (click)="placeOrder()"
                                    [loading]="placingOrder"
                                    styleClass="nav-btn primary">
                                </p-button>
                            </div>
                        </div>

                        <!-- Order Summary -->
                        <div class="summary-section">
                            <div class="summary-card">
                                <h2>{{ 'checkout.orderSummary' | translate }}</h2>
                                <div *ngIf="cart" class="summary-content">
                                    <div class="summary-row">
                                        <span>{{ 'checkout.subtotal' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.subtotal) }}</span>
                                    </div>
                                    <div *ngIf="cart.discountAmount > 0" class="summary-row discount">
                                        <span>{{ 'checkout.discount' | translate }}</span>
                                        <span class="value">-{{ formatCurrency(cart.discountAmount) }}</span>
                                    </div>
                                    <div class="summary-row">
                                        <span>{{ 'checkout.tax' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.taxAmount) }}</span>
                                    </div>
                                    <div class="summary-row">
                                        <span>{{ 'checkout.shipping' | translate }}</span>
                                        <span class="value">{{ formatCurrency(cart.shippingAmount) }}</span>
                                    </div>
                                    <div class="summary-total">
                                        <span>{{ 'checkout.total' | translate }}</span>
                                        <span class="total-value">{{ formatCurrency(cart.total) }}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
        <p-toast></p-toast>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .checkout-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 1200px;
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

        /* ============ CONTENT SECTION ============ */
        .content-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .loading-container,
        .empty-state {
            text-align: center;
            padding: 4rem 2rem;
            background: white;
            border-radius: 24px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.1);
        }

        .loading-spinner i {
            font-size: 3rem;
            color: var(--color-primary);
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

        :host ::ng-deep .cta-button {
            background: var(--gradient-primary) !important;
            border: none !important;
            padding: 1rem 2rem !important;
            border-radius: 50px !important;
        }

        /* ============ CHECKOUT GRID ============ */
        .checkout-grid {
            display: grid;
            grid-template-columns: 1fr 380px;
            gap: 2rem;
            align-items: start;
        }

        /* ============ STEPS ============ */
        .steps-wrapper {
            display: flex;
            justify-content: space-between;
            margin-bottom: 2rem;
            padding: 1.5rem;
            background: white;
            border-radius: 16px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }

        .step-item {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.5rem;
            flex: 1;
            position: relative;
        }

        .step-item::after {
            content: '';
            position: absolute;
            top: 18px;
            left: 50%;
            width: 100%;
            height: 2px;
            background: #e9ecef;
            z-index: 0;
        }

        .step-item:last-child::after {
            display: none;
        }

        .step-item.completed::after {
            background: var(--color-primary);
        }

        .step-number {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            background: #e9ecef;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            color: #6c757d;
            z-index: 1;
            transition: all 0.3s ease;
        }

        .step-item.active .step-number {
            background: var(--gradient-primary);
            color: white;
            box-shadow: var(--shadow-glow);
        }

        .step-item.completed .step-number {
            background: var(--color-primary);
            color: white;
        }

        .step-label {
            font-size: 0.75rem;
            color: #6c757d;
            font-weight: 600;
        }

        .step-item.active .step-label {
            color: var(--color-primary);
        }

        /* ============ SECTION CARD ============ */
        .section-card {
            background: white;
            border-radius: 24px;
            padding: 2rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
        }

        .card-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 2rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .card-header i {
            width: 50px;
            height: 50px;
            background: var(--gradient-primary);
            border-radius: 16px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.25rem;
        }

        .card-header h2 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin: 0;
        }

        .animate-fade-in {
            animation: fadeIn 0.4s ease-out;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* ============ FORM ============ */
        .checkout-form {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1.5rem;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            color: #1a1a2e;
            margin-bottom: 0.75rem;
        }

        .form-group input,
        .form-group :host ::ng-deep .p-select {
            width: 100%;
            border-radius: 12px !important;
        }

        /* ============ PAYMENT OPTIONS ============ */
        .payment-options {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .payment-option {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1.5rem;
            border: 2px solid #e9ecef;
            border-radius: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .payment-option:hover {
            border-color: var(--color-primary);
        }

        .payment-option.selected {
            border-color: var(--color-primary);
            background: rgba(102, 126, 234, 0.05);
        }

        .payment-option label {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            cursor: pointer;
            font-weight: 600;
            color: #1a1a2e;
        }

        .payment-option label i {
            font-size: 1.25rem;
            color: var(--color-primary);
        }

        /* ============ REVIEW ============ */
        .review-content {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
        }

        .review-section h3 {
            font-size: 1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1rem;
        }

        .review-section p {
            color: #6c757d;
            margin: 0.25rem 0;
        }

        .review-section:last-child {
            grid-column: 1 / -1;
        }

        .review-item {
            display: flex;
            justify-content: space-between;
            padding: 0.75rem 0;
            border-bottom: 1px solid #e9ecef;
        }

        .review-item .price {
            font-weight: 700;
            color: var(--color-primary);
        }

        /* ============ NAVIGATION ============ */
        .step-navigation {
            display: flex;
            align-items: center;
            margin-top: 2rem;
        }

        .spacer {
            flex: 1;
        }

        :host ::ng-deep .nav-btn {
            border-radius: 50px !important;
            padding: 0.875rem 2rem !important;
        }

        :host ::ng-deep .nav-btn.primary {
            background: var(--gradient-primary) !important;
            border: none !important;
        }

        :host ::ng-deep .nav-btn.p-button-outlined,
        :host ::ng-deep .p-button-outlined.nav-btn {
            background: transparent !important;
            border: 2px solid #667eea !important;
            color: #667eea !important;
        }

        :host ::ng-deep .nav-btn.p-button-outlined:hover,
        :host ::ng-deep .p-button-outlined.nav-btn:hover {
            background: rgba(102, 126, 234, 0.1) !important;
            border-color: #667eea !important;
            color: #667eea !important;
        }

        :host ::ng-deep .p-button-outlined {
            background: transparent !important;
            border: 2px solid #667eea !important;
            color: #667eea !important;
        }

        :host ::ng-deep .p-button-outlined:hover {
            background: rgba(102, 126, 234, 0.1) !important;
        }

        /* ============ SUMMARY ============ */
        .summary-card {
            background: white;
            border-radius: 24px;
            padding: 2rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            position: sticky;
            top: 100px;
        }

        .summary-card h2 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.5rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid #e9ecef;
        }

        .summary-row {
            display: flex;
            justify-content: space-between;
            padding: 0.75rem 0;
            color: #6c757d;
        }

        .summary-row .value {
            font-weight: 600;
            color: #1a1a2e;
        }

        .summary-row.discount .value {
            color: var(--color-primary);
        }

        .summary-total {
            display: flex;
            justify-content: space-between;
            padding: 1.5rem 0;
            border-top: 2px solid #e9ecef;
            margin-top: 1rem;
        }

        .summary-total span:first-child {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a2e;
        }

        .total-value {
            font-size: 1.75rem;
            font-weight: 800;
            background: var(--gradient-primary);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 992px) {
            .checkout-grid {
                grid-template-columns: 1fr;
            }
            .summary-card {
                position: static;
            }
            .steps-wrapper {
                flex-wrap: wrap;
                gap: 1rem;
            }
            .step-item::after {
                display: none;
            }
        }

        @media (max-width: 768px) {
            .review-content {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class CheckoutComponent implements OnInit {
    cart: Cart | null = null;
    loading = false;
    placingOrder = false;
    currentStep = 0;
    user: User | null = null;

    customerForm: FormGroup;
    shippingForm: FormGroup;
    paymentForm: FormGroup;

    stepsData: any[] = [];
    shippingMethods: any[] = [];
    paymentMethods: any[] = [];

    private fb = inject(FormBuilder);
    private cartService = inject(CartService);
    private authService = inject(AuthService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);
    private router = inject(Router);

    constructor() {
        this.customerForm = this.fb.group({
            fullName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            phone: ['', Validators.required]
        });

        this.shippingForm = this.fb.group({
            addressLine1: ['', Validators.required],
            addressLine2: [''],
            city: ['', Validators.required],
            state: [''],
            postalCode: [''],
            country: ['', Validators.required],
            shippingMethod: [ShippingMethod.Standard, Validators.required]
        });

        this.paymentForm = this.fb.group({
            paymentMethod: [PaymentMethodType.Cash, Validators.required]
        });
    }

    ngOnInit() {
        this.initData();
        this.loadCart();
        this.loadUser();
    }

    initData() {
        const isAr = this.languageService.language === 'ar';
        
        this.stepsData = [
            { label: isAr ? 'المعلومات' : 'Info' },
            { label: isAr ? 'العنوان' : 'Address' },
            { label: isAr ? 'الدفع' : 'Payment' },
            { label: isAr ? 'المراجعة' : 'Review' }
        ];

        this.shippingMethods = [
            { label: isAr ? 'شحن عادي' : 'Standard Shipping', value: ShippingMethod.Standard },
            { label: isAr ? 'شحن سريع' : 'Express Shipping', value: ShippingMethod.Express },
            { label: isAr ? 'شحن ليلي' : 'Overnight Shipping', value: ShippingMethod.Overnight }
        ];

        this.paymentMethods = [
            { label: isAr ? 'الدفع عند الاستلام' : 'Cash on Delivery', value: PaymentMethodType.Cash, icon: 'pi pi-wallet', disabled: false },
            { label: isAr ? 'بطاقة ائتمان' : 'Credit Card', value: PaymentMethodType.CreditCard, icon: 'pi pi-credit-card', disabled: true },
            { label: isAr ? 'تحويل بنكي' : 'Bank Transfer', value: PaymentMethodType.BankTransfer, icon: 'pi pi-building', disabled: true }
        ];
    }

    loadCart() {
        this.loading = true;
        this.cartService.getCart().subscribe({
            next: (cart) => {
                this.cart = cart;
                if (!cart || cart.items.length === 0) {
                    this.router.navigate(['/cart']);
                }
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    loadUser() {
        this.authService.getCurrentUser().subscribe({
            next: (user) => {
                this.user = user;
                if (user) {
                    this.customerForm.patchValue({
                        fullName: user.fullName,
                        email: user.email,
                        phone: user.phone || ''
                    });
                }
            }
        });
    }

    nextStep() {
        if (this.isCurrentStepValid() && this.currentStep < 3) {
            this.currentStep++;
        }
    }

    previousStep() {
        if (this.currentStep > 0) {
            this.currentStep--;
        }
    }

    isCurrentStepValid(): boolean {
        switch (this.currentStep) {
            case 0: return this.customerForm.valid;
            case 1: return this.shippingForm.valid;
            case 2: return this.paymentForm.valid;
            case 3: return true;
            default: return false;
        }
    }

    placeOrder() {
        if (!this.cart || !this.isCurrentStepValid()) return;

        // Check if user is authenticated
        if (!this.authService.authenticated) {
            this.messageService.add({
                severity: 'warn',
                summary: this.translateService.instant('messages.warning'),
                detail: this.translateService.instant('checkout.loginRequired')
            });
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/checkout' } });
            return;
        }

        this.placingOrder = true;

        const shippingAddress: ShippingAddress = {
            fullName: this.customerForm.value.fullName,
            phone: this.customerForm.value.phone,
            addressLine1: this.shippingForm.value.addressLine1,
            addressLine2: this.shippingForm.value.addressLine2,
            city: this.shippingForm.value.city,
            state: this.shippingForm.value.state,
            postalCode: this.shippingForm.value.postalCode,
            country: this.shippingForm.value.country
        };

        const billingAddress: BillingAddress = { ...shippingAddress };

        this.cartService.checkout({
            shippingAddress,
            billingAddress,
            paymentMethod: this.paymentForm.value.paymentMethod,
            shippingMethod: this.shippingForm.value.shippingMethod,
            notes: ''
        }).subscribe({
            next: (order) => {
                this.placingOrder = false;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translateService.instant('messages.success'),
                    detail: this.translateService.instant('checkout.orderPlaced')
                });
                this.router.navigate(['/account/orders', order.id], {
                    queryParams: { success: true }
                });
            },
            error: (error) => {
                this.placingOrder = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: error.error?.message || this.translateService.instant('checkout.orderFailed')
                });
            }
        });
    }

    getItemName(item: any): string {
        return this.languageService.language === 'ar' ? item.itemNameAr : item.itemNameEn;
    }

    getPaymentMethodLabel(value: string): string {
        const method = this.paymentMethods.find(m => m.value === value);
        return method ? method.label : value;
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat(this.languageService.language === 'ar' ? 'ar-IL' : 'en-IL', {
            style: 'currency',
            currency: 'ILS',
            minimumFractionDigits: 2
        }).format(value);
    }
}
