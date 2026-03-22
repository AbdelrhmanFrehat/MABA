import { describe, it, expect } from "vitest";
import { ModuleRegistry } from "../electron/services/moduleRegistry";

describe("ModuleRegistry", () => {
  it("starts empty and lists no manifests before load", () => {
    const registry = new ModuleRegistry();
    expect(registry.listManifests()).toEqual([]);
  });
});

