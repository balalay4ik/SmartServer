namespace HomeServer.Mqtt.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MqttRouteAttribute : Attribute
{
    public string Topic { get; }

    public MqttRouteAttribute(string topic)
    {
        Topic = topic;
    }
}