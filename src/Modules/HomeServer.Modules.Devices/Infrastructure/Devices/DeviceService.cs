using System.Data;
using System.Text.Json;
using HomeServer.Core.Entity;
using HomeServer.Core.Enums;
using HomeServer.Core.Interfaces;
using HomeServer.Core.Json;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Persistence.Interfeaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Interfaces.Events;
using HomeServer.Modules.Devices.Infrastructure.Extensions;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceTransportFactory _transportFactory;
    private readonly IDeviceAdapterFactory _adapterFactory;
    private readonly IRepositoryFactory _repository;
    private readonly ILoggerService _logger;
    private readonly IConnectedDeviceStore _connectedDeviceStore;
    private readonly IPendingCommandStore _pendingCommandStore;
    private readonly IDeviceEvents _deviceEvents;
    private readonly IEventWaiter<string, object> _eventWaiter;


    public DeviceService(
        IDeviceTransportFactory transportFactory,
        IDeviceAdapterFactory adapterFactory,
        IRepositoryFactory repository,
        ILoggerService logger,
        IConnectedDeviceStore connectedDeviceStore,
        IPendingCommandStore pendingCommandStore,
        IDeviceEvents deviceEvents,
        IEventWaiter<string, object> eventWaiter
        )
    {
        _transportFactory = transportFactory;
        _adapterFactory = adapterFactory;
        _repository = repository;
        _logger = logger;
        _connectedDeviceStore = connectedDeviceStore;
        _pendingCommandStore = pendingCommandStore;
        _deviceEvents = deviceEvents;
        _eventWaiter = eventWaiter;
    }

    public async Task Availabality(JsonElement payload, CancellationToken cancellationToken)
    {
        JsonElement element;
        string? externalId = null;

        if (JsonExtension.TryGetPropertyRecursive(payload, "clientId", out element))
        {
            externalId = element.GetString();
        }

        if (externalId == null) return;
        var adapterType = (await _repository.Table<Device>().QueryAsync(q => q.FirstAsync(x => x.ExternalId == externalId), cancellationToken)).Adapter;
        var adapter = _adapterFactory.Create(adapterType);

        StatusChangeEvent change = adapter.GetAvailabality(payload);
        change.ExternalId = externalId;

        await _deviceEvents.RaiseDeviceStatusChanged(change);

        _eventWaiter.TryComplete(externalId, null!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _repository.Table<Device>().DeleteAsync(id, cancellationToken);
    }

    public Task<IEnumerable<DiscoveredDevice>> DiscoverDevicesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<DeviceDescriptor?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await _repository.Table<Device>().QueryAsync(q => q
            .Include(x => x.Commands)
            .Include(x => x.Telemetries)
            .Include(x => x.Info)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken), cancellationToken);

        if (device is null) return null;

        var deviceDescriptor = device.ToDescription();
        deviceDescriptor.Status = DeviceStatus.Offline;

        var connected = _connectedDeviceStore.Get(device.ExternalId);
        if (connected is null)
        {
            return deviceDescriptor;
        }

        var adapter = _adapterFactory.Create(device.Adapter);
        var transport = _transportFactory.Create(device.Transport);

        await adapter.SendGeData(device, transport, cancellationToken);

        deviceDescriptor.Status = connected.Status;

        foreach (var command in deviceDescriptor.Commands)
        {
            if (!connected.Command.TryGetValue(command.Key, out var value))
                continue; // или логирование

            command.Value = value;
        }

        foreach (var telemetry in deviceDescriptor.Telemetries)
        {
            if (!connected.Telemetries.TryGetValue(telemetry.Key, out var value))
                continue; // или логирование

            telemetry.Value = value;
        }

        return deviceDescriptor;

    }

    public async Task<List<Device>> GetDevices(CancellationToken cancellationToken = default)
    {
        return await _repository.Table<Device>().QueryAsync(q => q
            .Include(x => x.Commands)
            .Include(x => x.Telemetries)
            .Include(x => x.Info)
            .ToListAsync(cancellationToken), cancellationToken);
    }

    public async Task<Device?> RegisterAsync(
        RegistrationContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var descriptor = context.AnalyzeResult!.Descriptor;

            var device = new Device
            {
                Name = descriptor.Name,
                ExternalId = descriptor.ExternalId,

                Address = context.Address == null ? descriptor.ExternalId : context.Address,
                Transport = context.Transport,
                Adapter = context.Adapter ?? DeviceAdapter.Unknown,

                Info = new DeviceInfo
                {
                    Vendor = descriptor.Info?.Vendor,
                    Product = descriptor.Info?.Product,
                    Model = descriptor.Info?.Model,

                    HardwareVersion = descriptor.Info?.HardwareVersion,
                    FirmwareVersion = descriptor.Info?.FirmwareVersion,
                    ProtocolVersion = descriptor.Info?.ProtocolVersion,

                    SerialNumber = descriptor.Info?.SerialNumber,
                    MacAddress = descriptor.Info?.MacAddress,

                    BuildDate = descriptor.Info?.BuildDate,
                    Description = descriptor.Info?.Description,

                    AdditionalDataJson = descriptor.Info?.AdditionalDataJson
                },

                Commands = descriptor.Commands
                    .Select(x => new Command
                    {
                        Key = x.Key,
                        Name = x.Name,
                        ControlType = x.ControlType,
                        ValueType = x.ValueType,
                        Method = x.Method,
                        Endpoint = x.Endpoint,
                        ConfigurationJson = x.Configuration is null
                            ? null
                            : JsonSerializer.Serialize(x.Configuration)
                    })
                    .ToList(),

                Telemetries = descriptor.Telemetries
                    .Select(x => new Telemetry
                    {
                        Key = x.Key,
                        Name = x.Name,
                        ValueType = x.ValueType,
                        Unit = x.Unit,
                        ConfigurationJson = x.Configuration is null
                            ? null
                            : JsonSerializer.Serialize(x.Configuration)
                    })
                    .ToList()
            };

            await _repository.Table<Device>().AddAsync(device);
            return device;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Level = LogLevel.Error,
                Category = LogCategory.Registration,

                Event = "Register",

                Source = nameof(DeviceService),

                Message = "Unhandled exception while processing device complete registration.",

                Exception = ex.ToString(),
                StackTrace = ex.StackTrace,
                ExpiresAt = DateTime.UtcNow.AddDays(1),

                Data = JsonSerializer.Serialize(new
                {
                    RegistrationId = context.Id
                })
            });

            return null;
        }
    }

    public async Task SendCommandAsync(Guid id, DeviceCommandRequest request, CancellationToken cancellationToken)
    {
        var device = await _repository.Table<Device>().QueryAsync(q => q.Include(x => x.Commands).FirstAsync(x => x.Id == id), cancellationToken);
        var transport = _transportFactory.Create(device.Transport);
        var adapter = _adapterFactory.Create(device.Adapter);
        var command = device.Commands.FirstOrDefault(x => x.Key == request.Key);

        if (command is null)
            throw new InvalidOperationException(
                $"Command '{request.Key}' not found.");

        var pending = await _pendingCommandStore.GetByCommandIdAsync(command.Id, cancellationToken);

        if (pending is not null)
            return;

        var transportRequest = await adapter.BuildCommandRequestAsync(device, command, request.Value, cancellationToken);

        var status = await _connectedDeviceStore
        .WaitForOnlineAsync(
            device.ExternalId,
            TimeSpan.FromSeconds(3),
            cancellationToken);

        if (status is null)
            return;

        pending = await _pendingCommandStore.GetByCommandIdAsync(command.Id, cancellationToken);

        if (pending is not null)
            return;

        await transport.SendAsync(transportRequest, cancellationToken);

        pending = new PendingCommand()
        {
            DeviceId = device.Id,
            ExternalId = device.ExternalId,
            CommandId = command.Id,
            Request = transportRequest
        };

        await _pendingCommandStore.AddAsync(pending, cancellationToken);
    }

    public async Task Response(JsonElement payload, CancellationToken cancellationToken)
    {
        JsonElement element;
        string? externalId = null;

        if (JsonExtension.TryGetPropertyRecursive(payload, "clientId", out element))
        {
            externalId = element.GetString();
        }

        if (externalId == null) return;

        var device = await _repository.Table<Device>().QueryAsync(q => q.FirstAsync(x => x.ExternalId == externalId), cancellationToken);
        var pending = await _pendingCommandStore.GetByDeviceIdAsync(device.Id);
        if (pending is null) return;

        await _pendingCommandStore.RemoveAsync(pending.CommandId);
    }
}