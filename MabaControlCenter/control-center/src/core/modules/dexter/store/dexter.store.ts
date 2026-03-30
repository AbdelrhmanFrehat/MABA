import { create } from "zustand";
import {
  MACRO_PAD_BUTTON_IDS,
  type MacroPadButtonId,
} from "../models/dexter-macropad.types";
import {
  type DexterProfile,
  type DexterMapping,
} from "../models/dexter.types";
import { dexterMacroPadService } from "../services/dexter.service";
import { builtinProfiles } from "../profiles/builtinProfiles";

const STORAGE_KEY = "maba.dexter.profiles";

type DexterModuleStore = {
  selectedPort: string | null;
  mapping: Record<MacroPadButtonId, string>;
  listeningButtonId: MacroPadButtonId | null;
  isApplying: boolean;
  lastAction: string;
  serialConnected: boolean;
  availablePorts: string[];
  profiles: DexterProfile[];
  activeProfileId: string | null;
  builtinProfilesLoaded: boolean;
  initialized: boolean;
  initialize: () => void;
  refreshPorts: () => void;
  setPort: (port: string | null) => void;
  setMapping: (id: MacroPadButtonId, value: string) => void;
  startListening: (id: MacroPadButtonId) => void;
  stopListening: () => void;
  connect: (port: string) => void;
  disconnectSerial: () => void;
  createProfile: (name: string, cloneFromActive?: boolean) => void;
  deleteProfile: (id: string) => void;
  renameProfile: (id: string, name: string) => void;
  setActiveProfile: (id: string) => void;
  saveActiveProfile: () => void;
  loadProfile: (id: string) => void;
  loadAllProfiles: () => void;
  load: () => void;
  save: () => void;
  cloneProfile: (id: string, name?: string) => void;
  importProfile: (rawJson: string) => void;
  exportProfile: (id: string) => string | null;
  apply: () => Promise<void>;
  defaults: () => void;
  autoMap: () => void;
};

let unsub: (() => void) | null = null;

function emptyMapping(): DexterMapping {
  return MACRO_PAD_BUTTON_IDS.reduce((acc, id) => {
    acc[id] = "";
    return acc;
  }, {} as DexterMapping);
}

