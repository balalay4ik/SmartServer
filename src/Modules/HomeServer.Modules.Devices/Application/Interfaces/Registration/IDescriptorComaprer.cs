using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDescriptorComparer
{
    DescriptorCompareResult Compare(
        DeviceDescriptor current,
        DeviceDescriptor incoming);
}