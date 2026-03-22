import React, { useEffect } from "react";
import { Link } from "react-router-dom";
import { useDeviceStore } from "../../core/devices/store/device.store";
import { useDexterModuleStore } from "../../core/modules/dexter/store/dexter.store";
import { MACRO_PAD_BUTTON_IDS } from "../../core/modules/dexter/models/dexter-macropad.types";
import { dexterMacroPadService } from "../../core/modules/dexter/services/dexter.service";
import { useDexterModuleDevice } from "./useDexterModuleDevice";

const btnStyle: React.CSSProperties = {
  padding: "0.35rem 0.65rem",
  borderRadius: "0.375rem",
  border: "1px solid #334155",
  background: "#1e293b",
  color: "#e5e7eb",
  cursor: "pointer",
};

const barStyle: React.CSSProperties = {
  display: "flex",
  flexWrap: "wrap",
  gap: "0.5rem",
  alignItems: "center",
  padding: "0.75rem",
  background: "#0f172a",
  borderRadius: "0.5rem",
  marginBottom: "1rem",
  border: "1px solid #1e293b",
};

export const DexterMacroPadPage: React.FC = () => {
  const {
    canUseDexterModule,
    wrongType,
    activeDevice,
    dexterDevice,
    needsConnect,
  } = useDexterModuleDevice();
  const connectDevice = useDeviceStore((s) => s.connectDevice);
  const startCal = useDeviceStore((s) => s.startDexterCalibration);
  const stopCal = useDeviceStore((s) => s.stopDexterCalibration);
  const resetCal = useDeviceStore((s) => s.resetDexterCalibration);

  const init = useDexterModuleStore((s) => s.initialize);
  const profiles = useDexterModuleStore((s) => s.profiles);
  const activeProfileId = useDexterModuleStore((s) => s.activeProfileId);
  const refreshPorts = useDexterModuleStore((s) => s.refreshPorts);
  const selectedPort = useDexterModuleStore((s) => s.selectedPort);
  const availablePorts = useDexterModuleStore((s) => s.availablePorts);
  const serialConnected = useDexterModuleStore((s) => s.serialConnected);
  const mapping = useDexterModuleStore((s) => s.mapping);
  const listeningButtonId = useDexterModuleStore((s) => s.listeningButtonId);
  const isApplying = useDexterModuleStore((s) => s.isApplying);
  const lastAction = useDexterModuleStore((s) => s.lastAction);
  const setPort = useDexterModuleStore((s) => s.setPort);
  const connectSerial = useDexterModuleStore((s) => s.connect);
  const disconnectSerial = useDexterModuleStore((s) => s.disconnectSerial);
  const load = useDexterModuleStore((s) => s.load);
  const save = useDexterModuleStore((s) => s.save);
  const apply = useDexterModuleStore((s) => s.apply);
  const defaults = useDexterModuleStore((s) => s.defaults);
  const autoMap = useDexterModuleStore((s) => s.autoMap);
  const setMapping = useDexterModuleStore((s) => s.setMapping);
  const startListening = useDexterModuleStore((s) => s.startListening);
  const stopListening = useDexterModuleStore((s) => s.stopListening);
  const createProfile = useDexterModuleStore((s) => s.createProfile);
  const cloneProfile = useDexterModuleStore((s) => s.cloneProfile);
  const deleteProfile = useDexterModuleStore((s) => s.deleteProfile);
  const renameProfile = useDexterModuleStore((s) => s.renameProfile);
  const setActiveProfile = useDexterModuleStore((s) => s.setActiveProfile);
  const importProfile = useDexterModuleStore((s) => s.importProfile);
  const exportProfile = useDexterModuleStore((s) => s.exportProfile);

  useEffect(() => {
    init();
    return () => {
      dexterMacroPadService.stopListening();
    };
  }, [init]);

  const dexterId =
    activeDevice?.type === "DEXTER" ? activeDevice.id : dexterDevice?.id ?? "";
  const cal = activeDevice?.dexter?.calibrationState ?? "IDLE";

  if (wrongType) {
    return (
      <div className="dexter-gate" style={{ maxWidth: "520px" }}>
        <p style={{ color: "#fbbf24" }}>
          Active device is not Dexter. Select Dexter as active on the Devices
          page.
        </p>
        <Link to="/devices">Devices</Link>
      </div>
    );
  }

  if (needsConnect && dexterDevice) {
    return (
      <div className="dexter-gate" style={{ maxWidth: "520px" }}>
        <p>Connect your Dexter device to use MacroPad.</p>
        <button type="button" style={btnStyle} onClick={() => connectDevice(dexterDevice.id)}>
          Connect Dexter
        </button>
        <p style={{ marginTop: "0.75rem" }}>
          <Link to="/devices">Devices</Link>
        </p>
      </div>
    );
  }

  if (!canUseDexterModule || !dexterId) {
    return (
      <div className="dexter-gate">
        <p>No Dexter device available.</p>
        <Link to="/devices">Devices</Link>
      </div>
    );
  }

  const enabled = canUseDexterModule;
  const activeProfile = profiles.find((p) => p.id === activeProfileId) ?? null;
  const isBuiltin = activeProfile?.source === "builtin";
  const canDeleteProfile = !!activeProfileId && !isBuiltin && profiles.length > 1;

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>MacroPad configurator</h2>
      <p style={{ color: "#94a3b8", fontSize: "0.875rem" }}>
        {activeDevice?.name} · {lastAction}
      </p>

      <div style={barStyle}>
        <span>Profile</span>
        <select
          value={activeProfileId ?? ""}
          onChange={(e) => {
            const id = e.target.value;
            if (id) setActiveProfile(id);
          }}
          style={{
            padding: "0.35rem",
            background: "#111827",
            color: "#e5e7eb",
            border: "1px solid #334155",
            borderRadius: "0.375rem",
          }}
        >
          {profiles.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>
        <button
          type="button"
          style={btnStyle}
          onClick={() => {
            const name = window.prompt("Profile name", "New profile");
            if (name) createProfile(name, false);
          }}
        >
          + New
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!activeProfileId}
          onClick={() => {
            if (!activeProfileId || !activeProfile) return;
            const name = window.prompt(
              "Clone profile as",
              `${activeProfile.name} copy`
            );
            if (name) cloneProfile(activeProfileId, name);
          }}
        >
          Clone
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!activeProfileId}
          onClick={() => {
            if (!activeProfileId) return;
            const current = profiles.find((p) => p.id === activeProfileId);
            const name = window.prompt(
              "Rename profile",
              current?.name ?? "Profile"
            );
            if (name) renameProfile(activeProfileId, name);
          }}
        >
          Rename
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!canDeleteProfile}
          onClick={() => {
            if (!activeProfileId || !canDeleteProfile) return;
            if (
              window.confirm("Delete current profile? This cannot be undone.")
            ) {
              deleteProfile(activeProfileId);
            }
          }}
        >
          Delete
        </button>
        <button
          type="button"
          style={btnStyle}
          onClick={() => {
            const raw = window.prompt(
              "Paste profile JSON to import",
              '{\n  "name": "Imported profile",\n  "category": "Custom",\n  "mapping": {\n    "a": "ctrl+c"\n  }\n}'
            );
            if (!raw) return;
            importProfile(raw);
          }}
        >
          Import
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!activeProfileId}
          onClick={async () => {
            if (!activeProfileId) return;
            const json = exportProfile(activeProfileId);
            if (!json) return;
            try {
              if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(json);
                window.alert("Profile JSON copied to clipboard.");
              } else {
                window.alert(json);
              }
            } catch {
              window.alert(json);
            }
          }}
        >
          Export
        </button>
        {activeProfile && (
          <span
            style={{
              marginLeft: "0.75rem",
              padding: "0.1rem 0.4rem",
              borderRadius: "999px",
              border: "1px solid #334155",
              fontSize: "0.7rem",
              textTransform: "uppercase",
              color:
                activeProfile.source === "builtin"
                  ? "#22c55e"
                  : activeProfile.source === "market"
                  ? "#38bdf8"
                  : "#e5e7eb",
            }}
          >
            {activeProfile.source}
          </span>
        )}
        {activeDevice && activeDevice.type === "DEXTER" && (
          <span style={{ marginLeft: "0.75rem", color: "#94a3b8" }}>
            Device type: {activeDevice.type}
          </span>
        )}
      </div>

      <div style={barStyle}>
        <label style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
          <span>Port</span>
          <select
            value={selectedPort ?? ""}
            onChange={(e) => setPort(e.target.value || null)}
            disabled={!enabled || serialConnected}
            style={{
              padding: "0.35rem",
              background: "#111827",
              color: "#e5e7eb",
              border: "1px solid #334155",
              borderRadius: "0.375rem",
            }}
          >
            <option value="">—</option>
            {availablePorts.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </label>
        <button
          type="button"
          style={btnStyle}
          disabled={!enabled || !selectedPort || serialConnected}
          onClick={() => selectedPort && connectSerial(selectedPort)}
        >
          Connect
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!serialConnected}
          onClick={() => disconnectSerial()}
        >
          Disconnect
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!enabled}
          onClick={() => refreshPorts()}
        >
          Refresh
        </button>
        <span style={{ color: serialConnected ? "#22c55e" : "#64748b" }}>
          Serial: {serialConnected ? "open" : "closed"}
        </span>
      </div>

      <div style={barStyle}>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => load()}>
          Load
        </button>
        <button
          type="button"
          style={btnStyle}
          disabled={!enabled || isApplying}
          onClick={() => {
            void apply().catch(() => {});
          }}
        >
          Apply
        </button>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => save()}>
          Save
        </button>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => defaults()}>
          Defaults
        </button>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => autoMap()}>
          Auto A→P
        </button>
        <span style={{ marginLeft: "0.5rem", color: "#94a3b8" }}>Layout</span>
        <button
          type="button"
          style={btnStyle}
          disabled={!enabled || cal === "CALIBRATING"}
          onClick={() => startCal(dexterId)}
        >
          Start
        </button>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => stopCal(dexterId)}>
          Stop
        </button>
        <button type="button" style={btnStyle} disabled={!enabled} onClick={() => resetCal(dexterId)}>
          Reset
        </button>
        <span style={{ color: "#94a3b8" }}>Cal: {cal}</span>
      </div>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(4, minmax(0, 1fr))",
          gap: "0.75rem",
          maxWidth: "720px",
        }}
      >
        {MACRO_PAD_BUTTON_IDS.map((id) => (
          <div
            key={id}
            style={{
              padding: "0.75rem",
              background: "#0f172a",
              borderRadius: "0.5rem",
              border:
                listeningButtonId === id
                  ? "1px solid #38bdf8"
                  : "1px solid #1e293b",
            }}
          >
            <div style={{ fontWeight: 600, marginBottom: "0.5rem" }}>
              Key {id.toUpperCase()}
            </div>
            <input
              type="text"
              value={mapping[id]}
              onChange={(e) => setMapping(id, e.target.value)}
              disabled={!enabled}
              placeholder="Mapping"
              style={{
                width: "100%",
                boxSizing: "border-box",
                marginBottom: "0.5rem",
                padding: "0.35rem",
                background: "#111827",
                border: "1px solid #334155",
                color: "#e5e7eb",
                borderRadius: "0.375rem",
              }}
            />
            <button
              type="button"
              style={btnStyle}
              disabled={!enabled}
              onClick={() =>
                listeningButtonId === id ? stopListening() : startListening(id)
              }
            >
              {listeningButtonId === id ? "Cancel" : "Listen"}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};
