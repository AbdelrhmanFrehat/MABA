import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService } from 'primeng/dynamicdialog';
import { AppAnnouncementsApiService, AppAnnouncementDto } from '../../../shared/services/app-announcements-api.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AppAnnouncementFormDialogComponent } from './app-announcement-form-dialog.component';

@Component({
    selector: 'app-announcements-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        TagModule,
        ToastModule,
        TooltipModule,
        TranslateModule
    ],
    providers: [MessageService, DialogService],
    template: `
        <p-toast />
        <div class="list-page">
            <div class="page-header">
                <h1>{{ 'admin.appAnnouncements.title' | translate }}</h1>
                <p-button
                    [label]="'admin.appAnnouncements.add' | translate"
                    icon="pi pi-plus"
                    (click)="openForm(null)"
                ></p-button>
            </div>

            <p-table
                [value]="items"
                [loading]="loading"
                styleClass="p-datatable-sm p-datatable-striped"
                [tableStyle]="{ 'min-width': '50rem' }"
            >
                <ng-template pTemplate="header">
                    <tr>
                        <th style="width:4rem">{{ 'admin.appAnnouncements.order' | translate }}</th>
                        <th>{{ 'admin.appAnnouncements.message' | translate }}</th>
                        <th style="width:8rem">{{ 'admin.appAnnouncements.type' | translate }}</th>
                        <th style="width:8rem">{{ 'admin.appAnnouncements.platform' | translate }}</th>
                        <th style="width:10rem">{{ 'admin.appAnnouncements.schedule' | translate }}</th>
                        <th style="width:6rem">{{ 'common.status' | translate }}</th>
                        <th style="width:10rem">{{ 'common.actions' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-item>
                    <tr>
                        <td class="text-center">{{ item.displayOrder }}</td>
                        <td class="msg-cell">{{ item.message }}</td>
                        <td>
                            <p-tag *ngIf="item.type" [value]="item.type" severity="info" />
                            <span *ngIf="!item.type" class="text-color-secondary">—</span>
                        </td>
                        <td>
                            <p-tag
                                [value]="item.targetPlatform"
                                [severity]="item.targetPlatform === 'Desktop' ? 'warn' : item.targetPlatform === 'Web' ? 'info' : 'secondary'"
                            />
                        </td>
                        <td class="schedule-cell">
                            <span *ngIf="item.startsAt || item.endsAt" class="text-sm">
                                {{ item.startsAt ? (item.startsAt | date:'short') : '∞' }}
                                →
                                {{ item.endsAt ? (item.endsAt | date:'short') : '∞' }}
                            </span>
                            <span *ngIf="!item.startsAt && !item.endsAt" class="text-color-secondary text-sm">{{ 'admin.appAnnouncements.always' | translate }}</span>
                        </td>
                        <td>
                            <p-tag
                                [value]="item.isActive ? ('common.active' | translate) : ('common.inactive' | translate)"
                                [severity]="item.isActive ? 'success' : 'secondary'"
                            />
                        </td>
                        <td>
                            <div class="action-buttons">
                                <p-button
                                    icon="pi pi-pencil"
                                    [rounded]="true" [text]="true" severity="secondary"
                                    (click)="openForm(item)"
                                    [pTooltip]="'common.edit' | translate"
                                ></p-button>
                                <p-button
                                    [icon]="item.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'"
                                    [rounded]="true" [text]="true"
                                    [severity]="item.isActive ? 'warn' : 'success'"
                                    (click)="toggleItem(item)"
                                    [pTooltip]="(item.isActive ? 'common.deactivate' : 'common.activate') | translate"
                                ></p-button>
                                <p-button
                                    icon="pi pi-trash"
                                    [rounded]="true" [text]="true" severity="danger"
                                    (click)="deleteItem(item)"
                                    [pTooltip]="'common.delete' | translate"
                                ></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                    <tr><td colspan="7" class="text-center p-4">{{ 'admin.appAnnouncements.noItems' | translate }}</td></tr>
                </ng-template>
            </p-table>
        </div>
    `,
    styles: [`
        .list-page { padding: 1rem; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; }
        .action-buttons { display: flex; gap: 0.25rem; }
        .msg-cell { max-width: 28rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .schedule-cell { font-size: 0.8rem; }
    `]
})
export class AppAnnouncementsListComponent implements OnInit {
    items: AppAnnouncementDto[] = [];
    loading = false;

    private announcementsApi = inject(AppAnnouncementsApiService);
    private messageService = inject(MessageService);
    private dialogService = inject(DialogService);
    private translate = inject(TranslateService);

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading = true;
        this.announcementsApi.getAdmin().subscribe({
            next: (items) => {
                this.items = items;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('messages.loadError'),
                    life: 5000
                });
            }
        });
    }

    openForm(item: AppAnnouncementDto | null): void {
        const ref = this.dialogService.open(AppAnnouncementFormDialogComponent, {
            header: item
                ? this.translate.instant('admin.appAnnouncements.editTitle')
                : this.translate.instant('admin.appAnnouncements.addTitle'),
            width: '520px',
            closable: true,
            closeOnEscape: true,
            data: { item }
        });
        if (!ref) {
            return;
        }

        ref.onClose.subscribe((saved) => {
            if (saved) this.load();
        });
    }

    toggleItem(item: AppAnnouncementDto): void {
        this.announcementsApi.toggle(item.id).subscribe({
            next: (updated) => {
                const idx = this.items.findIndex(x => x.id === item.id);
                if (idx !== -1) this.items[idx] = updated;
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('messages.success'),
                    detail: this.translate.instant('messages.itemUpdatedSuccessfully'),
                    life: 3000
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('messages.error'),
                    detail: this.translate.instant('messages.saveError'),
                    life: 5000
                });
            }
        });
    }

    deleteItem(item: AppAnnouncementDto): void {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            header: this.translate.instant('common.confirm'),
            width: '380px',
            data: { message: this.translate.instant('admin.appAnnouncements.confirmDelete') }
        });
        if (!ref) {
            return;
        }

        ref.onClose.subscribe((confirmed) => {
            if (!confirmed) return;
            this.announcementsApi.delete(item.id).subscribe({
                next: () => {
                    this.items = this.items.filter(x => x.id !== item.id);
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translate.instant('messages.success'),
                        detail: this.translate.instant('admin.appAnnouncements.deleted'),
                        life: 3000
                    });
                },
                error: () => {
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translate.instant('messages.error'),
                        detail: this.translate.instant('messages.deleteError'),
                        life: 5000
                    });
                }
            });
        });
    }
}
