import path from "path";
import { EventEmitter } from "events";

type ModuleManifest = {
  id: string;
  name: string;
  icon?: string;
  version: string;
  minCoreVersion: string;
  routes: Array<{ id: string; path: string; label: string }>;
  deviceTypes: string[];
  capabilities: string[];
};

type ModuleRegistration = {
  manifest: ModuleManifest;
  createRootComponent: () => Promise<unknown>;
};

type ModuleRecord = {
  id: string;
  manifest: ModuleManifest;
  registration: ModuleRegistration;
};

export class ModuleRegistry extends EventEmitter {
  private modules = new Map<string, ModuleRecord>();

  async loadBuiltInModules() {
    const dexterModule = await import(
      path.join(__dirname, "../renderer/modules/dexter")
    );
    if (dexterModule && dexterModule.registerModule) {
      const registration = dexterModule.registerModule({
        log: (message: string, meta?: Record<string, unknown>) => {
          // placeholder logging
          console.log("[dexter]", message, meta);
        }
      });
      this.modules.set(registration.manifest.id, {
        id: registration.manifest.id,
        manifest: registration.manifest,
        registration
      });
    }
  }

  listManifests(): ModuleManifest[] {
    return Array.from(this.modules.values()).map((m) => m.manifest);
  }

  getRegistration(id: string): ModuleRegistration | undefined {
    return this.modules.get(id)?.registration;
  }
}

