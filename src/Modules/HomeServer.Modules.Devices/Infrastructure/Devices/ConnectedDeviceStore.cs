using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading.Tasks;
using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Interfaces.Events;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Modules.Devices.Infrastructure.Services.Events;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Persistence.Events;
using HomeServer.Persistence.Interfeaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public sealed class ConnectedDeviceStore : IConnectedDeviceStore
{
    private readonly IMqttService _mqttService;
    private readonly IDeviceEvents _deviceEvents;
    private readonly IRepositoryFactory _repository;
    private readonly ConcurrentDictionary<string, ConnectedDevice> _devices = new();

    public IEnumerable<ConnectedDevice> Devices =>
        _devices.Values;


    public ConnectedDeviceStore(
        IMqttService mqttService,
        IDeviceEvents deviceEvents,
        IRepositoryFactory repository
    )
    {
        _mqttService = mqttService;
        _deviceEvents = deviceEvents;
        _repository = repository;

        _deviceEvents.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    public async Task InitializeAsync()
    {
        var devices = await _repository.Table<Device>().QueryAsync(q => q.ToArrayAsync());
        foreach (var device in devices)
        {
            Add(new ConnectedDevice() { ExternalId = device.ExternalId });
        }
    }

    public ConnectedDevice? Get(string externalId)
    {
        return _devices.GetValueOrDefault(externalId);
    }

    public void Add(ConnectedDevice device)
    {
        _devices[device.ExternalId] = device;
    }

    public void SetStatus(string externalId, DeviceStatus status)
    {
        if (_devices.TryGetValue(externalId, out var device))
        {
            device.Status = status;
        }

        Console.WriteLine($"Client {externalId}: {status}");
    }

    public bool Remove(string externalId)
    {
        return _devices.TryRemove(externalId, out _);
    }

    public async Task<ConnectedDevice?> WaitForOnlineAsync(
        string externalId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (_devices.TryGetValue(externalId, out var device) &&
            device.Status == DeviceStatus.Online)
        {
            return device;
        }

        var delayTask = Task.Delay(timeout, cancellationToken);

        while (!delayTask.IsCompleted)
        {
            if (_devices.TryGetValue(externalId, out device) &&
                device.Status == DeviceStatus.Online)
            {
                return device;
            }

            await Task.Delay(50, cancellationToken);
        }

        return null;
    }

    private async Task OnDeviceStatusChanged(StatusChangeEvent @event)
    {
        var connectedDevice = Get(@event.ExternalId);

        if (connectedDevice == null) return;

        connectedDevice.Command = @event.Commands;
        connectedDevice.Telemetries = @event.Telemetry;

        SetStatus(@event.ExternalId, @event.Status);
    }

}