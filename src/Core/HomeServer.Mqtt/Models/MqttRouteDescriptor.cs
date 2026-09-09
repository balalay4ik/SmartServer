using System.Reflection;
using System.Text.RegularExpressions;

namespace HomeServer.Mqtt.Models;

public sealed class MqttRouteDescriptor
{
    public required string Topic { get; init; }

    public required Type HandlerType { get; init; }

    public required MethodInfo Method { get; init; }
    public required ParameterInfo[] Parameters { get; init; }
    public required Regex Regex { get; init; }
    public required IReadOnlyList<MqttRouteParameter> RouteParameters { get; init; }
    public required string SubscribeTopic { get; init; }
}