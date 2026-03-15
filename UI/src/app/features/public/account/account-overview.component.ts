import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { SkeletonModule } from 'primeng/skeleton';
import { AuthService } from '../../../shared/services/auth.service';
import { LanguageService } from '../../../shared/services/language.service';
import { OrdersApiService } from '../../../shared/services/orders-api.service';
import { PrintingApiService } from '../../../shared/services/printing-api.service';
import { LaserApiService } from '../../../shared/services/laser-api.service';

interface RecentRequest {
    id: string;
    referenceNumber: string;
    type: 'print3d' | 'laser' | 'order';
    typeLabel: string;
    status: string;
    createdAt: string;
    link: string;
}

@Component({
    selector: 'app-account-overview',
    standalone: true,
    imports: [
        CommonModule, 
        RouterModule, 
        TranslateModule, 
        CardModule, 
        ButtonModule,
        TagModule,
        TableModule,
        SkeletonModule
    ],
    template: `
        <div class="client-portal" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-briefcase"></i>
                        {{ 'portal.badge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'portal.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'portal.subtitle' | translate }}</p>
                </div>
            </section>

            <!-- Main Content -->
            <section class="main-section">
                <div class="container">
                    <!-- Profile Card -->
                    <div class="profile-card" *ngIf="user">
                        <div class="profile-avatar">
                            <i class="pi pi-user"></i>
                        </div>
                        <div class="profile-info">
                            <div class="profile-header">
                                <h2>{{ user.fullName }}</h2>
                                <span class="role-badge" [class.admin]="isAdmin">
                                    {{ isAdmin ? ('portal.roleAdmin' | translate) : ('portal.roleUser' | translate) }}
                                </span>
                            </div>
                            <p class="profile-email">
                                <i class="pi pi-envelope"></i>
                                {{ user.email }}
                            </p>
                        </div>
                        <a routerLink="/account/profile" class="edit-profile-btn">
                            <i class="pi pi-pencil"></i>
                            {{ 'portal.editProfile' | translate }}
                        </a>
                    </div>

                    <!-- KPI Cards -->
                    <div class="section-header">
                        <h3>{{ 'portal.overview' | translate }}</h3>
                    </div>
                    <div class="kpi-grid">
                        <!-- 3D Print Requests -->
                        <a routerLink="/account/requests" [queryParams]="{type: 'print3d'}" class="kpi-card">
                            <div class="kpi-icon print3d-icon">
                                <i class="pi pi-box"></i>
                            </div>
                            <div class="kpi-info">
                                <span class="kpi-value" *ngIf="!loadingStats">{{ print3dCount }}</span>
                                <span class="kpi-value" *ngIf="loadingStats">-</span>
                                <span class="kpi-label">{{ 'portal.kpi.print3d' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right kpi-arrow"></i>
                        </a>

                        <!-- Laser Requests -->
                        <a routerLink="/account/requests" [queryParams]="{type: 'laser'}" class="kpi-card">
                            <div class="kpi-icon laser-icon">
                                <i class="pi pi-bolt"></i>
                            </div>
                            <div class="kpi-info">
                                <span class="kpi-value" *ngIf="!loadingStats">{{ laserCount }}</span>
                                <span class="kpi-value" *ngIf="loadingStats">-</span>
                                <span class="kpi-label">{{ 'portal.kpi.laser' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right kpi-arrow"></i>
                        </a>

                        <!-- Orders -->
                        <a routerLink="/account/orders" class="kpi-card">
                            <div class="kpi-icon orders-icon">
                                <i class="pi pi-shopping-bag"></i>
                            </div>
                            <div class="kpi-info">
                                <span class="kpi-value" *ngIf="!loadingStats">{{ ordersCount }}</span>
                                <span class="kpi-value" *ngIf="loadingStats">-</span>
                                <span class="kpi-label">{{ 'portal.kpi.orders' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right kpi-arrow"></i>
                        </a>

                        <!-- My Designs -->
                        <a routerLink="/account/designs" class="kpi-card">
                            <div class="kpi-icon designs-icon">
                                <i class="pi pi-file"></i>
                            </div>
                            <div class="kpi-info">
                                <span class="kpi-value" *ngIf="!loadingStats">{{ designsCount }}</span>
                                <span class="kpi-value" *ngIf="loadingStats">-</span>
                                <span class="kpi-label">{{ 'portal.kpi.designs' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right kpi-arrow"></i>
                        </a>
                    </div>

                    <!-- Recent Requests Section -->
                    <div class="section-header">
                        <h3>{{ 'portal.recentRequests' | translate }}</h3>
                        <div class="section-actions">
                            <a routerLink="/3d-print" class="action-btn primary">
                                <i class="pi pi-plus"></i>
                                {{ 'portal.new3dPrint' | translate }}
                            </a>
                            <a routerLink="/laser" class="action-btn">
                                <i class="pi pi-plus"></i>
                                {{ 'portal.newLaser' | translate }}
                            </a>
                        </div>
                    </div>
                    <div class="requests-card">
                        <div *ngIf="loadingRequests" class="loading-state">
                            <p-skeleton height="3rem" styleClass="mb-2"></p-skeleton>
                            <p-skeleton height="3rem" styleClass="mb-2"></p-skeleton>
                            <p-skeleton height="3rem"></p-skeleton>
                        </div>

                        <div *ngIf="!loadingRequests && recentRequests.length === 0" class="empty-state">
                            <i class="pi pi-inbox"></i>
                            <h4>{{ 'portal.noRequests' | translate }}</h4>
                            <p>{{ 'portal.noRequestsDesc' | translate }}</p>
                            <a routerLink="/3d-print" class="start-btn">
                                <i class="pi pi-plus"></i>
                                {{ 'portal.startRequest' | translate }}
                            </a>
                        </div>

                        <table *ngIf="!loadingRequests && recentRequests.length > 0" class="requests-table">
                            <thead>
                                <tr>
                                    <th>{{ 'portal.table.reference' | translate }}</th>
                                    <th>{{ 'portal.table.type' | translate }}</th>
                                    <th>{{ 'portal.table.status' | translate }}</th>
                                    <th>{{ 'portal.table.date' | translate }}</th>
                                    <th>{{ 'portal.table.action' | translate }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr *ngFor="let request of recentRequests">
                                    <td>
                                        <span class="ref-number">{{ request.referenceNumber }}</span>
                                    </td>
                                    <td>
                                        <span class="type-badge" [class]="request.type">{{ request.typeLabel }}</span>
                                    </td>
                                    <td>
                                        <span class="status-badge" [class]="request.status.toLowerCase()">{{ request.status }}</span>
                                    </td>
                                    <td>{{ formatDate(request.createdAt) }}</td>
                                    <td>
                                        <a [routerLink]="request.link" class="view-btn">
                                            <i class="pi pi-eye"></i>
                                        </a>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- Quick Actions -->
                    <div class="section-header">
                        <h3>{{ 'portal.quickActions' | translate }}</h3>
                    </div>
                    <div class="actions-grid">
                        <a routerLink="/3d-print" class="action-tile primary">
                            <div class="tile-icon">
                                <i class="pi pi-box"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.new3dPrint' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.new3dPrintDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>

                        <a routerLink="/laser" class="action-tile">
                            <div class="tile-icon">
                                <i class="pi pi-bolt"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.newLaser' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.newLaserDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>

                        <a routerLink="/account/orders" class="action-tile">
                            <div class="tile-icon">
                                <i class="pi pi-list"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.tracking' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.trackingDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>

                        <a routerLink="/account/profile" class="action-tile">
                            <div class="tile-icon">
                                <i class="pi pi-cog"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.profile' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.profileDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>

                        <a routerLink="/catalog" class="action-tile secondary">
                            <div class="tile-icon">
                                <i class="pi pi-shopping-cart"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.shop' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.shopDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>

                        <a routerLink="/contact" class="action-tile secondary">
                            <div class="tile-icon">
                                <i class="pi pi-envelope"></i>
                            </div>
                            <div class="tile-content">
                                <span class="tile-title">{{ 'portal.actions.contact' | translate }}</span>
                                <span class="tile-desc">{{ 'portal.actions.contactDesc' | translate }}</span>
                            </div>
                            <i class="pi pi-arrow-right tile-arrow"></i>
                        </a>
                    </div>

                    <!-- Account Section (Compact) -->
                    <div class="section-header small">
                        <h3>{{ 'portal.accountSettings' | translate }}</h3>
                    </div>
                    <div class="account-links">
                        <a routerLink="/account/addresses" class="account-link">
                            <i class="pi pi-map-marker"></i>
                            {{ 'portal.account.addresses' | translate }}
                        </a>
                        <a routerLink="/wishlist" class="account-link">
                            <i class="pi pi-heart"></i>
                            {{ 'portal.account.wishlist' | translate }}
                        </a>
                        <a routerLink="/account/designs" class="account-link">
                            <i class="pi pi-file"></i>
                            {{ 'portal.account.designs' | translate }}
                        </a>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
        }

        .client-portal {
            width: 100%;
            min-height: 100vh;
            background: #f8fafc;
        }

        .container {
            max-width: 1100px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            padding: 3rem 2rem;
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
            padding: 0.5rem 1.25rem;
            background: rgba(255,255,255,0.1);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
            border-radius: 50px;
            color: white;
            font-size: 0.8rem;
            margin-bottom: 1rem;
        }

        .hero-title {
            font-size: clamp(1.75rem, 4vw, 2.5rem);
            font-weight: 800;
            color: white;
            margin-bottom: 0.5rem;
        }

        .hero-subtitle {
            color: rgba(255,255,255,0.7);
            font-size: 1rem;
            max-width: 500px;
            margin: 0 auto;
        }

        /* ============ MAIN SECTION ============ */
        .main-section {
            padding: 2rem 1rem 3rem;
            margin-top: -1.5rem;
            position: relative;
            z-index: 20;
        }

        /* ============ SECTION HEADERS ============ */
        .section-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 1rem;
            margin-top: 2rem;
        }

        .section-header.small {
            margin-top: 1.5rem;
        }

        .section-header h3 {
            font-size: 1.1rem;
            font-weight: 700;
            color: #1e293b;
            margin: 0;
        }

        .section-header.small h3 {
            font-size: 0.95rem;
            color: #64748b;
        }

        .section-actions {
            display: flex;
            gap: 0.75rem;
        }

        .action-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 8px;
            font-size: 0.85rem;
            font-weight: 600;
            text-decoration: none;
            background: #f1f5f9;
            color: #475569;
            transition: all 0.2s;
        }

        .action-btn:hover {
            background: #e2e8f0;
        }

        .action-btn.primary {
            background: var(--gradient-primary);
            color: white;
        }

        .action-btn.primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
        }

        /* ============ PROFILE CARD ============ */
        .profile-card {
            display: flex;
            align-items: center;
            gap: 1.5rem;
            background: white;
            border-radius: 16px;
            padding: 1.5rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
        }

        .profile-avatar {
            width: 70px;
            height: 70px;
            background: var(--gradient-primary);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .profile-avatar i {
            font-size: 1.75rem;
            color: white;
        }

        .profile-info {
            flex: 1;
        }

        .profile-header {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-bottom: 0.25rem;
        }

        .profile-header h2 {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1e293b;
            margin: 0;
        }

        .role-badge {
            padding: 0.25rem 0.75rem;
            background: rgba(102, 126, 234, 0.1);
            color: #667eea;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .role-badge.admin {
            background: rgba(118, 75, 162, 0.15);
            color: #764ba2;
        }

        .profile-email {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #64748b;
            font-size: 0.9rem;
            margin: 0;
        }

        .edit-profile-btn {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.25rem;
            background: transparent;
            border: 2px solid var(--color-primary);
            border-radius: 10px;
            color: var(--color-primary);
            font-weight: 600;
            font-size: 0.9rem;
            text-decoration: none;
            transition: all 0.3s;
        }

        .edit-profile-btn:hover {
            background: var(--gradient-primary);
            border-color: transparent;
            color: white;
        }

        /* ============ KPI GRID ============ */
        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 1rem;
        }

        .kpi-card {
            display: flex;
            align-items: center;
            gap: 1rem;
            background: white;
            border-radius: 14px;
            padding: 1.25rem;
            box-shadow: 0 2px 12px rgba(0,0,0,0.04);
            text-decoration: none;
            transition: all 0.2s;
        }

        .kpi-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 8px 24px rgba(0,0,0,0.08);
        }

        .kpi-icon {
            width: 48px;
            height: 48px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .kpi-icon i {
            font-size: 1.25rem;
            color: white;
        }

        .print3d-icon { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
        .laser-icon { background: linear-gradient(135deg, #764ba2 0%, #667eea 100%); }
        .orders-icon { background: linear-gradient(135deg, #667eea 0%, #5a67d8 100%); }
        .designs-icon { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }

        .kpi-info {
            flex: 1;
        }

        .kpi-value {
            display: block;
            font-size: 1.5rem;
            font-weight: 800;
            color: #1e293b;
        }

        .kpi-label {
            color: #64748b;
            font-size: 0.8rem;
        }

        .kpi-arrow {
            color: #cbd5e1;
            font-size: 0.9rem;
        }

        /* ============ REQUESTS CARD ============ */
        .requests-card {
            background: white;
            border-radius: 14px;
            padding: 1.25rem;
            box-shadow: 0 2px 12px rgba(0,0,0,0.04);
            min-height: 180px;
        }

        .loading-state {
            padding: 1rem;
        }

        .empty-state {
            text-align: center;
            padding: 2rem;
        }

        .empty-state i {
            font-size: 2.5rem;
            color: #cbd5e1;
            margin-bottom: 1rem;
        }

        .empty-state h4 {
            color: #1e293b;
            margin: 0 0 0.5rem;
            font-size: 1rem;
        }

        .empty-state p {
            color: #64748b;
            margin: 0 0 1.5rem;
            font-size: 0.9rem;
        }

        .start-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: var(--gradient-primary);
            color: white;
            border-radius: 10px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.2s;
        }

        .start-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
        }

        .requests-table {
            width: 100%;
            border-collapse: collapse;
        }

        .requests-table th {
            text-align: start;
            padding: 0.75rem;
            color: #64748b;
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 1px solid #e2e8f0;
        }

        .requests-table td {
            padding: 0.75rem;
            border-bottom: 1px solid #f1f5f9;
            font-size: 0.9rem;
            color: #1e293b;
        }

        .requests-table tr:last-child td {
            border-bottom: none;
        }

        .ref-number {
            font-family: monospace;
            font-weight: 600;
            color: #667eea;
            font-size: 0.85rem;
        }

        .type-badge {
            padding: 0.25rem 0.75rem;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
        }

        .type-badge.print3d {
            background: rgba(102, 126, 234, 0.1);
            color: #667eea;
        }

        .type-badge.laser {
            background: rgba(118, 75, 162, 0.1);
            color: #764ba2;
        }

        .type-badge.order {
            background: rgba(100, 116, 139, 0.1);
            color: #64748b;
        }

        .status-badge {
            padding: 0.25rem 0.75rem;
            border-radius: 50px;
            font-size: 0.75rem;
            font-weight: 600;
            background: rgba(100, 116, 139, 0.1);
            color: #64748b;
        }

        .status-badge.pending {
            background: rgba(234, 179, 8, 0.1);
            color: #ca8a04;
        }

        .status-badge.completed {
            background: rgba(102, 126, 234, 0.1);
            color: #667eea;
        }

        .view-btn {
            width: 32px;
            height: 32px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            background: #f1f5f9;
            border-radius: 8px;
            color: #64748b;
            text-decoration: none;
            transition: all 0.2s;
        }

        .view-btn:hover {
            background: var(--gradient-primary);
            color: white;
        }

        /* ============ ACTIONS GRID ============ */
        .actions-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 1rem;
        }

        .action-tile {
            display: flex;
            align-items: center;
            gap: 1rem;
            background: white;
            border-radius: 14px;
            padding: 1.25rem;
            box-shadow: 0 2px 12px rgba(0,0,0,0.04);
            text-decoration: none;
            transition: all 0.2s;
            border: 1px solid transparent;
        }

        .action-tile:hover {
            transform: translateY(-3px);
            box-shadow: 0 8px 24px rgba(0,0,0,0.08);
            border-color: rgba(102, 126, 234, 0.2);
        }

        .action-tile.primary {
            background: var(--gradient-primary);
        }

        .action-tile.primary .tile-icon {
            background: rgba(255,255,255,0.2);
        }

        .action-tile.primary .tile-icon i,
        .action-tile.primary .tile-title,
        .action-tile.primary .tile-desc,
        .action-tile.primary .tile-arrow {
            color: white;
        }

        .action-tile.secondary {
            background: #f8fafc;
        }

        .tile-icon {
            width: 44px;
            height: 44px;
            border-radius: 12px;
            background: rgba(102, 126, 234, 0.1);
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .tile-icon i {
            font-size: 1.25rem;
            color: #667eea;
        }

        .tile-content {
            flex: 1;
        }

        .tile-title {
            display: block;
            font-weight: 600;
            color: #1e293b;
            font-size: 0.95rem;
            margin-bottom: 0.125rem;
        }

        .tile-desc {
            color: #64748b;
            font-size: 0.8rem;
        }

        .tile-arrow {
            color: #cbd5e1;
            font-size: 0.9rem;
        }

        /* ============ ACCOUNT LINKS ============ */
        .account-links {
            display: flex;
            gap: 0.75rem;
            flex-wrap: wrap;
        }

        .account-link {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.6rem 1rem;
            background: white;
            border-radius: 10px;
            font-size: 0.85rem;
            font-weight: 500;
            color: #64748b;
            text-decoration: none;
            transition: all 0.2s;
            box-shadow: 0 1px 4px rgba(0,0,0,0.04);
        }

        .account-link:hover {
            color: #667eea;
            background: rgba(102, 126, 234, 0.05);
        }

        .account-link i {
            font-size: 1rem;
        }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 992px) {
            .kpi-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            .actions-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .profile-card {
                flex-direction: column;
                text-align: center;
            }
            .profile-header {
                justify-content: center;
            }
            .profile-email {
                justify-content: center;
            }
            .section-header {
                flex-direction: column;
                align-items: flex-start;
                gap: 0.75rem;
            }
            .section-actions {
                width: 100%;
            }
            .action-btn {
                flex: 1;
                justify-content: center;
            }
        }

        @media (max-width: 576px) {
            .kpi-grid {
                grid-template-columns: 1fr;
            }
            .actions-grid {
                grid-template-columns: 1fr;
            }
            .requests-table th:nth-child(4),
            .requests-table td:nth-child(4) {
                display: none;
            }
        }
    `]
})
export class AccountOverviewComponent implements OnInit {
    user: any = null;
    isAdmin = false;
    
