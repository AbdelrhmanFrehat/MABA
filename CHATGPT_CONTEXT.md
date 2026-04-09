# CHATGPT Context

## Project Overview

MABA is a hybrid full-stack engineering platform for the main website product in this repository. It combines a public company website, an e-commerce/catalog system, engineering and manufacturing service request workflows, and an internal admin operations dashboard.

The main platform scope in this file is limited to the website system in [UI](C:/Users/PC/Desktop/maba/UI) and [API](C:/Users/PC/Desktop/maba/API). Other repo areas, including `MabaControlCenter`, are out of scope unless explicitly expanded later.

## Frontend Architecture

The frontend is an Angular 20 application built with standalone components and lazy-loaded routes. The main app lives in `UI/`, with bootstrap and app-wide provider setup in `UI/src/main.ts` and `UI/src/app.config.ts`, and root routing in `UI/src/app.routes.ts`.

The UI is split into a public application at `/` and an admin application at `/admin`. The frontend uses PrimeNG for component UI, TailwindCSS for utility styling, `ngx-translate` for EN/AR localization, and RTL-aware theming/layout behavior.

## Backend Architecture

The backend is an ASP.NET Core 8 application using a clean architecture split across `Maba.Api`, `Maba.Application`, `Maba.Domain`, and `Maba.Infrastructure` under `API/`. Startup and HTTP pipeline configuration begin in `API/Maba.Api/Program.cs`.

The backend uses EF Core with SQL Server, MediatR/CQRS, FluentValidation, JWT bearer authentication, SignalR, SMTP email, Serilog logging, rate limiting, and local file storage.

## Core Features

- Public company website and marketing pages
- Product catalog with items, categories, brands, tags, search, wishlist, and compare
- Cart and checkout flows
- User authentication, account management, order tracking, and request tracking
- Engineering service workflows for 3D printing, laser engraving, CNC, design/CAD, and projects
- Software library and download flows
- Admin dashboard for users, roles, permissions, products, media, taxonomy, orders, reviews, service requests, CMS content, support chat, and notifications

## Constraints and Rules

- Do not change the architecture without explicit instruction.
- Preserve the Angular standalone component structure and lazy route organization.
- Use PrimeNG for UI components unless instructed otherwise.
- Maintain EN/AR localization support and RTL compatibility.
- Respect `/api/v1` endpoint conventions.
- Preserve the existing JWT authentication flow, route guards, and HTTP interceptor behavior.
- Do not introduce conflicting architectural patterns, frameworks, or random state-management approaches.

## Known Risks

- Role naming is inconsistent between parts of the frontend and backend.
- Authorization is mixed between direct role checks and permission-based policies.
- File upload and storage handling are not fully unified across all backend flows.
- Sensitive configuration values should be moved to secure storage and removed from committed config where applicable.

## Active Conventions

- Main website platform code is split between `UI/` and `API/`.
- Angular frontend uses standalone components, lazy-loaded route trees, and app-level provider setup in `UI/src/app.config.ts`.
- Public routes are mounted at `/`; admin routes are mounted at `/admin`.
- Backend routes follow the `/api/v1` convention.
- Authentication uses JWT bearer tokens end-to-end.
- Frontend auth behavior depends on guards and HTTP interceptors.
- PrimeNG is the main UI component library in the Angular app.
- Localization is EN/AR with RTL-aware behavior in the frontend.
- Backend uses EF Core with SQL Server and local file storage.

## Important Decisions

- Chose Angular standalone components and lazy route loading to keep the frontend modular without NgModule-heavy structure.
- Chose a clean architecture split (`Maba.Api`, `Maba.Application`, `Maba.Domain`, `Maba.Infrastructure`) to separate HTTP concerns, application logic, domain entities, and infrastructure.
- Chose JWT bearer authentication to support the current API-first frontend/backend integration model.
- Chose MediatR/CQRS in the application layer to separate commands, queries, handlers, and validation flow.
- Chose EF Core with SQL Server as the primary persistence approach for the main platform.
- Chose local file storage for now instead of cloud storage, matching the current backend implementation and deployment shape.

## Change Log

