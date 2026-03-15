import { Injectable, signal, effect, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';

export type Language = 'ar' | 'en';

export interface LanguageOption {
    code: Language;
    name: string;
    nativeName: string;
    dir: 'ltr' | 'rtl';
}

@Injectable({
    providedIn: 'root'
})
export class LanguageService {
    private currentLang = signal<Language>('en');
    private isBrowser: boolean;

    // Available languages
    readonly languages: LanguageOption[] = [
        { code: 'en', name: 'English', nativeName: 'English', dir: 'ltr' },
        { code: 'ar', name: 'Arabic', nativeName: 'العربية', dir: 'rtl' }
    ];

    get language(): Language {
        return this.currentLang();
    }

    get languageSignal() {
        return this.currentLang;
    }

    get currentLanguage(): LanguageOption {
        return this.languages.find(l => l.code === this.currentLang()) || this.languages[0];
    }

    get isRTL(): boolean {
        return this.currentLanguage.dir === 'rtl';
    }

    get direction(): 'ltr' | 'rtl' {
        return this.currentLanguage.dir;
    }

    constructor(
        private translateService: TranslateService,
        @Inject(DOCUMENT) private document: Document,
        @Inject(PLATFORM_ID) private platformId: Object
    ) {
        this.isBrowser = isPlatformBrowser(this.platformId);
        
        // Set available languages
        this.translateService.addLangs(['en', 'ar']);
        this.translateService.setDefaultLang('en');

        // Initialize language from storage or browser
        this.initializeLanguage();

        // Effect to update DOM when language changes
        effect(() => {
            const lang = this.currentLang();
            this.applyLanguageToDOM(lang);
        });
    }

    private initializeLanguage(): void {
        let savedLang: Language | null = null;

        if (this.isBrowser) {
            savedLang = localStorage.getItem('app_language') as Language;
        }

        if (savedLang && this.languages.some(l => l.code === savedLang)) {
            this.setLanguage(savedLang);
        } else {
            // Try to detect browser language
            const browserLang = this.translateService.getBrowserLang() as Language;
            if (browserLang && this.languages.some(l => l.code === browserLang)) {
                this.setLanguage(browserLang);
            } else {
                this.setLanguage('en');
            }
        }
    }

    setLanguage(lang: Language): void {
        if (!this.languages.some(l => l.code === lang)) {
            console.warn(`Language ${lang} is not supported. Falling back to English.`);
            lang = 'en';
        }

        this.currentLang.set(lang);
        this.translateService.use(lang);

        if (this.isBrowser) {
            localStorage.setItem('app_language', lang);
        }
    }

    toggleLanguage(): void {
        const newLang = this.currentLang() === 'en' ? 'ar' : 'en';
        this.setLanguage(newLang);
    }

    private applyLanguageToDOM(lang: Language): void {
        if (!this.isBrowser) return;

        const langOption = this.languages.find(l => l.code === lang);
        if (!langOption) return;

        // Update HTML attributes
        this.document.documentElement.lang = lang;
        this.document.documentElement.dir = langOption.dir;

        // Update body classes for RTL/LTR
        const body = this.document.body;
        if (langOption.dir === 'rtl') {
            body.classList.add('rtl');
            body.classList.remove('ltr');
        } else {
            body.classList.add('ltr');
            body.classList.remove('rtl');
        }
    }

    /**
     * Gets the localized name based on current language
     */
    getLocalizedName(item: { nameAr?: string; nameEn?: string } | null | undefined): string {
        if (!item) return '';
        return this.currentLang() === 'ar' ? (item.nameAr || item.nameEn || '') : (item.nameEn || item.nameAr || '');
    }

    /**
     * Gets the localized property name suffix
     */
    getNameField(): 'nameAr' | 'nameEn' {
        return this.currentLang() === 'ar' ? 'nameAr' : 'nameEn';
    }

    /**
     * Translate a key
     */
    translate(key: string, params?: object): string {
        return this.translateService.instant(key, params);
    }

    /**
     * Get translation observable
     */
    get$(key: string, params?: object) {
        return this.translateService.get(key, params);
    }

    /**
     * Stream translation changes
     */
    stream$(key: string, params?: object) {
        return this.translateService.stream(key, params);
    }
}
