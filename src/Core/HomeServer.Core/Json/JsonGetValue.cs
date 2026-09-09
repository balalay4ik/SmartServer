using System.Text.Json;

namespace HomeServer.Core.Json;

public static class JsonExtension
{
    public static bool TryGetPropertyRecursive(
    JsonElement element,
    string propertyName,
    out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName))
                {
                    value = property.Value;
                    return true;
                }

                if (TryGetPropertyRecursive(property.Value, propertyName, out value))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryGetPropertyRecursive(item, propertyName, out value))
                    return true;
            }
        }

        value = default;
        return false;
    }

    public static int? GetInt(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.GetInt32(),

            JsonValueKind.String when int.TryParse(element.GetString(), out var value)
                => value,

            _ => null
        };
    }
}