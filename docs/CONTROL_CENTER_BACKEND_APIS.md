## MABA Control Center – Backend API Specification

This document defines the **backend/API surface** required for the MABA Control Center platform (for Dexter, MABA SCARA, and future hardware), **independent from the existing Maba website**.

The goals:
- **Complete domain coverage** for Control Center scenarios.
- **Clear, typed contracts** so multiple implementations are possible (e.g. .NET, Node, Go).
- **Online + offline support** at the client side (Control Center app).
- **Multi-tenant, multi-site** support with RBAC.

The API style below assumes **REST over HTTP** with JSON, with optional **WebSocket/SSE** channels for real-time events. All routes are versioned under:

- Base URL: `/control-center/api/v1`

> You can implement this spec in any tech stack (for example, a .NET 8 Web API service separate from the existing website solution, or a Node/NestJS microservice).

---

## 1. Auth & Identity API

### 1.1 Authentication

- **POST** `/auth/login`
  - Body: `{ usernameOrEmail, password }`
  - Response: `{ accessToken, refreshToken, expiresIn, user }`
  - Rate‑limited.

- **POST** `/auth/refresh`
  - Body: `{ refreshToken }`
  - Response: `{ accessToken, refreshToken, expiresIn }`

- **POST** `/auth/logout`
  - Invalidates refresh token(s).

- **POST** `/auth/device-login`
  - Optionally support device-code/activation:
  - Body: `{ deviceId, activationCode }`
  - Response: `{ accessToken, refreshToken, controlCenterInstanceId }`

### 1.2 Current user & sessions

- **GET** `/auth/me`
  - Returns current user profile and active org/site context.

- **GET** `/auth/sessions`
  - Lists active sessions for this user.

- **DELETE** `/auth/sessions/{sessionId}`
  - Revokes a specific session.

---

## 2. Tenants, Orgs, Sites, and RBAC

### 2.1 Organizations & Sites

- **GET** `/orgs`
- **GET** `/orgs/{orgId}`
- **POST** `/orgs`
- **PATCH** `/orgs/{orgId}`

- **GET** `/orgs/{orgId}/sites`
- **POST** `/orgs/{orgId}/sites`
- **GET** `/sites/{siteId}`
- **PATCH** `/sites/{siteId}`

> All major entities (devices, jobs, configs, logs) are scoped by **org** and **site**.

### 2.2 Users, Roles & Permissions

- **GET** `/orgs/{orgId}/users`
- **POST** `/orgs/{orgId}/users`
- **PATCH** `/orgs/{orgId}/users/{userId}`

- **GET** `/orgs/{orgId}/roles`
- **POST** `/orgs/{orgId}/roles`
- **PATCH** `/orgs/{orgId}/roles/{roleId}`

- **GET** `/permissions`
  - Static list of permission identifiers used for RBAC.

Authorization:
- JWT contains `orgId`, optionally `siteId`, and roles/permissions.
- Backend enforces RBAC per endpoint.

---

## 3. Licensing & Entitlements API

### 3.1 Products & Modules

- **GET** `/products`
  - Returns licensed product definitions (e.g. `dexter`, `maba-scara`, “control-center-core”).

- **GET** `/modules/catalog`
  - Full list of modules:
  - Response item example:
    - `{ id, name, description, productId, latestVersion, minCoreVersion, status }`

### 3.2 Licenses & Entitlements

- **GET** `/orgs/{orgId}/licenses`
- **GET** `/sites/{siteId}/licenses`

- **GET** `/entitlements`
  - For current user + site:
  - Response:
    - `modulesAllowed: string[]`
    - `deviceTypesAllowed: string[]`
    - `constraints: { maxDevices, maxSites, expiresAt, ... }`

> Control Center calls this periodically when online and **caches** the result for offline use.

---

## 4. Devices & Hardware Inventory API

### 4.1 Device registry

