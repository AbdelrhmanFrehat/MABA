import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { TabsModule } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { MachineCatalogService } from '../../../../shared/services/machine-catalog.service';
import { MachineCategory, MachineFamily, MachineDefinition, AxisRole, Direction } from '../../../../shared/models/machine-catalog.model';

const DRIVER_TYPES    = ['ArduinoSerial','GRBL','Marlin','RepRap','MabaCustom','Simulated','NetworkMaba','Unknown'];
const FW_PROTOCOLS    = ['GRBL_1_1','GRBL_0_9','Marlin_2','Marlin_1','RepRap','MabaProtocol','Custom','Unknown'];
const KINEMATICS      = ['MovingGantryXY','FixedGantryMovingBed','CoreXY','Delta','LaserFlatbed','CartesianPrinter','Scara','Unknown'];
const VIZ_TYPES       = ['CncTopDown2D','LaserFlatbed2D','Printer3DCartesian','Printer3DDelta','Generic2D','Generic3D'];
const SETUP_MODES     = ['RealOnly','SimulationOnly','RealAndSimulation'];
const CONN_TYPES      = ['Serial','USB','Network','Bluetooth','Simulated'];
const HOME_ORIGINS    = ['FrontLeft','FrontRight','BackLeft','BackRight','Center','Custom'];
const OP_TYPES        = ['Milling','Engraving','Drilling','LaserCutting','LaserEngraving','FDMPrint','ResinPrint','Plotting'];
const GCODE_DIALECTS  = ['GRBL','Marlin','RepRap','MabaCustom','Generic'];
const VIEW_MODES      = ['Top2D','Perspective3D','Side2D','Isometric'];
const COORD_MODES     = ['TopLeft','BottomLeft','Center'];
const SHAPE_HINTS     = ['Rectangular','Delta','Polar','Articulated'];
const AXIS_IDS        = ['X','Y','Z','A','B','C'];
const AXIS_ROLES      = ['Primary','Secondary','Vertical','Rotational','Extruder','Generic'];
const DIRECTIONS      = ['Normal','Inverted'];
const OVERRIDE_FIELDS = ['DriverType','BaudRate','Port','StepsPerMm','MaxFeed','MaxAccel','JogPresets','SafeZ','ParkPosition','WorkOffset','Visualization','Notes'];
const BAUD_RATES      = [9600,19200,38400,57600,115200,230400,250000];
const AXES            = ['X','Y','Z','A','B','C'];

