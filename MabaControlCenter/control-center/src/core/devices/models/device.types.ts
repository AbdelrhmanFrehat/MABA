export type DeviceType = "DEXTER" | "SCARA" | "CNC";

export type DeviceStatus = "DISCONNECTED" | "CONNECTED" | "RUNNING";

export type DeviceTelemetry = {
  position: { x: number; y: number; z: number };
  temperature: number;
  speed: number;
};

export type DexterCalibrationState =
  | "IDLE"
  | "CALIBRATING"
  | "COMPLETED"
  | "STOPPED";

export type DexterState = {
  calibrationState: DexterCalibrationState;
};

export type Device = {
  id: string;
  name: string;
  type: DeviceType;
  status: DeviceStatus;
  telemetry: DeviceTelemetry;
  isSimulated: boolean;
  lastSeenAt: string | null;
  dexter?: DexterState;
};

export const emptyTelemetry = (): DeviceTelemetry => ({
  position: { x: 0, y: 0, z: 0 },
  temperature: 0,
  speed: 0,
});
