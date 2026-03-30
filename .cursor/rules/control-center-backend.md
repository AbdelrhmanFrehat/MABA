You are working ONLY on the backend (.NET) API of the existing Maba system, located under `API/` in this repo.

## Context

- The current backend is a .NET 8 Web API with projects:
  - `Maba.Api`
  - `Maba.Application`
  - `Maba.Infrastructure`
  - `Maba.Domain`
  - plus `Maba.Application.Tests`.
- Stack:
  - ASP.NET Core Web API, MediatR (CQRS), EF Core 8 + SQL Server
  - FluentValidation, Serilog, JWT auth, custom permission-based RBAC, rate limiting, SignalR.
- Entry point:
  - `Maba.Api/Program.cs` configures:
    - `AddInfrastructure(builder.Configuration);`
    - `AddApplication();`
    - JWT bearer auth
    - rate limiting (`AddRateLimiter`, named `"auth"` policy)
    - custom `AuthorizationPolicies.ConfigurePolicies`
    - `PermissionHandler` for permission-based RBAC
    - automatic EF migrations and seeding in development.
- EF Core DbContext:
  - `Maba.Infrastructure/Data/ApplicationDbContext.cs`
  - Uses `BaseEntity` from `Maba.Domain.Common` (`Id`, `CreatedAt`, `UpdatedAt`) and sets timestamps in `SaveChangesAsync`.
  - `OnModelCreating` applies configurations via `ApplyConfigurationsFromAssembly(...)` and standardizes decimal precision.
- Existing domain organization:
  - `Maba.Domain` has folders for `Users`, `Catalog`, `Machines`, `Printing`, `Orders`, `Finance`, `Cms`, `AiChat`, `SupportChat`, `Laser`, `Software`, `Cnc`, `Projects`, `Faq`, `HeroTicker`, `Design`, `DesignCad`, `Media`, and `Common`.
- Authorization / RBAC:
  - Permission identifiers live in `Maba.Api.Authorization.AuthorizationPolicies` as constants (e.g. `"ViewUsers"`) mapped to `PermissionRequirement` with low-level keys like `"users.view"`, `"orders.manage"`, etc.
  - `PermissionHandler` checks user permissions via EF entities `UserRole`, `RolePermission`, `Permission` through `IApplicationDbContext`.

## Separate spec

- The file `docs/CONTROL_CENTER_BACKEND_APIS.md` defines the desired API surface for the **MABA Control Center** platform (Dexter, MABA SCARA, etc.), with all endpoints under a conceptual base URL `/control-center/api/v1`.
- Your job: **implement the missing Control Center backend APIs inside the existing Maba solution**, reusing the existing projects and stack — NOT creating a separate microservice.

## High-level goals

- Implement the Control Center endpoints and data model described in `docs/CONTROL_CENTER_BACKEND_APIS.md`, but:
  - Use the existing solution structure (`Maba.Api`, `Maba.Application`, `Maba.Infrastructure`, `Maba.Domain`, tests).
  - Use `ApplicationDbContext` from `Maba.Infrastructure.Data` (do NOT introduce a second DbContext).
  - Follow existing patterns:
    - CQRS with MediatR in `Maba.Application` (commands/queries, handlers, DTOs, validators).
    - Domain entities in `Maba.Domain`.
    - Persistence via EF Core in `Maba.Infrastructure` with configuration classes.
    - Controllers in `Maba.Api` using MediatR and the existing auth/RBAC setup.
- Expose all Control Center endpoints under a dedicated API prefix in `Maba.Api`, for example:
  - `[Route("api/v1/control-center/[controller]")]`
  - or `[Route("api/v1/cc/[controller]")]`
- Keep everything production-grade:
  - Validation via FluentValidation.
  - Auth + permission-based RBAC.
  - Org/Site scoping (multi-tenancy) fields on entities.
  - EF migrations aligned with the existing migration workflow.
  - Tests in `Maba.Application.Tests` and (optionally) API/integration tests.

