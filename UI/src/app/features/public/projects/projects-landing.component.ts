import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChipModule } from 'primeng/chip';
import { LanguageService } from '../../../shared/services/language.service';
import { ProjectsCategoryPillarBoxComponent } from './projects-category-pillar-box.component';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { ProjectListItem, ProjectCategory, ProjectStatus } from '../../../shared/models/project.model';

@Component({
    selector: 'app-projects-landing',
    standalone: true,
    imports: [
        CommonModule, RouterModule, FormsModule, TranslateModule,
        ButtonModule, CardModule, InputTextModule, SelectButtonModule,
        TagModule, ProgressSpinnerModule, ChipModule,
        ProjectsCategoryPillarBoxComponent
    ],
    template: `
        <div class="projects-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-floating-shapes">
                    <div class="shape shape-1"></div>
                    <div class="shape shape-2"></div>
                    <div class="shape shape-3"></div>
                </div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-folder-open"></i>
                        {{ 'projects.hero.badge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'projects.hero.title' | translate }}</h1>
                    <p class="hero-description">{{ 'projects.hero.description' | translate }}</p>
                    <div class="hero-cta">
                        <p-button
                            [label]="'projects.hero.cta' | translate"
                            icon="pi pi-arrow-down"
                            iconPos="right"
                            (onClick)="scrollToProjects()"
                            styleClass="hero-button-primary">
                        </p-button>
                        <p-button
                            [label]="'projects.hero.customProjects' | translate"
                            icon="pi pi-send"
                            iconPos="right"
                            routerLink="/projects/request"
                            [outlined]="true"
                            styleClass="hero-button-secondary">
                        </p-button>
                    </div>
                </div>
                <button type="button" class="hero-scroll-indicator" (click)="scrollToProjects()">
                    <i class="pi pi-chevron-down"></i>
                </button>
            </section>

            <!-- Filter Bar Section -->
            <section id="projects-section" class="filter-section">
                <div class="container">
                    <div class="filters-card">
                        <div class="filter-bar">
                            <div class="search-box">
                                <i class="pi pi-search"></i>
                                <input 
                                    type="text" 
                                    pInputText 
                                    [placeholder]="'projects.filter.searchPlaceholder' | translate"
                                    [(ngModel)]="searchTerm"
                                    (input)="onSearchChange()"
                                />
                            </div>
                            <div class="filter-row">
                                <span class="filter-label">{{ 'projects.filter.categoryLabel' | translate }}:</span>
                                <div class="filter-chips-wrap">
                                    <span 
                                        class="filter-chip" 
                                        [class.active]="selectedCategory() === null"
                                        (click)="setCategory(null)">
                                        {{ 'projects.filter.all' | translate }}
                                    </span>
                                    @for (cat of categoryOptions; track cat.value) {
                                        <span 
                                            class="filter-chip"
                                            [class.active]="selectedCategory() === cat.value"
                                            (click)="setCategory(cat.value)">
                                            {{ 'projects.categories.' + cat.name | translate }}
                                        </span>
                                    }
                                </div>
                            </div>
                            <div class="filter-row">
                                <span class="filter-label">{{ 'projects.filter.statusLabel' | translate }}:</span>
                                <div class="filter-chips-wrap">
                                    <span 
                                        class="filter-chip status-chip" 
                                        [class.active]="selectedStatus() === null"
                                        (click)="setStatus(null)">
                                        {{ 'projects.filter.allStatuses' | translate }}
                                    </span>
                                    @for (st of statusOptions; track st.value) {
                                        <span 
                                            class="filter-chip status-chip"
                                            [class.active]="selectedStatus() === st.value"
                                            (click)="setStatus(st.value)">
                                            {{ 'projects.statuses.' + st.name | translate }}
                                    </span>
                                    }
                                    <span 
                                        class="filter-chip featured-chip"
                                        [class.active]="selectedFeatured()"
                                        (click)="toggleFeatured()">
                                        <i class="pi pi-star-fill"></i>
                                        {{ 'projects.filter.featuredOnly' | translate }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Category pillar box (when a specific category is selected) -->
            @if (selectedCategory() !== null) {
                <app-projects-category-pillar-box [category]="selectedCategory()!"></app-projects-category-pillar-box>
            }

            <!-- Projects Grid Section -->
            <section class="projects-grid-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'projects.section.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'projects.section.title' | translate }}</h2>
                        <p class="section-description">{{ 'projects.section.description' | translate }}</p>
                    </div>

                    @if (loading()) {
                        <div class="loading-container">
                            <p-progressSpinner strokeWidth="3"></p-progressSpinner>
                            <p>{{ 'common.loading' | translate }}</p>
                        </div>
                    } @else if (projects().length === 0) {
                        <div class="empty-state">
                            <i class="pi pi-inbox"></i>
                            <h3>{{ 'projects.empty.title' | translate }}</h3>
                            <p>{{ 'projects.empty.description' | translate }}</p>
                            @if (hasActiveFilters()) {
                                <p-button 
                                    [label]="'projects.filter.clearFilters' | translate" 
                                    (onClick)="clearFilters()"
                                    styleClass="clear-filters-btn">
                                </p-button>
                            }
                        </div>
                    } @else {
                        <div class="projects-grid">
                            @for (project of projects(); track project.id) {
                                <a [routerLink]="['/projects', project.slug]" class="project-card">
                                    <div class="project-image">
                                        <img [src]="project.coverImageUrl || 'assets/img/project-placeholder.png'" [alt]="getTitle(project)" />
                                        <div class="project-year">{{ project.year }}</div>
                                        @if (project.isFeatured) {
                                            <div class="featured-badge">
                                                <i class="pi pi-star-fill"></i>
                                            </div>
                                        }
                                    </div>
                                    <div class="project-content">
                                        <div class="project-meta">
                                            <p-tag [value]="'projects.categories.' + project.categoryName | translate" [severity]="getCategorySeverity(project.category)" styleClass="category-tag"></p-tag>
                                            <p-tag [value]="'projects.statuses.' + project.statusName | translate" [severity]="getStatusSeverity(project.status)" styleClass="status-tag"></p-tag>
                                        </div>
                                        <h3 class="project-title">{{ getTitle(project) }}</h3>
                                        <p class="project-summary">{{ getSummary(project) }}</p>
                                        <div class="tech-stack">
                                            @for (tech of project.techStack.slice(0, 3); track tech) {
                                                <span class="tech-chip">{{ tech }}</span>
                                            }
                                            @if (project.techStack.length > 3) {
                                                <span class="tech-chip more">+{{ project.techStack.length - 3 }}</span>
                                            }
                                        </div>
                                        <div class="project-cta">
                                            <span class="view-case-study">
                                                {{ 'projects.viewCaseStudy' | translate }}
                                                <i class="pi pi-arrow-right"></i>
                                            </span>
                                        </div>
                                    </div>
                                </a>
                            }
                        </div>

                        @if (totalCount() > projects().length) {
                            <div class="load-more">
                                <p-button 
                                    [label]="'projects.loadMore' | translate"
                                    icon="pi pi-refresh"
                                    (onClick)="loadMore()"
                                    [loading]="loadingMore()"
                                    styleClass="load-more-btn">
                                </p-button>
                            </div>
                        }
                    }
                </div>
            </section>

            <!-- How We Deliver Section -->
            <section class="workflow-section">
                <div class="container">
                    <div class="section-header">
                        <span class="section-badge">{{ 'projects.workflow.badge' | translate }}</span>
                        <h2 class="section-title">{{ 'projects.workflow.title' | translate }}</h2>
                        <p class="section-description">{{ 'projects.workflow.description' | translate }}</p>
                    </div>
                    <div class="workflow-steps">
                        <div class="workflow-step">
                            <div class="step-number">01</div>
                            <div class="step-icon"><i class="pi pi-comments"></i></div>
                            <h3>{{ 'projects.workflow.step1.title' | translate }}</h3>
                            <p>{{ 'projects.workflow.step1.description' | translate }}</p>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-number">02</div>
                            <div class="step-icon"><i class="pi pi-pencil"></i></div>
                            <h3>{{ 'projects.workflow.step2.title' | translate }}</h3>
                            <p>{{ 'projects.workflow.step2.description' | translate }}</p>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-number">03</div>
                            <div class="step-icon"><i class="pi pi-cog"></i></div>
                            <h3>{{ 'projects.workflow.step3.title' | translate }}</h3>
                            <p>{{ 'projects.workflow.step3.description' | translate }}</p>
                        </div>
                        <div class="workflow-connector"></div>
                        <div class="workflow-step">
                            <div class="step-number">04</div>
                            <div class="step-icon"><i class="pi pi-check-circle"></i></div>
                            <h3>{{ 'projects.workflow.step4.title' | translate }}</h3>
                            <p>{{ 'projects.workflow.step4.description' | translate }}</p>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Lead Gen CTA Section -->
            <section class="cta-section">
                <div class="cta-bg"></div>
                <div class="container">
                    <div class="cta-content">
                        <h2 class="cta-title">{{ 'projects.cta.title' | translate }}</h2>
                        <p class="cta-description">{{ 'projects.cta.description' | translate }}</p>
                        <div class="cta-buttons">
                            <p-button
                                [label]="'projects.cta.requestSimilar' | translate"
                                icon="pi pi-copy"
                                routerLink="/projects/request"
                                [queryParams]="{type: 'similar'}"
                                styleClass="cta-btn-primary">
                            </p-button>
                            <p-button
                                [label]="'projects.cta.startCustom' | translate"
                                icon="pi pi-plus"
                                routerLink="/projects/request"
                                [queryParams]="{type: 'custom'}"
                                [outlined]="true"
                                styleClass="cta-btn-secondary">
                            </p-button>
                        </div>
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

        .projects-page { width: 100%; overflow-x: hidden; background: #fafbfc; }
        .container { max-width: 1200px; margin: 0 auto; padding: 0 1.5rem; }

        /* Hero Section */
        .hero-section {
            position: relative;
            min-height: 70vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 6rem 2rem;
            overflow: hidden;
        }
        .hero-bg-gradient { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .hero-pattern {
            position: absolute; inset: 0;
            background-image: radial-gradient(circle at 25% 25%, rgba(102, 126, 234, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(118, 75, 162, 0.15) 0%, transparent 50%);
            z-index: 1;
        }
        .hero-floating-shapes { position: absolute; inset: 0; z-index: 2; pointer-events: none; }
        .shape { position: absolute; border-radius: 50%; filter: blur(60px); opacity: 0.5; }
        .shape-1 { width: 400px; height: 400px; background: var(--gradient-primary); top: -100px; right: -100px; }
        .shape-2 { width: 300px; height: 300px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); bottom: -50px; left: -50px; }
        .shape-3 { width: 200px; height: 200px; background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); top: 50%; left: 10%; }

        .hero-content { position: relative; z-index: 10; text-align: center; max-width: 680px; margin: 0 auto; }
        .hero-badge { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: rgba(255,255,255,0.15); border-radius: 50px; font-size: 0.875rem; font-weight: 600; color: white; margin-bottom: 1.5rem; }
        .hero-title { font-size: clamp(2rem, 5vw, 3.5rem); font-weight: 800; color: white; margin-bottom: 1rem; line-height: 1.2; }
        .hero-description { font-size: clamp(1rem, 1.5vw, 1.2rem); color: rgba(255,255,255,0.9); line-height: 1.7; margin-bottom: 2rem; }
        .hero-cta { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
        :host ::ng-deep .hero-button-primary { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.875rem 1.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .hero-button-secondary { background: transparent !important; border: 2px solid rgba(255,255,255,0.6) !important; color: white !important; padding: 0.875rem 1.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .hero-button-secondary:hover { background: rgba(255,255,255,0.1) !important; border-color: white !important; }
        .hero-scroll-indicator { position: absolute; bottom: 2rem; left: 50%; transform: translateX(-50%); z-index: 10; color: rgba(255,255,255,0.6); font-size: 1.5rem; cursor: pointer; background: none; border: none; animation: bounce 2s infinite; }
        @keyframes bounce { 0%, 20%, 50%, 80%, 100% { transform: translateX(-50%) translateY(0); } 40% { transform: translateX(-50%) translateY(-15px); } 60% { transform: translateX(-50%) translateY(-7px); } }

        /* ── Filter Section ── */
        .filter-section {
            padding: 0;
            background: #fff;
            position: sticky;
            top: 0;
            z-index: 100;
            border-bottom: 1px solid #e5e7eb;
        }
        .filters-card { background: transparent; border: none; box-shadow: none; padding: 1.1rem 0; }
        .filter-bar { display: flex; flex-direction: column; gap: 0.85rem; }

        /* Search */
        .search-box { position: relative; width: 100%; }
        .search-box i {
            position: absolute; left: 0.85rem; top: 50%;
            transform: translateY(-50%);
            color: #9ca3af; font-size: 0.9rem; pointer-events: none;
        }
        .search-box input {
            width: 100%; height: 40px;
            padding: 0 1rem 0 2.5rem;
            border: 1px solid #d1d5db; border-radius: 5px;
            font-size: 0.95rem; color: #111827; background: #fff;
            transition: border-color 0.15s; box-sizing: border-box;
        }
        .search-box input:focus { outline: none; border-color: #667eea; }

        /* Filter rows */
        .filter-row { display: flex; align-items: baseline; gap: 0; flex-wrap: wrap; }
        .filter-label {
            font-size: 0.75rem; font-weight: 700;
            text-transform: uppercase; letter-spacing: 0.08em;
            color: #9ca3af; min-width: 6.5rem; flex-shrink: 0;
            padding-right: 0.5rem; line-height: 2.4;
        }
        .filter-chips-wrap { display: flex; flex-wrap: wrap; align-items: baseline; gap: 0; flex: 1; }

        /* Text-tab style */
        .filter-chip {
            display: inline-flex; align-items: center; gap: 0.3rem;
            padding: 0.4rem 0.9rem;
            font-size: 0.95rem; font-weight: 400;
            color: #6b7280; cursor: pointer; user-select: none;
            background: none; border: none; outline: none;
            position: relative; white-space: nowrap;
            transition: color 0.12s;
        }
        .filter-chip::after {
            content: ''; position: absolute;
            bottom: -1px; left: 0.9rem; right: 0.9rem;
            height: 2px; background: #667eea;
            transform: scaleX(0); transition: transform 0.15s ease;
        }
        .filter-chip:hover { color: #374151; }
        .filter-chip.active { color: #667eea; font-weight: 600; }
        .filter-chip.active::after { transform: scaleX(1); }
        .filter-chip.featured-chip.active { color: #667eea; }
        .filter-chip i { font-size: 0.78rem; }

        /* Projects Grid Section */
        .projects-grid-section { padding: 4rem 0; }
        .section-header { text-align: center; margin-bottom: 3rem; }
        .section-badge { display: inline-block; padding: 0.5rem 1.25rem; background: var(--gradient-primary); color: white; border-radius: 50px; font-size: 0.875rem; font-weight: 600; margin-bottom: 0.75rem; }
        .section-title { font-size: clamp(1.75rem, 4vw, 2.5rem); font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; }
        .section-description { color: #6b7280; max-width: 600px; margin: 0 auto; }

        .loading-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4rem; gap: 1rem; }
        .empty-state { text-align: center; padding: 4rem 2rem; }
        .empty-state i { font-size: 4rem; color: #d1d5db; margin-bottom: 1rem; display: block; }
        .empty-state h3 { font-size: 1.5rem; color: #374151; margin-bottom: 0.5rem; }
        .empty-state p { color: #6b7280; margin-bottom: 1.5rem; }
        :host ::ng-deep .clear-filters-btn { background: var(--gradient-primary) !important; border: none !important; }

        .projects-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 2rem; }
        .project-card {
            display: block;
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
            transition: transform 0.3s, box-shadow 0.3s;
            text-decoration: none;
            color: inherit;
        }
        .project-card:hover { transform: translateY(-6px); box-shadow: 0 12px 40px rgba(0,0,0,0.15); }
        .project-image { position: relative; aspect-ratio: 16/10; overflow: hidden; background: linear-gradient(135deg, #667eea20, #764ba220); }
        .project-image img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
        .project-card:hover .project-image img { transform: scale(1.05); }
        .project-year { position: absolute; top: 1rem; left: 1rem; background: rgba(0,0,0,0.7); color: white; padding: 0.25rem 0.75rem; border-radius: 6px; font-size: 0.8rem; font-weight: 600; }
        .featured-badge { position: absolute; top: 1rem; right: 1rem; background: linear-gradient(135deg, #f59e0b, #d97706); color: white; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; }

        .project-content { padding: 1.5rem; }
        .project-meta { display: flex; gap: 0.5rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
        :host ::ng-deep .category-tag, :host ::ng-deep .status-tag { font-size: 0.75rem !important; padding: 0.25rem 0.5rem !important; }
        .project-title { font-size: 1.25rem; font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; line-height: 1.3; }
        .project-summary { color: #6b7280; font-size: 0.9rem; line-height: 1.6; margin-bottom: 1rem; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

        .tech-stack { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 1rem; }
        .tech-chip { padding: 0.25rem 0.75rem; background: #f3f4f6; color: #4b5563; border-radius: 6px; font-size: 0.75rem; font-weight: 500; }
        .tech-chip.more { background: #e5e7eb; }

        .project-cta { padding-top: 1rem; border-top: 1px solid #f3f4f6; }
        .view-case-study { display: inline-flex; align-items: center; gap: 0.5rem; color: var(--color-primary); font-weight: 600; font-size: 0.9rem; }
        .view-case-study i { transition: transform 0.2s; }
        .project-card:hover .view-case-study i { transform: translateX(4px); }

        .load-more { text-align: center; margin-top: 3rem; }
        :host ::ng-deep .load-more-btn { background: var(--gradient-primary) !important; border: none !important; padding: 0.875rem 2rem !important; }

        /* Workflow Section */
        .workflow-section { padding: 5rem 0; background: linear-gradient(180deg, #f8fafc 0%, #ffffff 100%); }
        .workflow-steps { display: flex; align-items: flex-start; justify-content: center; gap: 0; flex-wrap: wrap; margin-top: 3rem; }
        .workflow-step { flex: 1; min-width: 200px; max-width: 250px; text-align: center; padding: 1.5rem; }
        .step-number { font-size: 0.75rem; font-weight: 700; color: var(--color-primary); margin-bottom: 0.5rem; }
        .step-icon { width: 64px; height: 64px; margin: 0 auto 1rem; background: var(--gradient-primary); border-radius: 16px; display: flex; align-items: center; justify-content: center; }
        .step-icon i { font-size: 1.5rem; color: white; }
        .workflow-step h3 { font-size: 1.1rem; font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; }
        .workflow-step p { font-size: 0.875rem; color: #6b7280; line-height: 1.6; }
        .workflow-connector { width: 60px; height: 2px; background: linear-gradient(90deg, var(--color-primary), var(--color-secondary)); margin-top: 4rem; flex-shrink: 0; }
        @media (max-width: 900px) { .workflow-connector { display: none; } }

        /* CTA Section */
        .cta-section { position: relative; padding: 5rem 2rem; overflow: hidden; }
        .cta-bg { position: absolute; inset: 0; background: var(--gradient-dark); z-index: 0; }
        .cta-content { position: relative; z-index: 10; text-align: center; max-width: 600px; margin: 0 auto; }
        .cta-title { font-size: clamp(1.5rem, 3vw, 2rem); font-weight: 700; color: white; margin-bottom: 0.75rem; }
        .cta-description { color: rgba(255,255,255,0.9); margin-bottom: 2rem; line-height: 1.7; }
        .cta-buttons { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
        :host ::ng-deep .cta-btn-primary { background: var(--gradient-primary) !important; border: none !important; color: white !important; padding: 0.875rem 1.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .cta-btn-secondary { background: transparent !important; border: 2px solid rgba(255,255,255,0.6) !important; color: white !important; padding: 0.875rem 1.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        :host ::ng-deep .cta-btn-secondary:hover { background: rgba(255,255,255,0.1) !important; border-color: white !important; }

        @media (max-width: 768px) {
            .projects-grid { grid-template-columns: 1fr; }
            .filter-chips-wrap { overflow-x: auto; flex-wrap: nowrap; padding-bottom: 4px; -webkit-overflow-scrolling: touch; scrollbar-width: none; }
            .filter-chips-wrap::-webkit-scrollbar { display: none; }
            .filter-label { min-width: 4.5rem; }
        }
    `]
})
export class ProjectsLandingComponent implements OnInit {
    languageService = inject(LanguageService);
    private projectsApi = inject(ProjectsApiService);
    private translate = inject(TranslateService);

