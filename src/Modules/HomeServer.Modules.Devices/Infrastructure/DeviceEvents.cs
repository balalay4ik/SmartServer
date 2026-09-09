using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Interfaces.Events;

namespace HomeServer.Modules.Devices.Infrastructure.Services.Events;

public class DeviceEvents : IDeviceEvents
{
    public event Func<StatusChangeEvent, Task>? DeviceStatusChanged;

    public async Task RaiseDeviceStatusChanged(StatusChangeEvent @event)
    {
        DeviceStatusChanged?.Invoke(@event);
    }
}