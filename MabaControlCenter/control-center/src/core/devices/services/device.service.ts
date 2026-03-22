import type {
  Device,
  DeviceStatus,
  DeviceTelemetry,
  DexterCalibrationState,
} from "../models/device.types";
import { emptyTelemetry } from "../models/device.types";
import { startSimulation, stopSimulation } from "./simulation.engine";

type Listener = () => void;

const defaultDexterCalibration = (): DexterCalibrationState => "IDLE";

function seedDevices(): Device[] {
  const now = null as string | null;
  return [
    {
      id: "dev-dexter-1",
      name: "Dexter Arm #1",
      type: "DEXTER",
      status: "DISCONNECTED",
      telemetry: emptyTelemetry(),
      isSimulated: true,
      lastSeenAt: now,
      dexter: { calibrationState: defaultDexterCalibration() },
    },
    {
      id: "dev-scara-1",
      name: "MABA SCARA #1",
      type: "SCARA",
      status: "DISCONNECTED",
      telemetry: emptyTelemetry(),
      isSimulated: true,
      lastSeenAt: now,
    },
    {
      id: "dev-cnc-1",
      name: "CNC Workcell #1",
      type: "CNC",
      status: "DISCONNECTED",
      telemetry: emptyTelemetry(),
      isSimulated: true,
      lastSeenAt: now,
    },
  ];
}

class DeviceService {
  private devices: Device[] = seedDevices();
  private activeDeviceId: string | null = null;
  private listeners = new Set<Listener>();
  private calibrationTimers = new Map<string, ReturnType<typeof setTimeout>>();

  subscribe(listener: Listener): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  private emit(): void {
    this.listeners.forEach((l) => l());
  }

  getDevices(): Device[] {
    return this.devices.map((d) => ({ ...d, telemetry: { ...d.telemetry, position: { ...d.telemetry.position } } }));
  }

  getActiveDeviceId(): string | null {
    return this.activeDeviceId;
  }

  getActiveDevice(): Device | null {
    if (!this.activeDeviceId) return null;
    return this.getDevices().find((d) => d.id === this.activeDeviceId) ?? null;
  }

  setActiveDevice(id: string | null): void {
    this.activeDeviceId = id;
    this.emit();
  }

  connectDevice(id: string): void {
    const idx = this.devices.findIndex((d) => d.id === id);
    if (idx < 0) return;
    this.devices[idx] = {
      ...this.devices[idx],
      status: "CONNECTED",
      lastSeenAt: new Date().toISOString(),
    };
    this.activeDeviceId = id;
    startSimulation(id, (telemetry: DeviceTelemetry, status: DeviceStatus) => {
      const i = this.devices.findIndex((d) => d.id === id);
      if (i < 0) return;
      this.devices[i] = {
        ...this.devices[i],
        telemetry: {
          position: { ...telemetry.position },
          temperature: telemetry.temperature,
          speed: telemetry.speed,
        },
        status,
        lastSeenAt: new Date().toISOString(),
      };
      this.emit();
    });
    this.emit();
  }

  disconnectDevice(id: string): void {
    stopSimulation(id);
    const tid = this.calibrationTimers.get(id);
    if (tid !== undefined) {
      clearTimeout(tid);
      this.calibrationTimers.delete(id);
    }
    const idx = this.devices.findIndex((d) => d.id === id);
    if (idx < 0) return;
    this.devices[idx] = {
      ...this.devices[idx],
      status: "DISCONNECTED",
      telemetry: emptyTelemetry(),
      lastSeenAt: null,
    };
    if (this.devices[idx].type === "DEXTER") {
      this.devices[idx] = {
        ...this.devices[idx],
        dexter: { calibrationState: "IDLE" },
      };
    }
    if (this.activeDeviceId === id) {
      this.activeDeviceId = null;
    }
    this.emit();
  }

  updateDevice(id: string, partial: Partial<Device>): void {
    const idx = this.devices.findIndex((d) => d.id === id);
    if (idx < 0) return;
    this.devices[idx] = { ...this.devices[idx], ...partial };
    this.emit();
  }

  connectFirstDisconnected(): string | null {
    const d = this.devices.find((x) => x.status === "DISCONNECTED");
    if (!d) return null;
    this.connectDevice(d.id);
    return d.id;
  }

  startDexterCalibration(deviceId: string): void {
    const d = this.devices.find((x) => x.id === deviceId && x.type === "DEXTER");
    if (!d) return;
    const existing = this.calibrationTimers.get(deviceId);
    if (existing !== undefined) clearTimeout(existing);
    this.updateDexterCalibration(deviceId, "CALIBRATING");
    const tid = setTimeout(() => {
      this.calibrationTimers.delete(deviceId);
      const cur = this.devices.find((x) => x.id === deviceId);
      if (cur?.dexter?.calibrationState === "CALIBRATING") {
        this.updateDexterCalibration(deviceId, "COMPLETED");
      }
    }, 3000);
    this.calibrationTimers.set(deviceId, tid);
  }

  stopDexterCalibration(deviceId: string): void {
    const tid = this.calibrationTimers.get(deviceId);
    if (tid !== undefined) {
      clearTimeout(tid);
      this.calibrationTimers.delete(deviceId);
    }
    this.updateDexterCalibration(deviceId, "STOPPED");
  }

  resetDexterCalibration(deviceId: string): void {
    const tid = this.calibrationTimers.get(deviceId);
    if (tid !== undefined) {
      clearTimeout(tid);
      this.calibrationTimers.delete(deviceId);
    }
    this.updateDexterCalibration(deviceId, "IDLE");
  }

  private updateDexterCalibration(
    deviceId: string,
    calibrationState: DexterCalibrationState
  ): void {
    const idx = this.devices.findIndex((x) => x.id === deviceId);
    if (idx < 0) return;
    const d = this.devices[idx];
    if (d.type !== "DEXTER") return;
    this.devices[idx] = {
      ...d,
      dexter: { calibrationState },
    };
    this.emit();
  }

  getDexterCalibration(deviceId: string): DexterCalibrationState {
    const d = this.devices.find((x) => x.id === deviceId);
    return d?.dexter?.calibrationState ?? "IDLE";
  }
}

export const deviceService = new DeviceService();
