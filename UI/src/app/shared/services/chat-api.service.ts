import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    ChatConversation,
    ChatConversationListResponse,
    ChatMessage,
    CreateChatMessageRequest,
    CreateConversationRequest
} from '../models/chat.model';

@Injectable({
    providedIn: 'root'
})
export class ChatApiService {
    private baseUrl = environment.apiUrl || '/api';

    constructor(private http: HttpClient) {}

    getConversations(params?: {
        page?: number;
        pageSize?: number;
    }): Observable<ChatConversationListResponse> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key as keyof typeof params];
                if (value !== null && value !== undefined) {
                    // Only add non-empty values
                    const stringValue = value.toString();
                    if (stringValue !== '') {
                        httpParams = httpParams.set(key, stringValue);
                    }
                }
            });
        }
        return this.http.get<ChatConversationListResponse>(`${this.baseUrl}/ai-sessions`, { params: httpParams });
    }

    getConversationById(id: string): Observable<ChatConversation> {
        return this.http.get<ChatConversation>(`${this.baseUrl}/ai-sessions/${id}`);
    }

    createConversation(request: CreateConversationRequest): Observable<ChatConversation> {
        return this.http.post<ChatConversation>(`${this.baseUrl}/ai-sessions`, request);
    }

    deleteConversation(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/ai-sessions/${id}`);
    }

    getMessages(conversationId: string, params?: {
        page?: number;
        pageSize?: number;
    }): Observable<{ items: ChatMessage[]; totalCount: number }> {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key as keyof typeof params];
                if (value !== null && value !== undefined) {
                    // Only add non-empty values
                    const stringValue = value.toString();
                    if (stringValue !== '') {
                        httpParams = httpParams.set(key, stringValue);
                    }
                }
            });
        }
        return this.http.get<{ items: ChatMessage[]; totalCount: number }>(`${this.baseUrl}/ai-sessions/${conversationId}/messages`, { params: httpParams });
    }

    sendMessage(conversationId: string, request: CreateChatMessageRequest): Observable<ChatMessage> {
        const formData = new FormData();
        formData.append('content', request.content);
        formData.append('type', request.type || 'Text');
        if (request.attachments) {
            request.attachments.forEach((file, index) => {
                formData.append(`attachments[${index}]`, file);
            });
        }
        return this.http.post<ChatMessage>(`${this.baseUrl}/ai-sessions/${conversationId}/messages`, formData);
    }
}

