## Architecture overview

The MABA Control Center is structured as a TypeScript monorepo with three main packages:

- `@maba/control-center`: Electron + React desktop client (shell, module host, sync engine).
- `@maba/backend`: Fastify-based backend APIs (auth, licensing, devices, modules, jobs, telemetry).
- `@maba/shared`: Shared domain models and SDK contracts for modules and devices.

The client is offline‑first and uses a local SQLite database (via Prisma) in the Electron main process. A sync engine coordinates data exchange with the backend when connectivity is available.

### Module/plugin system

- Shared SDK types (`@maba/shared`) define `ModuleManifest`, `DeviceDriver`, and `ModuleRegistration` contracts.
- The Control Center hosts product modules such as Dexter and MABA SCARA under `src/modules/*`, each exposing a `RootComponent`, manifest, and simulated device driver.
- A `ModuleRegistry` in the Electron main process is responsible for loading built-in and future external modules and exposing their manifests to the renderer UI.

### Sync engine and offline data flow

- Prisma models in the client track cached identity, licenses, devices, installed modules, jobs/configs, telemetry outbox, and per-domain sync state.
- On startup, the client opens the local database, loads cached session and entitlements, and is immediately usable even without a network connection.
- A `SyncEngine` in the Electron main process periodically (and on demand) will reconcile local state with the backend APIs, sending queued telemetry and receiving updates to jobs, configs, and module metadata.

### Adding new modules

To add a new module:

1. Create a folder under `control-center/src/modules/<module-id>`.
2. Define a `manifest.ts` that satisfies `ModuleManifest`.
3. Implement a simulated (or real) `DeviceDriver` in `driver.ts`.
4. Implement a `RootComponent` React entrypoint that provides the module’s internal navigation and views.
5. Register the module with the `ModuleRegistry` so that its routes and capabilities show up in the shell.
