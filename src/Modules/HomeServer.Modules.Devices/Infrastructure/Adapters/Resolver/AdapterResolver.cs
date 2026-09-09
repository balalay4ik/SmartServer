using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class AdapterResolver : IAdapterResolver
{
    private readonly IEnumerable<IDeviceAdapterStrategy> _adapters;

    public AdapterResolver(
        IEnumerable<IDeviceAdapterStrategy> adapters)
    {
        _adapters = adapters;
    }

    public AdapterResolveResult Resolve(
        DeviceTransport transport,
        string rawPayload)
    {
        AdapterResolveResult? compatible = null;

        foreach (var adapter in _adapters)
        {
            var match = adapter.Match(rawPayload);

            switch (match)
            {
                case AdapterMatchType.Exact:
                    return new AdapterResolveResult
                    {
                        Adapter = adapter.Adapter,
                        MatchType = AdapterMatchType.Exact
                    };

                case AdapterMatchType.Compatible:
                    compatible ??= new AdapterResolveResult
                    {
                        Adapter = adapter.Adapter,
                        MatchType = AdapterMatchType.Compatible
                    };
                    break;
            }
        }

        return compatible ?? new AdapterResolveResult
        {
            Adapter = DeviceAdapter.Unknown,
            MatchType = AdapterMatchType.None
        };
    }
}