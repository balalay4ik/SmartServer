using System.Text;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Infrastructure.Transport;

public class HttpTransportStrategy : IDeviceTransportStrategy
{
    public DeviceTransport Transport => DeviceTransport.Http;
    private readonly HttpClient _httpClient;

    public HttpTransportStrategy(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendAsync(
        TransportRequest request,
        CancellationToken cancellationToken
    )
    {
        var method = request.Metadata.TryGetValue(
            "Method",
            out var value)
            ? new HttpMethod(value)
            : HttpMethod.Get;

        using var httpRequest = new HttpRequestMessage(
            method,
            request.Address);

        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            httpRequest.Content = new StringContent(
                request.Message,
                Encoding.UTF8,
                "application/json");
        }

        var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(
            cancellationToken);
    }

    public Task Ping(TransportRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}