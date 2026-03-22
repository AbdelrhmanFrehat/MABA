import { MACRO_PAD_BUTTON_IDS } from "../models/dexter-macropad.types";
import type { DexterMapping, DexterProfile } from "../models/dexter.types";

function makeMapping(entries: Partial<DexterMapping>): DexterMapping {
  const base: DexterMapping = MACRO_PAD_BUTTON_IDS.reduce((acc, id) => {
    acc[id] = "";
    return acc;
  }, {} as DexterMapping);
  return { ...base, ...entries };
}

function now() {
  return new Date().toISOString();
}

function builtinProfile(
  id: string,
  name: string,
  category: string,
  mapping: Partial<DexterMapping>,
  description?: string,
  tags?: string[],
  version?: string
): DexterProfile {
  const ts = now();
  return {
    id,
    name,
    category,
    source: "builtin",
    mapping: makeMapping(mapping),
    description,
    tags,
    version,
    createdAt: ts,
    updatedAt: ts,
  };
}

export const builtinProfiles: DexterProfile[] = [
  // 1) VS CODE
  builtinProfile(
    "builtin.vscode",
    "VS Code",
    "IDE",
    {
      a: "ctrl+s", // save
      b: "ctrl+c", // copy
      c: "ctrl+v", // paste
      d: "ctrl+z", // undo
      e: "ctrl+y", // redo
      f: "f5", // run
      g: "ctrl+`", // terminal
      h: "ctrl+/", // comment
      i: "ctrl+f", // find
      j: "ctrl+h", // replace
      k: "ctrl+p", // quick open
      l: "ctrl+shift+p", // command palette
      m: "ctrl+b", // toggle sidebar
      n: "ctrl+shift+o", // go to symbol
      o: "ctrl+tab", // next tab
      p: "ctrl+shift+tab", // previous tab
    },
    "Core VS Code shortcuts for editing, navigation and running.",
    ["code", "editing", "development"],
    "1.0.0"
  ),

  // 2) SOLIDWORKS
  builtinProfile(
    "builtin.solidworks",
    "SOLIDWORKS",
    "CAD",
    {
      a: "ctrl+b", // rebuild
      b: "ctrl+s", // save
      c: "f", // zoom fit
      d: "shift+e", // extrude (example)
      e: "s", // sketch shortcut bar
      f: "ctrl+z", // undo
      g: "ctrl+y", // redo
      h: "space", // view selector
      i: "ctrl+1", // front view
      j: "ctrl+2", // back view
      k: "ctrl+3", // left view
      l: "ctrl+4", // right view
      m: "ctrl+5", // top view
      n: "ctrl+6", // bottom view
      o: "ctrl+8", // normal to
      p: "f5", // rebuild/refresh alternate
    },
    "Common SOLIDWORKS modeling and view commands.",
    ["cad", "solidworks"],
    "1.0.0"
  ),

  // 3) AUTODESK INVENTOR
  builtinProfile(
    "builtin.inventor",
    "Autodesk Inventor",
    "CAD",
    {
      a: "n", // new sketch (example hotkey)
      b: "e", // extrude
      c: "ctrl+s", // save
      d: "ctrl+z", // undo
      e: "ctrl+y", // redo
      f: "shift+f4", // orbit
      g: "f2", // pan
      h: "f3", // zoom
      i: "f4", // rotate/orbit
      j: "f6", // home view
      k: "ctrl+tab", // next document
      l: "ctrl+shift+tab", // previous document
      m: "ctrl+shift+s", // save as
      n: "ctrl+o", // open
      o: "ctrl+shift+n", // new file
      p: "esc", // cancel
    },
    "General-purpose Inventor sketching and view controls.",
    ["cad", "inventor", "3d"],
    "1.0.0"
  ),

  // 4) GENERAL (DEFAULT)
  builtinProfile(
    "builtin.general",
    "General",
    "General",
    {
      a: "ctrl+c", // copy
      b: "ctrl+v", // paste
      c: "ctrl+x", // cut
      d: "printscreen", // screenshot
      e: "win+shift+s", // snip
      f: "volumeup", // volume up
      g: "volumedown", // volume down
      h: "volumemute", // mute
      i: "ctrl+t", // new browser tab
      j: "ctrl+w", // close tab
      k: "alt+tab", // next app
      l: "ctrl+shift+esc", // task manager
      m: "win+d", // show desktop
      n: "ctrl+alt+delete", // security
      o: "win+e", // explorer
      p: "win+r", // run
    },
    "Everyday shortcuts for desktop, volume and browser.",
    ["general", "desktop"],
    "1.0.0"
  ),

  // 5) STREAMING / GAMING
  builtinProfile(
    "builtin.streaming",
    "Streaming / Gaming",
    "Streaming",
    {
      a: "ctrl+shift+m", // mute mic
      b: "ctrl+shift+d", // deafen
      c: "ctrl+shift+s", // start/stop streaming (example)
      d: "ctrl+shift+r", // start/stop recording
      e: "ctrl+shift+c", // switch scene (OBS hotkey)
      f: "ctrl+shift+p", // push-to-talk
      g: "ctrl+shift+u", // toggle overlay
      h: "ctrl+shift+f", // fullscreen
      i: "alt+tab", // next window
      j: "alt+shift+tab", // previous window
      k: "win+g", // Xbox Game Bar
      l: "ctrl+shift+y", // marker
      m: "ctrl+shift+q", // quit/exit app
      n: "ctrl+shift+k", // discord mute
      o: "ctrl+shift+/", // chat command
      p: "ctrl+shift+o", // overlay toggle
    },
    "Streaming and gaming controls for OBS / Discord style setups.",
    ["streaming", "gaming", "obs", "discord"],
    "1.0.0"
  ),
];

