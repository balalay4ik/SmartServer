using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IAdapterResolver
{
    AdapterResolveResult Resolve(
    DeviceTransport transport,
    string rawPayload);
}