import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpEventType, HttpRequest } from '@angular/common/http';
import { filter } from 'rxjs/operators';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { environment } from '../../../../environments/environment';

interface DesktopUpdateManifest {
    version: string; packageUri: string; notes: string; publishedAt: string;
}
interface ChannelInfo {
    channel: string; latestManifest: DesktopUpdateManifest | null;
    manifestUrl: string; packages: string[];
}
interface AppRuntimeMetadataDto {
    id: string; channel: string; appVersion: string;
    firmwareName: string; firmwareVersion: string; targetBoard: string;
    protocolName: string; commandSummary?: string; compatibilityNotes?: string;
    isActive: boolean; publishedAt?: string; createdAt: string;
}

@Component({
    selector: 'app-desktop-updates',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, TagModule, ToastModule,
              TooltipModule, DialogModule, CheckboxModule, InputTextModule],
    providers: [MessageService],
    template: `
    <p-toast />
    <div class="du-page">

        <!-- ── Header ── -->
        <div class="du-header">
            <div>
                <h1>Control Center — Releases &amp; Runtime</h1>
                <p class="du-sub">Desktop app releases, firmware metadata, and compatibility tracking for the MABA Control Center.</p>
            </div>
            <p-button label="Refresh" icon="pi pi-refresh" [outlined]="true" size="small" (click)="load()" />
        </div>

        <!-- ══ SECTION 1: Current Stable Runtime Stack ══ -->
        @if (currentRuntime()) {
            <div class="stack-box">
                <div class="stack-label">CURRENT STABLE RUNTIME STACK</div>
                <div class="stack-grid">
                    <div class="stack-item">
                        <span class="stack-key">Desktop App</span>
                        <span class="stack-val mono">v{{ currentRuntime()!.appVersion }}</span>
                    </div>
                    <div class="stack-item">
                        <span class="stack-key">Firmware</span>
                        <span class="stack-val mono">v{{ currentRuntime()!.firmwareVersion }}</span>
                    </div>
                    <div class="stack-item">
                        <span class="stack-key">Protocol</span>
                        <span class="stack-val">{{ currentRuntime()!.protocolName }}</span>
                    </div>
                    <div class="stack-item">
                        <span class="stack-key">Board</span>
                        <span class="stack-val">{{ currentRuntime()!.targetBoard }}</span>
                    </div>
                    <div class="stack-item">
                        <span class="stack-key">Firmware Name</span>
                        <span class="stack-val">{{ currentRuntime()!.firmwareName }}</span>
                    </div>
                    <div class="stack-item">
                        <span class="stack-key">Channel</span>
                        <p-tag [value]="currentRuntime()!.channel | titlecase"
                               [severity]="currentRuntime()!.channel === 'stable' ? 'success' : 'warn'" />
                    </div>
                </div>
                @if (currentRuntime()!.compatibilityNotes) {
                    <div class="stack-notes"><i class="pi pi-info-circle"></i> {{ currentRuntime()!.compatibilityNotes }}</div>
                }
            </div>
        }

        <!-- ══ SECTION 2: Release Feed ══ -->
        <div class="section-head">
            <span class="section-title"><i class="pi pi-desktop"></i> Desktop App Releases</span>
        </div>
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
                            <div class="ch-row"><span class="ch-lbl">App Version</span><code class="ch-code">{{ ch.latestManifest.version }}</code></div>
                            <div class="ch-row"><span class="ch-lbl">Package File</span><code class="ch-code">{{ ch.latestManifest.packageUri }}</code></div>
                            <div class="ch-row"><span class="ch-lbl">Published</span><span class="ch-val">{{ ch.latestManifest.publishedAt | date:'medium' }}</span></div>
                            <div class="ch-row"><span class="ch-lbl">Release Notes</span><span class="ch-val notes-text">{{ ch.latestManifest.notes }}</span></div>
                        </div>

                        <div class="ch-urls">
                            <div class="url-row">
                                <span class="url-lbl">Manifest URL</span>
                                <div class="url-val-row">
                                    <code class="url-val">{{ ch.manifestUrl }}</code>
                                    <button class="copy-btn" (click)="copy(ch.manifestUrl)" title="Copy"><i class="pi pi-copy"></i></button>
                                </div>
                            </div>
                            <div class="url-row">
                                <span class="url-lbl">Package URL</span>
                                <div class="url-val-row">
                                    <code class="url-val">{{ ch.manifestUrl.replace('manifest.json', ch.latestManifest.packageUri) }}</code>
                                    <button class="copy-btn" (click)="copy(ch.manifestUrl.replace('manifest.json', ch.latestManifest.packageUri))" title="Copy"><i class="pi pi-copy"></i></button>
                                </div>
                            </div>
                        </div>
                    }

                    @if (ch.packages.length > 0) {
                        <div class="packages-section">
                            <span class="pkg-title">Hosted packages ({{ ch.packages.length }})</span>
                            @for (pkg of ch.packages; track pkg) {
                                <div class="pkg-row">
                                    <i class="pi pi-file-export"></i>
                                    <span class="pkg-name">{{ pkg }}</span>
                                    <button class="pkg-del-btn" (click)="deletePackage(ch.channel, pkg)" title="Delete"><i class="pi pi-trash"></i></button>
                                </div>
                            }
                        </div>
                    }
                </div>
            }
            @if (channels().length === 0 && !loading()) {
                <div class="empty-state"><i class="pi pi-desktop"></i><p>No channels yet. Publish the first release below.</p></div>
            }
        </div>

        <!-- ══ SECTION 3: Firmware & Protocol Metadata ══ -->
        <div class="section-head">
            <span class="section-title"><i class="pi pi-microchip"></i> Firmware &amp; Protocol Metadata</span>
            <p-button label="Add Entry" icon="pi pi-plus" size="small" (click)="openRuntimeDialog(null)" />
        </div>

        <div class="section-head">
            <span class="section-title"><i class="pi pi-download"></i> Windows Installer Downloads</span>
        </div>
        <div class="channels-grid">
            @for (ch of installerChannels(); track ch.channel) {
                <div class="channel-card installer-card">
                    <div class="ch-head">
                        <span class="ch-name">{{ ch.channel | titlecase }} Installer</span>
                        <p-tag *ngIf="ch.latestManifest" [value]="'v' + ch.latestManifest.version" severity="info" />
                        <p-tag *ngIf="!ch.latestManifest" value="No installer" severity="secondary" />
                    </div>

                    @if (ch.latestManifest) {
                        <div class="ch-info">
                            <div class="ch-row"><span class="ch-lbl">Installer Version</span><code class="ch-code">{{ ch.latestManifest.version }}</code></div>
                            <div class="ch-row"><span class="ch-lbl">Installer File</span><code class="ch-code">{{ ch.latestManifest.packageUri }}</code></div>
                            <div class="ch-row"><span class="ch-lbl">Published</span><span class="ch-val">{{ ch.latestManifest.publishedAt | date:'medium' }}</span></div>
                            <div class="ch-row"><span class="ch-lbl">Notes</span><span class="ch-val notes-text">{{ ch.latestManifest.notes }}</span></div>
                        </div>

                        <div class="installer-actions">
                            <a class="dl-btn" [href]="buildPackageUrl(ch)" target="_blank" rel="noopener noreferrer">
                                <i class="pi pi-download"></i>
                                <span>Download Installer</span>
                            </a>
                            <button class="secondary-link-btn" (click)="copy(buildPackageUrl(ch))">
                                <i class="pi pi-copy"></i>
                                <span>Copy Download URL</span>
                            </button>
                        </div>

                        <div class="ch-urls">
                            <div class="url-row">
                                <span class="url-lbl">Manifest URL</span>
                                <div class="url-val-row">
                                    <code class="url-val">{{ ch.manifestUrl }}</code>
                                    <button class="copy-btn" (click)="copy(ch.manifestUrl)" title="Copy"><i class="pi pi-copy"></i></button>
                                </div>
                            </div>
                            <div class="url-row">
                                <span class="url-lbl">Installer URL</span>
                                <div class="url-val-row">
                                    <code class="url-val">{{ buildPackageUrl(ch) }}</code>
                                    <button class="copy-btn" (click)="copy(buildPackageUrl(ch))" title="Copy"><i class="pi pi-copy"></i></button>
                                </div>
                            </div>
                        </div>
                    }

                    @if (ch.packages.length > 0) {
                        <div class="packages-section">
                            <span class="pkg-title">Hosted installers ({{ ch.packages.length }})</span>
                            @for (pkg of ch.packages; track pkg) {
                                <div class="pkg-row">
                                    <i class="pi pi-desktop"></i>
                                    <a class="pkg-link" [href]="buildDirectPackageUrl(ch.manifestUrl, pkg)" target="_blank" rel="noopener noreferrer">{{ pkg }}</a>
                                </div>
                            }
                        </div>
                    }
                </div>
            }
            @if (installerChannels().length === 0 && !loading()) {
                <div class="empty-state"><i class="pi pi-download"></i><p>No installer releases yet. Publish an installer to make direct downloads available here.</p></div>
            }
        </div>

        <div class="runtime-table">
            <div class="rt-head">
                <div class="rt-c ch">Channel</div>
                <div class="rt-c av">App Ver.</div>
                <div class="rt-c fw">Firmware</div>
                <div class="rt-c board">Board</div>
                <div class="rt-c proto">Protocol</div>
                <div class="rt-c date">Published</div>
                <div class="rt-c status">Status</div>
                <div class="rt-c act">Actions</div>
            </div>
            @for (r of runtimeMetadata(); track r.id) {
                <div class="rt-row" [class.inactive]="!r.isActive">
                    <div class="rt-c ch"><p-tag [value]="r.channel | titlecase" [severity]="r.channel === 'stable' ? 'success' : 'warn'" /></div>
                    <div class="rt-c av mono">v{{ r.appVersion }}</div>
                    <div class="rt-c fw">
                        <span class="fw-name">{{ r.firmwareName }}</span>
                        <code class="fw-ver">v{{ r.firmwareVersion }}</code>
                    </div>
                    <div class="rt-c board">{{ r.targetBoard }}</div>
                    <div class="rt-c proto">{{ r.protocolName }}</div>
                    <div class="rt-c date">{{ r.publishedAt ? (r.publishedAt | date:'mediumDate') : '—' }}</div>
                    <div class="rt-c status">
                        <p-tag [value]="r.isActive ? 'Active' : 'Inactive'" [severity]="r.isActive ? 'success' : 'secondary'" />
                    </div>
                    <div class="rt-c act">
                        <button class="act-btn" (click)="openRuntimeDialog(r)" title="Edit"><i class="pi pi-pencil"></i></button>
                        <button class="act-btn act-danger" (click)="deleteRuntime(r)" title="Delete"><i class="pi pi-trash"></i></button>
                    </div>
                </div>
                @if (r.commandSummary || r.compatibilityNotes) {
                    <div class="rt-expand">
                        @if (r.commandSummary) {
                            <div class="rt-detail"><span class="rt-dl">Commands:</span> {{ r.commandSummary }}</div>
                        }
                        @if (r.compatibilityNotes) {
                            <div class="rt-detail"><span class="rt-dl">Compatibility:</span> {{ r.compatibilityNotes }}</div>
                        }
                    </div>
                }
            }
            @if (runtimeMetadata().length === 0) {
                <div class="empty-state small">No firmware metadata entries yet. Add an entry to track app/firmware compatibility.</div>
            }
        </div>

        <!-- ══ SECTION 4: Publish New Release ══ -->
        <div class="section-head">
            <span class="section-title"><i class="pi pi-cloud-upload"></i> Publish New Desktop Release</span>
        </div>
        <div class="publish-card">
            <div class="pub-form">
                <div class="pub-row">
                    <div class="pf"><label>Version *</label><input class="p-inputtext" [(ngModel)]="form.version" placeholder="e.g. 0.1.6" /></div>
                    <div class="pf"><label>Channel</label>
                        <select class="native-select" [(ngModel)]="form.channel">
                            <option value="stable">Stable</option>
                            <option value="beta">Beta</option>
                        </select>
                    </div>
                </div>
                <div class="pf">
                    <label>Release Package (.zip) *</label>
                    <input type="file" #fileInput accept=".zip" class="hidden-file" (change)="onFileSelected($event)" />
                    <div class="file-drop" (click)="fileInput.click()">
                        <i class="pi pi-upload"></i>
                        <span>{{ form.file ? form.file.name + ' (' + formatSize(form.file.size) + ')' : 'Click to select .zip package' }}</span>
                    </div>
                </div>
                <div class="pf"><label>Release Notes</label>
                    <textarea class="p-inputtextarea" rows="3" [(ngModel)]="form.notes" placeholder="What's new…"></textarea>
                </div>

                <div class="progress-section" *ngIf="uploadPct > 0 && uploadPct < 100">
                    <div class="progress-label">
                        <span>Uploading… {{ uploadPct }}%</span>
                        <span class="progress-size">{{ uploadedMB }} / {{ totalMB }} MB</span>
                    </div>
                    <div class="progress-track"><div class="progress-fill" [style.width.%]="uploadPct"></div></div>
                </div>
                <div class="progress-section processing" *ngIf="uploadPct === 100 && publishing()">
                    <i class="pi pi-spin pi-spinner"></i> Writing to server…
                </div>

                <div class="pub-actions">
                    <div class="pub-hint" *ngIf="form.version && form.file">
                        <i class="pi pi-info-circle"></i>
                        Will publish to: <code>/desktop-updates/{{ form.channel }}/{{ form.file.name }}</code>
                    </div>
                    <p-button label="Publish Release" icon="pi pi-cloud-upload" [loading]="publishing()"
                        [disabled]="!form.version || !form.file || publishing()" (click)="publish()">
                    </p-button>
                </div>
            </div>
        </div>
    </div>

    <!-- ── Runtime metadata dialog ── -->
    <p-dialog [(visible)]="rtDlg.visible" [modal]="true" [style]="{width:'580px'}" [draggable]="false">
        <ng-template pTemplate="header">
            <div class="dlg-header">
                <span class="dlg-label">Control Center</span>
                <span class="dlg-title">{{ rtDlg.isEdit ? 'Edit' : 'Add' }} Runtime Metadata</span>
            </div>
        </ng-template>
        <div class="rt-form">
            <div class="rf-row">
                <div class="rf"><label>App Version *</label><input pInputText [(ngModel)]="rtDlg.d.appVersion" class="w-full" placeholder="0.1.5" /></div>
                <div class="rf"><label>Channel</label>
                    <select class="native-select w-full" [(ngModel)]="rtDlg.d.channel">
                        <option value="stable">Stable</option><option value="beta">Beta</option>
                    </select>
                </div>
            </div>
            <div class="rf-row">
                <div class="rf"><label>Firmware Name</label><input pInputText [(ngModel)]="rtDlg.d.firmwareName" class="w-full" placeholder="MABA CNC Motion Firmware" /></div>
                <div class="rf"><label>Firmware Version</label><input pInputText [(ngModel)]="rtDlg.d.firmwareVersion" class="w-full" placeholder="2.0.0" /></div>
            </div>
            <div class="rf-row">
                <div class="rf"><label>Target Board</label><input pInputText [(ngModel)]="rtDlg.d.targetBoard" class="w-full" placeholder="Arduino Uno" /></div>
                <div class="rf"><label>Protocol Name</label><input pInputText [(ngModel)]="rtDlg.d.protocolName" class="w-full" placeholder="MABA CNC Serial v2" /></div>
            </div>
            <div class="rf"><label>Command Summary <span class="opt">(optional — no source code)</span></label>
                <textarea pInputText rows="3" [(ngModel)]="rtDlg.d.commandSummary" class="w-full"
                    placeholder="e.g. G0 rapid move, G1 linear, G28 home, M3/M5 spindle, custom MABA extensions for CNC setup"></textarea>
            </div>
            <div class="rf"><label>Compatibility Notes <span class="opt">(optional)</span></label>
                <textarea pInputText rows="2" [(ngModel)]="rtDlg.d.compatibilityNotes" class="w-full"
                    placeholder="e.g. Requires firmware v2.0.0+. Breaks with v1.x. Tested on Uno R3."></textarea>
            </div>
            <div class="rf-row">
                <label class="check-lbl"><p-checkbox [(ngModel)]="rtDlg.d.isActive" [binary]="true"></p-checkbox> Active</label>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="Cancel" [text]="true" (click)="rtDlg.visible = false" />
            <p-button label="Save" [loading]="rtDlg.saving" (click)="saveRuntime()" />
        </ng-template>
    </p-dialog>
    `,
    styles: [`
        .du-page { padding: 1.5rem; max-width: 1000px; }
        .du-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.25rem; }
        .du-header h1 { margin: 0 0 0.25rem; font-size: 1.3rem; font-weight: 700; }
        .du-sub { margin: 0; color: #6b7280; font-size: 0.875rem; }

        /* Stack box */
        .stack-box { background: linear-gradient(135deg, #0c1445 0%, #1a1a2e 60%, #4a2080 100%); border-radius: 12px; padding: 1.1rem 1.4rem; margin-bottom: 1.5rem; color: #fff; }
        .stack-label { font-size: 0.68rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.1em; color: rgba(255,255,255,0.5); margin-bottom: 0.75rem; }
        .stack-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.65rem 1.5rem; }
        .stack-item { display: flex; flex-direction: column; gap: 0.1rem; }
        .stack-key { font-size: 0.72rem; color: rgba(255,255,255,0.5); text-transform: uppercase; letter-spacing: 0.05em; }
        .stack-val { font-size: 0.9rem; color: #fff; font-weight: 600; }
        .stack-notes { margin-top: 0.75rem; font-size: 0.8rem; color: rgba(255,255,255,0.65); display: flex; align-items: center; gap: 0.35rem; border-top: 1px solid rgba(255,255,255,0.1); padding-top: 0.5rem; }

        /* Section headers */
        .section-head { display: flex; align-items: center; justify-content: space-between; margin: 1.5rem 0 0.75rem; border-bottom: 1px solid #e5e7eb; padding-bottom: 0.5rem; }
        .section-title { font-size: 0.85rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.07em; color: #4338ca; display: flex; align-items: center; gap: 0.4rem; }

        /* Channel cards */
        .channels-grid { display: flex; flex-direction: column; gap: 1rem; margin-bottom: 0.5rem; }
        .channel-card { border: 1px solid #e5e7eb; border-radius: 10px; padding: 1.1rem; background: #fff; }
        .ch-head { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; }
        .ch-name { font-size: 1rem; font-weight: 700; color: #1a1a2e; }
        .ch-info { display: flex; flex-direction: column; gap: 0.3rem; margin-bottom: 0.75rem; }
        .ch-row { display: flex; gap: 0.75rem; font-size: 0.84rem; align-items: baseline; flex-wrap: wrap; }
        .ch-lbl { min-width: 6rem; color: #9ca3af; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; flex-shrink: 0; }
        .ch-val { color: #374151; }
        .notes-text { white-space: pre-wrap; line-height: 1.5; }
        .ch-code { font-family: monospace; font-size: 0.82rem; color: #4338ca; background: #f0f2ff; padding: 0.1rem 0.3rem; border-radius: 3px; }
        .ch-urls { background: #f8f9ff; border-radius: 8px; padding: 0.65rem 0.85rem; display: flex; flex-direction: column; gap: 0.5rem; }
        .installer-card { border-color: #dbeafe; background: linear-gradient(180deg, #ffffff 0%, #f8fbff 100%); }
        .installer-actions { display: flex; gap: 0.75rem; flex-wrap: wrap; margin: 0.5rem 0 0.85rem; }
        .dl-btn, .secondary-link-btn { display: inline-flex; align-items: center; gap: 0.45rem; border-radius: 8px; padding: 0.55rem 0.85rem; font-size: 0.84rem; font-weight: 600; text-decoration: none; cursor: pointer; }
        .dl-btn { background: linear-gradient(135deg, #4338ca, #6366f1); color: #fff; border: none; }
        .dl-btn:hover { opacity: 0.95; }
        .secondary-link-btn { background: #fff; color: #4338ca; border: 1px solid #c7d2fe; }
        .secondary-link-btn:hover { background: #eef2ff; }
        .url-row { display: flex; flex-direction: column; gap: 0.2rem; }
        .url-lbl { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; color: #9ca3af; letter-spacing: 0.05em; }
        .url-val-row { display: flex; align-items: center; gap: 0.4rem; }
        .url-val { font-family: monospace; font-size: 0.78rem; color: #4338ca; word-break: break-all; flex: 1; }
        .copy-btn { background: none; border: none; cursor: pointer; color: #667eea; padding: 0.2rem; border-radius: 4px; flex-shrink: 0; }
        .copy-btn:hover { background: #eff2ff; }
        .packages-section { border-top: 1px solid #f3f4f6; padding-top: 0.6rem; margin-top: 0.75rem; }
        .pkg-title { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #9ca3af; display: block; margin-bottom: 0.35rem; }
        .pkg-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.25rem 0; font-size: 0.82rem; color: #374151; }
        .pkg-name { flex: 1; font-family: monospace; font-size: 0.78rem; }
        .pkg-link { flex: 1; font-family: monospace; font-size: 0.78rem; color: #4338ca; text-decoration: none; }
        .pkg-link:hover { text-decoration: underline; }
        .pkg-del-btn { background: none; border: none; color: #ef4444; cursor: pointer; padding: 0.2rem; border-radius: 4px; opacity: 0.6; }
        .pkg-del-btn:hover { opacity: 1; background: #fee2e2; }

        /* Runtime table */
        .runtime-table { border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden; margin-bottom: 0.5rem; }
        .rt-head { display: grid; grid-template-columns: 6rem 5rem 1fr 7rem 8rem 7rem 6rem 5rem; padding: 0.45rem 0.75rem; background: #f9fafb; border-bottom: 1px solid #e5e7eb; font-size: 0.7rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; color: #6b7280; }
        .rt-row { display: grid; grid-template-columns: 6rem 5rem 1fr 7rem 8rem 7rem 6rem 5rem; padding: 0.55rem 0.75rem; border-bottom: 1px solid #f3f4f6; align-items: center; font-size: 0.83rem; }
        .rt-row:hover { background: #fafbff; }
        .rt-row.inactive { opacity: 0.5; }
        .rt-c { overflow: hidden; }
        .mono { font-family: monospace; font-size: 0.8rem; }
        .fw-name { display: block; font-size: 0.82rem; color: #374151; }
        .fw-ver { display: block; font-family: monospace; font-size: 0.75rem; color: #6b7280; }
        .rt-expand { padding: 0.35rem 0.75rem 0.55rem 0.75rem; background: #f8f9ff; border-bottom: 1px solid #e9ecef; font-size: 0.8rem; color: #4b5563; }
        .rt-detail { margin-bottom: 0.2rem; }
        .rt-dl { font-weight: 700; color: #6b7280; margin-right: 0.35rem; }
        .act-btn { width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; background: transparent; border: none; border-radius: 4px; color: #6b7280; cursor: pointer; font-size: 0.8rem; }
        .act-btn:hover { background: #f3f4f6; color: #374151; }
        .act-danger { color: #ef4444; }
        .act-danger:hover { background: #fee2e2; }
        .empty-state { text-align: center; padding: 2rem; color: #9ca3af; font-size: 0.9rem; }
        .empty-state i { font-size: 2rem; display: block; margin-bottom: 0.5rem; }
        .empty-state.small { padding: 1.25rem; }

        /* Publish */
        .publish-card { border: 1px solid #e0e7ff; border-radius: 10px; padding: 1.1rem; background: #fafbff; margin-bottom: 1rem; }
        .pub-form { display: flex; flex-direction: column; gap: 0.85rem; }
        .pub-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .pf { display: flex; flex-direction: column; gap: 0.3rem; }
        .pf label { font-size: 0.8rem; font-weight: 600; color: #374151; }
        .p-inputtext, .p-inputtextarea, .native-select { width: 100%; padding: 0.5rem 0.75rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 0.875rem; box-sizing: border-box; }
        .native-select { height: 36px; background: #fff; }
        textarea { resize: vertical; min-height: 60px; }
        .hidden-file { display: none; }
        .file-drop { display: flex; align-items: center; gap: 0.6rem; padding: 0.6rem 0.85rem; border: 1.5px dashed #c7d2fe; border-radius: 8px; background: #fff; cursor: pointer; font-size: 0.85rem; color: #667eea; }
        .file-drop:hover { background: #eff2ff; }
        .pub-actions { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; }
        .pub-hint { font-size: 0.78rem; color: #6b7280; display: flex; align-items: center; gap: 0.35rem; }
        .pub-hint code { background: #f3f4f6; padding: 0.1rem 0.35rem; border-radius: 4px; font-family: monospace; }
        .progress-section { padding: 0.5rem 0; }
        .progress-label { display: flex; justify-content: space-between; font-size: 0.82rem; color: #374151; margin-bottom: 0.3rem; }
        .progress-size { color: #9ca3af; }
        .progress-track { height: 7px; background: #e5e7eb; border-radius: 999px; overflow: hidden; }
        .progress-fill { height: 100%; background: linear-gradient(90deg, #667eea, #764ba2); border-radius: 999px; transition: width 0.2s ease; }
        .progress-section.processing { font-size: 0.85rem; color: #667eea; display: flex; align-items: center; gap: 0.5rem; }

        /* Runtime dialog */
        .rt-form { display: flex; flex-direction: column; gap: 0.75rem; }
        .rf-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .rf { display: flex; flex-direction: column; gap: 0.3rem; }
        .rf label { font-size: 0.8rem; font-weight: 600; color: #374151; }
        .opt { font-weight: 400; font-size: 0.72rem; color: #9ca3af; }
        .w-full { width: 100%; box-sizing: border-box; }
        .check-lbl { display: flex; align-items: center; gap: 0.4rem; font-size: 0.85rem; cursor: pointer; }
    `]
})
export class DesktopUpdatesComponent implements OnInit {
    private http = inject(HttpClient);
    private msg = inject(MessageService);

