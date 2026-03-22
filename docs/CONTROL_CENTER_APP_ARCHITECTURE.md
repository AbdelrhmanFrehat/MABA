## MABA Control Center – Application Architecture

This document defines the **application-side architecture** for the MABA Control Center, which manages MABA hardware (e.g. Dexter, MABA SCARA) using the backend APIs described in `CONTROL_CENTER_BACKEND_APIS.md`.

The design is implementation-agnostic but assumes:
- Desktop app (Windows required, Linux/macOS nice-to-have).
- Offline-first behavior with local storage.
- Clear separation of concerns to allow multiple modules and hardware types.

---

## 1. High-Level Overview

The Control Center is composed of three main parts:

1. **UI Shell (Desktop Frontend)**
   - Built with a desktop runtime (e.g. Electron, Tauri) and a web UI framework (e.g. React or Angular).
   - Responsibilities:
     - Global navigation (Dashboard, Devices, Jobs, Modules, Settings).
     - Online/offline indicator and sync status.
     - User session display and org/site selection.
     - Notification center.
     - Hosting and routing into product modules (Dexter, MABA SCARA, etc.).

2. **Local Core Service Layer**
   - Runs in the same process (main process) or as a companion service.
   - Responsibilities:
     - **Module registry & loader**.
     - **Device abstraction & drivers** (real or simulated).
     - **Job execution coordinator** (per device/module).
     - **Local data store** (SQLite or similar, accessed through a small DAL).
     - **Sync engine** (between local store and backend API).
     - **Command & event bus** between UI, modules, and device drivers.

3. **Backend Integration Layer**
   - Typed clients for the backend APIs (HTTP + WebSocket/SSE).
   - Responsibilities:
     - Auth & token handling.
     - Data sync: users, org/sites, devices, jobs, configs, licenses.
     - Command channel: receive remote commands, send results.
     - Telemetry and audit upload.

---

## 2. Recommended Tech Stack (Reference)

You can adjust as needed, but a practical stack is:

- **Desktop runtime**: Electron 30+ or Tauri.
- **UI framework**: React (with TypeScript), using:
  - React Router for routing.
  - A UI kit (e.g. MUI/Ant Design/PrimeReact) for consistent components.
- **State management**:
  - React Query (TanStack Query) for server/cache state.
  - Lightweight local state via React Context/Zustand/Recoil.
- **Local core**:
  - Node.js (Electron main process) with TypeScript.
  - SQLite database via a library (e.g. better-sqlite3 or Prisma).
- **Backend client**:
  - OpenAPI-generated or hand-written TypeScript clients for `/control-center/api/v1`.
  - WebSocket/SSE for commands and events.

If you choose a different stack (e.g. .NET WPF/MAUI or Angular in Electron), keep the same **layer boundaries**.

---

## 3. Layered Architecture

### 3.1 Presentation Layer (UI Shell + Module UIs)

Packages:
- `ui-shell/`
- `modules/` (Dexter, MABA SCARA, future modules)

Responsibilities:
- Layout, navigation, theming.
- Views: Dashboard, Devices, Jobs, Modules, Settings, Notifications.
- Per-module UIs:
  - Device list for that module.
  - Device detail (status, controls, logs).
  - Job management screens.

UI communicates **only** with:
- A **core API** (in-process service) for local operations.
- Backend integration indirectly via the core (not directly from every component).

### 3.2 Core Application Layer

Packages (conceptual):
- `core/modules` – module registry, manifests, lifecycle.
- `core/devices` – device registry, drivers, connections.
- `core/jobs` – job scheduler/executor per device.
- `core/config` – configuration storage and validation (using policies where needed).
- `core/sync` – synchronization engine (online/offline).
- `core/events` – event bus (pub/sub).
- `core/storage` – abstraction over local DB (SQLite).
- `core/security` – local token storage, encryption helpers.

Responsibilities:
- Owns **authoritative local state**.
- Exposes a **typed internal API** that the UI uses (e.g. via IPC or direct function calls depending on runtime).
- Coordinates between multiple modules and devices.
- Implements offline-first logic:
  - Reads/writes to local DB.
  - Syncs to/from backend when online.

### 3.3 Backend Integration Layer

Packages:
- `integration/api-client` – HTTP client wrappers around the backend API.
- `integration/realtime` – WebSocket/SSE clients for commands/events.

Responsibilities:
- Encapsulate **all network logic**.
- Provide higher-level operations, such as:
  - `syncOrgContext()`
  - `syncDevices()`
  - `syncJobs()`
  - `pullCommands()`
  - `pushCommandResults()`
  - `uploadTelemetryBatch()`
  - `uploadAuditBatch()`

Core layer calls these integration services; UI does not call backend directly.

---

## 4. Module System Design

### 4.1 Module Manifest

Each module (e.g. Dexter, MABA SCARA) must provide a manifest, similar to:

```ts
interface ModuleManifest {
  id: string;             // e.g. "dexter"
  name: string;           // "Dexter Robotic Arm"
  version: string;        // "1.0.0"
  icon: string;           // path or icon key
  supportedDeviceTypes: string[]; // e.g. ["dexter-arm"]
  routes: {
    basePath: string;     // e.g. "/modules/dexter"
    entryComponent: React.ComponentType<any>; // main module view
  };
  capabilities: string[]; // e.g. ["jobs", "live-status", "manual-control"]
}
```

Modules are loaded by a **Module Registry** which:
- Discovers built-in and installed modules.
- Registers their routes/components into the UI shell.
- Registers available driver factories for device types.