- **GET** `/sites/{siteId}/devices`
- **POST** `/sites/{siteId}/devices`
  - Create/register a device (e.g. Dexter #123, MABA SCARA #55).

- **GET** `/devices/{deviceId}`
- **PATCH** `/devices/{deviceId}`
- **DELETE** `/devices/{deviceId}`

Attributes:
- `id`, `orgId`, `siteId`
- `name`, `serialNumber`, `type` (e.g. `dexter-arm`, `maba-scara`)
- `status` (logical cloud status, not real‑time hardware status)
- `firmwareVersion`, `moduleId` (which module controls it)
- `location`, `tags`, `metadata`

### 4.2 Device groups / cells

- **GET** `/sites/{siteId}/cells`
- **POST** `/sites/{siteId}/cells`
- **PATCH** `/cells/{cellId}`
- **DELETE** `/cells/{cellId}`
- **POST** `/cells/{cellId}/devices/{deviceId}` – add device to cell
- **DELETE** `/cells/{cellId}/devices/{deviceId}`

---

## 5. Modules & Control Center Versions API

### 5.1 Module catalog

- **GET** `/modules/catalog`
- **GET** `/modules/catalog/{moduleId}`

Response fields:
- `id`, `name`, `description`, `productId`
- `currentVersion`, `supportedCoreVersions`
- `status` (`active`, `deprecated`, `blocked`)
- `capabilities: string[]`

### 5.2 Module distribution

- **GET** `/modules/bundles/{moduleId}/{version}`
  - Returns a signed URL or direct binary stream for the module bundle.
  - Includes integrity metadata (`checksum`, `signature`).

- **GET** `/modules/compatibility`
  - Query: `coreVersion`, optional `deviceType`
  - Returns compatible module versions.

### 5.3 Control Center instances

- **POST** `/instances/register`
  - Body: `{ machineId, hostname, os, coreVersion }`
  - Response: `{ instanceId, authInfo }`

- **PATCH** `/instances/{instanceId}`
  - Update last‑seen, installed modules, etc.

- **GET** `/instances/{instanceId}`

---

## 6. Jobs & Configurations API

### 6.1 Job templates & library

- **GET** `/sites/{siteId}/job-templates`
- **POST** `/sites/{siteId}/job-templates`
- **GET** `/job-templates/{templateId}`
- **PATCH** `/job-templates/{templateId}`
- **DELETE** `/job-templates/{templateId}`

Fields:
- `id`, `orgId`, `siteId`
- `name`, `description`
- `deviceType`, `moduleId`
- `definition` (JSON/binary reference to G-code, path-plan, etc.)
- `version`, `createdBy`, `updatedBy`

### 6.2 Jobs (executions)

- **GET** `/sites/{siteId}/jobs`
- **POST** `/sites/{siteId}/jobs`
  - Body includes:
    - `templateId` or inline `definition`
    - `deviceId`
    - `parameters` (key/value)

- **GET** `/jobs/{jobId}`
- **PATCH** `/jobs/{jobId}`
  - e.g. cancel, change priority.

Job state:
- `status` = `queued`, `running`, `paused`, `completed`, `failed`, `canceled`.
- `progress`, `startedAt`, `completedAt`, `resultSummary`.

> The Control Center is responsible for **actual execution** and reports state back via Telemetry and Command Results (below).

### 6.3 Configurations

- **GET** `/sites/{siteId}/configs`
- **POST** `/sites/{siteId}/configs`
- **GET** `/configs/{configId}`
- **PATCH** `/configs/{configId}`

Fields:
- `scope` (`device`, `cell`, `site`, `global`)
- `deviceType` / `moduleId`
- `schemaId`, `values` (JSON), `version`

---

## 7. Telemetry & Audit API

### 7.1 Telemetry ingestion

- **POST** `/telemetry/batch`
  - Body: array of records like:
  - `{ timestamp, instanceId, deviceId, metricType, value, unit, tags }`

### 7.2 Device & instance heartbeats

- **POST** `/instances/{instanceId}/heartbeat`
  - Body: `{ coreVersion, modules: { id, version }[], status, osInfo }`

- **POST** `/devices/{deviceId}/heartbeat`
  - Body: `{ status, firmwareVersion, lastJobId, metricsSnapshot }`

### 7.3 Audit logs

- **POST** `/audit/batch`
  - Records:
  - `{ timestamp, userId, orgId, siteId, instanceId, deviceId, action, meta }`

- **GET** `/audit`
  - Query: `from`, `to`, `userId`, `deviceId`, `siteId`, `action`.

---

## 8. Command & Remote Control API

Cloud orchestrates **commands**; Control Center executes them and reports results.

### 8.1 Commands

- **POST** `/commands`
  - Body:
    - `targetType`: `instance` | `device`
    - `targetId`: `instanceId` or `deviceId`
    - `commandType`: e.g. `RUN_JOB`, `APPLY_CONFIG`, `PAUSE_JOB`, `STOP_JOB`, `REQUEST_DIAGNOSTICS`
    - `payload`: JSON command body
  - Response: `{ commandId, status: 'queued' }`

- **GET** `/commands/{commandId}`
  - Check status and basic result.

### 8.2 Command polling (fallback when WS not available)

- **GET** `/instances/{instanceId}/commands/pending`
  - Control Center polls for commands addressed to this instance or its devices.

### 8.3 Command results

- **POST** `/commands/{commandId}/result`
  - Body:
    - `status`: `acknowledged` | `in-progress` | `completed` | `failed`
    - `resultPayload`: JSON
    - `errorMessage?`

### 8.4 Real‑time channel

- **WebSocket/SSE** `/ws/commands`
  - Authenticated stream:
    - Server → client: `commandCreated`, `commandUpdated`.
    - Client → server: `commandAck`, `commandResult`.

---

## 9. Notifications & Events API

### 9.1 Notifications

- **GET** `/notifications`
  - Query: `status` (`unread`, `all`), `type`, `siteId`.

- **POST** `/notifications`
  - For system‑generated or admin‑created notifications (e.g. maintenance windows).

- **PATCH** `/notifications/{notificationId}`
  - Mark as read/archived.

Types:
- `ALERT` (job failed, device offline)
- `WARNING` (license near expiry, high error rate)
- `INFO` (job completed, sync ok)

### 9.2 Events stream

- **SSE/WS** `/ws/events`
  - Streams notifications and important realtime events to Control Center and other tools.

---

## 10. Webhooks & Integrations API

### 10.1 Webhook management

- **GET** `/integrations/webhooks`
- **POST** `/integrations/webhooks`
  - Body:
    - `url`
    - `secret`
    - `events: string[]` (e.g. `job.completed`, `device.offline`)

- **PATCH** `/integrations/webhooks/{webhookId}`
- **DELETE** `/integrations/webhooks/{webhookId}`

### 10.2 Outbound events

Events (examples):
- `job.created`, `job.started`, `job.completed`, `job.failed`
- `device.registered`, `device.offline`, `device.online`
- `config.applied`

> Implementation: send signed POSTs with HMAC using the stored secret.

### 10.3 Inbound basic integration

- **POST** `/integrations/jobs/start`
  - For external systems to request a job run:
  - Body: `{ siteId, deviceId, templateId, parameters }`
  - Internally creates a Job + Command record.

---

## 11. Firmware & Software Distribution API

### 11.1 Firmware catalog

- **GET** `/firmware/device-types`
- **GET** `/firmware/device-types/{deviceType}/releases`

Release object:
- `version`, `minModuleVersion`, `minCoreVersion`
- `releaseNotes`, `createdAt`
- `status`: `stable`, `beta`, `deprecated`

### 11.2 Firmware download

- **GET** `/firmware/bundles/{deviceType}/{version}`
  - Returns binary or signed URL.

### 11.3 Firmware history

- **GET** `/devices/{deviceId}/firmware-history`

---

## 12. Configuration Policy & Compliance API

### 12.1 Policies

- **GET** `/sites/{siteId}/policies`
- **POST** `/sites/{siteId}/policies`
- **GET** `/policies/{policyId}`
- **PATCH** `/policies/{policyId}`

Policy fields:
- `scope` (site/deviceType/device)
- `rules` (structured JSON)
- `severity` (info/warn/block)

### 12.2 Validation

- **POST** `/policies/validate-config`
  - Body:
    - `siteId`, `deviceType`, `configValues`
  - Response:
    - `isCompliant: boolean`
    - `violations: { ruleId, message, severity }[]`

---

## 13. Backup & Restore API

### 13.1 Backups

- **POST** `/backups`
  - Control Center uploads an encrypted backup.
  - Body: metadata + binary (or multipart).

- **GET** `/backups`
  - List backups for a site/instance.

- **GET** `/backups/{backupId}/download`

### 13.2 Restore

- **POST** `/backups/{backupId}/restore-request`
  - Enqueues a restore operation for a given instance/site.

> Actual crypto/key management can be simplified but must be conceptually sound in the implementation (e.g. client‑side encryption keys).

---

## 14. Health, Diagnostics & Support API

### 14.1 Health & diagnostics

- **GET** `/health`
  - Liveness and readiness probes.

- **POST** `/instances/{instanceId}/diagnostics`
  - Uploads a diagnostic snapshot/log bundle (archived file or JSON).

### 14.2 Support tickets

- **GET** `/support/tickets`
- **POST** `/support/tickets`
  - Body includes `subject`, `description`, optional `deviceId`, `jobId`.

- **GET** `/support/tickets/{ticketId}`
- **PATCH** `/support/tickets/{ticketId}`

---

## 15. Implementation Notes

- **Versioning**: all endpoints under `/control-center/api/v1`; future versions can use `/v2`.
- **Security**: JWT bearer tokens, HTTPS only, role/permission checks per route.
- **Rate limiting**: stricter for auth and command‑related endpoints.
- **Errors**: standardized error envelope:
  - `{ code, message, details?, traceId? }`
- **Pagination**: for list endpoints use `page`, `pageSize`, `total` in responses.

This document is intentionally **backend‑centric**. The Control Center client (Electron/Tauri/desktop app) is expected to:
- Cache data locally for offline use (SQLite or equivalent).
- Use these APIs when online to:
  - Refresh identity and entitlements.
  - Sync jobs/configs/telemetry.
  - Receive commands and send results.
- Be the ultimate authority for **physical device control**, while the backend coordinates workflows, visibility, and governance.

