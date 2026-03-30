import type {
  MACRO_PAD_BUTTON_IDS,
  MacroPadButtonId,
} from "./dexter-macropad.types";

export type DexterMapping = Record<MacroPadButtonId, string>;

export type ProfileSource = "builtin" | "user" | "market";

export interface DexterProfile {
  id: string;
  name: string;
  category: string;
  source: ProfileSource;
  mapping: DexterMapping;
  description?: string;
  tags?: string[];
  version?: string;
  createdAt: string;
  updatedAt: string;
}

export type DexterProfilesState = {
  profiles: DexterProfile[];
  activeProfileId: string | null;
};

export type MacroPadButton = (typeof MACRO_PAD_BUTTON_IDS)[number];

