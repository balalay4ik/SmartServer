using System.Reflection;
using HomeServer.Persistence.Configuration;
using HomeServer.Persistence.Context;
using HomeServer.Persistence.Interfeaces;
using HomeServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServer.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        params Assembly[] modelAssemblies)
    {
        var registry = new ModelConfigurationRegistry();

        foreach (var assembly in modelAssemblies)
        {
            registry.Register(assembly);
        }

        services.AddSingleton(registry);

        var dbPath = Path.Combine(
            AppContext.BaseDirectory,
            "homeserver.db");

        services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlite(
                $"Data Source={dbPath}");
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

        return services;
    }
}