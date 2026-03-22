import React, { useEffect } from "react";
import { useDeviceStore } from "../core/devices/store/device.store";

export const AppInitializer: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const initialize = useDeviceStore((s) => s.initialize);

  useEffect(() => {
    initialize();
  }, [initialize]);

  return <>{children}</>;
};