### [2026-04-05]
- Added: Initial `CHATGPT_CONTEXT.md` as a living project context file for the main website platform.
- Added: Project overview, frontend architecture, backend architecture, core features, constraints, risks, active conventions, and important decisions.
- Added: Rules for keeping this file updated incrementally on meaningful future changes.
- Fixed: Added a temporary compatibility endpoint at `API/Maba.Api/Controllers/SalesOrdersController.cs` so the admin sales orders page no longer fails with `404` while the full backend sales module is still incomplete.
- Fixed: Added a compatibility stock overview endpoint at `API/Maba.Api/Controllers/InventoryController.cs` backed by `API/Maba.Application/Features/Inventory/Queries/StockOverviewQueries.cs` so the admin business inventory page can load `/api/v1/inventory/stock` without returning `404`.

### [2026-04-06]
- Fixed: Replaced the remaining hardcoded admin sidebar labels in `UI/src/app/layout/component/app.menu.ts` with translation keys for `menu.storageItems` and `menu.homeStatistics`, and added EN/AR locale values so the sidebar stays bilingual.
- Fixed: Added missing active-locale admin translation keys for users, roles, tags, and `common.createdAt` in `UI/src/assets/i18n/en.json` and `UI/src/assets/i18n/ar.json`, resolving raw `admin.*` text appearing across multiple admin list pages.
- Fixed: Aligned the CRM supplier form and backend supplier commands/DTOs so supplier creation and editing now use the real `SupplierTypeId` contract and persist basic fields such as `TaxNumber` and `Notes`, making supplier records usable for Purchase Orders testing.
- Fixed: Changed the storefront sales-orders fallback in `API/Maba.Api/Controllers/SalesOrdersController.cs` to materialize orders first and map nested line DTOs in memory, resolving the EF Core translation error that caused `GET /api/v1/sales-orders` to return `400`.
- Fixed: Corrected the storefront sales-orders fallback in `API/Maba.Api/Controllers/SalesOrdersController.cs` to use `Item.Sku` instead of the non-existent `Item.SKU`, resolving the backend publish failure.
- Fixed: Merged the missing `admin.dashboard.*` translation keys into the active `admin` locale blocks in `UI/src/assets/i18n/en.json` and `UI/src/assets/i18n/ar.json` so the admin dashboard no longer shows raw translation keys.
- Fixed: Added a temporary compatibility endpoint at `API/Maba.Api/Controllers/PaymentVouchersController.cs` so the admin payment vouchers page can load `/api/v1/payment-vouchers` without returning `404` while the full backend payments module is still incomplete.
- Fixed: Added a temporary compatibility endpoint at `API/Maba.Api/Controllers/AccountsController.cs` so the admin chart of accounts page can load `/api/v1/accounts/tree` without returning `404` while the full backend accounting module is still incomplete.
- Fixed: Added a temporary compatibility endpoint at `API/Maba.Api/Controllers/PriceListsController.cs` so the admin price lists page can load `/api/v1/price-lists` without returning `404` while the full backend pricing module is still incomplete.
- Fixed: Added a temporary compatibility endpoint at `API/Maba.Api/Controllers/PurchaseOrdersController.cs` so the admin purchase orders page can load `/api/v1/purchase-orders` without returning `404` while the full backend purchasing module is still incomplete.
- Added: A real admin purchase orders workspace in `UI/src/app/features/admin/purchasing/orders/purchase-orders-list.component.ts` with a live table, supplier-backed create/edit dialog, order detail dialog, line-item editing, and local fallback behavior while the purchasing backend is still incomplete.
- Fixed: Adjusted the new purchase orders page totals typing and cleaned support-chat template nullability warnings so the Angular build can proceed without the reported `TS2322` error and redundant optional-chain warnings.
- Fixed: Corrected purchase-orders component field initialization order so `localOrders` exists before the preview-number generator runs, resolving the runtime `this.localOrders is not iterable` crash on page load.
- Updated: Extended the CRM customer create/edit flow so admin users can link a customer record to an existing website user account via `UserId`, wiring the backend customer commands/DTOs and the frontend customer dialog to the existing `/api/v1/users` list endpoint.
- Refactored: Replaced the temporary price-lists compatibility endpoint with a lightweight real `PriceListsController` that reads seeded price lists and supports create/update for the admin pricing page.
- Added: Upgraded `UI/src/app/features/admin/pricing/price-lists.component.ts` from a thin placeholder into a working admin table with create/edit dialogs backed by `UI/src/app/features/admin/pricing/price-list-form.component.ts`.
- Fixed: Guarded shared table CSV export in `UI/src/app/shared/components/data-table/data-table.ts` so clicking export with no rows no longer throws the PrimeNG `filter` runtime error.
- Fixed: Reduced unnecessary `GET /api/v1/auth/me` requests by making `UI/src/app/features/public/layout/public-header.component.ts` refresh the current user only when a token exists and no user is already loaded in auth state.
- Fixed: Updated the repo root `global.json` SDK pin from `9.0.309` to `10.0.103` to match the installed local SDK and restore C# Dev Kit project-system startup in VS Code.
- Added: Replaced the scaffolded admin expenses screens with a working list page and a real new-expense form in `UI/src/app/features/admin/expenses/`, aligned to the existing backend `ExpensesController` and `ExpenseCategoriesController`.
- Refactored: Updated `UI/src/app/shared/services/expenses.service.ts` and `UI/src/app/shared/models/expense.model.ts` from the older paged/number-based contract to the current GUID-based finance DTOs used by the backend.
- Added: Implemented a lightweight backend `ReportsController` sales endpoint at `API/Maba.Api/Controllers/ReportsController.cs` based on existing storefront invoices and payments so the admin sales report can load real data.
- Added: Replaced the scaffolded admin sales report screen with a working report page in `UI/src/app/features/admin/reports/sales-report.component.ts`, including date filters, summary totals, and a live invoice-level sales table.
- Fixed: Replaced the temporary empty `API/Maba.Api/Controllers/SalesOrdersController.cs` response with a real fallback projection from existing storefront `Orders`, so purchases made on the website now appear in the admin sales orders list.
- Updated: Marked storefront-origin sales rows in `UI/src/app/features/admin/sales/orders/sales-orders-list.component.ts` and hid ERP-only actions for those rows to avoid misleading edit/approval/invoice buttons on website orders.
- Refactored: Introduced a normalized service-request workflow layer for CAD, design, 3D print, laser, and CNC admin modules, including shared workflow utilities, a reusable status badge component, bilingual EN/AR workflow translations, and `workflowStatus` fields in the relevant backend request DTOs so service requests now read as an engineering job pipeline instead of shop/order statuses.

