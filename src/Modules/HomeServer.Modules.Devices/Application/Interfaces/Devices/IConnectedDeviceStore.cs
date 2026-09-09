using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IConnectedDeviceStore
{
    IEnumerable<ConnectedDevice> Devices { get; }

    Task InitializeAsync();

    ConnectedDevice? Get(string externalId);

    void Add(ConnectedDevice device);

    void SetStatus(string externalId, DeviceStatus status);

    bool Remove(string externalId);

    Task<ConnectedDevice?> WaitForOnlineAsync(
        string externalId,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}