import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import {
    SystemSettingsApiService,
    SystemSettingDto
} from '../../../shared/services/system-settings-api.service';

interface EditableHomeStat {
    icon: string;
    value: number;
    labelEn: string;
    labelAr: string;
}

@Component({
    selector: 'app-home-stats-settings',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CardModule,
        InputTextModule,
        InputNumberModule,
        ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="page-wrap">
            <div class="page-header">
                <h1>Home Statistics</h1>
                <p>Edit homepage counters shown under the hero section.</p>
            </div>

            <p-card>
                <div class="stats-grid">
                    <div class="stat-card" *ngFor="let stat of stats; let i = index">
                        <h3>Card {{ i + 1 }}</h3>
                        <div class="field">
                            <label>Icon (Prime icon class)</label>
                            <input pInputText [(ngModel)]="stat.icon" placeholder="pi pi-cog" />
                        </div>
                        <div class="field">
                            <label>Value</label>
                            <p-inputNumber [(ngModel)]="stat.value" [min]="0" />
                        </div>
                        <div class="field">
                            <label>Label (English)</label>
                            <input pInputText [(ngModel)]="stat.labelEn" />
                        </div>
                        <div class="field">
                            <label>Label (Arabic)</label>
                            <input pInputText [(ngModel)]="stat.labelAr" />
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <p-button label="Reset Defaults" [outlined]="true" (onClick)="setDefaults()" />
                    <p-button label="Save" [loading]="saving" (onClick)="save()" />
                </div>
            </p-card>
        </div>
    `,
    styles: [`
        .page-wrap { padding: 1rem; }
        .page-header { margin-bottom: 1rem; }
        .page-header h1 { margin: 0 0 0.25rem; }
        .page-header p { margin: 0; color: #64748b; }
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 1rem;
        }
        .stat-card {
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 1rem;
        }
        .stat-card h3 { margin: 0 0 0.75rem; font-size: 1rem; }
        .field {
            display: flex;
            flex-direction: column;
            gap: 0.4rem;
            margin-bottom: 0.75rem;
        }
        .field label {
            font-size: 0.85rem;
            font-weight: 600;
            color: #334155;
        }
        .actions {
            margin-top: 1rem;
            display: flex;
            gap: 0.5rem;
            justify-content: flex-end;
        }
    `]
})
export class HomeStatsSettingsComponent implements OnInit {
    private settingsApi = inject(SystemSettingsApiService);
    private messageService = inject(MessageService);

    saving = false;
    private settingId: string | null = null;
    readonly settingKey = 'home.statistics';

    stats: EditableHomeStat[] = [];

    ngOnInit(): void {
        this.setDefaults();
        this.load();
    }

    setDefaults(): void {
        this.stats = [
            { icon: 'pi pi-cog', value: 150, labelEn: 'Projects Completed', labelAr: 'مشروع مُنجز' },
            { icon: 'pi pi-building', value: 50, labelEn: 'Industrial Partners', labelAr: 'شريك صناعي' },
            { icon: 'pi pi-users', value: 200, labelEn: 'Active Clients', labelAr: 'عميل نشط' },
            { icon: 'pi pi-clock', value: 10, labelEn: 'Years Experience', labelAr: 'سنوات خبرة' }
        ];
    }

    private load(): void {
        this.settingsApi.getByKey(this.settingKey).subscribe({
            next: (setting: SystemSettingDto) => {
                this.settingId = setting.id;
                this.applySettingValue(setting.value);
            },
            error: () => {
                // Keep defaults when setting does not exist yet.
            }
        });
    }

    private applySettingValue(rawValue: string): void {
        try {
            const parsed = JSON.parse(rawValue) as EditableHomeStat[];
            if (Array.isArray(parsed) && parsed.length > 0) {
                this.stats = parsed.map((x) => ({
                    icon: x.icon || 'pi pi-circle',
                    value: Number(x.value ?? 0),
                    labelEn: x.labelEn || '',
                    labelAr: x.labelAr || ''
                }));
            }
        } catch {
            // Ignore invalid stored JSON and keep current values.
        }
    }

    save(): void {
        this.saving = true;
        const payload = JSON.stringify(this.stats);

        if (!this.settingId) {
            this.settingsApi.create({
                key: this.settingKey,
                value: payload,
                category: 'Home',
                dataType: 'JSON',
                isPublic: true,
                descriptionEn: 'Home page statistics cards',
                descriptionAr: 'بطاقات إحصائيات الصفحة الرئيسية'
            }).subscribe({
                next: (created) => {
                    this.settingId = created.id;
                    this.saving = false;
                    this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Home statistics updated.' });
                },
                error: () => {
                    this.saving = false;
                    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save settings.' });
                }
            });
            return;
        }

        this.settingsApi.update(this.settingId, {
            value: payload,
            category: 'Home',
            isPublic: true
        }).subscribe({
            next: () => {
                this.saving = false;
                this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'Home statistics updated.' });
            },
            error: () => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save settings.' });
            }
        });
    }
}
