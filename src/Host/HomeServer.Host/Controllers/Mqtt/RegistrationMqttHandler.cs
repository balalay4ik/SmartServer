using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using HomeServer.Module.Devices.Application.Dto;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Mqtt.Attributes;
using HomeServer.Mqtt.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class RegistrationMqttHandler : IMqttHandler
{
    private readonly IRegistrationService _registrationService;

    public RegistrationMqttHandler(
        IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [MqttRoute("registration/request")]
    public async Task Register(
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.ReceiveAsync(
            DeviceTransport.Mqtt,
            payload.GetRawText(),
            null,
            cancellationToken);

        if (result == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Registration failed.");
        }
    }

    [MqttRoute("registration/analyze")]
    public async Task<PendingRegistrationDto?> Analyze(
        Guid id,
        DeviceAdapter adapter,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.AnalyzeAsync(
            id,
            adapter,
            cancellationToken);

        return result;
    }

    [MqttRoute("registration/complete")]
    public Task Complete(
        Guid id,
        RegistrationCompleteRequest request,
        CancellationToken cancellationToken)
    {
        return _registrationService.CompleteAsync(
            id,
            request,
            cancellationToken);
    }
}