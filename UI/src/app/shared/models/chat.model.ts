// Chat/AI Assistant Models
export interface ChatConversation {
    id: string; // GUID
    userId: string;
    title?: string;
    messages: ChatMessage[];
    createdAt: string;
    updatedAt?: string;
}

export interface ChatMessage {
    id: string; // GUID
    conversationId: string;
    type: ChatMessageType;
    content: string;
    role: ChatMessageRole;
    attachments?: ChatAttachment[];
    metadata?: any;
    createdAt: string;
}

export enum ChatMessageType {
    Text = 'Text',
    Image = 'Image',
    File = 'File',
    System = 'System'
}

export enum ChatMessageRole {
    User = 'User',
    Assistant = 'Assistant',
    System = 'System'
}

export interface ChatAttachment {
    id: string; // GUID
    messageId: string;
    fileName: string;
    fileUrl: string;
    fileType: string;
    fileSize: number;
}

export interface CreateChatMessageRequest {
    conversationId?: string;
    content: string;
    type?: ChatMessageType;
    attachments?: File[];
}

export interface CreateConversationRequest {
    title?: string;
    initialMessage?: string;
}

export interface ChatConversationListResponse {
    items: ChatConversation[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

