/** MacroPad button ids (legacy MABA-Dexter 1.1 MacroPad Configurator: keys a–p). */
export const MACRO_PAD_BUTTON_IDS = [
  "a",
  "b",
  "c",
  "d",
  "e",
  "f",
  "g",
  "h",
  "i",
  "j",
  "k",
  "l",
  "m",
  "n",
  "o",
  "p",
] as const;

export type MacroPadButtonId = (typeof MACRO_PAD_BUTTON_IDS)[number];

export type MacroPadMapping = Record<MacroPadButtonId, string>;
