import type { DeviceDriver, DeviceStatus } from "@maba/shared";
import { EventEmitter } from "events";

export type ScaraDiagnostics = {
  homed: boolean;
  limitSwitch: boolean;
  overcurrent: boolean;
};

export type ScaraState = {
  position: { x: number; y: number; z: number; theta: number };
  mode: "IDLE" | "RUNNING" | "ERROR";
  currentProgramId?: string;
  diagnostics: ScaraDiagnostics;
};

export type ScaraCommand =
  | { type: "home" }
  | { type: "moveTo"; x: number; y: number; z: number; theta: number }
  | { type: "runProgram"; programId: string }
  | { type: "stop" };

const emitter = new EventEmitter();
const stateByDevice = new Map<string, ScaraState>();

function getOrCreateState(deviceId: string): ScaraState {
  let s = stateByDevice.get(deviceId);
  if (!s) {
    s = {
      position: { x: 0, y: 0, z: 0, theta: 0 },
      mode: "IDLE",
      diagnostics: { homed: false, limitSwitch: false, overcurrent: false }
    };
    stateByDevice.set(deviceId, s);
  }
  return s;
}

function buildStatus(deviceId: string, state: ScaraState): DeviceStatus<ScaraState> {
  return {
    deviceId,
    state,
    lastUpdated: new Date().toISOString()
  };
}

export const mabaScaraDriver: DeviceDriver<ScaraCommand, ScaraState> = {
  async connect(deviceId) {
    getOrCreateState(deviceId);
  },

  async disconnect(deviceId) {
    emitter.removeAllListeners(deviceId);
  },

  async sendCommand(deviceId, command) {
    const state = getOrCreateState(deviceId);
    if (command.type === "home") {
      state.mode = "RUNNING";
      setTimeout(() => {
        state.mode = "IDLE";
        state.diagnostics.homed = true;
        emitter.emit(deviceId, buildStatus(deviceId, state));
      }, 800);
    } else if (command.type === "moveTo") {
      state.mode = "RUNNING";
      state.position = {
        x: command.x,
        y: command.y,
        z: command.z,
        theta: command.theta
      };
      setTimeout(() => {
        state.mode = "IDLE";
        emitter.emit(deviceId, buildStatus(deviceId, state));
      }, 600);
    } else if (command.type === "runProgram") {
      state.mode = "RUNNING";
      state.currentProgramId = command.programId;
      setTimeout(() => {
        state.mode = "IDLE";
        // randomly toggle diagnostics bit to simulate issue
        state.diagnostics.limitSwitch = Math.random() < 0.2;
        state.diagnostics.overcurrent = Math.random() < 0.1;
        emitter.emit(deviceId, buildStatus(deviceId, state));
      }, 1200);
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
    const listener = (status: DeviceStatus<ScaraState>) => callback(status);
    emitter.on(deviceId, listener);
    void this.getStatus(deviceId).then((s) => emitter.emit(deviceId, s));
    return () => {
      emitter.off(deviceId, listener);
    };
  }
};

