import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageSection, SectionType } from '../../../../shared/models/cms.model';
import { MediaPickerComponent } from '../../../../shared/components/media-picker/media-picker.component';
import { MediaAsset } from '../../../../shared/models/media.model';

@Component({
    selector: 'app-section-editor',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        TranslateModule,
        MediaPickerComponent
    ],
    template: `
        <form [formGroup]="sectionForm" class="flex flex-column gap-4">
            <div>
                <label class="block mb-2 font-medium">{{ 'admin.cms.sectionType' | translate }}</label>
                <p class="text-500">{{ section.type }}</p>
            </div>

            <div>
                <label class="block mb-2 font-medium">{{ 'common.nameEn' | translate }}</label>
                <input 
                    pInputText 
                    formControlName="titleEn"
                    [placeholder]="'admin.cms.titleEnPlaceholder' | translate"
                    class="w-full"
                />
            </div>

            <div>
                <label class="block mb-2 font-medium">{{ 'common.nameAr' | translate }}</label>
                <input 
                    pInputText 
                    formControlName="titleAr"
                    [placeholder]="'admin.cms.titleArPlaceholder' | translate"
                    class="w-full"
                />
            </div>

            <!-- Hero Section Fields -->
            <ng-container *ngIf="section.type === SectionType.Hero">
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.subtitleEn' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="subtitleEn"
                        [placeholder]="'admin.cms.subtitleEnPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.subtitleAr' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="subtitleAr"
                        [placeholder]="'admin.cms.subtitleArPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.ctaTextEn' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="ctaTextEn"
                        [placeholder]="'admin.cms.ctaTextEnPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.ctaTextAr' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="ctaTextAr"
                        [placeholder]="'admin.cms.ctaTextArPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.ctaLink' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="ctaLink"
                        [placeholder]="'admin.cms.ctaLinkPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.backgroundImage' | translate }}</label>
                    <app-media-picker
                        [buttonLabel]="'admin.cms.selectImage' | translate"
                        [multiple]="false"
                        (mediaSelected)="onMediaSelected($event)"
                    ></app-media-picker>
                    <img 
                        *ngIf="selectedImageUrl" 
                        [src]="selectedImageUrl" 
                        alt="Background"
                        class="mt-2 border-round"
                        style="max-width: 200px; max-height: 150px;"
                    />
                </div>
            </ng-container>

            <!-- Content Section Fields -->
            <ng-container *ngIf="section.type === SectionType.Content">
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.contentEn' | translate }}</label>
                    <textarea 
                        pInputTextarea 
                        formControlName="contentEn"
                        [rows]="8"
                        [placeholder]="'admin.cms.contentEnPlaceholder' | translate"
                        class="w-full"
                    ></textarea>
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.contentAr' | translate }}</label>
                    <textarea 
                        pInputTextarea 
                        formControlName="contentAr"
                        [rows]="8"
                        [placeholder]="'admin.cms.contentArPlaceholder' | translate"
                        class="w-full"
                    ></textarea>
                </div>
            </ng-container>

            <!-- Banner Section Fields -->
            <ng-container *ngIf="section.type === SectionType.Banner">
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.image' | translate }}</label>
                    <app-media-picker
                        [buttonLabel]="'admin.cms.selectImage' | translate"
                        [multiple]="false"
                        (mediaSelected)="onMediaSelected($event)"
                    ></app-media-picker>
                    <img 
                        *ngIf="selectedImageUrl" 
                        [src]="selectedImageUrl" 
                        alt="Banner"
                        class="mt-2 border-round"
                        style="max-width: 200px; max-height: 150px;"
                    />
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.linkUrl' | translate }}</label>
                    <input 
                        pInputText 
                        formControlName="linkUrl"
                        [placeholder]="'admin.cms.linkUrlPlaceholder' | translate"
                        class="w-full"
                    />
                </div>
            </ng-container>

            <!-- Generic Content Fields for other types -->
            <ng-container *ngIf="section.type !== SectionType.Hero && section.type !== SectionType.Content && section.type !== SectionType.Banner">
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.contentEn' | translate }}</label>
                    <textarea 
                        pInputTextarea 
                        formControlName="contentEn"
                        [rows]="6"
                        [placeholder]="'admin.cms.contentEnPlaceholder' | translate"
                        class="w-full"
                    ></textarea>
                </div>
                <div>
                    <label class="block mb-2 font-medium">{{ 'admin.cms.contentAr' | translate }}</label>
                    <textarea 
                        pInputTextarea 
                        formControlName="contentAr"
                        [rows]="6"
                        [placeholder]="'admin.cms.contentArPlaceholder' | translate"
                        class="w-full"
                    ></textarea>
                </div>
            </ng-container>

            <div class="flex justify-content-end gap-2 pt-3 border-top-1">
                <p-button 
                    [label]="'common.cancel' | translate"
                    [outlined]="true"
                    (click)="cancel()"
                ></p-button>
                <p-button 
                    [label]="'common.save' | translate"
                    (click)="save()"
                ></p-button>
            </div>
        </form>
    `
})
export class SectionEditorComponent implements OnInit {
    @Input() section!: PageSection;
    @Output() onSave = new EventEmitter<PageSection>();
    @Output() onCancel = new EventEmitter<void>();

