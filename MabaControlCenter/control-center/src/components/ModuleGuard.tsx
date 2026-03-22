import React from "react";
import { Link } from "react-router-dom";
import { useDeviceStore } from "../core/devices/store/device.store";

export const ModuleGuard: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const activeDeviceId = useDeviceStore((s) => s.activeDeviceId);
  const devices = useDeviceStore((s) => s.devices);
  const active =
    activeDeviceId != null
      ? devices.find(
          (d) =>
            d.id === activeDeviceId &&
            (d.status === "CONNECTED" || d.status === "RUNNING")
        ) ?? null
      : null;

  if (!active) {
    return (
      <div className="module-guard">
        <p>
          No device connected. Connect a device from the Dashboard or Devices
          page.
        </p>
        <Link to="/">Dashboard</Link>
        {" · "}
        <Link to="/devices">Devices</Link>
      </div>
    );
  }

  return <>{children}</>;
};
