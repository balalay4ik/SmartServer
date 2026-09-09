using System.Reflection;
using System.Text.Json;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;

namespace HomeServer.Modules.Devices.Infrastructure.Extensions;

public static class DeviceMapperExtensions
{
    public static DeviceDescriptor ToDescription(this Device device)
    {
        var descriptor = new DeviceDescriptor();

        Copy(device, descriptor);

        descriptor.Info = device.Info?.ToDescription();

        descriptor.Commands = device.Commands
            .Select(x => x.ToDescription())
            .ToList();

        descriptor.Telemetries = device.Telemetries
            .Select(x => x.ToDescription())
            .ToList();

        return descriptor;
    }

    public static Device ToDevice(this DeviceDescriptor descriptor)
    {
        var device = new Device();

        Copy(descriptor, device);

        device.Info = descriptor.Info?.ToDevice();

        device.Commands = descriptor.Commands
            .Select(x => x.ToDevice())
            .ToList();

        device.Telemetries = descriptor.Telemetries
            .Select(x => x.ToDevice())
            .ToList();

        return device;
    }

    public static DeviceInfoDescriptor ToDescription(this DeviceInfo info)
    {
        var descriptor = new DeviceInfoDescriptor();

        Copy(info, descriptor);

        return descriptor;
    }

    public static DeviceInfo ToDevice(this DeviceInfoDescriptor descriptor)
    {
        var info = new DeviceInfo();

        Copy(descriptor, info);

        return info;
    }

    public static CommandDescriptor ToDescription(this Command command)
    {
        var descriptor = new CommandDescriptor();

        Copy(command, descriptor);

        descriptor.Configuration =
            Deserialize(command.ConfigurationJson);

        return descriptor;
    }

    public static Command ToDevice(this CommandDescriptor descriptor)
    {
        var command = new Command();

        Copy(descriptor, command);

        command.ConfigurationJson =
            Serialize(descriptor.Configuration);

        return command;
    }

    public static TelemetryDescriptor ToDescription(this Telemetry telemetry)
    {
        var descriptor = new TelemetryDescriptor();

        Copy(telemetry, descriptor);

        descriptor.Configuration =
            Deserialize(telemetry.ConfigurationJson);

        return descriptor;
    }

    public static Telemetry ToDevice(this TelemetryDescriptor descriptor)
    {
        var telemetry = new Telemetry();

        Copy(descriptor, telemetry);

        telemetry.ConfigurationJson =
            Serialize(descriptor.Configuration);

        return telemetry;
    }

    private static void Copy<TSource, TDestination>(
        TSource source,
        TDestination destination)
    {
        var sourceProperties = typeof(TSource)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var destinationProperties = typeof(TDestination)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(x => x.Name);

        foreach (var sourceProperty in sourceProperties)
        {
            if (!sourceProperty.CanRead)
                continue;

            if (!destinationProperties.TryGetValue(
                    sourceProperty.Name,
                    out var destinationProperty))
                continue;

            if (!destinationProperty.CanWrite)
                continue;

            if (sourceProperty.PropertyType != destinationProperty.PropertyType)
                continue;

            destinationProperty.SetValue(
                destination,
                sourceProperty.GetValue(source));
        }
    }

    private static string? Serialize(
        Dictionary<string, object>? dictionary)
    {
        return dictionary is null
            ? null
            : JsonSerializer.Serialize(dictionary);
    }

    private static Dictionary<string, object>? Deserialize(
        string? json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

}