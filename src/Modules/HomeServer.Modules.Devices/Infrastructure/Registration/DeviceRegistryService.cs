using System.Text.Json;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Persistence.Interfeaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DeviceRegistryService : IDeviceRegistryService
{
    private readonly IRepositoryFactory _repository;
    public DeviceRegistryService(
        IRepositoryFactory repository
    )
    {
        _repository = repository;
    }

    public async Task RegisterAsync(
        DeviceRegistrationContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Descriptor);
        ArgumentNullException.ThrowIfNull(context.Discovered);

        if (context.Discovered.Adapter is null)
            throw new InvalidOperationException("Device adapter is not specified.");

        if (context.Discovered.Transport is null)
            throw new InvalidOperationException("Device transport is not specified.");

        var repository = _repository.Table<Device>();

        var device = await repository.QueryAsync(query =>
            query
                .Include(x => x.Commands)
                .Include(x => x.Telemetries)
                .FirstOrDefaultAsync(
                    x => x.ExternalId == context.Descriptor.ExternalId,
                    cancellationToken));

        if (device is null)
        {
            device = new Device();

            await FillDevice(device, context);

            await repository.AddAsync(device);
        }
        else
        {
            await FillDevice(device, context);

            await repository.UpdateAsync(device);
        }
    }

    private async Task FillDevice(
        Device device,
        DeviceRegistrationContext context)
    {
        device.ExternalId = context.Descriptor.ExternalId;
        device.Name = context.Descriptor.Name;

        device.Address = context.Discovered.Address;
        device.Adapter = context.Discovered.Adapter!.Value;
        device.Transport = context.Discovered.Transport!.Value;

        await SyncCommands(device, context.Descriptor);
        await SyncTelemetries(device, context.Descriptor);
    }

    private static List<Command> CreateCommands(
            DeviceDescriptor descriptor)
    {
        return descriptor.Commands
            .Select(command => new Command
            {
                Key = command.Key,
                Name = command.Name,

                ControlType = command.ControlType,
                ValueType = command.ValueType,

                ConfigurationJson = command.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(command.Configuration)
            })
            .ToList();
    }

    private static List<Telemetry> CreateTelemetries(
        DeviceDescriptor descriptor)
    {
        return descriptor.Telemetries
            .Select(telemetry => new Telemetry
            {
                Key = telemetry.Key,
                Name = telemetry.Name,

                ValueType = telemetry.ValueType,
                Unit = telemetry.Unit,

                ConfigurationJson = telemetry.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(telemetry.Configuration)
            })
            .ToList();
    }

    private async Task SyncCommands(
    Device device,
    DeviceDescriptor descriptor)
    {
        // Удаляем отсутствующие
        var removed = device.Commands
            .Where(x => descriptor.Commands.All(c => c.Key != x.Key))
            .ToList();

        foreach (var command in removed)
            device.Commands.Remove(command);

        await _repository.Table<Command>().DeleteRangeAsync(removed);


        // Обновляем существующие / создаем новые
        foreach (var commandDescriptor in descriptor.Commands)
        {
            var command = device.Commands
                .FirstOrDefault(x => x.Key == commandDescriptor.Key);

            if (command is null)
            {
                device.Commands.Add(new Command
                {
                    Key = commandDescriptor.Key,
                    Name = commandDescriptor.Name,
                    ControlType = commandDescriptor.ControlType,
                    ValueType = commandDescriptor.ValueType,
                    ConfigurationJson = commandDescriptor.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(commandDescriptor.Configuration)
                });

                continue;
            }

            command.Name = commandDescriptor.Name;
            command.ControlType = commandDescriptor.ControlType;
            command.ValueType = commandDescriptor.ValueType;
            command.ConfigurationJson = commandDescriptor.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(commandDescriptor.Configuration);
        }
    }

    private async Task SyncTelemetries(
    Device device,
    DeviceDescriptor descriptor)
    {
        var removed = device.Telemetries
            .Where(x => descriptor.Telemetries.All(t => t.Key != x.Key))
            .ToList();

        foreach (var telemetry in removed)
            device.Telemetries.Remove(telemetry);

        await _repository.Table<Telemetry>().DeleteRangeAsync(removed);

        foreach (var telemetryDescriptor in descriptor.Telemetries)
        {
            var telemetry = device.Telemetries
                .FirstOrDefault(x => x.Key == telemetryDescriptor.Key);

            if (telemetry is null)
            {
                device.Telemetries.Add(new Telemetry
                {
                    Key = telemetryDescriptor.Key,
                    Name = telemetryDescriptor.Name,
                    ValueType = telemetryDescriptor.ValueType,
                    Unit = telemetryDescriptor.Unit,
                    ConfigurationJson = telemetryDescriptor.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(telemetryDescriptor.Configuration)
                });

                continue;
            }

            telemetry.Name = telemetryDescriptor.Name;
            telemetry.ValueType = telemetryDescriptor.ValueType;
            telemetry.Unit = telemetryDescriptor.Unit;
            telemetry.ConfigurationJson = telemetryDescriptor.Configuration is null
                    ? null
                    : JsonSerializer.Serialize(telemetryDescriptor.Configuration);
        }
    }
}