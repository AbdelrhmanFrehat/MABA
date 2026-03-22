# Maba Website – Full Overview (for ChatGPT)

Use this document to give ChatGPT (or any AI) full context about the Maba project so it can help with code, features, or architecture.

---

## What Maba Is

**Maba** (branded “MABA (BETA)” in the UI) is a **full‑stack B2B/B2C web application** that combines:

- **E‑commerce / product catalog** (shop, cart, checkout, orders)
- **Service request flows** (3D print, laser engraving, CNC, design/CAD requests)
- **Projects** (project requests and management)
- **Software product catalog** (software listings and details)
- **Admin back office** for catalog, orders, users, CMS, finance, support chat, and more
- **Multi-language** (English and Arabic, RTL support)
- **Real-time support chat** (SignalR)

So it’s one website with a **public storefront** and an **admin panel**, plus several “capability” areas (3D print, laser, CNC, design, projects, software).

---

## Tech Stack

### Backend (API)

- **.NET 8** ASP.NET Core Web API
- **SQL Server** via Entity Framework Core 8
- **MediatR** (CQRS-style: commands, queries, handlers)
- **FluentValidation** for request validation
- **JWT** for authentication (Bearer tokens, refresh tokens)
- **Serilog** for logging
- **Swagger** for API docs
- **Rate limiting** (built-in .NET 8): global per-IP limit + stricter limit on auth endpoints (e.g. login/register)
- **SignalR** for real-time hubs (e.g. support chat at `/hubs/support-chat`)
- **Background workers** (e.g. slicing jobs, print jobs)
- **Layered structure:** `Maba.Api` (controllers) → `Maba.Application` (handlers, DTOs, validators) → `Maba.Infrastructure` (EF, DB, file storage, workers) → `Maba.Domain` (entities)

API base path: **`/api/v1`**. Controllers use `[Route("api/v1/[controller]")]` (e.g. `AuthController` → `/api/v1/auth`).

### Frontend (UI)

- **Angular 20** (standalone components, lazy-loaded routes)
- **PrimeNG 20** (UI components: tables, dialogs, forms, toast, etc.)
- **PrimeUX (Aura)** for theming (purple/blue primary)
- **Tailwind CSS** + **tailwindcss-primeui**
- **ngx-translate** for i18n (EN/AR, assets in `src/assets/i18n/`)
- **RxJS** for async/HTTP
- **SignalR client** (`@microsoft/signalr`) for real-time chat
- **HTTP client** with:
  - **Auth interceptor** (adds Bearer token, handles 401 and redirect to login)
  - **Error interceptor** (global error toasts via `ErrorHandlerService` + PrimeNG `MessageService`)
- **Guards:** `authGuard`, `loginGuard`, `adminGuard`, `managerGuard`, `storeOwnerGuard` for route protection and roles

Default API URL in dev: **`http://localhost:5000/api/v1`** (see `UI/src/environments/environment.ts`).

---

## High-Level Architecture

- **Public site:** Everything under the **public layout** (header, footer, beta banner). No auth required for browsing catalog, services, about, contact, etc.
- **Admin site:** Everything under **`/admin`**, protected by **`storeOwnerGuard`** (Admin + StoreOwner only). Uses a different layout (sidebar/menu).
- **Auth:** Login/register on public side; JWT stored (e.g. `auth_token`, `refresh_token`, `auth_user`). Roles include Admin, Manager, StoreOwner, User, Viewer, etc., with permission-based checks where needed.

---

## Main Public Routes (UI)

- **`/`** – Home
- **`/auth/*`** – Login, register, forgot-password, reset-password
- **`/catalog`** – Product catalog (list, filters, sort)
- **`/item/:id`** – Product detail
- **`/cart`** – Shopping cart
- **`/checkout`** – Checkout (auth required)
- **`/3d-print`** – 3D print service (landing + request form)
- **`/laser-engraving`** – Laser engraving
- **`/cnc`** – CNC services
- **`/software`** – Software catalog
- **`/projects`** – Projects (request/project flows)
- **`/design`** – Design requests
- **`/design-cad`** – Design CAD requests
- **`/account/*`** – Profile, orders, addresses, payment methods, designs (auth required)
- **`/chat`** – Support chat (auth required)
- **`/contact`** – Contact form
- **`/wishlist`**, **`/compare`**, **`/search`**
- **`/about`**, **`/faq`**
- **`/privacy-policy`**, **`/terms-of-service`**, **`/project-terms`**, **`/confidentiality`**, **`/service-sla`** – Legal/static pages
- **`/help`**, **`/laser-services`** – CMS-driven pages by slug

