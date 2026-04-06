import { Component, Input, Output, EventEmitter, ViewChild, ContentChild, TemplateRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Table, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputIconModule } from 'primeng/inputicon';
import { IconFieldModule } from 'primeng/iconfield';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

export interface TableColumn {
    field: string;
    header?: string;
    headerKey?: string; // Translation key for header
    sortable?: boolean;
    type?: 'text' | 'number' | 'currency' | 'date' | 'boolean' | 'custom';
    width?: string;
    align?: 'left' | 'center' | 'right';
    currencyCode?: string;
    dateFormat?: string;
    trueLabel?: string;
    trueLabelKey?: string; // Translation key for true label
    falseLabel?: string;
    falseLabelKey?: string; // Translation key for false label
}

export interface TableAction {
    icon: string;
    tooltip?: string;
    tooltipKey?: string; // Translation key for tooltip
    action: string;
    severity?: 'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'danger';
    visible?: (row: any) => boolean;
}

@Component({
    selector: 'app-data-table',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        InputIconModule,
        IconFieldModule,
        TooltipModule,
        TagModule,
        ToolbarModule,
        ConfirmDialogModule,
        ToastModule,
        TranslateModule
    ],
    providers: [ConfirmationService, MessageService],
    template: `
        <p-toast />
        <p-toolbar styleClass="mb-4" *ngIf="showToolbar">
            <ng-template #start>
                <p-button 
                    *ngIf="showAddButton"
                    [label]="addButtonLabel || ('common.new' | translate)" 
                    icon="pi pi-plus" 
                    severity="primary" 
                    class="mr-2" 
                    (onClick)="onAdd.emit()" 
                />
                <p-button 
                    *ngIf="showDeleteSelected && selectedItems?.length"
                    severity="danger" 
                    [label]="'common.delete' | translate" 
                    icon="pi pi-trash" 
                    outlined 
                    (onClick)="confirmDeleteSelected()" 
                />
            </ng-template>

            <ng-template #end>
                <p-button 
                    *ngIf="showExport"
                    [label]="'common.export' | translate" 
                    icon="pi pi-download" 
                    severity="secondary"
                    outlined
                    (onClick)="exportCSV()" 
                />
            </ng-template>
        </p-toolbar>

        <p-table
            #dt
            [value]="data"
            [rows]="rows"
            [paginator]="paginator"
            [globalFilterFields]="globalFilterFields"
            [tableStyle]="{ 'min-width': '75rem' }"
            [(selection)]="selectedItems"
            [rowHover]="true"
            [dataKey]="dataKey"
            [currentPageReportTemplate]="pageReportTemplate || ('common.showing' | translate)"
            [showCurrentPageReport]="true"
            [rowsPerPageOptions]="rowsPerPageOptions"
            [loading]="loading"
            [lazy]="lazy"
            (onLazyLoad)="onLazyLoad.emit($event)"
            [totalRecords]="totalRecords"
        >
            <ng-template #caption *ngIf="showCaption">
                <div class="flex items-center justify-between">
                    <h5 class="m-0">{{ title }}</h5>
                    <p-iconfield *ngIf="showSearch">
                        <p-inputicon styleClass="pi pi-search" />
                        <input 
                            pInputText 
                            type="text" 
                            [(ngModel)]="searchValue"
                            (input)="onSearch($event)" 
                            [placeholder]="'common.search' | translate" 
                        />
                    </p-iconfield>
                </div>
            </ng-template>

            <ng-template #header>
                <tr>
                    <th *ngIf="selectable" style="width: 3rem">
                        <p-tableHeaderCheckbox />
                    </th>
                    <th 
                        *ngFor="let col of columns" 
                        [pSortableColumn]="col.sortable ? col.field : undefined"
                        [style.width]="col.width"
                        [style.text-align]="col.align || 'left'"
                    >
                        {{ col.headerKey ? (col.headerKey | translate) : col.header }}
                        <p-sortIcon *ngIf="col.sortable" [field]="col.field" />
                    </th>
                    <th *ngIf="actions?.length" style="min-width: 12rem; text-align: center">{{ 'common.actions' | translate }}</th>
                </tr>
            </ng-template>

            <ng-template #body let-row>
                <tr>
                    <td *ngIf="selectable" style="width: 3rem">
                        <p-tableCheckbox [value]="row" />
                    </td>
                    <td 
                        *ngFor="let col of columns"
                        [style.text-align]="col.align || 'left'"
                    >
                        <ng-container [ngSwitch]="col.type">
                            <span *ngSwitchCase="'currency'">
                                {{ row[col.field] | currency: (col.currencyCode || 'USD') }}
                            </span>
                            <span *ngSwitchCase="'date'">
                                {{ row[col.field] | date: (col.dateFormat || 'mediumDate') }}
                            </span>
                            <span *ngSwitchCase="'boolean'">
                                <p-tag 
                                    [value]="row[col.field] 
                                        ? (col.trueLabelKey ? (col.trueLabelKey | translate) : (col.trueLabel || ('common.yes' | translate))) 
                                        : (col.falseLabelKey ? (col.falseLabelKey | translate) : (col.falseLabel || ('common.no' | translate)))" 
                                    [severity]="row[col.field] ? 'success' : 'danger'" 
                                />
                            </span>
                            <ng-container *ngSwitchCase="'custom'">
                                <ng-container 
                                    *ngTemplateOutlet="customColumnTemplate; context: { $implicit: row, column: col }"
                                ></ng-container>
                            </ng-container>
                            <span *ngSwitchDefault>{{ row[col.field] }}</span>
                        </ng-container>
                    </td>
                    <td *ngIf="actions?.length" style="text-align: center">
                        <ng-container *ngFor="let action of actions">
                            <p-button 
                                *ngIf="!action.visible || action.visible(row)"
                                [icon]="action.icon" 
                                [pTooltip]="action.tooltipKey ? (action.tooltipKey | translate) : action.tooltip"
                                tooltipPosition="top"
                                class="mr-1" 
                                [rounded]="true" 
                                [outlined]="true" 
                                [severity]="action.severity || 'primary'"
                                (onClick)="onAction.emit({ action: action.action, data: row })"
                            />
                        </ng-container>
                    </td>
                </tr>
            </ng-template>

            <ng-template #emptymessage>
                <tr>
                    <td [attr.colspan]="getColSpan()" style="text-align: center; padding: 2rem;">
                        <div class="flex flex-col items-center gap-3">
                            <i class="pi pi-inbox text-4xl text-gray-400"></i>
                            <span class="text-gray-500">{{ emptyMessageKey ? (emptyMessageKey | translate) : emptyMessage }}</span>
                        </div>
                    </td>
                </tr>
            </ng-template>
        </p-table>

        <p-confirmdialog [style]="{ width: '450px' }" />
    `
})
export class DataTableComponent implements OnInit {
    @Input() data: any[] = [];
    @Input() columns: TableColumn[] = [];
    @Input() actions: TableAction[] = [];
    @Input() title: string = '';
    @Input() dataKey: string = 'id';
    @Input() rows: number = 10;
    @Input() rowsPerPageOptions: number[] = [10, 25, 50];
    @Input() paginator: boolean = true;
    @Input() globalFilterFields: string[] = [];
    @Input() selectable: boolean = false;
    @Input() showToolbar: boolean = true;
    @Input() showAddButton: boolean = true;
    @Input() addButtonLabel: string = '';
    @Input() showDeleteSelected: boolean = true;
    @Input() showExport: boolean = true;
    @Input() showCaption: boolean = true;
    @Input() showSearch: boolean = true;
    @Input() loading: boolean = false;
    @Input() lazy: boolean = false;
    @Input() totalRecords: number = 0;
    @Input() emptyMessage: string = '';
    @Input() emptyMessageKey: string = 'common.noDataFound';
    @Input() pageReportTemplate: string = '';

