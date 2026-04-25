import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef, AfterViewChecked, NgZone } from '@angular/core';
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
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
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
        TooltipModule,
        DialogModule,
        TextareaModule
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <p-dialog
            [(visible)]="showNewDialog"
            [modal]="true"
            [draggable]="false"
            [style]="{ width: '95vw', maxWidth: '480px' }"
            [header]="'chat.newConversation' | translate">
            <div class="new-conv-form">
                <label class="field-label">{{ 'chat.subject' | translate }}</label>
                <input pInputText class="w-full" [(ngModel)]="newSubject" [placeholder]="'chat.subjectPlaceholder' | translate" />
                <label class="field-label">{{ 'chat.firstMessageOptional' | translate }}</label>
                <textarea pInputTextarea rows="4" class="w-full" [(ngModel)]="newInitialMessage" [placeholder]="'chat.firstMessagePlaceholder' | translate"></textarea>
                <button type="button" class="new-chat-btn" (click)="submitNewConversation()" [disabled]="creating || !newSubject.trim()">
                    <i *ngIf="creating" class="pi pi-spin pi-spinner"></i>
                    {{ 'chat.startConversationBtn' | translate }}
                </button>
            </div>
        </p-dialog>
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
                                <button type="button" class="new-chat-btn" (click)="openNewDialog()">
                                    <i class="pi pi-plus"></i>
                                    {{ 'chat.startConversationBtn' | translate }}
                                </button>
                                <div *ngIf="conversationsLoading" class="loading-state">
                                    <i class="pi pi-spin pi-spinner"></i>
                                </div>
                                <div *ngIf="loadError" class="error-banner">{{ 'chat.loadFailed' | translate }}</div>
                                <div class="conversations-list">
                                    <div
                                        *ngFor="let c of conversations"
                                        class="ticket-item"
                                        [class.active]="selectedConversation?.id === c.id"
                                        [class.ticket-closed]="c.status === 1"
                                        (click)="selectConversation(c)">
                                        <div class="ticket-top-row">
                                            <span class="ticket-ref">{{ ticketRef(c.id) }}</span>
                                            <span class="ticket-status" [class.open]="c.status === 0" [class.closed]="c.status === 1">
                                                {{ c.status === 0 ? ('chat.open' | translate) : ('chat.closed' | translate) }}
                                            </span>
                                        </div>
                                        <div class="ticket-subject">{{ c.subject || ('chat.supportThread' | translate) }}</div>
                                        <div class="ticket-preview" *ngIf="c.lastMessagePreview">{{ c.lastMessagePreview }}</div>
                                        <div class="ticket-preview muted" *ngIf="!c.lastMessagePreview">{{ 'chat.noMessagesPreview' | translate }}</div>
                                        <div class="ticket-date">{{ c.lastMessageAt ? formatDate(c.lastMessageAt) : formatDate(c.createdAt) }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Chat Main -->
                        <div class="chat-main">
                            <div *ngIf="!conversationsLoading && conversations.length === 0" class="chat-card empty-main">
                                <div class="empty-chat">
                                    <div class="empty-icon">
                                        <i class="pi pi-comments"></i>
                                    </div>
                                    <h3>{{ 'chat.noConversationsYet' | translate }}</h3>
                                    <p>{{ 'chat.noConversationsHint' | translate }}</p>
                                    <button type="button" class="new-chat-btn" (click)="openNewDialog()">
                                        {{ 'chat.startConversationBtn' | translate }}
                                    </button>
                                </div>
                            </div>
                            <div class="chat-card" *ngIf="selectedConversation">
                                <div class="thread-header">
                                    <div>
                                        <div class="thread-ref-row">
                                            <span class="thread-ref">{{ ticketRef(selectedConversation.id) }}</span>
                                            <span class="ticket-status" [class.open]="selectedConversation.status === 0" [class.closed]="selectedConversation.status === 1">
                                                {{ selectedConversation.status === 0 ? ('chat.open' | translate) : ('chat.closed' | translate) }}
                                            </span>
                                        </div>
                                        <h2 class="thread-title">{{ selectedConversation.subject || ('chat.supportThread' | translate) }}</h2>
                                    </div>
                                    <button
                                        *ngIf="selectedConversation.status === 0"
                                        type="button"
                                        class="close-thread-btn"
                                        (click)="closeMyConversation()"
                                        [disabled]="closing">
                                        {{ 'chat.closeConversation' | translate }}
                                    </button>
                                </div>
                                <div class="closed-banner" *ngIf="selectedConversation.status === 1">
                                    {{ 'chat.closedHint' | translate }}
                                </div>
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
                                            [disabled]="!canSendMessages || sending"
                                            [pTooltip]="'chat.attach' | translate">
                                            <i class="pi pi-paperclip"></i>
                                        </button>
                                        <input 
                                            type="text" 
                                            [(ngModel)]="messageText"
                                            [placeholder]="'chat.typeMessage' | translate"
                                            (keyup.enter)="sendMessage()"
                                            [disabled]="!canSendMessages || sending">
                                        <span class="pending-attachment" *ngIf="pendingAttachment">
                                            <i class="pi pi-file"></i> {{ pendingAttachment.fileName }}
                                        </span>
                                        <button 
                                            class="send-btn"
                                            (click)="sendMessage()"
                                            [disabled]="(!messageText.trim() && !pendingAttachment) || !canSendMessages || sending">
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

        /* ---- Ticket cards ---- */
        .ticket-item {
            padding: 0.85rem 0.9rem;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.2s;
            border: 1px solid #e9ecef;
            margin-bottom: 0.5rem;
            background: white;
        }
        .ticket-item:hover { border-color: var(--color-primary); background: #fafbff; }
        .ticket-item.active { border-color: var(--color-primary); background: linear-gradient(135deg, rgba(102,126,234,0.08) 0%, rgba(118,75,162,0.08) 100%); }
        .ticket-item.ticket-closed { opacity: 0.75; }
        .ticket-top-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.3rem; }
        .ticket-ref { font-size: 0.72rem; font-weight: 700; color: #667eea; letter-spacing: 0.5px; font-family: monospace; }
        .ticket-subject { font-weight: 600; font-size: 0.88rem; color: #1a1a2e; margin-bottom: 0.25rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .ticket-preview { font-size: 0.78rem; color: #6c757d; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-bottom: 0.2rem; }
        .ticket-preview.muted { font-style: italic; }
        .ticket-date { font-size: 0.72rem; color: #aaa; }
        .ticket-status {
            font-size: 0.68rem; font-weight: 700; padding: 0.15rem 0.5rem;
            border-radius: 999px; text-transform: uppercase; letter-spacing: 0.4px;
        }
        .ticket-status.open { background: #d1fae5; color: #065f46; }
        .ticket-status.closed { background: #f3f4f6; color: #6b7280; }
        /* Thread header ref */
        .thread-ref-row { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.25rem; }
        .thread-ref { font-size: 0.78rem; font-weight: 700; color: #667eea; font-family: monospace; letter-spacing: 0.5px; }
        .error-banner {
            color: #c62828;
            font-size: 0.9rem;
            padding: 0.5rem 0;
        }
        .thread-header {
            display: flex;
            align-items: flex-start;
            justify-content: space-between;
            gap: 1rem;
            padding: 1rem 1.5rem 0;
            border-bottom: 1px solid #e9ecef;
        }
        .thread-title {
            margin: 0 0 0.25rem 0;
            font-size: 1.15rem;
            font-weight: 700;
            color: #1a1a2e;
        }
        .close-thread-btn {
            background: transparent;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 0.4rem 0.75rem;
            cursor: pointer;
            font-size: 0.85rem;
            color: #555;
        }
        .close-thread-btn:hover:not(:disabled) {
            border-color: var(--color-primary);
            color: var(--color-primary);
        }
        .closed-banner {
            background: #fff8e1;
            color: #6d4c41;
            padding: 0.75rem 1.5rem;
            font-size: 0.9rem;
        }
        .empty-main {
            justify-content: center;
            align-items: center;
        }
        .new-conv-form {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }
        .field-label {
            font-size: 0.85rem;
            font-weight: 600;
            color: #333;
        }
        .w-full { width: 100%; box-sizing: border-box; }

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

    conversations: SupportConversationDto[] = [];
    selectedConversation: SupportConversationDto | null = null;
    messages: SupportMessageDto[] = [];
    messageText = '';
    pendingAttachment: { url: string; fileName: string } | null = null;
    conversationsLoading = false;
    messagesLoading = false;
    sending = false;
    loadError = false;
    showNewDialog = false;
    newSubject = '';
    newInitialMessage = '';
    creating = false;
    closing = false;
    private shouldScroll = false;

    private supportApi = inject(SupportChatApiService);
    private hub = inject(SupportChatHubService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private zone = inject(NgZone);
    public languageService = inject(LanguageService);

    private refreshInterval: ReturnType<typeof setInterval> | null = null;

    get canSendMessages(): boolean {
        return !!this.selectedConversation && this.selectedConversation.status === 0;
    }

    ticketRef(id: string): string {
        return '#SUP-' + id.replace(/-/g, '').slice(-6).toUpperCase();
    }

    ngOnInit() {
        // Auto-refresh every 30s to pick up admin replies and keep previews current
        this.zone.runOutsideAngular(() => {
            this.refreshInterval = setInterval(() => {
                this.zone.run(() => this.silentRefresh());
            }, 30000);
        });

        this.hub.onReceiveMessage((msg) => {
            if (this.selectedConversation && msg.conversationId === this.selectedConversation.id) {
                if (!this.messages.some((m) => m.id === msg.id)) {
                    this.messages = [...this.messages, msg];
                    this.shouldScroll = true;
                }
            }
            this.patchConversationPreview(msg);
        });
        this.loadConversations();
    }

    private patchConversationPreview(msg: SupportMessageDto) {
        const idx = this.conversations.findIndex((c) => c.id === msg.conversationId);
        if (idx < 0) return;
        const c = this.conversations[idx];
        this.conversations = [
            ...this.conversations.slice(0, idx),
            {
                ...c,
                lastMessagePreview: msg.content?.slice(0, 80) || c.lastMessagePreview,
                lastMessageAt: msg.createdAt,
                updatedAt: msg.createdAt
            },
            ...this.conversations.slice(idx + 1)
        ];
    }

    private startHub() {
        this.hub.start().then(() => this.hub.joinMyConversations()).catch(() => {});
    }

    loadConversations() {
        this.conversationsLoading = true;
        this.loadError = false;
        this.supportApi.getMine().subscribe({
            next: (list) => {
                this.conversations = list || [];
                this.conversationsLoading = false;
                if (this.conversations.length > 0) {
                    const stillThere = this.selectedConversation
                        ? this.conversations.some((c) => c.id === this.selectedConversation!.id)
                        : false;
                    if (!this.selectedConversation || !stillThere) {
                        this.selectConversation(this.conversations[0]);
                    } else {
                        const updated = this.conversations.find((c) => c.id === this.selectedConversation!.id);
                        if (updated) this.selectedConversation = updated;
                        this.loadMessages();
                    }
                } else {
                    this.selectedConversation = null;
                    this.messages = [];
                }
                this.startHub();
            },
            error: () => {
                this.conversationsLoading = false;
                this.loadError = true;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: this.translate.instant('chat.loadFailed')
                });
            }
        });
    }

    selectConversation(c: SupportConversationDto) {
        this.selectedConversation = c;
        this.loadMessages();
        this.hub.joinConversation(c.id).catch(() => {});
    }

    openNewDialog() {
        this.newSubject = '';
        this.newInitialMessage = '';
        this.showNewDialog = true;
    }

    submitNewConversation() {
        const subject = this.newSubject.trim();
        if (!subject || this.creating) return;
        this.creating = true;
        this.supportApi
            .createConversation({
                subject,
                initialMessage: this.newInitialMessage.trim() || undefined
            })
            .subscribe({
                next: (conv) => {
                    this.creating = false;
                    this.showNewDialog = false;
                    this.conversations = [conv, ...this.conversations.filter((x) => x.id !== conv.id)];
                    this.selectConversation(conv);
                    this.hub.joinMyConversations().catch(() => {});
                    this.messageService.add({
                        severity: 'success',
                        summary: this.translate.instant('common.success'),
                        detail: this.translate.instant('chat.conversationCreated')
                    });
                },
                error: () => {
                    this.creating = false;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.translate.instant('common.error'),
                        detail: this.translate.instant('chat.createFailed')
                    });
                }
            });
    }

    closeMyConversation() {
        if (!this.selectedConversation || this.selectedConversation.status !== 0 || this.closing) return;
        this.closing = true;
        this.supportApi.closeConversation(this.selectedConversation.id).subscribe({
            next: () => {
                this.closing = false;
                const id = this.selectedConversation!.id;
                this.conversations = this.conversations.map((c) =>
                    c.id === id ? { ...c, status: 1 as const } : c
                );
                this.selectedConversation = this.conversations.find((c) => c.id === id) || null;
            },
            error: () => {
                this.closing = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: this.translate.instant('chat.closeFailed')
                });
            }
        });
    }

    ngOnDestroy() {
        if (this.refreshInterval) clearInterval(this.refreshInterval);
        this.hub.stop();
    }

    /** Reload conversations list without resetting selection or showing spinners */
    private silentRefresh(): void {
        this.supportApi.getMine().subscribe({
            next: (list) => {
                this.conversations = list || [];
                // Refresh messages if a conversation is open
                if (this.selectedConversation) {
                    const updated = this.conversations.find(c => c.id === this.selectedConversation!.id);
                    if (updated) this.selectedConversation = updated;
                    this.loadMessages();
                }
            },
            error: () => {} // silent — don't show errors on background refresh
        });
    }

    ngAfterViewChecked() {
        if (this.shouldScroll) {
            this.scrollToBottom();
            this.shouldScroll = false;
        }
    }

    loadMessages() {
        if (!this.selectedConversation) return;
        this.messagesLoading = true;
        this.supportApi.getMessages(this.selectedConversation.id).subscribe({
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
        if ((!this.messageText.trim() && !this.pendingAttachment) || !this.selectedConversation || !this.canSendMessages || this.sending) return;
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

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        input.value = '';
        if (!file || !this.selectedConversation || !this.canSendMessages) return;
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
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('common.error'),
                    detail: this.translate.instant('chat.uploadFailed')
                });
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
