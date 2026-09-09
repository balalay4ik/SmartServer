using HomeServer.Core.Interfaces;
using HomeServer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServer.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services)
    {

        services.AddScoped<ILoggerService, LoggerService>();
        services.AddSingleton(typeof(IEventWaiter<,>), typeof(EventWaiter<,>));


        return services;
    }
}