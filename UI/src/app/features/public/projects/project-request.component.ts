import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FileUploadModule } from 'primeng/fileupload';
import { MultiSelectModule } from 'primeng/multiselect';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { LanguageService } from '../../../shared/services/language.service';
import { ProjectsApiService } from '../../../shared/services/projects-api.service';
import { AuthService } from '../../../shared/services/auth.service';
import {
    CreateProjectRequestPayload,
    ProjectCategory,
    ProjectListItem,
    ProjectRequestAttachment,
    ProjectRequestCapability,
    ProjectRequestMainDomain,
    ProjectRequestProjectType,
    ProjectRequestStage,
    ProjectRequestType
} from '../../../shared/models/project.model';

@Component({
    selector: 'app-project-request',
    standalone: true,
    imports: [
        CommonModule, RouterModule, FormsModule, ReactiveFormsModule, TranslateModule,
        ButtonModule, CardModule, InputTextModule, TextareaModule, SelectModule,
        SelectButtonModule, FileUploadModule, MultiSelectModule, ToastModule, AutoCompleteModule
    ],
    providers: [MessageService],
    template: `
        <div class="request-page" [dir]="languageService.direction">
            <p-toast></p-toast>
            <section class="hero-section">
                <div class="hero-bg"></div>
                <div class="hero-content">
                    <span class="hero-badge"><i class="pi pi-send"></i>{{ 'projects.request.badge' | translate }}</span>
                    <h1 class="hero-title">{{ 'projects.request.title' | translate }}</h1>
                    <p class="hero-description">{{ 'projects.request.description' | translate }}</p>
                </div>
            </section>
            <div class="form-wrapper">
                <div class="container">
                    @if (submitted()) {
                        <div class="success-card">
                            <div class="success-icon"><i class="pi pi-check-circle"></i></div>
                            <h2>{{ 'projects.request.success.title' | translate }}</h2>
                            <p>{{ 'projects.request.success.description' | translate }}</p>
                            <div class="reference-number">
                                <span class="label">{{ 'projects.request.success.referenceNumber' | translate }}</span>
                                <span class="number">{{ referenceNumber() }}</span>
                            </div>
                            <p class="follow-up">{{ 'projects.request.success.followUp' | translate }}</p>
                            <p-button [label]="'menu.projects' | translate" icon="pi pi-arrow-left" routerLink="/projects" styleClass="back-btn"></p-button>
                        </div>
                    } @else {
                        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="request-form">
                            <div class="form-section">
                                <h3>{{ 'projects.request.requestType' | translate }}</h3>
                                <p-selectButton [options]="projectTypeOptions" formControlName="projectType" optionLabel="label" optionValue="value" styleClass="request-type-toggle"></p-selectButton>
                            </div>

                            @if (form.get('projectType')?.value === projectTypeEnum.BasedOnExistingProduct) {
                                <div class="form-section">
                                    <h3>{{ 'projects.request.selectProject' | translate }}</h3>
                                    <p-autoComplete formControlName="projectId" [suggestions]="filteredProjects()" (completeMethod)="searchProjects($event)" field="titleEn" [dropdown]="true" [placeholder]="'projects.request.searchProject' | translate" styleClass="w-full">
                                        <ng-template let-project pTemplate="item">
                                            <div class="project-suggestion">
                                                <span class="project-title">{{ getProjectTitle(project) }}</span>
                                                <span class="project-year">{{ project.year }}</span>
                                            </div>
                                        </ng-template>
                                    </p-autoComplete>
                                </div>
                            }

                            <div class="form-section">
                                <h3>{{ 'projects.request.scopeSection' | translate }}</h3>
                                <div class="form-grid">
                                    <div class="form-field">
                                        <label>{{ 'projects.request.mainDomain' | translate }}</label>
                                        <p-select formControlName="mainDomain" [options]="mainDomainOptions" optionLabel="label" optionValue="value" [placeholder]="'projects.request.selectMainDomain' | translate" styleClass="w-full"></p-select>
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'projects.request.projectStage' | translate }}</label>
                                        <p-select formControlName="projectStage" [options]="projectStageOptions" optionLabel="label" optionValue="value" [placeholder]="'projects.request.selectProjectStage' | translate" styleClass="w-full"></p-select>
                                    </div>
                                </div>
                                <div class="form-field mt-3">
                                    <label>{{ 'projects.request.requiredCapabilities' | translate }}</label>
                                    <p-multiselect formControlName="requiredCapabilities" [options]="capabilityOptions" optionLabel="label" optionValue="value" display="chip" [showToggleAll]="false" [maxSelectedLabels]="3" [placeholder]="'projects.request.selectRequiredCapabilities' | translate" styleClass="w-full"></p-multiselect>
                                </div>
                            </div>

                            <div class="form-section">
                                <h3>{{ 'projects.request.budgetTimeline' | translate }}</h3>
                                <div class="form-grid">
                                    <div class="form-field">
                                        <label>{{ 'projects.request.budgetRange' | translate }}</label>
                                        <p-select formControlName="budgetRange" [options]="budgetOptions" optionLabel="label" optionValue="value" [placeholder]="'projects.request.selectBudget' | translate" styleClass="w-full"></p-select>
                                    </div>
                                    <div class="form-field">
                                        <label>{{ 'projects.request.timeline' | translate }}</label>
                                        <p-select formControlName="timeline" [options]="timelineOptions" optionLabel="label" optionValue="value" [placeholder]="'projects.request.selectTimeline' | translate" styleClass="w-full"></p-select>
                                    </div>
                                </div>
                            </div>

                            <div class="form-section">
                                <h3>{{ 'projects.request.projectDescription' | translate }}</h3>
                                <textarea pTextarea formControlName="description" rows="5" [placeholder]="'projects.request.descriptionPlaceholder' | translate" class="w-full"></textarea>
                            </div>

                            <div class="form-section">
                                <h3>{{ 'projects.request.attachments' | translate }}</h3>
                                <p-fileUpload mode="advanced" [multiple]="true" [customUpload]="true" [showUploadButton]="true" [showCancelButton]="false" [auto]="false" chooseIcon="pi pi-paperclip" uploadIcon="pi pi-upload" [chooseLabel]="'projects.request.chooseAttachments' | translate" [uploadLabel]="'projects.request.uploadAttachments' | translate" [accept]="acceptedAttachmentTypes" [maxFileSize]="25000000" (uploadHandler)="uploadAttachments($event)"></p-fileUpload>
                                @if (uploadedAttachments().length) {
                                    <div class="attachment-list">
                                        @for (attachment of uploadedAttachments(); track attachment.url) {
                                            <div class="attachment-item">
                                                <div><strong>{{ attachment.fileName }}</strong><small>{{ attachment.url }}</small></div>
                                                <p-button icon="pi pi-times" [text]="true" severity="secondary" (onClick)="removeAttachment(attachment.url)"></p-button>
                                            </div>
                                        }
                                    </div>
                                }
                            </div>

                            @if (!isLoggedIn()) {
                                <div class="form-section">
                                    <h3>{{ 'projects.request.contactInfo' | translate }}</h3>
                                    <div class="form-grid">
                                        <div class="form-field">
                                            <label>{{ 'projects.request.fullName' | translate }} *</label>
                                            <input pInputText formControlName="fullName" class="w-full" />
                                            @if (form.get('fullName')?.invalid && form.get('fullName')?.touched) { <small class="error">{{ 'validation.required' | translate }}</small> }
                                        </div>
                                        <div class="form-field">
                                            <label>{{ 'projects.request.email' | translate }} *</label>
                                            <input pInputText type="email" formControlName="email" class="w-full" />
                                            @if (form.get('email')?.invalid && form.get('email')?.touched) { <small class="error">{{ 'validation.invalidEmail' | translate }}</small> }
                                        </div>
                                        <div class="form-field">
                                            <label>{{ 'projects.request.phone' | translate }}</label>
                                            <input pInputText formControlName="phone" class="w-full" />
                                        </div>
                                    </div>
                                </div>
                            } @else {
                                <div class="form-section">
                                    <h3>{{ 'projects.request.contactInfo' | translate }}</h3>
                                    <p class="account-info-note">{{ 'projects.request.loggedInNote' | translate }}</p>
                                </div>
                            }

                            <div class="form-actions">
                                <p-button type="submit" [label]="'projects.request.submit' | translate" icon="pi pi-send" [loading]="submitting()" [disabled]="form.invalid || uploadingAttachments()" styleClass="submit-btn"></p-button>
                            </div>
                        </form>
                    }
                </div>
            </div>
        </div>
    `,
    styles: [`
        :host { --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%); --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%); --color-primary: #667eea; }
        .request-page { min-height: 100vh; background: #fafbfc; } .container { max-width: 800px; margin: 0 auto; padding: 0 1.5rem; }
        .hero-section { position: relative; padding: 4rem 2rem 3rem; text-align: center; } .hero-bg { position: absolute; inset: 0; background: var(--gradient-dark); } .hero-content { position: relative; z-index: 10; }
        .hero-badge { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: rgba(255,255,255,0.15); border-radius: 50px; font-size: 0.875rem; font-weight: 600; color: white; margin-bottom: 1rem; }
        .hero-title { font-size: clamp(1.75rem, 4vw, 2.5rem); font-weight: 800; color: white; margin-bottom: 0.75rem; } .hero-description { color: rgba(255,255,255,0.9); max-width: 500px; margin: 0 auto; }
        .form-wrapper { padding: 3rem 0; margin-top: -2rem; } .request-form { background: white; border-radius: 20px; padding: 2.5rem; box-shadow: 0 10px 40px rgba(0,0,0,0.1); }
        .form-section { margin-bottom: 2rem; } .form-section h3 { font-size: 1.1rem; font-weight: 700; color: #1a1a2e; margin-bottom: 1rem; }
        :host ::ng-deep .request-type-toggle { display: flex; width: 100%; flex-wrap: wrap; } :host ::ng-deep .request-type-toggle .p-button { flex: 1 1 12rem; justify-content: center; }
        .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; } .form-field { display: flex; flex-direction: column; gap: 0.5rem; } .form-field label { font-size: 0.875rem; font-weight: 600; color: #374151; }
        .error { color: #ef4444; font-size: 0.75rem; } .account-info-note { margin: 0; color: #475569; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px; padding: 0.65rem 0.85rem; font-size: 0.92rem; }
        .attachment-list { display: grid; gap: 0.75rem; margin-top: 1rem; } .attachment-item { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; border: 1px solid #e2e8f0; border-radius: 12px; padding: 0.75rem 0.9rem; background: #fff; }
        .attachment-item strong, .attachment-item small { display: block; } .attachment-item small { color: #64748b; margin-top: 0.25rem; word-break: break-all; }
        .form-actions { text-align: center; padding-top: 1rem; } :host ::ng-deep .submit-btn { background: var(--gradient-primary) !important; border: none !important; padding: 0.875rem 2.5rem !important; font-weight: 600 !important; border-radius: 12px !important; }
        .success-card { background: white; border-radius: 20px; padding: 3rem; box-shadow: 0 10px 40px rgba(0,0,0,0.1); text-align: center; } .success-icon { width: 80px; height: 80px; margin: 0 auto 1.5rem; background: var(--gradient-primary); border-radius: 50%; display: flex; align-items: center; justify-content: center; } .success-icon i { font-size: 2.5rem; color: white; }
        .success-card h2 { font-size: 1.5rem; font-weight: 700; color: #1a1a2e; margin-bottom: 0.5rem; } .success-card > p { color: #6b7280; margin-bottom: 1.5rem; } .reference-number { background: #f3f4f6; border-radius: 12px; padding: 1rem; margin-bottom: 1.5rem; }
        .reference-number .label { display: block; font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem; } .reference-number .number { font-size: 1.5rem; font-weight: 700; color: var(--color-primary); } .follow-up { font-size: 0.9rem; color: #6b7280; margin-bottom: 1.5rem; }
        :host ::ng-deep .back-btn { background: var(--gradient-primary) !important; border: none !important; } .project-suggestion { display: flex; justify-content: space-between; align-items: center; width: 100%; padding: 0.5rem 0; } .project-title { font-weight: 600; } .project-year { color: #6b7280; font-size: 0.875rem; }
    `]
})
export class ProjectRequestComponent implements OnInit {
    readonly projectTypeEnum = ProjectRequestProjectType;
    readonly mainDomainEnum = ProjectRequestMainDomain;
    readonly projectStageEnum = ProjectRequestStage;
    languageService = inject(LanguageService);
    private projectsApi = inject(ProjectsApiService);
    private route = inject(ActivatedRoute);
    private fb = inject(FormBuilder);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private authService = inject(AuthService);
    form!: FormGroup;
    submitted = signal(false);
    submitting = signal(false);
    uploadingAttachments = signal(false);
    referenceNumber = signal('');
    allProjects = signal<ProjectListItem[]>([]);
    filteredProjects = signal<ProjectListItem[]>([]);
    uploadedAttachments = signal<ProjectRequestAttachment[]>([]);
    acceptedAttachmentTypes = '.pdf,.png,.jpg,.jpeg,.webp,.svg,.step,.stp,.stl,.obj,.dxf,.dwg,.zip,.rar,.7z';
    projectTypeOptions: { label: string; value: ProjectRequestProjectType }[] = [];
    mainDomainOptions: { label: string; value: ProjectRequestMainDomain }[] = [];
    capabilityOptions: { label: string; value: ProjectRequestCapability }[] = [];
    budgetOptions: { label: string; value: string }[] = [];
    timelineOptions: { label: string; value: string }[] = [];
    projectStageOptions: { label: string; value: ProjectRequestStage }[] = [];

