/**
 * MacroPad + simulated serial layer (legacy WinForms app logic abstracted).
 * Real COM wiring can replace internals without changing the public API.
 */
import {
  MACRO_PAD_BUTTON_IDS,
  type DexterMacroPadConfigV1,
  type MacroPadButtonId,
  type MacroPadMapping,
} from "../models/dexter-macropad.types";

const STORAGE_KEY = "maba-cc-dexter-macropad-v1";

type Listener = () => void;

function emptyMapping(): MacroPadMapping {
  return MACRO_PAD_BUTTON_IDS.reduce((acc, id) => {
    acc[id] = "";
    return acc;
  }, {} as MacroPadMapping);
}

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

class DexterMacroPadService {
  private selectedPort: string | null = null;
  private serialConnected = false;
  private mapping: MacroPadMapping = emptyMapping();
  private listeningButtonId: MacroPadButtonId | null = null;
  private isApplying = false;
  private lastAction = "";
  private listeners = new Set<Listener>();
  private keyHandler: ((e: KeyboardEvent) => void) | null = null;

  subscribe(fn: Listener): () => void {
    this.listeners.add(fn);
    return () => this.listeners.delete(fn);
  }

  private emit(): void {
    this.listeners.forEach((l) => l());
  }

  getSnapshot() {
    return {
      selectedPort: this.selectedPort,
      mapping: { ...this.mapping },
      listeningButtonId: this.listeningButtonId,
      isApplying: this.isApplying,
      lastAction: this.lastAction,
      serialConnected: this.serialConnected,
    };
  }

  /** Simulated port scan (replace with real serial enumeration later). */
  refreshPorts(): string[] {
    return ["COM1", "COM2", "COM3", "COM4", "COM5", "COM6"];
  }

  setPort(port: string | null): void {
    this.selectedPort = port;
    this.lastAction = port ? `Port ${port} selected` : "Port cleared";
    this.emit();
  }

  connect(port: string): void {
    this.selectedPort = port;
    this.serialConnected = true;
    this.lastAction = `Simulated open ${port}`;
    this.emit();
  }

  disconnect(): void {
    this.stopListening();
    this.serialConnected = false;
    this.lastAction = "Serial disconnected (simulated)";
    this.emit();
  }

  loadConfig(): boolean {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        this.lastAction = "No saved config";
        this.emit();
        return false;
      }
      const parsed = JSON.parse(raw) as DexterMacroPadConfigV1;
      if (parsed.version !== 1 || !parsed.mappings) {
        this.lastAction = "Invalid config file";
        this.emit();
        return false;
      }
      const m = emptyMapping();
      for (const id of MACRO_PAD_BUTTON_IDS) {
        m[id] = String(parsed.mappings[id] ?? "");
      }
      this.mapping = m;
      if (parsed.port) this.selectedPort = parsed.port;
      this.lastAction = "Config loaded";
      this.emit();
      return true;
    } catch {
      this.lastAction = "Load failed";
      this.emit();
      return false;
    }
  }

  saveConfig(): void {
    const cfg: DexterMacroPadConfigV1 = {
      version: 1,
      port: this.selectedPort,
      mappings: { ...this.mapping },
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(cfg));
    this.lastAction = "Config saved";
    this.emit();
  }

  applyConfig(): Promise<void> {
    if (!this.serialConnected) {
      this.lastAction = "Apply failed: connect serial first";
      this.emit();
      return Promise.reject(new Error("not_connected"));
    }
    this.isApplying = true;
    this.lastAction = "Applying…";
    this.emit();
    return sleep(400).then(() => {
      this.isApplying = false;
      this.lastAction = "Config applied (simulated)";
      this.emit();
    });
  }

  resetDefaults(): void {
    this.mapping = emptyMapping();
    this.lastAction = "Defaults restored";
    this.emit();
  }

  /** Map button a→"a", b→"b", … p→"p" (keyboard chars). */
  autoMap(): void {
    for (let i = 0; i < MACRO_PAD_BUTTON_IDS.length; i++) {
      const id = MACRO_PAD_BUTTON_IDS[i];
      this.mapping[id] = id;
    }
    this.lastAction = "Auto-mapped a→p";
    this.emit();
  }

  setMapping(buttonId: MacroPadButtonId, value: string): void {
    this.mapping[buttonId] = value;
    this.lastAction = `Key ${buttonId} updated`;
    this.emit();
  }

  startListening(buttonId: MacroPadButtonId): void {
    this.stopListening();
    this.listeningButtonId = buttonId;
    this.lastAction = `Listening for key → ${buttonId}`;
    this.keyHandler = (e: KeyboardEvent) => {
      e.preventDefault();
      e.stopPropagation();
      let v = e.key;
      if (v === " ") v = "Space";
      else if (v.length > 1 && e.code) v = e.code;
      this.mapping[buttonId] = v;
      this.stopListening();
      this.lastAction = `${buttonId} → ${v}`;
      this.emit();
    };
    window.addEventListener("keydown", this.keyHandler, true);
    this.emit();
  }

  stopListening(): void {
    if (this.keyHandler) {
      window.removeEventListener("keydown", this.keyHandler, true);
      this.keyHandler = null;
    }
    this.listeningButtonId = null;
    this.emit();
  }

  /** Layout calibration is owned by device.service; this is a no-op hook. */
  calibrate(): void {
    this.lastAction = "Use Calibrate Layout + device calibration";
    this.emit();
  }
}

export const dexterMacroPadService = new DexterMacroPadService();
