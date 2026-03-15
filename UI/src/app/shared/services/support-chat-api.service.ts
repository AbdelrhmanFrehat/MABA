import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SupportConversationDto, SupportMessageDto, UploadAttachmentResponse } from '../models/support-chat.model';

@Injectable({
  providedIn: 'root'
})
export class SupportChatApiService {
  private baseUrl = `${environment.apiUrl}/support-conversations`;

  constructor(private http: HttpClient) {}

  /** Customer: get or create my conversation */
  getMine(): Observable<SupportConversationDto> {
    return this.http.get<SupportConversationDto>(`${this.baseUrl}/mine`);
  }

  /** Admin: list conversations (optional status filter) */
  getConversations(params?: { status?: number; page?: number; pageSize?: number }): Observable<SupportConversationDto[]> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.status != null) httpParams = httpParams.set('status', params.status);
      if (params.page != null) httpParams = httpParams.set('page', params.page);
      if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize);
    }
    return this.http.get<SupportConversationDto[]>(`${this.baseUrl}`, { params: httpParams });
  }

  /** Get messages for a conversation */
  getMessages(conversationId: string, params?: { page?: number; pageSize?: number }): Observable<SupportMessageDto[]> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.page != null) httpParams = httpParams.set('page', params.page);
      if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize);
    }
    return this.http.get<SupportMessageDto[]>(`${this.baseUrl}/${conversationId}/messages`, { params: httpParams });
  }

  /** Upload an attachment (image or file) for a conversation. Returns url and fileName to send in a message. */
  uploadAttachment(conversationId: string, file: File): Observable<UploadAttachmentResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadAttachmentResponse>(`${this.baseUrl}/${conversationId}/upload`, formData);
  }

  /** Admin: assign conversation to a user */
  assign(conversationId: string, assignToUserId?: string): Observable<void> {
    let params = new HttpParams();
    if (assignToUserId) params = params.set('assignToUserId', assignToUserId);
    return this.http.post<void>(`${this.baseUrl}/${conversationId}/assign`, null, { params });
  }
}
