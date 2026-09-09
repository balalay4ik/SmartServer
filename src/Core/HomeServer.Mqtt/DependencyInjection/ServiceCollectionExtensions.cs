using HomeServer.Core.Interfaces;
using HomeServer.Core.Services;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Mqtt.Models;
using HomeServer.Mqtt.Services;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace HomeServer.Mqtt.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMqtt(
        this IServiceCollection services)
    {
        services.AddSingleton<IMqttClient>(sp =>
        {
            var factory = new MqttClientFactory();
            return factory.CreateMqttClient();
        });
        services.AddSingleton<IMqttService, MqttService>();
        services.AddSingleton<IMqttRouteRegistry, MqttRouteRegistry>();
        services.AddSingleton<IMqttDispatcher, MqttDispatcher>();
        services.AddSingleton<IMqttModelBinder, MqttModelBinder>();


        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x =>
                !x.IsDynamic &&
                x.FullName!.StartsWith("HomeServer."));

        var handlerTypes = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x =>
                x.IsClass &&
                !x.IsAbstract &&
                typeof(IMqttHandler).IsAssignableFrom(x));

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler);
        }

        return services;
    }
}