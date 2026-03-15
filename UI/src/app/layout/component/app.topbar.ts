import { Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';
import { LanguageSwitcher } from '../../shared/components/language-switcher/language-switcher';
import { AuthService } from '../../shared/services/auth.service';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [RouterModule, CommonModule, StyleClassModule, TranslateModule, ButtonModule, MenuModule, AppConfigurator, LanguageSwitcher],
    template: ` <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                <i class="pi pi-bars"></i>
            </button>
            <a class="layout-topbar-logo" routerLink="/">
                <img src="assets/img/logo.jpeg" alt="MABA Logo" style="height: 36px; width: auto; border-radius: 6px;" />
                <span>MABA</span>
            </a>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
                    <i [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
                </button>
                
                <!-- Language Switcher -->
                <app-language-switcher />
                
                <div class="relative">
                    <button
                        class="layout-topbar-action layout-topbar-action-highlight"
                        pStyleClass="@next"
                        enterFromClass="hidden"
                        enterActiveClass="animate-scalein"
                        leaveToClass="hidden"
                        leaveActiveClass="animate-fadeout"
                        [hideOnOutsideClick]="true"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <app-configurator />
                </div>
            </div>

            <button class="layout-topbar-menu-button layout-topbar-action" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true">
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    @if (authService.authenticated) {
                        <!-- User Info -->
                        <div class="flex items-center gap-2 px-3">
                            <span class="text-surface-600 dark:text-surface-300 text-sm">
                                {{ authService.user?.fullName || authService.user?.email }}
                            </span>
                            <span class="text-xs px-2 py-1 rounded-full bg-primary text-primary-contrast">
                                {{ authService.userRole }}
                            </span>
                        </div>
                        <button type="button" class="layout-topbar-action" (click)="logout()">
                            <i class="pi pi-sign-out"></i>
                            <span>{{ 'auth.logout' | translate }}</span>
                        </button>
                    } @else {
                        <button type="button" class="layout-topbar-action" routerLink="/auth/login">
                            <i class="pi pi-sign-in"></i>
                            <span>{{ 'auth.login' | translate }}</span>
                        </button>
                    }
                </div>
            </div>
        </div>
    </div>`
})
export class AppTopbar {
    items!: MenuItem[];

    constructor(
        public layoutService: LayoutService,
        public authService: AuthService
    ) {}

    toggleDarkMode() {
        this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }

    logout() {
        this.authService.logout();
    }
}
