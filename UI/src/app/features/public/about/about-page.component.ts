import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
    selector: 'app-about-page',
    standalone: true,
    imports: [CommonModule, RouterModule, TranslateModule, ButtonModule],
    templateUrl: './about-page.component.html',
    styleUrl: './about-page.component.scss'
})
export class AboutPageComponent {
    languageService = inject(LanguageService);
}