    sectionForm: FormGroup;
    selectedImageUrl: string | null = null;
    SectionType = SectionType;

    private fb = inject(FormBuilder);
    private translateService = inject(TranslateService);

    constructor() {
        this.sectionForm = this.fb.group({
            titleEn: [''],
            titleAr: [''],
            subtitleEn: [''],
            subtitleAr: [''],
            contentEn: [''],
            contentAr: [''],
            ctaTextEn: [''],
            ctaTextAr: [''],
            ctaLink: [''],
            linkUrl: ['']
        });
    }

    ngOnInit() {
        if (this.section) {
            this.sectionForm.patchValue({
                titleEn: this.section.titleEn || '',
                titleAr: this.section.titleAr || '',
                contentEn: this.section.contentEn || '',
                contentAr: this.section.contentAr || ''
            });

            // Load settings for hero/banner sections
            if (this.section.settings) {
                const settings = typeof this.section.settings === 'string' 
                    ? JSON.parse(this.section.settings) 
                    : this.section.settings;
                
                this.sectionForm.patchValue({
                    subtitleEn: settings.subtitleEn || '',
                    subtitleAr: settings.subtitleAr || '',
                    ctaTextEn: settings.ctaTextEn || '',
                    ctaTextAr: settings.ctaTextAr || '',
                    ctaLink: settings.ctaLink || '',
                    linkUrl: settings.linkUrl || ''
                });

                if (settings.imageUrl) {
                    this.selectedImageUrl = settings.imageUrl;
                }
            }
        }
    }

    onMediaSelected(media: MediaAsset[]) {
        if (media && media.length > 0) {
            this.selectedImageUrl = media[0].fileUrl;
        }
    }

    save() {
        const formValue = this.sectionForm.value;
        const updatedSection: PageSection = {
            ...this.section,
            titleEn: formValue.titleEn,
            titleAr: formValue.titleAr,
            contentEn: formValue.contentEn,
            contentAr: formValue.contentAr
        };

        // Build settings object based on section type
        const settings: any = {};
        
        if (this.section.type === SectionType.Hero) {
            settings.subtitleEn = formValue.subtitleEn;
            settings.subtitleAr = formValue.subtitleAr;
            settings.ctaTextEn = formValue.ctaTextEn;
            settings.ctaTextAr = formValue.ctaTextAr;
            settings.ctaLink = formValue.ctaLink;
            if (this.selectedImageUrl) {
                settings.imageUrl = this.selectedImageUrl;
            }
        } else if (this.section.type === SectionType.Banner) {
            settings.linkUrl = formValue.linkUrl;
            if (this.selectedImageUrl) {
                settings.imageUrl = this.selectedImageUrl;
            }
        }

        updatedSection.settings = settings;
        this.onSave.emit(updatedSection);
    }

    cancel() {
        this.onCancel.emit();
    }
}

