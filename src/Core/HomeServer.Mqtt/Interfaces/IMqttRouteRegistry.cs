using System.Reflection;
using HomeServer.Mqtt.Models;

namespace HomeServer.Mqtt.Interfaces;

public interface IMqttRouteRegistry
{
    IReadOnlyDictionary<string, MqttRouteDescriptor> Routes { get; }

    void Initialize();
}