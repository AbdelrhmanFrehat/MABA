using System.Globalization;
using System.Text;

namespace MabaControlCenter.Services;

/// <summary>Port of legacy Python MacroPad shortcut + MAP line logic.</summary>
public static class MacroPadProtocol
{
    public const int BtnCount = 16;
    public static readonly string[] UiOrder = "abcdefghijklmnop".ToCharArray().Select(c => c.ToString()).ToArray();

    public static readonly string[][] LayoutHardwareDefault =
    [
        ["e", "f", "m", "n"],
        ["p", "k", "d", "j"],
        ["h", "i", "b", "l"],
        ["o", "g", "c", "a"]
    ];

    private static readonly HashSet<string> ModCtrl = new(StringComparer.OrdinalIgnoreCase) { "ctrl", "control", "left ctrl", "right ctrl", "ctl", "ct" };
    private static readonly HashSet<string> ModAlt = new(StringComparer.OrdinalIgnoreCase) { "alt", "alt gr", "altgr", "option", "opt", "menu" };
    private static readonly HashSet<string> ModShift = new(StringComparer.OrdinalIgnoreCase) { "shift", "left shift", "right shift", "sh" };
    private static readonly HashSet<string> ModWin = new(StringComparer.OrdinalIgnoreCase) { "win", "windows", "left windows", "right windows", "command", "cmd", "super", "meta" };

    private static readonly Dictionary<string, string> _kbNameToToken = BuildKbMap();