@Component({
    selector: 'app-machine-definition-form',
    standalone: true,
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        ButtonModule, InputTextModule, TextareaModule, InputNumberModule,
        CheckboxModule, SelectModule, MultiSelectModule, TabsModule, TagModule,
        ToastModule, TooltipModule, DividerModule
    ],
    providers: [MessageService],
    styles: [`
        .mp-header { border-inline-start: 3px solid var(--tech-primary-blue, #0066FF); padding-inline-start: 1rem; }
        .mp-badge  { display:inline-flex; align-items:center; gap:.35rem; font-size:.68rem; font-weight:800; letter-spacing:.1em; text-transform:uppercase; color:var(--tech-primary-blue,#0066FF); background:rgba(0,102,255,.08); border:1px solid rgba(0,102,255,.2); border-radius:4px; padding:2px 8px; margin-bottom:.4rem; }
        .mp-code   { font-family:monospace; font-size:.8rem; color:var(--tech-primary-blue,#0066FF); background:rgba(0,102,255,.07); border-radius:3px; padding:1px 6px; }
        .sec-title { font-size:.78rem; font-weight:800; text-transform:uppercase; letter-spacing:.08em; color:var(--p-text-muted-color); margin-bottom:.75rem; padding-bottom:.35rem; border-bottom:1px solid var(--surface-border); }
        .field label  { font-weight:600; display:block; margin-bottom:4px; font-size:.875rem; }
        .field small  { color:var(--p-text-muted-color); font-size:.75rem; margin-top:2px; display:block; }
        .axis-table   { width:100%; border-collapse:collapse; }
        .axis-table th { font-size:.72rem; font-weight:700; text-transform:uppercase; letter-spacing:.06em; color:var(--p-text-muted-color); padding:.3rem .5rem; text-align:start; }
        .axis-table td { padding:.3rem .5rem; vertical-align:top; }
        .axis-lbl  { display:inline-flex; align-items:center; justify-content:center; width:28px; height:28px; background:rgba(0,102,255,.08); border:1px solid rgba(0,102,255,.2); border-radius:4px; font-family:monospace; font-size:.8rem; font-weight:800; color:var(--tech-primary-blue,#0066FF); }
        .cap-grid  { display:grid; grid-template-columns:repeat(auto-fill,minmax(210px,1fr)); gap:.4rem; }
        .cap-item  { display:flex; align-items:center; gap:.5rem; padding:.25rem 0; }
        .cap-item label { font-size:.875rem; cursor:pointer; margin:0; font-weight:normal; }
        .jog-row   { display:grid; grid-template-columns:1fr 130px 130px 36px; gap:.5rem; align-items:end; margin-bottom:.5rem; }
        .field-error { color:#ef4444; font-size:.75rem; margin-top:3px; display:block; }
        .tab-err-dot { display:inline-block; width:7px; height:7px; border-radius:50%; background:#ef4444; margin-inline-start:5px; vertical-align:middle; position:relative; top:-1px; }
        .ng-invalid.ng-touched input, input.ng-invalid.ng-touched,
        .ng-invalid.ng-touched .p-inputtext, .ng-invalid.ng-touched p-select .p-select,
        .ng-invalid.ng-touched p-multiselect .p-multiselect { border-color:#ef4444 !important; }
    `],
    template: `
        <p-toast />
        <div class="p-4">

            <div class="flex justify-between items-center mb-4">
                <div class="mp-header">
                    <div class="mp-badge"><i class="pi pi-server" style="font-size:.65rem"></i> Machine Platform</div>
                    <h1 class="text-2xl font-bold m-0">{{ isEdit ? 'Edit Definition' : 'New Machine Definition' }}</h1>
                    <p class="text-500 mt-1 mb-0 text-sm">
                        <ng-container *ngIf="!isEdit">Runtime-ready machine spec — loaded by the desktop app.</ng-container>
                        <ng-container *ngIf="isEdit">
                            <span class="mp-code">{{ form.get('code')?.value }}</span>
                            &nbsp;— edit sections then save to update the platform registry.
                        </ng-container>
                    </p>
                </div>
                <div class="flex gap-2">
                    <p-button label="Cancel" [outlined]="true" (onClick)="cancel()"></p-button>
                    <p-button label="Save Definition" icon="pi pi-save" [loading]="saving" (onClick)="save()"></p-button>
                </div>
            </div>

            <form [formGroup]="form">
                <p-tabs value="identity">
                    <p-tablist>
                        <p-tab value="identity">Identity<span *ngIf="tabErr(['categoryId','familyId','code','version','displayNameEn','displayNameAr','manufacturer'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="runtime">Runtime Binding<span *ngIf="tabErr(['runtimeBinding.defaultDriverType','runtimeBinding.supportedDriverTypes','runtimeBinding.firmwareProtocol','runtimeBinding.supportedSetupModes','runtimeBinding.visualizationType','runtimeBinding.kinematicsType','runtimeBinding.runtimeUiVariant'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="axis">Axis Config<span *ngIf="tabErr(['axisConfig.axisCount','axisConfig.supportedAxes'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="workspace">Workspace<span *ngIf="tabErr(['workspace.workAreaWidth','workspace.workAreaDepth','workspace.workAreaHeight'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="motion">Motion Defaults</p-tab>
                        <p-tab value="connection">Connection<span *ngIf="tabErr(['connectionDefaults.defaultBaudRate','connectionDefaults.supportedBaudRates','connectionDefaults.supportedConnectionTypes'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="capabilities">Capabilities</p-tab>
                        <p-tab value="filesupport">File Support<span *ngIf="tabErr(['fileSupport.gcodeDialect'])" class="tab-err-dot"></span></p-tab>
                        <p-tab value="visualization">Visualization</p-tab>
                        <p-tab value="profilerules">Profile Rules</p-tab>
                    </p-tablist>

                    <p-tabpanels>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: IDENTITY
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="identity">
                            <div class="grid grid-cols-2 gap-4 mt-3">
                                <div class="field">
                                    <label>Category <span class="text-red-500">*</span></label>
                                    <p-select formControlName="categoryId" [options]="categories"
                                              optionLabel="displayNameEn" optionValue="id"
                                              placeholder="Select category" class="w-full" appendTo="body"
                                              (onChange)="onCategoryChange()"></p-select>
                                    <small *ngIf="isInvalid('categoryId')" class="field-error">Category is required.</small>
                                </div>
                                <div class="field">
                                    <label>Family <span class="text-red-500">*</span></label>
                                    <p-select formControlName="familyId" [options]="filteredFamilies"
                                              optionLabel="displayNameEn" optionValue="id"
                                              placeholder="Select family" class="w-full" appendTo="body"></p-select>
                                    <small *ngIf="isInvalid('familyId')" class="field-error">Family is required.</small>
                                </div>
                                <div class="field">
                                    <label>Code <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="code" class="w-full" placeholder="MABA_CNC_400"
                                           [attr.disabled]="isEdit ? true : null" />
                                    <small *ngIf="isInvalid('code')" class="field-error">Code is required.</small>
                                    <small *ngIf="!isInvalid('code')">Uppercase unique identifier — cannot be changed after creation.</small>
                                </div>
                                <div class="field">
                                    <label>Version <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="version" class="w-full" placeholder="1.0" />
                                    <small *ngIf="isInvalid('version')" class="field-error">Version is required.</small>
                                </div>
                                <div class="field">
                                    <label>Name (EN) <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="displayNameEn" class="w-full" />
                                    <small *ngIf="isInvalid('displayNameEn')" class="field-error">English name is required.</small>
                                </div>
                                <div class="field">
                                    <label>Name (AR) <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="displayNameAr" class="w-full" dir="rtl" />
                                    <small *ngIf="isInvalid('displayNameAr')" class="field-error">Arabic name is required.</small>
                                </div>
                                <div class="field col-span-2">
                                    <label>Manufacturer <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="manufacturer" class="w-full" />
                                    <small *ngIf="isInvalid('manufacturer')" class="field-error">Manufacturer is required.</small>
                                </div>
                                <div class="field col-span-2">
                                    <label>Description (EN)</label>
                                    <textarea pTextarea formControlName="descriptionEn" class="w-full" rows="2"></textarea>
                                </div>
                                <div class="field col-span-2">
                                    <label>Description (AR)</label>
                                    <textarea pTextarea formControlName="descriptionAr" class="w-full" rows="2" dir="rtl"></textarea>
                                </div>
                                <div class="field">
                                    <label>Tags</label>
                                    <input pInputText formControlName="tagsInput" class="w-full" placeholder="cnc, milling, gantry" />
                                    <small>Comma-separated</small>
                                </div>
                                <div class="field">
                                    <label>Sort Order</label>
                                    <p-inputnumber formControlName="sortOrder" class="w-full" [min]="0"></p-inputnumber>
                                </div>
                                <div class="field">
                                    <label>Released At</label>
                                    <input pInputText formControlName="releasedAt" class="w-full" placeholder="YYYY-MM-DD (or any valid date)" />
                                </div>
                                <div class="field">
                                    <label>Revision Note</label>
                                    <input pInputText formControlName="revisionNote" class="w-full" />
                                </div>
                                <div class="field col-span-2">
                                    <label>Internal Notes <span class="text-500 font-normal text-sm">(admin only, never exposed)</span></label>
                                    <textarea pTextarea formControlName="internalNotes" class="w-full" rows="2"></textarea>
                                </div>
                                <div class="col-span-2">
                                    <div class="flex flex-wrap gap-5">
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isActive" [binary]="true" inputId="defActive"></p-checkbox>
                                            <label for="defActive" class="cursor-pointer">Active</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isPublic" [binary]="true" inputId="defPublic"></p-checkbox>
                                            <label for="defPublic" class="cursor-pointer">Public</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isDeprecated" [binary]="true" inputId="defDepr"></p-checkbox>
                                            <label for="defDepr" class="cursor-pointer text-orange-500">Deprecated</label>
                                        </div>
                                    </div>
                                </div>
                                <div class="field col-span-2" *ngIf="form.get('isDeprecated')?.value">
                                    <label>Deprecation Note</label>
                                    <textarea pTextarea formControlName="deprecationNote" class="w-full" rows="2"
                                              placeholder="Why this definition was deprecated…"></textarea>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: RUNTIME BINDING
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="runtime">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="runtimeBinding">
                                <div class="sec-title col-span-2">Driver &amp; Protocol</div>
                                <div class="field">
                                    <label>Default Driver Type <span class="text-red-500">*</span></label>
                                    <p-select formControlName="defaultDriverType" [options]="driverTypeOpts"
                                              class="w-full" appendTo="body"></p-select>
                                    <small *ngIf="isInvalid('runtimeBinding.defaultDriverType')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Supported Driver Types <span class="text-red-500">*</span></label>
                                    <p-multiselect formControlName="supportedDriverTypes" [options]="driverTypeOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                    <small *ngIf="isInvalid('runtimeBinding.supportedDriverTypes')" class="field-error">Select at least one.</small>
                                </div>
                                <div class="field">
                                    <label>Firmware Protocol <span class="text-red-500">*</span></label>
                                    <p-select formControlName="firmwareProtocol" [options]="fwProtocolOpts"
                                              class="w-full" appendTo="body"></p-select>
                                    <small *ngIf="isInvalid('runtimeBinding.firmwareProtocol')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Supported Setup Modes <span class="text-red-500">*</span></label>
                                    <p-multiselect formControlName="supportedSetupModes" [options]="setupModeOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                    <small *ngIf="isInvalid('runtimeBinding.supportedSetupModes')" class="field-error">Select at least one.</small>
                                </div>
                                <div class="sec-title col-span-2 mt-2">Kinematics &amp; Visualization</div>
                                <div class="field">
                                    <label>Kinematics Type <span class="text-red-500">*</span></label>
                                    <p-select formControlName="kinematicsType" [options]="kinematicsOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Visualization Type <span class="text-red-500">*</span></label>
                                    <p-select formControlName="visualizationType" [options]="vizTypeOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field col-span-2">
                                    <label>Runtime UI Variant <span class="text-red-500">*</span></label>
                                    <input pInputText formControlName="runtimeUiVariant" class="w-full"
                                           placeholder="cnc-standard | printer3d-fdm | laser-flatbed" />
                                    <small *ngIf="isInvalid('runtimeBinding.runtimeUiVariant')" class="field-error">Required.</small>
                                    <small *ngIf="!isInvalid('runtimeBinding.runtimeUiVariant')">Identifies the control-center UI layout loaded by the desktop app for this machine type.</small>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: AXIS CONFIG
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="axis">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="axisConfig">
                                <div class="sec-title col-span-2">Axis Setup</div>
                                <div class="field">
                                    <label>Axis Count <span class="text-red-500">*</span></label>
                                    <p-inputnumber formControlName="axisCount" class="w-full" [min]="1" [max]="6"></p-inputnumber>
                                </div>
                                <div class="field">
                                    <label>Supported Axes <span class="text-red-500">*</span></label>
                                    <p-multiselect formControlName="supportedAxes" [options]="axisIdOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                </div>
                                <div class="field">
                                    <label>Home Origin Convention</label>
                                    <p-select formControlName="homeOriginConvention" [options]="homeOriginOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field col-span-2">
                                    <div class="flex flex-wrap gap-5">
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="workCoordinateSupport" [binary]="true" inputId="wcs"></p-checkbox>
                                            <label for="wcs" class="cursor-pointer font-normal">Work Coordinate Support</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="machineCoordinateSupport" [binary]="true" inputId="mcs"></p-checkbox>
                                            <label for="mcs" class="cursor-pointer font-normal">Machine Coordinate Support</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="relativeMoveSupport" [binary]="true" inputId="rel"></p-checkbox>
                                            <label for="rel" class="cursor-pointer font-normal">Relative Moves</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="absoluteMoveSupport" [binary]="true" inputId="abs"></p-checkbox>
                                            <label for="abs" class="cursor-pointer font-normal">Absolute Moves</label>
                                        </div>
                                    </div>
                                </div>

                                <div class="sec-title col-span-2 mt-2">Per-Axis Configuration</div>
                                <div class="col-span-2 overflow-x-auto">
                                    <table class="axis-table">
                                        <thead>
                                            <tr>
                                                <th style="width:48px">Axis</th>
                                                <th>Role</th>
                                                <th>Direction</th>
                                                <th style="width:80px">Homing</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr *ngFor="let ax of axisRows" [formGroupName]="'axis_' + ax.id">
                                                <td><span class="axis-lbl">{{ ax.id }}</span></td>
                                                <td>
                                                    <p-select formControlName="role" [options]="axisRoleOpts"
                                                              class="w-full" appendTo="body" [style]="{minWidth:'150px'}"></p-select>
                                                </td>
                                                <td>
                                                    <p-select formControlName="direction" [options]="directionOpts"
                                                              class="w-full" appendTo="body" [style]="{minWidth:'120px'}"></p-select>
                                                </td>
                                                <td class="text-center">
                                                    <p-checkbox formControlName="homing" [binary]="true"
                                                                [inputId]="'hm_'+ax.id"></p-checkbox>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: WORKSPACE
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="workspace">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="workspace">
                                <div class="sec-title col-span-2">Work Area (mm)</div>
                                <div class="field">
                                    <label>Width <span class="text-red-500">*</span></label>
                                    <p-inputnumber formControlName="workAreaWidth" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                    <small *ngIf="isInvalid('workspace.workAreaWidth')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Depth <span class="text-red-500">*</span></label>
                                    <p-inputnumber formControlName="workAreaDepth" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                    <small *ngIf="isInvalid('workspace.workAreaDepth')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Height <span class="text-red-500">*</span></label>
                                    <p-inputnumber formControlName="workAreaHeight" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                    <small *ngIf="isInvalid('workspace.workAreaHeight')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Safe Z Height</label>
                                    <p-inputnumber formControlName="safeZHeightMm" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                </div>

                                <div class="sec-title col-span-2 mt-2">Per-Axis Travel &amp; Park (mm)</div>
                                <div class="col-span-2 overflow-x-auto">
                                    <table class="axis-table">
                                        <thead>
                                            <tr>
                                                <th style="width:48px">Axis</th>
                                                <th>Max Travel (mm)</th>
                                                <th>Min Travel (mm)</th>
                                                <th>Park Position (mm)</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr *ngFor="let ax of axisRows" [formGroupName]="'travel_' + ax.id">
                                                <td><span class="axis-lbl">{{ ax.id }}</span></td>
                                                <td><p-inputnumber formControlName="maxTravel" class="w-full" [min]="0"></p-inputnumber></td>
                                                <td><p-inputnumber formControlName="minTravel" class="w-full"></p-inputnumber></td>
                                                <td><p-inputnumber formControlName="park" class="w-full"></p-inputnumber></td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                                <div class="sec-title col-span-2 mt-2">Machine Outer Dimensions (mm, optional)</div>
                                <div class="field">
                                    <label>Machine Width</label>
                                    <p-inputnumber formControlName="machineDimWidth" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                </div>
                                <div class="field">
                                    <label>Machine Depth</label>
                                    <p-inputnumber formControlName="machineDimDepth" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                </div>
                                <div class="field">
                                    <label>Machine Height</label>
                                    <p-inputnumber formControlName="machineDimHeight" class="w-full" [min]="0" suffix=" mm"></p-inputnumber>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: MOTION DEFAULTS
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="motion">
                            <div class="mt-3" formGroupName="motionDefaults">
                                <div class="sec-title">Per-Axis Motion Parameters</div>
                                <div class="overflow-x-auto mb-4">
                                    <table class="axis-table">
                                        <thead>
                                            <tr>
                                                <th style="width:48px">Axis</th>
                                                <th>Steps / mm</th>
                                                <th>Max Feed (mm/min)</th>
                                                <th>Max Accel (mm/s²)</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr *ngFor="let ax of axisRows" [formGroupName]="'motion_' + ax.id">
                                                <td><span class="axis-lbl">{{ ax.id }}</span></td>
                                                <td><p-inputnumber formControlName="steps" class="w-full" [min]="0"></p-inputnumber></td>
                                                <td><p-inputnumber formControlName="feed" class="w-full" [min]="0"></p-inputnumber></td>
                                                <td><p-inputnumber formControlName="accel" class="w-full" [min]="0"></p-inputnumber></td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                                <div class="sec-title mt-2">Jog Presets</div>
                                <div formArrayName="jogPresets">
                                    <div *ngFor="let p of jogPresets.controls; let i = index" [formGroupName]="i" class="jog-row">
                                        <div class="field mb-0">
                                            <label class="text-xs">Label</label>
                                            <input pInputText formControlName="label" class="w-full" placeholder="Slow" />
                                        </div>
                                        <div class="field mb-0">
                                            <label class="text-xs">Feed (mm/min)</label>
                                            <p-inputnumber formControlName="feedMmMin" class="w-full" [min]="0"></p-inputnumber>
                                        </div>
                                        <div class="field mb-0">
                                            <label class="text-xs">Distance (mm)</label>
                                            <p-inputnumber formControlName="distanceMm" class="w-full" [min]="0"></p-inputnumber>
                                        </div>
                                        <p-button icon="pi pi-trash" [text]="true" severity="danger" size="small"
                                                  (onClick)="removeJogPreset(i)"></p-button>
                                    </div>
                                </div>
                                <p-button label="Add Jog Preset" icon="pi pi-plus" [outlined]="true" size="small"
                                          class="mt-2" (onClick)="addJogPreset()"></p-button>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: CONNECTION DEFAULTS
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="connection">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="connectionDefaults">
                                <div class="sec-title col-span-2">Serial / Connection</div>
                                <div class="field">
                                    <label>Default Baud Rate <span class="text-red-500">*</span></label>
                                    <p-select formControlName="defaultBaudRate" [options]="baudRateOpts"
                                              class="w-full" appendTo="body"></p-select>
                                    <small *ngIf="isInvalid('connectionDefaults.defaultBaudRate')" class="field-error">Required.</small>
                                </div>
                                <div class="field">
                                    <label>Supported Baud Rates <span class="text-red-500">*</span></label>
                                    <p-multiselect formControlName="supportedBaudRates" [options]="baudRateOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                    <small *ngIf="isInvalid('connectionDefaults.supportedBaudRates')" class="field-error">Select at least one.</small>
                                </div>
                                <div class="field col-span-2">
                                    <label>Supported Connection Types <span class="text-red-500">*</span></label>
                                    <p-multiselect formControlName="supportedConnectionTypes" [options]="connTypeOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                    <small *ngIf="isInvalid('connectionDefaults.supportedConnectionTypes')" class="field-error">Select at least one.</small>
                                </div>
                                <div class="sec-title col-span-2 mt-2">Protocol</div>
                                <div class="field">
                                    <label>Command Terminator</label>
                                    <input pInputText formControlName="commandTerminator" class="w-full" placeholder='\n' />
                                    <small>e.g. \n or \r\n</small>
                                </div>
                                <div class="field">
                                    <label>Response ACK Pattern</label>
                                    <input pInputText formControlName="responseAckPattern" class="w-full" placeholder="ok" />
                                </div>
                                <div class="field col-span-2">
                                    <label>Protocol Notes</label>
                                    <textarea pTextarea formControlName="protocolNotes" class="w-full" rows="2"></textarea>
                                </div>
                                <div class="field col-span-2">
                                    <div class="flex items-center gap-2">
                                        <p-checkbox formControlName="requiresHandshake" [binary]="true" inputId="handshake"></p-checkbox>
                                        <label for="handshake" class="cursor-pointer font-normal">Requires Handshake on Connect</label>
                                    </div>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: CAPABILITIES
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="capabilities">
                            <div class="mt-3" formGroupName="capabilities">

                                <div class="sec-title">Motion Capabilities</div>
                                <div class="cap-grid mb-4" formGroupName="motion">
                                    <div *ngFor="let k of motionCapKeys" class="cap-item">
                                        <p-checkbox [formControlName]="k" [binary]="true" [inputId]="'mc_'+k"></p-checkbox>
                                        <label [for]="'mc_'+k">{{ k }}</label>
                                    </div>
                                </div>

                                <p-divider />
                                <div class="sec-title">Execution Capabilities</div>
                                <div class="cap-grid mb-4" formGroupName="execution">
                                    <div *ngFor="let k of executionCapKeys" class="cap-item">
                                        <p-checkbox [formControlName]="k" [binary]="true" [inputId]="'ec_'+k"></p-checkbox>
                                        <label [for]="'ec_'+k">{{ k }}</label>
                                    </div>
                                </div>

                                <p-divider />
                                <div class="sec-title">Protocol Capabilities</div>
                                <div class="cap-grid mb-4" formGroupName="protocol">
                                    <div *ngFor="let k of protocolCapKeys" class="cap-item">
                                        <p-checkbox [formControlName]="k" [binary]="true" [inputId]="'pr_'+k"></p-checkbox>
                                        <label [for]="'pr_'+k">{{ k }}</label>
                                    </div>
                                </div>

                                <p-divider />
                                <div class="sec-title">Visualization Capabilities</div>
                                <div class="cap-grid mb-4" formGroupName="visualization">
                                    <div *ngFor="let k of vizCapKeys" class="cap-item">
                                        <p-checkbox [formControlName]="k" [binary]="true" [inputId]="'vc_'+k"></p-checkbox>
                                        <label [for]="'vc_'+k">{{ k }}</label>
                                    </div>
                                </div>

                                <p-divider />
                                <div class="sec-title">File Handling Capabilities</div>
                                <div class="cap-grid" formGroupName="fileHandling">
                                    <div *ngFor="let k of fileCapKeys" class="cap-item">
                                        <p-checkbox [formControlName]="k" [binary]="true" [inputId]="'fh_'+k"></p-checkbox>
                                        <label [for]="'fh_'+k">{{ k }}</label>
                                    </div>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: FILE SUPPORT
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="filesupport">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="fileSupport">
                                <div class="sec-title col-span-2">G-code &amp; Operations</div>
                                <div class="field">
                                    <label>G-code Dialect <span class="text-red-500">*</span></label>
                                    <p-select formControlName="gcodeDialect" [options]="gcodeDialectOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Supported Operation Types</label>
                                    <p-multiselect formControlName="supportedOperationTypes" [options]="opTypeOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                </div>
                                <div class="field col-span-2">
                                    <label>Supported Input File Types</label>
                                    <input pInputText formControlName="inputFileTypesInput" class="w-full"
                                           placeholder=".gcode, .nc, .ngc, .tap" />
                                    <small>Comma-separated extensions including the leading dot.</small>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: VISUALIZATION
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="visualization">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="visualization">
                                <div class="sec-title col-span-2">Rendering &amp; Coordinate System</div>
                                <div class="field">
                                    <label>Visualization Type</label>
                                    <p-select formControlName="visualizationType" [options]="vizTypeOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Kinematics Type</label>
                                    <p-select formControlName="kinematicsType" [options]="kinematicsOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Coordinate Presentation Mode</label>
                                    <p-select formControlName="coordinatePresentationMode" [options]="coordModeOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Machine Shape Hint</label>
                                    <p-select formControlName="machineShapeHint" [options]="shapeHintOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                                <div class="field">
                                    <label>Default View Mode</label>
                                    <p-select formControlName="defaultViewMode" [options]="viewModeOpts"
                                              class="w-full" appendTo="body"></p-select>
                                </div>
                            </div>
                        </p-tabpanel>

                        <!-- ═══════════════════════════════════════════════════════
                             TAB: PROFILE RULES
                        ════════════════════════════════════════════════════════ -->
                        <p-tabpanel value="profilerules">
                            <div class="grid grid-cols-2 gap-4 mt-3" formGroupName="profileRules">
                                <div class="field col-span-2">
                                    <div class="sec-title">Override Permissions</div>
                                    <label>Allowed User Overrides</label>
                                    <p-multiselect formControlName="allowedOverrides" [options]="overrideFieldOpts"
                                                   class="w-full" appendTo="body" display="chip"></p-multiselect>
                                    <small>Fields the user is permitted to override in their machine profile.</small>
                                </div>
                                <div class="field">
                                    <div class="sec-title">Built-in Profile Rules</div>
                                    <div class="flex flex-col gap-3" formGroupName="builtInProfileRules">
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isEditable" [binary]="true" inputId="biEdit"></p-checkbox>
                                            <label for="biEdit" class="cursor-pointer font-normal">Editable</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isDeletable" [binary]="true" inputId="biDel"></p-checkbox>
                                            <label for="biDel" class="cursor-pointer font-normal">Deletable</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isDuplicatable" [binary]="true" inputId="biDup"></p-checkbox>
                                            <label for="biDup" class="cursor-pointer font-normal">Duplicatable</label>
                                        </div>
                                        <div class="field mb-0">
                                            <label class="text-sm">Duplicate Produces Type</label>
                                            <input pInputText formControlName="duplicateProducesType" class="w-full" placeholder="UserProfile" />
                                        </div>
                                    </div>
                                </div>
                                <div class="field">
                                    <div class="sec-title">User Profile Rules</div>
                                    <div class="flex flex-col gap-3" formGroupName="userProfileRules">
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isEditable" [binary]="true" inputId="upEdit"></p-checkbox>
                                            <label for="upEdit" class="cursor-pointer font-normal">Editable</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isDeletable" [binary]="true" inputId="upDel"></p-checkbox>
                                            <label for="upDel" class="cursor-pointer font-normal">Deletable</label>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <p-checkbox formControlName="isDuplicatable" [binary]="true" inputId="upDup"></p-checkbox>
                                            <label for="upDup" class="cursor-pointer font-normal">Duplicatable</label>
                                        </div>
                                        <div class="field mb-0">
                                            <label class="text-sm">Duplicate Produces Type</label>
                                            <input pInputText formControlName="duplicateProducesType" class="w-full" placeholder="UserProfile" />
                                        </div>
                                        <div class="field mb-0">
                                            <label class="text-sm">Max User Profiles</label>
                                            <p-inputnumber formControlName="maxUserProfiles" class="w-full" [min]="0"></p-inputnumber>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </p-tabpanel>

                    </p-tabpanels>
                </p-tabs>
            </form>
        </div>
    `
})
export class MachineDefinitionFormComponent implements OnInit {
    private svc    = inject(MachineCatalogService);
    private msg    = inject(MessageService);
    private fb     = inject(FormBuilder);
    private route  = inject(ActivatedRoute);
    private router = inject(Router);