    ordersCount = 0;
    print3dCount = 0;
    laserCount = 0;
    designsCount = 0;
    
    loadingStats = true;
    loadingRequests = true;
    
    recentRequests: RecentRequest[] = [];

    private authService = inject(AuthService);
    private ordersApiService = inject(OrdersApiService);
    private printingApiService = inject(PrintingApiService);
    private laserApiService = inject(LaserApiService);
    private translateService = inject(TranslateService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.user = this.authService.user;
        this.isAdmin = this.authService.isAdmin();
        this.loadStats();
        this.loadRecentRequests();
    }

    loadStats() {
        this.loadingStats = true;
        let completedCalls = 0;
        const totalCalls = 3;

        const checkComplete = () => {
            completedCalls++;
            if (completedCalls >= totalCalls) {
                this.loadingStats = false;
            }
        };

        // Load orders count
        if (this.user?.id) {
            this.ordersApiService.getUserOrders(this.user.id, { page: 1, pageSize: 1 }).subscribe({
                next: (response) => {
                    this.ordersCount = response.totalCount || 0;
                    checkComplete();
                },
                error: () => checkComplete()
            });
        } else {
            checkComplete();
        }

        // Load 3D print requests count
        this.printingApiService.getRequests({ page: 1, pageSize: 1 }).subscribe({
            next: (response) => {
                this.print3dCount = response.totalCount || 0;
                checkComplete();
            },
            error: () => checkComplete()
        });

        // Load laser requests count
        this.laserApiService.getServiceRequests({ limit: 100 }).subscribe({
            next: (requests) => {
                this.laserCount = requests?.length || 0;
                checkComplete();
            },
            error: () => checkComplete()
        });
    }

