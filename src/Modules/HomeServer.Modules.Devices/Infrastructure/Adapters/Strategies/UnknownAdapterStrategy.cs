using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Mqtt.Models;
using System.Text.Json;
using HomeServer.Application.Models.Event;

namespace HomeServer.Infrastructure.Adapter;

public class UnknownAdapterStrategy : IDeviceAdapterStrategy
{
    public DeviceAdapter Adapter => DeviceAdapter.HomeServer;
    public AdapterMatchType Match(string payload)
    {
        return AdapterMatchType.None;
    }

    public async Task<AdapterAnalyzeResult> AnalyzeAsync(
    AdapterAnalyzeContext context)
    {
        return new AdapterAnalyzeResult() { Descriptor = new() };
    }

    public Task<TransportRequest> BuildCommandRequestAsync(Device device, Command command, object? value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TransportRequest> BuildRegistrationResponseAsync(Device registeredDevice, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public DeviceStatus GetAvailabality(JsonElement payload)
    {
        throw new NotImplementedException();
    }

    StatusChangeEvent IDeviceAdapterStrategy.GetAvailabality(JsonElement payload)
    {
        throw new NotImplementedException();
    }

    public Task<TransportRequest> BuildPingRequest(Device device)
    {
        throw new NotImplementedException();
    }

    public Task SendGeData(Device device, IDeviceTransportStrategy transport, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
