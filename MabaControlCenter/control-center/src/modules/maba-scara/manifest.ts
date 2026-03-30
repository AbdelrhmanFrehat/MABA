import type { ModuleManifest } from "@maba/shared";

export const mabaScaraManifest: ModuleManifest = {
  id: "maba-scara",
  name: "MABA SCARA Robot",
  icon: "🦾",
  version: "0.1.0",
  minCoreVersion: "0.1.0",
  routes: [
    { id: "maba-scara-devices", path: "/modules/maba-scara/devices", label: "SCARA Devices" },
    { id: "maba-scara-diagnostics", path: "/modules/maba-scara/diagnostics", label: "Diagnostics" }
  ],
  deviceTypes: ["maba-scara"],
  capabilities: ["device-control", "diagnostics"]
};

