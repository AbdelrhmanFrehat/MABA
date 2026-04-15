import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { GalleriaModule } from 'primeng/galleria';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { LanguageService } from '../../../shared/services/language.service';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { Project, ProjectCategory, ProjectStatus } from '../../../shared/models/project.model';
import { DownloadableFilesSectionComponent } from '../../../shared/components/downloadable-files-section/downloadable-files-section.component';

@Component({
    selector: 'app-project-detail',
    standalone: true,
    imports: [
        CommonModule, RouterModule, TranslateModule,
        ButtonModule, TagModule, ChipModule, GalleriaModule, ProgressSpinnerModule,
        DownloadableFilesSectionComponent
    ],
    template: `
        <div class="project-detail-page" [dir]="languageService.direction">
            @if (loading()) {
                <div class="loading-container">
                    <p-progressSpinner strokeWidth="3"></p-progressSpinner>
                    <p>{{ 'common.loading' | translate }}</p>
                </div>
            } @else if (!project()) {
                <div class="not-found">
                    <i class="pi pi-exclamation-triangle"></i>
                    <h2>{{ 'projects.notFound.title' | translate }}</h2>
                    <p>{{ 'projects.notFound.description' | translate }}</p>
                    <p-button [label]="'projects.notFound.backToProjects' | translate" routerLink="/projects" styleClass="back-btn"></p-button>
                </div>
            } @else {
                <!-- Hero Section -->
                <section class="hero-section">
                    <div class="hero-bg">
                        <img [src]="project()!.coverImageUrl || 'assets/img/project-placeholder.png'" [alt]="getTitle()" />
                        <div class="hero-overlay"></div>
                    </div>
                    <div class="hero-content">
                        <div class="breadcrumb">
                            <a routerLink="/projects">{{ 'menu.projects' | translate }}</a>
                            <i class="pi pi-chevron-right"></i>
                            <span>{{ getTitle() }}</span>
                        </div>
                        <div class="hero-meta">
                            <p-tag [value]="'projects.categories.' + project()!.categoryName | translate" [severity]="getCategorySeverity()" styleClass="category-tag"></p-tag>
                            <p-tag [value]="'projects.statuses.' + project()!.statusName | translate" [severity]="getStatusSeverity()" styleClass="status-tag"></p-tag>
                            <span class="year-badge">{{ project()!.year }}</span>
                        </div>
                        <h1 class="hero-title">{{ getTitle() }}</h1>
                        <p class="hero-summary">{{ getSummary() }}</p>
                    </div>
                </section>

                <div class="content-wrapper">
                    <div class="container">
                        <div class="content-grid">
                            <!-- Main Content -->
                            <div class="main-content">
                                <!-- Highlights -->
                                @if (project()!.highlights && project()!.highlights.length > 0) {
                                    <section class="highlights-section">
                                        <h2><i class="pi pi-star"></i> {{ 'projects.detail.highlights' | translate }}</h2>
                                        <ul class="highlights-list">
                                            @for (highlight of project()!.highlights; track highlight) {
                                                <li>
                                                    <i class="pi pi-check-circle"></i>
                                                    <span>{{ highlight }}</span>
                                                </li>
                                            }
                                        </ul>
                                    </section>
                                }

                                <!-- Description -->
                                @if (getDescription()) {
                                    <section class="description-section">
                                        <h2><i class="pi pi-file-edit"></i> {{ 'projects.detail.description' | translate }}</h2>
                                        <div class="description-content" [innerHTML]="getSafeDescription()"></div>
                                    </section>
                                }

                                <!-- Gallery -->
                                @if (project()!.gallery && project()!.gallery.length > 0) {
                                    <section class="gallery-section">
                                        <h2><i class="pi pi-images"></i> {{ 'projects.detail.gallery' | translate }}</h2>
                                        <p-galleria 
                                            [value]="galleryImages()" 
                                            [numVisible]="5"
                                            [circular]="true"
                                            [showItemNavigators]="true"
                                            [showThumbnails]="true"
                                            [responsiveOptions]="galleriaResponsiveOptions">
                                            <ng-template pTemplate="item" let-item>
                                                <img [src]="item" class="gallery-main-image" />
                                            </ng-template>
                                            <ng-template pTemplate="thumbnail" let-item>
                                                <img [src]="item" class="gallery-thumbnail" />
                                            </ng-template>
                                        </p-galleria>
                                    </section>
                                }
                                <!-- Downloadable Files Section -->
                                <app-downloadable-files-section
                                    entityType="Project"
                                    [entityId]="project()!.id">
                                </app-downloadable-files-section>
                            </div>

                            <!-- Sidebar -->
                            <aside class="sidebar">
                                <!-- Tech Stack -->
                                @if (project()!.techStack && project()!.techStack.length > 0) {
                                    <div class="sidebar-card">
                                        <h3><i class="pi pi-code"></i> {{ 'projects.detail.techStack' | translate }}</h3>
                                        <div class="tech-chips">
                                            @for (tech of project()!.techStack; track tech) {
                                                <span class="tech-chip">{{ tech }}</span>
                                            }
                                        </div>
                                    </div>
                                }

                                <!-- Project Info -->
                                <div class="sidebar-card">
                                    <h3><i class="pi pi-info-circle"></i> {{ 'projects.detail.info' | translate }}</h3>
                                    <ul class="info-list">
                                        <li>
                                            <span class="label">{{ 'projects.detail.category' | translate }}</span>
                                            <span class="value">{{ 'projects.categories.' + project()!.categoryName | translate }}</span>
                                        </li>
                                        <li>
                                            <span class="label">{{ 'projects.detail.status' | translate }}</span>
                                            <span class="value">{{ 'projects.statuses.' + project()!.statusName | translate }}</span>
                                        </li>
                                        <li>
                                            <span class="label">{{ 'projects.detail.year' | translate }}</span>
                                            <span class="value">{{ project()!.year }}</span>
                                        </li>
                                    </ul>
                                </div>

                                <!-- CTA Card -->
                                <div class="sidebar-card cta-card">
                                    <h3>{{ 'projects.detail.interested' | translate }}</h3>
                                    <p>{{ 'projects.detail.interestedDescription' | translate }}</p>
                                    <p-button
                                        [label]="'projects.cta.requestSimilar' | translate"
                                        icon="pi pi-copy"
                                        routerLink="/projects/request"
                                        [queryParams]="{type: 'similar', project: project()!.id}"
                                        styleClass="w-full cta-btn-primary mb-2">
                                    </p-button>
                                    <p-button
                                        [label]="'projects.cta.startCustom' | translate"
                                        icon="pi pi-plus"
                                        routerLink="/projects/request"
                                        [queryParams]="{type: 'custom'}"
                                        [outlined]="true"
                                        styleClass="w-full cta-btn-secondary">
                                    </p-button>
                                </div>
                            </aside>
                        </div>
                    </div>
                </div>

                <!-- Bottom CTA -->
                <section class="bottom-cta">
                    <div class="container">
                        <div class="cta-content">
                            <h2>{{ 'projects.detail.ctaTitle' | translate }}</h2>
                            <p>{{ 'projects.detail.ctaDescription' | translate }}</p>
                            <div class="cta-buttons">
                                <p-button
                                    [label]="'projects.cta.requestSimilar' | translate"
                                    icon="pi pi-copy"
                                    routerLink="/projects/request"
                                    [queryParams]="{type: 'similar', project: project()!.id}"
                                    styleClass="cta-btn-primary">
                                </p-button>
                                <p-button
                                    [label]="'menu.projects' | translate"
                                    icon="pi pi-arrow-left"
                                    routerLink="/projects"
                                    [outlined]="true"
                                    styleClass="cta-btn-secondary">
                                </p-button>
                            </div>
                        </div>
                    </div>
                </section>
            }
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
        }

        .project-detail-page { min-height: 100vh; background: #fafbfc; }
        .container { max-width: 1200px; margin: 0 auto; padding: 0 1.5rem; }

        .loading-container, .not-found { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 60vh; gap: 1rem; text-align: center; }
        .not-found i { font-size: 4rem; color: #d1d5db; }
        .not-found h2 { font-size: 1.5rem; color: #374151; }
        .not-found p { color: #6b7280; margin-bottom: 1rem; }
        :host ::ng-deep .back-btn { background: var(--gradient-primary) !important; border: none !important; }

        /* Hero Section */
        .hero-section { position: relative; min-height: 50vh; display: flex; align-items: flex-end; padding: 4rem 0; }
        .hero-bg { position: absolute; inset: 0; }
        .hero-bg img { width: 100%; height: 100%; object-fit: cover; }
        .hero-overlay { position: absolute; inset: 0; background: linear-gradient(to top, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.4) 50%, rgba(0,0,0,0.2) 100%); }
        .hero-content { position: relative; z-index: 10; width: 100%; max-width: 1200px; margin: 0 auto; padding: 0 1.5rem; }

        .breadcrumb { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; font-size: 0.875rem; }
        .breadcrumb a { color: rgba(255,255,255,0.7); text-decoration: none; }
        .breadcrumb a:hover { color: white; }
        .breadcrumb i { color: rgba(255,255,255,0.5); font-size: 0.75rem; }
        .breadcrumb span { color: white; }

        .hero-meta { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; flex-wrap: wrap; }
        :host ::ng-deep .category-tag, :host ::ng-deep .status-tag { font-size: 0.8rem !important; }
        .year-badge { background: rgba(255,255,255,0.2); color: white; padding: 0.25rem 0.75rem; border-radius: 6px; font-size: 0.8rem; font-weight: 600; }

        .hero-title { font-size: clamp(2rem, 5vw, 3rem); font-weight: 800; color: white; margin-bottom: 1rem; line-height: 1.2; }
        .hero-summary { font-size: 1.1rem; color: rgba(255,255,255,0.9); line-height: 1.7; max-width: 700px; }

        /* Content */
        .content-wrapper { padding: 4rem 0; }
        .content-grid { display: grid; grid-template-columns: 1fr 350px; gap: 3rem; }
        @media (max-width: 900px) { .content-grid { grid-template-columns: 1fr; } }

        .main-content section { background: white; border-radius: 16px; padding: 2rem; margin-bottom: 2rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
        .main-content h2 { display: flex; align-items: center; gap: 0.75rem; font-size: 1.25rem; font-weight: 700; color: #1a1a2e; margin-bottom: 1.5rem; padding-bottom: 1rem; border-bottom: 1px solid #f3f4f6; }
        .main-content h2 i { color: var(--color-primary); }

        .highlights-list { list-style: none; padding: 0; margin: 0; }
        .highlights-list li { display: flex; align-items: flex-start; gap: 0.75rem; padding: 0.75rem 0; border-bottom: 1px solid #f3f4f6; }
        .highlights-list li:last-child { border-bottom: none; }
        .highlights-list li i { color: var(--color-primary); margin-top: 0.2rem; }
        .highlights-list li span { color: #374151; line-height: 1.6; }

        .description-content { color: #4b5563; line-height: 1.8; }
        .description-content p { margin-bottom: 1rem; }

        .gallery-section { padding: 2rem; }
        :host ::ng-deep .gallery-main-image { width: 100%; max-height: 500px; object-fit: contain; border-radius: 12px; }
        :host ::ng-deep .gallery-thumbnail { width: 80px; height: 60px; object-fit: cover; border-radius: 6px; }

        /* Sidebar */
        .sidebar { position: sticky; top: 2rem; }
        .sidebar-card { background: white; border-radius: 16px; padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
        .sidebar-card h3 { display: flex; align-items: center; gap: 0.5rem; font-size: 1rem; font-weight: 700; color: #1a1a2e; margin-bottom: 1rem; }
        .sidebar-card h3 i { color: var(--color-primary); }

        .tech-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; }
        .tech-chip { padding: 0.375rem 0.75rem; background: linear-gradient(135deg, #667eea15, #764ba215); color: var(--color-primary); border-radius: 8px; font-size: 0.8rem; font-weight: 500; }

        .info-list { list-style: none; padding: 0; margin: 0; }
        .info-list li { display: flex; justify-content: space-between; padding: 0.75rem 0; border-bottom: 1px solid #f3f4f6; }
        .info-list li:last-child { border-bottom: none; }
        .info-list .label { color: #6b7280; }
        .info-list .value { color: #1a1a2e; font-weight: 600; }

        .cta-card { background: var(--gradient-dark); }
        .cta-card h3 { color: white !important; }
        .cta-card h3 i { color: white !important; }
        .cta-card p { color: rgba(255,255,255,0.8); font-size: 0.9rem; line-height: 1.6; margin-bottom: 1.5rem; }
        :host ::ng-deep .cta-btn-primary { background: var(--gradient-primary) !important; border: none !important; }
        :host ::ng-deep .cta-btn-secondary { border-color: rgba(255,255,255,0.4) !important; color: white !important; }
        :host ::ng-deep .cta-btn-secondary:hover { background: rgba(255,255,255,0.1) !important; }

        /* Bottom CTA */
        .bottom-cta { background: var(--gradient-dark); padding: 4rem 0; }
        .bottom-cta .cta-content { text-align: center; max-width: 600px; margin: 0 auto; }
        .bottom-cta h2 { font-size: 1.75rem; font-weight: 700; color: white; margin-bottom: 0.75rem; }
        .bottom-cta p { color: rgba(255,255,255,0.9); margin-bottom: 2rem; line-height: 1.7; }
        .cta-buttons { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
    `]
})
export class ProjectDetailComponent implements OnInit {
    languageService = inject(LanguageService);
    private projectsApi = inject(ProjectsApiService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private sanitizer = inject(DomSanitizer);

    loading = signal(true);
    project = signal<Project | null>(null);

    galleryImages = signal<string[]>([]);

    galleriaResponsiveOptions = [
        { breakpoint: '1024px', numVisible: 5 },
        { breakpoint: '768px', numVisible: 3 },
        { breakpoint: '560px', numVisible: 1 }
    ];

    ngOnInit() {
        const slug = this.route.snapshot.paramMap.get('slug');
        if (slug) {
            this.projectsApi.getProjectBySlug(slug).subscribe({
                next: (project) => {
                    this.project.set(project);
                    this.galleryImages.set(project.gallery || []);
                    this.loading.set(false);
                },
                error: () => {
                    this.loading.set(false);
                }
            });
        } else {
            this.loading.set(false);
        }
    }

    getTitle(): string {
        const p = this.project();
        if (!p) return '';
        return this.languageService.language === 'ar' && p.titleAr ? p.titleAr : p.titleEn;
    }

    getSummary(): string {
        const p = this.project();
        if (!p) return '';
        return this.languageService.language === 'ar' && p.summaryAr ? p.summaryAr : (p.summaryEn || '');
    }

    getDescription(): string {
        const p = this.project();
        if (!p) return '';
        return this.languageService.language === 'ar' && p.descriptionAr ? p.descriptionAr : (p.descriptionEn || '');
    }

    getSafeDescription(): SafeHtml {
        return this.sanitizer.bypassSecurityTrustHtml(this.getDescription());
    }

    getCategorySeverity(): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const p = this.project();
        if (!p) return 'secondary';
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectCategory.Robotics]: 'info',
            [ProjectCategory.CNC]: 'warn',
            [ProjectCategory.Embedded]: 'success',
            [ProjectCategory.Monitoring]: 'contrast',
            [ProjectCategory.Software]: 'secondary',
            [ProjectCategory.RnD]: 'danger'
        };
        return map[p.category] || 'secondary';
    }

    getStatusSeverity(): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" | undefined {
        const p = this.project();
        if (!p) return 'secondary';
        const map: Record<number, "success" | "secondary" | "info" | "warn" | "danger" | "contrast"> = {
            [ProjectStatus.Concept]: 'warn',
            [ProjectStatus.Prototype]: 'info',
            [ProjectStatus.Delivered]: 'success'
        };
        return map[p.status] || 'secondary';
    }
}
