import { TranslateService } from '@ngx-translate/core';
import { CncServiceRequestStatus } from '../models/cnc.model';
import { LaserServiceRequestStatus } from '../models/laser.model';
import { Print3dRequestStatus } from '../models/printing.model';

export type ServiceWorkflowStatus =
    | 'New'
    | 'UnderReview'
    | 'AwaitingCustomerConfirmation'
    | 'Approved'
    | 'InProgress'
    | 'ReadyForDelivery'
    | 'Completed'
    | 'Rejected'
    | 'Cancelled';

export type ServiceWorkflowModule = 'project' | 'design' | 'designCad' | 'print3d' | 'laser' | 'cnc';

type WorkflowOption = { value: ServiceWorkflowStatus; label: string };
type WorkflowInput = string | null | undefined;

const WORKFLOW_BY_MODULE: Record<ServiceWorkflowModule, ServiceWorkflowStatus[]> = {
    project: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'Completed', 'Cancelled'],
    design: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'ReadyForDelivery', 'Completed', 'Rejected', 'Cancelled'],
    designCad: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'Completed', 'Rejected', 'Cancelled'],
    print3d: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'Completed', 'Rejected', 'Cancelled'],
    laser: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'Completed', 'Rejected', 'Cancelled'],
    cnc: ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'Completed', 'Rejected', 'Cancelled']
};

const WORKFLOW_KEY: Record<ServiceWorkflowStatus, string> = {
    New: 'admin.serviceWorkflow.statusNew',
    UnderReview: 'admin.serviceWorkflow.statusUnderReview',
    AwaitingCustomerConfirmation: 'admin.serviceWorkflow.statusAwaitingCustomerConfirmation',
    Approved: 'admin.serviceWorkflow.statusApproved',
    InProgress: 'admin.serviceWorkflow.statusInProgress',
    ReadyForDelivery: 'admin.serviceWorkflow.statusReadyForDelivery',
    Completed: 'admin.serviceWorkflow.statusCompleted',
    Rejected: 'admin.serviceWorkflow.statusRejected',
    Cancelled: 'admin.serviceWorkflow.statusCancelled'
};

function isWorkflowStatus(status: string | null | undefined): status is ServiceWorkflowStatus {
    return !!status && [
        'New',
        'UnderReview',
        'AwaitingCustomerConfirmation',
        'Approved',
        'InProgress',
        'ReadyForDelivery',
        'Completed',
        'Rejected',
        'Cancelled'
    ].includes(status);
}

export function getWorkflowOptions(module: ServiceWorkflowModule, translate: TranslateService): WorkflowOption[] {
    return WORKFLOW_BY_MODULE[module].map((value) => ({
        value,
        label: translate.instant(WORKFLOW_KEY[value])
    }));
}

export function getWorkflowStatusKey(status: ServiceWorkflowStatus): string {
    return WORKFLOW_KEY[status];
}

export function getWorkflowSeverity(status: ServiceWorkflowStatus): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    switch (status) {
        case 'New':
            return 'warn';
        case 'UnderReview':
        case 'AwaitingCustomerConfirmation':
            return 'info';
        case 'Approved':
        case 'InProgress':
        case 'ReadyForDelivery':
            return 'contrast';
        case 'Completed':
            return 'success';
        case 'Rejected':
            return 'danger';
        case 'Cancelled':
            return 'secondary';
        default:
            return 'secondary';
    }
}

export function normalizeDesignWorkflowStatus(status: WorkflowInput): ServiceWorkflowStatus {
    if (isWorkflowStatus(status)) {
        return status;
    }
    switch (status) {
        case 'UnderReview':
            return 'UnderReview';
        case 'Quoted':
            return 'AwaitingCustomerConfirmation';
        case 'Approved':
            return 'Approved';
        case 'InProgress':
            return 'InProgress';
        case 'Delivered':
            return 'ReadyForDelivery';
        case 'Closed':
            return 'Completed';
        case 'Rejected':
            return 'Rejected';
        case 'Cancelled':
            return 'Cancelled';
        default:
            return 'New';
    }
}

