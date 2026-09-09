using System.Reflection;

namespace HomeServer.Persistence.Configuration;

public sealed class ModelConfigurationRegistry
{
    private readonly List<Assembly> _assemblies = [];

    public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

    public void Register(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
    }
}