import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { LanguageService } from '../../../shared/services/language.service';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { ProjectListItem, ProjectCategory, ProjectRequestType, CreateProjectRequestPayload } from '../../../shared/models/project.model';

@Component({
    selector: 'app-project-request',
    standalone: true,
    imports: [
        CommonModule, RouterModule, FormsModule, ReactiveFormsModule, TranslateModule,
        ButtonModule, CardModule, InputTextModule, TextareaModule, SelectModule,
        SelectButtonModule, FileUploadModule, ToastModule, AutoCompleteModule
    ],
    providers: [MessageService],
    template: `
        <div class="request-page" [dir]="languageService.direction">
            <p-toast></p-toast>
            
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-send"></i>
                        {{ 'projects.request.badge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'projects.request.title' | translate }}</h1>
                    <p class="hero-description">{{ 'projects.request.description' | translate }}</p>
                </div>
            </section>

            <div class="form-wrapper">
                <div class="container">
                    @if (submitted()) {
                        <div class="success-card">
                            <div class="success-icon">
                                <i class="pi pi-check-circle"></i>
                            </div>
                            <h2>{{ 'projects.request.success.title' | translate }}</h2>
                            <p>{{ 'projects.request.success.description' | translate }}</p>
                            <div class="reference-number">
                                <span class="label">{{ 'projects.request.success.referenceNumber' | translate }}</span>
                                <span class="number">{{ referenceNumber() }}</span>
                            </div>
                            <p class="follow-up">{{ 'projects.request.success.followUp' | translate }}</p>
                            <p-button
                                [label]="'menu.projects' | translate"
                                icon="pi pi-arrow-left"
                                routerLink="/projects"
                                styleClass="back-btn">
                            </p-button>
                        </div>
                    } @else {
                        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="request-form">
                            <!-- Request Type Toggle -->
                            <div class="form-section">
                                <h3>{{ 'projects.request.requestType' | translate }}</h3>
                                <p-selectButton 
                                    [options]="requestTypeOptions" 
                                    formControlName="requestType"
                                    optionLabel="label"
                                    optionValue="value"
                                    styleClass="request-type-toggle">
                                </p-selectButton>
                            </div>

                            <!-- Similar Project Selection -->
                            @if (form.get('requestType')?.value === 0) {
                                <div class="form-section">
                                    <h3>{{ 'projects.request.selectProject' | translate }}</h3>
                                    <p-autoComplete
                                        formControlName="projectId"
                                        [suggestions]="filteredProjects()"
                                        (completeMethod)="searchProjects($event)"
                                        field="titleEn"
                                        [dropdown]="true"
                                        [placeholder]="'projects.request.searchProject' | translate"
                                        styleClass="w-full">
                                        <ng-template let-project pTemplate="item">
                                            <div class="project-suggestion">
                                                <span class="project-title">{{ getProjectTitle(project) }}</span>
                                                <span class="project-year">{{ project.year }}</span>
                                            </div>
                                        </ng-template>
                                    </p-autoComplete>
                                </div>
                            }

                            <!-- Category (for Custom) -->
                            @if (form.get('requestType')?.value === 1) {
                                <div class="form-section">
                                    <h3>{{ 'projects.request.category' | translate }}</h3>
                                    <p-select
                                        formControlName="category"
                                        [options]="categoryOptions"
                                        optionLabel="label"
                                        optionValue="value"
                                        [placeholder]="'projects.request.selectCategory' | translate"
                                        styleClass="w-full">
                                    </p-select>
                                </div>
                            }

                            <!-- Budget & Timeline -->
                            <div class="form-section">
                                <h3>{{ 'projects.request.budgetTimeline' | translate }}</h3>
                                <div class="form-grid">
                                    <div class="form-field">
                                        <label>{{ 'projects.request.budgetRange' | translate }}</label>
                                        <p-select
                                            formControlName="budgetRange"
                                            [options]="budgetOptions"
                                            optionLabel="label"
                                            optionValue="value"
                                            [placeholder]="'projects.request.selectBudget' | translate"
                                            styleClass="w-full">
                                        </p-select>
                                        @if (form.get('budgetRange')?.value === 'custom') {
                                            <input
                                                type="text"
                                                pInputText
                                                formControlName="customBudgetText"
                                                [placeholder]="'projects.request.customBudgetPlaceholder' | translate"
                                                class="w-full mt-2">
                                        }
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'projects.request.timeline' | translate }}</label>
                                        <p-select
                                            formControlName="timeline"
                                            [options]="timelineOptions"
                                            optionLabel="label"
                                            optionValue="value"
                                            [placeholder]="'projects.request.selectTimeline' | translate"
                                            styleClass="w-full">
                                        </p-select>
                                    </div>
                                </div>
                            </div>

                            <!-- Description -->
                            <div class="form-section">
                                <h3>{{ 'projects.request.projectDescription' | translate }}</h3>
                                <textarea 
                                    pTextarea 
                                    formControlName="description"
                                    rows="5"
                                    [placeholder]="'projects.request.descriptionPlaceholder' | translate"
                                    class="w-full">
                                </textarea>
                            </div>

                            <!-- Contact Information -->
                            <div class="form-section">
                                <h3>{{ 'projects.request.contactInfo' | translate }}</h3>
                                <div class="form-grid">
                                    <div class="form-field">
                                        <label>{{ 'projects.request.fullName' | translate }} *</label>
                                        <input pInputText formControlName="fullName" class="w-full" />
                                        @if (form.get('fullName')?.invalid && form.get('fullName')?.touched) {
                                            <small class="error">{{ 'validation.required' | translate }}</small>
                                        }
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'projects.request.email' | translate }} *</label>
                                        <input pInputText type="email" formControlName="email" class="w-full" />
                                        @if (form.get('email')?.invalid && form.get('email')?.touched) {
                                            <small class="error">{{ 'validation.invalidEmail' | translate }}</small>
                                        }
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'projects.request.phone' | translate }}</label>
                                        <input pInputText formControlName="phone" class="w-full" />
                                    </div>
                                </div>
                            </div>

                            <!-- Submit Button -->
                            <div class="form-actions">
                                <p-button
                                    type="submit"
                                    [label]="'projects.request.submit' | translate"
                                    icon="pi pi-send"
                                    [loading]="submitting()"
                                    [disabled]="form.invalid"
                                    styleClass="submit-btn">
                                </p-button>
                            </div>
                        </form>
                    }
                </div>
            </div>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --color-primary: #667eea;
        }

        .request-page { min-height: 100vh; background: #fafbfc; }
        .container { max-width: 800px; margin: 0 auto; padding: 0 1.5rem; }

        /* Hero */
        .hero-section { position: relative; padding: 4rem 2rem 3rem; text-align: center; }
        .hero-bg { position: absolute; inset: 0; background: var(--gradient-dark); }
        .hero-content { position: relative; z-index: 10; }
        .hero-badge { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: rgba(255,255,255,0.15); border-radius: 50px; font-size: 0.875rem; font-weight: 600; color: white; margin-bottom: 1rem; }
        .hero-title { font-size: clamp(1.75rem, 4vw, 2.5rem); font-weight: 800; color: white; margin-bottom: 0.75rem; }
        .hero-description { color: rgba(255,255,255,0.9); max-width: 500px; margin: 0 auto; }

        /* Form Wrapper */
        .form-wrapper { padding: 3rem 0; margin-top: -2rem; }
        .request-form { background: white; border-radius: 20px; padding: 2.5rem; box-shadow: 0 10px 40px rgba(0,0,0,0.1); }

        .form-section { margin-bottom: 2rem; }
        .form-section h3 { font-size: 1.1rem; font-weight: 700; color: #1a1a2e; margin-bottom: 1rem; }

        :host ::ng-deep .request-type-toggle { display: flex; width: 100%; }
        :host ::ng-deep .request-type-toggle .p-button { flex: 1; justify-content: center; }

        .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; }
        .form-field { display: flex; flex-direction: column; gap: 0.5rem; }
        .form-field label { font-size: 0.875rem; font-weight: 600; color: #374151; }
        .error { color: #ef4444; font-size: 0.75rem; }

        .form-actions { text-align: center; padding-top: 1rem; }
        :host ::ng-deep .submit-btn { background: var(--gradient-primary) !important; border: none !important; padding: 0.875rem 2.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }

        /* Success Card */
        .success-card { background: white; border-radius: 20px; padding: 3rem; box-shadow: 0 10px 40px rgba(0,0,0,0.1); text-align: center; }
        .success-icon { width: 80px; height: 80px; margin: 0 auto 1.5rem; background: var(--gradient-primary); border-radius: 50%; display: flex; align-items: center; justify-content: center; }
        .success-icon i { font-size: 2.5rem; color: white; }
        .success-card h2 { font-size: 1.5rem; font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; }
        .success-card > p { color: #6b7280; margin-bottom: 1.5rem; }
        .reference-number { background: #f3f4f6; border-radius: 12px; padding: 1rem; margin-bottom: 1.5rem; }
        .reference-number .label { display: block; font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem; }
        .reference-number .number { font-size: 1.5rem; font-weight: 700; color: var(--color-primary); }
        .follow-up { font-size: 0.9rem; color: #6b7280; margin-bottom: 1.5rem; }
        :host ::ng-deep .back-btn { background: var(--gradient-primary) !important; border: none !important; }

        .project-suggestion { display: flex; justify-content: space-between; align-items: center; width: 100%; padding: 0.5rem 0; }
        .project-title { font-weight: 600; }
        .project-year { color: #6b7280; font-size: 0.875rem; }
    `]
})
export class ProjectRequestComponent implements OnInit {
    languageService = inject(LanguageService);
    private projectsApi = inject(ProjectsApiService);
    private route = inject(ActivatedRoute);
    private fb = inject(FormBuilder);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    form!: FormGroup;
    submitted = signal(false);
    submitting = signal(false);
    referenceNumber = signal('');
    
