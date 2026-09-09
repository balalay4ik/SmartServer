using HomeServer.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HomeServer.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    private readonly ModelConfigurationRegistry _registry;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ModelConfigurationRegistry registry)
        : base(options)
    {
        _registry = registry;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var assembly in _registry.Assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        base.OnModelCreating(modelBuilder);
    }
}