---

## Main Admin Routes (UI)

All under **`/admin`**, with **storeOwnerGuard** or **authGuard** (and sometimes role-specific guards):

- **`/admin`** – Dashboard
- **Users, roles, permissions** – User and role management
- **Media** – Media library
- **Categories, tags, brands** – Catalog taxonomy
- **Items** – Product CRUD, inventory
- **Machines, machine-parts, item-machine-links** – Equipment and links to items
- **Orders** – Order list and detail
- **3d-requests, design-requests, cad-requests** – Service request lists
- **Materials, laser-materials, print-quality-profiles** – Materials and print settings
- **Laser-requests** – Laser service requests
- **Reviews** – Product/customer reviews
- **Finance** – Finance dashboard
- **CMS** – Pages and content management
- **Hero-ticker** – Home page hero strip content
- **FAQ** – FAQ management (storeOwner)
- **Support-chat** – Support chat (storeOwner)
- **Software** – Software products (admin)
- **Requests** – Unified requests view
- **CNC:** cnc-materials, cnc-requests, cnc-settings
- **Projects, project-requests** – Project and request lists

---

## Main API Surface (Backend)

- **Auth:** `POST /api/v1/auth/register`, `login`, `refresh-token`, `forgot-password`, etc.; `GET /api/v1/auth/me` (current user). Auth endpoints use stricter rate limiting.
- **Home:** `GET /api/v1/home` – Returns the published “home” page (same data as `GET /api/v1/pages/key/home`).
- **Pages (CMS):** `GET /api/v1/pages`, `GET /api/v1/pages/key/{key}`, `GET /api/v1/pages/slug/{slug}`, `GET /api/v1/pages/{id}`, CRUD, publish/unpublish, sections.
- **Catalog/Items:** Items, categories, brands, tags (CRUD and listing).
- **Cart:** Cart and checkout-related endpoints (e.g. add to cart, apply coupon).
- **Orders:** Order creation and listing.
- **Design / CAD / 3D / Laser / CNC:** Controllers and handlers for each service type and their requests.
- **Software:** Software products and releases.
- **Projects:** Projects and project requests.
- **Users, roles, permissions** – Admin APIs for user/role management.
- **Media** – Upload and serve media.
- **Support:** Support conversations and SignalR hub at **`/hubs/support-chat`**.
- **Other:** Dashboard, hero ticker, FAQ, reviews, coupons, notifications, AI chat sessions, etc.

API uses **JWT in Authorization header** (and for SignalR, token can be passed in query string). **Rate limiting** returns **429** when limits are exceeded (global and auth policies).

---

## Important Conventions

- **API versioning:** All under `/api/v1`.
- **Auth:** JWT; 401 triggers redirect to login on protected routes; public routes can be browsed without login.
- **Errors:** Global HTTP error interceptor shows toasts (PrimeNG); 429, 4xx, 5xx are handled centrally.
- **i18n:** Keys in `assets/i18n/en.json` and `ar.json`; direction (RTL) for Arabic.
- **Roles:** Admin, Manager, StoreOwner, User, Viewer, etc.; permission-based checks in API and guards in UI.

---

## Repo / Folders (Summary)

- **`API/`** – .NET solution: `Maba.Api` (host, controllers, middleware), `Maba.Application` (CQRS, DTOs, validators), `Maba.Infrastructure` (EF, DB, workers, file storage), `Maba.Domain` (entities), `Maba.Application.Tests` (e.g. validator tests).
- **`UI/`** – Angular app: `src/app/` (features, shared, layout, pages), `src/assets/` (i18n, styles), `src/environments/` (e.g. `apiUrl`).

---

## Summary in One Paragraph

Maba is a full-stack web app: Angular 20 + PrimeNG front end and .NET 8 Web API backend with SQL Server, JWT auth, and SignalR. It offers a public storefront (catalog, cart, checkout, 3D print, laser, CNC, design, projects, software) and an admin panel for catalog, orders, users, CMS, finance, and support chat. The API is REST under `/api/v1` with rate limiting and global error handling; the UI uses interceptors for auth and error toasts and guards for protected routes. The project is in beta and supports English and Arabic.

---

*You can copy everything above (or just the sections you need) and paste it into ChatGPT so it has full context about the Maba website.*
