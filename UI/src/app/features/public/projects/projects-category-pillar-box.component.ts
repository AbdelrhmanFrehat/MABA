import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectCategory } from '../../../shared/models/project.model';

export interface PillarPill {
    icon: string;
    labelKey: string;
}

export interface CategoryPillarConfig {
    titleKey: string;
    subtitleKey: string;
    pills: PillarPill[];
}

const PILLAR_CONFIG: Record<ProjectCategory, CategoryPillarConfig> = {
    [ProjectCategory.Robotics]: {
        titleKey: 'projects.pillars.robotics.title',
        subtitleKey: 'projects.pillars.robotics.subtitle',
        pills: [
            { icon: 'pi pi-android', labelKey: 'projects.pillars.robotics.p1' },
            { icon: 'pi pi-cog', labelKey: 'projects.pillars.robotics.p2' },
            { icon: 'pi pi-sitemap', labelKey: 'projects.pillars.robotics.p3' },
            { icon: 'pi pi-bolt', labelKey: 'projects.pillars.robotics.p4' },
            { icon: 'pi pi-signal', labelKey: 'projects.pillars.robotics.p5' },
            { icon: 'pi pi-shield', labelKey: 'projects.pillars.robotics.p6' }
        ]
    },
    [ProjectCategory.CNC]: {
        titleKey: 'projects.pillars.cnc.title',
        subtitleKey: 'projects.pillars.cnc.subtitle',
        pills: [
            { icon: 'pi pi-box', labelKey: 'projects.pillars.cnc.p1' },
            { icon: 'pi pi-sliders-h', labelKey: 'projects.pillars.cnc.p2' },
            { icon: 'pi pi-pencil', labelKey: 'projects.pillars.cnc.p3' },
            { icon: 'pi pi-microchip', labelKey: 'projects.pillars.cnc.p4' },
            { icon: 'pi pi-wrench', labelKey: 'projects.pillars.cnc.p5' },
            { icon: 'pi pi-check-circle', labelKey: 'projects.pillars.cnc.p6' }
        ]
    },
    [ProjectCategory.Embedded]: {
        titleKey: 'projects.pillars.embedded.title',
        subtitleKey: 'projects.pillars.embedded.subtitle',
        pills: [
            { icon: 'pi pi-microchip', labelKey: 'projects.pillars.embedded.p1' },
            { icon: 'pi pi-wifi', labelKey: 'projects.pillars.embedded.p2' },
            { icon: 'pi pi-clock', labelKey: 'projects.pillars.embedded.p3' },
            { icon: 'pi pi-bolt', labelKey: 'projects.pillars.embedded.p4' },
            { icon: 'pi pi-code', labelKey: 'projects.pillars.embedded.p5' },
            { icon: 'pi pi-cog', labelKey: 'projects.pillars.embedded.p6' }
        ]
    },
    [ProjectCategory.Monitoring]: {
        titleKey: 'projects.pillars.monitoring.title',
        subtitleKey: 'projects.pillars.monitoring.subtitle',
        pills: [
            { icon: 'pi pi-chart-line', labelKey: 'projects.pillars.monitoring.p1' },
            { icon: 'pi pi-desktop', labelKey: 'projects.pillars.monitoring.p2' },
            { icon: 'pi pi-bell', labelKey: 'projects.pillars.monitoring.p3' },
            { icon: 'pi pi-database', labelKey: 'projects.pillars.monitoring.p4' },
            { icon: 'pi pi-cloud', labelKey: 'projects.pillars.monitoring.p5' },
            { icon: 'pi pi-signal', labelKey: 'projects.pillars.monitoring.p6' }
        ]
    },
    [ProjectCategory.Software]: {
        titleKey: 'projects.pillars.softwareSystems.title',
        subtitleKey: 'projects.pillars.softwareSystems.subtitle',
        pills: [
            { icon: 'pi pi-globe', labelKey: 'projects.pillars.softwareSystems.p1' },
            { icon: 'pi pi-link', labelKey: 'projects.pillars.softwareSystems.p2' },
            { icon: 'pi pi-chart-bar', labelKey: 'projects.pillars.softwareSystems.p3' },
            { icon: 'pi pi-mobile', labelKey: 'projects.pillars.softwareSystems.p4' },
            { icon: 'pi pi-cog', labelKey: 'projects.pillars.softwareSystems.p5' },
            { icon: 'pi pi-palette', labelKey: 'projects.pillars.softwareSystems.p6' }
        ]
    },
    [ProjectCategory.RnD]: {
        titleKey: 'projects.pillars.rnd.title',
        subtitleKey: 'projects.pillars.rnd.subtitle',
        pills: [
            { icon: 'pi pi-lightbulb', labelKey: 'projects.pillars.rnd.p1' },
            { icon: 'pi pi-box', labelKey: 'projects.pillars.rnd.p2' },
            { icon: 'pi pi-flask', labelKey: 'projects.pillars.rnd.p3' },
            { icon: 'pi pi-search', labelKey: 'projects.pillars.rnd.p4' },
            { icon: 'pi pi-star', labelKey: 'projects.pillars.rnd.p5' },
            { icon: 'pi pi-verified', labelKey: 'projects.pillars.rnd.p6' }
        ]
    },
    [ProjectCategory.Electronics]: { titleKey: '', subtitleKey: '', pills: [] },
    [ProjectCategory.Mechanical]: { titleKey: '', subtitleKey: '', pills: [] },
    [ProjectCategory.Automation]: { titleKey: '', subtitleKey: '', pills: [] },
    [ProjectCategory.Custom]: { titleKey: '', subtitleKey: '', pills: [] }
};

