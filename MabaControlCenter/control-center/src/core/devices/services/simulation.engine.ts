import type { DeviceStatus, DeviceTelemetry } from "../models/device.types";

export type SimulationTickCallback = (
  telemetry: DeviceTelemetry,
  status: DeviceStatus
) => void;

const intervals = new Map<string, ReturnType<typeof setInterval>>();

export function startSimulation(
  deviceId: string,
  callback: SimulationTickCallback
): void {
  stopSimulation(deviceId);
  let tick = 0;
  const id = setInterval(() => {
    tick += 1;
    const t = tick / 10;
    const position = {
      x: Math.round(Math.sin(t) * 1000) / 10,
      y: Math.round(Math.cos(t) * 1000) / 10,
      z: Math.round(Math.sin(t / 2) * 200) / 10,
    };
    const temperature = Math.round((36 + Math.sin(t) * 4) * 10) / 10;
    const runningPhase = tick % 8 < 4;
    const speed = runningPhase
      ? Math.round((15 + Math.random() * 75) * 10) / 10
      : 0;
    const status: DeviceStatus = runningPhase ? "RUNNING" : "CONNECTED";
    callback(
      { position, temperature, speed },
      status
    );
  }, 1000);
  intervals.set(deviceId, id);
}

export function stopSimulation(deviceId: string): void {
  const handle = intervals.get(deviceId);
  if (handle !== undefined) {
    clearInterval(handle);
    intervals.delete(deviceId);
  }
}
