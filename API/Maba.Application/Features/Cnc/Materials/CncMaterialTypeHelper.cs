namespace Maba.Application.Features.Cnc.Materials;

internal static class CncMaterialTypeHelper
{
    internal static readonly string[] ValidTypes = { "routing", "pcb", "both" };

    internal static string Normalize(string? type) =>
        string.IsNullOrWhiteSpace(type) ? "routing" : type.Trim().ToLowerInvariant();

    internal static void Validate(string type)
    {
        if (!ValidTypes.Contains(type))
        {
            throw new ArgumentException(
                $"Invalid material type '{type}'. Valid types: {string.Join(", ", ValidTypes)}");
        }
    }

    /// <summary>True when material is exclusive to PCB mode (legacy flag; not used for "both").</summary>
    internal static bool IsPcbOnlyFromType(string type) => type == "pcb";
}