### [2026-04-07]
- Added: Introduced a real central service-request management hub at `UI/src/app/features/admin/unified-requests/` backed by a new unified backend endpoint in `API/Maba.Api/Controllers/AllRequestsController.cs`, giving admins one screen to filter, open, edit, and manage project, 3D print, design, CAD, laser, and CNC requests.
- Added: Created shared unified request contracts in `API/Maba.Application/Common/ServiceRequests/AdminServiceRequestDtos.cs` and `UI/src/app/shared/models/all-requests.model.ts`, plus `UI/src/app/shared/services/all-requests.service.ts`, to standardize cross-module list/detail/update behavior without removing the existing module-specific pages.
- Updated: Extended the shared service-request workflow mapping to include project requests and reverse mappings for all managed request types, so the central admin screen can safely edit legacy request records while presenting a professional operational workflow.
- Updated: Added `TotalRequests` to the dashboard summary backend/frontend contracts and wired the admin dashboard card to route directly to `/admin/requests`, making the KPI act as an entry point into the new all-requests operations screen.
- Fixed: Relaxed the shared service-request workflow helper inputs for laser and CNC legacy/new status values and added the missing PrimeNG tooltip import on the laser admin list, resolving the Angular compile break introduced by the unified workflow refactor.
- Fixed: Added the missing `admin.serviceWorkflow` and `admin.allRequests` locale keys to the final active `admin` blocks in `UI/src/assets/i18n/en.json` and `UI/src/assets/i18n/ar.json`, resolving raw translation keys across the unified all-requests page and management dialog.
- Fixed: Wired the unified all-requests dialog `visibleChange` event back to the component signal in `UI/src/app/features/admin/unified-requests/`, so PrimeNG's built-in close `X` correctly dismisses the modal instead of leaving the dialog state stuck open.
- Added: Upgraded the public `Request a Project` flow in `UI/src/app/features/public/projects/project-request.component.ts` to support engineering-oriented project intake fields including project type, main domain, required capabilities, project stage, updated budget/timeline choices, improved description guidance, and multiple reference attachments while preserving the existing page shell.
- Updated: Extended the project request backend pipeline across `API/Maba.Domain/Projects/ProjectRequest.cs`, `API/Maba.Application/Features/Projects/`, and `API/Maba.Api/Controllers/ProjectsController.cs` to store and retrieve the new engineering project fields with backward-compatible legacy mappings for older records.
- Added: Introduced JSON-based persistence helpers in `API/Maba.Application/Features/Projects/ProjectRequestSerialization.cs` plus migration `API/Maba.Infrastructure/Migrations/20260407170000_UpgradeProjectRequestsForEngineeringWorkflow.cs` to support multi-select required capabilities and multiple project attachments without breaking existing project request data.
- Updated: Reworked the admin project request management page in `UI/src/app/features/admin/projects/project-requests-list.component.ts` and the unified requests hub in `UI/src/app/features/admin/unified-requests/` so admins can view and edit the new project type, main domain, project stage, required capabilities, budget/timeline, description, and attachments from the existing MABA admin patterns.
- Fixed: Cleaned project-request admin text and status presentation so the updated project intake fields use native translated labels, workflow-oriented status text, and clean fallback values instead of hardcoded or malformed placeholders.
- Fixed: Added the missing `Maba.Application.Features.Projects.DTOs` namespace import to `API/Maba.Application/Features/Projects/Commands/UpdateProjectRequestCommand.cs`, resolving the backend compile break caused by the new project attachment DTO reference.
- Refactored: Moved `ProjectRequestAttachmentDto` into its own file at `API/Maba.Application/Features/Projects/DTOs/ProjectRequestAttachmentDto.cs` so project request commands, DTOs, and handlers all reference a single explicit attachment contract.
- Fixed: Resolved Angular typing regressions in the project request admin screens by removing an unreachable enum fallback in `UI/src/app/features/admin/projects/project-requests-list.component.ts` and normalizing nullable `requiredCapabilities` in `UI/src/app/features/admin/unified-requests/unified-requests-list.component.ts`.
- Fixed: Aligned `API/Maba.Api/Controllers/SalesQuotationsController.cs` with the real `OrderItem` entity by storing quotation service-line descriptions in `OrderItem.MetaJson`, parsing them back for DTO output, and safely converting quotation quantities to integer order quantities during quotation-to-order conversion.
- Fixed: Disabled automatic EF migration execution on production API startup in `API/Maba.Api/appsettings.Production.json` and changed the startup default in `API/Maba.Api/Program.cs` so migration failures no longer take down the deployed API and surface as `502` on login.
- Added: Transformed the project request system into a full engineering workflow system. Added `WorkflowStatus` (10-value string enum: New→Completed/Rejected), assignment fields (`AssignedToName`, `AssignedToUserId`), and internal engineering fields (`Priority`, `TechnicalFeasibility`, `EstimatedCost`, `EstimatedTimeline`, `ComplexityLevel`, `InternalNotes`) to `ProjectRequest` entity and all DTOs/commands.
- Added: New `ProjectRequestActivity` entity for activity logging. Activities are recorded on create, status change, assignment, and internal note update. Exposed via `GET /api/v1/projects/admin/requests/{id}/activities`.
- Added: Migration `20260407190000_AddEngineeringWorkflowToProjectRequests` that adds all new columns and creates the `ProjectRequestActivities` table. Existing `Status` integer values are converted to `WorkflowStatus` strings automatically.
- Added: Workflow transition validation rules enforced in `UpdateProjectRequestCommandHandler`: QuoteSent requires EstimatedCost, Approved requires QuoteSent, InExecution requires Approved, Completed requires InExecution.
- Updated: Admin project requests list (`project-requests-list.component.ts`) upgraded with colored workflow status badges, quick inline status change per table row, new dialog sections for Internal Review and Commercial Evaluation, and an activity log timeline at the bottom of the details dialog.
- Updated: EN and AR translation keys added for all new workflow statuses, priorities, feasibility options, complexity levels, and admin labels under `admin.projectRequests.*`.
