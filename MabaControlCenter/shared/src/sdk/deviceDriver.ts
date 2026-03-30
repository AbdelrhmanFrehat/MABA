export type DeviceStatus<State = unknown> = {
  deviceId: string;
  state: State;
  lastUpdated: string;
};

export interface DeviceDriver<Command = unknown, State = unknown> {
  connect(deviceId: string): Promise<void>;
  disconnect(deviceId: string): Promise<void>;
  sendCommand(deviceId: string, command: Command): Promise<void>;
  getStatus(deviceId: string): Promise<DeviceStatus<State>>;
  subscribeStatus(
    deviceId: string,
    callback: (status: DeviceStatus<State>) => void
  ): () => void;
}

