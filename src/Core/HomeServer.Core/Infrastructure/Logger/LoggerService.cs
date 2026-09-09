using HomeServer.Core.Entity;
using HomeServer.Core.Interfaces;
using HomeServer.Persistence.Interfeaces;
using Microsoft.Extensions.Logging;

namespace HomeServer.Core.Services;

public sealed class LoggerService : ILoggerService
{
    private readonly IRepositoryFactory _repositoryFactory;

    public LoggerService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        entry.CreatedAt = DateTime.UtcNow;

        await _repositoryFactory
            .Table<LogEntry>()
            .AddAsync(entry, cancellationToken);
    }
}