    isEdit    = false;
    editId:   string | null = null;
    saving    = false;
    submitted = false;

    categories:      MachineCategory[] = [];
    allFamilies:     MachineFamily[]   = [];
    filteredFamilies: MachineFamily[]  = [];

    // ── Options ──────────────────────────────────────────────────────────────
    driverTypeOpts    = DRIVER_TYPES;
    fwProtocolOpts    = FW_PROTOCOLS;
    kinematicsOpts    = KINEMATICS;
    vizTypeOpts       = VIZ_TYPES;
    setupModeOpts     = SETUP_MODES;
    connTypeOpts      = CONN_TYPES;
    homeOriginOpts    = HOME_ORIGINS;
    opTypeOpts        = OP_TYPES;
    gcodeDialectOpts  = GCODE_DIALECTS;
    viewModeOpts      = VIEW_MODES;
    coordModeOpts     = COORD_MODES;
    shapeHintOpts     = SHAPE_HINTS;
    axisIdOpts        = AXIS_IDS;
    axisRoleOpts      = AXIS_ROLES;
    directionOpts     = DIRECTIONS;
    overrideFieldOpts = OVERRIDE_FIELDS;
    baudRateOpts      = BAUD_RATES;

    axisRows = AXES.map(id => ({ id }));

    // ── Capability key lists ──────────────────────────────────────────────────
    motionCapKeys    = ['homing','zHoming','combinedXYHoming','relativeMoves','absoluteMoves','pause','resume','stop','park','centerMove','workOffset','jogContinuous','jogStep'];
    executionCapKeys = ['realExecution','simulation','previewPlayback','dryRun','fileRun','frame','boundingBoxPreview','liveReportedPosition','estimatedPositionOnly','toolpathPreview','progressTracking'];
    protocolCapKeys  = ['handshake','acknowledgements','alarmReporting','alarmReset','statusQuery','positionQuery','motorEnable','motorDisable','feedHold','softReset'];
    vizCapKeys       = ['machineVisualization','topView2D','perspective3D','kinematicsAnimation','realTimePositionDisplay'];
    fileCapKeys      = ['localFileRun','streamingExecution','gcodeValidation','multipleFileFormats'];

