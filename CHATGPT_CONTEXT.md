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
