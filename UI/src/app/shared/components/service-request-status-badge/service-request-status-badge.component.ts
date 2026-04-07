import { Component, Input } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { TranslateModule } from '@ngx-translate/core';
import {
    ServiceWorkflowModule,
    ServiceWorkflowStatus,
    getWorkflowSeverity,
    getWorkflowStatusKey,
    normalizeProjectWorkflowStatus,
    normalizeCncWorkflowStatus,
    normalizeDesignCadWorkflowStatus,
    normalizeDesignWorkflowStatus,
    normalizeLaserWorkflowStatus,
    normalizePrint3dWorkflowStatus
} from '../../utils/service-request-workflow';

@Component({
    selector: 'app-service-request-status-badge',
    standalone: true,
    imports: [TagModule, TranslateModule],
    template: `<p-tag [value]="statusKey | translate" [severity]="severity"></p-tag>`
})
export class ServiceRequestStatusBadgeComponent {
    @Input({ required: true }) module!: ServiceWorkflowModule;
    @Input({ required: true }) status!: string | number | null | undefined;

    get normalizedStatus(): ServiceWorkflowStatus {
        switch (this.module) {
            case 'project':
                return normalizeProjectWorkflowStatus(this.status);
            case 'design':
                return normalizeDesignWorkflowStatus(this.status as string | null | undefined);
            case 'designCad':
                return normalizeDesignCadWorkflowStatus(this.status as string | null | undefined);
            case 'print3d':
                return normalizePrint3dWorkflowStatus(this.status as string | null | undefined);
            case 'laser':
                return normalizeLaserWorkflowStatus(this.status as never);
            case 'cnc':
                return normalizeCncWorkflowStatus(this.status as number | null | undefined);
        }
    }

    get severity() {
        return getWorkflowSeverity(this.normalizedStatus);
    }

    get statusKey(): string {
        return getWorkflowStatusKey(this.normalizedStatus);
    }
}
