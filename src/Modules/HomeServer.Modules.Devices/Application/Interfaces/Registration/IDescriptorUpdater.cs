using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDescriptorUpdater
{
    public void Update(
        DeviceDescriptor current,
        DeviceDescriptor incoming,
        DescriptorCompareResult compareResult);
}