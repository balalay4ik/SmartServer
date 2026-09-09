using HomeServer.Application.Models;
using HomeServer.Domain.Entities;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Modules.Devices.Infrastructure.Extensions;

namespace HomeServer.Modules.Devices.Infrastructure.Services;


public sealed class DescriptorUpdater : IDescriptorUpdater
{
    public void Update(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorCompareResult compareResult)
    {

        foreach (var change in compareResult.Changes)
        {
            if (!change.AutoApply)
                continue;

            switch (change.Type)
            {
                case DescriptorChangeType.Device:
                    Apply(current, change);
                    break;

                case DescriptorChangeType.DeviceInfo:
                    Apply(current.Info!, change);
                    break;

                case DescriptorChangeType.Command:
                    ApplyCommand(current, incoming, change);
                    break;

                case DescriptorChangeType.Telemetry:
                    ApplyTelemetry(current, incoming, change);
                    break;
            }
        }

    }

    private static void Apply(
        object target,
        DescriptorChange change)
    {
        if (change.Action != DescriptorChangeAction.Update)
            return;

        var property = target
            .GetType()
            .GetProperty(change.Property);

        property?.SetValue(target, change.NewValue);
    }

    private static void ApplyCommand(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorChange change)
    {
        switch (change.Action)
        {
            case DescriptorChangeAction.Add:

                current.Commands.Add(
                    incoming.Commands.First(x => x.Key == change.Key));

                break;

            case DescriptorChangeAction.Remove:

                current.Commands.Remove(
                    current.Commands.First(x => x.Key == change.Key));

                break;

            case DescriptorChangeAction.Update:

                var command = current.Commands
                    .First(x => x.Key == change.Key);

                Apply(command, change);

                break;
        }
    }

    private static void ApplyTelemetry(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorChange change)
    {
        switch (change.Action)
        {
            case DescriptorChangeAction.Add:

                current.Telemetries.Add(
                    incoming.Telemetries.First(x => x.Key == change.Key));

                break;

            case DescriptorChangeAction.Remove:

                current.Telemetries.Remove(
                    current.Telemetries.First(x => x.Key == change.Key));

                break;

            case DescriptorChangeAction.Update:

                var telemetry = current.Telemetries
                    .First(x => x.Key == change.Key);

                Apply(telemetry, change);

                break;
        }
    }
}