    // ── Form ──────────────────────────────────────────────────────────────────
    form: FormGroup = this.fb.group({
        // Identity
        categoryId:     ['', Validators.required],
        familyId:       ['', Validators.required],
        code:           ['', [Validators.required, Validators.maxLength(100)]],
        version:        ['1.0', Validators.required],
        displayNameEn:  ['', [Validators.required, Validators.maxLength(200)]],
        displayNameAr:  ['', [Validators.required, Validators.maxLength(200)]],
        manufacturer:   ['', Validators.required],
        descriptionEn:  [''],
        descriptionAr:  [''],
        tagsInput:      [''],
        sortOrder:      [0],
        releasedAt:     [''],
        revisionNote:   [''],
        internalNotes:  [''],
        isActive:       [true],
        isPublic:       [true],
        isDeprecated:   [false],
        deprecationNote:[''],

        // Runtime Binding
        runtimeBinding: this.fb.group({
            defaultDriverType:   ['GRBL', Validators.required],
            supportedDriverTypes:[['GRBL'], Validators.required],
            firmwareProtocol:    ['GRBL_1_1', Validators.required],
            supportedSetupModes: [['RealOnly'], Validators.required],
            visualizationType:   ['CncTopDown2D', Validators.required],
            kinematicsType:      ['MovingGantryXY', Validators.required],
            runtimeUiVariant:    ['cnc-standard', Validators.required]
        }),

        // Axis Config — flat + per-axis sub-groups
        axisConfig: this.fb.group({
            axisCount:                [3, Validators.required],
            supportedAxes:            [['X','Y','Z'], Validators.required],
            homeOriginConvention:     ['FrontLeft'],
            workCoordinateSupport:    [true],
            machineCoordinateSupport: [true],
            relativeMoveSupport:      [true],
            absoluteMoveSupport:      [true],
            ...this.buildPerAxisGroup('axis', { role: 'Generic', direction: 'Normal', homing: false }),
        }),

        // Workspace — flat work area + per-axis travel sub-groups
        workspace: this.fb.group({
            workAreaWidth:    [400, Validators.required],
            workAreaDepth:    [300, Validators.required],
            workAreaHeight:   [100, Validators.required],
            safeZHeightMm:    [10],
            machineDimWidth:  [null],
            machineDimDepth:  [null],
            machineDimHeight: [null],
            ...this.buildPerAxisGroup('travel', { maxTravel: 0, minTravel: 0, park: 0 }),
        }),

        // Motion Defaults — per-axis sub-groups + jog presets
        motionDefaults: this.fb.group({
            ...this.buildPerAxisGroup('motion', { steps: 200, feed: 5000, accel: 500 }),
            jogPresets: this.fb.array([])
        }),

        // Connection Defaults
        connectionDefaults: this.fb.group({
            defaultBaudRate:         [115200, Validators.required],
            supportedBaudRates:      [[115200, 9600], Validators.required],
            supportedConnectionTypes:[['Serial'], Validators.required],
            requiresHandshake:       [true],
            commandTerminator:       ['\\n'],
            responseAckPattern:      ['ok'],
            protocolNotes:           ['']
        }),

        // Capabilities
        capabilities: this.fb.group({
            motion:       this.fb.group(this.boolGroup(this.motionCapKeys)),
            execution:    this.fb.group(this.boolGroup(this.executionCapKeys)),
            protocol:     this.fb.group(this.boolGroup(this.protocolCapKeys)),
            visualization:this.fb.group(this.boolGroup(this.vizCapKeys)),
            fileHandling: this.fb.group(this.boolGroup(this.fileCapKeys))
        }),

        // File Support
        fileSupport: this.fb.group({
            gcodeDialect:           ['GRBL', Validators.required],
            supportedOperationTypes:[['Milling']],
            inputFileTypesInput:    ['.gcode,.nc,.ngc']
        }),

        // Visualization
        visualization: this.fb.group({
            visualizationType:           ['CncTopDown2D'],
            kinematicsType:              ['MovingGantryXY'],
            coordinatePresentationMode:  ['BottomLeft'],
            machineShapeHint:            ['Rectangular'],
            defaultViewMode:             ['Top2D']
        }),

        // Profile Rules
        profileRules: this.fb.group({
            allowedOverrides: [[]],
            builtInProfileRules: this.fb.group({
                isEditable:           [false],
                isDeletable:          [false],
                isDuplicatable:       [true],
                duplicateProducesType:['UserProfile']
            }),
            userProfileRules: this.fb.group({
                isEditable:           [true],
                isDeletable:          [true],
                isDuplicatable:       [true],
                duplicateProducesType:['UserProfile'],
                maxUserProfiles:      [null]
            })
        })
    });

