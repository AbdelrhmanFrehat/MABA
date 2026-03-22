import { contextBridge } from "electron";

contextBridge.exposeInMainWorld("maba", {
  // IPC APIs will be added later for sync, modules, devices, etc.
});

