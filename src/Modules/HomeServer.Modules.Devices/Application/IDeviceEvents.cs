using HomeServer.Application.Models.Event;

namespace HomeServer.Modules.Devices.Application.Interfaces.Events;

public interface IDeviceEvents
{
    event Func<StatusChangeEvent, Task>? DeviceStatusChanged;
    Task RaiseDeviceStatusChanged(StatusChangeEvent @event);

}