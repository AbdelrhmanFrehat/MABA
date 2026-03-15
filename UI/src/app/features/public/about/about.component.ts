import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-about',
    standalone: true,
    imports: [CommonModule, TranslateModule],
    template: `
        <div class="about-page">
            <div class="about-header">
                <h1>{{ 'about.title' | translate }}</h1>
                <p>{{ 'about.subtitle' | translate }}</p>
            </div>
            <div class="about-content">
                <section class="about-section">
                    <h2>{{ 'about.mission.title' | translate }}</h2>
                    <p>{{ 'about.mission.description' | translate }}</p>
                </section>
                <section class="about-section">
                    <h2>{{ 'about.vision.title' | translate }}</h2>
                    <p>{{ 'about.vision.description' | translate }}</p>
                </section>
            </div>
        </div>
    `,
    styles: [`
        .about-page {
            max-width: 1200px;
            margin: 0 auto;
            padding: 2rem;
        }
        .about-header {
            text-align: center;
            margin-bottom: 3rem;
            padding: 3rem 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 16px;
            color: white;
        }
        .about-header h1 {
            font-size: 2.5rem;
            margin: 0 0 1rem 0;
        }
        .about-header p {
            font-size: 1.25rem;
            opacity: 0.9;
            margin: 0;
        }
        .about-content {
            display: grid;
            gap: 2rem;
        }
        .about-section {
            background: var(--surface-card);
            padding: 2rem;
            border-radius: 12px;
            border: 1px solid var(--surface-border);
        }
        .about-section h2 {
            color: var(--text-color);
            margin: 0 0 1rem 0;
        }
        .about-section p {
            color: var(--text-color-secondary);
            line-height: 1.7;
            margin: 0;
        }
    `]
})
export class AboutComponent {}