    ngOnInit() {
        const typeParam = this.route.snapshot.queryParamMap.get('type');
        const projectParam = this.route.snapshot.queryParamMap.get('project');
        const initialProjectType = typeParam === 'similar' ? ProjectRequestProjectType.BasedOnExistingProduct : ProjectRequestProjectType.CustomProject;
        const isLogged = this.isLoggedIn();
        this.form = this.fb.group({
            projectType: [initialProjectType, Validators.required],
            projectId: [null],
            mainDomain: [null],
            requiredCapabilities: [[]],
            budgetRange: [null],
            timeline: [null],
            projectStage: [null],
            description: [''],
            fullName: ['', isLogged ? [] : [Validators.required]],
            email: ['', isLogged ? [] : [Validators.required, Validators.email]],
            phone: ['']
        });
        if (isLogged) {
            this.form.patchValue({ fullName: this.authService.user?.fullName || '', email: this.authService.user?.email || '', phone: this.authService.user?.phone || '' });
        }
        this.buildOptions();
        this.translate.onLangChange.subscribe(() => this.buildOptions());
        this.projectsApi.getProjects({ pageSize: 100 }).subscribe({
            next: (response) => {
                this.allProjects.set(response.items);
                if (projectParam) {
                    const selectedProject = response.items.find(p => p.id === projectParam);
                    if (selectedProject) this.form.patchValue({ projectId: selectedProject });
                }
            }
        });
    }

