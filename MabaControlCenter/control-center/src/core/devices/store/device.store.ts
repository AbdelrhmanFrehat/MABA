import { create } from "zustand";
import type { Device } from "../models/device.types";
import { deviceService } from "../services/device.service";

type DeviceStoreState = {
  devices: Device[];
  activeDeviceId: string | null;
  initialized: boolean;
  initialize: () => void;
  connectDevice: (id: string) => void;
  disconnectDevice: (id: string) => void;
  setActiveDevice: (id: string | null) => void;
  getActiveDevice: () => Device | null;
  connectFirstDisconnected: () => string | null;
  startDexterCalibration: (deviceId: string) => void;
  stopDexterCalibration: (deviceId: string) => void;
  resetDexterCalibration: (deviceId: string) => void;
};

let unsubscribe: (() => void) | null = null;

export const useDeviceStore = create<DeviceStoreState>((set, get) => ({
  devices: [],
  activeDeviceId: null,
  initialized: false,

  initialize: () => {
    if (get().initialized) return;
    unsubscribe = deviceService.subscribe(() => {
      set({
        devices: deviceService.getDevices(),
        activeDeviceId: deviceService.getActiveDeviceId(),
      });
    });
    set({
      devices: deviceService.getDevices(),
      activeDeviceId: deviceService.getActiveDeviceId(),
      initialized: true,
    });
  },

  connectDevice: (id: string) => {
    deviceService.connectDevice(id);
  },

  disconnectDevice: (id: string) => {
    deviceService.disconnectDevice(id);
  },

  setActiveDevice: (id: string | null) => {
    deviceService.setActiveDevice(id);
  },

  getActiveDevice: () => deviceService.getActiveDevice(),

  connectFirstDisconnected: () => deviceService.connectFirstDisconnected(),

  startDexterCalibration: (deviceId: string) => {
    deviceService.startDexterCalibration(deviceId);
  },

  stopDexterCalibration: (deviceId: string) => {
    deviceService.stopDexterCalibration(deviceId);
  },

  resetDexterCalibration: (deviceId: string) => {
    deviceService.resetDexterCalibration(deviceId);
  },
}));

export function teardownDeviceStoreSubscription(): void {
  unsubscribe?.();
  unsubscribe = null;
}