    @Output() onAdd = new EventEmitter<void>();
    @Output() onAction = new EventEmitter<{ action: string; data: any }>();
    @Output() onDeleteSelected = new EventEmitter<any[]>();
    @Output() onSearchChange = new EventEmitter<string>();
    @Output() onLazyLoad = new EventEmitter<any>();

    @ContentChild('customColumn') customColumnTemplate!: TemplateRef<any>;

    @ViewChild('dt') table!: Table;

    selectedItems: any[] = [];
    searchValue: string = '';

    constructor(
        private confirmationService: ConfirmationService,
        private messageService: MessageService,
        private translateService: TranslateService
    ) {}

    ngOnInit() {}

    onSearch(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        if (this.lazy) {
            this.onSearchChange.emit(value);
        } else {
            this.table.filterGlobal(value, 'contains');
        }
    }

    exportCSV() {
        if (!this.table || !this.data?.length) {
            return;
        }

        this.table.exportCSV();
    }

    confirmDeleteSelected() {
        this.confirmationService.confirm({
            message: this.translateService.instant('messages.confirmDeleteMultiple'),
            header: this.translateService.instant('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.onDeleteSelected.emit(this.selectedItems);
                this.selectedItems = [];
            }
        });
    }

    getColSpan(): number {
        let span = this.columns.length;
        if (this.selectable) span++;
        if (this.actions?.length) span++;
        return span;
    }

    clearSelection() {
        this.selectedItems = [];
    }
}