    private static Dictionary<string, string> BuildKbMap()
    {
        var m = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["esc"] = "ESC", ["escape"] = "ESC", ["tab"] = "TAB", ["enter"] = "ENTER", ["return"] = "ENTER",
            ["backspace"] = "BACKSPACE", ["delete"] = "DELETE", ["home"] = "HOME", ["end"] = "END",
            ["left"] = "LEFT", ["right"] = "RIGHT", ["up"] = "UP", ["down"] = "DOWN",
            ["page up"] = "PAGEUP", ["pageup"] = "PAGEUP", ["page down"] = "PAGEDOWN", ["pagedown"] = "PAGEDOWN",
            ["space"] = "SPACE", ["slash"] = "/", ["/"] = "/", ["minus"] = "-", ["-"] = "-",
            ["equal"] = "=", ["="] = "=", ["semicolon"] = ";", [";"] = ";", ["apostrophe"] = "'", ["'"] = "'",
            ["comma"] = ",", [","] = ",", ["period"] = ".", ["."] = ".",
            ["plus"] = "+", ["kp plus"] = "+", ["+"] = "+",
        };
        for (var i = 1; i <= 12; i++)
            m[$"f{i}"] = $"F{i}";
        return m;
    }

    public static string? KeyToToken(System.Windows.Input.Key key)
    {
        return key switch
        {
            System.Windows.Input.Key.Escape => "ESC",
            System.Windows.Input.Key.Tab => "TAB",
            System.Windows.Input.Key.Enter or System.Windows.Input.Key.Return => "ENTER",
            System.Windows.Input.Key.Back => "BACKSPACE",
            System.Windows.Input.Key.Delete => "DELETE",
            System.Windows.Input.Key.Home => "HOME",
            System.Windows.Input.Key.End => "END",
            System.Windows.Input.Key.Left => "LEFT",
            System.Windows.Input.Key.Right => "RIGHT",
            System.Windows.Input.Key.Up => "UP",
            System.Windows.Input.Key.Down => "DOWN",
            System.Windows.Input.Key.PageUp => "PAGEUP",
            System.Windows.Input.Key.PageDown => "PAGEDOWN",
            System.Windows.Input.Key.Space => "SPACE",
            System.Windows.Input.Key.OemMinus => "-",
            System.Windows.Input.Key.OemPlus => "+",
            System.Windows.Input.Key.OemComma => ",",
            System.Windows.Input.Key.OemPeriod => ".",
            System.Windows.Input.Key.Oem2 => "/",
            System.Windows.Input.Key.Oem1 => ";",
            System.Windows.Input.Key.Oem3 => "'",
            System.Windows.Input.Key.F1 => "F1", System.Windows.Input.Key.F2 => "F2", System.Windows.Input.Key.F3 => "F3",
            System.Windows.Input.Key.F4 => "F4", System.Windows.Input.Key.F5 => "F5", System.Windows.Input.Key.F6 => "F6",
            System.Windows.Input.Key.F7 => "F7", System.Windows.Input.Key.F8 => "F8", System.Windows.Input.Key.F9 => "F9",
            System.Windows.Input.Key.F10 => "F10", System.Windows.Input.Key.F11 => "F11", System.Windows.Input.Key.F12 => "F12",
            _ => key >= System.Windows.Input.Key.D0 && key <= System.Windows.Input.Key.D9
                ? ((char)('0' + (key - System.Windows.Input.Key.D0))).ToString()
                : key >= System.Windows.Input.Key.A && key <= System.Windows.Input.Key.Z
                    ? ((char)('A' + (key - System.Windows.Input.Key.A))).ToString()
                    : null
        };
    }

    public static string KbNameToToken(string? name)
    {
        var n = (name ?? "").ToLowerInvariant().Trim();
        if (_kbNameToToken.TryGetValue(n, out var t)) return t;
        if (n.Length == 1 && char.IsLetterOrDigit(n[0])) return n.ToUpperInvariant();
        return "";
    }

    public static (int mods, List<string> keys) ParseShortcutMulti(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Shortcut text is empty.");
        var s = text.ToLowerInvariant().Trim().Replace(" ", "").Replace("-", "+");
        var parts = s.Split('+', StringSplitOptions.RemoveEmptyEntries);
        var isCtrl = false;
        var isAlt = false;
        var isShift = false;
        var isWin = false;
        var keys = new List<string>();
        foreach (var p in parts)
        {
            if (ModCtrl.Contains(p)) { isCtrl = true; continue; }
            if (ModAlt.Contains(p)) { isAlt = true; continue; }
            if (ModShift.Contains(p)) { isShift = true; continue; }
            if (ModWin.Contains(p)) { isWin = true; continue; }
            string tok;
            if (p == "pageup") tok = "PAGEUP";
            else if (p == "pagedown") tok = "PAGEDOWN";
            else if (p is "plus" or "kpplus" or "+") tok = "+";
            else
            {
                tok = KbNameToToken(p);
                if (string.IsNullOrEmpty(tok))
                    throw new InvalidOperationException($"Unknown key token: {p}");
            }
            keys.Add(tok);
        }
        if (keys.Count == 0)
            throw new InvalidOperationException("No main key found in shortcut.");
        var mods = (isCtrl ? 1 : 0) | (isAlt ? 2 : 0) | (isShift ? 4 : 0) | (isWin ? 8 : 0);
        return (mods, keys);
    }

    public static string FormatShortcutMulti(int mods, IReadOnlyList<string> keytoks)
    {
        var parts = new List<string>();
        if ((mods & 1) != 0) parts.Add("ctrl");
        if ((mods & 2) != 0) parts.Add("alt");
        if ((mods & 4) != 0) parts.Add("shift");
        if ((mods & 8) != 0) parts.Add("win");
        foreach (var k in keytoks ?? Array.Empty<string>())
            parts.Add((k ?? "").ToLowerInvariant());
        return string.Join(" + ", parts);
    }

    public static bool TryParseMapLine(string line, out int index, out int mods, out List<string> keys)
    {
        index = 0;
        mods = 0;
        keys = new List<string>();
        if (!line.StartsWith("MAP ", StringComparison.OrdinalIgnoreCase)) return false;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out index)) return false;
        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out mods)) return false;
        keys.AddRange(parts[3].Split(',', StringSplitOptions.RemoveEmptyEntries));
        return true;
    }

    public static Dictionary<string, int> LayoutToLabelToIndex(string[][] rows)
    {
        var flat = rows.SelectMany(r => r).ToList();
        if (flat.Count != BtnCount || flat.Distinct(StringComparer.OrdinalIgnoreCase).Count() != BtnCount)
            throw new InvalidOperationException("layout must be 16 unique labels a-p.");
        var set = new HashSet<string>(UiOrder, StringComparer.OrdinalIgnoreCase);
        if (!flat.All(x => set.Contains(x)))
            throw new InvalidOperationException("layout must use a-p once each.");
        var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < flat.Count; i++)
            d[flat[i].ToLowerInvariant()] = i;
        return d;
    }

    public static string[][] IndexPermutationToRows(Dictionary<string, int> labelToPhysicalIndex)
    {
        var arr = new string[BtnCount];
        foreach (var kv in labelToPhysicalIndex)
            arr[kv.Value] = kv.Key.ToLowerInvariant();
        return
        [
            arr.Take(4).ToArray(),
            arr.Skip(4).Take(4).ToArray(),
            arr.Skip(8).Take(4).ToArray(),
            arr.Skip(12).Take(4).ToArray()
        ];
    }
}
