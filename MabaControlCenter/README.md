## MABA Control Center

This repository contains a greenfield implementation of the **MABA Control Center** platform:

- **Control Center client**: Electron + React desktop shell for Windows (cross‑platform capable).
- **Backend API service**: Node.js + Fastify reference backend for auth, licensing, devices, modules, jobs, and telemetry.
- **Shared library**: TypeScript domain models and SDK contracts reused across client and backend.

See `ARCHITECTURE.md` for a detailed overview of layers, module/plugin system, and sync engine.

### Running in development

- **Backend**: `npm run dev:backend` (Fastify server on port 4000).
- **Control Center client**: `npm run dev:client` (starts Vite dev server and Electron shell).

The client starts in an offline-capable mode, using a local SQLite database for cached users, licenses, devices, modules, jobs, and telemetry, and a sync engine in the Electron main process to talk to the backend when connectivity is available.
