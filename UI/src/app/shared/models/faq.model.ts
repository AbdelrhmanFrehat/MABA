export type FaqCategory =
    | 'Print3d'
    | 'Laser'
    | 'Cnc'
    | 'Software'
    | 'OrdersShipping'
    | 'Payments'
    | 'Support'
    | 'General';

export interface FaqItem {
    id: string;
    category: FaqCategory;
    questionEn: string;
    questionAr?: string;
    answerEn: string;
    answerAr?: string;
    isActive: boolean;
    isFeatured: boolean;
    sortOrder: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateFaqPayload {
    category: FaqCategory;
    questionEn: string;
    questionAr?: string;
    answerEn: string;
    answerAr?: string;
    isActive: boolean;
    isFeatured: boolean;
    sortOrder: number;
}

export interface ReorderFaqPayload {
    items: { id: string; sortOrder: number }[];
}
