import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { SupportChatApiService } from '../../../shared/services/support-chat-api.service';
import { SupportChatHubService } from '../../../shared/services/support-chat-hub.service';
import { LanguageService } from '../../../shared/services/language.service';
import { SupportConversationDto, SupportMessageDto } from '../../../shared/models/support-chat.model';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-chat',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        InputTextModule,
        ButtonModule,
        CardModule,
        ScrollPanelModule,
        ProgressSpinnerModule,
        ToastModule,
        TooltipModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="chat-page" [dir]="languageService.direction">
            <!-- Hero Section -->
            <section class="hero-section">
                <div class="hero-bg-gradient"></div>
                <div class="hero-pattern"></div>
                <div class="hero-particles">
                    <div class="particle" *ngFor="let p of [1,2,3,4,5]" [style.animation-delay]="(p * 0.5) + 's'"></div>
                </div>
                <div class="hero-content">
                    <span class="hero-badge">
                        <i class="pi pi-comments"></i>
                        {{ 'chat.supportBadge' | translate }}
                    </span>
                    <h1 class="hero-title">{{ 'chat.title' | translate }}</h1>
                    <p class="hero-subtitle">{{ 'chat.supportSubtitle' | translate }}</p>
                </div>
            </section>

            <!-- Chat Section -->
            <section class="chat-section">
                <div class="container">
                    <div class="chat-grid">
                        <!-- Conversations Sidebar (single: my support thread) -->
                        <div class="conversations-sidebar">
                            <div class="sidebar-card">
                                <h3 class="sidebar-title">
                                    <i class="pi pi-comments"></i>
                                    {{ 'chat.conversations' | translate }}
                                </h3>
                                <div *ngIf="conversationsLoading" class="loading-state">
                                    <i class="pi pi-spin pi-spinner"></i>
                                </div>
                                <div class="conversations-list">
                                    <div *ngIf="conversation" 
                                         class="conversation-item active">
                                        <div class="conv-icon">
                                            <i class="pi pi-headset"></i>
                                        </div>
                                        <div class="conv-info">
                                            <div class="conv-title">{{ 'chat.supportThread' | translate }}</div>
                                            <div class="conv-date">{{ conversation.updatedAt ? formatDate(conversation.updatedAt) : formatDate(conversation.createdAt) }}</div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Chat Main -->
                        <div class="chat-main">
                            <div class="chat-card">
                                <!-- Messages Container -->
                                <div #messagesContainer class="messages-container">
                                    <div *ngIf="messagesLoading" class="loading-state">
                                        <i class="pi pi-spin pi-spinner"></i>
                                        <p>{{ languageService.language === 'ar' ? 'جاري التحميل...' : 'Loading...' }}</p>
                                    </div>

                                    <div *ngIf="!messagesLoading && messages.length === 0" class="empty-chat">
                                        <div class="empty-icon">
                                            <i class="pi pi-sparkles"></i>
                                        </div>
                                        <h3>{{ 'chat.noMessages' | translate }}</h3>
                                        <p>{{ 'chat.startConversation' | translate }}</p>
                                    </div>

                                    <div class="messages-list">
                                        <div *ngFor="let message of messages" 
                                             class="message"
                                             [class.user-message]="message.isFromCustomer"
                                             [class.ai-message]="!message.isFromCustomer">
                                            <div class="message-avatar">
                                                <i [class]="message.isFromCustomer ? 'pi pi-user' : 'pi pi-headset'"></i>
                                            </div>
                                            <div class="message-content">
                                                <div class="message-header">
                                                    <span class="message-sender">{{ message.isFromCustomer ? ('chat.you' | translate) : (message.senderName || ('chat.staff' | translate)) }}</span>
                                                    <span class="message-time">{{ formatTime(message.createdAt) }}</span>
                                                </div>
                                                <div class="message-text" *ngIf="message.content">{{ message.content }}</div>
                                                <div class="message-attachment" *ngIf="message.attachmentUrl">
                                                    <a *ngIf="isImageUrl(message.attachmentUrl)" [href]="getAttachmentFullUrl(message.attachmentUrl)" target="_blank" rel="noopener" class="attachment-image-link">
                                                        <img [src]="getAttachmentFullUrl(message.attachmentUrl)" [alt]="message.attachmentFileName || 'Attachment'" class="attachment-image" />
                                                    </a>
                                                    <a *ngIf="!isImageUrl(message.attachmentUrl)" [href]="getAttachmentFullUrl(message.attachmentUrl)" target="_blank" rel="noopener" class="attachment-file">
                                                        <i class="pi pi-paperclip"></i> {{ message.attachmentFileName || 'chat.attachment' | translate }}
                                                    </a>
                                                </div>
                                            </div>
                                        </div>

                                        <!-- Typing Indicator -->
                                        <div *ngIf="sending" class="message ai-message">
                                            <div class="message-avatar">
                                                <i class="pi pi-headset"></i>
                                            </div>
                                            <div class="message-content">
                                                <div class="typing-indicator">
                                                    <span></span>
                                                    <span></span>
                                                    <span></span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Input Area -->
                                <div class="chat-input-area">
                                    <div class="input-wrapper">
                                        <input 
                                            type="file" 
                                            #fileInput
                                            class="file-input-hidden"
                                            accept="image/*,.pdf,.doc,.docx,.xls,.xlsx"
                                            (change)="onFileSelected($event)">
                                        <button 
                                            type="button"
                                            class="attach-btn"
                                            (click)="fileInput.click()"
                                            [disabled]="!conversation || sending"
                                            [pTooltip]="'chat.attach' | translate">
                                            <i class="pi pi-paperclip"></i>
                                        </button>
                                        <input 
                                            type="text" 
                                            [(ngModel)]="messageText"
                                            [placeholder]="'chat.typeMessage' | translate"
                                            (keyup.enter)="sendMessage()"
                                            [disabled]="!conversation || sending">
                                        <span class="pending-attachment" *ngIf="pendingAttachment">
                                            <i class="pi pi-file"></i> {{ pendingAttachment.fileName }}
                                        </span>
                                        <button 
                                            class="send-btn"
                                            (click)="sendMessage()"
                                            [disabled]="(!messageText.trim() && !pendingAttachment) || !conversation || sending">
                                            <i [class]="sending ? 'pi pi-spin pi-spinner' : 'pi pi-send'"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    `,
    styles: [`
        :host {
            --gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            --gradient-dark: linear-gradient(135deg, #0c1445 0%, #1a1a2e 50%, #16213e 100%);
            --gradient-ai: linear-gradient(135deg, #00f5d4 0%, #00bbf9 50%, #9b5de5 100%);
            --color-primary: #667eea;
            --color-secondary: #764ba2;
            --color-ai: #00f5d4;
            --shadow-glow: 0 0 40px rgba(102, 126, 234, 0.4);
        }

        .chat-page {
            width: 100%;
            min-height: 100vh;
            background: #fafbfc;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }

        /* ============ HERO SECTION ============ */
        .hero-section {
            position: relative;
            padding: 4rem 2rem;
            overflow: hidden;
        }

        .hero-bg-gradient {
            position: absolute;
            inset: 0;
            background: var(--gradient-dark);
            z-index: 0;
        }

        .hero-pattern {
            position: absolute;
            inset: 0;
            background-image:
                radial-gradient(circle at 25% 25%, rgba(0, 245, 212, 0.15) 0%, transparent 50%),
                radial-gradient(circle at 75% 75%, rgba(155, 93, 229, 0.15) 0%, transparent 50%);
            z-index: 1;
        }

        .hero-particles {
            position: absolute;
            inset: 0;
            z-index: 2;
        }

        .particle {
            position: absolute;
            width: 6px;
            height: 6px;
            background: var(--color-ai);
            border-radius: 50%;
            animation: floatParticle 6s ease-in-out infinite;
        }

        .particle:nth-child(1) { top: 20%; left: 10%; }
        .particle:nth-child(2) { top: 60%; left: 80%; }
        .particle:nth-child(3) { top: 40%; left: 30%; }
        .particle:nth-child(4) { top: 80%; left: 60%; }
        .particle:nth-child(5) { top: 10%; left: 70%; }

        @keyframes floatParticle {
            0%, 100% { transform: translateY(0) scale(1); opacity: 0.5; }
            50% { transform: translateY(-20px) scale(1.2); opacity: 1; }
        }

        .hero-content {
            position: relative;
            z-index: 10;
            text-align: center;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: linear-gradient(135deg, rgba(0, 245, 212, 0.2) 0%, rgba(155, 93, 229, 0.2) 100%);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(0, 245, 212, 0.3);
            border-radius: 50px;
            color: var(--color-ai);
            font-size: 0.875rem;
            margin-bottom: 1.5rem;
        }

        .hero-title {
            font-size: clamp(2rem, 4vw, 3rem);
            font-weight: 800;
            color: white;
            margin-bottom: 1rem;
        }

        .hero-subtitle {
            color: rgba(255,255,255,0.7);
            font-size: 1.1rem;
            max-width: 600px;
            margin: 0 auto;
        }

        /* ============ CHAT SECTION ============ */
        .chat-section {
            padding: 3rem 1rem;
            margin-top: -2rem;
            position: relative;
            z-index: 20;
        }

        .chat-grid {
            display: grid;
            grid-template-columns: 300px 1fr;
            gap: 2rem;
        }

        /* ============ SIDEBAR ============ */
        .sidebar-card {
            background: white;
            border-radius: 20px;
            padding: 1.5rem;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            height: calc(100vh - 300px);
            display: flex;
            flex-direction: column;
        }

        .sidebar-title {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 1.1rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 1.5rem;
        }

        .sidebar-title i {
            color: var(--color-primary);
        }

        .new-chat-btn {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
            width: 100%;
            padding: 0.875rem 1.5rem;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
            margin-bottom: 1.5rem;
        }

        .new-chat-btn:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-glow);
        }

        .conversations-list {
            flex: 1;
            overflow-y: auto;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .conversation-item {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s;
        }

        .conversation-item:hover {
            background: #f8f9fa;
        }

        .conversation-item.active {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
        }

        .conv-icon {
            width: 40px;
            height: 40px;
            background: #f8f9fa;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--color-primary);
        }

        .conversation-item.active .conv-icon {
            background: var(--gradient-primary);
            color: white;
        }

        .conv-info {
            flex: 1;
            min-width: 0;
        }

        .conv-title {
            font-weight: 600;
            color: #1a1a2e;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .conv-date {
            font-size: 0.8rem;
            color: #6c757d;
        }

        /* ============ CHAT MAIN ============ */
        .chat-card {
            background: white;
            border-radius: 20px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.08);
            height: calc(100vh - 300px);
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }

        .messages-container {
            flex: 1;
            overflow-y: auto;
            padding: 2rem;
        }

        .loading-state {
            text-align: center;
            padding: 2rem;
            color: #6c757d;
        }

        .loading-state i {
            font-size: 2rem;
            color: var(--color-primary);
        }

        .empty-chat {
            text-align: center;
            padding: 4rem 2rem;
        }

        .empty-icon {
            width: 100px;
            height: 100px;
            background: var(--gradient-ai);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 2rem;
        }

        .empty-icon i {
            font-size: 2.5rem;
            color: white;
        }

        .empty-chat h3 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.5rem;
        }

        .empty-chat p {
            color: #6c757d;
        }

        .messages-list {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .message {
            display: flex;
            gap: 1rem;
        }

        .user-message {
            flex-direction: row-reverse;
        }

        .message-avatar {
            width: 40px;
            height: 40px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }

        .user-message .message-avatar {
            background: var(--gradient-primary);
            color: white;
        }

        .ai-message .message-avatar {
            background: var(--gradient-ai);
            color: white;
        }

        .message-content {
            max-width: 70%;
            background: #f8f9fa;
            padding: 1rem 1.25rem;
            border-radius: 16px;
        }

        .user-message .message-content {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            border-top-right-radius: 4px;
        }

        .ai-message .message-content {
            border-top-left-radius: 4px;
        }

        .message-header {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 0.5rem;
        }

        .message-sender {
            font-weight: 700;
            font-size: 0.85rem;
            color: #1a1a2e;
        }

        .message-time {
            font-size: 0.75rem;
            color: #6c757d;
        }

        .message-text {
            color: #1a1a2e;
            line-height: 1.6;
        }

        .typing-indicator {
            display: flex;
            gap: 4px;
            padding: 0.5rem 0;
        }

        .typing-indicator span {
            width: 8px;
            height: 8px;
            background: #6c757d;
            border-radius: 50%;
            animation: typing 1.4s infinite ease-in-out;
        }

        .typing-indicator span:nth-child(2) {
            animation-delay: 0.2s;
        }

        .typing-indicator span:nth-child(3) {
            animation-delay: 0.4s;
        }

        @keyframes typing {
            0%, 60%, 100% { transform: translateY(0); opacity: 0.4; }
            30% { transform: translateY(-8px); opacity: 1; }
        }

        /* ============ INPUT AREA ============ */
        .chat-input-area {
            padding: 1.5rem 2rem;
            border-top: 1px solid #e9ecef;
            background: #fafbfc;
        }

        .input-wrapper {
            display: flex;
            gap: 1rem;
            background: white;
            padding: 0.5rem;
            border-radius: 16px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }

        .input-wrapper input {
            flex: 1;
            padding: 1rem 1.5rem;
            border: none;
            background: transparent;
            font-size: 1rem;
            outline: none;
        }

        .send-btn {
            width: 50px;
            height: 50px;
            background: var(--gradient-primary);
            border: none;
            border-radius: 12px;
            color: white;
            cursor: pointer;
            transition: all 0.3s;
        }

        .send-btn:hover:not(:disabled) {
            transform: scale(1.05);
            box-shadow: var(--shadow-glow);
        }

        .send-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .file-input-hidden {
            position: absolute;
            width: 0;
            height: 0;
            opacity: 0;
            pointer-events: none;
        }
        .attach-btn {
            width: 44px;
            height: 44px;
            background: #f0f0f0;
            border: none;
            border-radius: 10px;
            color: #666;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .attach-btn:hover:not(:disabled) {
            background: #e0e0e0;
            color: var(--color-primary);
        }
        .pending-attachment {
            font-size: 0.85rem;
            color: #666;
            display: flex;
            align-items: center;
            gap: 0.25rem;
        }
        .message-attachment { margin-top: 0.5rem; }
        .attachment-image-link { display: block; }
        .attachment-image { max-width: 280px; max-height: 200px; border-radius: 8px; object-fit: cover; }
        .attachment-file {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 0.75rem;
            background: #f0f0f0;
            border-radius: 8px;
            color: var(--color-primary);
            text-decoration: none;
        }
        .attachment-file:hover { background: #e8e8e8; }

        /* ============ RESPONSIVE ============ */
        @media (max-width: 768px) {
            .chat-grid {
                grid-template-columns: 1fr;
            }
            .sidebar-card {
                height: auto;
                max-height: 200px;
            }
            .chat-card {
                height: calc(100vh - 500px);
                min-height: 400px;
            }
            .message-content {
                max-width: 85%;
            }
        }
    `]
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
    @ViewChild('messagesContainer') messagesContainer!: ElementRef;

    conversation: SupportConversationDto | null = null;
    messages: SupportMessageDto[] = [];
    messageText = '';
    pendingAttachment: { url: string; fileName: string } | null = null;
    conversationsLoading = false;
    messagesLoading = false;
    sending = false;
    private shouldScroll = false;

    private supportApi = inject(SupportChatApiService);
    private hub = inject(SupportChatHubService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    public languageService = inject(LanguageService);

    ngOnInit() {
        this.conversationsLoading = true;
        this.supportApi.getMine().subscribe({
            next: (conv) => {
                this.conversation = conv;
                this.conversationsLoading = false;
                this.loadMessages();
                this.startHub();
            },
            error: () => {
                this.conversationsLoading = false;
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load chat' });
            }
        });
    }

    private startHub() {
        this.hub.onReceiveMessage((msg) => {
            if (this.conversation && msg.conversationId === this.conversation.id) {
                this.messages = [...this.messages, msg];
                this.shouldScroll = true;
            }
        });
        this.hub.start().then(() => this.hub.joinMyConversation()).catch(() => {});
    }

    ngOnDestroy() {
        this.hub.stop();
    }

    ngAfterViewChecked() {
        if (this.shouldScroll) {
            this.scrollToBottom();
            this.shouldScroll = false;
        }
    }

    loadMessages() {
        if (!this.conversation) return;
        this.messagesLoading = true;
        this.supportApi.getMessages(this.conversation.id).subscribe({
            next: (items) => {
                this.messages = items || [];
                this.messagesLoading = false;
                this.shouldScroll = true;
            },
            error: () => {
                this.messagesLoading = false;
            }
        });
    }

    sendMessage() {
        if ((!this.messageText.trim() && !this.pendingAttachment) || !this.conversation || this.sending) return;
        const content = this.messageText.trim();
        const att = this.pendingAttachment;
        this.messageText = '';
        this.pendingAttachment = null;
        this.sending = true;
        this.hub.sendMessage(this.conversation.id, content, att?.url, att?.fileName).then(() => {
            this.sending = false;
            this.shouldScroll = true;
        }).catch(() => {
            this.sending = false;
            this.messageText = content;
            if (att) this.pendingAttachment = att;
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to send message' });
        });
    }

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        input.value = '';
        if (!file || !this.conversation) return;
        if (file.size > 10 * 1024 * 1024) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('chat.attachmentTooBig'), detail: '' });
            return;
        }
        this.sending = true;
        this.supportApi.uploadAttachment(this.conversation.id, file).subscribe({
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

    scrollToBottom() {
        if (this.messagesContainer) {
            const element = this.messagesContainer.nativeElement;
            element.scrollTop = element.scrollHeight;
        }
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleDateString();
    }

    formatTime(date: string): string {
        return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
}
