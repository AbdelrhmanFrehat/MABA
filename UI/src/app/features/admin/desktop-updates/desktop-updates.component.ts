import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { environment } from '../../../../environments/environment';

interface DesktopUpdateManifest {
    version: string;
    packageUri: string;
    notes: string;
    publishedAt: string;
}

interface ChannelInfo {
    channel: string;
    latestManifest: DesktopUpdateManifest | null;
    manifestUrl: string;
    packages: string[];
}

@Component({
    selector: 'app-desktop-updates',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, TagModule, ToastModule, TooltipModule],
    providers: [MessageService],
    template: `
    <p-toast />
    <div class="du-page">

        <!-- Header -->
        <div class="du-header">
            <div>
                <h1>Desktop App Updates</h1>
                <p class="du-sub">Manage the MABA Control Center update feed. The desktop app polls the manifest URL on startup.</p>
            </div>
            <p-button label="Refresh" icon="pi pi-refresh" [outlined]="true" size="small" (click)="load()" />
        </div>

        <!-- Channels list -->
        <div class="channels-grid">
            @for (ch of channels(); track ch.channel) {
                <div class="channel-card">
                    <div class="ch-head">
                        <span class="ch-name">{{ ch.channel | titlecase }}</span>
                        <p-tag *ngIf="ch.latestManifest" [value]="'v' + ch.latestManifest.version" severity="success" />
                        <p-tag *ngIf="!ch.latestManifest" value="No releases" severity="secondary" />
                    </div>

                    @if (ch.latestManifest) {
                        <div class="ch-info">
                            <div class="ch-row"><span class="ch-lbl">Version</span><span class="ch-val mono">{{ ch.latestManifest.version }}</span></div>
                            <div class="ch-row"><span class="ch-lbl">Package</span><span class="ch-val mono">{{ ch.latestManifest.packageUri }}</span></div>
                            <div class="ch-row"><span class="ch-lbl">Published</span><span class="ch-val">{{ ch.latestManifest.publishedAt | date:'medium' }}</span></div>
                            <div class="ch-row"><span class="ch-lbl">Notes</span><span class="ch-val">{{ ch.latestManifest.notes }}</span></div>
                        </div>
                    }

                    <div class="ch-manifest">
                        <span class="manifest-label">Manifest URL (point desktop app here):</span>
                        <div class="manifest-url-row">
                            <code class="manifest-url">{{ ch.manifestUrl }}</code>
                            <button class="copy-btn" (click)="copy(ch.manifestUrl)" title="Copy URL">
                                <i class="pi pi-copy"></i>
                            </button>
                        </div>
                    </div>

                    @if (ch.packages.length > 0) {
                        <div class="packages-section">
                            <span class="pkg-title">Hosted packages ({{ ch.packages.length }})</span>
                            @for (pkg of ch.packages; track pkg) {
                                <div class="pkg-row">
                                    <i class="pi pi-file-export"></i>
                                    <span class="pkg-name">{{ pkg }}</span>
                                    <button class="pkg-del-btn" (click)="deletePackage(ch.channel, pkg)" title="Delete package">
                                        <i class="pi pi-trash"></i>
                                    </button>
                                </div>
                            }
                        </div>
                    }
                </div>
            }

            @if (channels().length === 0 && !loading()) {
                <div class="empty-state">
                    <i class="pi pi-desktop"></i>
                    <p>No update channels yet. Publish the first release below.</p>
                </div>
            }
        </div>

        <!-- Publish form -->
        <div class="publish-card">
            <div class="pub-title">
                <i class="pi pi-cloud-upload"></i>
                Publish New Release
            </div>

            <div class="pub-form">
                <div class="pub-row">
                    <div class="pf">
                        <label>Version *</label>
                        <input class="p-inputtext" [(ngModel)]="form.version" placeholder="e.g. 0.1.2" />
                    </div>
                    <div class="pf">
                        <label>Channel</label>
                        <select class="native-select" [(ngModel)]="form.channel">
                            <option value="stable">Stable</option>
                            <option value="beta">Beta</option>
                        </select>
                    </div>
                </div>

                <div class="pf">
                    <label>Release Package (.zip) *</label>
                    <input type="file" #fileInput accept=".zip" class="hidden-file"
                        (change)="onFileSelected($event)" />
                    <div class="file-drop" (click)="fileInput.click()">
                        <i class="pi pi-upload"></i>
                        <span>{{ form.file ? form.file.name + ' (' + formatSize(form.file.size) + ')' : 'Click to select .zip package' }}</span>
                    </div>
                </div>

                <div class="pf">
                    <label>Release Notes</label>
                    <textarea class="p-inputtextarea" rows="3" [(ngModel)]="form.notes"
                        placeholder="What's new in this release…"></textarea>
                </div>

                <div class="pub-actions">
                    <div class="pub-hint" *ngIf="form.version && form.file">
                        <i class="pi pi-info-circle"></i>
                        Will publish to:
                        <code>/desktop-updates/{{ form.channel }}/{{ form.file.name }}</code>
                    </div>
                    <p-button
                        label="Publish Release"
                        icon="pi pi-cloud-upload"
                        [loading]="publishing()"
                        [disabled]="!form.version || !form.file"
                        (click)="publish()">
                    </p-button>
                </div>
            </div>
        </div>
    </div>
    `,
    styles: [`
        .du-page { padding: 1.5rem; max-width: 900px; }
        .du-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
        .du-header h1 { margin: 0 0 0.25rem; font-size: 1.35rem; font-weight: 700; }
        .du-sub { margin: 0; color: #6b7280; font-size: 0.875rem; }

        /* Channels */
        .channels-grid { display: flex; flex-direction: column; gap: 1rem; margin-bottom: 1.5rem; }
        .channel-card { border: 1px solid #e5e7eb; border-radius: 10px; padding: 1.25rem; background: #fff; }
        .ch-head { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.85rem; }
        .ch-name { font-size: 1rem; font-weight: 700; color: #1a1a2e; }
        .ch-info { display: flex; flex-direction: column; gap: 0.35rem; margin-bottom: 0.85rem; }
        .ch-row { display: flex; gap: 0.75rem; font-size: 0.85rem; align-items: baseline; }
        .ch-lbl { min-width: 5.5rem; color: #9ca3af; font-size: 0.78rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
        .ch-val { color: #374151; }
        .mono { font-family: monospace; font-size: 0.82rem; }

        .ch-manifest { background: #f8f9ff; border-radius: 8px; padding: 0.65rem 0.85rem; margin-bottom: 0.75rem; }
        .manifest-label { font-size: 0.75rem; color: #6b7280; font-weight: 600; display: block; margin-bottom: 0.35rem; }
        .manifest-url-row { display: flex; align-items: center; gap: 0.5rem; }
        .manifest-url { font-family: monospace; font-size: 0.82rem; color: #4338ca; word-break: break-all; flex: 1; }
        .copy-btn { background: none; border: none; cursor: pointer; color: #667eea; padding: 0.2rem; border-radius: 4px; display: flex; align-items: center; }
        .copy-btn:hover { background: #eff2ff; }

        .packages-section { border-top: 1px solid #f3f4f6; padding-top: 0.65rem; }
        .pkg-title { font-size: 0.72rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #9ca3af; display: block; margin-bottom: 0.4rem; }
        .pkg-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.3rem 0; font-size: 0.83rem; color: #374151; }
        .pkg-name { flex: 1; font-family: monospace; font-size: 0.78rem; }
        .pkg-del-btn { background: none; border: none; color: #ef4444; cursor: pointer; padding: 0.2rem; border-radius: 4px; opacity: 0.6; }
        .pkg-del-btn:hover { opacity: 1; background: #fee2e2; }

        .empty-state { text-align: center; padding: 2rem; color: #9ca3af; font-size: 0.9rem; }
        .empty-state i { font-size: 2rem; display: block; margin-bottom: 0.5rem; }

        /* Publish card */
        .publish-card { border: 1px solid #e0e7ff; border-radius: 10px; padding: 1.25rem; background: #fafbff; }
        .pub-title { font-size: 0.95rem; font-weight: 700; color: #4338ca; display: flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; }
        .pub-form { display: flex; flex-direction: column; gap: 0.85rem; }
        .pub-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .pf { display: flex; flex-direction: column; gap: 0.3rem; }
        .pf label { font-size: 0.8rem; font-weight: 600; color: #374151; }
        .p-inputtext, .p-inputtextarea, .native-select { width: 100%; padding: 0.5rem 0.75rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 0.875rem; box-sizing: border-box; }
        .native-select { height: 36px; background: #fff; }
        textarea { resize: vertical; min-height: 60px; }
        .hidden-file { display: none; }
        .file-drop { display: flex; align-items: center; gap: 0.6rem; padding: 0.65rem 0.85rem; border: 1.5px dashed #c7d2fe; border-radius: 8px; background: #fff; cursor: pointer; font-size: 0.85rem; color: #667eea; transition: background 0.15s; }
        .file-drop:hover { background: #eff2ff; }
        .pub-actions { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; }
        .pub-hint { font-size: 0.78rem; color: #6b7280; display: flex; align-items: center; gap: 0.35rem; }
        .pub-hint code { background: #f3f4f6; padding: 0.1rem 0.35rem; border-radius: 4px; font-family: monospace; }
    `]
})
export class DesktopUpdatesComponent implements OnInit {
    private http = inject(HttpClient);
    private msg = inject(MessageService);

