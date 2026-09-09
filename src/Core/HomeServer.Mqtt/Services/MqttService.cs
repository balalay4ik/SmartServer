using HomeServer.Core.Interfaces;
using HomeServer.Mqtt.Interfaces;
using HomeServer.Mqtt.Models;
using HomeServer.Persistence.Events;
using HomeServer.Persistence.Interfeaces;
using HomeServer.SDK.Devices;
using HomeServer.SDK.Devices.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace HomeServer.Mqtt.Services;

public sealed class MqttService : IMqttService
{
    private readonly IMqttClient _client;
    private readonly MqttOptions _options;
    private readonly IMqttRouteRegistry _mqttRegistry;
    private readonly IMqttDispatcher _mqttDispatcher;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRepositoryFactory _repository;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private CancellationTokenSource? _reconnectCts;

    private bool _initialized;
    private bool _shutdownRequested;

    public bool IsConnected => _client.IsConnected;

    public MqttService(
        IMqttClient client,
        IOptions<MqttOptions> options,
        IMqttRouteRegistry mqttRegistry,
        IMqttDispatcher mqttDispatcher,
        IServiceScopeFactory scopeFactory,
        IRepositoryFactory repository
        )
    {
        _client = client;
        _options = options.Value;
        _mqttRegistry = mqttRegistry;
        _mqttDispatcher = mqttDispatcher;
        _scopeFactory = scopeFactory;
        _repository = repository;

        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.ConnectedAsync += OnConnectedAsync;
        _client.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (_client.IsConnected)
            return;

        _shutdownRequested = false;

        _mqttRegistry.Initialize();

        await ConnectAsync(cancellationToken);

        _initialized = true;
    }

    public async Task ShutdownAsync(
        CancellationToken cancellationToken = default)
    {
        _shutdownRequested = true;

        _reconnectCts?.Cancel();

        if (!_client.IsConnected)
            return;

        try
        {
            await _client.DisconnectAsync(
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"MQTT shutdown failed: {ex.Message}");
        }
    }

    private async Task ConnectAsync(
        CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_client.IsConnected)
                return;

            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    _options.Host,
                    _options.Port)
                .WithCleanSession(
                    _options.CleanSession)
                .WithKeepAlivePeriod(
                    TimeSpan.FromSeconds(30));

            if (!string.IsNullOrWhiteSpace(_options.ClientId))
            {
                builder.WithClientId(
                    _options.ClientId);
            }

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                builder.WithCredentials(
                    _options.Username,
                    _options.Password);
            }

            await _client.ConnectAsync(
                builder.Build(),
                cancellationToken);

            await SubscribeAsync(cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task SubscribeAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var route in _mqttRegistry.Routes.Values)
        {
            await _client.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        route.SubscribeTopic)
                    .Build(),
                cancellationToken);
        }
    }

    private Task OnConnectedAsync(
        MqttClientConnectedEventArgs args)
    {
        Console.WriteLine("MQTT connected.");

        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(
        MqttClientDisconnectedEventArgs args)
    {
        Console.WriteLine(
            $"MQTT disconnected: {args.Reason}");

        if (_shutdownRequested)
            return Task.CompletedTask;

        StartReconnect();

        return Task.CompletedTask;
    }

    private void StartReconnect()
    {
        if (_reconnectCts is not null)
            return;

        _reconnectCts = new CancellationTokenSource();

        _ = ReconnectAsync(_reconnectCts.Token);
    }

    private async Task ReconnectAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            while (
                !_client.IsConnected &&
                !_shutdownRequested &&
                !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        cancellationToken);

                    if (_client.IsConnected)
                        break;

                    Console.WriteLine(
                        "MQTT reconnecting...");

                    await ConnectAsync(
                        cancellationToken);

                    if (_client.IsConnected)
                    {
                        Console.WriteLine(
                            "MQTT reconnected.");
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"MQTT reconnect failed: {ex.Message}");
                }
            }
        }
        finally
        {
            _reconnectCts?.Dispose();
            _reconnectCts = null;
        }
    }

    private Task OnMessageReceivedAsync(
        MqttApplicationMessageReceivedEventArgs args)
    {
        return _mqttDispatcher.DispatchAsync(args);
    }

    public async Task PublishAsync(
        MqttApplicationMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected)
            throw new InvalidOperationException(
                "MQTT client is not connected.");

        await _client.PublishAsync(
            message,
            cancellationToken);
    }


}

public sealed class MqttOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 1883;

    public string? ClientId { get; set; } = "Server";

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool CleanSession { get; set; } = false;
}