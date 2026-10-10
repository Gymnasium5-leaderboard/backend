using System.Net.Mime;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Interfaces.Notifier;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Public leaderboards of the current academic year. Anyone can read them.
/// </summary>
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class LeaderboardController(
    ILeaderboardService leaderboardService,
    ILeaderboardNotifier leaderboardNotifier,
    IHostApplicationLifetime lifetime) : BaseController
{
    private const string ChangedEventType = "changed";

    // Keeps proxies from closing an idle stream. EventSource does not dispatch an event with empty data
    private static readonly SseItem<string> Ping = new(string.Empty, "ping");
    private static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     Gets all classes of the school by average score.
    /// </summary>
    [HttpGet("classes")]
    public async Task<ActionResult<CollectionResult<ClassLeaderboardEntryDto>>> GetSchoolClassesAsync(
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetSchoolClassesAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets classes of one grade by average score.
    /// </summary>
    [HttpGet("grades/{grade:int}/classes")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CollectionResult<ClassLeaderboardEntryDto>>> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetGradeClassesAsync(grade, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets students of one class by score.
    /// </summary>
    [HttpGet("classes/{classId:long}/students")]
    public async Task<ActionResult<CollectionResult<StudentLeaderboardEntryDto>>> GetClassStudentsAsync(
        long classId, CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetClassStudentsAsync(classId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets rank and score of a student among the classmates.
    /// </summary>
    [HttpGet("students/{studentId:long}")]
    public async Task<ActionResult<BaseResult<StudentRankDto>>> GetStudentRankAsync(long studentId,
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetStudentRankAsync(studentId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Server-sent events: "changed" after any change of the leaderboards, with the time of the change as data.
    ///     The client loads the leaderboards it shows again, and also after a reconnect, as events are not resent.
    ///     Several changes in a row can come as one event.
    /// </summary>
    [HttpGet("events")]
    [Produces(MediaTypeNames.Text.EventStream)]
    public IResult GetEvents(CancellationToken cancellationToken)
    {
        return TypedResults.ServerSentEvents(GetEventsAsync(cancellationToken));
    }

    private async IAsyncEnumerable<SseItem<string>> GetEventsAsync(
        [EnumeratorCancellation] CancellationToken requestAborted)
    {
        // The stream ends on shutdown too, otherwise the server waits for it until the shutdown timeout
        using var streamCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(requestAborted, lifetime.ApplicationStopping);
        var cancellationToken = streamCancellation.Token;

        await using var changes = leaderboardNotifier.SubscribeAsync(cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        var nextChange = changes.MoveNextAsync().AsTask();

        // The subscription is already made, so the client knows from the first ping that no change is missed
        yield return Ping;

        while (true)
        {
            if (await Task.WhenAny(nextChange, Task.Delay(PingInterval, cancellationToken)) != nextChange &&
                !cancellationToken.IsCancellationRequested)
            {
                yield return Ping;
                continue;
            }

            // A change came or the stream ends. The subscription takes the same token, so it ends as well and the
            // enumerator can be disposed
            await ((Task)nextChange).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            if (!nextChange.IsCompletedSuccessfully || !nextChange.Result) yield break;

            yield return new SseItem<string>(changes.Current.ToString("O"), ChangedEventType);

            nextChange = changes.MoveNextAsync().AsTask();
        }
    }
}