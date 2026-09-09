using System.Text.Json;
using System.Threading.Tasks;
using HomeServer.Application.Models;
using HomeServer.Application.Models.Event;
using HomeServer.Core.Interfaces;
using HomeServer.Core.Json;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Mqtt.Models;

namespace HomeServer.Infrastructure.Adapter;

public class HomeServerAdapterStrategy : IDeviceAdapterStrategy
{
    public DeviceAdapter Adapter => DeviceAdapter.HomeServer;

    private readonly IEventWaiter<string, object> _eventWaiter;

    public HomeServerAdapterStrategy(
        IEventWaiter<string, object> eventWaiter
    )
    {
        _eventWaiter = eventWaiter;
    }

    public AdapterMatchType Match(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);

            var root = document.RootElement;

            if (!root.TryGetProperty("system", out var system))
                return AdapterMatchType.Compatible;

            if (!system.TryGetProperty("vendor", out var vendor))
                return AdapterMatchType.Compatible;

            if (vendor.GetString() != "HomeServer")
                return AdapterMatchType.None;

            return AdapterMatchType.Exact;
        }
        catch
        {
            return AdapterMatchType.None;
        }
    }

    public async Task<AdapterAnalyzeResult> AnalyzeAsync(
    AdapterAnalyzeContext context)
    {
        using var document = JsonDocument.Parse(context.Payload);

        var root = document.RootElement;

        var system = root.GetProperty("system");
        var device = root.GetProperty("device");
        var info = device.GetProperty("info");

        var descriptor = new DeviceDescriptor
        {
            ExternalId = GetString(device, "externalId")!,
            Name = GetString(device, "name")!,
            Adapter = DeviceAdapter.HomeServer,

            Info = new DeviceInfoDescriptor
            {
                Vendor = GetString(info, "vendor"),
                Product = GetString(info, "product"),
                Model = GetString(info, "model"),

                HardwareVersion = GetString(info, "hardwareVersion"),
                FirmwareVersion = GetString(system, "firmwareVersion"),
                ProtocolVersion = GetString(system, "protocolVersion"),

                SerialNumber = GetString(info, "serialNumber"),
                MacAddress = GetString(info, "macAddress"),

                BuildDate = GetString(info, "buildDate"),
                Description = GetString(info, "description"),

                AdditionalDataJson = info.TryGetProperty("additionData", out var metadata)
                    ? metadata.GetRawText()
                    : null
            }
        };

        if (string.IsNullOrWhiteSpace(context.Address)) descriptor.Address = descriptor.ExternalId;


        if (device.TryGetProperty("commands", out var commands))
        {
            foreach (var command in commands.EnumerateArray())
            {
                descriptor.Commands.Add(new CommandDescriptor
                {
                    Key = GetString(command, "key")!,
                    Name = GetString(command, "name")!,

                    ControlType = Enum.Parse<CommandControlType>(
                        GetString(command, "controlType")!),

                    Method = GetString(command, "method")!,
                    Endpoint = GetString(command, "endpoint")!,

                    ValueType = GetString(command, "valueType")!,

                    Configuration = command.TryGetProperty("configuration", out var configuration)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(configuration.GetRawText())
                        : null
                });
            }
        }

        if (device.TryGetProperty("telemetries", out var telemetries))
        {
            foreach (var telemetry in telemetries.EnumerateArray())
            {
                descriptor.Telemetries.Add(new TelemetryDescriptor
                {
                    Key = GetString(telemetry, "key")!,
                    Name = GetString(telemetry, "name")!,

                    ValueType = GetString(telemetry, "valueType")!,

                    Unit = GetString(telemetry, "unit"),

                    Configuration = telemetry.TryGetProperty("configuration", out var configuration)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(configuration.GetRawText())
                        : null
                });
            }
        }

        var messages = new List<AdapterMessage>();

        if (descriptor.Commands.Count == 0)
        {
            messages.Add(new AdapterMessage
            {
                Type = AdapterMessageType.Warning,
                Message = "No commands were detected."
            });
        }

        if (descriptor.Telemetries.Count == 0)
        {
            messages.Add(new AdapterMessage
            {
                Type = AdapterMessageType.Warning,
                Message = "No telemetries were detected."
            });
        }

        return new AdapterAnalyzeResult
        {
            Descriptor = descriptor,
            Messages = messages
        };
    }

    private static string? GetString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => property.GetRawText()
        };
    }

    public async Task<TransportRequest> BuildCommandRequestAsync(
        Device device,
        Command command,
        object? value,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new
        {
            value
        });

        return device.Transport switch
        {
            DeviceTransport.Http =>
                await BuildHttpRequest(
                    device.Address,
                    command.Endpoint,
                    message,
                    command.Method!),

            DeviceTransport.Mqtt =>
                await BuildMqttRequest(
                    device.ExternalId,
                    command.Endpoint,
                    message,
                    command.Method),

            DeviceTransport.WebSocket =>
                await BuildWebSocketRequest(
                    device.Address,
                    command.Endpoint,
                    message),

            _ => throw new NotSupportedException(
                $"Transport '{device.Transport}' is not supported.")
        };
    }

    public async Task<TransportRequest> BuildPingRequest(Device device)
    {
        var enpoint = "ping";
        var message = JsonSerializer.Serialize(new
        {

        });
        var method = "";
        var request = device.Transport switch
        {
            DeviceTransport.Http =>
                await BuildHttpRequest(
                    device.Address,
                    enpoint,
                    message,
                    method!),

            DeviceTransport.Mqtt =>
                await BuildMqttRequest(
                    device.ExternalId,
                    enpoint,
                    message,
                    method),

            DeviceTransport.WebSocket =>
                await BuildWebSocketRequest(
                    device.Address,
                    enpoint,
                    message),

            _ => throw new NotSupportedException(
                $"Transport '{device.Transport}' is not supported.")
        };

        return request;
    }

    private async Task<TransportRequest> BuildHttpRequest(
    string address,
    string endpoint,
    string message,
    string method)
    {
        return new TransportRequest
        {
            Address = $"{address}/{endpoint}",
            Message = message,
            Metadata =
        {
            ["Method"] = method
        }
        };
    }

    private async Task<TransportRequest> BuildMqttRequest(
        string address,
        string endpoint,
        string message,
        string? method = null,
        bool retain = false)
    {
        var request = new TransportRequest
        {
            Address = $"{address}/{endpoint}",
            Message = message
        };

        if (method is not null)
        {
            request.Metadata["Method"] = method;
        }

        return request;
    }

    private async Task<TransportRequest> BuildWebSocketRequest(
        string address,
        string endpoint,
        string message)
    {
        return new TransportRequest
        {
            Address = $"{address}/{endpoint}",
            Message = message
        };
    }

    public StatusChangeEvent GetAvailabality(JsonElement payload)
    {
        JsonElement element;
        StatusChangeEvent change = new();
        change.Status = DeviceStatus.Offline;
        string? newStatus = null;

        if (JsonExtension.TryGetPropertyRecursive(payload, "status", out element))
        {
            newStatus = element.GetString();
        }

        if (newStatus == null)
            return change;

        var deviceStatus = Enum.Parse<DeviceStatus>(
            newStatus,
            ignoreCase: true);

        change.Status = deviceStatus;

        if (JsonExtension.TryGetPropertyRecursive(payload, "command", out element) &&
element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var value = property.Value.ToString();

                // здесь записываем значение команды
                change.Commands[property.Name] = value;
            }
        }

        return change;
    }

    public async Task SendGeData(Device device, IDeviceTransportStrategy transport, CancellationToken cancellationToken)
    {
        var waitTask = _eventWaiter.WaitAsync(
            device.ExternalId,
            cancellationToken
        );
        var request = await BuildPingRequest(device);
        await transport.Ping(request, cancellationToken);
        await waitTask;
    }

}
