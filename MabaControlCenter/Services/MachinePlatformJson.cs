using System.Text.Json;
using System.Text.Json.Serialization;

namespace MabaControlCenter.Services;

public static class MachinePlatformJson
{
    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new TolerantEnumJsonConverterFactory());
        return options;
    }
}

public class TolerantEnumJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var type = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        return type.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var nullableEnumType = Nullable.GetUnderlyingType(typeToConvert);
        var converterType = nullableEnumType != null
            ? typeof(NullableTolerantEnumJsonConverter<>).MakeGenericType(nullableEnumType)
            : typeof(TolerantEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class TolerantEnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed))
                    return parsed;

                var normalized = raw.Replace("-", "_").Replace(" ", "_");
                if (Enum.TryParse<TEnum>(normalized, ignoreCase: true, out parsed))
                    return parsed;
            }

            return UnknownOrDefault(raw);
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numeric))
        {
            if (Enum.IsDefined(typeof(TEnum), numeric))
                return (TEnum)Enum.ToObject(typeof(TEnum), numeric);
        }

        return UnknownOrDefault(reader.TokenType.ToString());
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    private static TEnum UnknownOrDefault(string? raw)
    {
        if (Enum.TryParse<TEnum>("Unknown", ignoreCase: true, out var unknown))
            return unknown;

        // Enums without Unknown are local app-owned states; keep their default rather than crashing cache/profile load.
        return default;
    }
}

public class NullableTolerantEnumJsonConverter<TEnum> : JsonConverter<TEnum?> where TEnum : struct, Enum
{
    private readonly TolerantEnumJsonConverter<TEnum> _inner = new();

    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return _inner.Read(ref reader, typeof(TEnum), options);
    }

    public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString());
        else
            writer.WriteNullValue();
    }
}
