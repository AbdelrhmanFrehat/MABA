import { Component, OnInit, inject, signal, computed, ChangeDetectorRef } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { combineLatest, map, startWith, switchMap } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { LanguageService } from '../../../shared/services/language.service';
import { AuthService } from '../../../shared/services/auth.service';
import { DesignCadRequestService } from '../../../shared/services/design-cad-request.service';
import type { CadRequestScenario, DesignCadTargetProcess, CreateDesignCadRequestPayload } from '../../../shared/models/design-cad-request.model';
import { SCENARIO_TO_REQUEST_TYPE } from '../../../shared/models/design-cad-request.model';

const DRAFT_KEY = 'designCadDraft';
const ACCEPTED_EXTENSIONS = '.png,.jpg,.jpeg,.webp,.pdf,.step,.stp,.iges,.igs,.sldprt,.sldasm,.stl,.obj,.dxf,.dwg,.zip';
const MAX_FILE_SIZE_MB = 50;
const MAX_FILES = 20;

/** Single source of truth for scenario metadata (title, icon, helper) used in Step 1, 2, and 3. */
const REQUEST_SCENARIO_META: Record<CadRequestScenario, { titleKey: string; icon: string; helperKey: string }> = {
    ideaOnly: { titleKey: 'designCad.new.types.IdeaOnly', icon: 'pi pi-lightbulb', helperKey: 'designCad.new.helperIdeaOnly' },
    existingCad: { titleKey: 'designCad.new.types.ExistingFiles', icon: 'pi pi-file-edit', helperKey: 'designCad.new.helperExistingCad' },
    reverseEngineering: { titleKey: 'designCad.new.types.ReverseEngineering', icon: 'pi pi-refresh', helperKey: 'designCad.new.helperReverseEngineering' },
    physicalObject: { titleKey: 'designCad.new.types.PhysicalItem', icon: 'pi pi-box', helperKey: 'designCad.new.helperPhysicalObject' },
    modifyProduct: { titleKey: 'designCad.new.types.ModifyProduct', icon: 'pi pi-wrench', helperKey: 'designCad.new.helperModifyProduct' },
    mechanicalAssembly: { titleKey: 'designCad.new.types.MechanicalAssembly', icon: 'pi pi-cog', helperKey: 'designCad.new.helperMechanicalAssembly' }
};

const REQUEST_SCENARIO_CARDS: { key: CadRequestScenario; icon: string; titleKey: string }[] = (
    Object.entries(REQUEST_SCENARIO_META) as [CadRequestScenario, { titleKey: string; icon: string; helperKey: string }][]
).map(([key, meta]) => ({ key, icon: meta.icon, titleKey: meta.titleKey }));

