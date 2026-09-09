namespace HomeServer.Core.Interfaces;

public interface IEventWaiter<TKey, TValue>
    where TKey : notnull
{
    Task<TValue?> WaitAsync(
        TKey key,
        CancellationToken cancellationToken = default);

    bool TryComplete(
        TKey key,
        TValue value);
}