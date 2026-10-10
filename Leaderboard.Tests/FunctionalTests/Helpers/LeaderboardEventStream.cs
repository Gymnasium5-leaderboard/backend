using System.Net.ServerSentEvents;

namespace Leaderboard.Tests.FunctionalTests.Helpers;

/// <summary>
///     A client of /api/leaderboard/events. Pings are skipped, only other events are returned.
/// </summary>
internal sealed class LeaderboardEventStream : IAsyncDisposable
{
    public const string ChangedEventType = "changed";
    private const string PingEventType = "ping";

    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(5);

    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly IAsyncEnumerator<SseItem<string>> _events;
    private readonly HttpResponseMessage _response;

    // The read that is still waiting for an event, kept for the next call instead of starting another one
    private Task<bool>? _nextEvent;

    private LeaderboardEventStream(HttpResponseMessage response, Stream stream)
    {
        _response = response;
        _events = SseParser.Create(stream).EnumerateAsync(_disposeCancellation.Token)
            .GetAsyncEnumerator(_disposeCancellation.Token);
    }

    public async ValueTask DisposeAsync()
    {
        // Closing the response ends the pending read, so the enumerator is not disposed while it runs
        await _disposeCancellation.CancelAsync();
        _response.Dispose();
        if (_nextEvent != null) await ((Task)_nextEvent).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        await _events.DisposeAsync();
        _disposeCancellation.Dispose();
    }

    /// <summary>
    ///     Opens the stream and waits for the first ping: after it no change is missed.
    /// </summary>
    public static async Task<LeaderboardEventStream> ConnectAsync(HttpClient httpClient)
    {
        var response = await httpClient.GetAsync("/api/leaderboard/events", HttpCompletionOption.ResponseHeadersRead);
        var eventStream = new LeaderboardEventStream(response, await response.Content.ReadAsStreamAsync());

        if (!await eventStream.MoveNextAsync(EventTimeout) || eventStream._events.Current.EventType != PingEventType)
            throw new InvalidOperationException("The stream did not start with a ping");

        return eventStream;
    }

    /// <summary>
    ///     Type of the next event other than a ping, or null if none came within the timeout.
    /// </summary>
    public async Task<string?> ReadEventTypeAsync(TimeSpan? timeout = null)
    {
        while (await MoveNextAsync(timeout ?? EventTimeout))
            if (_events.Current.EventType != PingEventType)
                return _events.Current.EventType;

        return null;
    }

    private async Task<bool> MoveNextAsync(TimeSpan timeout)
    {
        _nextEvent ??= _events.MoveNextAsync().AsTask();
        if (await Task.WhenAny(_nextEvent, Task.Delay(timeout)) != _nextEvent) return false;

        var hasEvent = await _nextEvent;
        _nextEvent = null;

        return hasEvent;
    }
}