function createProfile(name: string): DexterProfile {
  const now = new Date().toISOString();
  const id =
    (globalThis.crypto && "randomUUID" in globalThis.crypto
      ? (globalThis.crypto as Crypto).randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`) ??
    `${Date.now()}`;
  return {
    id,
    name,
    category: "Custom",
    source: "user",
    mapping: emptyMapping(),
    createdAt: now,
    updatedAt: now,
  };
}

function persistProfiles(profiles: DexterProfile[], activeProfileId: string | null) {
  const payload = { profiles, activeProfileId };
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
  } catch {
    // ignore
  }
}

function loadProfilesFromStorage():
  | { profiles: DexterProfile[]; activeProfileId: string | null }
  | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as {
      profiles?: Partial<DexterProfile>[];
      activeProfileId?: string | null;
    };
    if (!parsed.profiles || parsed.profiles.length === 0) return null;

    const fixedProfiles: DexterProfile[] = parsed.profiles.map((p, idx) => {
      const now = new Date().toISOString();
      const id =
        p.id ||
        `${Date.now()}-${idx.toString(16)}`;
      return {
        id,
        name: p.name || "Profile",
        category: p.category || "Custom",
        source: p.source ?? "user",
        mapping: {
          ...emptyMapping(),
          ...(p.mapping ?? {}),
        },
        description: p.description,
        tags: p.tags,
        version: p.version,
        createdAt: p.createdAt || now,
        updatedAt: p.updatedAt || now,
      };
    });

    return {
      profiles: fixedProfiles,
      activeProfileId: parsed.activeProfileId ?? fixedProfiles[0].id,
    };
  } catch {
    return null;
  }
}

function syncFromService(set: (p: Partial<DexterModuleStore>) => void) {
  const s = dexterMacroPadService.getSnapshot();
  set({
    selectedPort: s.selectedPort,
    listeningButtonId: s.listeningButtonId,
    isApplying: s.isApplying,
    lastAction: s.lastAction,
    serialConnected: s.serialConnected,
  });
}

export const useDexterModuleStore = create<DexterModuleStore>((set, get) => ({
  selectedPort: null,
  mapping: emptyMapping(),
  listeningButtonId: null,
  isApplying: false,
  lastAction: "",
  serialConnected: false,
  availablePorts: [],
  profiles: [],
  activeProfileId: null,
  builtinProfilesLoaded: false,
  initialized: false,

  initialize: () => {
    if (get().initialized) return;

    // wire serial layer
    unsub = dexterMacroPadService.subscribe(() => {
      syncFromService(set);
    });

    const stored = loadProfilesFromStorage();
    let profiles: DexterProfile[] = [];
    let activeId: string | null = null;

    if (stored) {
      profiles = stored.profiles;
      activeId = stored.activeProfileId;
    }

    // merge built-in profiles (do not overwrite user profiles)
    const existingIds = new Set(profiles.map((p) => p.id));
    const merged: DexterProfile[] = [
      ...profiles,
      ...builtinProfiles.filter((bp) => !existingIds.has(bp.id)),
    ];

    if (merged.length === 0) {
      const profile = createProfile("Default");
      merged.push(profile);
      activeId = profile.id;
    } else if (!activeId || !merged.some((p) => p.id === activeId)) {
      activeId = merged[0].id;
    }

    const active =
      merged.find((p) => p.id === activeId) ?? merged[0] ?? createProfile("Default");

    set({
      availablePorts: dexterMacroPadService.refreshPorts(),
      initialized: true,
      profiles: merged,
      activeProfileId: active.id,
      mapping: { ...active.mapping },
      builtinProfilesLoaded: true,
    });

    // Keep service mapping in sync for apply()
    MACRO_PAD_BUTTON_IDS.forEach((id) => {
      dexterMacroPadService.setMapping(id, active.mapping[id] ?? "");
    });

    syncFromService(set);
  },

  refreshPorts: () => {
    set({ availablePorts: dexterMacroPadService.refreshPorts() });
  },

  setPort: (port) => {
    dexterMacroPadService.setPort(port);
  },

  setMapping: (id, value) => {
    const { profiles, activeProfileId, mapping } = get();
    const nextMapping: DexterMapping = { ...mapping, [id]: value };

    const updatedProfiles = profiles.map((p) =>
      p.id === activeProfileId
        ? {
            ...p,
            mapping: { ...p.mapping, [id]: value },
            updatedAt: new Date().toISOString(),
          }
        : p
    );

    set({ mapping: nextMapping, profiles: updatedProfiles });
    persistProfiles(updatedProfiles, activeProfileId ?? null);

    dexterMacroPadService.setMapping(id, value);
  },

  startListening: (id) => {
    dexterMacroPadService.startListening(id);
  },

  stopListening: () => {
    dexterMacroPadService.stopListening();
  },

  connect: (port) => {
    dexterMacroPadService.connect(port);
  },

  disconnectSerial: () => {
    dexterMacroPadService.disconnect();
  },

  createProfile: (name, cloneFromActive = false) => {
    const { profiles, activeProfileId, mapping } = get();
    const base = cloneFromActive
      ? mapping
      : emptyMapping();
    const profile: DexterProfile = {
      ...createProfile(name || "Profile"),
      mapping: { ...base },
    };
    const nextProfiles = [...profiles, profile];
    set({
      profiles: nextProfiles,
      activeProfileId: profile.id,
      mapping: { ...profile.mapping },
    });
    persistProfiles(nextProfiles, profile.id);
  },

  cloneProfile: (id, name) => {
    const { profiles } = get();
    const base = profiles.find((p) => p.id === id);
    if (!base) return;
    const template = createProfile(name || `${base.name} copy`);
    const cloned: DexterProfile = {
      ...template,
      category: base.category,
      source: "user",
      mapping: { ...base.mapping },
    };
    const nextProfiles = [...profiles, cloned];
    set({
      profiles: nextProfiles,
      activeProfileId: cloned.id,
      mapping: { ...cloned.mapping },
    });
    persistProfiles(nextProfiles, cloned.id);
  },

  deleteProfile: (id) => {
    const { profiles, activeProfileId } = get();
    if (profiles.length <= 1) return;
    const nextProfiles = profiles.filter((p) => p.id !== id);
    const nextActive =
      activeProfileId && activeProfileId !== id
        ? activeProfileId
        : nextProfiles[0]?.id ?? null;
    const active =
      nextProfiles.find((p) => p.id === nextActive) ?? nextProfiles[0];
    set({
      profiles: nextProfiles,
      activeProfileId: active?.id ?? null,
      mapping: active ? { ...active.mapping } : emptyMapping(),
    });
    persistProfiles(nextProfiles, active?.id ?? null);
  },

  renameProfile: (id, name) => {
    const { profiles, activeProfileId, mapping } = get();
    const nextProfiles = profiles.map((p) =>
      p.id === id
        ? { ...p, name: name || p.name, updatedAt: new Date().toISOString() }
        : p
    );
    set({ profiles: nextProfiles });
    persistProfiles(nextProfiles, activeProfileId ?? null);
  },

  setActiveProfile: (id) => {
    const { profiles } = get();
    const profile = profiles.find((p) => p.id === id);
    if (!profile) return;
    set({
      activeProfileId: id,
      mapping: { ...profile.mapping },
    });
    persistProfiles(profiles, id);
    MACRO_PAD_BUTTON_IDS.forEach((btn) => {
      dexterMacroPadService.setMapping(btn, profile.mapping[btn] ?? "");
    });
  },

  saveActiveProfile: () => {
    const { profiles, activeProfileId, mapping } = get();
    const nextProfiles = profiles.map((p) =>
      p.id === activeProfileId
        ? { ...p, mapping: { ...mapping }, updatedAt: new Date().toISOString() }
        : p
    );
    set({ profiles: nextProfiles });
    persistProfiles(nextProfiles, activeProfileId ?? null);
  },

  loadProfile: (id) => {
    const { profiles } = get();
    const profile = profiles.find((p) => p.id === id);
    if (!profile) return;
    set({
      activeProfileId: id,
      mapping: { ...profile.mapping },
    });
    MACRO_PAD_BUTTON_IDS.forEach((btn) => {
      dexterMacroPadService.setMapping(btn, profile.mapping[btn] ?? "");
    });
  },

  loadAllProfiles: () => {
    const stored = loadProfilesFromStorage();
    if (!stored) return;
    const active =
      stored.profiles.find((p) => p.id === stored.activeProfileId) ??
      stored.profiles[0];
    set({
      profiles: stored.profiles,
      activeProfileId: active?.id ?? null,
      mapping: active ? { ...active.mapping } : emptyMapping(),
    });
  },

  load: () => {
    // alias for loadAllProfiles to keep old API
    get().loadAllProfiles();
  },

  save: () => {
    get().saveActiveProfile();
  },

  apply: async () => {
    const { mapping } = get();
    MACRO_PAD_BUTTON_IDS.forEach((id) => {
      dexterMacroPadService.setMapping(id, mapping[id] ?? "");
    });
    await dexterMacroPadService.applyConfig();
  },

  defaults: () => {
    const { profiles, activeProfileId } = get();
    const fresh = emptyMapping();
    const nextProfiles = profiles.map((p) =>
      p.id === activeProfileId
        ? { ...p, mapping: { ...fresh }, updatedAt: new Date().toISOString() }
        : p
    );
    set({
      mapping: fresh,
      profiles: nextProfiles,
    });
    persistProfiles(nextProfiles, activeProfileId ?? null);
  },

  importProfile: (rawJson) => {
    let payload: unknown;
    try {
      payload = JSON.parse(rawJson);
    } catch {
      return;
    }

    if (!payload || typeof payload !== "object") return;
    const obj = payload as {
      name?: string;
      category?: string;
      mapping?: Record<string, string>;
      description?: string;
      tags?: string[];
      version?: string;
    };

    const { profiles } = get();
    const base = createProfile(obj.name || "Imported profile");
    const mapped: DexterMapping = emptyMapping();
    if (obj.mapping) {
      for (const [key, value] of Object.entries(obj.mapping)) {
        if (!value) continue;
        if (MACRO_PAD_BUTTON_IDS.includes(key as MacroPadButtonId)) {
          mapped[key as MacroPadButtonId] = value;
        }
      }
    }

    const imported: DexterProfile = {
      ...base,
      category: obj.category || "Imported",
      source: "market",
      mapping: mapped,
      description: obj.description,
      tags: obj.tags,
      version: obj.version,
    };

    const nextProfiles = [...profiles, imported];
    set({
      profiles: nextProfiles,
      activeProfileId: imported.id,
      mapping: { ...imported.mapping },
    });
    persistProfiles(nextProfiles, imported.id);
  },

  exportProfile: (id) => {
    const { profiles } = get();
    const profile = profiles.find((p) => p.id === id);
    if (!profile) return null;

    const payload = {
      name: profile.name,
      category: profile.category,
      mapping: profile.mapping,
      description: profile.description,
      tags: profile.tags,
      version: profile.version,
    };

    try {
      return JSON.stringify(payload, null, 2);
    } catch {
      return null;
    }
  },

  autoMap: () => {
    const { profiles, activeProfileId } = get();
    const mapped = MACRO_PAD_BUTTON_IDS.reduce((acc, id) => {
      acc[id] = id;
      return acc;
    }, {} as DexterMapping);
    const nextProfiles = profiles.map((p) =>
      p.id === activeProfileId
        ? { ...p, mapping: { ...mapped }, updatedAt: new Date().toISOString() }
        : p
    );
    set({
      mapping: mapped,
      profiles: nextProfiles,
    });
    persistProfiles(nextProfiles, activeProfileId ?? null);
  },
}));

export function teardownDexterModuleStore(): void {
  unsub?.();
  unsub = null;
}
