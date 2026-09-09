using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using HomeServer.Persistence.Context;
using HomeServer.Modules.Devices.DependencyInjection;
using HomeServer.Persistence.DependencyInjection;
using HomeServer.Modules.Devices;
using HomeServer.Core;
using HomeServer.Mqtt.DependencyInjection;
using HomeServer.Core.DependencyInjection;
using HomeServer.Mqtt.Interfaces;

try
{

    SQLitePCL.Batteries.Init();


    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();

    var modulesAssembly = new[] {
        typeof(DevicesAssemblyMarker).Assembly,
        typeof(CoreAssemblyMarker).Assembly
    };

    builder.Services.AddCore();
    builder.Services.AddMqtt();

    builder.Services.AddDevicesModule();

    builder.Services.AddPersistence(modulesAssembly);

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        await InitializeAsync(scope);

    }

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}

static async Task InitializeAsync(IServiceScope scope)
{
    var factory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<AppDbContext>>();

    await using var db = await factory.CreateDbContextAsync();

    await db.Database.MigrateAsync();

    var mqttService = scope.ServiceProvider.GetRequiredService<IMqttService>();

    await mqttService.InitializeAsync();

    // var registry = scope.ServiceProvider.GetRequiredService<IMqttRouteRegistry>();

    // registry.Initialize();

    // foreach (var route in registry.Routes)
    // {
    //     Console.WriteLine($"{route.Key} -> {route.Value.HandlerType.Name}.{route.Value.Method.Name}");
    // }
}