import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService } from 'primeng/dynamicdialog';
import { HeroTickerApiService, HeroTickerItemDto } from '../../../shared/services/hero-ticker-api.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { HeroTickerFormDialogComponent } from './hero-ticker-form-dialog.component';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-hero-ticker-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
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
        <div class="hero-ticker-list-page">
            <div class="page-header">
                <h1>{{ 'admin.heroTicker.title' | translate }}</h1>
                <p-button
                    [label]="'admin.heroTicker.addImage' | translate"
                    icon="pi pi-plus"
                    routerLink="/admin/hero-ticker/new"
                ></p-button>
            </div>

            <p-table
                [value]="items"
                [loading]="loading"
                styleClass="p-datatable-sm p-datatable-striped"
                [tableStyle]="{ 'min-width': '40rem' }"
            >
                <ng-template pTemplate="header">
                    <tr>
                        <th style="width:4rem">{{ 'admin.heroTicker.order' | translate }}</th>
                        <th style="width:8rem">{{ 'admin.heroTicker.image' | translate }}</th>
                        <th>{{ 'admin.heroTicker.titleOptional' | translate }}</th>
                        <th style="width:6rem">{{ 'common.status' | translate }}</th>
                        <th style="width:12rem">{{ 'common.actions' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-item let-i="rowIndex">
                    <tr>
                        <td>
                            <div class="flex align-items-center gap-1">
                                <p-button icon="pi pi-arrow-up" [rounded]="true" [text]="true" [disabled]="i === 0" (click)="moveUp(i)" pTooltip="Move up"></p-button>
                                <p-button icon="pi pi-arrow-down" [rounded]="true" [text]="true" [disabled]="i === items.length - 1" (click)="moveDown(i)" pTooltip="Move down"></p-button>
                                <span class="sort-num">{{ item.sortOrder }}</span>
                            </div>
                        </td>
                        <td>
                            <img [src]="getImageUrl(item.imageUrl)" alt="" class="thumb" onerror="this.src='assets/img/defult.png'" />
                        </td>
                        <td>{{ item.title || '—' }}</td>
                        <td>
                            <p-tag [value]="item.isActive ? ('common.active' | translate) : ('common.inactive' | translate)" [severity]="item.isActive ? 'success' : 'secondary'" />
                        </td>
                        <td>
                            <div class="action-buttons">
                                <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="secondary" (click)="edit(item)" [pTooltip]="'common.edit' | translate"></p-button>
                                <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" (click)="deleteItem(item)" [pTooltip]="'common.delete' | translate"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                    <tr><td colspan="5">{{ 'admin.heroTicker.noImages' | translate }}</td></tr>
                </ng-template>
            </p-table>
        </div>
    `,
    styles: [`
        .hero-ticker-list-page { padding: 1rem; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page-header h1 { margin: 0; font-size: 1.5rem; }
        .thumb { width: 80px; height: 44px; object-fit: cover; border-radius: 6px; }
        .action-buttons { display: flex; gap: 0.25rem; }
        .sort-num { min-width: 1.5rem; display: inline-block; }
    `]
})
export class HeroTickerListComponent implements OnInit {
    private heroTickerApi = inject(HeroTickerApiService);
    private messageService = inject(MessageService);
    private dialogService = inject(DialogService);
    private translate = inject(TranslateService);

    items: HeroTickerItemDto[] = [];
    loading = false;

    ngOnInit() {
        this.loadItems();
    }

    getImageUrl(url: string): string {
        if (!url) return 'assets/img/defult.png';
        if (url.startsWith('http')) return url;
        const base = (environment.apiUrl || '').replace(/\/api\/v1\/?$/, '').replace(/\/api\/?$/, '') || '';
        const path = url.startsWith('/') ? url : '/' + url;
        return base ? `${base}${path}` : url;
    }

    loadItems() {
        this.loading = true;
        this.heroTickerApi.getAdmin().subscribe({
            next: (list) => {
                this.items = list ?? [];
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                const msg = err?.status === 401 ? this.translate.instant('messages.unauthorized') || 'Please sign in.' : (err?.error?.message || err?.message || this.translate.instant('common.error'));
                this.messageService.add({ severity: 'error', summary: '', detail: String(msg) });
            }
        });
    }

    openFormDialog(item?: HeroTickerItemDto) {
        const ref = this.dialogService.open(HeroTickerFormDialogComponent, {
            header: item ? this.translate.instant('admin.heroTicker.editImage') : this.translate.instant('admin.heroTicker.addImage'),
            width: '500px',
            data: { item: item ?? null }
        });
        ref?.onClose.subscribe((reload: boolean) => {
            if (reload) {
                this.loadItems();
                this.messageService.add({ severity: 'success', summary: '', detail: String(this.translate.instant('common.saved') || 'Saved') });
            }
        });
    }

    edit(item: HeroTickerItemDto) {
        this.openFormDialog(item);
    }

    deleteItem(item: HeroTickerItemDto) {
        const ref = this.dialogService.open(ConfirmDialogComponent, {
            data: { message: this.translate.instant('messages.confirmDelete'), title: this.translate.instant('common.confirm') },
            header: this.translate.instant('common.confirm')
        });
        ref?.onClose.subscribe((confirmed: boolean) => {
            if (confirmed) {
                this.heroTickerApi.delete(item.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: '', detail: String(this.translate.instant('messages.deleteSuccess') || 'Deleted') });
                        this.loadItems();
                    }
                });
            }
        });
    }

    moveUp(index: number) {
        if (index <= 0) return;
        const copy = [...this.items];
        [copy[index - 1], copy[index]] = [copy[index], copy[index - 1]];
        this.applyOrder(copy);
    }

    moveDown(index: number) {
        if (index >= this.items.length - 1) return;
        const copy = [...this.items];
        [copy[index], copy[index + 1]] = [copy[index + 1], copy[index]];
        this.applyOrder(copy);
    }

    private applyOrder(ordered: HeroTickerItemDto[]) {
        const updates = ordered.map((item, i) => ({ id: item.id, sortOrder: i }));
        this.heroTickerApi.reorder(updates).subscribe({
            next: () => {
                this.loadItems();
                this.messageService.add({ severity: 'success', summary: '', detail: String(this.translate.instant('common.saved') || 'Order updated') });
            }
        });
    }
}
