import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService, Language } from '../../services/language.service';
import { MenuItem } from 'primeng/api';

@Component({
    selector: 'app-language-switcher',
    standalone: true,
    imports: [CommonModule, ButtonModule, MenuModule, TooltipModule, TranslateModule],
    template: `
        <button 
            type="button" 
            class="layout-topbar-action"
            [pTooltip]="languageService.currentLanguage.nativeName"
            tooltipPosition="bottom"
            (click)="menu.toggle($event)"
        >
            <i class="pi pi-globe"></i>
            <span class="ml-2 hidden lg:inline">{{ languageService.currentLanguage.nativeName }}</span>
        </button>
        <p-menu #menu [model]="languageMenuItems" [popup]="true" appendTo="body" />
    `,
    styles: [`
        :host {
            display: flex;
            align-items: center;
        }
    `]
})
export class LanguageSwitcher {
    languageMenuItems: MenuItem[] = [];

    constructor(public languageService: LanguageService) {
        this.buildMenuItems();
    }

    private buildMenuItems(): void {
        this.languageMenuItems = this.languageService.languages.map(lang => ({
            label: lang.nativeName,
            icon: this.languageService.language === lang.code ? 'pi pi-check' : 'pi pi-globe',
            command: () => this.switchLanguage(lang.code)
        }));
    }

    switchLanguage(lang: Language): void {
        this.languageService.setLanguage(lang);
        this.buildMenuItems(); // Rebuild to update checkmark
    }
}
