import { app, BrowserWindow } from "electron";
import * as path from "path";
import { SyncEngine } from "./services/syncEngine";
import { getPrismaClient } from "./db/client";

const isDev = process.env.NODE_ENV === "development";
const syncEngine = new SyncEngine();

async function createWindow() {
  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: {
      preload: path.join(__dirname, "preload.js")
    }
  });

  if (isDev) {
    await win.loadURL("http://localhost:5173");
  } else {
    await win.loadFile(path.join(__dirname, "../renderer/index.html"));
  }

  // Ensure DB is initialized and start sync
  void getPrismaClient().$connect();
  syncEngine.start();
}

app.whenReady().then(() => {
  createWindow();

  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});

