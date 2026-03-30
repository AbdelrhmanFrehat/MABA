import React from "react";
import { useDeviceStore } from "../core/devices/store/device.store";

export const DevicesPage: React.FC = () => {
  const devices = useDeviceStore((s) => s.devices);
  const activeDeviceId = useDeviceStore((s) => s.activeDeviceId);
  const connectDevice = useDeviceStore((s) => s.connectDevice);
  const disconnectDevice = useDeviceStore((s) => s.disconnectDevice);
  const setActiveDevice = useDeviceStore((s) => s.setActiveDevice);

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Devices</h1>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Name</th>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Type</th>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Status</th>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {devices.map((d) => (
            <tr key={d.id}>
              <td style={{ padding: "0.5rem" }}>
                {d.name}
                {activeDeviceId === d.id ? " (active)" : ""}
              </td>
              <td style={{ padding: "0.5rem" }}>{d.type}</td>
              <td style={{ padding: "0.5rem" }}>{d.status}</td>
              <td style={{ padding: "0.5rem" }}>
                {d.status === "DISCONNECTED" ? (
                  <button type="button" onClick={() => connectDevice(d.id)}>
                    Connect
                  </button>
                ) : (
                  <button type="button" onClick={() => disconnectDevice(d.id)}>
                    Disconnect
                  </button>
                )}
                {d.status !== "DISCONNECTED" ? (
                  <button
                    type="button"
                    style={{ marginLeft: "0.5rem" }}
                    onClick={() => setActiveDevice(d.id)}
                  >
                    Set active
                  </button>
                ) : null}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
