using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Leaderboard.Application.Helpers;

/// <summary>
///     Runs one load at a time for the same caller and argument: concurrent callers wait for the running load instead
///     of starting their own. Registered as a singleton, so it works within one instance of the API.
/// </summary>
public sealed class SingleFlight
{
    // "{file}:{method}:{argument}" -> the running load
    private readonly ConcurrentDictionary<string, Task<object?>> _flights = new();

    /// <summary>
    ///     Runs the load, or waits for the same one if it is already running. The load is identified by the calling
    ///     file and method plus <paramref name="argument" />, so a method must call this only once.
    /// </summary>
    /// <typeparam name="T">The type of the load result.</typeparam>
    /// <param name="loadAsync">
    ///     The load. Its result goes to every waiting caller, so it must not take the token of one of them and the
    ///     result must not be changed.
    /// </param>
    /// <param name="cancellationToken">Stops waiting for this caller only, the load goes on.</param>
    /// <param name="argument">
    ///     What the load depends on besides the caller, e.g. an id or a tuple of values. Its ToString goes into the
    ///     key.
    /// </param>
    /// <param name="callerFile">Set by the compiler, not passed.</param>
    /// <param name="callerMethod">Set by the compiler, not passed.</param>
    /// <returns>The result of the load. If the load failed, every waiting caller gets its exception.</returns>
    public async Task<T> RunAsync<T>(Func<Task<T>> loadAsync, CancellationToken cancellationToken,
        object? argument = null, [CallerFilePath] string callerFile = "", [CallerMemberName] string callerMethod = "")
    {
        var key = $"{callerFile}:{callerMethod}:{argument}";

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var flight = _flights.GetOrAdd(key, completion.Task);

        if (flight != completion.Task) return (T)(await flight.WaitAsync(cancellationToken))!;

        // The caller that started the load waits for it to the end, so its scope, with the DbContext, lives until
        // the load is done
        try
        {
            var result = await loadAsync();
            completion.SetResult(result);
            return result;
        }
        catch (Exception e)
        {
            completion.SetException(e);
            throw;
        }
        finally
        {
            // The next caller loads again, so a failed load is not kept
            _flights.TryRemove(key, out _);
        }
    }
}