    loadRecentRequests() {
        this.loadingRequests = true;
        const requests: RecentRequest[] = [];
        let completedCalls = 0;
        const totalCalls = 2;

        const checkComplete = () => {
            completedCalls++;
            if (completedCalls >= totalCalls) {
                this.recentRequests = requests
                    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                    .slice(0, 5);
                this.loadingRequests = false;
            }
        };

        // Load 3D print requests
        this.printingApiService.getRequests({ page: 1, pageSize: 5 }).subscribe({
            next: (response) => {
                (response.items || []).forEach((r: any) => {
                    requests.push({
                        id: r.id,
                        referenceNumber: r.referenceNumber || r.id?.slice(0, 8),
                        type: 'print3d',
                        typeLabel: this.languageService.language === 'ar' ? 'طباعة 3D' : '3D Print',
                        status: r.status,
                        createdAt: r.createdAt,
                        link: '/account/requests'
                    });
                });
                checkComplete();
            },
            error: () => checkComplete()
        });

        // Load laser requests
        this.laserApiService.getServiceRequests({ limit: 5 }).subscribe({
            next: (laserRequests) => {
                (laserRequests || []).forEach((r: any) => {
                    requests.push({
                        id: r.id,
                        referenceNumber: r.referenceNumber || r.id?.slice(0, 8),
                        type: 'laser',
                        typeLabel: this.languageService.language === 'ar' ? 'ليزر' : 'Laser',
                        status: r.status || r.statusName,
                        createdAt: r.createdAt,
                        link: '/account/requests'
                    });
                });
                checkComplete();
            },
            error: () => checkComplete()
        });
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '-';
        const date = new Date(dateStr);
        return date.toLocaleDateString(this.languageService.language === 'ar' ? 'ar-EG' : 'en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
    }
}
