import { useMemo } from "react";
import { useDeviceStore } from "../../core/devices/store/device.store";
import type { Device } from "../../core/devices/models/device.types";

export type DexterModuleDeviceState = {
  activeDevice: Device | null;
  dexterDevice: Device | null;
  canUseDexterModule: boolean;
  wrongType: boolean;
  needsConnect: boolean;
};

export function useDexterModuleDevice(): DexterModuleDeviceState {
  const devices = useDeviceStore((s) => s.devices);
  const activeDeviceId = useDeviceStore((s) => s.activeDeviceId);

  return useMemo(() => {
    const activeDevice =
      activeDeviceId != null
        ? devices.find((d) => d.id === activeDeviceId) ?? null
        : null;

    const dexterDevice =
      devices.find(
        (d) =>
          d.type === "DEXTER" &&
          (d.status === "CONNECTED" || d.status === "RUNNING")
      ) ??
      devices.find((d) => d.type === "DEXTER") ??
      null;

    const wrongType =
      !!activeDevice &&
      activeDevice.type !== "DEXTER" &&
      (activeDevice.status === "CONNECTED" ||
        activeDevice.status === "RUNNING");

    const needsConnect =
      !activeDevice ||
      activeDevice.type !== "DEXTER" ||
      activeDevice.status === "DISCONNECTED";

    const canUseDexterModule =
      !!activeDevice &&
      activeDevice.type === "DEXTER" &&
      (activeDevice.status === "CONNECTED" ||
        activeDevice.status === "RUNNING");

    return {
      activeDevice,
      dexterDevice,
      canUseDexterModule,
      wrongType,
      needsConnect,
    };
  }, [devices, activeDeviceId]);
}