    get jogPresets(): FormArray {
        return this.form.get('motionDefaults.jogPresets') as FormArray;
    }

    ngOnInit() {
        this.editId = this.route.snapshot.paramMap.get('id');
        this.isEdit = !!this.editId;

        this.svc.getCategories(true).subscribe({ next: d => { this.categories = d; } });
        this.svc.getFamilies(undefined, true).subscribe({
            next: d => { this.allFamilies = d; this.filteredFamilies = d; }
        });

        if (this.isEdit && this.editId) {
            this.svc.getDefinitionById(this.editId).subscribe({
                next: def => this.populateForm(def),
                error: () => { this.err('Failed to load definition.'); this.router.navigate(['/admin/machine-catalog/definitions']); }
            });
        } else {
            // Seed sane defaults for primary axes
            this.patchAxisDefaults();
            this.addJogPreset();
        }
    }

    onCategoryChange() {
        const catId = this.form.get('categoryId')?.value;
        this.filteredFamilies = catId ? this.allFamilies.filter(f => f.categoryId === catId) : this.allFamilies;
        this.form.get('familyId')?.setValue('');
    }

    addJogPreset() {
        this.jogPresets.push(this.fb.group({
            label:      ['', Validators.required],
            feedMmMin:  [1000, Validators.required],
            distanceMm: [10,   Validators.required]
        }));
    }