    loading = signal(true);
    loadingMore = signal(false);
    projects = signal<ProjectListItem[]>([]);
    totalCount = signal(0);
    currentPage = signal(1);
    pageSize = 9;

    searchTerm = '';
    selectedCategory = signal<ProjectCategory | null>(null);
    selectedStatus = signal<ProjectStatus | null>(null);
    selectedFeatured = signal(false);

    categoryOptions = [
        { value: ProjectCategory.Robotics, name: 'Robotics' },
        { value: ProjectCategory.CNC, name: 'CNC' },
        { value: ProjectCategory.Embedded, name: 'Embedded' },
        { value: ProjectCategory.Monitoring, name: 'Monitoring' },
        { value: ProjectCategory.Software, name: 'softwareSystems' },
        { value: ProjectCategory.RnD, name: 'RnD' }
    ];

    statusOptions = [
        { value: ProjectStatus.Concept, name: 'Concept' },
        { value: ProjectStatus.Prototype, name: 'Prototype' },
        { value: ProjectStatus.Delivered, name: 'Delivered' }
    ];

    ngOnInit() {
        this.loadProjects();
    }

    loadProjects(append = false) {
        if (append) {
            this.loadingMore.set(true);
        } else {
            this.loading.set(true);
            this.currentPage.set(1);
        }

        this.projectsApi.getProjects({
            search: this.searchTerm || undefined,
            category: this.selectedCategory() ?? undefined,
            status: this.selectedStatus() ?? undefined,
            isFeatured: this.selectedFeatured() ? true : undefined,
            page: this.currentPage(),
            pageSize: this.pageSize
        }).subscribe({
            next: (response) => {
                if (append) {
                    this.projects.update(current => [...current, ...response.items]);
                } else {
                    this.projects.set(response.items);
                }
                this.totalCount.set(response.totalCount);
                this.loading.set(false);
                this.loadingMore.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.loadingMore.set(false);
            }
        });
    }

