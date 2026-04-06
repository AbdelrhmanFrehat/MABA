import { Component, OnInit, HostListener, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../shared/services/auth.service';
import { LanguageService } from '../../../shared/services/language.service';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { AvatarModule } from 'primeng/avatar';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { MenuItem } from 'primeng/api';
import { CartApiService } from '../../../shared/services/cart-api.service';

@Component({
    selector: 'app-public-header',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule, MenuModule, AvatarModule, TooltipModule, BadgeModule],
    template: `
        <header class="public-header" [class.scrolled]="isScrolled" [dir]="languageService.direction">
            <div class="header-container">
                <!-- Logo -->
                <a routerLink="/" class="logo-link">
                    <img src="assets/images/maba-hero-logo.png" alt="MABA" class="header-logo-img" width="2048" height="1364" />
                    <span class="beta-chip" [pTooltip]="'beta.tooltip' | translate" tooltipPosition="bottom">{{ 'beta.label' | translate }}</span>
                </a>

                <!-- Main Navigation -->
                <nav class="main-nav" [class.active]="isMobileMenuOpen">
                    <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-link">
                        {{ languageService.language === 'ar' ? 'الرئيسية' : 'Home' }}
                    </a>

                    <a routerLink="/catalog" routerLinkActive="active" class="nav-link">
                        {{ languageService.language === 'ar' ? 'المتجر' : 'Shop' }}
                    </a>
                    
                    <!-- Capabilities Dropdown -->
                    <div class="nav-dropdown" (mouseenter)="showCapabilities = true" (mouseleave)="showCapabilities = false">
                        <button class="nav-link dropdown-trigger" [class.active]="isCapabilitiesRoute()">
                            {{ languageService.language === 'ar' ? 'القدرات' : 'Capabilities' }}
                            <i class="pi pi-chevron-down dropdown-arrow"></i>
                        </button>
                        <div class="dropdown-menu" [class.show]="showCapabilities">
                            <a routerLink="/design-cad" class="dropdown-item" (click)="showCapabilities = false">
                                <i class="pi pi-pencil"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'التصميم و CAD' : 'Design & CAD' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'التصميم والرسم بمساعدة الحاسوب' : 'CAD design & drawing services' }}</span>
                                </div>
                            </a>
                            <a routerLink="/3d-print" class="dropdown-item" (click)="showCapabilities = false">
                                <i class="pi pi-box"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'الطباعة ثلاثية الأبعاد' : '3D Printing' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'النماذج الأولية والتصنيع' : 'Prototyping & Manufacturing' }}</span>
                                </div>
                            </a>
                            <a routerLink="/laser-engraving" class="dropdown-item" (click)="showCapabilities = false">
                                <i class="pi pi-bolt"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'النقش بالليزر' : 'Laser Engraving' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'القطع والنقش الدقيق' : 'Precision cutting & engraving' }}</span>
                                </div>
                            </a>
                            <a routerLink="/cnc" class="dropdown-item" (click)="showCapabilities = false">
                                <i class="pi pi-cog"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'توجيه CNC' : 'CNC Routing' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'تصنيع دقيق وتفريز PCB' : 'Precision machining & PCB milling' }}</span>
                                </div>
                            </a>
                            <div class="dropdown-divider"></div>
                            <div class="dropdown-item disabled" (click)="$event.preventDefault()">
                                <i class="pi pi-sitemap"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'تصميم الأجهزة والدوائر المطبوعة' : 'Hardware Design & PCB Development' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                </div>
                            </div>
                            <div class="dropdown-item disabled" (click)="$event.preventDefault()">
                                <i class="pi pi-code"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'هندسة البرمجيات المدمجة' : 'Embedded Firmware Engineering' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                </div>
                            </div>
                            <div class="dropdown-item disabled" (click)="$event.preventDefault()">
                                <i class="pi pi-th-large"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'التصميم الميكانيكي والمحاكاة' : 'Mechanical CAD & Simulation' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                </div>
                            </div>
                            <div class="dropdown-item disabled" (click)="$event.preventDefault()">
                                <i class="pi pi-sync"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'تكامل الأنظمة والاختبار' : 'System Integration & Testing' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                </div>
                            </div>
                            <div class="dropdown-item disabled" (click)="$event.preventDefault()">
                                <i class="pi pi-cog"></i>
                                <div class="dropdown-item-content">
                                    <span class="dropdown-item-title">{{ languageService.language === 'ar' ? 'التصنيع الصناعي' : 'Industrial Fabrication' }}</span>
                                    <span class="dropdown-item-desc">{{ languageService.language === 'ar' ? 'قريباً' : 'Coming Soon' }}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <a routerLink="/projects" routerLinkActive="active" class="nav-link">
                        {{ 'menu.projects' | translate }}
                    </a>

                    <a routerLink="/software" routerLinkActive="active" class="nav-link">
                        {{ languageService.language === 'ar' ? 'البرمجيات' : 'Software' }}
                    </a>

                    <a routerLink="/about" routerLinkActive="active" class="nav-link">
                        {{ 'nav.about' | translate }}
                    </a>

                    @if (authService.authenticated) {
                        <div class="mobile-auth-links">
                            <a routerLink="/account" class="nav-link mobile-account-link" (click)="isMobileMenuOpen = false">
                                <i class="pi pi-user"></i>
                                {{ 'menu.clientPortal' | translate }}
                            </a>
                            <a routerLink="/account/requests" class="nav-link mobile-account-link" (click)="isMobileMenuOpen = false">
                                <i class="pi pi-list"></i>
                                {{ 'menu.myRequests' | translate }}
                            </a>
                            <a routerLink="/account/designs" class="nav-link mobile-account-link" (click)="isMobileMenuOpen = false">
                                <i class="pi pi-images"></i>
                                {{ 'menu.myDesigns' | translate }}
                            </a>
                            <button type="button" class="nav-link mobile-account-link mobile-logout-link" (click)="logout(); isMobileMenuOpen = false">
                                <i class="pi pi-sign-out"></i>
                                {{ 'nav.auth.logout' | translate }}
                            </button>
                        </div>
                    } @else {
                        <div class="mobile-auth-links">
                            <a [routerLink]="['/auth/login']" [queryParams]="{ returnUrl: '/account' }" class="nav-link mobile-account-link" (click)="isMobileMenuOpen = false">
                                <i class="pi pi-sign-in"></i>
                                {{ 'nav.auth.signIn' | translate }}
                            </a>
                        </div>
                    }
                </nav>

                <!-- Header Actions -->
                <div class="header-actions">
                    <!-- Social Dropdown -->
                    <div class="social-dropdown" (mouseenter)="showSocial = true" (mouseleave)="showSocial = false">
                        <button class="icon-btn" aria-label="Social Links">
                            <i class="pi pi-share-alt"></i>
                        </button>
                        <div class="social-menu" [class.show]="showSocial">
                            <a href="https://www.facebook.com/MABA.Engineering.Solutions" target="_blank" class="social-item facebook">
                                <i class="pi pi-facebook"></i>
                                <span>Facebook</span>
                            </a>
                            <a href="https://www.instagram.com/maba_tech" target="_blank" class="social-item instagram">
                                <i class="pi pi-instagram"></i>
                                <span>Instagram</span>
                            </a>
                            <a href="https://wa.me/qr/DHGKCLYUCIOUD1" target="_blank" class="social-item whatsapp">
                                <i class="pi pi-whatsapp"></i>
                                <span>WhatsApp</span>
                            </a>
                        </div>
                    </div>

                    <!-- Language Switcher -->
                    <button 
                        type="button"
                        class="icon-btn"
                        (click)="languageService.toggleLanguage()"
                        [pTooltip]="languageService.language === 'en' ? 'العربية' : 'English'">
                        <i class="pi pi-globe"></i>
                    </button>

                    <!-- Admin Panel (Admin/StoreOwner only; no placeholder for others) -->
                    @if (isAdminOrOwner) {
                        <a routerLink="/admin" class="icon-btn admin-panel-btn" pTooltip="Admin Panel" tooltipPosition="bottom" aria-label="Admin Panel">
                            <i class="pi pi-shield"></i>
                        </a>
                    }

                    <!-- Cart -->
                    <a routerLink="/cart" class="icon-btn cart-btn" [class.has-items]="cartItemsCount > 0">
                        <i class="pi pi-shopping-cart"></i>
                        <span class="cart-count" *ngIf="cartItemsCount > 0">{{ cartItemsCount }}</span>
                    </a>

                    <!-- Start a Project CTA: when logged out redirect to login with returnUrl -->
                    @if (authService.authenticated) {
                        <a routerLink="/projects/request" class="cta-btn">
                            {{ 'menu.startProject' | translate }}
                        </a>
                    } @else {
                        <a [routerLink]="['/auth/login']" [queryParams]="{ returnUrl: '/projects/request' }" class="cta-btn">
                            {{ 'menu.startProject' | translate }}
                        </a>
                    }

                    <!-- Auth: low-emphasis Sign in only (no Register – login page has link to register) -->
                    @if (!authService.authenticated) {
                        <a [routerLink]="['/auth/login']" [queryParams]="{ returnUrl: currentUrl }" class="signin-link desktop-only">{{ 'nav.auth.signIn' | translate }}</a>
                    } @else {
                        <div class="user-menu">
                            <button class="user-avatar" (click)="showUserMenu = !showUserMenu">
                                {{ getUserInitials() }}
                            </button>
                            <div class="user-dropdown-menu" [class.show]="showUserMenu">
                                <!-- User Header -->
                                <div class="dropdown-header">
                                    <div class="user-info">
                                        <span class="user-name">{{ authService.user?.fullName || 'User' }}</span>
                                        <span class="user-email">{{ authService.user?.email }}</span>
                                    </div>
                                </div>
                                
                                <div class="dropdown-divider"></div>
                                
                                <!-- Main Menu Items -->
                                <a routerLink="/account" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-user"></i>
                                    <span>{{ 'menu.clientPortal' | translate }}</span>
                                </a>
                                <a routerLink="/account/requests" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-list"></i>
                                    <span>{{ 'menu.myRequests' | translate }}</span>
                                </a>
                                <a routerLink="/account/designs" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-images"></i>
                                    <span>{{ 'menu.myDesigns' | translate }}</span>
                                </a>
                                <a routerLink="/projects/request" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-briefcase"></i>
                                    <span>{{ 'menu.startProject' | translate }}</span>
                                </a>
                                <a routerLink="/design/new" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-pencil"></i>
                                    <span>{{ 'menu.designRequest' | translate }}</span>
                                </a>
                                <a routerLink="/3d-print" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-box"></i>
                                    <span>{{ 'menu.print3dRequest' | translate }}</span>
                                </a>
                                <a routerLink="/laser-engraving" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-bolt"></i>
                                    <span>{{ 'menu.laserRequest' | translate }}</span>
                                </a>
                                <a routerLink="/cnc" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-cog"></i>
                                    <span>{{ languageService.language === 'ar' ? 'طلب CNC' : 'CNC Request' }}</span>
                                </a>
                                
                                <div class="dropdown-divider"></div>
                                
                                <a routerLink="/account/orders" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-shopping-bag"></i>
                                    <span>{{ 'menu.myOrders' | translate }}</span>
                                </a>
                                <a routerLink="/account/profile" class="dropdown-menu-item" (click)="showUserMenu = false">
                                    <i class="pi pi-cog"></i>
                                    <span>{{ 'menu.profileSecurity' | translate }}</span>
                                </a>
                                
                                <div class="dropdown-divider"></div>
                                
                                <button class="dropdown-menu-item logout" (click)="logout(); showUserMenu = false">
                                    <i class="pi pi-sign-out"></i>
                                    <span>{{ 'nav.auth.logout' | translate }}</span>
                                </button>
                            </div>
                        </div>
                    }

                    <!-- Mobile Menu Toggle -->
                    <button 
                        type="button"
                        class="mobile-toggle"
                        (click)="toggleMobileMenu()"
                        [attr.aria-label]="'menu.toggle' | translate">
                        <span class="hamburger" [class.active]="isMobileMenuOpen">
                            <span></span>
                            <span></span>
                            <span></span>
                        </span>
                    </button>
                </div>
            </div>
        </header>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            
            /* Social Brand Colors */
            --facebook-color: #1877F2;
            --instagram-gradient: linear-gradient(45deg, #f09433 0%,#e6683c 25%,#dc2743 50%,#cc2366 75%,#bc1888 100%);
            --whatsapp-color: #25D366;
        }

        .public-header {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            z-index: 1000;
            background: rgba(255,255,255,0.95);
            backdrop-filter: blur(10px);
            transition: all 0.3s ease;
        }

        .public-header.scrolled {
            background: rgba(255,255,255,0.98);
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        }

        .header-container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 1.5rem;
            display: flex;
            align-items: center;
            justify-content: space-between;
            height: 64px;
        }

        /* Logo — height drives scale; wide max-width avoids squashing below bar height */
        .logo-link {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            text-decoration: none;
            flex-shrink: 0;
        }

        .header-logo-img {
            display: block;
            height: 56px;
            width: auto;
            max-height: 56px;
            max-width: min(320px, 72vw);
            object-fit: contain;
            object-position: left center;
        }
        [dir="rtl"] .header-logo-img {
            object-position: right center;
        }

        .beta-chip {
            display: inline-flex;
            align-items: center;
            padding: 0.2rem 0.55rem;
            margin-inline-start: 0.2rem;
            font-size: 0.72rem;
            font-weight: 600;
            letter-spacing: 0.04em;
            text-transform: uppercase;
            color: #667eea;
            border: 1px solid #667eea;
            border-radius: 999px;
            background: transparent;
            cursor: default;
            flex-shrink: 0;
        }

        /* Navigation */
        .main-nav {
            display: flex;
            align-items: center;
            gap: 0.25rem;
        }

        /* Hidden by default; shown only in mobile menu */
        .mobile-auth-links {
            display: none;
        }

        .nav-link {
            display: flex;
            align-items: center;
            gap: 0.375rem;
            padding: 0.5rem 0.875rem;
            text-decoration: none;
            color: #374151;
            font-weight: 500;
            font-size: 0.9rem;
            border-radius: 8px;
            border: none;
            background: none;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .nav-link:hover {
            background: rgba(102, 126, 234, 0.08);
            color: var(--color-primary);
        }

        .nav-link.active {
            background: rgba(102, 126, 234, 0.12);
            color: var(--color-primary);
            font-weight: 600;
        }

        /* Dropdown */
        .nav-dropdown {
            position: relative;
        }

        .dropdown-trigger {
            display: flex;
            align-items: center;
            gap: 0.375rem;
        }

        .dropdown-arrow {
            font-size: 0.7rem;
            transition: transform 0.2s ease;
        }

        .nav-dropdown:hover .dropdown-arrow {
            transform: rotate(180deg);
        }

        .dropdown-menu {
            position: absolute;
            top: 100%;
            left: 50%;
            transform: translateX(-50%) translateY(8px);
            min-width: 280px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.12), 0 0 0 1px rgba(0,0,0,0.05);
            padding: 0.5rem;
            opacity: 0;
            visibility: hidden;
            pointer-events: none;
            transition: all 0.2s ease;
        }

        .dropdown-menu.show {
            opacity: 1;
            visibility: visible;
            pointer-events: all;
            transform: translateX(-50%) translateY(0);
        }

        .dropdown-item {
            display: flex;
            align-items: flex-start;
            gap: 0.75rem;
            padding: 0.75rem;
            border-radius: 8px;
            text-decoration: none;
            color: #374151;
            transition: all 0.2s ease;
        }

        .dropdown-item:hover {
            background: rgba(102, 126, 234, 0.08);
        }

        .dropdown-item.disabled {
            opacity: 0.6;
            cursor: not-allowed;
            pointer-events: none;
        }

        .dropdown-item.disabled:hover {
            background: transparent;
        }

        .dropdown-item.disabled .dropdown-item-desc {
            color: #9ca3af;
            font-style: italic;
        }

        .dropdown-item i {
            width: 32px;
            height: 32px;
            background: var(--gradient-primary);
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 0.9rem;
            flex-shrink: 0;
        }

        .dropdown-item-content {
            display: flex;
            flex-direction: column;
            gap: 0.125rem;
        }

        .dropdown-item-title {
            font-weight: 600;
            font-size: 0.9rem;
            color: #1a1a2e;
        }

        .dropdown-item-desc {
            font-size: 0.75rem;
            color: #6b7280;
        }

        .dropdown-divider {
            height: 1px;
            background: #e5e7eb;
            margin: 0.375rem 0.5rem;
        }

        /* Header Actions */
        .header-actions {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        /* Icon Button */
        .icon-btn {
            width: 36px;
            height: 36px;
            background: #f3f4f6;
            border: none;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #374151;
            font-size: 1rem;
            cursor: pointer;
            transition: all 0.2s ease;
            text-decoration: none;
        }

        .icon-btn:hover {
            background: rgba(102, 126, 234, 0.1);
            color: var(--color-primary);
        }

        .admin-panel-btn {
            color: #6b7280;
        }
        .admin-panel-btn:hover {
            background: rgba(102, 126, 234, 0.1);
            color: var(--color-primary);
        }

        /* Cart with count */
        .cart-btn {
            position: relative;
        }

        .cart-btn.has-items {
            background: rgba(102, 126, 234, 0.1);
            color: var(--color-primary);
        }

        .cart-count {
            position: absolute;
            top: -4px;
            right: -4px;
            min-width: 18px;
            height: 18px;
            background: #ef4444;
            color: white;
            font-size: 0.65rem;
            font-weight: 700;
            border-radius: 50px;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 0 4px;
        }

        /* Social Dropdown */
        .social-dropdown {
            position: relative;
        }

        .social-menu {
            position: absolute;
            top: 100%;
            right: 0;
            min-width: 160px;
            background: white;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.12);
            padding: 0.375rem;
            opacity: 0;
            visibility: hidden;
            pointer-events: none;
            transform: translateY(8px);
            transition: all 0.2s ease;
        }

        .social-menu.show {
            opacity: 1;
            visibility: visible;
            pointer-events: all;
            transform: translateY(0);
        }

        .social-item {
            display: flex;
            align-items: center;
            gap: 0.625rem;
            padding: 0.5rem 0.75rem;
            border-radius: 6px;
            text-decoration: none;
            color: #374151;
            font-size: 0.85rem;
            font-weight: 500;
            transition: all 0.2s ease;
        }

        .social-item:hover {
            background: #f3f4f6;
        }

        .social-item.facebook i { color: var(--facebook-color); }
        .social-item.instagram i { color: #E1306C; }
        .social-item.whatsapp i { color: var(--whatsapp-color); }

        /* CTA Button */
        .cta-btn {
            padding: 0.5rem 1rem;
            background: var(--gradient-primary);
            color: white;
            text-decoration: none;
            font-weight: 600;
            font-size: 0.85rem;
            border-radius: 8px;
            transition: all 0.2s ease;
            white-space: nowrap;
        }

        .cta-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.35);
        }

        /* Sign in link – low emphasis (auth only when user chooses or hits protected action) */
        .signin-link {
            font-size: 0.8125rem;
            color: #6b7280;
            text-decoration: none;
            padding: 0.25rem 0.5rem;
            transition: color 0.2s ease;
        }
        .signin-link:hover {
            color: var(--color-primary);
        }

        /* User Menu */
        .user-menu {
            position: relative;
        }

        .user-avatar {
            width: 36px;
            height: 36px;
            background: var(--gradient-primary);
            border: none;
            border-radius: 50%;
            color: white;
            font-weight: 700;
            font-size: 0.85rem;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .user-avatar:hover {
            transform: scale(1.05);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.35);
        }

        .user-dropdown-menu {
            position: absolute;
            top: calc(100% + 8px);
            right: 0;
            min-width: 260px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.15);
            border: 1px solid rgba(0,0,0,0.08);
            opacity: 0;
            visibility: hidden;
            transform: translateY(-10px);
            transition: all 0.2s ease;
            z-index: 1000;
            overflow: hidden;
        }

        [dir="rtl"] .user-dropdown-menu {
            right: auto;
            left: 0;
        }

        .user-dropdown-menu.show {
            opacity: 1;
            visibility: visible;
            transform: translateY(0);
        }

        .dropdown-header {
            padding: 1rem;
            background: linear-gradient(135deg, #667eea11 0%, #764ba211 100%);
        }

        .user-info {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }

        .user-name {
            font-weight: 600;
            color: #1f2937;
            font-size: 0.95rem;
        }

        .user-email {
            font-size: 0.8rem;
            color: #6b7280;
        }

        .dropdown-divider {
            height: 1px;
            background: #e5e7eb;
            margin: 0.5rem 0;
        }

        .dropdown-menu-item {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            padding: 0.75rem 1rem;
            color: #374151;
            text-decoration: none;
            font-size: 0.9rem;
            transition: all 0.15s ease;
            cursor: pointer;
            border: none;
            background: none;
            width: 100%;
            text-align: start;
        }

        .dropdown-menu-item:hover {
            background: #f3f4f6;
            color: #667eea;
        }

        .dropdown-menu-item i {
            width: 18px;
            font-size: 1rem;
            color: #667eea;
        }

        .dropdown-menu-item.disabled {
            opacity: 0.6;
            cursor: not-allowed;
            pointer-events: none;
        }

        .dropdown-menu-item.disabled:hover {
            background: transparent;
        }

        .coming-soon-badge {
            margin-inline-start: auto;
            font-size: 0.7rem;
            background: #f3f4f6;
            color: #6b7280;
            padding: 0.2rem 0.5rem;
            border-radius: 10px;
        }

        .dropdown-menu-item.logout {
            color: #dc2626;
        }

        .dropdown-menu-item.logout:hover {
            background: #fef2f2;
            color: #dc2626;
        }

        .dropdown-menu-item.logout i {
            color: #dc2626;
        }

        /* Mobile Toggle */
        .mobile-toggle {
            display: none;
            padding: 0.5rem;
            background: none;
            border: none;
            cursor: pointer;
        }

        .hamburger {
            display: flex;
            flex-direction: column;
            gap: 5px;
            width: 22px;
        }

        .hamburger span {
            display: block;
            height: 2px;
            background: #374151;
            border-radius: 2px;
            transition: all 0.3s ease;
        }

        .hamburger.active span:nth-child(1) {
            transform: rotate(45deg) translate(5px, 5px);
        }

        .hamburger.active span:nth-child(2) {
            opacity: 0;
        }

        .hamburger.active span:nth-child(3) {
            transform: rotate(-45deg) translate(5px, -5px);
        }

        /* Responsive */
        @media (max-width: 1024px) {
            .social-dropdown {
                display: none;
            }
            .desktop-only {
                display: none;
            }
        }

        @media (max-width: 768px) {
            .header-container {
                height: 60px;
            }

            .header-logo-img {
                height: 48px;
                max-height: 48px;
                max-width: min(280px, 78vw);
            }

            .beta-chip {
                font-size: 0.68rem;
                padding: 0.18rem 0.5rem;
            }

            .main-nav {
                position: fixed;
                top: 60px;
                left: 12px;
                right: 12px;
                background: white;
                flex-direction: column;
                padding: 1rem;
                gap: 0.25rem;
                box-shadow: 0 10px 30px rgba(0,0,0,0.1);
                border-radius: 16px;
                transform: translateY(-100%);
                opacity: 0;
                pointer-events: none;
                transition: all 0.3s ease;
                max-height: calc(100vh - 80px);
                overflow-y: auto;
            }

            .main-nav.active {
                transform: translateY(0);
                opacity: 1;
                pointer-events: all;
            }

            .nav-link {
                width: 100%;
                justify-content: flex-start;
                padding: 0.75rem 1rem;
            }

            .mobile-auth-links {
                display: block;
                width: 100%;
                margin-top: 0.5rem;
                padding-top: 0.5rem;
                border-top: 1px solid #e5e7eb;
            }

            .mobile-account-link i {
                width: 1rem;
                text-align: center;
            }

            .mobile-logout-link {
                color: #dc2626;
            }

            .nav-dropdown {
                width: 100%;
            }

            .dropdown-menu {
                position: static;
                transform: none;
                box-shadow: none;
                border: 1px solid #e5e7eb;
                margin-top: 0.25rem;
                opacity: 1;
                visibility: visible;
                pointer-events: all;
                display: none;
            }

            .dropdown-menu.show {
                display: block;
                transform: none;
            }

            .mobile-toggle {
                display: block;
            }

            .cta-btn {
                display: none;
            }
        }

        /* RTL Support */
        [dir="rtl"] .dropdown-menu {
            left: auto;
            right: 50%;
            transform: translateX(50%) translateY(8px);
        }

        [dir="rtl"] .dropdown-menu.show {
            transform: translateX(50%) translateY(0);
        }

        [dir="rtl"] .social-menu {
            right: auto;
            left: 0;
        }

        [dir="rtl"] .cart-count {
            right: auto;
            left: -4px;
        }
    `]
})
export class PublicHeaderComponent implements OnInit {
    isMobileMenuOpen = false;
    isScrolled = false;
    userMenuItems: MenuItem[] = [];
    cartItemsCount = 0;
    showCapabilities = false;
    showSocial = false;
    showUserMenu = false;