    removeJogPreset(i: number) { this.jogPresets.removeAt(i); }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private buildPerAxisGroup(prefix: string, defaults: Record<string, any>): Record<string, FormGroup> {
        const result: Record<string, FormGroup> = {};
        for (const ax of AXES) {
            result[`${prefix}_${ax}`] = this.fb.group(
                Object.fromEntries(Object.entries(defaults).map(([k, v]) => [k, [v]]))
            );
        }
        return result;
    }

    private boolGroup(keys: string[]): Record<string, any> {
        return Object.fromEntries(keys.map(k => [k, [false]]));
    }

    private patchAxisDefaults() {
        const axisDefaults: Record<string, any> = {
            X: { role: 'Primary',   direction: 'Normal', homing: true,  steps: 200, feed: 5000, accel: 500, maxTravel: 400, minTravel: 0, park: 0   },
            Y: { role: 'Secondary', direction: 'Normal', homing: true,  steps: 200, feed: 5000, accel: 500, maxTravel: 300, minTravel: 0, park: 0   },
            Z: { role: 'Vertical',  direction: 'Normal', homing: true,  steps: 800, feed: 1000, accel: 200, maxTravel: 100, minTravel: 0, park: 10  },
            A: { role: 'Generic',   direction: 'Normal', homing: false, steps: 200, feed: 5000, accel: 500, maxTravel: 0,   minTravel: 0, park: 0   },
            B: { role: 'Generic',   direction: 'Normal', homing: false, steps: 200, feed: 5000, accel: 500, maxTravel: 0,   minTravel: 0, park: 0   },
            C: { role: 'Generic',   direction: 'Normal', homing: false, steps: 200, feed: 5000, accel: 500, maxTravel: 0,   minTravel: 0, park: 0   },
        };
        for (const ax of AXES) {
            const d = axisDefaults[ax];
            this.form.get(`axisConfig.axis_${ax}`)?.patchValue({ role: d.role, direction: d.direction, homing: d.homing });
            this.form.get(`workspace.travel_${ax}`)?.patchValue({ maxTravel: d.maxTravel, minTravel: d.minTravel, park: d.park });
            this.form.get(`motionDefaults.motion_${ax}`)?.patchValue({ steps: d.steps, feed: d.feed, accel: d.accel });
        }
    }

