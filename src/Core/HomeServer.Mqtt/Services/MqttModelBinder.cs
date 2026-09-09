using System.Text;
using System.Text.Json;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Mqtt.Models;
using MQTTnet;

namespace HomeServer.Mqtt.Services;

public sealed class MqttModelBinder : IMqttModelBinder
{
    public object?[] Bind(
        MqttRouteDescriptor descriptor,
        MqttApplicationMessageReceivedEventArgs args,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var values = new object?[descriptor.Parameters.Length];

        var payload = args.ApplicationMessage.Payload.Length > 0
            ? Encoding.UTF8.GetString(args.ApplicationMessage.Payload)
            : string.Empty;

        JsonDocument? document = null;
        JsonElement root = default;

        if (!string.IsNullOrWhiteSpace(payload))
        {
            document = JsonDocument.Parse(payload);
            root = document.RootElement.Clone();
        }

        try
        {
            for (var i = 0; i < descriptor.Parameters.Length; i++)
            {
                var parameter = descriptor.Parameters[i];
                var type = parameter.ParameterType;

                if (type == typeof(CancellationToken))
                {
                    values[i] = cancellationToken;
                    continue;
                }

                if (type == typeof(MqttApplicationMessageReceivedEventArgs))
                {
                    values[i] = args;
                    continue;
                }

                if (type == typeof(JsonDocument))
                {
                    values[i] = document;
                    continue;
                }

                if (type == typeof(JsonElement))
                {
                    values[i] = root;
                    continue;
                }

                if (type == typeof(string) &&
                    descriptor.Parameters.Length == 1)
                {
                    values[i] = payload;
                    continue;
                }

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty(parameter.Name!, out var property))
                {
                    values[i] = BindPrimitive(property, type);
                    continue;
                }

                if (root.ValueKind == JsonValueKind.Object &&
                    type.IsClass &&
                    type != typeof(string))
                {
                    values[i] = JsonSerializer.Deserialize(
                        root.GetRawText(),
                        type);

                    continue;
                }
            }

            return values;
        }
        finally
        {
            document?.Dispose();
        }
    }

    private static object? BindPrimitive(
        JsonElement property,
        Type type)
    {
        if (type == typeof(string))
            return property.GetString();

        if (type == typeof(bool))
            return property.GetBoolean();

        if (type == typeof(byte))
            return property.GetByte();

        if (type == typeof(short))
            return property.GetInt16();

        if (type == typeof(int))
            return property.GetInt32();

        if (type == typeof(long))
            return property.GetInt64();

        if (type == typeof(float))
            return property.GetSingle();

        if (type == typeof(double))
            return property.GetDouble();

        if (type == typeof(decimal))
            return property.GetDecimal();

        if (type == typeof(Guid))
            return property.GetGuid();

        if (type == typeof(DateTime))
            return property.GetDateTime();

        if (type.IsEnum)
            return Enum.Parse(type, property.GetString()!, true);

        return JsonSerializer.Deserialize(
            property.GetRawText(),
            type);
    }
}