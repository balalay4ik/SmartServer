using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Application.Models;

public sealed class DescriptorCompareResult
{
    public bool IsEqual => Changes.Count == 0;

    public bool RequiresAdminApproval =>
        Warnings.Count != 0;

    public List<DescriptorChange> Changes { get; } = [];

    public List<DeviceWarning> Warnings { get; } = [];
}