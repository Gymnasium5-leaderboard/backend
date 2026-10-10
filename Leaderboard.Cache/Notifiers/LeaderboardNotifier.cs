using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Leaderboard.Cache.Extensions;
using Leaderboard.Cache.Helpers;
using Leaderboard.Domain.Interfaces.Notifier;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace Leaderboard.Cache.Notifiers;

/// <summary>
///     A change goes through Redis pub/sub, so subscribers of every instance get it. Each instance subscribes to Redis
///     once on startup and passes the change to its own subscribers.
/// </summary>
public class LeaderboardNotifier(IConnectionMultiplexer multiplexer, ILogger logger)
    : ILeaderboardNotifier, IHostedService
{
    private static readonly RedisChannel RedisChannel =
        RedisChannel.Literal(CacheKeyHelper.GetLeaderboardChangedChannel());

    // Subscribers of this instance, each keeps only the latest change it has not read yet
    // ConcurrentDictionary has dummy values
    private readonly ConcurrentDictionary<Channel<DateTime>, byte> _subscribers = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await multiplexer.GetSubscriber().SubscribeAsync(RedisChannel,
                (_, message) => Deliver(new DateTime((long)message, DateTimeKind.Utc)));
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The subscription stays registered and is made when Redis connects
            logger.LogRedisFailure(e, [RedisChannel.ToString()]);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task NotifyChangedAsync()
    {
        var changedAt = DateTime.UtcNow;

        try
        {
            await multiplexer.GetSubscriber().PublishAsync(RedisChannel, changedAt.Ticks);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // Subscribers of other instances miss the change, subscribers of this one still get it
            logger.LogRedisFailure(e, [RedisChannel.ToString()]);
            Deliver(changedAt);
        }
    }

    public async IAsyncEnumerable<DateTime> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(1)
            { FullMode = BoundedChannelFullMode.DropOldest });
        _subscribers.TryAdd(channel, default);

        try
        {
            // The subscriber is removed after the token is canceled, so we yield return until then
            await foreach (var changedAt in channel.Reader.ReadAllAsync(cancellationToken)) yield return changedAt;
        }
        finally
        {
            _subscribers.TryRemove(channel, out _);
        }
    }

    private void Deliver(DateTime changedAt)
    {
        foreach (var subscriber in _subscribers.Keys) subscriber.Writer.TryWrite(changedAt);
    }
}