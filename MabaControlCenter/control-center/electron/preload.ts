import { contextBridge } from "electron/renderer";

contextBridge.exposeInMainWorld("maba", {
  // IPC APIs will be added later for sync, modules, devices, etc.
});