    private populateForm(def: MachineDefinition) {
        if (def.categoryId) {
            this.filteredFamilies = this.allFamilies.filter(f => f.categoryId === def.categoryId);
        }
        this.form.get('code')!.disable();

        this.form.patchValue({
            categoryId: def.categoryId, familyId: def.familyId,
            code: def.code, version: def.version,
            displayNameEn: def.displayNameEn, displayNameAr: def.displayNameAr,
            manufacturer: def.manufacturer,
            descriptionEn: def.descriptionEn || '', descriptionAr: def.descriptionAr || '',
            tagsInput: (def.tags || []).join(', '),
            sortOrder: def.sortOrder, releasedAt: def.releasedAt || '',
            revisionNote: def.revisionNote || '', internalNotes: def.internalNotes || '',
            isActive: def.isActive, isPublic: def.isPublic,
            isDeprecated: def.isDeprecated, deprecationNote: def.deprecationNote || '',

            runtimeBinding: {
                defaultDriverType:    def.runtimeBinding?.defaultDriverType,
                supportedDriverTypes: def.runtimeBinding?.supportedDriverTypes || [],
                firmwareProtocol:     def.runtimeBinding?.firmwareProtocol,
                supportedSetupModes:  def.runtimeBinding?.supportedSetupModes || [],
                visualizationType:    def.runtimeBinding?.visualizationType,
                kinematicsType:       def.runtimeBinding?.kinematicsType,
                runtimeUiVariant:     def.runtimeBinding?.runtimeUiVariant
            },

            axisConfig: {
                axisCount:                def.axisConfig?.axisCount,
                supportedAxes:            def.axisConfig?.supportedAxes || [],
                homeOriginConvention:     def.axisConfig?.homeOriginConvention,
                workCoordinateSupport:    def.axisConfig?.workCoordinateSupport,
                machineCoordinateSupport: def.axisConfig?.machineCoordinateSupport,
                relativeMoveSupport:      def.axisConfig?.relativeMoveSupport,
                absoluteMoveSupport:      def.axisConfig?.absoluteMoveSupport,
            },

            workspace: {
                workAreaWidth: def.workspace?.workAreaMm?.width,
                workAreaDepth: def.workspace?.workAreaMm?.depth,
                workAreaHeight: def.workspace?.workAreaMm?.height,
                safeZHeightMm: def.workspace?.safeZHeightMm,
                machineDimWidth:  def.workspace?.machineDimensionsMm?.width,
                machineDimDepth:  def.workspace?.machineDimensionsMm?.depth,
                machineDimHeight: def.workspace?.machineDimensionsMm?.height,
            },

            connectionDefaults: {
                defaultBaudRate:          def.connectionDefaults?.defaultBaudRate,
                supportedBaudRates:       def.connectionDefaults?.supportedBaudRates || [],
                supportedConnectionTypes: def.connectionDefaults?.supportedConnectionTypes || [],
                requiresHandshake:        def.connectionDefaults?.requiresHandshake,
                commandTerminator:        def.connectionDefaults?.commandTerminator,
                responseAckPattern:       def.connectionDefaults?.responseAckPattern || '',
                protocolNotes:            def.connectionDefaults?.protocolNotes || ''
            },

            capabilities: {
                motion:        def.capabilities?.motion        || {},
                execution:     def.capabilities?.execution     || {},
                protocol:      def.capabilities?.protocol      || {},
                visualization: def.capabilities?.visualization || {},
                fileHandling:  def.capabilities?.fileHandling  || {}
            },

            fileSupport: {
                gcodeDialect:            def.fileSupport?.gcodeDialect,
                supportedOperationTypes: def.fileSupport?.supportedOperationTypes || [],
                inputFileTypesInput:     (def.fileSupport?.supportedInputFileTypes || []).join(',')
            },

            visualization: {
                visualizationType:          def.visualization?.visualizationType,
                kinematicsType:             def.visualization?.kinematicsType,
                coordinatePresentationMode: def.visualization?.coordinatePresentationMode,
                machineShapeHint:           def.visualization?.machineShapeHint,
                defaultViewMode:            def.visualization?.defaultViewMode
            },

            profileRules: {
                allowedOverrides:   def.profileRules?.allowedOverrides || [],
                builtInProfileRules:def.profileRules?.builtInProfileRules || {},
                userProfileRules:   def.profileRules?.userProfileRules || {}
            }
        });

        // Per-axis: axis config
        for (const ax of AXES) {
            this.form.get(`axisConfig.axis_${ax}`)?.patchValue({
                role:      def.axisConfig?.axisRoles?.[ax]     ?? 'Generic',
                direction: def.axisConfig?.axisDirections?.[ax] ?? 'Normal',
                homing:    def.axisConfig?.homingSupport?.[ax]  ?? false
            });
            this.form.get(`workspace.travel_${ax}`)?.patchValue({
                maxTravel: def.workspace?.maxTravelMm?.[ax] ?? 0,
                minTravel: def.workspace?.minTravelMm?.[ax] ?? 0,
                park:      def.workspace?.parkPositionMm?.[ax] ?? 0
            });
            this.form.get(`motionDefaults.motion_${ax}`)?.patchValue({
                steps: def.motionDefaults?.stepsPerMm?.[ax]    ?? 200,
                feed:  def.motionDefaults?.maxFeedMmMin?.[ax]  ?? 5000,
                accel: def.motionDefaults?.maxAccelMmSec2?.[ax] ?? 500
            });
        }

        // Jog presets
        this.jogPresets.clear();
        for (const p of (def.motionDefaults?.jogPresets || [])) {
            this.jogPresets.push(this.fb.group({
                label:      [p.label,      Validators.required],
                feedMmMin:  [p.feedMmMin,  Validators.required],
                distanceMm: [p.distanceMm, Validators.required]
            }));
        }
    }

    isInvalid(path: string): boolean {
        return this.submitted && (this.form.get(path)?.invalid ?? false);
    }

    tabErr(paths: string[]): boolean {
        return this.submitted && paths.some(p => this.form.get(p)?.invalid);
    }

