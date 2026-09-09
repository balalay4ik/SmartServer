using HomeServer.Modules.Devices.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using HomeServer.Infrastructure.Extension;
using HomeServer.Modules.Devices.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Persistence.Interfeaces;
using HomeServer.Core.Interfaces;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Core.Enums;
using HomeServer.Core.Entity;
using HomeServer.Module.Devices.Application.Dto;
using HomeServer.Mqtt.Interfaces;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationStore _registrationStore;
    private readonly IAdapterResolver _adapterResolver;
    private readonly IDeviceAdapterFactory _adapterFactory;
    private readonly IDeviceTransportFactory _transportFactory;
    private readonly IRepositoryFactory _repository;
    private readonly IDeviceService _deviceService;
    private readonly IDescriptorComparer _descriptorComparer;
    private readonly IDescriptorUpdater _descriptorUpdater;
    private readonly ILoggerService _logger;

    public RegistrationService(
        IRepositoryFactory repository,
        IRegistrationStore registrationStore,
        IAdapterResolver adapterResolver,
        IDeviceAdapterFactory adapterFactory,
        IDeviceTransportFactory transportFactory,
        IDeviceService deviceService,
        IDescriptorComparer descriptorComparer,
        IDescriptorUpdater descriptorUpdater,
        ILoggerService logger
    )
    {
        _registrationStore = registrationStore;
        _adapterResolver = adapterResolver;
        _adapterFactory = adapterFactory;
        _transportFactory = transportFactory;
        _repository = repository;
        _deviceService = deviceService;
        _descriptorComparer = descriptorComparer;
        _descriptorUpdater = descriptorUpdater;
        _logger = logger;
    }

    public async Task<Guid> ReceiveAsync(
        DeviceTransport transport,
        string payload,
        string? remoteAddress,
        CancellationToken cancellationToken)
    {
        Guid? registrationId = null;
        string? externalId = null;
        Device? registeredDevice = null;

        try
        {
            var resolveResult = _adapterResolver.Resolve(transport, payload);

            var adapter = _adapterFactory.Create(
                resolveResult.MatchType == AdapterMatchType.None
                    ? DeviceAdapter.Unknown
                    : resolveResult.Adapter);

            var analyzeResult = await adapter.AnalyzeAsync(new AdapterAnalyzeContext
            {
                Payload = payload,
                Address = remoteAddress,
                Transport = transport,
                CancellationToken = cancellationToken
            });


            if (resolveResult.MatchType == AdapterMatchType.Exact)
            {
                externalId = analyzeResult.Descriptor.ExternalId;

                registeredDevice = await _repository
                    .Table<Device>()
                    .QueryAsync(
                        q => q.FirstOrDefaultAsync(x => x.ExternalId == externalId),
                        cancellationToken);

                if (registeredDevice is not null)
                {
                    // var dbDescription = registeredDevice.ToDescription();

                    // var compareResult = _descriptorComparer.Compare(
                    //     dbDescription,
                    //     analyzeResult.Descriptor);

                    // _descriptorUpdater.Update(
                    //     dbDescription,
                    //     analyzeResult.Descriptor,
                    //     compareResult);

                    // await _repository
                    //     .Table<Device>()
                    //     .UpdateAsync(dbDescription.ToDevice(), cancellationToken);

                    return registeredDevice.Id;
                }

                var pendingRegistration = _registrationStore
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.MatchType == AdapterMatchType.Exact &&
                        x.AnalyzeResult?.Descriptor.ExternalId == externalId);

                if (pendingRegistration is not null)
                {
                    registrationId = pendingRegistration.Id;

                    var compareResult = _descriptorComparer.Compare(
                        pendingRegistration.AnalyzeResult!.Descriptor,
                        analyzeResult.Descriptor);

                    _descriptorUpdater.Update(
                        pendingRegistration.AnalyzeResult.Descriptor,
                        analyzeResult.Descriptor,
                        compareResult);

                    return pendingRegistration.Id;
                }
            }

            var registration = new RegistrationContext
            {
                Transport = transport,
                Address = remoteAddress,
                RawPayload = payload,

                Adapter = adapter.Adapter,
                MatchType = resolveResult.MatchType,

                AnalyzeResult = analyzeResult
            };

            registrationId = registration.Id;

            _registrationStore.Add(registration);

            return registration.Id;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Level = LogLevel.Error,
                Category = LogCategory.Registration,

                Event = "Receive",

                Source = nameof(RegistrationService),

                Message = "Unhandled exception while processing device registration.",

                Exception = ex.ToString(),
                StackTrace = ex.StackTrace,
                ExpiresAt = DateTime.UtcNow.AddDays(1),

                Data = JsonSerializer.Serialize(new
                {
                    Transport = transport,
                    RemoteAddress = remoteAddress,
                    ExternalId = externalId,
                    RegistrationId = registrationId,
                    Payload = payload
                })
            });

            return Guid.Empty;
        }
    }

    public IReadOnlyCollection<PendingRegistrationDto> GetAll()
    {
        return _registrationStore.GetAll().ToDto();
    }

    public PendingRegistrationDto? Get(Guid id)
    {
        return _registrationStore.Get(id)?.ToDto();
    }

    public async Task<PendingRegistrationDto?> AnalyzeAsync(
    Guid id,
    DeviceAdapter adapterType,
    CancellationToken cancellationToken)
    {
        RegistrationContext? context = null;
        try
        {
            context = _registrationStore.Get(id);

            if (context is null)
                throw new InvalidOperationException();

            var adapter = _adapterFactory.Create(adapterType);

            context.Adapter = adapterType;

            context.AnalyzeResult = await adapter.AnalyzeAsync(new AdapterAnalyzeContext
            {
                Payload = context.RawPayload,
                Address = context.Address,
                Transport = context.Transport,
                CancellationToken = cancellationToken
            });

            return context.ToDto();
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Level = LogLevel.Error,
                Category = LogCategory.Registration,

                Event = "Analyze",

                Source = nameof(RegistrationService),

                Message = "Unhandled exception while processing analyze registration.",

                Exception = ex.ToString(),
                StackTrace = ex.StackTrace,
                ExpiresAt = DateTime.UtcNow.AddDays(1),

                Data = JsonSerializer.Serialize(new
                {
                    Id = id,
                    AdapterType = adapterType,
                    Context = context
                })
            });
            return null;
        }
    }

    public async Task<bool> CompleteAsync(
    Guid id,
    RegistrationCompleteRequest request,
    CancellationToken cancellationToken)
    {
        RegistrationContext? context = null;

        try
        {
            context = _registrationStore.Get(id);

            if (context is null)
                throw new InvalidOperationException();

            context.AnalyzeResult!.Descriptor.Name = request.Name;

            var registeredDevice = await _deviceService.RegisterAsync(
            context,
            cancellationToken);

            if (registeredDevice != null)
            {
                _registrationStore.Remove(id);
            }
            else
                return false;

            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Level = LogLevel.Error,
                Category = LogCategory.Registration,

                Event = "Complete",

                Source = nameof(RegistrationService),

                Message = "Unhandled exception while processing complete registration.",

                Exception = ex.ToString(),
                StackTrace = ex.StackTrace,
                ExpiresAt = DateTime.UtcNow.AddDays(1),

                Data = JsonSerializer.Serialize(new
                {
                    Id = id,
                    Request = request,
                    Context = context
                })
            });

            return false;
        }
    }
}