    allProjects = signal<ProjectListItem[]>([]);
    filteredProjects = signal<ProjectListItem[]>([]);

    requestTypeOptions = [
        { label: 'Similar to Existing', value: ProjectRequestType.SimilarToExisting },
        { label: 'Custom Project', value: ProjectRequestType.Custom }
    ];

    categoryOptions = [
        { label: 'Robotics', value: ProjectCategory.Robotics },
        { label: 'CNC', value: ProjectCategory.CNC },
        { label: 'Embedded', value: ProjectCategory.Embedded },
        { label: 'Monitoring', value: ProjectCategory.Monitoring },
        { label: 'Software', value: ProjectCategory.Software },
        { label: 'R&D', value: ProjectCategory.RnD },
        { label: 'Electronics', value: ProjectCategory.Electronics },
        { label: 'Mechanical', value: ProjectCategory.Mechanical },
        { label: 'Automation', value: ProjectCategory.Automation }
    ];

    budgetOptions = [
        { label: '< ₪1,000', value: 'under-1k' },
        { label: '₪1,000 - ₪5,000', value: '1k-5k' },
        { label: '₪5,000 - ₪10,000', value: '5k-10k' },
        { label: '₪10,000 - ₪25,000', value: '10k-25k' },
        { label: '₪25,000+', value: '25k-plus' },
        { label: 'Not Sure', value: 'not-sure' },
        { label: 'Other / Custom', value: 'custom' }
    ];