## Step 1 – Inspect and follow existing conventions

When adding or refactoring code, always refer to these existing patterns:

- `Maba.Api/Program.cs`:
  - Startup, DI, authentication, rate limiting, SignalR hub mapping.
  - Exception handling via `UseExceptionHandling()` middleware.
  - Automatic migrations and seeding in development.
- Controllers:
  - Example: `Maba.Api/Controllers/AuthController.cs` (pattern for:
    - `[ApiController]`
    - `[Route("api/v1/[controller]")]`
    - `[Authorize]` attributes
    - MediatR usage with commands/queries as inputs and DTOs as responses.
- DbContext:
  - `Maba.Infrastructure/Data/ApplicationDbContext.cs`:
    - Add new `DbSet<T>` properties for new entities.
    - Let configurations be picked up via `ApplyConfigurationsFromAssembly(...)`.
    - Rely on `BaseEntity` for `Id`, `CreatedAt`, `UpdatedAt`.
- Domain:
  - Use folders under `Maba.Domain` per feature.
  - Use `BaseEntity` as base for new aggregates.
- Application:
  - Use `Maba.Application/Features/<FeatureName>/...` to group:
    - Commands, Queries, DTOs, Handlers, Validators.
  - Commands/queries implement `IRequest<T>` (MediatR).
  - Handlers inject `IApplicationDbContext` and any other infrastructure services.
  - Validation via FluentValidation.

## Step 2 – Design Control Center domain model on top of existing DB

Using `docs/CONTROL_CENTER_BACKEND_APIS.md` as the source of truth, introduce a new domain area under `Maba.Domain/ControlCenter/` (create it if it doesn’t exist) with the following entities (names can be slightly adapted, but keep them consistent across Domain/Application/Api):

### Instances

- `ControlCenterInstance` (represents one installed Control Center on a machine)
  - Example fields:
    - `Id` (from `BaseEntity`)
    - `OrgId` (Guid) – multi-tenancy
    - `SiteId` (Guid?) – multi-site
    - `MachineId` (Guid?) – link to existing `Machine` if appropriate
    - `Hostname` (string)
    - `OsInfo` (string / JSON)
    - `CoreVersion` (string)
    - `LastSeenAt` (DateTime?)
    - `InstalledModules` (e.g. JSON or related table)
    - Plus `CreatedAt` / `UpdatedAt` from `BaseEntity`.

### Modules

- `CcModuleCatalogEntry` – backend view of Control Center modules:
  - `ProductId`, `Name`, `Description`
  - `CurrentVersion`
  - `SupportedCoreVersions` (string or JSON)
  - `Status` (`active`, `deprecated`, `blocked`)
  - `Capabilities` (string / JSON).

### Devices

- If existing `Maba.Domain.Machines` already models devices you can reuse/extend them.
- Otherwise introduce:
  - `CcDevice`:
    - `OrgId`, `SiteId`
    - `Name`, `SerialNumber`, `Type` (`dexter-arm`, `maba-scara`, etc.)
    - `Status` (logical cloud status)
    - `FirmwareVersion`, `ModuleId`
    - `Location`, `Tags`, `Metadata` (JSON).
  - Optionally `CcCell` for grouping devices.

### Jobs & Job Templates

- `CcJobTemplate`:
  - `OrgId`, `SiteId`
  - `Name`, `Description`
  - `DeviceType`, `ModuleId`
  - `Definition` (JSON/string reference to G-code/path-plan/etc.)
  - `Version`, `CreatedBy`, `UpdatedBy`.
- `CcJob`:
  - `OrgId`, `SiteId`
  - `TemplateId` (Guid)
  - `DeviceId` (Guid)
  - `Status` = enum `{Queued, Running, Paused, Completed, Failed, Canceled}`
  - `Progress`, `StartedAt`, `CompletedAt`, `ResultSummary`.
- `CcJobExecution` or similar if needed as a separate execution history table.

### Configs

- `CcConfig`:
  - `OrgId`, `SiteId`
  - `Scope` (`device`, `cell`, `site`, `global`)
  - `DeviceType` / `ModuleId`
  - `SchemaId`, `Values` (JSON), `Version`.

### Commands

- `CcCommand`:
  - `OrgId`, `SiteId`
  - `TargetType` (`instance` | `device`)
  - `TargetId` (Guid)
  - `CommandType` (string)
  - `Payload` (JSON)
  - `Status` enum: `Queued`, `Acknowledged`, `InProgress`, `Completed`, `Failed`
  - Timestamps for queued/acknowledged/completed.
- Optionally `CcCommandResult` if you prefer to store results separately.

### Telemetry & Audit

- If existing `AuditLog` etc. are a good fit, extend them.
- Otherwise add:
  - `CcTelemetryRecord`:
    - `Timestamp`, `InstanceId`, `DeviceId`
    - `MetricType`, `Value`, `Unit`, `Tags` (JSON).
  - `CcAuditEvent`:
    - `Timestamp`, `UserId`, `OrgId`, `SiteId`
    - `InstanceId`, `DeviceId`
    - `Action`, `Metadata` (JSON).

### Notifications

- If existing `Notification` entity is present, you can introduce a `ControlCenter`-specific subtype or additional fields.
- Otherwise create `CcNotification`:
  - `OrgId`, `SiteId`, `UserId?`
  - `Type`, `Title`, `Message`
  - `Severity` (`INFO`, `WARNING`, `ALERT`)
  - `Status` (`Unread`, `Read`, `Archived`).

### Policies, Backups, Firmware

- `CcPolicy`, `CcBackup`, `CcFirmwareRelease` and any other entities required by `CONTROL_CENTER_BACKEND_APIS.md` as time allows.

Update `ApplicationDbContext` (`Maba.Infrastructure/Data/ApplicationDbContext.cs`):

- Add `DbSet<T>` properties for each new entity, e.g.:
  - `public DbSet<ControlCenterInstance> ControlCenterInstances => Set<ControlCenterInstance>();`
  - `public DbSet<CcDevice> CcDevices => Set<CcDevice>();`
  - etc.
- Add EF Core configuration classes under `Maba.Infrastructure/Data/Configurations/ControlCenter/` (follow existing patterns in that folder) to configure keys, indexes, relationships, and property lengths.

## Step 3 – EF Core migrations

- Use the existing migration workflow documented in `API/` (see `CREATE_MIGRATION.md` and related scripts like `create-migration.ps1`).
- Add one or more migrations to create/update tables for the Control Center entities, for example:
  - `AddControlCenterCoreEntities`
- Ensure:
  - `OrgId` and `SiteId` are present on multi-tenant entities and typed consistently (`Guid`).
  - `CreatedAt`/`UpdatedAt` are handled by `BaseEntity` + the overridden `SaveChangesAsync`.
  - Foreign keys:
    - To existing `User` where needed (`CreatedBy`, `UpdatedBy`, etc.).
    - To Control Center entities (e.g. `CcJob` → `CcJobTemplate`, `CcDevice`, `ControlCenterInstance`).
- After adding migrations:
  - Make sure they compile.
  - Run them using the established dev workflow (e.g. `dotnet ef database update` via the provided scripts).

## Step 4 – Application layer (CQRS) for Control Center

Create a new feature root under `Maba.Application/Features/ControlCenter/` with subfolders:

- `Instances`
- `Devices`
- `Modules`
- `Jobs`
- `JobTemplates`
- `Configs`
- `Commands`
- `Telemetry`
- `Audit`
- `Notifications`
- `Policies`
- `Backups`
- `Firmware`

For each sub-area:

1. **DTOs (request/response)**
   - Mirror the JSON payloads defined in `CONTROL_CENTER_BACKEND_APIS.md`.
   - Keep DTOs in `DTOs` subfolders or next to commands/queries, following existing patterns.

2. **Commands/Queries**
   - Examples:
     - Instances:
       - `RegisterInstanceCommand : IRequest<InstanceRegisteredDto>`
       - `UpdateInstanceStatusCommand : IRequest<Unit>`
       - `GetInstanceByIdQuery : IRequest<InstanceDto>`
     - Devices:
       - `CreateDeviceCommand : IRequest<DeviceDto>`
       - `GetDevicesQuery : IRequest<PagedResult<DeviceDto>>`
       - `UpdateDeviceCommand : IRequest<DeviceDto>`
       - `DeleteDeviceCommand : IRequest<Unit>`
     - Jobs:
       - `CreateJobTemplateCommand`, `GetJobTemplatesQuery`, `UpdateJobTemplateCommand`
       - `CreateJobCommand`, `GetJobsQuery`, `UpdateJobStatusCommand`
     - Commands:
       - `CreateCommandCommand`, `GetCommandByIdQuery`
       - `GetPendingCommandsForInstanceQuery`
       - `SubmitCommandResultCommand`
     - Telemetry & Audit:
       - `IngestTelemetryBatchCommand`
       - `IngestAuditBatchCommand`
   - Enforce Org/Site scoping based on current user context (see Step 6).

3. **Handlers**
   - Under `Handlers` subfolders, implement handlers that:
     - Inject `IApplicationDbContext`.
     - Use `CancellationToken`.
     - Filter by `OrgId`/`SiteId` (multi-tenancy).
     - Apply business rules from the spec (status transitions, constraints).

4. **Validators**
   - For each write command, add a FluentValidation validator in `Validators` subfolders.
   - Reuse existing validation patterns (e.g. from `Auth` and other features) for style.

## Step 5 – API controllers for Control Center

Under `Maba.Api/Controllers/ControlCenter/`, create controllers grouped by subdomain:

- `InstancesController`
- `DevicesController`
- `ModulesController`
- `JobsController`
- `JobTemplatesController`
- `ConfigsController`
- `CommandsController`
- `TelemetryController`
- `AuditController`
- `NotificationsController`
- `PoliciesController`
- `BackupsController`
- `FirmwareController`

Each controller should:

- Be decorated with:
  - `[ApiController]`
  - `[Route("api/v1/control-center/[controller]")]`
  - `[Authorize]` and, where appropriate, `[Authorize(Policy = AuthorizationPolicies.<PolicyName>)]`.
- Use **MediatR**:
  - Inject `IMediator`.
  - In each action, construct the appropriate command/query and call `_mediator.Send(...)`.
- Implement the routes from `CONTROL_CENTER_BACKEND_APIS.md`, adapted to the prefix:
  - Instances:
    - `POST /api/v1/control-center/instances/register`
    - `PATCH /api/v1/control-center/instances/{id}`
    - `GET /api/v1/control-center/instances/{id}`
    - `POST /api/v1/control-center/instances/{instanceId}/heartbeat`
  - Devices:
    - `GET /api/v1/control-center/sites/{siteId}/devices`
    - `POST /api/v1/control-center/sites/{siteId}/devices`
    - `GET /api/v1/control-center/devices/{deviceId}`
    - `PATCH /api/v1/control-center/devices/{deviceId}`
    - `DELETE /api/v1/control-center/devices/{deviceId}`
  - Jobs & Templates:
    - `GET /api/v1/control-center/sites/{siteId}/job-templates`
    - `POST /api/v1/control-center/sites/{siteId}/job-templates`
    - `GET /api/v1/control-center/job-templates/{templateId}`
    - `PATCH /api/v1/control-center/job-templates/{templateId}`
    - `GET /api/v1/control-center/sites/{siteId}/jobs`
    - `POST /api/v1/control-center/sites/{siteId}/jobs`
    - `GET /api/v1/control-center/jobs/{jobId}`
    - `PATCH /api/v1/control-center/jobs/{jobId}`
  - Commands:
    - `POST /api/v1/control-center/commands`
    - `GET /api/v1/control-center/commands/{commandId}`
    - `GET /api/v1/control-center/instances/{instanceId}/commands/pending`
    - `POST /api/v1/control-center/commands/{commandId}/result`
  - Telemetry & Audit:
    - `POST /api/v1/control-center/telemetry/batch`
    - `POST /api/v1/control-center/audit/batch`
    - `GET /api/v1/control-center/audit` (with query params).
  - Other areas (Notifications, Policies, Backups, Firmware) as time permits.

Use proper HTTP response codes consistent with existing controllers:

- `201 Created` for create endpoints (include `Location` header where applicable).
- `200 OK` / `204 NoContent` for updates.
- `404 NotFound` when an entity does not exist **or does not belong to the current user’s org/site**.
- `400 BadRequest` for validation errors (rely on FluentValidation + global exception handling).

Reuse:

- Existing error envelope and exception middleware.
- Rate-limiting policies:
  - Consider stricter policies for commands, backups, etc., using the same `AddRateLimiter` mechanism as in `Program.cs`.

## Step 6 – Auth, multi-tenancy, and RBAC integration

- Use the existing JWT auth configuration:
  - `NameIdentifier` claim (`ClaimTypes.NameIdentifier`) is already used in `AuthController` to get the current `UserId`.
- For Org/Site context:
  - If org/site claims are already present in tokens, read them from `HttpContext.User`.
  - If not yet modeled, introduce:
    - Org/Site identifiers in JWT when you extend auth.
    - Org/Site fields on Control Center entities, even if you start with a single org/site per user.
- Add new permission identifiers to `Maba.Api.Authorization.AuthorizationPolicies` for Control Center:
  - Example constants:
    - `public const string ViewControlCenterDevices = "ViewControlCenterDevices";`
    - `public const string ManageControlCenterDevices = "ManageControlCenterDevices";`
    - `public const string ViewControlCenterJobs = "ViewControlCenterJobs";`
    - `public const string ManageControlCenterJobs = "ManageControlCenterJobs";`
    - `public const string ManageControlCenterPolicies = "ManageControlCenterPolicies";`
  - Map each to a `PermissionRequirement` with a string key consistent with existing ones, e.g.:
    - `"controlcenter.devices.view"`, `"controlcenter.jobs.manage"`, etc.
- Update seed data (if any) so that appropriate roles have these new permissions.
- In controllers:
  - Use `[Authorize(Policy = AuthorizationPolicies.ManageControlCenterDevices)]` etc. on endpoints that require elevated rights.

## Step 7 – Real-time endpoints (if applicable)

The spec mentions WebSocket/SSE endpoints like:

- `/ws/commands` – for commands stream.
- `/ws/events` – for notifications/events.

In this solution, real-time capabilities are already provided via SignalR (see `SupportChatHub` mapped at `/hubs/support-chat`).

Implement Control Center hubs using SignalR:

- Add new hubs under `Maba.Api/Hubs/ControlCenter/`, for example:
  - `ControlCenterCommandsHub` (e.g. `/hubs/control-center/commands`)
  - `ControlCenterEventsHub` (e.g. `/hubs/control-center/events`)
- In `Program.cs`:
  - Map them with:
    - `app.MapHub<ControlCenterCommandsHub>("/hubs/control-center/commands");`
    - `app.MapHub<ControlCenterEventsHub>("/hubs/control-center/events");`
  - Ensure they are protected with JWT bearer tokens (SignalR configuration already supports taking token from `access_token` query param).
- Hub responsibilities:
  - Server → client:
    - `CommandCreated`, `CommandUpdated`, `NotificationCreated`, etc.
  - Client → server:
    - `AckCommand`, `SendCommandResult`, etc.
- Optionally keep REST polling endpoints as fallback (`GET /instances/{instanceId}/commands/pending`) for clients that can’t use WebSockets.

## Step 8 – Tests

Under `Maba.Application.Tests`:

- Add tests for core handlers:
  - `RegisterInstanceCommandHandler`
  - `CreateDeviceCommandHandler`
  - `CreateJobCommandHandler`
  - `CreateCommandCommandHandler`
  - `IngestTelemetryBatchCommandHandler`
- Test scenarios:
  - Org/Site scoping (ensure queries/commands do not leak across tenants).
  - Status transitions (job status, command status).
  - Validation failures (e.g., missing required fields, invalid enums).
- If existing tests use in-memory `ApplicationDbContext` or test fixtures, extend those patterns.

Optionally add integration/API tests (if there is an existing pattern) for:

- Control Center instance registration and retrieval.
- Device creation and listing.
- Command creation and polling.

## Step 9 – Documentation & wiring

- Add a markdown file for Control Center integration, e.g.:
  - `docs/CONTROL_CENTER_BACKEND_INTEGRATION.md`
- Document:
  - Base route prefix: `/api/v1/control-center/...`
  - Auth requirements (JWT bearer, permissions).
  - Example request/response payloads for:
    - Instance registration.
    - Listing devices at a site.
    - Creating a job from a template.
    - Queuing commands and retrieving results.
    - Telemetry and audit batch ingestion.
- Ensure `Swagger`:
  - Is already enabled in development via `Program.cs`.
  - Automatically includes the new controllers and DTOs.

## Focus and priority

If time is limited, prioritize a working vertical slice covering:

1. **Instances**
   - Domain: `ControlCenterInstance`.
   - CQRS:
     - `RegisterInstanceCommand`
     - `UpdateInstanceStatusCommand`
     - `GetInstanceByIdQuery`
   - API:
     - `POST /api/v1/control-center/instances/register`
     - `PATCH /api/v1/control-center/instances/{id}`
     - `GET /api/v1/control-center/instances/{id}`
     - `POST /api/v1/control-center/instances/{id}/heartbeat`

2. **Devices**
   - Domain: `CcDevice`.
   - CQRS:
     - `CreateDeviceCommand`, `GetDevicesQuery`, `GetDeviceByIdQuery`
     - `UpdateDeviceCommand`, `DeleteDeviceCommand`
   - API:
     - `GET /api/v1/control-center/sites/{siteId}/devices`
     - `POST /api/v1/control-center/sites/{siteId}/devices`
     - `GET /api/v1/control-center/devices/{id}`
     - `PATCH /api/v1/control-center/devices/{id}`
     - `DELETE /api/v1/control-center/devices/{id}`

3. **Jobs (templates + jobs)**
   - Domain: `CcJobTemplate`, `CcJob`.
   - CQRS and API endpoints matching `CONTROL_CENTER_BACKEND_APIS.md`.

4. **Commands**
   - Domain: `CcCommand` (+ optional `CcCommandResult`).
   - CQRS and API:
     - `POST /api/v1/control-center/commands`
     - `GET /api/v1/control-center/commands/{id}`
     - `GET /api/v1/control-center/instances/{instanceId}/commands/pending`
     - `POST /api/v1/control-center/commands/{id}/result`

5. **Telemetry & Audit ingestion**
   - Domain: `CcTelemetryRecord`, `CcAuditEvent` (or reuse/extend existing logs).
   - CQRS:
     - `IngestTelemetryBatchCommand`
     - `IngestAuditBatchCommand`
   - API:
     - `POST /api/v1/control-center/telemetry/batch`
     - `POST /api/v1/control-center/audit/batch`

Once this core slice is working end-to-end and covered by tests, you can iteratively add the remaining APIs from `CONTROL_CENTER_BACKEND_APIS.md` (notifications, policies, backups, firmware, webhooks, etc.) following the same patterns.