@Component({
    selector: 'app-design-cad-new',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        TranslateModule,
        ButtonModule,
        CardModule,
        StepsModule,
        InputTextModule,
        TextareaModule,
        SelectModule,
        CheckboxModule,
        ToastModule,
        MessageModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <section class="design-cad-request-page" [dir]="languageService.direction">
            <div class="request-page-bg"></div>
            <div class="request-page-grid" aria-hidden="true"></div>
            <div class="request-page-fade" aria-hidden="true"></div>

            <header class="request-page-header">
                <div class="request-header-inner">
                    <div class="request-breadcrumb request-breadcrumb-flex">
                        <a routerLink="/design-cad" class="request-breadcrumb-link">{{ 'designCad.hero.badge' | translate }}</a>
                        <i class="pi pi-chevron-right request-breadcrumb-sep"></i>
                        <span>{{ 'designCad.new.pageTitle' | translate }}</span>
                        @if (scenarioLabel()) {
                            <i class="pi pi-chevron-right request-breadcrumb-sep"></i>
                            <span class="request-breadcrumb-scenario">{{ scenarioLabel() }}</span>
                        }
                    </div>
                    <h1 class="request-page-title">{{ 'designCad.new.wizardTitle' | translate }}</h1>
                    <p class="request-page-subtitle">{{ 'designCad.new.wizardSubtitle' | translate }}</p>
                </div>
            </header>

            <div class="request-page-content">
            @if (submitted()) {
                <div class="request-container success-box">
                    <div class="success-icon"><i class="pi pi-check"></i></div>
                    <h2>{{ 'designCad.new.successTitle' | translate }}</h2>
                    <p class="ref-display">{{ 'designCad.new.reference' | translate }}: <strong>{{ referenceNumber() }}</strong></p>
                    <p-button [label]="'designCad.new.backToLanding' | translate" routerLink="/design-cad" icon="pi pi-arrow-left" iconPos="left" severity="primary" styleClass="success-back-btn"></p-button>
                </div>
            } @else {
                <div class="request-container request-card">
                    <div class="stepper-wrapper">
                        <p-steps [model]="stepItems()" [activeIndex]="currentStep()" [readonly]="true" styleClass="wizard-steps" />
                    </div>
                    <div class="form-wrap">
                    <!-- Step 1: Scenario cards -->
                    @if (currentStep() === 0) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'designCad.new.step1Title' | translate }}</h2>
                            <div class="type-grid type-grid-6">
                                @for (card of requestScenarioCards; track card.key) {
                                    <button type="button" class="type-option type-option-with-check" [class.selected]="selectedScenario() === card.key" (click)="selectScenario(card.key)">
                                        @if (selectedScenario() === card.key) {
                                            <div class="type-option-selected-check"><i class="pi pi-check"></i></div>
                                        }
                                        <i class="pi" [ngClass]="card.icon"></i>
                                        <span>{{ card.titleKey | translate }}</span>
                                    </button>
                                }
                            </div>
                            <div class="step-actions">
                                <p-button [label]="'designCad.new.next' | translate" icon="pi pi-arrow-right" iconPos="right" (onClick)="nextStep()" [disabled]="!selectedScenario()" severity="primary" styleClass="design-cad-step-btn"></p-button>
                            </div>
                        </div>
                    }

                    <!-- Step 2: One base form + conditional sections -->
                    @if (currentStep() === 1) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'designCad.new.step2Title' | translate }}</h2>
                            @if (selectedScenarioMeta) {
                                <div class="selected-request-type-block">
                                    <div class="selected-request-type-label">{{ 'designCad.new.selectedRequestTypeLabel' | translate }}</div>
                                    <div class="selected-request-type-inner">
                                        <div class="selected-request-type-icon"><i class="pi" [ngClass]="selectedScenarioMeta.icon"></i></div>
                                        <div>
                                            <div class="selected-request-type-title">{{ selectedScenarioMeta.titleKey | translate }}</div>
                                            <div class="selected-request-type-helper">{{ selectedScenarioMeta.helperKey | translate }}</div>
                                        </div>
                                    </div>
                                </div>
                            }

                            <form [formGroup]="form" class="step2-form">
                                <div class="section mt-8">
                                    <h3 class="section-heading">{{ 'designCad.new.sectionDetails' | translate }}</h3>
                                    <div class="section-fields">
                                        <div class="form-group">
                                            <label>{{ 'designCad.new.title' | translate }} <span class="required">*</span></label>
                                            <input pInputText formControlName="title" class="w-full" [placeholder]="'designCad.new.titlePlaceholder' | translate" maxlength="500" />
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'designCad.new.description' | translate }} <span class="required">*</span></label>
                                            <textarea pTextarea formControlName="description" rows="4" class="w-full"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="section mt-8">
                                    <h3 class="section-heading">{{ 'designCad.new.sectionEngineering' | translate }}</h3>
                                    <p class="engineering-help">{{ engineeringHelpText() }}</p>
                                    <div class="section-fields">
                                        @if (selectedScenario() === 'ideaOnly') {
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.intendedUse' | translate }} <span class="required">*</span></label>
                                                <input pInputText formControlName="intendedUse" class="w-full" placeholder="e.g. Wall bracket for a small sensor enclosure" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.materialPreference' | translate }}</label>
                                                <input pInputText formControlName="materialPreference" class="w-full" placeholder="e.g. PLA, PETG, aluminum, or not sure" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.approximateDimensions' | translate }}</label>
                                                <input pInputText formControlName="approximateDimensions" class="w-full" placeholder="e.g. Approx. 80mm x 40mm x 25mm" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.targetProcess' | translate }}</label>
                                                <p-select formControlName="targetProcess" [options]="targetProcessOptions" optionLabel="label" optionValue="value" [placeholder]="'designCad.new.targetProcessPlaceholder' | translate" styleClass="w-full"></p-select>
                                            </div>
                                        }
                                        @if (selectedScenario() === 'existingCad') {
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.changeRequests' | translate }} <span class="required">*</span></label>
                                                <textarea pTextarea formControlName="changeRequests" rows="3" class="w-full" placeholder="e.g. Increase hole diameter from 3mm to 4mm and thicken the side walls"></textarea>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.toleranceNotes' | translate }}</label>
                                                <input pInputText formControlName="toleranceNotes" class="w-full" placeholder="e.g. Critical fit on bearing seat ±0.05 mm" />
                                            </div>
                                        }
                                        @if (selectedScenario() === 'reverseEngineering') {
                                            <div class="form-group checkbox-group">
                                                <p-checkbox formControlName="confirmOwnership" [binary]="true" inputId="confirmOwnership"></p-checkbox>
                                                <label for="confirmOwnership">{{ 'designCad.new.confirmOwnership' | translate }}</label>
                                            </div>
                                            <div class="form-group checkbox-group">
                                                <p-checkbox formControlName="canDeliverPhysicalPart" [binary]="true" inputId="canDeliverPart"></p-checkbox>
                                                <label for="canDeliverPart">{{ 'designCad.new.canDeliverPart' | translate }}</label>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.criticalSurfaces' | translate }}</label>
                                                <textarea pTextarea formControlName="criticalSurfaces" rows="2" class="w-full" placeholder="e.g. Outer clip edges and center shaft profile are critical"></textarea>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.fitmentRequirements' | translate }}</label>
                                                <textarea pTextarea formControlName="fitmentRequirements" rows="2" class="w-full" placeholder="e.g. Must snap into original housing and rotate smoothly"></textarea>
                                            </div>
                                        }
                                        @if (selectedScenario() === 'physicalObject') {
                                            <div class="form-group checkbox-group">
                                                <p-checkbox formControlName="canDeliverPhysicalItem" [binary]="true" inputId="canDeliverItem"></p-checkbox>
                                                <label for="canDeliverItem">{{ 'designCad.new.canDeliverPhysicalItem' | translate }}</label>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.purposeAndConstraints' | translate }} <span class="required">*</span></label>
                                                <textarea pTextarea formControlName="purposeAndConstraints" rows="3" class="w-full" placeholder="e.g. Need a CAD model of this handheld cover. Must stay lightweight and fit a 2mm wall thickness target."></textarea>
                                            </div>
                                        }
                                        @if (selectedScenario() === 'modifyProduct') {
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.whatToModify' | translate }} <span class="required">*</span></label>
                                                <textarea pTextarea formControlName="whatToModify" rows="3" class="w-full" [placeholder]="'designCad.new.whatToModifyPlaceholder' | translate"></textarea>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.referenceProductLink' | translate }}</label>
                                                <input pInputText formControlName="referenceProductLink" class="w-full" [placeholder]="'designCad.new.referenceProductLinkPlaceholder' | translate" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.measurementsOrConstraints' | translate }}</label>
                                                <textarea pTextarea formControlName="measurementsOrConstraints" rows="2" class="w-full" [placeholder]="'designCad.new.measurementsOrConstraintsPlaceholder' | translate"></textarea>
                                            </div>
                                        }
                                        @if (selectedScenario() === 'mechanicalAssembly') {
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.purposeOfAssembly' | translate }} <span class="required">*</span></label>
                                                <textarea pTextarea formControlName="purposeOfAssembly" rows="3" class="w-full" [placeholder]="'designCad.new.purposeOfAssemblyPlaceholder' | translate"></textarea>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.numberOfParts' | translate }}</label>
                                                <input pInputText formControlName="numberOfParts" class="w-full" [placeholder]="'designCad.new.numberOfPartsPlaceholder' | translate" />
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.movingPartsOrMechanisms' | translate }}</label>
                                                <textarea pTextarea formControlName="movingPartsOrMechanisms" rows="2" class="w-full" [placeholder]="'designCad.new.movingPartsPlaceholder' | translate"></textarea>
                                            </div>
                                            <div class="form-group">
                                                <label>{{ 'designCad.new.spaceSizeConstraints' | translate }}</label>
                                                <textarea pTextarea formControlName="spaceSizeConstraints" rows="2" class="w-full" [placeholder]="'designCad.new.spaceSizeConstraintsPlaceholder' | translate"></textarea>
                                            </div>
                                        }
                                    </div>
                                </div>

                                <div class="section mt-8">
                                    <h3 class="section-heading">{{ 'designCad.new.sectionFiles' | translate }}</h3>
                                    <div class="section-fields">
                                        <div class="form-group">
                                            <label>{{ 'designCad.new.uploadFiles' | translate }}</label>
                                            <p class="upload-hint">{{ uploadHintText() }}</p>
                                            <input type="file" #fileInput multiple [accept]="acceptExtensions" (change)="onFilesSelected($event)" style="display: none" />
                                            <button type="button" class="upload-trigger" (click)="fileInput.click()">
                                                <i class="pi pi-upload"></i> {{ 'designCad.new.chooseFiles' | translate }}
                                            </button>
                                            @if (files().length > 0) {
                                                <ul class="file-list">
                                                    @for (f of files(); track f.name + f.size) {
                                                        <li>
                                                            <span>{{ f.name }}</span>
                                                            <button type="button" class="remove-file" (click)="removeFile(f)"><i class="pi pi-times"></i></button>
                                                        </li>
                                                    }
                                                </ul>
                                            }
                                        </div>
                                    </div>
                                </div>

                                <div class="section mt-8">
                                    <h3 class="section-heading">{{ 'designCad.new.sectionExtra' | translate }}</h3>
                                    <div class="section-fields">
                                        <div class="form-group">
                                            <label>{{ 'designCad.new.deadline' | translate }}</label>
                                            <input pInputText formControlName="deadline" class="w-full" />
                                        </div>
                                        <div class="form-group">
                                            <label>{{ 'designCad.new.additionalNotes' | translate }}</label>
                                            <textarea pTextarea formControlName="additionalNotes" rows="2" class="w-full"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="step-actions">
                                    <p-button [label]="'designCad.new.prev' | translate" icon="pi pi-arrow-left" iconPos="left" (onClick)="prevStep()" [outlined]="true"></p-button>
                                    <p-button [label]="'designCad.new.next' | translate" icon="pi pi-arrow-right" iconPos="right" (onClick)="nextStep()" [disabled]="!isStep2Valid()" severity="primary" styleClass="design-cad-step-btn"></p-button>
                                </div>
                            </form>
                        </div>
                    }

                    <!-- Step 3: Review & submit -->
                    @if (currentStep() === 2) {
                        <div class="step-card">
                            <h2 class="step-heading">{{ 'designCad.new.step3Title' | translate }}</h2>
                            @if (selectedScenarioMeta) {
                                <div class="review-request-type-block">
                                    <div class="review-request-type-label">{{ 'designCad.new.reviewScenarioLabel' | translate }}</div>
                                    <div class="review-request-type-inner">
                                        <div class="review-request-type-icon"><i class="pi" [ngClass]="selectedScenarioMeta.icon"></i></div>
                                        <div class="review-request-type-title">{{ selectedScenarioMeta.titleKey | translate }}</div>
                                    </div>
                                </div>
                            }
                            <div class="review-block">
                                <p><strong>{{ 'designCad.new.title' | translate }}:</strong> {{ form.get('title')?.value }}</p>
                                <p><strong>{{ 'designCad.new.description' | translate }}:</strong> {{ form.get('description')?.value }}</p>
                                @if (selectedScenario() === 'ideaOnly' && form.get('intendedUse')?.value) { <p><strong>{{ 'designCad.new.intendedUse' | translate }}:</strong> {{ form.get('intendedUse')?.value }}</p> }
                                @if (selectedScenario() === 'existingCad' && form.get('changeRequests')?.value) { <p><strong>{{ 'designCad.new.changeRequests' | translate }}:</strong> {{ form.get('changeRequests')?.value }}</p> }
                                @if (selectedScenario() === 'reverseEngineering') {
                                    @if (form.get('criticalSurfaces')?.value) { <p><strong>{{ 'designCad.new.criticalSurfaces' | translate }}:</strong> {{ form.get('criticalSurfaces')?.value }}</p> }
                                    @if (form.get('fitmentRequirements')?.value) { <p><strong>{{ 'designCad.new.fitmentRequirements' | translate }}:</strong> {{ form.get('fitmentRequirements')?.value }}</p> }
                                }
                                @if (selectedScenario() === 'physicalObject' && form.get('purposeAndConstraints')?.value) { <p><strong>{{ 'designCad.new.purposeAndConstraints' | translate }}:</strong> {{ form.get('purposeAndConstraints')?.value }}</p> }
                                @if (selectedScenario() === 'modifyProduct') {
                                    @if (form.get('whatToModify')?.value) { <p><strong>{{ 'designCad.new.whatToModify' | translate }}:</strong> {{ form.get('whatToModify')?.value }}</p> }
                                    @if (form.get('referenceProductLink')?.value) { <p><strong>{{ 'designCad.new.referenceProductLink' | translate }}:</strong> {{ form.get('referenceProductLink')?.value }}</p> }
                                    @if (form.get('measurementsOrConstraints')?.value) { <p><strong>{{ 'designCad.new.measurementsOrConstraints' | translate }}:</strong> {{ form.get('measurementsOrConstraints')?.value }}</p> }
                                }
                                @if (selectedScenario() === 'mechanicalAssembly') {
                                    @if (form.get('purposeOfAssembly')?.value) { <p><strong>{{ 'designCad.new.purposeOfAssembly' | translate }}:</strong> {{ form.get('purposeOfAssembly')?.value }}</p> }
                                    @if (form.get('numberOfParts')?.value) { <p><strong>{{ 'designCad.new.numberOfParts' | translate }}:</strong> {{ form.get('numberOfParts')?.value }}</p> }
                                    @if (form.get('movingPartsOrMechanisms')?.value) { <p><strong>{{ 'designCad.new.movingPartsOrMechanisms' | translate }}:</strong> {{ form.get('movingPartsOrMechanisms')?.value }}</p> }
                                    @if (form.get('spaceSizeConstraints')?.value) { <p><strong>{{ 'designCad.new.spaceSizeConstraints' | translate }}:</strong> {{ form.get('spaceSizeConstraints')?.value }}</p> }
                                }
                                <p><strong>{{ 'designCad.new.filesCount' | translate }}:</strong> {{ files().length }}</p>
                                @if (form.get('deadline')?.value) { <p><strong>{{ 'designCad.new.deadline' | translate }}:</strong> {{ form.get('deadline')?.value }}</p> }
                                @if (form.get('additionalNotes')?.value) { <p><strong>{{ 'designCad.new.additionalNotes' | translate }}:</strong> {{ form.get('additionalNotes')?.value }}</p> }
                            </div>
                            @if (!authService.authenticated) {
                                <p-message severity="info" [text]="'designCad.new.loginRequired' | translate" styleClass="w-full mb-3"></p-message>
                            }
                            <div class="step-actions">
                                <p-button [label]="'designCad.new.prev' | translate" icon="pi pi-arrow-left" iconPos="left" (onClick)="prevStep()" [outlined]="true"></p-button>
                                <p-button
                                    [label]="authService.authenticated ? ('designCad.new.submit' | translate) : ('designCad.new.loginAndSubmit' | translate)"
                                    icon="pi pi-send"
                                    iconPos="right"
                                    (onClick)="submitRequest()"
                                    [loading]="submitting()"
                                    [disabled]="!canSubmit()"
                                    severity="primary"
                                    styleClass="design-cad-step-btn">
                                </p-button>
                            </div>
                        </div>
                    }
                    </div>
                </div>
            }
            </div>
        </section>
    `,
    styles: [`
        :host { --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%); --color-primary: #6d7cff; }
        .design-cad-request-page { position: relative; min-height: calc(100vh - 64px); overflow: hidden; }
        .request-page-bg {
            position: absolute; inset: 0;
            background: linear-gradient(to right, #0b0f2a 0%, #0a1533 50%, #0b2a44 100%);
            z-index: 0;
        }
        .request-page-grid {
            position: absolute; inset: 0; z-index: 1; pointer-events: none; opacity: 0.2;
            background-image: linear-gradient(rgba(140,210,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(140,210,255,0.1) 1px, transparent 1px);
            background-size: 56px 56px;
        }
        .request-page-fade {
            position: absolute; inset: 0; z-index: 1; pointer-events: none;
            mask-image: radial-gradient(ellipse at center, transparent 0%, transparent 35%, black 65%);
            -webkit-mask-image: radial-gradient(ellipse at center, transparent 0%, transparent 35%, black 65%);
            background: transparent;
        }
        .request-page-header {
            position: relative; z-index: 10;
            border-bottom: 1px solid rgba(255,255,255,0.1);
            background: rgba(255,255,255,0.05);
        }
        .request-header-inner { max-width: 1280px; margin: 0 auto; padding: 1.75rem 1.5rem; text-align: center; }
        .request-breadcrumb { font-size: 0.8rem; color: rgba(255,255,255,0.6); }
        .request-breadcrumb-flex { display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 0.75rem; }
        .request-breadcrumb-link { color: rgba(255,255,255,0.75); text-decoration: none; }
        .request-breadcrumb-link:hover { text-decoration: underline; }
        .request-breadcrumb-sep { margin: 0; opacity: 0.5; font-size: 0.7rem; }
        .request-breadcrumb-scenario { color: #9fb0ff; }
        .request-page-title { margin: 0.5rem 0 0 0; font-size: 1.875rem; font-weight: 600; color: white; }
        .request-page-subtitle { margin: 0.35rem 0 0 0; font-size: 0.9375rem; color: rgba(255,255,255,0.6); }
        @media (min-width: 768px) {
            .request-page-title { font-size: 2.25rem; }
            .request-page-subtitle { font-size: 1rem; }
        }
        .request-page-content { position: relative; z-index: 10; max-width: 1280px; margin: 0 auto; padding: 2.5rem 1.5rem; pointer-events: auto; }
        .request-container { max-width: 1280px; margin: 0 auto; padding: 0 1.5rem; }
        .request-card {
            position: relative;
            z-index: 1;
            border-radius: 1rem; border: 1px solid rgba(255,255,255,0.1);
            background: rgba(11,16,38,0.7); padding: 1.5rem 1.5rem 2rem;
            box-shadow: 0 10px 50px rgba(0,0,0,0.45); backdrop-filter: blur(12px);
            pointer-events: auto;
        }
        .stepper-wrapper { margin-left: auto; margin-right: auto; margin-bottom: 2.5rem; width: 100%; max-width: 760px; padding: 0 1rem; position: relative; z-index: 0; }
        .form-wrap { position: relative; z-index: 2; padding: 0; pointer-events: auto; }
        @media (min-width: 768px) {
            .request-card { padding: 2rem 2rem 2.5rem; }
        }
        .step-card { padding: 0; margin-bottom: 0; }
        .step-heading { font-size: 1.35rem; font-weight: 700; color: rgba(255,255,255,0.95); margin: 0 0 0.5rem 0; }
        .step-subtitle { font-size: 0.9375rem; color: rgba(255,255,255,0.6); margin: 0 0 0.75rem 0; }
        .selected-request-type-block { margin-top: 0.75rem; margin-bottom: 1.5rem; border-radius: 0.75rem; border: 1px solid rgba(109,124,255,0.2); background: rgba(109,124,255,0.08); padding: 1rem 1.25rem; }
        .selected-request-type-label { font-size: 0.6875rem; text-transform: uppercase; letter-spacing: 0.1em; color: rgba(255,255,255,0.55); }
        .selected-request-type-inner { margin-top: 0.5rem; display: flex; align-items: flex-start; gap: 0.75rem; }
        .selected-request-type-icon { flex-shrink: 0; width: 2.5rem; height: 2.5rem; display: flex; align-items: center; justify-content: center; border-radius: 0.5rem; border: 1px solid rgba(109,124,255,0.3); background: rgba(109,124,255,0.1); color: #9fb0ff; }
        .selected-request-type-icon .pi { font-size: 1.15rem; }
        .selected-request-type-title { font-size: 0.875rem; font-weight: 600; color: white; }
        .selected-request-type-helper { margin-top: 0.25rem; font-size: 0.875rem; color: rgba(255,255,255,0.65); }
        .review-request-type-block { margin-bottom: 1.5rem; border-radius: 0.75rem; border: 1px solid rgba(255,255,255,0.1); background: rgba(255,255,255,0.03); padding: 0.75rem 1rem; }
        .review-request-type-label { font-size: 0.6875rem; text-transform: uppercase; letter-spacing: 0.08em; color: rgba(255,255,255,0.5); }
        .review-request-type-inner { margin-top: 0.5rem; display: flex; align-items: center; gap: 0.5rem; }
        .review-request-type-icon { width: 2rem; height: 2rem; display: flex; align-items: center; justify-content: center; border-radius: 0.375rem; border: 1px solid rgba(109,124,255,0.25); background: rgba(109,124,255,0.08); color: #9fb0ff; }
        .review-request-type-icon .pi { font-size: 0.9rem; }
        .review-request-type-title { font-size: 0.8125rem; font-weight: 600; color: rgba(255,255,255,0.9); }
        .type-option-selected-check { position: absolute; right: 0.75rem; top: 0.75rem; width: 1.5rem; height: 1.5rem; display: inline-flex; align-items: center; justify-content: center; border-radius: 9999px; background: #6d7cff; color: white; }
        .type-option-selected-check .pi { font-size: 0.65rem; }
        .mt-8 { margin-top: 2rem; }
        .section-heading { font-size: 0.875rem; font-weight: 600; color: rgba(255,255,255,0.8); text-transform: uppercase; letter-spacing: 0.08em; margin: 0 0 0.75rem 0; }
        .section-fields { display: flex; flex-direction: column; gap: 1rem; }
        .engineering-help { font-size: 0.875rem; color: rgba(255,255,255,0.55); margin: 0 0 0.75rem 0; }
        .type-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 0.75rem; margin-bottom: 1.5rem; }
        .type-grid-6 { grid-template-columns: 1fr; }
        @media (min-width: 640px) { .type-grid-6 { grid-template-columns: repeat(2, 1fr); } }
        @media (min-width: 1024px) { .type-grid-6 { grid-template-columns: repeat(3, 1fr); } }
        .type-option {
            position: relative;
            display: flex; flex-direction: column; align-items: center; gap: 0.5rem; padding: 1rem;
            border: 2px solid rgba(255,255,255,0.15); border-radius: 12px;
            background: rgba(255,255,255,0.04); color: rgba(255,255,255,0.9); cursor: pointer; transition: all 0.2s;
            pointer-events: auto;
        }
        .type-option:hover { border-color: rgba(109,124,255,0.5); background: rgba(109,124,255,0.08); }
        .type-option.selected { border-color: var(--color-primary); background: rgba(109,124,255,0.2); }
        .form-group { margin-bottom: 1.25rem; }
        .form-group label { display: block; font-weight: 600; color: rgba(255,255,255,0.75); margin-bottom: 0.35rem; }
        .required { color: #f87171; }
        .w-full { width: 100%; }
        .checkbox-group { display: flex; align-items: center; gap: 0.5rem; }
        .checkbox-group label { margin-bottom: 0; color: rgba(255,255,255,0.75); }
        .upload-hint { font-size: 0.85rem; color: rgba(255,255,255,0.5); margin: 0.25rem 0 0.5rem 0; }
        .upload-trigger {
            padding: 0.5rem 1rem; border: 2px dashed rgba(255,255,255,0.2); border-radius: 10px;
            background: rgba(255,255,255,0.04); color: rgba(255,255,255,0.85); cursor: pointer;
            display: inline-flex; align-items: center; gap: 0.5rem;
        }
        .upload-trigger:hover { border-color: rgba(109,124,255,0.5); background: rgba(109,124,255,0.08); }
        .file-list { list-style: none; padding: 0; margin: 0.5rem 0 0 0; }
        .file-list li { display: flex; align-items: center; justify-content: space-between; padding: 0.35rem 0; border-bottom: 1px solid rgba(255,255,255,0.08); color: rgba(255,255,255,0.8); }
        .remove-file { background: none; border: none; color: rgba(255,255,255,0.5); cursor: pointer; padding: 0.25rem; }
        .step-actions { display: flex; gap: 0.75rem; margin-top: 1.5rem; flex-wrap: wrap; }
        .review-block { background: rgba(255,255,255,0.06); border-radius: 10px; padding: 1rem; margin-bottom: 1rem; border: 1px solid rgba(255,255,255,0.08); }
        .review-block p { margin: 0.35rem 0; color: rgba(255,255,255,0.85); }
        .success-box { text-align: center; padding: 3rem 1.5rem; }
        .success-icon { width: 64px; height: 64px; margin: 0 auto 1rem; background: var(--gradient-primary); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 2rem; color: white; }
        .success-box h2 { margin: 0 0 0.5rem 0; color: white; }
        .ref-display { margin-bottom: 1.5rem; color: rgba(255,255,255,0.8); }
        .mb-3 { margin-bottom: 1rem; }

        /* Stepper: constrained, centered, no overflow */
        :host ::ng-deep .p-steps {
            width: 100%;
        }
        :host ::ng-deep .p-steps > ul,
        :host ::ng-deep .p-steps .p-steps-list {
            display: flex !important;
            align-items: flex-start;
            justify-content: space-between;
            width: 100%;
            margin: 0;
            padding: 0;
        }
        :host ::ng-deep .p-steps .p-steps-item {
            flex: 1 1 0;
            min-width: 0;
            position: relative;
        }
        :host ::ng-deep .p-steps .p-menuitem-link,
        :host ::ng-deep .p-steps .p-steps-item-link {
            background: transparent !important;
            border: 0 !important;
            display: flex !important;
            flex-direction: column;
            align-items: center;
            justify-content: flex-start;
            gap: 0.65rem;
            padding: 0;
            text-decoration: none !important;
        }
        :host ::ng-deep .p-steps .p-steps-number,
        :host ::ng-deep .p-steps .p-steps-item-number {
            width: 2.25rem !important;
            height: 2.25rem !important;
            min-width: 2.25rem !important;
            border-radius: 9999px !important;
            display: inline-flex !important;
            align-items: center;
            justify-content: center;
            margin: 0 auto;
            background: rgba(255,255,255,0.04) !important;
            border: 1px solid rgba(255,255,255,0.14) !important;
            color: rgba(255,255,255,0.92) !important;
            font-size: 0.95rem !important;
            font-weight: 600 !important;
            line-height: 1 !important;
        }
        :host ::ng-deep .p-steps .p-steps-title,
        :host ::ng-deep .p-steps .p-steps-item-label {
            display: block !important;
            margin: 0 !important;
            text-align: center;
            color: rgba(255,255,255,0.58) !important;
            font-size: 0.92rem !important;
            font-weight: 500 !important;
            line-height: 1.25rem !important;
            max-width: 140px;
            white-space: normal !important;
        }
        :host ::ng-deep .p-steps .p-steps-item::before {
            content: "";
            position: absolute;
            top: 1.125rem;
            margin-top: -0.5px;
            height: 1px;
            background: rgba(255,255,255,0.16);
            z-index: 0;
            width: calc(100% - 2.25rem);
            left: calc(-50% + 1.125rem);
            right: auto;
        }
        [dir="rtl"] :host ::ng-deep .p-steps .p-steps-item::before {
            left: auto;
            right: calc(-50% + 1.125rem);
        }
        :host ::ng-deep .p-steps .p-steps-item:first-child::before {
            display: none;
        }
        :host ::ng-deep .p-steps .p-steps-item .p-menuitem-link,
        :host ::ng-deep .p-steps .p-steps-item .p-steps-item-link {
            position: relative;
            z-index: 1;
        }
        :host ::ng-deep .p-steps .p-steps-item.p-highlight .p-steps-number,
        :host ::ng-deep .p-steps .p-steps-item.p-highlight .p-steps-item-number,
        :host ::ng-deep .p-steps .p-steps-item.p-steps-item-active .p-steps-number,
        :host ::ng-deep .p-steps .p-steps-item.p-steps-item-active .p-steps-item-number {
            background: rgba(109,124,255,0.22) !important;
            border-color: rgba(109,124,255,0.7) !important;
            color: #ffffff !important;
        }
        :host ::ng-deep .p-steps .p-steps-item.p-highlight .p-steps-title,
        :host ::ng-deep .p-steps .p-steps-item.p-highlight .p-steps-item-label,
        :host ::ng-deep .p-steps .p-steps-item.p-steps-item-active .p-steps-title,
        :host ::ng-deep .p-steps .p-steps-item.p-steps-item-active .p-steps-item-label {
            color: #7f92ff !important;
        }
        @media (max-width: 768px) {
            :host ::ng-deep .p-steps .p-steps-title,
            :host ::ng-deep .p-steps .p-steps-item-label {
                font-size: 0.82rem !important;
                max-width: 90px;
            }
            :host ::ng-deep .p-steps .p-steps-number,
            :host ::ng-deep .p-steps .p-steps-item-number {
                width: 2rem !important;
                height: 2rem !important;
                min-width: 2rem !important;
                font-size: 0.9rem !important;
            }
            :host ::ng-deep .p-steps .p-steps-item::before {
                top: 1rem;
                margin-top: -0.5px;
                left: calc(-50% + 1rem);
                width: calc(100% - 2rem);
            }
            [dir="rtl"] :host ::ng-deep .p-steps .p-steps-item::before {
                left: auto;
                right: calc(-50% + 1rem);
            }
        }
        :host ::ng-deep .p-inputtext,
        :host ::ng-deep .p-dropdown {
            background: rgba(255,255,255,0.04) !important; color: rgba(255,255,255,0.9) !important;
            border: 1px solid rgba(255,255,255,0.12) !important;
            min-height: 44px !important; height: 44px !important; font-size: 0.95rem !important;
        }
        :host ::ng-deep textarea,
        :host ::ng-deep .p-inputtextarea {
            background: rgba(255,255,255,0.04) !important; color: rgba(255,255,255,0.9) !important;
            border: 1px solid rgba(255,255,255,0.12) !important;
            min-height: 140px !important; font-size: 0.95rem !important;
        }
        :host ::ng-deep .p-inputtext:focus,
        :host ::ng-deep textarea:focus,
        :host ::ng-deep .p-inputtextarea:focus,
        :host ::ng-deep .p-dropdown.p-focus {
            border-color: rgba(109,124,255,0.65) !important;
            box-shadow: 0 0 0 3px rgba(109,124,255,0.18) !important;
        }
        :host ::ng-deep .p-dropdown-label { color: rgba(255,255,255,0.9) !important; }
        :host ::ng-deep .p-button.p-button-primary,
        :host ::ng-deep .design-cad-request-page .p-button.p-button-success {
            background: #6d7cff !important; border-color: #6d7cff !important; color: white !important;
        }
        :host ::ng-deep .p-button.p-button-primary:hover:not(:disabled),
        :host ::ng-deep .design-cad-request-page .p-button.p-button-success:hover:not(:disabled) {
            filter: brightness(1.08); box-shadow: 0 4px 16px rgba(109,124,255,0.3) !important;
        }
        :host ::ng-deep .p-button.p-button-secondary,
        :host ::ng-deep .p-button[outlined] .p-button {
            background: rgba(255,255,255,0.06) !important; border-color: rgba(109,124,255,0.4) !important; color: rgba(255,255,255,0.9) !important;
        }
        :host ::ng-deep .p-button {
            padding: 0.65rem 1.05rem !important; font-size: 0.95rem !important; border-radius: 0.75rem !important;
        }
        :host ::ng-deep .success-back-btn .p-button,
        :host ::ng-deep .success-box .p-button { background: #6d7cff !important; background-color: #6d7cff !important; border: none !important; border-color: #6d7cff !important; color: white !important; font-weight: 600 !important; border-radius: 0.75rem !important; }
        :host ::ng-deep .success-back-btn .p-button:hover,
        :host ::ng-deep .success-box .p-button:hover { filter: brightness(1.08); box-shadow: 0 4px 16px rgba(109,124,255,0.35) !important; background: #6d7cff !important; background-color: #6d7cff !important; }
        :host ::ng-deep .design-cad-step-btn .p-button { background: #6d7cff !important; border: none !important; color: white !important; font-weight: 600 !important; border-radius: 0.75rem !important; }
        :host ::ng-deep .design-cad-step-btn .p-button:hover:not(:disabled) { filter: brightness(1.08); box-shadow: 0 4px 16px rgba(109,124,255,0.35) !important; }
    `]
})
export class DesignCadNewComponent implements OnInit {
    languageService = inject(LanguageService);
    authService = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private designCadService = inject(DesignCadRequestService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private fb = inject(FormBuilder);
    private cdr = inject(ChangeDetectorRef);

    readonly acceptExtensions = ACCEPTED_EXTENSIONS;
    readonly requestScenarioCards = REQUEST_SCENARIO_CARDS;
    targetProcessOptions: { label: string; value: DesignCadTargetProcess }[] = [];

    form = this.fb.group({
        scenario: ['ideaOnly' as CadRequestScenario, Validators.required],
        title: ['', Validators.required],
        description: ['', Validators.required],
        deadline: [''],
        additionalNotes: [''],
        intendedUse: [''],
        materialPreference: [''],
        approximateDimensions: [''],
        targetProcess: [null as DesignCadTargetProcess | null],
        changeRequests: [''],
        toleranceNotes: [''],
        confirmOwnership: [false],
        canDeliverPhysicalPart: [false],
        criticalSurfaces: [''],
        fitmentRequirements: [''],
        purposeAndConstraints: [''],
        canDeliverPhysicalItem: [false],
        whatToModify: [''],
        referenceProductLink: [''],
        measurementsOrConstraints: [''],
        purposeOfAssembly: [''],
        numberOfParts: [''],
        movingPartsOrMechanisms: [''],
        spaceSizeConstraints: ['']
    });

    currentStep = signal(0);
    files = signal<File[]>([]);
    submitting = signal(false);
    submitted = signal(false);
    referenceNumber = signal('');

    /** Writable signal kept in sync with form so the template updates immediately on click. */
    selectedScenario = signal<CadRequestScenario | null>(null);

    /** Meta for the currently selected scenario (titleKey, icon, helperKey). */
    get selectedScenarioMeta(): { titleKey: string; icon: string; helperKey: string } | null {
        const scenario = this.selectedScenario();
        return (scenario && REQUEST_SCENARIO_META[scenario]) ? REQUEST_SCENARIO_META[scenario] : null;
    }

    stepItems = toSignal(
        this.translate.onLangChange.pipe(
            startWith(null),
            switchMap(() =>
                combineLatest([
                    this.translate.get('designCad.new.step1Title'),
                    this.translate.get('designCad.new.step2Title'),
                    this.translate.get('designCad.new.step3Title')
                ])
            ),
            map(([a, b, c]) => [
                { label: a },
                { label: b },
                { label: c }
            ])
        ),
        { initialValue: [{ label: 'Request type' }, { label: 'Details & files' }, { label: 'Review & submit' }] }
    );

    ngOnInit() {
        this.targetProcessOptions = [
            { label: this.languageService.translate('designCad.new.targetGeneral'), value: 'General' },
            { label: this.languageService.translate('designCad.new.targetPrint3d'), value: 'Print3d' },
            { label: this.languageService.translate('designCad.new.targetCnc'), value: 'Cnc' },
            { label: this.languageService.translate('designCad.new.targetInjection'), value: 'Injection' },
            { label: this.languageService.translate('designCad.new.targetOther'), value: 'Other' }
        ];
        this.restoreDraft();
        this.selectedScenario.set(this.form.get('scenario')?.value ?? null);
        this.updateScenarioValidators();
        this.form.get('scenario')?.valueChanges.subscribe((value: CadRequestScenario | null) => {
            this.selectedScenario.set(value ?? null);
            this.updateScenarioValidators();
        });
        const type = this.route.snapshot.queryParams['type'] as string | undefined;
        const typeToScenario: Record<string, CadRequestScenario> = {
            custom: 'ideaOnly',
            reverse: 'reverseEngineering',
            fix: 'existingCad',
            outputs: 'existingCad',
            modify: 'modifyProduct',
            assembly: 'mechanicalAssembly'
        };
        const scenario = type ? typeToScenario[type] : undefined;
        if (scenario) {
            this.form.patchValue({ scenario });
            this.selectedScenario.set(scenario);
            this.updateScenarioValidators();
            this.saveDraft();
        }
    }

    selectScenario(key: CadRequestScenario) {
        this.form.patchValue({ scenario: key });
        this.selectedScenario.set(key);
        this.saveDraft();
        this.cdr.detectChanges();
    }

    step2HelperText(): string {
        const s = this.selectedScenario();
        if (!s) return '';
        const key = REQUEST_SCENARIO_META[s]?.helperKey ?? 'designCad.new.uploadHint';
        return this.languageService.translate(key);
    }

    engineeringHelpText(): string {
        const s = this.selectedScenario();
        if (!s) return '';
        const suffix = s === 'ideaOnly' ? 'IdeaOnly' : s === 'existingCad' ? 'ExistingCad' : s === 'reverseEngineering' ? 'ReverseEngineering' : s === 'physicalObject' ? 'PhysicalObject' : s === 'modifyProduct' ? 'ModifyProduct' : 'MechanicalAssembly';
        return this.languageService.translate('designCad.new.engineeringHelp' + suffix);
    }

    uploadHintText(): string {
        const s = this.selectedScenario();
        if (!s) return this.languageService.translate('designCad.new.uploadHint');
        const suffix = s === 'ideaOnly' ? 'IdeaOnly' : s === 'existingCad' ? 'ExistingCad' : s === 'reverseEngineering' ? 'ReverseEngineering' : s === 'physicalObject' ? 'PhysicalObject' : s === 'modifyProduct' ? 'ModifyProduct' : 'MechanicalAssembly';
        return this.languageService.translate('designCad.new.uploadHint' + suffix);
    }

    scenarioLabel(): string {
        const s = this.selectedScenario();
        if (!s) return '';
        return this.languageService.translate('designCad.new.scenarioBadge.' + s);
    }

    isStep2Valid(): boolean {
        const v = this.form.getRawValue();
        if (!v.scenario || !v.title?.trim() || !v.description?.trim()) return false;
        if (v.scenario === 'ideaOnly' && !v.intendedUse?.trim()) return false;
        if (v.scenario === 'existingCad' && !v.changeRequests?.trim()) return false;
        if (v.scenario === 'reverseEngineering' && !v.confirmOwnership) return false;
        if (v.scenario === 'physicalObject' && !v.purposeAndConstraints?.trim()) return false;
        if (v.scenario === 'modifyProduct' && !v.whatToModify?.trim()) return false;
        if (v.scenario === 'mechanicalAssembly' && !v.purposeOfAssembly?.trim()) return false;
        return true;
    }

    private updateScenarioValidators() {
        const s = this.selectedScenario();
        const intendedUse = this.form.get('intendedUse');
        const changeRequests = this.form.get('changeRequests');
        const confirmOwnership = this.form.get('confirmOwnership');
        const purposeAndConstraints = this.form.get('purposeAndConstraints');
        const whatToModify = this.form.get('whatToModify');
        const purposeOfAssembly = this.form.get('purposeOfAssembly');
        intendedUse?.clearValidators();
        changeRequests?.clearValidators();
        confirmOwnership?.clearValidators();
        purposeAndConstraints?.clearValidators();
        whatToModify?.clearValidators();
        purposeOfAssembly?.clearValidators();
        if (s === 'ideaOnly') intendedUse?.setValidators(Validators.required);
        if (s === 'existingCad') changeRequests?.setValidators(Validators.required);
        if (s === 'reverseEngineering') confirmOwnership?.setValidators(Validators.requiredTrue);
        if (s === 'physicalObject') purposeAndConstraints?.setValidators(Validators.required);
        if (s === 'modifyProduct') whatToModify?.setValidators(Validators.required);
        if (s === 'mechanicalAssembly') purposeOfAssembly?.setValidators(Validators.required);
        intendedUse?.updateValueAndValidity();
        changeRequests?.updateValueAndValidity();
        confirmOwnership?.updateValueAndValidity();
        purposeAndConstraints?.updateValueAndValidity();
        whatToModify?.updateValueAndValidity();
        purposeOfAssembly?.updateValueAndValidity();
    }

    nextStep() {
        if (this.currentStep() < 2) {
            this.currentStep.update(s => s + 1);
            this.saveDraft();
        }
    }

    prevStep() {
        if (this.currentStep() > 0) this.currentStep.update(s => s - 1);
    }

    onFilesSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const list = input.files ? Array.from(input.files) : [];
        const current = this.files();
        const combined = [...current];
        for (const f of list) {
            if (combined.length >= MAX_FILES) break;
            if (f.size <= MAX_FILE_SIZE_MB * 1024 * 1024) combined.push(f);
        }
        this.files.set(combined);
        input.value = '';
        this.saveDraft();
    }

    removeFile(file: File) {
        this.files.update(list => list.filter(f => f !== file));
        this.saveDraft();
    }

    canSubmit(): boolean {
        const s = this.selectedScenario();
        if (!s || !this.form.get('title')?.value?.trim() || !this.form.get('description')?.value?.trim()) return false;
        if (s === 'ideaOnly' && !this.form.get('intendedUse')?.value?.trim()) return false;
        if (s === 'existingCad' && !this.form.get('changeRequests')?.value?.trim()) return false;
        if (s === 'reverseEngineering' && !this.form.get('confirmOwnership')?.value) return false;
        if (s === 'physicalObject' && !this.form.get('purposeAndConstraints')?.value?.trim()) return false;
        if (s === 'modifyProduct' && !this.form.get('whatToModify')?.value?.trim()) return false;
        if (s === 'mechanicalAssembly' && !this.form.get('purposeOfAssembly')?.value?.trim()) return false;
        return true;
    }

    submitRequest() {
        if (!this.canSubmit()) return;
        if (!this.authService.authenticated) {
            this.saveDraft();
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/design-cad/new' } });
            return;
        }
        const payload = this.buildPayload();
        const fileList = this.files();
        this.submitting.set(true);
        this.designCadService.create(payload, fileList).subscribe({
            next: (res) => {
                this.submitting.set(false);
                this.submitted.set(true);
                this.referenceNumber.set(res.referenceNumber);
                this.clearDraft();
                this.messageService.add({ severity: 'success', summary: this.languageService.translate('messages.success'), detail: this.languageService.translate('designCad.new.submitSuccess') });
            },
            error: () => {
                this.submitting.set(false);
                this.messageService.add({ severity: 'error', summary: this.languageService.translate('messages.error'), detail: this.languageService.translate('messages.saveError') });
            }
        });
    }

    private buildPayload(): CreateDesignCadRequestPayload {
        const v = this.form.getRawValue();
        const scenario = v.scenario ?? 'ideaOnly';
        const requestType = SCENARIO_TO_REQUEST_TYPE[scenario];
        const hasPhysicalPart = scenario === 'reverseEngineering' || scenario === 'physicalObject';
        return {
            scenario,
            requestType,
            title: (v.title ?? '').trim(),
            description: (v.description ?? '').trim() || undefined,
            deadline: (v.deadline ?? '').trim() || undefined,
            additionalNotes: (v.additionalNotes ?? '').trim() || undefined,
            intendedUse: (v.intendedUse ?? '').trim() || undefined,
            materialPreference: (v.materialPreference ?? '').trim() || undefined,
            approximateDimensions: (v.approximateDimensions ?? '').trim() || undefined,
            materialNotes: (v.materialPreference ?? '').trim() || undefined,
            dimensionsNotes: (v.approximateDimensions ?? '').trim() || undefined,
            targetProcess: v.targetProcess ?? undefined,
            changeRequests: (v.changeRequests ?? '').trim() || undefined,
            whatNeedsChange: (v.changeRequests ?? '').trim() || undefined,
            toleranceNotes: (v.toleranceNotes ?? '').trim() || undefined,
            confirmOwnership: v.confirmOwnership ?? false,
            canDeliverPhysicalPart: v.canDeliverPhysicalPart ?? false,
            criticalSurfaces: (v.criticalSurfaces ?? '').trim() || undefined,
            fitmentRequirements: (v.fitmentRequirements ?? '').trim() || undefined,
            purposeAndConstraints: (v.purposeAndConstraints ?? '').trim() || undefined,
            canDeliverPhysicalItem: v.canDeliverPhysicalItem ?? false,
            whatToModify: (v.whatToModify ?? '').trim() || undefined,
            referenceProductLink: (v.referenceProductLink ?? '').trim() || undefined,
            measurementsOrConstraints: (v.measurementsOrConstraints ?? '').trim() || undefined,
            purposeOfAssembly: (v.purposeOfAssembly ?? '').trim() || undefined,
            numberOfParts: (v.numberOfParts ?? '').trim() || undefined,
            movingPartsOrMechanisms: (v.movingPartsOrMechanisms ?? '').trim() || undefined,
            spaceSizeConstraints: (v.spaceSizeConstraints ?? '').trim() || undefined,
            hasPhysicalPart,
            legalConfirmation: v.confirmOwnership ?? false,
            customerNotes: (v.additionalNotes ?? '').trim() || undefined
        };
    }

    private saveDraft() {
        try {
            const value = this.form.getRawValue();
            const draft = { step: this.currentStep(), formValue: value };
            localStorage.setItem(DRAFT_KEY, JSON.stringify(draft));
        } catch (_) {}
    }

    private restoreDraft() {
        try {
            const raw = localStorage.getItem(DRAFT_KEY);
            if (!raw) return;
            const draft = JSON.parse(raw);
            if (draft.step != null) this.currentStep.set(draft.step);
            if (draft.formValue && typeof draft.formValue === 'object') {
                this.form.patchValue(draft.formValue, { emitEvent: false });
                this.updateScenarioValidators();
            }
        } catch (_) {}
    }

    private clearDraft() {
        try {
            localStorage.removeItem(DRAFT_KEY);
        } catch (_) {}
    }
}
