namespace Maba.Application.Features.Laser;

public static class LaserMaterialTypeValidator
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "cut",
        "engrave",
        "both"
    };

    public static string NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return "both";
        }

        return type.Trim().ToLowerInvariant();
    }

    public static bool IsValidType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return false;
        }

        return AllowedTypes.Contains(type.Trim());
    }

    public static (bool IsValid, string NormalizedType, string? ErrorMessage) ValidateAndNormalize(string? type)
    {
        var normalized = NormalizeType(type);

        if (!AllowedTypes.Contains(normalized))
        {
            return (false, normalized, $"Invalid material type '{type}'. Allowed values: cut, engrave, both");
        }

        return (true, normalized, null);
    }
}
