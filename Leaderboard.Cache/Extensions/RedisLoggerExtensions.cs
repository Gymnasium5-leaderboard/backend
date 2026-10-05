using System.Runtime.CompilerServices;
using Serilog;
using StackExchange.Redis;

namespace Leaderboard.Cache.Extensions;

public static class RedisLoggerExtensions
{
    private const string FailureMessageTemplate = "Redis operation {Operation} failed for keys {Keys}";

    /// <summary>
    ///     True for errors of Redis itself. RedisTimeoutException derives from TimeoutException, not from
    ///     RedisException, so it is checked separately.
    /// </summary>
    public static bool IsRedisFailure(this Exception exception)
    {
        return exception is RedisException or RedisTimeoutException;
    }

    /// <summary>
    ///     Logs a failed cache operation with its keys. A lost connection is already logged once by the connection
    ///     events, so here it is only a short debug line without the stack trace.
    /// </summary>
    public static void LogRedisFailure(this ILogger logger, Exception exception, IEnumerable<string> keys,
        [CallerMemberName] string operation = "")
    {
        if (exception is RedisConnectionException)
            logger.Debug(FailureMessageTemplate + ": {Reason}", operation, keys, exception.Message);
        else
            logger.Warning(exception, FailureMessageTemplate, operation, keys);
    }
}