export function normalizeProjectWorkflowStatus(status: string | number | null | undefined): ServiceWorkflowStatus {
    if (typeof status === 'string' && isWorkflowStatus(status)) {
        return status;
    }
    switch (status) {
        case 1:
        case 'InReview':
            return 'UnderReview';
        case 2:
        case 'Quoted':
            return 'AwaitingCustomerConfirmation';
        case 3:
        case 'InProgress':
            return 'InProgress';
        case 4:
        case 'Closed':
            return 'Completed';
        default:
            return 'New';
    }
}

export function denormalizeDesignWorkflowStatus(status: ServiceWorkflowStatus): string {
    switch (status) {
        case 'AwaitingCustomerConfirmation':
            return 'Quoted';
        case 'ReadyForDelivery':
            return 'Delivered';
        case 'Completed':
            return 'Closed';
        default:
            return status;
    }
}

export function normalizeDesignCadWorkflowStatus(status: WorkflowInput): ServiceWorkflowStatus {
    if (isWorkflowStatus(status)) {
        return status;
    }
    switch (status) {
        case 'UnderReview':
            return 'UnderReview';
        case 'Quoted':
            return 'AwaitingCustomerConfirmation';
        case 'Approved':
            return 'Approved';
        case 'Completed':
            return 'Completed';
        case 'Rejected':
            return 'Rejected';
        case 'Cancelled':
            return 'Cancelled';
        default:
            return 'New';
    }
}

export function denormalizeDesignCadWorkflowStatus(status: ServiceWorkflowStatus): string {
    switch (status) {
        case 'AwaitingCustomerConfirmation':
            return 'Quoted';
        case 'InProgress':
            return 'Approved';
        default:
            return status === 'New' ? 'Pending' : status;
    }
}

export function normalizePrint3dWorkflowStatus(status: Print3dRequestStatus | string | null | undefined): ServiceWorkflowStatus {
    if (typeof status === 'string' && isWorkflowStatus(status)) {
        return status;
    }
    switch (status) {
        case Print3dRequestStatus.UnderReview:
        case 'UnderReview':
            return 'UnderReview';
        case Print3dRequestStatus.Quoted:
        case 'Quoted':
            return 'AwaitingCustomerConfirmation';
        case Print3dRequestStatus.Approved:
        case 'Approved':
            return 'Approved';
        case Print3dRequestStatus.Queued:
        case Print3dRequestStatus.Slicing:
        case Print3dRequestStatus.Printing:
        case 'Queued':
        case 'Slicing':
        case 'Printing':
            return 'InProgress';
        case Print3dRequestStatus.Completed:
        case 'Completed':
            return 'Completed';
        case Print3dRequestStatus.Rejected:
        case Print3dRequestStatus.Failed:
        case 'Rejected':
        case 'Failed':
            return 'Rejected';
        case Print3dRequestStatus.Cancelled:
        case 'Cancelled':
            return 'Cancelled';
        default:
            return 'New';
    }
}

export function denormalizePrint3dWorkflowStatus(status: ServiceWorkflowStatus): Print3dRequestStatus {
    switch (status) {
        case 'New':
            return Print3dRequestStatus.Pending;
        case 'UnderReview':
            return Print3dRequestStatus.UnderReview;
        case 'AwaitingCustomerConfirmation':
            return Print3dRequestStatus.Quoted;
        case 'Approved':
            return Print3dRequestStatus.Approved;
        case 'InProgress':
        case 'ReadyForDelivery':
            return Print3dRequestStatus.Printing;
        case 'Completed':
            return Print3dRequestStatus.Completed;
        case 'Rejected':
            return Print3dRequestStatus.Rejected;
        case 'Cancelled':
            return Print3dRequestStatus.Cancelled;
    }
}

