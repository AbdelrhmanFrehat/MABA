import type { ModuleManifest } from "./moduleManifest";

export interface CoreApi {
  log: (message: string, meta?: Record<string, unknown>) => void;
}

export interface ModuleRegistration {
  manifest: ModuleManifest;
  createRootComponent: () => Promise<unknown>;
}

export type ModuleRegisterFn = (core: CoreApi) => ModuleRegistration;

