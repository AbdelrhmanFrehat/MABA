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
