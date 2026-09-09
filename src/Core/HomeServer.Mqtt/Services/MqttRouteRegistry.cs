using HomeServer.Mqtt.Attributes;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Mqtt.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HomeServer.Mqtt.Services;

public sealed class MqttRouteRegistry : IMqttRouteRegistry
{
    private readonly Dictionary<string, MqttRouteDescriptor> _routes = new();

    public IReadOnlyDictionary<string, MqttRouteDescriptor> Routes => _routes;

    public void Initialize()
    {
        _routes.Clear();

        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x =>
                !x.IsDynamic &&
                x.FullName is not null &&
                x.FullName.StartsWith("HomeServer."));

        foreach (var assembly in assemblies)
        {
            RegisterAssembly(assembly);
        }
    }

    private void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            if (!typeof(IMqttHandler).IsAssignableFrom(type))
                continue;

            RegisterHandler(type);
        }
    }

    private void RegisterHandler(Type handlerType)
    {
        foreach (var method in handlerType.GetMethods(
                     BindingFlags.Instance |
                     BindingFlags.Public))
        {
            var route = method.GetCustomAttribute<MqttRouteAttribute>();

            if (route is null)
                continue;

            if (_routes.ContainsKey(route.Topic))
            {
                throw new InvalidOperationException(
                    $"MQTT route '{route.Topic}' already exists.");
            }

            var routeInfo = BuildRoute(
                route.Topic,
                method);

            _routes.Add(
                route.Topic,
                new MqttRouteDescriptor
                {
                    Topic = route.Topic,
                    HandlerType = handlerType,
                    Method = method,
                    Parameters = method.GetParameters(),
                    Regex = routeInfo.Regex,
                    RouteParameters = routeInfo.Parameters,
                    SubscribeTopic = ToMqttTopic(route.Topic),
                });
        }
    }

    private static (Regex Regex, IReadOnlyList<MqttRouteParameter> Parameters)
    BuildRoute(string topic, MethodInfo method)
    {
        var parameters = new List<MqttRouteParameter>();

        var pattern = Regex.Replace(
            topic,
            @"\{(\w+)\}",
            match =>
            {
                var name = match.Groups[1].Value;

                var parameter = method.GetParameters()
                    .First(x => x.Name == name);

                parameters.Add(new MqttRouteParameter
                {
                    Name = name,
                    Type = parameter.ParameterType
                });

                return $"(?<{name}>[^/]+)";
            });

        pattern = "^" + pattern + "$";

        return (
            new Regex(pattern, RegexOptions.Compiled),
            parameters);
    }

    private static string ToMqttTopic(string route)
    {
        return Regex.Replace(
            route,
            @"\{\w+\}",
            "+");
    }
}