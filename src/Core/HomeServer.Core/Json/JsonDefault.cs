using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeServer.Core.Json;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
            {
                new JsonStringEnumConverter()
            }
    };
}