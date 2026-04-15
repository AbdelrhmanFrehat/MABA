import {
    Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DownloadableFilesService } from '../../services/downloadable-files.service';
import { DownloadableFileDto, DOWNLOAD_CATEGORIES, categoryIcon, formatFileSize } from '../../models/downloadable-file.model';

@Component({
    selector: 'app-downloadable-files-section',
    standalone: true,
    imports: [CommonModule, TranslateModule, ButtonModule, TooltipModule],
    template: `
        @if (!loading() && files().length > 0) {
            <section class="downloads-section">
                <div class="section-header">
                    <div class="header-icon">
                        <i class="pi pi-download"></i>
                    </div>
                    <div>
                        <h2 class="section-title">{{ 'downloads.section.title' | translate }}</h2>
                        <p class="section-subtitle">{{ 'downloads.section.subtitle' | translate }}</p>
                    </div>
                </div>

                <div class="files-grid">
                    @for (file of files(); track file.id) {
                        <div class="file-card" [class.featured]="file.isFeatured">
                            <div class="card-icon-col">
                                <div class="icon-circle">
                                    <i [class]="getCategoryIcon(file.category)"></i>
                                </div>
                            </div>
                            <div class="card-body">
                                <div class="card-meta">
                                    <span class="category-badge">{{ getCategoryLabel(file.category) }}</span>
                                    <span class="file-size">{{ formatFileSize(file.fileSizeBytes) }}</span>
                                </div>
                                <h4 class="file-title">{{ file.title }}</h4>
                                @if (file.description) {
                                    <p class="file-description">{{ file.description }}</p>
                                }
                            </div>
                            <div class="card-action">
                                <a [href]="getDownloadUrl(file.id)"
                                   target="_blank"
                                   rel="noopener noreferrer"
                                   class="download-btn"
                                   [attr.aria-label]="('downloads.section.downloadFile' | translate) + ' ' + file.title">
                                    <i class="pi pi-download"></i>
                                    <span>{{ 'downloads.section.download' | translate }}</span>
                                </a>
                            </div>
                        </div>
                    }
                </div>
            </section>
        }
    `,
    styles: [`
        .downloads-section {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 1.75rem;
            margin: 2rem 0;
        }

        .section-header {
            display: flex;
            align-items: flex-start;
            gap: 1rem;
            margin-bottom: 1.5rem;
            padding-bottom: 1.25rem;
            border-bottom: 2px solid #f1f5f9;
        }

        .header-icon {
            width: 2.5rem;
            height: 2.5rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .header-icon i {
            color: white;
            font-size: 1.1rem;
        }

        .section-title {
            margin: 0 0 0.2rem;
            font-size: 1.15rem;
            font-weight: 700;
            color: #0f172a;
            letter-spacing: -0.01em;
        }

        .section-subtitle {
            margin: 0;
            font-size: 0.8rem;
            color: #64748b;
        }

        /* Files grid — 1 column default, 2 on wider screens */
        .files-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 0.75rem;
        }

        @media (min-width: 640px) {
            .files-grid {
                grid-template-columns: 1fr 1fr;
            }
        }

        /* File card */
        .file-card {
            display: flex;
            align-items: center;
            gap: 0.875rem;
            padding: 0.875rem 1rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background: #fafbfc;
            transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
        }

        .file-card:hover {
            border-color: #667eea;
            background: #f5f8ff;
            box-shadow: 0 2px 8px rgba(102, 126, 234, 0.1);
        }

        .file-card.featured {
            border-color: #667eea;
            background: linear-gradient(to right, #f5f8ff, #fafbff);
        }

        .card-icon-col { flex-shrink: 0; }

        .icon-circle {
            width: 2.25rem;
            height: 2.25rem;
            border-radius: 8px;
            background: #ede9fe;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .icon-circle i {
            font-size: 1rem;
            color: #667eea;
        }

        .card-body {
            flex: 1;
            min-width: 0;
        }

        .card-meta {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 0.25rem;
            flex-wrap: wrap;
        }

        .category-badge {
            font-size: 0.7rem;
            font-weight: 600;
            background: #ede9fe;
            color: #5b21b6;
            padding: 0.1rem 0.45rem;
            border-radius: 4px;
            text-transform: uppercase;
            letter-spacing: 0.03em;
        }

        .file-size {
            font-size: 0.7rem;
            color: #94a3b8;
        }

        .file-title {
            margin: 0 0 0.15rem;
            font-size: 0.875rem;
            font-weight: 600;
            color: #1e293b;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .file-description {
            margin: 0;
            font-size: 0.78rem;
            color: #64748b;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .card-action { flex-shrink: 0; }

        .download-btn {
            display: inline-flex;
            align-items: center;
            gap: 0.35rem;
            padding: 0.4rem 0.85rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 6px;
            font-size: 0.8rem;
            font-weight: 600;
            text-decoration: none;
            transition: opacity 0.2s, transform 0.15s;
            white-space: nowrap;
        }

        .download-btn:hover {
            opacity: 0.9;
            transform: translateY(-1px);
            color: white;
        }

        .download-btn i { font-size: 0.75rem; }
    `]
})
export class DownloadableFilesSectionComponent implements OnInit, OnChanges {
    @Input({ required: true }) entityType!: string;
    @Input({ required: true }) entityId!: string;

    private service = inject(DownloadableFilesService);
    private translate = inject(TranslateService);

    files = signal<DownloadableFileDto[]>([]);
    loading = signal(false);

    readonly formatFileSize = formatFileSize;

    ngOnInit() {
        if (this.entityId) this.loadFiles();
    }

    ngOnChanges(changes: SimpleChanges) {
        if ((changes['entityId'] || changes['entityType']) && this.entityId) {
            this.loadFiles();
        }
    }

    loadFiles() {
        this.loading.set(true);
        this.service.listPublic(this.entityType, this.entityId).subscribe({
            next: (files) => { this.files.set(files); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    getCategoryIcon(category: string): string {
        return categoryIcon(category);
    }

    getCategoryLabel(category: string): string {
        const opt = DOWNLOAD_CATEGORIES.find(c => c.value === category);
        return opt ? this.translate.instant(opt.labelKey) : category;
    }

    getDownloadUrl(id: string): string {
        return this.service.downloadUrl(id);
    }
}
