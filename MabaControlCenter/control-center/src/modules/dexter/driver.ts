import type { DeviceDriver, DeviceStatus } from "@maba/shared";
import { EventEmitter } from "events";

export type DexterState = {
  joints: number[];
  mode: "IDLE" | "RUNNING" | "ERROR";
  currentJobId?: string;
};

export type DexterCommand =
  | { type: "moveJoints"; targets: number[] }
  | { type: "runJob"; jobId: string }
  | { type: "stop" };

const emitter = new EventEmitter();
const stateByDevice = new Map<string, DexterState>();

function getOrCreateState(deviceId: string): DexterState {
  let s = stateByDevice.get(deviceId);
  if (!s) {
    s = { joints: [0, 0, 0, 0, 0, 0], mode: "IDLE" };
    stateByDevice.set(deviceId, s);
  }
  return s;
}

export const dexterDriver: DeviceDriver<DexterCommand, DexterState> = {
  async connect(deviceId) {
    getOrCreateState(deviceId);
  },

  async disconnect(deviceId) {
    emitter.removeAllListeners(deviceId);
  },

  async sendCommand(deviceId, command) {
    const state = getOrCreateState(deviceId);
    if (command.type === "moveJoints") {
      state.mode = "RUNNING";
      state.joints = command.targets;
      setTimeout(() => {
        state.mode = "IDLE";
        emitter.emit(deviceId, buildStatus(deviceId, state));
      }, 500);
    } else if (command.type === "runJob") {
      state.mode = "RUNNING";
      state.currentJobId = command.jobId;
      setTimeout(() => {
        state.mode = "IDLE";
        emitter.emit(deviceId, buildStatus(deviceId, state));
      }, 1000);
    } else if (command.type === "stop") {
      state.mode = "IDLE";
      emitter.emit(deviceId, buildStatus(deviceId, state));
    }
  },

  async getStatus(deviceId) {
    const state = getOrCreateState(deviceId);
    return buildStatus(deviceId, state);
  },

  subscribeStatus(deviceId, callback) {
    const listener = (status: DeviceStatus<DexterState>) => callback(status);
    emitter.on(deviceId, listener);
    void this.getStatus(deviceId).then((s) => emitter.emit(deviceId, s));
    return () => {
      emitter.off(deviceId, listener);
    };
  }
};

function buildStatus(deviceId: string, state: DexterState): DeviceStatus<DexterState> {
  return {
    deviceId,
    state,
    lastUpdated: new Date().toISOString()
  };
}

