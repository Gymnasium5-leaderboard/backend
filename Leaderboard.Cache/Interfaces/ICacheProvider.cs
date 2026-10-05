namespace Leaderboard.Cache.Interfaces;

public interface ICacheProvider
{
    /// <summary>
    ///     Asynchronously sets multiple string key-value pairs in the cache with a specified time-to-live (TTL).
    /// </summary>
    /// <typeparam name="TValue">The type of the values to be stored. Values are serialized to JSON.</typeparam>
    /// <param name="keysWithValues">A collection of key-value pairs to store in the cache.</param>
    /// <param name="timeToLiveInSeconds">The optional time-to-live (TTL) for each key-value pair in seconds.</param>
    /// <param name="fireAndForget">If true, sends the command in fire-and-forget mode (no result or error reported).</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task StringSetAsync<TValue>(IEnumerable<KeyValuePair<string, TValue>> keysWithValues,
        int? timeToLiveInSeconds = null, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves and deserializes a collection of JSON strings from the cache into objects of type T.
    ///     Missing keys and values that cannot be deserialized are skipped.
    /// </summary>
    /// <typeparam name="T">The type into which the JSON data will be deserialized.</typeparam>
    /// <param name="keys">A collection of keys representing the JSON-serialized objects to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    Task<IEnumerable<T>> GetJsonParsedAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the keys.
    /// </summary>
    /// <param name="keys">The keys to be removed from the cache.</param>
    /// <param name="fireAndForget">If true, sends the command in fire-and-forget mode (no result or error reported).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>The number of keys that were removed from the cache.</returns>
    Task<long> KeysDeleteAsync(IEnumerable<string> keys, bool fireAndForget = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all keys matching the patterns. Keys are found with SCAN, so Redis is
    ///     not blocked.
    /// </summary>
    /// <param name="patterns">Glob-style key patterns, e.g. "class:*".</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>The number of keys that were removed from the cache.</returns>
    Task<long> KeysDeleteByPatternsAsync(IEnumerable<string> patterns, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Increments the integer value of the key by one. A missing key is created with 0 first.
    /// </summary>
    /// <param name="key">The key of the counter.</param>
    /// <returns>The value after the increment.</returns>
    Task<long> StringIncrementAsync(string key);
}