using System.Globalization;
using System.Text.Json;

namespace Infrastructure.Helpers;

/// <summary>
/// Shared parsing utilities for incoming Kepware REST IoT JSON payloads.
/// </summary>
internal static class IoTPacketParserHelper
{
    public static string ConvertToString(object? obj)
    {
        if (obj == null) return string.Empty;

        if (obj is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                _ => element.ToString()
            };
        }

        return obj.ToString() ?? string.Empty;
    }

    public static bool ParseQuality(object? qObj)
    {
        if (qObj == null) return false;
        if (qObj is bool b) return b;
        if (qObj is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.True) return true;
            if (element.ValueKind == JsonValueKind.False) return false;
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var intVal)) return intVal != 0;
            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (bool.TryParse(str, out var parsedBool)) return parsedBool;
                if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt)) return parsedInt != 0;
            }
            return false;
        }

        var raw = qObj.ToString();
        if (bool.TryParse(raw, out var res)) return res;
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i != 0;
        return false;
    }

    public static bool TryGetBoolean(object? obj)
    {
        if (obj == null) return false;
        if (obj is bool b) return b;
        if (obj is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.True) return true;
            if (element.ValueKind == JsonValueKind.False) return false;
            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var d)) return d != 0;
            if (element.ValueKind == JsonValueKind.String)
            {
                var s = element.GetString();
                if (bool.TryParse(s, out var parsed)) return parsed;
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var num)) return num != 0;
            }
            return false;
        }

        var str = obj.ToString();
        if (bool.TryParse(str, out var boolRes)) return boolRes;
        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var dRes)) return dRes != 0;
        return false;
    }
}

