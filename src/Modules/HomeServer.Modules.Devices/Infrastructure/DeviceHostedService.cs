using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Persistence.Events;
using HomeServer.Persistence.Interfeaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeServer.Modules.Devices.Infrastructure.Services.Events;

public class DeviceHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRepositoryFactory _repository;
    private readonly IConnectedDeviceStore _connectedDeviceStore;


    public DeviceHostedService(
        IServiceScopeFactory scopeFactory,
        IRepositoryFactory repository,
        IConnectedDeviceStore connectedDeviceStore
    )
    {
        _scopeFactory = scopeFactory;
        _repository = repository;
        _connectedDeviceStore = connectedDeviceStore;

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _repository.EntityChanged += OnEntityChanged;
        _connectedDeviceStore.InitializeAsync();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _repository.EntityChanged -= OnEntityChanged;
        return Task.CompletedTask;
    }



    private async Task OnEntityChanged(DbEntityChangedEvent @event)
    {
        CancellationToken cancellationToken = new();
        foreach (var entity in @event.Entities)
        {
            if (entity is not Device device)
                continue;

            using var scope = _scopeFactory.CreateScope();

            switch (@event.ChangeType)
            {
                case DbEntityChangeType.Added:

                    var _adapterFactory = scope.ServiceProvider.GetRequiredService<IDeviceAdapterFactory>();
                    var _transportFactory = scope.ServiceProvider.GetRequiredService<IDeviceTransportFactory>();

                    _connectedDeviceStore.Add(new ConnectedDevice() { ExternalId = device.ExternalId });
                    var adapter = _adapterFactory.Create(device.Adapter);
                    var transport = _transportFactory.Create(device.Transport);

                    var request = await adapter.BuildPingRequest(device);
                    await transport.Ping(request, cancellationToken);

                    break;

                case DbEntityChangeType.Removed:
                    _connectedDeviceStore.Remove(
                        device.ExternalId);
                    break;

                case DbEntityChangeType.Updated:
                    // обработка изменения
                    break;
            }
        }
    }


}