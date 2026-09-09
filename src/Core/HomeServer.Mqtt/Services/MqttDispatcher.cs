using System.Text.Json;
using System.Text.RegularExpressions;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Mqtt.Models;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace HomeServer.Mqtt.Services;

public class MqttDispatcher : IMqttDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMqttRouteRegistry _routeRegistry;
    private readonly IMqttModelBinder _mqttModelBinder;
    private readonly IMqttClient _client;

    public MqttDispatcher(
        IServiceScopeFactory scopeFactory,
        IMqttRouteRegistry routeRegistry,
        IMqttModelBinder mqttModelBinder,
        IMqttClient client
        )
    {
        _scopeFactory = scopeFactory;
        _routeRegistry = routeRegistry;
        _mqttModelBinder = mqttModelBinder;
        _client = client;
    }
    public async Task DispatchAsync(
        MqttApplicationMessageReceivedEventArgs args,
        CancellationToken cancellationToken)
    {
        MqttRouteDescriptor? descriptor = null;
        Match? match = null;

        foreach (var route in _routeRegistry.Routes.Values)
        {
            var current = route.Regex.Match(
                args.ApplicationMessage.Topic);

            if (!current.Success)
                continue;

            descriptor = route;
            match = current;

            break;
        }

        if (descriptor is null)
            return;

        using var scope = _scopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService(
            descriptor.HandlerType);

        var parameters = _mqttModelBinder.Bind(
            descriptor,
            args,
            scope.ServiceProvider,
            cancellationToken);

        var result = descriptor.Method.Invoke(
            handler,
            parameters);

        object? response = null;

        if (result is Task task)
        {
            await task;

            var taskType = task.GetType();

            if (taskType.IsGenericType)
            {
                response = taskType
                    .GetProperty("Result")!
                    .GetValue(task);
            }
        }
        else
        {
            response = result;
        }

        if (response is not null &&
    !string.IsNullOrWhiteSpace(args.ApplicationMessage.ResponseTopic))
        {
            var json = JsonSerializer.Serialize(response);

            await _client.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic(args.ApplicationMessage.ResponseTopic)
                    .WithPayload(json)
                    .WithCorrelationData(args.ApplicationMessage.CorrelationData)
                    .Build(),
                cancellationToken);
        }
    }
}