    timelineOptions = [
        { label: '< 1 Month', value: 'under-1-month' },
        { label: '1-3 Months', value: '1-3-months' },
        { label: '3-6 Months', value: '3-6-months' },
        { label: '6+ Months', value: '6-plus-months' },
        { label: 'Flexible', value: 'flexible' }
    ];

    ngOnInit() {
        const typeParam = this.route.snapshot.queryParamMap.get('type');
        const projectParam = this.route.snapshot.queryParamMap.get('project');

        const initialType = typeParam === 'similar' ? ProjectRequestType.SimilarToExisting : ProjectRequestType.Custom;

        this.form = this.fb.group({
            requestType: [initialType],
            projectId: [null],
            category: [null],
            budgetRange: [null],
            customBudgetText: [''],
            timeline: [null],
            description: [''],
            fullName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            phone: ['']
        });

        this.projectsApi.getProjects({ pageSize: 100 }).subscribe({
            next: (response) => {
                this.allProjects.set(response.items);
                if (projectParam) {
                    const selectedProject = response.items.find(p => p.id === projectParam);
                    if (selectedProject) {
                        this.form.patchValue({ projectId: selectedProject });
                    }
                }
            }
        });

        this.translate.get('projects.request.requestTypes.similar').subscribe(label => {
            this.requestTypeOptions[0].label = label;
        });
        this.translate.get('projects.request.requestTypes.custom').subscribe(label => {
            this.requestTypeOptions[1].label = label;
        });
    }

    searchProjects(event: { query: string }) {
        const query = event.query.toLowerCase();
        this.filteredProjects.set(
            this.allProjects().filter(p => 
                p.titleEn.toLowerCase().includes(query) ||
                (p.titleAr && p.titleAr.includes(query))
            )
        );
    }

    getProjectTitle(project: ProjectListItem): string {
        return this.languageService.language === 'ar' && project.titleAr ? project.titleAr : project.titleEn;
    }

    onSubmit() {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.submitting.set(true);

        const formValue = this.form.value;
        const budgetRange = formValue.budgetRange === 'custom' && formValue.customBudgetText
            ? formValue.customBudgetText
            : (formValue.budgetRange || undefined);
        const payload: CreateProjectRequestPayload = {
            fullName: formValue.fullName,
            email: formValue.email,
            phone: formValue.phone || undefined,
            requestType: formValue.requestType,
            projectId: formValue.requestType === ProjectRequestType.SimilarToExisting && formValue.projectId 
                ? formValue.projectId.id 
                : undefined,
            category: formValue.requestType === ProjectRequestType.Custom ? formValue.category : undefined,
            budgetRange: budgetRange || undefined,
            timeline: formValue.timeline || undefined,
            description: formValue.description || undefined
        };

        this.projectsApi.submitProjectRequest(payload).subscribe({
            next: (response) => {
                this.referenceNumber.set(response.referenceNumber);
                this.submitted.set(true);
                this.submitting.set(false);
            },
            error: (err) => {
                this.submitting.set(false);
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: err.error?.message || this.translate.instant('messages.genericError')
                });
            }
        });
    }
}
