using HomeServer.Mqtt.Attributes;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Modules.Devices.Domain.Entities;
using System.Text.Json;

public sealed class DeviceMqttHandler : IMqttHandler
{
    private readonly IDeviceService _deviceService;

    public DeviceMqttHandler(
        IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [MqttRoute("devices/get-all")]
    public Task<List<Device>> GetAll(
        CancellationToken cancellationToken)
    {
        return _deviceService.GetDevices();
    }

    [MqttRoute("devices/get")]
    public async Task<DeviceDescriptor?> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _deviceService.GetAsync(id, cancellationToken);
    }

    [MqttRoute("devices/delete")]
    public async Task Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(
            id,
            cancellationToken);
    }

    [MqttRoute("devices/command")]
    public async Task Command(
        Guid id,
        DeviceCommandRequest request,
        CancellationToken cancellationToken)
    {
        await _deviceService.SendCommandAsync(
            id,
            request,
            cancellationToken);
    }

    [MqttRoute("device/availability")]
    public async Task Availabality(
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        await _deviceService.Availabality(payload, cancellationToken);
    }

    [MqttRoute("device/response")]
    public async Task Response(
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        await _deviceService.Response(payload, cancellationToken);
    }
}