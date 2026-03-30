import React from "react";
import { useDeviceStore } from "../core/devices/store/device.store";
import type { DeviceStatus } from "../core/devices/models/device.types";

function statusColor(status: DeviceStatus): string {
  if (status === "CONNECTED") return "#22c55e";
  if (status === "RUNNING") return "#eab308";
  return "#ef4444";
}

export const DashboardPage: React.FC = () => {
  const devices = useDeviceStore((s) => s.devices);
  const activeDeviceId = useDeviceStore((s) => s.activeDeviceId);
  const active =
    activeDeviceId != null
      ? devices.find((d) => d.id === activeDeviceId) ?? null
      : null;
  const connectFirstDisconnected = useDeviceStore(
    (s) => s.connectFirstDisconnected
  );
  const disconnectDevice = useDeviceStore((s) => s.disconnectDevice);

  const handleConnect = () => {
    connectFirstDisconnected();
  };

  const handleDisconnect = () => {
    if (active) disconnectDevice(active.id);
  };

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Dashboard</h1>
      {!active ? (
        <div>
          <p>No active device.</p>
          <button type="button" onClick={handleConnect}>
            Connect first available device
          </button>
          <p style={{ fontSize: "0.875rem", color: "#9ca3af" }}>
            {devices.every((d) => d.status === "DISCONNECTED")
              ? "Connects the first disconnected device in the registry."
              : null}
          </p>
        </div>
      ) : (
        <div>
          <h2 style={{ marginBottom: "0.5rem" }}>{active.name}</h2>
          <p style={{ color: statusColor(active.status) }}>
            Status: {active.status}
          </p>
          <div style={{ marginTop: "1rem" }}>
            <div>Position X: {active.telemetry.position.x}</div>
            <div>Position Y: {active.telemetry.position.y}</div>
            <div>Position Z: {active.telemetry.position.z}</div>
            <div>Temperature: {active.telemetry.temperature} °C</div>
            <div>Speed: {active.telemetry.speed}</div>
          </div>
          <button
            type="button"
            style={{ marginTop: "1rem" }}
            onClick={handleDisconnect}
          >
            Disconnect active device
          </button>
        </div>
      )}
    </div>
  );
};
