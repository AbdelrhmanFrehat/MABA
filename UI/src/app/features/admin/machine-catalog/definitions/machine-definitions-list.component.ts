import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { MachineCatalogService } from '../../../../shared/services/machine-catalog.service';
import { MachineCategory, MachineFamily, MachineDefinitionSummary } from '../../../../shared/models/machine-catalog.model';

@Component({
    selector: 'app-machine-definitions-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule,
        ButtonModule, TableModule, SelectModule, InputTextModule,
        CheckboxModule, TagModule, ToastModule, TooltipModule, ConfirmDialogModule
    ],
    providers: [MessageService, ConfirmationService],
    styles: [`
        .mp-header { border-inline-start: 3px solid var(--tech-primary-blue, #0066FF); padding-inline-start: 1rem; }
        .mp-badge { display: inline-flex; align-items: center; gap: 0.35rem; font-size: 0.68rem; font-weight: 800; letter-spacing: 0.1em; text-transform: uppercase; color: var(--tech-primary-blue, #0066FF); background: rgba(0,102,255,0.08); border: 1px solid rgba(0,102,255,0.2); border-radius: 4px; padding: 2px 8px; margin-bottom: 0.4rem; }
    `],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="p-4">
            <div class="flex justify-between items-center mb-4">
                <div class="mp-header">
                    <div class="mp-badge"><i class="pi pi-server" style="font-size:0.65rem"></i> Machine Platform</div>
                    <h1 class="text-2xl font-bold m-0">Machine Definitions</h1>
                    <p class="text-500 mt-1 mb-0 text-sm">Runtime-ready machine specs — the source of truth loaded by the desktop app.</p>
                </div>
                <p-button label="New Definition" icon="pi pi-plus" (onClick)="openNew()"></p-button>
            </div>

            <!-- Filters -->
            <div class="flex flex-wrap gap-3 mb-4 p-3 surface-100 border-round">
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-semibold text-500">Category</label>
                    <p-select [options]="categories" optionLabel="displayNameEn" optionValue="id"
                              [(ngModel)]="filterCategoryId" placeholder="All categories"
                              [showClear]="true" (onChange)="onCategoryFilterChange()" appendTo="body"
                              style="min-width:160px"></p-select>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-semibold text-500">Family</label>
                    <p-select [options]="filteredFamilies" optionLabel="displayNameEn" optionValue="id"
                              [(ngModel)]="filterFamilyId" placeholder="All families"
                              [showClear]="true" appendTo="body"
                              style="min-width:160px"></p-select>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-sm font-semibold text-500">Search</label>
                    <input pInputText [(ngModel)]="filterSearch" placeholder="Code, name…" style="width:180px" />
                </div>
                <div class="flex items-end gap-3">
                    <div class="flex items-center gap-2">
                        <p-checkbox [(ngModel)]="filterActiveOnly" [binary]="true" inputId="activeOnly"></p-checkbox>
                        <label for="activeOnly" class="text-sm cursor-pointer">Active only</label>
                    </div>
                    <div class="flex items-center gap-2">
                        <p-checkbox [(ngModel)]="filterIncludeDeprecated" [binary]="true" inputId="inclDepr"></p-checkbox>
                        <label for="inclDepr" class="text-sm cursor-pointer">Include deprecated</label>
                    </div>
                    <p-button label="Search" icon="pi pi-search" size="small" (onClick)="load()"></p-button>
                    <p-button label="Clear" icon="pi pi-times" [outlined]="true" size="small" (onClick)="clearFilters()"></p-button>
                </div>
            </div>

            <p-table [value]="rows" [loading]="loading" stripedRows>
                <ng-template pTemplate="header">
                    <tr>
                        <th>Code</th>
                        <th>Name (EN)</th>
                        <th>Ver.</th>
                        <th>Category</th>
                        <th>Family</th>
                        <th>Driver</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-row>
                    <tr>
                        <td>
                            <div class="flex flex-col">
                                <code class="text-primary font-bold text-sm">{{ row.code }}</code>
                                <span *ngIf="row.isDeprecated" class="text-xs text-orange-500 mt-1">
                                    <i class="pi pi-exclamation-triangle mr-1"></i>Deprecated
                                </span>
                            </div>
                        </td>
                        <td>
                            <div>{{ row.displayNameEn }}</div>
                            <div class="text-xs text-500">{{ row.manufacturer }}</div>
                        </td>
                        <td><span class="text-sm text-500">v{{ row.version }}</span></td>
                        <td><span class="text-sm">{{ row.categoryDisplayNameEn }}</span></td>
                        <td><span class="text-sm">{{ row.familyDisplayNameEn }}</span></td>
                        <td><span class="text-xs text-500">{{ row.defaultDriverType }}</span></td>
                        <td>
                            <div class="flex flex-col gap-1">
                                <p-tag [value]="row.isActive ? 'Active' : 'Inactive'"
                                       [severity]="row.isActive ? 'success' : 'danger'"></p-tag>
                                <p-tag *ngIf="!row.isPublic" value="Private" severity="secondary"></p-tag>
                            </div>
                        </td>
                        <td>
                            <p-button icon="pi pi-pencil" [text]="true" [rounded]="true" severity="primary"
                                      pTooltip="Edit" (onClick)="openEdit(row)"></p-button>
                            <p-button [icon]="row.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'"
                                      [text]="true" [rounded]="true"
                                      [severity]="row.isActive ? 'warn' : 'success'"
                                      [pTooltip]="row.isActive ? 'Deactivate' : 'Activate'"
                                      (onClick)="toggleActive(row)"></p-button>
                            <p-button icon="pi pi-trash" [text]="true" [rounded]="true" severity="danger"
                                      pTooltip="Delete" (onClick)="confirmDelete(row)"></p-button>
                        </td>
                    </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                    <tr><td colspan="8" class="text-center p-5 text-500">No machine definitions found.</td></tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class MachineDefinitionsListComponent implements OnInit {
    private svc = inject(MachineCatalogService);
    private msg = inject(MessageService);
    private confirm = inject(ConfirmationService);
    private router = inject(Router);

    rows: MachineDefinitionSummary[] = [];
    categories: MachineCategory[] = [];
    allFamilies: MachineFamily[] = [];
    filteredFamilies: MachineFamily[] = [];

    loading = false;
    filterCategoryId: string | null = null;
    filterFamilyId: string | null = null;
    filterSearch = '';
    filterActiveOnly = false;
    filterIncludeDeprecated = false;

    ngOnInit() {
        this.svc.getCategories(true).subscribe({ next: d => { this.categories = d; } });
        this.svc.getFamilies(undefined, true).subscribe({ next: d => { this.allFamilies = d; this.filteredFamilies = d; } });
        this.load();
    }

    onCategoryFilterChange() {
        this.filterFamilyId = null;
        this.filteredFamilies = this.filterCategoryId
            ? this.allFamilies.filter(f => f.categoryId === this.filterCategoryId)
            : this.allFamilies;
    }

    load() {
        this.loading = true;
        this.svc.getDefinitions({
            categoryId: this.filterCategoryId || undefined,
            familyId: this.filterFamilyId || undefined,
            activeOnly: this.filterActiveOnly || undefined,
            includeDeprecated: this.filterIncludeDeprecated || undefined,
            search: this.filterSearch || undefined
        }).subscribe({
            next: d => { this.rows = d; this.loading = false; },
            error: () => { this.loading = false; this.err('Failed to load definitions.'); }
        });
    }

    clearFilters() {
        this.filterCategoryId = null;
        this.filterFamilyId = null;
        this.filterSearch = '';
        this.filterActiveOnly = false;
        this.filterIncludeDeprecated = false;
        this.filteredFamilies = this.allFamilies;
        this.load();
    }

    openNew() {
        this.router.navigate(['/admin/machine-catalog/definitions/new']);
    }

    openEdit(row: MachineDefinitionSummary) {
        this.router.navigate(['/admin/machine-catalog/definitions', row.id, 'edit']);
    }

    toggleActive(row: MachineDefinitionSummary) {
        this.svc.patchStatus(row.id, { isActive: !row.isActive }).subscribe({
            next: () => { this.load(); this.msg.add({ severity: 'info', summary: 'Updated', detail: `Definition ${row.isActive ? 'deactivated' : 'activated'}.` }); },
            error: () => this.err('Failed to update status.')
        });
    }

    confirmDelete(row: MachineDefinitionSummary) {
        this.confirm.confirm({
            message: `Delete definition <strong>${row.code}</strong>? This cannot be undone.`,
            header: 'Confirm Delete',
            icon: 'pi pi-trash',
            accept: () => {
                this.svc.deleteDefinition(row.id).subscribe({
                    next: () => { this.load(); this.msg.add({ severity: 'success', summary: 'Deleted', detail: 'Definition deleted.' }); },
                    error: () => this.err('Failed to delete definition.')
                });
            }
        });
    }

    private err(msg: string) {
        this.msg.add({ severity: 'error', summary: 'Error', detail: msg });
    }
}