    /** Visible only for authenticated Admin or StoreOwner; not rendered for others (no layout shift). */
    get isAdminOrOwner(): boolean {
        return this.authService.hasAnyRole(['Admin', 'StoreOwner', 'Store Owner']);
    }

    constructor(
        public authService: AuthService,
        public languageService: LanguageService,
        private cartApiService: CartApiService,
        private router: Router
    ) {
        // Update user menu when language changes
        effect(() => {
            this.languageService.languageSignal();
            this.setupUserMenu();
        });
    }

    @HostListener('window:scroll', [])
    onWindowScroll() {
        this.isScrolled = window.scrollY > 20;
    }

    @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent) {
        const target = event.target as HTMLElement;
        const userMenu = target.closest('.user-menu');
        if (!userMenu && this.showUserMenu) {
            this.showUserMenu = false;
        }
    }

    ngOnInit() {
        this.setupUserMenu();
        if (this.authService.authenticated) {
            this.loadCartCount();
        } else {
            this.cartItemsCount = 0;
        }
        
        if (this.authService.authenticated && this.authService.token && !this.authService.user) {
            this.authService.getCurrentUser().subscribe(() => {
                this.setupUserMenu();
            });
        }
    }

    loadCartCount() {
        this.cartApiService.getCart().subscribe({
            next: (cart) => {
                this.cartItemsCount = cart?.items?.length || 0;
            }
        });
    }

    setupUserMenu() {
        const user = this.authService.user;
        this.userMenuItems = [
            {
                label: user?.fullName || 'User',
                items: [
                    {
                        label: this.languageService.translate('menu.myAccount'),
                        icon: 'pi pi-user',
                        routerLink: '/account'
                    },
                    {
                        label: this.languageService.translate('menu.myOrders'),
                        icon: 'pi pi-shopping-bag',
                        routerLink: '/account/orders'
                    },
                    {
                        label: this.languageService.translate('menu.wishlist'),
                        icon: 'pi pi-heart',
                        routerLink: '/wishlist'
                    },
                    { separator: true },
                    {
                        label: this.languageService.translate('auth.logout'),
                        icon: 'pi pi-sign-out',
                        command: () => this.logout()
                    }
                ]
            }
        ];
    }

    getUserInitials(): string {
        const user = this.authService.user;
        if (!user?.fullName) return 'U';
        const parts = user.fullName.split(' ');
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        }
        return user.fullName[0].toUpperCase();
    }

    toggleMobileMenu() {
        this.isMobileMenuOpen = !this.isMobileMenuOpen;
    }

    logout() {
        this.authService.logout();
    }

    /** Current URL for returnUrl when redirecting to login */
    get currentUrl(): string {
        return this.router.url;
    }

    isCapabilitiesRoute(): boolean {
        const url = this.router.url;
        return url.includes('/3d-print') || url.includes('/laser-engraving') || url.includes('/cnc') || url.includes('/design') || url.includes('/design-cad');
    }

    scrollToSection(sectionId: string) {
        this.router.navigate(['/'], { fragment: sectionId }).then(() => {
            setTimeout(() => {
                const element = document.getElementById(sectionId);
                if (element) {
                    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }, 100);
        });
    }
}
