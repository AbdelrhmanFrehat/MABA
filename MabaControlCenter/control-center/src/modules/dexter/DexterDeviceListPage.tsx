import React from "react";
import { useDeviceStore } from "../../core/devices/store/device.store";

export const DexterDeviceListPage: React.FC = () => {
  const devices = useDeviceStore((s) => s.devices);
  const dexterDevices = devices.filter((d) => d.type === "DEXTER");
  const connectDevice = useDeviceStore((s) => s.connectDevice);
  const disconnectDevice = useDeviceStore((s) => s.disconnectDevice);

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Dexter devices</h1>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Name</th>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Status</th>
            <th style={{ textAlign: "left", padding: "0.5rem" }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {dexterDevices.map((d) => (
            <tr key={d.id}>
              <td style={{ padding: "0.5rem" }}>{d.name}</td>
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
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
