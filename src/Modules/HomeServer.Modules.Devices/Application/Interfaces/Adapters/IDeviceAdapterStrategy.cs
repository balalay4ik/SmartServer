using System.Text.Json;
using HomeServer.Application.Models;
using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceAdapterStrategy
{
    DeviceAdapter Adapter { get; }

    AdapterMatchType Match(string payload);

    Task<AdapterAnalyzeResult> AnalyzeAsync(
        AdapterAnalyzeContext context);
    Task<TransportRequest> BuildCommandRequestAsync(Device device, Command command, object? value, CancellationToken cancellationToken);
    StatusChangeEvent GetAvailabality(JsonElement payload);
    Task<TransportRequest> BuildPingRequest(Device device);
    Task SendGeData(Device device, IDeviceTransportStrategy transport, CancellationToken cancellationToken);

}