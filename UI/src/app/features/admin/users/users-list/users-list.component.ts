import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DataTableComponent, TableColumn, TableAction } from '../../../../shared/components/data-table/data-table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UsersApiService } from '../../../../shared/services/users-api.service';
import { User } from '../../../../shared/models/auth.model';

@Component({
    selector: 'app-users-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        DataTableComponent,
        ButtonModule,
        InputTextModule,
        CheckboxModule,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="users-list-container">
            <!-- Header -->
            <div class="page-header">
                <div class="header-content">
                    <div>
                        <h1>{{ 'admin.users.title' | translate }}</h1>
                    </div>
                    <div class="header-actions">
                        <p-button 
                            [label]="'common.refresh' | translate" 
                            icon="pi pi-refresh" 
                            [outlined]="true"
                            (click)="loadUsers()"
                            [loading]="loading"
                        ></p-button>
                    </div>
                </div>
            </div>

            <!-- Filters -->
            <div class="filters-section">
                <p-checkbox 
                    [(ngModel)]="filterActive"
                    binary
                    (onChange)="loadUsers()"
                    [inputId]="'filterActive'"
                ></p-checkbox>
                <label [for]="'filterActive'" class="filter-label">
                    {{ filterActive !== undefined ? ('common.active' | translate) : ('common.all' | translate) }}
                </label>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="users"
                    [columns]="columns"
                    [actions]="actions"
                    [loading]="loading"
                    [title]="'admin.users.list' | translate"
                    [showAddButton]="false"
                    [showExport]="true"
                    (onAction)="handleAction($event)"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .users-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .users-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .users-list-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
        }

        .header-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            align-items: flex-start;
        }

        @media (min-width: 768px) {
            .header-content {
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
            }
        }

        .page-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0;
        }

        @media (min-width: 768px) {
            .page-header h1 {
                font-size: 2rem;
            }
        }

        .header-actions {
            display: flex;
            gap: 0.5rem;
        }

        .filters-section {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin-bottom: 1.5rem;
            padding: 1rem;
            background: var(--surface-50);
            border-radius: 8px;
        }

        .filter-label {
            font-size: 0.9375rem;
            color: var(--text-color);
            cursor: pointer;
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .users-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }

            .filters-section {
                padding: 0.75rem;
            }
        }
    `]
})
export class UsersListComponent implements OnInit {
    users: User[] = [];
    loading = false;
    filterActive: boolean | undefined = undefined;
    searchTerm = '';

    columns: TableColumn[] = [
        { field: 'fullName', headerKey: 'admin.users.fullName', sortable: true },
        { field: 'email', headerKey: 'admin.users.email', sortable: true },
        { field: 'phone', headerKey: 'admin.users.phone', sortable: false },
        { 
            field: 'isActive', 
            headerKey: 'admin.users.isActive', 
            type: 'boolean',
            trueLabelKey: 'common.yes',
            falseLabelKey: 'common.no',
            sortable: true
        },
        { field: 'roles', headerKey: 'admin.users.roles', type: 'custom', sortable: false },
        { field: 'createdAt', headerKey: 'common.createdAt', type: 'date', sortable: true }
    ];

    actions: TableAction[] = [
        {
            icon: 'pi pi-eye',
            tooltipKey: 'common.view',
            action: 'view'
        },
        {
            icon: 'pi pi-pencil',
            tooltipKey: 'common.edit',
            action: 'edit'
        }
    ];

    constructor(
        private usersApiService: UsersApiService,
        private messageService: MessageService,
        private translateService: TranslateService
    ) {}

    ngOnInit() {
        this.loadUsers();
    }

    loadUsers() {
        this.loading = true;
        this.usersApiService.getAllUsers(this.filterActive).subscribe({
            next: (users) => {
                this.users = users;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingUsers'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }

    handleAction(event: { action: string; data: User }) {
        if (event.action === 'view' || event.action === 'edit') {
            // Navigate to edit page
            window.location.href = `/admin/users/${event.data.id}`;
        }
    }
}