### 4.2 Driver Interface

Each module that controls hardware must implement driver factories for its device types:

```ts
interface DeviceDriver {
  connect(): Promise<void>;
  disconnect(): Promise<void>;
  runJob(jobDefinition: any, params: any): Promise<void>;
  sendCommand(command: any): Promise<any>;
  getStatus(): Promise<DeviceStatusSnapshot>;
  onStatusChange(cb: (s: DeviceStatusSnapshot) => void): void;
  onError(cb: (err: Error) => void): void;
}
```

`core/devices` manages instances of these drivers and exposes:
- `connectDevice(deviceId)`
- `disconnectDevice(deviceId)`
- `runJobOnDevice(deviceId, jobId)`
- `getDeviceStatus(deviceId)`

Modules implement the **UI** and **driver**; the core coordinates and persists.

### 4.3 Example Modules

At minimum:
- `modules/dexter`
  - Simulated robotic arm: joint positions, job execution with progress.
- `modules/maba-scara`
  - Simulated SCARA robot: XYθZ positions, homing, program execution.

Both:
- Use the same `DeviceDriver` interface.
- Register a manifest and driver factory with the core.

---

## 5. Local Data & Offline-First Behavior

### 5.1 Local Data Store

Use **SQLite** via a small data access layer:
- Tables (conceptual):
  - `users` (cached current user/org context).
  - `devices`, `device_status_cache`.
  - `jobs`, `job_templates`, `job_runs`.
  - `configs`.
  - `telemetry_queue`, `audit_queue`.
  - `commands_outbox`, `commands_inbox_cache`.
  - `modules` (installed modules and versions).
  - `settings` (app settings, feature flags).

The core exposes repository-like operations; the UI never talks to SQLite directly.

### 5.2 Offline-first rules

- App must be able to **start fully offline**:
  - Load last known data from SQLite.
  - Allow local job execution, config edits, device control (subject to cached entitlements).
- When network becomes available:
  - Authenticate/refresh token.
  - Run **sync cycles**:
    - Upload telemetry, audit, queued jobs/configs, command results.
    - Download updated devices, jobs, configs, licenses, modules metadata.
  - Mark last sync time and status.

### 5.3 Sync Engine

The sync engine:
- Runs:
  - On startup (if online).
  - On a schedule (e.g. every N minutes).
  - On-demand (“Sync now” button).
- Has **directional phases**:
  1. **Upload phase**:
     - Telemetry, audit logs, outbox commands, locally created jobs/configs.
  2. **Download phase**:
     - Updated org/site info, licenses, devices, templates, policies, modules metadata.
- Handles **conflicts** with a simple strategy:
  - Prefer latest `updatedAt` timestamp (last-writer-wins) while recording audit entries.

---

## 6. Command & Event Handling

### 6.1 Internal Event Bus

Implement a simple pub/sub bus in `core/events`:
- Events examples:
  - `deviceStatusUpdated`
  - `jobStatusChanged`
  - `syncStateChanged`
  - `commandReceived`
  - `notificationCreated`

UI subscribes to these events to update views in real-time.

### 6.2 Backend Commands

- Integration layer subscribes to:
  - WebSocket/SSE stream for `/ws/commands`.
  - Or falls back to polling `/instances/{id}/commands/pending`.
- When a `command` arrives:
  - It is stored locally in `commands_inbox_cache`.
  - The core translates it into local operations:
    - Run job, apply config, request diagnostics, etc.
  - Progress and results are:
    - Updated locally.
    - Sent back via `/commands/{id}/result`.

---

## 7. UX Shell Structure (Reference)

High-level React route structure (example):

- `/` – Dashboard
- `/devices` – Device list
- `/devices/:deviceId` – Device detail
- `/jobs` – Jobs list
- `/jobs/:jobId` – Job detail
- `/modules` – Module catalog
- `/modules/:moduleId/*` – Routed into module UIs
- `/settings` – App settings (org/site selection, network, updates)
- `/notifications` – Notification center

Layout:
- Left navigation sidebar for main sections.
- Top bar:
  - User/avatar + org/site picker.
  - Online/offline indicator.
  - Sync status (icon + last sync time).
  - Notifications icon.

---

## 8. Security & Configuration (Client-Side)

- Store tokens securely (in OS-protected storage or encrypted on disk).
- Do not store raw passwords.
- All HTTP traffic goes over HTTPS.
- Configuration:
  - Environment profiles (dev/stage/prod) for backend URLs, logging levels.
  - Feature flags for beta modules.

---

## 9. Implementation Roadmap (for another Cursor instance)

Suggested order of implementation:

1. **Bootstrap desktop shell**:
   - Electron/Tauri app with React.
   - Basic layout, routing, and dummy pages.
2. **Implement core storage & models**:
   - SQLite schema.
   - Repository/DAO layer.
3. **Implement core services**:
   - Devices, jobs, configs, modules registry.
   - Internal event bus.
4. **Implement backend integration layer**:
   - Auth, org/site, devices, jobs, configs, telemetry, commands.
5. **Implement sync engine & online/offline flow**.
6. **Implement module SDK & register two modules** (Dexter, MABA SCARA) with simulated devices.
7. **Wire up UI**:
   - Devices, Jobs, Modules, Settings, Notifications.
8. **Add tests** for:
   - Module registry.
   - Sync engine logic.
   - Device driver simulations.

This architecture doc, together with `CONTROL_CENTER_BACKEND_APIS.md`, should provide enough guidance for a separate Cursor agent to implement a **professional, extensible MABA Control Center** end-to-end.

