import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { SupportMessageDto } from '../models/support-chat.model';

const hubPath = '/hubs/support-chat';
const hubUrl = environment.apiUrl.replace(/\/api\/v1$/, '') + hubPath;

@Injectable({
  providedIn: 'root'
})
export class SupportChatHubService implements OnDestroy {
  private connection: signalR.HubConnection | null = null;
  private onMessageCallback: ((msg: SupportMessageDto) => void) | null = null;

  constructor(private auth: AuthService) {}

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;
    const token = this.auth.token;
    if (!token) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveMessage', (payload: Partial<SupportMessageDto> & { id: string; conversationId: string; senderUserId: string; senderName?: string; isFromCustomer: boolean; content: string; attachmentUrl?: string; attachmentFileName?: string; createdAt: string; readAt?: string }) => {
      const msg: SupportMessageDto = {
        id: payload.id,
        conversationId: payload.conversationId,
        senderUserId: payload.senderUserId,
        senderName: payload.senderName,
        isFromCustomer: payload.isFromCustomer,
        content: payload.content,
        attachmentUrl: payload.attachmentUrl,
        attachmentFileName: payload.attachmentFileName,
        createdAt: payload.createdAt,
        readAt: payload.readAt
      };
      this.onMessageCallback?.(msg);
    });

    await this.connection.start();
  }

  onReceiveMessage(callback: (msg: SupportMessageDto) => void): void {
    this.onMessageCallback = callback;
  }

  /** Customer: join my conversation group */
  async joinMyConversation(): Promise<void> {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) return;
    await this.connection.invoke('JoinMyConversation');
  }

  /** Admin: join a specific conversation */
  async joinConversation(conversationId: string): Promise<void> {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) return;
    await this.connection.invoke('JoinConversation', conversationId);
  }

  async sendMessage(conversationId: string, content: string, attachmentUrl?: string, attachmentFileName?: string): Promise<void> {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected');
    }
    await this.connection.invoke('SendMessage', conversationId, content || '', attachmentUrl || null, attachmentFileName || null);
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
    this.onMessageCallback = null;
  }

  ngOnDestroy(): void {
    this.stop();
  }
}