    private readonly base = `${environment.apiUrl}/desktop-updates`;

    channels = signal<ChannelInfo[]>([]);
    installerChannels = signal<ChannelInfo[]>([]);
    runtimeMetadata = signal<AppRuntimeMetadataDto[]>([]);
    currentRuntime = signal<AppRuntimeMetadataDto | null>(null);
    loading = signal(false);
    publishing = signal(false);

    form = { version: '', channel: 'stable', notes: '', file: null as File | null };
    uploadPct = 0; uploadedMB = '0'; totalMB = '0';

    rtDlg = {
        visible: false, isEdit: false, saving: false, editId: '',
        d: this.emptyRt()
    };

    ngOnInit() { this.load(); }

    load() {
        this.loading.set(true);
        this.http.get<ChannelInfo[]>(`${this.base}/channels`).subscribe({
            next: c => { this.channels.set(c); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
        this.http.get<ChannelInfo[]>(`${this.base}/installer-channels`).subscribe({
            next: c => this.installerChannels.set(c),
            error: () => this.installerChannels.set([])
        });
        this.http.get<AppRuntimeMetadataDto[]>(`${this.base}/runtime-metadata`).subscribe({
            next: r => {
                this.runtimeMetadata.set(r);
                const stable = r.find(x => x.channel === 'stable' && x.isActive);
                this.currentRuntime.set(stable ?? r.find(x => x.isActive) ?? null);
            },
            error: () => {}
        });
    }

    // ── Publish ──────────────────────────────────────────────────────────────

    onFileSelected(e: Event) {
        this.form.file = (e.target as HTMLInputElement).files?.[0] ?? null;
    }

    publish() {
        if (!this.form.version.trim() || !this.form.file || this.publishing()) return;
        this.publishing.set(true);
        this.uploadPct = 0;
        this.totalMB = (this.form.file.size / (1024 * 1024)).toFixed(1);

        const fd = new FormData();
        fd.append('version', this.form.version.trim());
        fd.append('channel', this.form.channel);
        if (this.form.notes.trim()) fd.append('notes', this.form.notes.trim());
        fd.append('file', this.form.file);

        const channel = this.form.channel;
        this.http.request(new HttpRequest('POST', `${this.base}/publish`, fd, { reportProgress: true }))
            .pipe(filter((e: any) => e.type === HttpEventType.UploadProgress || e.type === HttpEventType.Response))
            .subscribe({
                next: (event: any) => {
                    if (event.type === HttpEventType.UploadProgress && event.total) {
                        this.uploadPct = Math.round(100 * event.loaded / event.total);
                        this.uploadedMB = (event.loaded / (1024 * 1024)).toFixed(1);
                    } else if (event.type === HttpEventType.Response) {
                        this.publishing.set(false); this.uploadPct = 0;
                        this.form = { version: '', channel: 'stable', notes: '', file: null };
                        this.msg.add({ severity: 'success', summary: 'Published', detail: `v${event.body.version} is live on ${channel}.`, life: 5000 });
                        this.load();
                    }
                },
                error: err => {
                    this.publishing.set(false); this.uploadPct = 0;
                    this.msg.add({ severity: 'error', summary: 'Error', detail: err?.error?.message || 'Publish failed.', life: 5000 });
                }
            });
    }

    deletePackage(channel: string, pkg: string) {
        if (!confirm(`Delete ${pkg}?`)) return;
        this.http.delete(`${this.base}/channels/${channel}/packages/${encodeURIComponent(pkg)}`).subscribe({
            next: () => { this.msg.add({ severity: 'success', summary: 'Deleted', detail: pkg, life: 3000 }); this.load(); },
            error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.', life: 3000 })
        });
    }

    // ── Runtime metadata ─────────────────────────────────────────────────────

    openRuntimeDialog(r: AppRuntimeMetadataDto | null) {
        this.rtDlg.isEdit = !!r;
        this.rtDlg.editId = r?.id ?? '';
        this.rtDlg.saving = false;
        this.rtDlg.d = r ? {
            channel: r.channel, appVersion: r.appVersion,
            firmwareName: r.firmwareName, firmwareVersion: r.firmwareVersion,
            targetBoard: r.targetBoard, protocolName: r.protocolName,
            commandSummary: r.commandSummary ?? '', compatibilityNotes: r.compatibilityNotes ?? '',
            isActive: r.isActive, publishedAt: r.publishedAt ?? ''
        } : this.emptyRt();
        this.rtDlg.visible = true;
    }

    saveRuntime() {
        if (!this.rtDlg.d.appVersion?.trim()) {
            this.msg.add({ severity: 'warn', summary: 'Required', detail: 'App version is required.', life: 4000 });
            return;
        }
        this.rtDlg.saving = true;
        const payload = { ...this.rtDlg.d, publishedAt: this.rtDlg.d.publishedAt || undefined };
        const call = this.rtDlg.isEdit
            ? this.http.put(`${this.base}/runtime-metadata/${this.rtDlg.editId}`, payload)
            : this.http.post(`${this.base}/runtime-metadata`, payload);
        call.subscribe({
            next: () => { this.rtDlg.visible = false; this.rtDlg.saving = false; this.load(); },
            error: (err) => { this.rtDlg.saving = false; this.msg.add({ severity: 'error', summary: 'Error', detail: err?.error?.message || 'Save failed.', life: 5000 }); }
        });
    }

    deleteRuntime(r: AppRuntimeMetadataDto) {
        if (!confirm(`Delete metadata for app v${r.appVersion}?`)) return;
        this.http.delete(`${this.base}/runtime-metadata/${r.id}`).subscribe({
            next: () => { this.load(); this.msg.add({ severity: 'success', summary: 'Deleted', detail: `v${r.appVersion} metadata removed.`, life: 3000 }); },
            error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'Delete failed.', life: 3000 })
        });
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    copy(url: string) {
        navigator.clipboard.writeText(url).then(() =>
            this.msg.add({ severity: 'info', summary: 'Copied', detail: 'URL copied to clipboard.', life: 2000 })
        );
    }

    buildPackageUrl(ch: ChannelInfo): string {
        return ch.latestManifest ? this.buildDirectPackageUrl(ch.manifestUrl, ch.latestManifest.packageUri) : ch.manifestUrl;
    }

    buildDirectPackageUrl(manifestUrl: string, packageName: string): string {
        return manifestUrl.replace('manifest.json', packageName);
    }

    formatSize(bytes: number): string {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    private emptyRt() {
        return { channel: 'stable', appVersion: '', firmwareName: '', firmwareVersion: '',
                 targetBoard: '', protocolName: '', commandSummary: '', compatibilityNotes: '',
                 isActive: true, publishedAt: '' };
    }
}
