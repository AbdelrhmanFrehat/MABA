using System.Collections.ObjectModel;
using System.Linq;

namespace MabaControlCenter.Services;

/// <summary>Official MacroPad profiles (aligned with control-center builtinProfiles.ts).</summary>
public static class MacroPadBuiltinProfiles
{
    public static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        "VS Code", "SOLIDWORKS", "Autodesk Inventor", "General", "Streaming / Gaming",
    };

    /// <summary>Display order in profile dropdown (built-ins first, then Default, then rest A–Z).</summary>
    public static readonly string[] OrderedBuiltinNames =
    [
        "VS Code", "SOLIDWORKS", "Autodesk Inventor", "General", "Streaming / Gaming",
    ];

    public static bool IsBuiltin(string? profileName) =>
        !string.IsNullOrWhiteSpace(profileName) && Names.Contains(profileName.Trim());

    public static void EnsureBuiltins(Dictionary<string, Dictionary<string, string>> perProduct)
    {
        foreach (var (name, map) in AllMaps())
        {
            if (perProduct.ContainsKey(name))
                continue;
            perProduct[name] = new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);
        }
    }

    public static void FillProfilesList(
        Dictionary<string, Dictionary<string, string>> perProduct,
        ObservableCollection<string> profiles)
    {
        profiles.Clear();
        var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in OrderedBuiltinNames)
        {
            if (!perProduct.ContainsKey(n)) continue;
            profiles.Add(n);
            added.Add(n);
        }
        if (perProduct.ContainsKey("Default") && !added.Contains("Default"))
        {
            profiles.Add("Default");
            added.Add("Default");
        }
        foreach (var n in perProduct.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            if (added.Contains(n)) continue;
            profiles.Add(n);
            added.Add(n);
        }
    }

    private static IEnumerable<(string Name, Dictionary<string, string> Map)> AllMaps()
    {
        yield return ("VS Code", Map(
            ("a", "ctrl+s"), ("b", "ctrl+c"), ("c", "ctrl+v"), ("d", "ctrl+z"), ("e", "ctrl+y"),
            ("f", "f5"), ("g", "ctrl+`"), ("h", "ctrl+/"), ("i", "ctrl+f"), ("j", "ctrl+h"),
            ("k", "ctrl+p"), ("l", "ctrl+shift+p"), ("m", "ctrl+b"), ("n", "ctrl+shift+o"),
            ("o", "ctrl+tab"), ("p", "ctrl+shift+tab")));
        yield return ("SOLIDWORKS", Map(
            ("a", "ctrl+b"), ("b", "ctrl+s"), ("c", "f"), ("d", "shift+e"), ("e", "s"),
            ("f", "ctrl+z"), ("g", "ctrl+y"), ("h", "space"), ("i", "ctrl+1"), ("j", "ctrl+2"),
            ("k", "ctrl+3"), ("l", "ctrl+4"), ("m", "ctrl+5"), ("n", "ctrl+6"), ("o", "ctrl+8"), ("p", "f5")));
        yield return ("Autodesk Inventor", Map(
            ("a", "n"), ("b", "e"), ("c", "ctrl+s"), ("d", "ctrl+z"), ("e", "ctrl+y"),
            ("f", "shift+f4"), ("g", "f2"), ("h", "f3"), ("i", "f4"), ("j", "f6"),
            ("k", "ctrl+tab"), ("l", "ctrl+shift+tab"), ("m", "ctrl+shift+s"), ("n", "ctrl+o"),
            ("o", "ctrl+shift+n"), ("p", "esc")));
        yield return ("General", Map(
            ("a", "ctrl+c"), ("b", "ctrl+v"), ("c", "ctrl+x"), ("d", "printscreen"), ("e", "win+shift+s"),
            ("f", "volumeup"), ("g", "volumedown"), ("h", "volumemute"), ("i", "ctrl+t"), ("j", "ctrl+w"),
            ("k", "alt+tab"), ("l", "ctrl+shift+esc"), ("m", "win+d"), ("n", "ctrl+alt+delete"),
            ("o", "win+e"), ("p", "win+r")));
        yield return ("Streaming / Gaming", Map(
            ("a", "ctrl+shift+m"), ("b", "ctrl+shift+d"), ("c", "ctrl+shift+s"), ("d", "ctrl+shift+r"),
            ("e", "ctrl+shift+c"), ("f", "ctrl+shift+p"), ("g", "ctrl+shift+u"), ("h", "ctrl+shift+f"),
            ("i", "alt+tab"), ("j", "alt+shift+tab"), ("k", "win+g"), ("l", "ctrl+shift+y"),
            ("m", "ctrl+shift+q"), ("n", "ctrl+shift+k"), ("o", "ctrl+shift+/"), ("p", "ctrl+shift+o")));
    }

    private static Dictionary<string, string> Map(params (string k, string v)[] pairs)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in pairs)
            d[k] = v;
        return d;
    }
}
