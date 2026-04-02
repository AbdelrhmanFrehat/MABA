// Support chat (live chat customer ↔ admin) — matches API DTOs
export type SupportConversationStatus = 0 | 1; // Open = 0, Closed = 1

export interface SupportConversationDto {
    id: string;
    customerId: string;
    customerName?: string;
    subject: string;
    assignedToUserId?: string;
    assignedToUserName?: string;
    status: SupportConversationStatus;
    relatedOrderId?: string;
    relatedDesignId?: string;
    createdAt: string;
    updatedAt?: string;
    lastMessagePreview?: string;
    lastMessageAt?: string;
    unreadCount: number;
}

export interface SupportMessageDto {
    id: string;
    conversationId: string;
    senderUserId: string;
    senderName?: string;
    isFromCustomer: boolean;
    content: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    createdAt: string;
    readAt?: string;
}

export interface UploadAttachmentResponse {
    url: string;
    fileName: string;
}

export interface CreateSupportConversationRequest {
    subject?: string;
    initialMessage?: string;
    relatedOrderId?: string;
    relatedDesignId?: string;
}

export interface SendMessageRequestBody {
    content?: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
}
