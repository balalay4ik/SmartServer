namespace HomeServer.SDK.Devices;

public interface IDeviceSDK
{
    public Task<IEnumerable<string>> GetExternalIdAllRegisteredDevice();
    public Task SendCommandByExternalId(string id, string key, object? value, CancellationToken cancellationToken);
    public Task SendCommandByDbId(Guid id, string key, object? value, CancellationToken cancellationToken);
}