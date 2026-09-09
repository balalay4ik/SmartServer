using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Application.Models;

public sealed class AdapterAnalyzeResult
{
    public required DeviceDescriptor Descriptor { get; init; }

    public List<AdapterMessage> Messages { get; init; } = [];
}