    save() {
        this.submitted = true;
        this.form.markAllAsTouched();
        if (this.form.invalid) { this.err('Please fill all required fields — check the tabs with a red dot.'); return; }
        this.saving = true;
        const v       = this.form.getRawValue() as any;
        const payload = this.buildPayload(v);

        const obs = this.isEdit && this.editId
            ? this.svc.updateDefinition(this.editId, payload)
            : this.svc.createDefinition(payload);

        obs.subscribe({
            next: () => {
                this.saving = false;
                this.msg.add({ severity: 'success', summary: 'Saved', detail: 'Machine definition saved.' });
                setTimeout(() => this.router.navigate(['/admin/machine-catalog/definitions']), 800);
            },
            error: (e: any) => { this.saving = false; this.err(e?.error?.message || 'Save failed.'); }
        });
    }

    private buildPayload(v: any): Partial<MachineDefinition> {
        // Build per-axis dicts from sub-groups
        const axisRoles:       Record<string, AxisRole>  = {};
        const axisDirections:  Record<string, Direction> = {};
        const homingSupport:   Record<string, boolean> = {};
        const maxTravelMm:     Record<string, number>  = {};
        const minTravelMm:     Record<string, number>  = {};
        const parkPositionMm:  Record<string, number>  = {};
        const stepsPerMm:      Record<string, number>  = {};
        const maxFeedMmMin:    Record<string, number>  = {};
        const maxAccelMmSec2:  Record<string, number>  = {};

        for (const ax of AXES) {
            const axCfg = v.axisConfig[`axis_${ax}`];
            axisRoles[ax]      = axCfg.role as AxisRole;
            axisDirections[ax] = axCfg.direction as Direction;
            homingSupport[ax]  = axCfg.homing;

            const travel = v.workspace[`travel_${ax}`];
            maxTravelMm[ax]    = travel.maxTravel ?? 0;
            minTravelMm[ax]    = travel.minTravel ?? 0;
            parkPositionMm[ax] = travel.park ?? 0;

            const mot = v.motionDefaults[`motion_${ax}`];
            stepsPerMm[ax]     = mot.steps ?? 0;
            maxFeedMmMin[ax]   = mot.feed  ?? 0;
            maxAccelMmSec2[ax] = mot.accel ?? 0;
        }

        return {
            categoryId: v.categoryId, familyId: v.familyId,
            code: v.code, version: v.version,
            displayNameEn: v.displayNameEn, displayNameAr: v.displayNameAr,
            manufacturer: v.manufacturer,
            descriptionEn: v.descriptionEn || undefined,
            descriptionAr: v.descriptionAr || undefined,
            tags: v.tagsInput ? v.tagsInput.split(',').map((t: string) => t.trim()).filter(Boolean) : [],
            sortOrder: v.sortOrder,
            releasedAt: this.isoDate(v.releasedAt),
            revisionNote: v.revisionNote || undefined,
            internalNotes: v.internalNotes || undefined,
            isActive: v.isActive, isPublic: v.isPublic,
            isDeprecated: v.isDeprecated,
            deprecationNote: v.deprecationNote || undefined,

            runtimeBinding: {
                defaultDriverType:    v.runtimeBinding.defaultDriverType,
                supportedDriverTypes: v.runtimeBinding.supportedDriverTypes,
                firmwareProtocol:     v.runtimeBinding.firmwareProtocol,
                supportedSetupModes:  v.runtimeBinding.supportedSetupModes,
                visualizationType:    v.runtimeBinding.visualizationType,
                kinematicsType:       v.runtimeBinding.kinematicsType,
                runtimeUiVariant:     v.runtimeBinding.runtimeUiVariant
            },

            axisConfig: {
                axisCount:                v.axisConfig.axisCount,
                supportedAxes:            v.axisConfig.supportedAxes,
                axisRoles, axisDirections, homingSupport,
                homeOriginConvention:     v.axisConfig.homeOriginConvention,
                workCoordinateSupport:    v.axisConfig.workCoordinateSupport,
                machineCoordinateSupport: v.axisConfig.machineCoordinateSupport,
                relativeMoveSupport:      v.axisConfig.relativeMoveSupport,
                absoluteMoveSupport:      v.axisConfig.absoluteMoveSupport
            },

            workspace: {
                maxTravelMm, minTravelMm,
                workAreaMm: {
                    width:  v.workspace.workAreaWidth,
                    depth:  v.workspace.workAreaDepth,
                    height: v.workspace.workAreaHeight
                },
                machineDimensionsMm: (v.workspace.machineDimWidth || v.workspace.machineDimDepth || v.workspace.machineDimHeight)
                    ? { width: v.workspace.machineDimWidth, depth: v.workspace.machineDimDepth, height: v.workspace.machineDimHeight }
                    : undefined,
                safeZHeightMm: v.workspace.safeZHeightMm || undefined,
                parkPositionMm
            },

            motionDefaults: {
                stepsPerMm, maxFeedMmMin, maxAccelMmSec2,
                jogPresets: v.motionDefaults.jogPresets
            },

            connectionDefaults: {
                defaultBaudRate:         v.connectionDefaults.defaultBaudRate,
                supportedBaudRates:      v.connectionDefaults.supportedBaudRates,
                supportedConnectionTypes:v.connectionDefaults.supportedConnectionTypes,
                requiresHandshake:       v.connectionDefaults.requiresHandshake,
                commandTerminator:       v.connectionDefaults.commandTerminator,
                responseAckPattern:      v.connectionDefaults.responseAckPattern || undefined,
                protocolNotes:           v.connectionDefaults.protocolNotes || undefined
            },

            capabilities: {
                motion:        v.capabilities.motion,
                execution:     v.capabilities.execution,
                protocol:      v.capabilities.protocol,
                visualization: v.capabilities.visualization,
                fileHandling:  v.capabilities.fileHandling
            },

            fileSupport: {
                gcodeDialect:            v.fileSupport.gcodeDialect,
                supportedOperationTypes: v.fileSupport.supportedOperationTypes,
                supportedInputFileTypes: v.fileSupport.inputFileTypesInput
                    ? v.fileSupport.inputFileTypesInput.split(',').map((s: string) => s.trim()).filter(Boolean)
                    : []
            },

            visualization: {
                visualizationType:          v.visualization.visualizationType,
                kinematicsType:             v.visualization.kinematicsType,
                coordinatePresentationMode: v.visualization.coordinatePresentationMode,
                machineShapeHint:           v.visualization.machineShapeHint,
                defaultViewMode:            v.visualization.defaultViewMode
            },

            profileRules: {
                allowedOverrides:    v.profileRules.allowedOverrides,
                overrideConstraints: [],
                builtInProfileRules: v.profileRules.builtInProfileRules,
                userProfileRules:    v.profileRules.userProfileRules
            }
        };
    }

    cancel() { this.router.navigate(['/admin/machine-catalog/definitions']); }

    private isoDate(s: string): string | undefined {
        if (!s) return undefined;
        const d = new Date(s);
        if (isNaN(d.getTime())) return undefined;
        return d.toISOString().split('T')[0];
    }

    private err(msg: string) {
        this.msg.add({ severity: 'error', summary: 'Error', detail: msg });
    }
}
