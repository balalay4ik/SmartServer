using System.Collections.Concurrent;
using HomeServer.Core.Interfaces;

namespace HomeServer.Core.Services;

public sealed class EventWaiter<TKey, TValue> : IEventWaiter<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<
        TKey,
        TaskCompletionSource<TValue>> _waiters = new();

    public Task<TValue?> WaitAsync(
        TKey key,
        CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<TValue>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_waiters.TryAdd(key, tcs))
        {
            throw new InvalidOperationException(
                $"Already waiting for event with key '{key}'.");
        }

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                if (_waiters.TryRemove(
                    new KeyValuePair<TKey, TaskCompletionSource<TValue>>(
                        key,
                        tcs)))
                {
                    tcs.TrySetCanceled(cancellationToken);
                }
            });
        }

        return tcs.Task!;
    }

    public bool TryComplete(
        TKey key,
        TValue value)
    {
        if (!_waiters.TryRemove(key, out var tcs))
            return false;

        return tcs.TrySetResult(value);
    }
}