    private readonly base = `${environment.apiUrl}/desktop-updates`;

    channels = signal<ChannelInfo[]>([]);
    loading = signal(false);
    publishing = signal(false);

    form = { version: '', channel: 'stable', notes: '', file: null as File | null };

    ngOnInit() { this.load(); }

    load() {
        this.loading.set(true);
        this.http.get<ChannelInfo[]>(`${this.base}/channels`).subscribe({
            next: (c) => { this.channels.set(c); this.loading.set(false); },
            error: () => { this.loading.set(false); }
        });
    }

    onFileSelected(event: Event) {
        const f = (event.target as HTMLInputElement).files?.[0] ?? null;
        this.form.file = f;
    }

    publish() {
        if (!this.form.version.trim() || !this.form.file) return;
        this.publishing.set(true);

        const fd = new FormData();
        fd.append('file', this.form.file);
        fd.append('version', this.form.version.trim());
        fd.append('channel', this.form.channel);
        if (this.form.notes.trim()) fd.append('notes', this.form.notes.trim());

        this.http.post<DesktopUpdateManifest>(`${this.base}/publish`, fd).subscribe({
            next: (manifest) => {
                this.publishing.set(false);
                this.form = { version: '', channel: 'stable', notes: '', file: null };
                this.msg.add({ severity: 'success', summary: 'Published', detail: `v${manifest.version} is now live on the ${this.form.channel || 'stable'} channel.`, life: 5000 });
                this.load();
            },
            error: (err) => {
                this.publishing.set(false);
                this.msg.add({ severity: 'error', summary: 'Error', detail: err?.error?.message || 'Publish failed.', life: 5000 });
            }
        });
    }

    deletePackage(channel: string, pkg: string) {
        if (!confirm(`Delete ${pkg} from ${channel}? This cannot be undone.`)) return;
        this.http.delete(`${this.base}/channels/${channel}/packages/${encodeURIComponent(pkg)}`).subscribe({
            next: () => { this.msg.add({ severity: 'success', summary: 'Deleted', detail: pkg, life: 3000 }); this.load(); },
            error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.', life: 3000 })
        });
    }

    copy(url: string) {
        navigator.clipboard.writeText(url).then(() =>
            this.msg.add({ severity: 'info', summary: 'Copied', detail: 'Manifest URL copied to clipboard.', life: 2000 })
        );
    }

    formatSize(bytes: number): string {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }
}