@Component({
    selector: 'app-projects-category-pillar-box',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        @if (config && config.pills.length > 0) {
            <section class="pillar-section">
                <div class="container">
                    <div class="pillar-card">
                        <h2 class="pillar-title">{{ config.titleKey | translate }}</h2>
                        <p class="pillar-subtitle">{{ config.subtitleKey | translate }}</p>
                        <div class="pillar-badges">
                            @for (pill of config.pills; track pill.labelKey) {
                                <div class="pillar-badge">
                                    <i [class]="pill.icon"></i>
                                    <span>{{ pill.labelKey | translate }}</span>
                                </div>
                            }
                        </div>
                    </div>
                </div>
            </section>
        }
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --color-primary: #667eea;
        }
        .pillar-section { padding: 2rem 0; background: linear-gradient(180deg, #f8fafc 0%, #fff 100%); }
        .container { max-width: 1200px; margin: 0 auto; padding: 0 1.5rem; }
        .pillar-card {
            background: white;
            border-radius: 20px;
            padding: 2rem;
            box-shadow: 0 4px 20px rgba(0,0,0,0.06);
            border: 1px solid rgba(102, 126, 234, 0.12);
        }
        .pillar-title { font-size: 1.35rem; font-weight: 700; color: #1a1a2e; margin: 0 0 0.75rem 0; }
        .pillar-subtitle { color: #6b7280; line-height: 1.7; margin: 0 0 1.5rem 0; font-size: 0.95rem; }
        .pillar-badges { display: flex; flex-wrap: wrap; gap: 0.75rem; }
        .pillar-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.08), rgba(118, 75, 162, 0.08));
            border: 1px solid rgba(102, 126, 234, 0.15);
            border-radius: 12px;
            font-size: 0.85rem;
            font-weight: 500;
            color: #374151;
        }
        .pillar-badge i { color: var(--color-primary); }
    `]
})
export class ProjectsCategoryPillarBoxComponent {
    @Input() category!: ProjectCategory | null;

    get config(): CategoryPillarConfig | null {
        if (this.category == null) return null;
        const c = PILLAR_CONFIG[this.category];
        return c && c.pills.length > 0 ? c : null;
    }
}
