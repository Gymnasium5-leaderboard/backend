using System.Net.Http.Json;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;

namespace Leaderboard.Tests.FunctionalTests.Helpers;

internal static class ScoreHelper
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    public static Task<HttpResponseMessage> PostScoreAsync(this HttpClient httpClient, string url, object dto,
        Guid? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(dto) };
        if (idempotencyKey != null) request.Headers.Add(IdempotencyKeyHeader, idempotencyKey.ToString());

        return httpClient.SendAsync(request);
    }

    public static Task<HttpResponseMessage> ChangeScoreAsync(this HttpClient httpClient, ChangeScoreDto dto,
        Guid? idempotencyKey = null)
    {
        return httpClient.PostScoreAsync("/api/score", dto, idempotencyKey);
    }

    public static Task<HttpResponseMessage> ChangeScoreBatchAsync(this HttpClient httpClient,
        ChangeScoreBatchDto dto, Guid? idempotencyKey = null)
    {
        return httpClient.PostScoreAsync("/api/score/batch", dto, idempotencyKey);
    }

    /// <summary>
    ///     Current score of an active student, read from the public leaderboard.
    /// </summary>
    public static async Task<int> GetScoreAsync(this HttpClient httpClient, long studentId)
    {
        var result = await httpClient.GetFromJsonAsync<BaseResult<StudentRankDto>>(
            $"/api/leaderboard/students/{studentId}");

        return result!.Data!.Score;
    }
}