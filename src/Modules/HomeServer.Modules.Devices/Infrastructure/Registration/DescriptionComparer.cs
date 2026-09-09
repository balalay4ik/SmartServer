using HomeServer.Application.Models;
using HomeServer.Domain.Entities;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DescriptorComparer : IDescriptorComparer
{
    public DescriptorCompareResult Compare(
    DeviceDescriptor current,
    DeviceDescriptor incoming)
    {
        var result = new DescriptorCompareResult();

        CompareObject(
            current,
            incoming,
            DescriptorChangeType.Device,
            result);

        CompareObject(
            current.Info,
            incoming.Info,
            DescriptorChangeType.DeviceInfo,
            result);

        CompareCollection(
            current.Commands,
            incoming.Commands,
            x => x.Key,
            DescriptorChangeType.Command,
            result);

        CompareCollection(
            current.Telemetries,
            incoming.Telemetries,
            x => x.Key,
            DescriptorChangeType.Telemetry,
            result);

        // Бизнес-проверки
        CompareFirmware(current, incoming, result);
        CompareMac(current, incoming, result);
        CompareSerialNumber(current, incoming, result);

        return result;
    }

    #region Warnings
    private static void CompareFirmware(
    DeviceDescriptor current,
    DeviceDescriptor incoming,
    DescriptorCompareResult result)
    {
        if (current.Info?.FirmwareVersion == incoming.Info?.FirmwareVersion)
            return;

        result.Changes.Add(new DescriptorChange
        {
            Type = DescriptorChangeType.DeviceInfo,
            Property = nameof(DeviceInfoDescriptor.FirmwareVersion),
            Action = DescriptorChangeAction.Update,
            OldValue = current.Info?.FirmwareVersion,
            NewValue = incoming.Info?.FirmwareVersion,
            AutoApply = true
        });
    }

    private static void CompareSerialNumber(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorCompareResult result)
    {
        if (current.Info?.SerialNumber == incoming.Info?.SerialNumber)
            return;

        result.Warnings.Add(new DeviceWarning
        {
            Type = DeviceWarningType.SerialNumberChanged,
            Severity = WarningSeverity.Warning,
            Message = "Serial number has changed."
        });
    }

    private static void CompareMac(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorCompareResult result)
    {
        if (current.Info?.MacAddress == incoming.Info?.MacAddress)
            return;

        result.Warnings.Add(new DeviceWarning
        {
            Type = DeviceWarningType.MacAddressChanged,
            Severity = WarningSeverity.Warning,
            Message = "MAC address has changed."
        });
    }

    #endregion

    #region AutoApply
    private static void CompareCollection<T>(
    IReadOnlyCollection<T> current,
    IReadOnlyCollection<T> incoming,
    Func<T, string> keySelector,
    DescriptorChangeType objectType,
    DescriptorCompareResult result)
    {
        var currentItems = current.ToDictionary(keySelector);
        var incomingItems = incoming.ToDictionary(keySelector);

        // Добавление и изменение
        foreach (var (key, incomingItem) in incomingItems)
        {
            if (!currentItems.TryGetValue(key, out var currentItem))
            {
                result.Changes.Add(new DescriptorChange
                {
                    Type = objectType,
                    Key = key,
                    Property = string.Empty,
                    Action = DescriptorChangeAction.Add,
                    NewValue = incomingItem,
                    AutoApply = true
                });

                continue;
            }

            foreach (var property in typeof(T).GetProperties())
            {
                if (!property.CanRead)
                    continue;

                var oldValue = property.GetValue(currentItem);
                var newValue = property.GetValue(incomingItem);

                if (Equals(oldValue, newValue))
                    continue;

                result.Changes.Add(new DescriptorChange
                {
                    Type = objectType,
                    Key = key,
                    Property = property.Name,
                    Action = DescriptorChangeAction.Update,
                    OldValue = oldValue,
                    NewValue = newValue,
                    AutoApply = true
                });
            }
        }

        // Удаление
        foreach (var (key, currentItem) in currentItems)
        {
            if (incomingItems.ContainsKey(key))
                continue;

            result.Changes.Add(new DescriptorChange
            {
                Type = objectType,
                Key = key,
                Property = string.Empty,
                Action = DescriptorChangeAction.Remove,
                OldValue = currentItem,
                AutoApply = true
            });
        }
    }

    private static void CompareObject<T>(
    T? current,
    T? incoming,
    DescriptorChangeType objectType,
    DescriptorCompareResult result)
    {
        if (current is null || incoming is null)
            return;

        foreach (var property in typeof(T).GetProperties())
        {
            if (!property.CanRead)
                continue;

            var oldValue = property.GetValue(current);
            var newValue = property.GetValue(incoming);

            if (Equals(oldValue, newValue))
                continue;

            result.Changes.Add(new DescriptorChange
            {
                Type = objectType,
                Property = property.Name,
                OldValue = oldValue,
                NewValue = newValue,
                AutoApply = true
            });
        }
    }

    #endregion
}