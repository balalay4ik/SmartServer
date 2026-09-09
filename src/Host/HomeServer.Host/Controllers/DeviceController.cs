using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/devices")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceAdapterFactory _adapterFactory;
    private readonly IDeviceRegistryService _deviceRegistry;

    public DeviceController(
        IDeviceService deviceService,
        IDeviceAdapterFactory adapterFactory,
        IDeviceRegistryService deviceRegistry
    )
    {
        _deviceService = deviceService;
        _adapterFactory = adapterFactory;
        _deviceRegistry = deviceRegistry;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _deviceService.GetDevices());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetAsync(id, cancellationToken);

        if (device is null)
            return NotFound();

        return Ok(device);
    }

    // [HttpPut("{id:guid}")]
    // public async Task<IActionResult> Update(
    // Guid id,
    // UpdateDeviceRequest request,
    // CancellationToken cancellationToken)
    // {
    // await _deviceService.UpdateAsync(
    // id,
    // request,
    // cancellationToken);
    // 
    // return NoContent();
    // }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/command")]
    public async Task<IActionResult> Command(
    Guid id,
    [FromBody] DeviceCommandRequest request,
    CancellationToken cancellationToken)
    {
        await _deviceService.SendCommandAsync(
            id,
            request,
            cancellationToken);

        return Ok();
    }
}