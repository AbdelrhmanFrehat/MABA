import type { ModuleManifest } from "@maba/shared";

export const dexterManifest: ModuleManifest = {
  id: "dexter",
  name: "Dexter Robotic Arm",
  icon: "🤖",
  version: "0.1.0",
  minCoreVersion: "0.1.0",
  routes: [
    { id: "dexter-overview", path: "/modules/dexter", label: "Overview" },
    { id: "dexter-macropad", path: "/modules/dexter/macropad", label: "MacroPad" },
    { id: "dexter-devices", path: "/modules/dexter/devices", label: "Dexter Devices" },
    { id: "dexter-jobs", path: "/modules/dexter/jobs", label: "Dexter Jobs" }
  ],
  deviceTypes: ["dexter-arm"],
  capabilities: ["device-control", "job-management"]
};

