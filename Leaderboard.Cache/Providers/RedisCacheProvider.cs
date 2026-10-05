using System.Text.Json;
using Leaderboard.Cache.Interfaces;
using StackExchange.Redis;

namespace Leaderboard.Cache.Providers;

public class RedisCacheProvider(IDatabase redisDatabase) : ICacheProvider
{
    public async Task StringSetAsync<TValue>(IEnumerable<KeyValuePair<string, TValue>> keysWithValues,
        int? timeToLiveInSeconds = null, bool fireAndForget = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keysWithValues);

        var commandFlags = GetCommandFlags(fireAndForget);

        var pairs = keysWithValues.DistinctBy(x => x.Key).ToArray();
        var tasks = pairs.Select(x =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = x.Value as string ?? JsonSerializer.Serialize(x.Value);
            return redisDatabase.StringSetAsync(x.Key, value,
                timeToLiveInSeconds != null ? TimeSpan.FromSeconds((int)timeToLiveInSeconds) : null,
                flags: commandFlags);
        });

        var result = await Task.WhenAll(tasks);
        if (fireAndForget) return;

        var failedKeys = pairs.Where((_, i) => !result[i]).Select(x => x.Key).ToArray();
        if (failedKeys.Length > 0)
            throw new RedisException($"SET returned false for keys: {string.Join(", ", failedKeys)}");
    }

    public async Task<IEnumerable<T>> GetJsonParsedAsync<T>(IEnumerable<string> keys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var redisKeys = keys.Distinct().Select(x => (RedisKey)x).ToArray();
        if (redisKeys.Length == 0) return [];

        cancellationToken.ThrowIfCancellationRequested();

        var result = await redisDatabase.StringGetAsync(redisKeys);

        var jsonResult = new List<T>();

        foreach (var value in result)
        {
            if (value.IsNull) continue;

            try
            {
                var jsonValue = JsonSerializer.Deserialize<T>(value.ToString());
                if (jsonValue is not null) jsonResult.Add(jsonValue);
            }
            catch (JsonException)
            {
                // A value that cannot be deserialized is treated as missing and fetched again
            }
        }

        return jsonResult;
    }

    public Task<long> KeysDeleteAsync(IEnumerable<string> keys, bool fireAndForget = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        cancellationToken.ThrowIfCancellationRequested();

        return redisDatabase.KeyDeleteAsync(keys.Distinct().Select(x => (RedisKey)x).ToArray(),
            GetCommandFlags(fireAndForget));
    }

    public async Task<long> KeysDeleteByPatternsAsync(IEnumerable<string> patterns,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        var distinctPatterns = patterns.Distinct().ToArray();

        var servers = redisDatabase.Multiplexer.GetServers().Where(x => x is { IsConnected: true, IsReplica: false })
            .ToArray();
        if (servers.Length == 0)
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect,
                $"No connected primary server to scan keys by patterns: {string.Join(", ", distinctPatterns)}");

        var keys = new List<RedisKey>();
        foreach (var pattern in distinctPatterns)
        foreach (var server in servers)
        await foreach (var key in server.KeysAsync(redisDatabase.Database, pattern).WithCancellation(cancellationToken))
            keys.Add(key);

        return keys.Count == 0 ? 0 : await redisDatabase.KeyDeleteAsync([.. keys.Distinct()]);
    }

    private static CommandFlags GetCommandFlags(bool fireAndForget)
    {
        return fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None;
    }
}