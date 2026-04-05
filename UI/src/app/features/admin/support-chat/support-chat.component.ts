import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { SupportChatApiService } from '../../../shared/services/support-chat-api.service';
import { SupportChatHubService } from '../../../shared/services/support-chat-hub.service';
import { SupportConversationDto, SupportMessageDto } from '../../../shared/models/support-chat.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-support-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    CardModule,
    ButtonModule,
    ScrollPanelModule,
    ProgressSpinnerModule,
    ToastModule,
    TooltipModule
  ],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="support-chat-admin">
      <div class="page-header">
        <h1>{{ 'admin.supportChat.title' | translate }}</h1>
      </div>
      <div class="chat-layout">
        <div class="conversations-panel">
          <p-card>
            <h3 class="panel-title">
              <i class="pi pi-comments"></i>
              {{ 'admin.supportChat.conversations' | translate }}
            </h3>
            <div *ngIf="conversationsLoading" class="loading-state">
              <i class="pi pi-spin pi-spinner"></i>
            </div>
            <div class="conversations-list">
              <div
                *ngFor="let c of conversations"
                class="conv-row"
                [class.active]="selectedConversation?.id === c.id"
                (click)="selectConversation(c)">
                <div class="conv-meta">
                  <span class="conv-subject">{{ c.subject }}</span>
                  <span class="conv-customer">{{ c.customerName || c.customerId }}</span>
                  <span class="conv-preview">{{ c.lastMessagePreview || ('admin.supportChat.noMessagesPreview' | translate) }}</span>
                  <span class="conv-date">{{ c.lastMessageAt ? formatDate(c.lastMessageAt) : formatDate(c.createdAt) }}</span>
                  <span class="status-tag" *ngIf="c.status === 1">{{ 'admin.supportChat.closed' | translate }}</span>
                </div>
              </div>
              <div *ngIf="!conversationsLoading && conversations.length === 0" class="empty-list">
                {{ 'admin.supportChat.noConversations' | translate }}
              </div>
            </div>
          </p-card>
        </div>
        <div class="messages-panel">
          <p-card *ngIf="selectedConversation">
            <div class="conv-header">
              <div>
                <h3>{{ selectedConversation.subject }}</h3>
                <p class="conv-sub">{{ selectedConversation.customerName || selectedConversation.customerId }}</p>
              </div>
              <div class="conv-actions">
                <button *ngIf="selectedConversation.status === 0" type="button" pButton class="p-button-sm p-button-secondary" [label]="'admin.supportChat.close' | translate" (click)="closeSelected()" [disabled]="statusBusy"></button>
                <button *ngIf="selectedConversation.status === 1" type="button" pButton class="p-button-sm" [label]="'admin.supportChat.reopen' | translate" (click)="reopenSelected()" [disabled]="statusBusy"></button>
              </div>
            </div>
            <div #messagesContainer class="messages-container">
              <div *ngIf="messagesLoading" class="loading-state">
                <i class="pi pi-spin pi-spinner"></i>
              </div>
              <div *ngIf="!messagesLoading && messages.length === 0" class="empty-chat">
                {{ 'admin.supportChat.noMessages' | translate }}
              </div>
              <div class="messages-list">
                <div
                  *ngFor="let msg of messages"
                  class="message"
                  [class.user-message]="msg.isFromCustomer"
                  [class.staff-message]="!msg.isFromCustomer">
                  <div class="message-avatar">
                    <i [class]="msg.isFromCustomer ? 'pi pi-user' : 'pi pi-headset'"></i>
                  </div>
                  <div class="message-content">
                    <div class="message-header">
                      <span class="message-sender">{{ msg.isFromCustomer ? (selectedConversation.customerName || 'Customer') : (msg.senderName || 'Support') }}</span>
                      <span class="message-time">{{ formatTime(msg.createdAt) }}</span>
                    </div>
                    <div class="message-text" *ngIf="msg.content">{{ msg.content }}</div>
                    <div class="message-attachment" *ngIf="msg.attachmentUrl">
                      <a *ngIf="isImageUrl(msg.attachmentUrl)" [href]="getAttachmentFullUrl(msg.attachmentUrl)" target="_blank" rel="noopener" class="attachment-image-link">
                        <img [src]="getAttachmentFullUrl(msg.attachmentUrl)" [alt]="msg.attachmentFileName || 'Attachment'" class="attachment-image" />
                      </a>
                      <a *ngIf="!isImageUrl(msg.attachmentUrl)" [href]="getAttachmentFullUrl(msg.attachmentUrl)" target="_blank" rel="noopener" class="attachment-file">
                        <i class="pi pi-paperclip"></i> {{ msg.attachmentFileName || ('chat.attachment' | translate) }}
                      </a>
                    </div>
                  </div>
                </div>
                <div *ngIf="sending" class="message staff-message">
                  <div class="message-avatar"><i class="pi pi-headset"></i></div>
                  <div class="message-content">
                    <div class="typing-indicator"><span></span><span></span><span></span></div>
                  </div>
                </div>
              </div>
            </div>
            <div class="input-area">
              <input type="file" #fileInput class="file-input-hidden" accept="image/*,.pdf,.doc,.docx,.xls,.xlsx" (change)="onFileSelected($event)">
              <button type="button" class="attach-btn" (click)="fileInput.click()" [disabled]="sending || selectedConversation.status === 1" [pTooltip]="'chat.attach' | translate">
                <i class="pi pi-paperclip"></i>
              </button>
              <input
                type="text"
                [(ngModel)]="messageText"
                [placeholder]="'chat.typeMessage' | translate"
                (keyup.enter)="sendMessage()"
                [disabled]="sending || selectedConversation.status === 1" />
              <span class="pending-attachment" *ngIf="pendingAttachment"><i class="pi pi-file"></i> {{ pendingAttachment.fileName }}</span>
              <button
                pButton
                icon="pi pi-send"
                [disabled]="(!messageText.trim() && !pendingAttachment) || sending || selectedConversation.status === 1"
                (click)="sendMessage()">
              </button>
            </div>
          </p-card>
          <p-card *ngIf="!selectedConversation && !conversationsLoading">
            <div class="empty-state">{{ 'admin.supportChat.selectConversation' | translate }}</div>
          </p-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .support-chat-admin { padding: 1rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; }
    .chat-layout { display: grid; grid-template-columns: 320px 1fr; gap: 1rem; min-height: 70vh; }
    .conversations-panel .panel-title { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; font-size: 1rem; }
    .conversations-list { max-height: 60vh; overflow-y: auto; }
    .conv-row { padding: 0.75rem; cursor: pointer; border-radius: 8px; margin-bottom: 4px; }
    .conv-row:hover { background: #f4f4f4; }
    .conv-row.active { background: #e3f2fd; }
    .conv-subject { font-weight: 700; display: block; font-size: 0.9rem; }
    .conv-customer { font-weight: 600; display: block; font-size: 0.85rem; color: #555; }
    .status-tag { font-size: 0.7rem; color: #888; }
    .conv-preview { font-size: 0.85rem; color: #666; display: block; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .conv-date { font-size: 0.75rem; color: #999; }
    .empty-list, .empty-state, .empty-chat { text-align: center; color: #888; padding: 2rem; }
    .messages-container { max-height: 50vh; overflow-y: auto; padding: 1rem; }
    .messages-list { display: flex; flex-direction: column; gap: 1rem; }
    .message { display: flex; gap: 0.75rem; }
    .user-message { flex-direction: row-reverse; }
    .message-avatar { width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; background: #e0e0e0; }
    .user-message .message-avatar { background: #667eea; color: white; }
    .staff-message .message-avatar { background: #00bbf9; color: white; }
    .message-content { max-width: 70%; }
    .message-header { display: flex; gap: 0.5rem; margin-bottom: 0.25rem; font-size: 0.8rem; }
    .message-sender { font-weight: 600; }
    .message-time { color: #888; }
    .message-text { padding: 0.5rem; border-radius: 8px; background: #f5f5f5; }
    .user-message .message-content .message-text { background: #e8eaf6; }
    .typing-indicator { display: flex; gap: 4px; padding: 0.5rem; }
    .typing-indicator span { width: 8px; height: 8px; background: #999; border-radius: 50%; animation: typing 1.4s infinite ease-in-out; }
    .typing-indicator span:nth-child(2) { animation-delay: 0.2s; }
    .typing-indicator span:nth-child(3) { animation-delay: 0.4s; }
    @keyframes typing { 0%, 60%, 100% { opacity: 0.4; } 30% { opacity: 1; } }
    .input-area { display: flex; gap: 0.5rem; padding-top: 1rem; border-top: 1px solid #eee; margin-top: 1rem; align-items: center; flex-wrap: wrap; }
    .input-area input[type="text"] { flex: 1; min-width: 120px; padding: 0.75rem; border: 1px solid #ddd; border-radius: 8px; }
    .file-input-hidden { position: absolute; width: 0; height: 0; opacity: 0; pointer-events: none; }
    .attach-btn { width: 40px; height: 40px; background: #f0f0f0; border: none; border-radius: 8px; color: #666; cursor: pointer; display: flex; align-items: center; justify-content: center; }
    .attach-btn:hover:not(:disabled) { background: #e0e0e0; color: #667eea; }
    .pending-attachment { font-size: 0.85rem; color: #666; }
    .message-attachment { margin-top: 0.5rem; }
    .attachment-image { max-width: 240px; max-height: 180px; border-radius: 8px; object-fit: cover; }
    .attachment-file { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.5rem; background: #f0f0f0; border-radius: 8px; color: #667eea; text-decoration: none; }
    .loading-state { text-align: center; padding: 1rem; }
    .conv-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 0.5rem; }
    .conv-sub { margin: 0.25rem 0 0 0; font-size: 0.9rem; color: #666; }
    .conv-actions { display: flex; gap: 0.5rem; flex-shrink: 0; }
    @media (max-width: 768px) { .chat-layout { grid-template-columns: 1fr; } }
  `]
})
export class SupportChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  conversations: SupportConversationDto[] = [];
  selectedConversation: SupportConversationDto | null = null;
  messages: SupportMessageDto[] = [];
  messageText = '';
  pendingAttachment: { url: string; fileName: string } | null = null;
  conversationsLoading = false;
  messagesLoading = false;
  sending = false;
  statusBusy = false;
  private shouldScroll = false;

  private supportApi = inject(SupportChatApiService);
  private hub = inject(SupportChatHubService);
  private messageService = inject(MessageService);
  private translate = inject(TranslateService);

  ngOnInit() {
    this.loadConversations();
    this.hub.onReceiveMessage((msg) => {
      if (this.selectedConversation && msg.conversationId === this.selectedConversation.id) {
        if (!this.messages.some((m) => m.id === msg.id)) {
          this.messages = [...this.messages, msg];
          this.shouldScroll = true;
        }
      }
    });
    this.hub.start().catch(() => {});
  }

  ngOnDestroy() {
    this.hub.stop();
  }

  ngAfterViewChecked() {
    if (this.shouldScroll && this.messagesContainer?.nativeElement) {
      this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      this.shouldScroll = false;
    }
  }

  loadConversations() {
    this.conversationsLoading = true;
    this.supportApi.getConversations({}).subscribe({
      next: (list) => {
        this.conversations = list || [];
        this.conversationsLoading = false;
      },
      error: () => {
        this.conversationsLoading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load conversations' });
      }
    });
  }

  selectConversation(c: SupportConversationDto) {
    this.selectedConversation = c;
    this.messages = [];
    this.messagesLoading = true;
    this.supportApi.getMessages(c.id).subscribe({
      next: (list) => {
        this.messages = list || [];
        this.messagesLoading = false;
        this.shouldScroll = true;
        this.hub.joinConversation(c.id).catch(() => {});
      },
      error: () => {
        this.messagesLoading = false;
      }
    });
  }

  sendMessage() {
    if (
      (!this.messageText.trim() && !this.pendingAttachment) ||
      !this.selectedConversation ||
      this.selectedConversation.status === 1 ||
      this.sending
    )
      return;
    const content = this.messageText.trim();
    const att = this.pendingAttachment;
    const convId = this.selectedConversation.id;
    this.messageText = '';
    this.pendingAttachment = null;
    this.sending = true;
    const done = () => {
      this.sending = false;
      this.shouldScroll = true;
    };
    const fail = () => {
      this.sending = false;
      this.messageText = content;
      if (att) this.pendingAttachment = att;
      this.messageService.add({
        severity: 'error',
        summary: this.translate.instant('common.error'),
        detail: this.translate.instant('chat.sendFailed')
      });
    };
    this.hub
      .sendMessage(convId, content, att?.url, att?.fileName)
      .then(done)
      .catch(() => {
        this.supportApi
          .postMessage(convId, {
            content: content || undefined,
            attachmentUrl: att?.url,
            attachmentFileName: att?.fileName
          })
          .subscribe({
            next: (msg) => {
              if (!this.messages.some((m) => m.id === msg.id)) {
                this.messages = [...this.messages, msg];
              }
              done();
            },
            error: fail
          });
      });
  }

  closeSelected() {
    if (!this.selectedConversation || this.statusBusy) return;
    this.statusBusy = true;
    this.supportApi.closeConversation(this.selectedConversation.id).subscribe({
      next: () => {
        this.statusBusy = false;
        const id = this.selectedConversation!.id;
        this.conversations = this.conversations.map((c) => (c.id === id ? { ...c, status: 1 as const } : c));
        this.selectedConversation = this.conversations.find((c) => c.id === id) || null;
      },
      error: () => {
        this.statusBusy = false;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('chat.closeFailed')
        });
      }
    });
  }

  reopenSelected() {
    if (!this.selectedConversation || this.statusBusy) return;
    this.statusBusy = true;
    this.supportApi.reopenConversation(this.selectedConversation.id).subscribe({
      next: () => {
        this.statusBusy = false;
        const id = this.selectedConversation!.id;
        this.conversations = this.conversations.map((c) => (c.id === id ? { ...c, status: 0 as const } : c));
        this.selectedConversation = this.conversations.find((c) => c.id === id) || null;
      },
      error: () => {
        this.statusBusy = false;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('chat.createFailed')
        });
      }
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !this.selectedConversation) return;
    if (file.size > 10 * 1024 * 1024) {
      this.messageService.add({ severity: 'warn', summary: this.translate.instant('chat.attachmentTooBig'), detail: '' });
      return;
    }
    this.sending = true;
    this.supportApi.uploadAttachment(this.selectedConversation.id, file).subscribe({
      next: (res) => {
        this.pendingAttachment = { url: res.url, fileName: res.fileName };
        this.sending = false;
      },
      error: () => {
        this.sending = false;
        this.messageService.add({ severity: 'error', summary: this.translate.instant('Error'), detail: this.translate.instant('chat.uploadFailed') });
      }
    });
  }

  getAttachmentFullUrl(url: string): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    const base = environment.apiUrl?.replace(/\/api\/v1$/, '') || '';
    return base + (url.startsWith('/') ? url : '/' + url);
  }

  isImageUrl(url: string): boolean {
    if (!url) return false;
    const u = url.toLowerCase();
    return /\.(jpe?g|png|gif|webp|bmp)(\?|$)/i.test(u) || u.includes('/image/');
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  formatTime(date: string): string {
    return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
