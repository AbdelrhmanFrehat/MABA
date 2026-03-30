import React, { useMemo } from "react";
import { useDeviceStore } from "../../core/devices/store/device.store";

export const MabaScaraDiagnosticsPage: React.FC = () => {
  const devices = useDeviceStore((s) => s.devices);
  const scara = useMemo(
    () =>
      devices.find(
        (d) =>
          d.type === "SCARA" &&
          (d.status === "CONNECTED" || d.status === "RUNNING")
      ),
    [devices]
  );

  if (!scara) {
    return (
      <div>
        <h1 style={{ marginTop: 0 }}>MABA SCARA diagnostics</h1>
        <p>Connect a SCARA device to view live telemetry.</p>
      </div>
    );
  }

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>MABA SCARA diagnostics</h1>
      <p>Device: {scara.name}</p>
      <ul>
        <li>
          Position: X {scara.telemetry.position.x}, Y{" "}
          {scara.telemetry.position.y}, Z {scara.telemetry.position.z}
        </li>
        <li>Temperature: {scara.telemetry.temperature} °C</li>
        <li>Speed: {scara.telemetry.speed}</li>
        <li>Link status: {scara.status}</li>
      </ul>
    </div>
  );
};
