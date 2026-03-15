import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, TableColumn } from '../../../shared/components/data-table/data-table';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RolesApiService } from '../../../shared/services/roles-api.service';
import { Permission } from '../../../shared/models/role.model';

@Component({
    selector: 'app-permissions-list',
    standalone: true,
    imports: [
        CommonModule,
        DataTableComponent,
        ToastModule,
        TranslateModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="permissions-list-container">
            <!-- Header -->
            <div class="page-header">
                <div>
                    <h1>{{ 'admin.permissions.title' | translate }}</h1>
                    <p class="page-description">{{ 'admin.permissions.description' | translate }}</p>
                </div>
            </div>

            <!-- Data Table -->
            <div class="table-section">
                <app-data-table
                    [data]="permissions"
                    [columns]="columns"
                    [loading]="loading"
                    [title]="'admin.permissions.list' | translate"
                    [showAddButton]="false"
                    [showDeleteSelected]="false"
                ></app-data-table>
            </div>
        </div>
    `,
    styles: [`
        .permissions-list-container {
            width: 100%;
            max-width: 100%;
            padding: 0.5rem;
            box-sizing: border-box;
        }

        @media (min-width: 768px) {
            .permissions-list-container {
                padding: 1rem;
            }
        }

        @media (min-width: 1024px) {
            .permissions-list-container {
                padding: 1.5rem;
            }
        }

        .page-header {
            margin-bottom: 1.5rem;
        }

        .page-header h1 {
            font-size: 1.5rem;
            font-weight: bold;
            margin: 0 0 0.5rem 0;
        }

        @media (min-width: 768px) {
            .page-header h1 {
                font-size: 2rem;
            }
        }

        .page-description {
            color: var(--text-color-secondary);
            font-size: 0.875rem;
            margin: 0;
        }

        @media (min-width: 768px) {
            .page-description {
                font-size: 1rem;
            }
        }

        .table-section {
            width: 100%;
        }

        /* Mobile optimizations */
        @media (max-width: 575px) {
            .permissions-list-container {
                padding: 0.5rem;
            }

            .page-header h1 {
                font-size: 1.25rem;
            }
        }
    `]
})
export class PermissionsListComponent implements OnInit {
    permissions: Permission[] = [];
    loading = false;

    private rolesApiService = inject(RolesApiService);
    private messageService = inject(MessageService);
    private translateService = inject(TranslateService);

    columns: TableColumn[] = [
        { field: 'key', headerKey: 'admin.permissions.key', sortable: true },
        { field: 'name', headerKey: 'admin.permissions.name', sortable: true },
        { field: 'createdAt', headerKey: 'common.createdAt', type: 'date', sortable: true }
    ];

    ngOnInit() {
        this.loadPermissions();
    }

    loadPermissions() {
        this.loading = true;
        this.rolesApiService.getAllPermissions().subscribe({
            next: (permissions) => {
                this.permissions = permissions;
                this.loading = false;
            },
            error: (error) => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translateService.instant('messages.error'),
                    detail: this.translateService.instant('messages.errorLoadingPermissions'),
                    life: 5000
                });
                this.loading = false;
            }
        });
    }
}