    onSearchChange() {
        this.loadProjects();
    }

    setCategory(category: ProjectCategory | null) {
        this.selectedCategory.set(category);
        this.loadProjects();
    }

    setStatus(status: ProjectStatus | null) {
        this.selectedStatus.set(status);
        this.loadProjects();
    }

    toggleFeatured() {
        this.selectedFeatured.update(v => !v);
        this.loadProjects();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedCategory() !== null || this.selectedStatus() !== null || this.selectedFeatured();
    }

    clearFilters() {
        this.searchTerm = '';
        this.selectedCategory.set(null);
        this.selectedStatus.set(null);
        this.selectedFeatured.set(false);
        this.loadProjects();
    }

    loadMore() {
        this.currentPage.update(p => p + 1);
        this.loadProjects(true);
    }

    scrollToProjects() {
        document.querySelector('#projects-section')?.scrollIntoView({ behavior: 'smooth' });
    }

    getTitle(project: ProjectListItem): string {
        return this.languageService.language === 'ar' && project.titleAr ? project.titleAr : project.titleEn;
    }

    getSummary(project: ProjectListItem): string {
        return this.languageService.language === 'ar' && project.summaryAr ? project.summaryAr : (project.summaryEn || '');
    }

    getCategorySeverity(category: ProjectCategory): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectCategory.Robotics]: 'info',
            [ProjectCategory.CNC]: 'warn',
            [ProjectCategory.Embedded]: 'success',
            [ProjectCategory.Monitoring]: 'contrast',
            [ProjectCategory.Software]: 'secondary',
            [ProjectCategory.RnD]: 'danger'
        };
        return map[category] || 'secondary';
    }

    getStatusSeverity(status: ProjectStatus): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectStatus.Concept]: 'warn',
            [ProjectStatus.Prototype]: 'info',
            [ProjectStatus.Delivered]: 'success'
        };
        return map[status] || 'secondary';
    }
}