export function normalizeLaserWorkflowStatus(status: string | null | undefined): ServiceWorkflowStatus {
    if (!status) {
        return 'New';
    }
    if (isWorkflowStatus(status)) {
        return status;
    }

    const normalized = status.trim();
    switch (normalized) {
        case 'Pending':
            return 'New';
        case 'Quoted':
            return 'AwaitingCustomerConfirmation';
        case 'UnderReview':
        case 'Approved':
        case 'InProgress':
        case 'Completed':
        case 'Rejected':
        case 'Cancelled':
            return normalized;
        default:
            return 'New';
    }
}

export function denormalizeLaserWorkflowStatus(status: ServiceWorkflowStatus): LaserServiceRequestStatus {
    switch (status) {
        case 'New':
            return 'Pending';
        case 'UnderReview':
            return 'UnderReview';
        case 'AwaitingCustomerConfirmation':
            return 'Quoted';
        case 'Approved':
            return 'Approved';
        case 'InProgress':
        case 'ReadyForDelivery':
            return 'InProgress';
        case 'Completed':
            return 'Completed';
        case 'Rejected':
            return 'Rejected';
        case 'Cancelled':
            return 'Cancelled';
    }
}

export function normalizeCncWorkflowStatus(status: string | number | null | undefined): ServiceWorkflowStatus {
    if (status == null) {
        return 'New';
    }

    if (typeof status === 'string') {
        const normalized = status.trim();
        if (isWorkflowStatus(normalized)) {
            return normalized;
        }
        if (normalized !== '' && !Number.isNaN(Number(normalized))) {
            return normalizeCncWorkflowStatus(Number(normalized));
        }
    }

    switch (status) {
        case CncServiceRequestStatus.Pending:
            return 'New';
        case CncServiceRequestStatus.InReview:
            return 'UnderReview';
        case CncServiceRequestStatus.Quoted:
            return 'AwaitingCustomerConfirmation';
        case CncServiceRequestStatus.Accepted:
            return 'Approved';
        case CncServiceRequestStatus.InProgress:
            return 'InProgress';
        case CncServiceRequestStatus.Completed:
            return 'Completed';
        case CncServiceRequestStatus.Rejected:
            return 'Rejected';
        case CncServiceRequestStatus.Cancelled:
            return 'Cancelled';
        default:
            return 'New';
    }
}

export function denormalizeCncWorkflowStatus(status: ServiceWorkflowStatus): CncServiceRequestStatus {
    switch (status) {
        case 'New':
            return CncServiceRequestStatus.Pending;
        case 'UnderReview':
            return CncServiceRequestStatus.InReview;
        case 'AwaitingCustomerConfirmation':
            return CncServiceRequestStatus.Quoted;
        case 'Approved':
            return CncServiceRequestStatus.Accepted;
        case 'InProgress':
        case 'ReadyForDelivery':
            return CncServiceRequestStatus.InProgress;
        case 'Completed':
            return CncServiceRequestStatus.Completed;
        case 'Rejected':
            return CncServiceRequestStatus.Rejected;
        case 'Cancelled':
            return CncServiceRequestStatus.Cancelled;
    }
}

export function getWorkflowTimeline(status: ServiceWorkflowStatus, milestones: {
    createdAt?: string | null;
    reviewedAt?: string | null;
    approvedAt?: string | null;
    completedAt?: string | null;
}): { labelKey: string; at?: string | null; done: boolean }[] {
    const order: ServiceWorkflowStatus[] = ['New', 'UnderReview', 'AwaitingCustomerConfirmation', 'Approved', 'InProgress', 'ReadyForDelivery', 'Completed'];
    const currentIndex = order.indexOf(status);

    return order.map((item, index) => ({
        labelKey: getWorkflowStatusKey(item),
        at:
            item === 'New' ? milestones.createdAt :
            item === 'UnderReview' ? milestones.reviewedAt :
            item === 'Approved' ? milestones.approvedAt :
            item === 'Completed' ? milestones.completedAt :
            null,
        done: currentIndex >= 0 && index <= currentIndex
    }));
}