    private buildOptions(): void {
        this.projectTypeOptions = [
            { value: ProjectRequestProjectType.CustomProject, label: this.translate.instant('projects.request.projectTypeOptions.CustomProject') },
            { value: ProjectRequestProjectType.BasedOnExistingProduct, label: this.translate.instant('projects.request.projectTypeOptions.BasedOnExistingProduct') },
            { value: ProjectRequestProjectType.UpgradeModification, label: this.translate.instant('projects.request.projectTypeOptions.UpgradeModification') },
            { value: ProjectRequestProjectType.ReverseEngineeringRepair, label: this.translate.instant('projects.request.projectTypeOptions.ReverseEngineeringRepair') }
        ];
        this.mainDomainOptions = [
            { value: ProjectRequestMainDomain.Robotics, label: this.translate.instant('projects.request.mainDomainOptions.Robotics') },
            { value: ProjectRequestMainDomain.EmbeddedSystems, label: this.translate.instant('projects.request.mainDomainOptions.EmbeddedSystems') },
            { value: ProjectRequestMainDomain.MedicalDevices, label: this.translate.instant('projects.request.mainDomainOptions.MedicalDevices') },
            { value: ProjectRequestMainDomain.IoTSystems, label: this.translate.instant('projects.request.mainDomainOptions.IoTSystems') },
            { value: ProjectRequestMainDomain.IndustrialAutomation, label: this.translate.instant('projects.request.mainDomainOptions.IndustrialAutomation') },
            { value: ProjectRequestMainDomain.MechanicalSystem, label: this.translate.instant('projects.request.mainDomainOptions.MechanicalSystem') },
            { value: ProjectRequestMainDomain.SoftwareSystem, label: this.translate.instant('projects.request.mainDomainOptions.SoftwareSystem') },
            { value: ProjectRequestMainDomain.MixedSystem, label: this.translate.instant('projects.request.mainDomainOptions.MixedSystem') }
        ];
        this.capabilityOptions = [
            { value: ProjectRequestCapability.CadDesign, label: this.translate.instant('projects.request.capabilityOptions.CadDesign') },
            { value: ProjectRequestCapability.Modeling3d, label: this.translate.instant('projects.request.capabilityOptions.Modeling3d') },
            { value: ProjectRequestCapability.Printing3d, label: this.translate.instant('projects.request.capabilityOptions.Printing3d') },
            { value: ProjectRequestCapability.CncMachining, label: this.translate.instant('projects.request.capabilityOptions.CncMachining') },
            { value: ProjectRequestCapability.LaserCutting, label: this.translate.instant('projects.request.capabilityOptions.LaserCutting') },
            { value: ProjectRequestCapability.LaserEngraving, label: this.translate.instant('projects.request.capabilityOptions.LaserEngraving') },
            { value: ProjectRequestCapability.PcbDesign, label: this.translate.instant('projects.request.capabilityOptions.PcbDesign') },
            { value: ProjectRequestCapability.EmbeddedProgramming, label: this.translate.instant('projects.request.capabilityOptions.EmbeddedProgramming') },
            { value: ProjectRequestCapability.ElectronicsIntegration, label: this.translate.instant('projects.request.capabilityOptions.ElectronicsIntegration') },
            { value: ProjectRequestCapability.MechanicalAssembly, label: this.translate.instant('projects.request.capabilityOptions.MechanicalAssembly') },
            { value: ProjectRequestCapability.Prototyping, label: this.translate.instant('projects.request.capabilityOptions.Prototyping') },
            { value: ProjectRequestCapability.TestingValidation, label: this.translate.instant('projects.request.capabilityOptions.TestingValidation') },
            { value: ProjectRequestCapability.SoftwareDevelopment, label: this.translate.instant('projects.request.capabilityOptions.SoftwareDevelopment') },
            { value: ProjectRequestCapability.UiControlPanelDevelopment, label: this.translate.instant('projects.request.capabilityOptions.UiControlPanelDevelopment') },
            { value: ProjectRequestCapability.ReverseEngineering, label: this.translate.instant('projects.request.capabilityOptions.ReverseEngineering') }
        ];
        this.projectStageOptions = [
            { value: ProjectRequestStage.Idea, label: this.translate.instant('projects.request.projectStageOptions.Idea') },
            { value: ProjectRequestStage.ConceptReady, label: this.translate.instant('projects.request.projectStageOptions.ConceptReady') },
            { value: ProjectRequestStage.PrototypeExists, label: this.translate.instant('projects.request.projectStageOptions.PrototypeExists') },
            { value: ProjectRequestStage.NeedsManufacturing, label: this.translate.instant('projects.request.projectStageOptions.NeedsManufacturing') },
            { value: ProjectRequestStage.NeedsImprovement, label: this.translate.instant('projects.request.projectStageOptions.NeedsImprovement') }
        ];
        this.budgetOptions = [
            { value: 'Under100Ils', label: this.translate.instant('projects.request.budgetOptions.Under100Ils') },
            { value: 'Between100And500Ils', label: this.translate.instant('projects.request.budgetOptions.Between100And500Ils') },
            { value: 'Between500And1000Ils', label: this.translate.instant('projects.request.budgetOptions.Between500And1000Ils') },
            { value: 'Between1000And5000Ils', label: this.translate.instant('projects.request.budgetOptions.Between1000And5000Ils') },
            { value: 'Between5000And10000Ils', label: this.translate.instant('projects.request.budgetOptions.Between5000And10000Ils') },
            { value: 'Over10000Ils', label: this.translate.instant('projects.request.budgetOptions.Over10000Ils') },
            { value: 'NotSureYet', label: this.translate.instant('projects.request.budgetOptions.NotSureYet') }
        ];
        this.timelineOptions = [
            { value: 'AsapUrgent', label: this.translate.instant('projects.request.timelineOptions.AsapUrgent') },
            { value: 'OneToTwoWeeks', label: this.translate.instant('projects.request.timelineOptions.OneToTwoWeeks') },
            { value: 'TwoToFourWeeks', label: this.translate.instant('projects.request.timelineOptions.TwoToFourWeeks') },
            { value: 'OneToTwoMonths', label: this.translate.instant('projects.request.timelineOptions.OneToTwoMonths') },
            { value: 'TwoToSixMonths', label: this.translate.instant('projects.request.timelineOptions.TwoToSixMonths') },
            { value: 'Flexible', label: this.translate.instant('projects.request.timelineOptions.Flexible') }
        ];
    }

