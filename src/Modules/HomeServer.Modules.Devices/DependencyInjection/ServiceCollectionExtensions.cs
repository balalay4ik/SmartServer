using HomeServer.Core.Interfaces;
using HomeServer.Infrastructure.Adapter;
using HomeServer.Infrastructure.Transport;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Interfaces.Events;
using HomeServer.Modules.Devices.Infrastructure.Services;
using HomeServer.Modules.Devices.Infrastructure.Services.Events;
using HomeServer.SDK.Devices;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace HomeServer.Modules.Devices.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevicesModule(
        this IServiceCollection services)
    {

        services.AddHttpClient<HttpTransportStrategy>();


        services.AddScoped<IDeviceTransportStrategy, HttpTransportStrategy>();
        services.AddScoped<IDeviceTransportStrategy, MqttTransportStrategy>();
        services.AddScoped<IDeviceTransportStrategy, WebSocketTransportStrategy>();

        services.AddScoped<IDeviceAdapterStrategy, HomeServerAdapterStrategy>();

        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IDeviceAdapterFactory, DeviceAdapterFactory>();
        services.AddScoped<IDeviceTransportFactory, DeviceTransportFactory>();
        services.AddScoped<IDeviceRegistryService, DeviceRegistryService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IAdapterResolver, AdapterResolver>();
        services.AddScoped<IDescriptorComparer, DescriptorComparer>();
        services.AddScoped<IDescriptorUpdater, DescriptorUpdater>();
        services.AddScoped<IPendingCommandStore, PendingCommandStore>();

        services.AddSingleton<IConnectedDeviceStore, ConnectedDeviceStore>();
        services.AddSingleton<IRegistrationStore, RegistrationStore>();
        services.AddSingleton<IDeviceEvents, DeviceEvents>();
        services.AddHostedService<DeviceHostedService>();

        services.AddScoped<IDeviceSDK, DeviceSDK>();

        return services;
    }
}