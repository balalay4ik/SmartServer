using HomeServer.Core.Entity;

namespace HomeServer.Core.Interfaces;

public interface ILoggerService
{
    Task LogAsync(LogEntry entry, CancellationToken cancellationToken = default);
}