    searchProjects(event: { query: string }) {
        const query = event.query.toLowerCase();
        this.filteredProjects.set(this.allProjects().filter(p => p.titleEn.toLowerCase().includes(query) || (p.titleAr && p.titleAr.includes(query))));
    }

    uploadAttachments(event: { files: File[] }) {
        const files = event.files ?? [];
        if (!files.length) return;
        this.uploadingAttachments.set(true);
        forkJoin(files.map(file => this.projectsApi.uploadAttachment(file))).subscribe({
            next: (uploaded) => {
                this.uploadedAttachments.update(existing => [...existing, ...uploaded.map(x => ({ url: x.url, fileName: x.fileName }))]);
                this.uploadingAttachments.set(false);
            },
            error: () => {
                this.uploadingAttachments.set(false);
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: this.translate.instant('projects.request.attachmentUploadFailed') });
            }
        });
    }

    removeAttachment(url: string) { this.uploadedAttachments.update(existing => existing.filter(x => x.url !== url)); }
    getProjectTitle(project: ProjectListItem): string { return this.languageService.language === 'ar' && project.titleAr ? project.titleAr : project.titleEn; }

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.submitting.set(true);
        const formValue = this.form.getRawValue();
        const requestType = formValue.projectType === ProjectRequestProjectType.BasedOnExistingProduct ? ProjectRequestType.SimilarToExisting : ProjectRequestType.Custom;
        const attachments = this.uploadedAttachments();
        const payload: CreateProjectRequestPayload = {
            fullName: this.authService.user?.fullName || formValue.fullName,
            email: this.authService.user?.email || formValue.email,
            phone: this.authService.user?.phone || formValue.phone || undefined,
            requestType,
            projectId: requestType === ProjectRequestType.SimilarToExisting && formValue.projectId ? formValue.projectId.id : undefined,
            projectType: formValue.projectType || undefined,
            mainDomain: formValue.mainDomain || undefined,
            requiredCapabilities: (formValue.requiredCapabilities ?? []).length ? formValue.requiredCapabilities : undefined,
            category: this.mapDomainToLegacyCategory(formValue.mainDomain),
            budgetRange: formValue.budgetRange || undefined,
            timeline: formValue.timeline || undefined,
            description: formValue.description || undefined,
            projectDescription: formValue.description || undefined,
            projectStage: formValue.projectStage || undefined,
            attachmentUrl: attachments[0]?.url,
            attachmentFileName: attachments[0]?.fileName,
            attachments: attachments.length ? attachments : undefined
        };
        this.projectsApi.submitProjectRequest(payload).subscribe({
            next: (response) => { this.referenceNumber.set(response.referenceNumber); this.submitted.set(true); this.submitting.set(false); },
            error: (err) => {
                this.submitting.set(false);
                this.messageService.add({ severity: 'error', summary: this.translate.instant('messages.error'), detail: err.error?.message || this.translate.instant('messages.genericError') });
            }
        });
    }

    private mapDomainToLegacyCategory(mainDomain: ProjectRequestMainDomain | null | undefined): ProjectCategory | undefined {
        switch (mainDomain) {
            case ProjectRequestMainDomain.Robotics: return ProjectCategory.Robotics;
            case ProjectRequestMainDomain.EmbeddedSystems: return ProjectCategory.Embedded;
            case ProjectRequestMainDomain.IoTSystems: return ProjectCategory.Monitoring;
            case ProjectRequestMainDomain.IndustrialAutomation: return ProjectCategory.Automation;
            case ProjectRequestMainDomain.MechanicalSystem: return ProjectCategory.Mechanical;
            case ProjectRequestMainDomain.SoftwareSystem: return ProjectCategory.Software;
            case ProjectRequestMainDomain.MixedSystem: return ProjectCategory.Custom;
            case ProjectRequestMainDomain.MedicalDevices: return ProjectCategory.Electronics;
            default: return undefined;
        }
    }

    isLoggedIn(): boolean { return this.authService.authenticated; }
}
