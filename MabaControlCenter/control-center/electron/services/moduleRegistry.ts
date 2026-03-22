import path from "path";
import { EventEmitter } from "events";
import type { ModuleManifest, ModuleRegistration } from "@maba/shared";

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
        log: (message, meta) => {
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

