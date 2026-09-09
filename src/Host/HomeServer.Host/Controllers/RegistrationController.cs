using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using HomeServer.Core.Json;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(
        IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    public async Task<IActionResult> Register(
    [FromBody] JsonElement payload,
    CancellationToken cancellationToken)
    {
        var transport = DeviceTransport.Http;

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        string? address = null;

        // baseAddress
        if (payload.TryGetProperty("system", out var system))
        {
            if (system.TryGetProperty("baseAddress", out var baseAddress))
            {
                address = baseAddress.GetString();
            }
        }

        // protocol + port
        if (address is null && system.ValueKind != JsonValueKind.Undefined)
        {
            var protocol = "http";
            var port = 80;


            if (JsonExtension.TryGetPropertyRecursive(payload, "protocol", out var protocolElement))
            {
                protocol = protocolElement.GetString() ?? "http";
            }

            if (JsonExtension.TryGetPropertyRecursive(payload, "httpPort", out var portElement))
            {
                port = JsonExtension.GetInt(portElement) ?? port;
            }
            else if (JsonExtension.TryGetPropertyRecursive(payload, "port", out portElement))
            {
                port = JsonExtension.GetInt(portElement) ?? port;
            }

            address = BuildAddress(protocol, ip!, port);
        }

        // Совсем ничего не нашли
        address ??= ip;

        var result = await _registrationService.ReceiveAsync(
            transport,
            payload.GetRawText(),
            address,
            cancellationToken);

        if (result == Guid.Empty)
        {
            return StatusCode(500);
        }

        return Ok();
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_registrationService.GetAll());
    }

    [HttpGet("{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var context = _registrationService.Get(id);

        if (context is null)
            return NotFound();

        return Ok(context);
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<IActionResult> Analyze(
    Guid id,
    [FromBody] DeviceAdapter adapter,
    CancellationToken cancellationToken)
    {
        var result = await _registrationService.AnalyzeAsync(
            id,
            adapter,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
    Guid id,
    [FromBody] RegistrationCompleteRequest request,
    CancellationToken cancellationToken)
    {
        await _registrationService.CompleteAsync(
            id,
            request,
            cancellationToken);

        return NoContent();
    }



    private static string BuildAddress(string protocol, string ip, int port)
    {
        if (IPAddress.TryParse(ip, out var address) &&
            address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            ip = $"[{ip}]";
        }

        return $"{protocol}://{ip}:{port}";
    }
}