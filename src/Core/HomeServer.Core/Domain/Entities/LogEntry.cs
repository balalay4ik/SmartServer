using HomeServer.Core.Enums;
using HomeServer.Persistence.Interfeaces;
using Microsoft.Extensions.Logging;

namespace HomeServer.Core.Entity;

public class LogEntry : IDbEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public LogLevel Level { get; set; }

    public LogCategory Category { get; set; }

    public string Event { get; set; } = null!;

    public string Source { get; set; } = null!;

    public string Stage { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Exception { get; set; }

    public string? StackTrace { get; set; }

    public string? Data { get; set; }

    public DateTime? ExpiresAt { get; set; }
}