import React from "react";
import { Link } from "react-router-dom";
import { useDeviceStore } from "../../core/devices/store/device.store";
import type { DeviceStatus } from "../../core/devices/models/device.types";
import { useDexterModuleDevice } from "./useDexterModuleDevice";

function statusColor(status: DeviceStatus): string {
  if (status === "CONNECTED") return "#22c55e";
  if (status === "RUNNING") return "#eab308";
  return "#ef4444";
}

export const DexterMainPage: React.FC = () => {
  const connectDevice = useDeviceStore((s) => s.connectDevice);
  const startCal = useDeviceStore((s) => s.startDexterCalibration);
  const stopCal = useDeviceStore((s) => s.stopDexterCalibration);
  const resetCal = useDeviceStore((s) => s.resetDexterCalibration);

  const {
    wrongType,
    needsConnect,
    dexterDevice,
    canUseDexterModule,
    activeDevice,
  } = useDexterModuleDevice();

  if (wrongType) {
    return (
      <div style={{ maxWidth: "520px" }}>
        <p style={{ color: "#fbbf24" }}>
          Active device is not Dexter. Set a Dexter device as active on Devices.
        </p>
        <Link to="/devices">Devices</Link>
      </div>
    );
  }

  if (needsConnect && dexterDevice) {
    return (
      <div>
        <p>Dexter is disconnected.</p>
        <button type="button" onClick={() => connectDevice(dexterDevice.id)}>
          Connect Dexter
        </button>
        <p style={{ marginTop: "0.75rem" }}>
          <Link to="/devices">Devices</Link> ·{" "}
          <Link to="/modules/dexter/macropad">MacroPad</Link>
        </p>
      </div>
    );
  }

  if (!canUseDexterModule || !activeDevice || activeDevice.type !== "DEXTER") {
    return (
      <div>
        <p>No Dexter device available.</p>
        <Link to="/devices">Devices</Link>
      </div>
    );
  }

  const cal = activeDevice.dexter?.calibrationState ?? "IDLE";

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Dexter control</h2>
      <p>
        <Link to="/modules/dexter/macropad">Open MacroPad configurator →</Link>
      </p>
      <p style={{ color: statusColor(activeDevice.status) }}>
        Device status: {activeDevice.status}
      </p>
      <div style={{ marginTop: "1rem" }}>
        <div>
          Position X: {activeDevice.telemetry.position.x} · Y:{" "}
          {activeDevice.telemetry.position.y} · Z:{" "}
          {activeDevice.telemetry.position.z}
        </div>
        <div>Temperature: {activeDevice.telemetry.temperature} °C</div>
        <div>Speed: {activeDevice.telemetry.speed}</div>
      </div>
      <div style={{ marginTop: "1.5rem" }}>
        <p>Calibration: {cal}</p>
        <button
          type="button"
          onClick={() => startCal(activeDevice.id)}
          disabled={cal === "CALIBRATING"}
        >
          Start calibration
        </button>
        <button
          type="button"
          style={{ marginLeft: "0.5rem" }}
          onClick={() => stopCal(activeDevice.id)}
        >
          Stop
        </button>
        <button
          type="button"
          style={{ marginLeft: "0.5rem" }}
          onClick={() => resetCal(activeDevice.id)}
        >
          Reset
        </button>
      </div>
    </div>
  );
};
