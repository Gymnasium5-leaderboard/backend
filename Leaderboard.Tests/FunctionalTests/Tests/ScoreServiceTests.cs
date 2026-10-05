using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class ScoreServiceTests : SequentialFunctionalTest
{
    public ScoreServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task ChangeScore_ActiveStudent_ReturnsOk()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 5, "Olympiad");

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.Score);
        Assert.Equal(10, await HttpClient.GetScoreAsync(1));
    }

    [Fact]
    public async Task ChangeScore_ScoreBelowZero_ReturnsConflict()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, -6, null);

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.ScoreWouldBeNegative, 1), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScore_InactiveStudent_ReturnsNotFound()
    {
        //Arrange
        var dto = new ChangeScoreDto(5, 1, null);

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.StudentsNotFound, 5), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScore_ParallelRequests_ReturnsOkForAll()
    {
        //Arrange
        var dto = new ChangeScoreDto(2, 1, null);

        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => HttpClient.ChangeScoreAsync(dto)));

        //Assert
        Assert.All(responses, x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        Assert.Equal(23, await HttpClient.GetScoreAsync(2));
    }

    [Fact]
    public async Task ChangeScore_ParallelWithdrawals_ReturnsOkUntilZeroThenConflict()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, -1, null);

        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => HttpClient.ChangeScoreAsync(dto)));

        //Assert
        Assert.Equal(5, responses.Count(x => x.StatusCode == HttpStatusCode.OK));
        Assert.Equal(5, responses.Count(x => x.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(0, await HttpClient.GetScoreAsync(1));
    }

    [Fact]
    public async Task ChangeScore_UsedIdempotencyKey_ReturnsOkWithExistingTransaction()
    {
        //Arrange
        var dto = new ChangeScoreDto(3, 10, null);

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto, TestConstants.ExistingIdempotencyKey);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TransactionId);
        Assert.Equal(10, await HttpClient.GetScoreAsync(3));
    }

    [Fact]
    public async Task ChangeScore_SameIdempotencyKeyInParallel_ReturnsOkAndAppliesOnce()
    {
        //Arrange
        var dto = new ChangeScoreDto(2, 10, null);
        var idempotencyKey = Guid.NewGuid();

        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => HttpClient.ChangeScoreAsync(dto, idempotencyKey)));
        var results = await Task.WhenAll(responses.Select(x =>
            x.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>()));

        //Assert
        Assert.All(responses, x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        Assert.Single(results.Select(x => x!.Data!.TransactionId).Distinct());
        Assert.Equal(13, await HttpClient.GetScoreAsync(2));
    }

    [Fact]
    public async Task ChangeScoreBatch_ClassId_ReturnsOkForActiveStudentsOnly()
    {
        //Arrange
        var dto = new ChangeScoreBatchDto(null, 3, 2, null);

        //Act
        var response = await HttpClient.ChangeScoreBatchAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(3L, 12), (4L, 12)], result.Data.Select(x => (x.StudentId, x.Score)));
    }

    [Fact]
    public async Task ChangeScore_CachedLeaderboard_ReturnsNewRanksOnNextGet()
    {
        //Arrange
        const long classId = 3;
        var dto = new ChangeScoreDto(4, 1, null);
        await HttpClient.GetAsync($"/api/leaderboard/classes/{classId}/students");
        await HttpClient.GetAsync("/api/leaderboard/classes");

        //Act
        await HttpClient.ChangeScoreAsync(dto);
        var students = await HttpClient.GetFromJsonAsync<CollectionResult<StudentLeaderboardEntryDto>>(
            $"/api/leaderboard/classes/{classId}/students");
        var classes = await HttpClient.GetFromJsonAsync<CollectionResult<ClassLeaderboardEntryDto>>(
            "/api/leaderboard/classes");

        //Assert
        Assert.Equal([(1, 4L, 11), (2, 3L, 10)], students!.Data!.Select(x => (x.Rank, x.StudentId, x.Score)));
        Assert.Contains((classId, 10.5), classes!.Data!.Select(x => (x.ClassId, x.Score)));
    }

    [Fact]
    public async Task ChangeScoreBatch_CachedLeaderboard_ReturnsNewClassScoreOnNextGet()
    {
        //Arrange
        const long classId = 4;
        var dto = new ChangeScoreBatchDto(null, classId, 2, null);
        await HttpClient.GetAsync("/api/leaderboard/grades/7/classes");

        //Act
        await HttpClient.ChangeScoreBatchAsync(dto);
        var seventhGrade = await HttpClient.GetFromJsonAsync<CollectionResult<ClassLeaderboardEntryDto>>(
            "/api/leaderboard/grades/7/classes");

        //Assert
        Assert.Contains((classId, 8.0), seventhGrade!.Data!.Select(x => (x.ClassId, x.Score)));
    }

    [Fact]
    public async Task ChangeScoreBatch_OneStudentInactive_ReturnsNotFoundAndRollsBack()
    {
        //Arrange
        var dto = new ChangeScoreBatchDto([1, 5], null, 2, null);

        //Act
        var response = await HttpClient.ChangeScoreBatchAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.StudentsNotFound, 5), result.ErrorMessage);
        Assert.Equal(5, await HttpClient.GetScoreAsync(1));
    }

    [Fact]
    public async Task ChangeScoreBatch_OneScoreBelowZero_ReturnsConflictAndRollsBack()
    {
        //Arrange
        var dto = new ChangeScoreBatchDto([1, 2], null, -4, null);

        //Act
        var response = await HttpClient.ChangeScoreBatchAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.ScoreWouldBeNegative, 2), result.ErrorMessage);
        Assert.Equal(5, await HttpClient.GetScoreAsync(1));
        Assert.Equal(3, await HttpClient.GetScoreAsync(2));
    }

    [Fact]
    public async Task ChangeScoreBatch_RepeatedIdempotencyKey_ReturnsOkWithSameTransactions()
    {
        //Arrange
        var dto = new ChangeScoreBatchDto([1, 2], null, 4, null);
        var idempotencyKey = Guid.NewGuid();

        //Act
        var firstResponse = await HttpClient.ChangeScoreBatchAsync(dto, idempotencyKey);
        var secondResponse = await HttpClient.ChangeScoreBatchAsync(dto, idempotencyKey);
        var first = await firstResponse.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();
        var second = await secondResponse.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(first!.Data!.Select(x => x.TransactionId), second!.Data!.Select(x => x.TransactionId));
        Assert.Equal(9, await HttpClient.GetScoreAsync(1));
    }

    [Fact]
    public async Task GetHistory_StudentFilter_ReturnsOkNewestFirst()
    {
        //Arrange
        (await HttpClient.ChangeScoreAsync(new ChangeScoreDto(1, 2, null))).EnsureSuccessStatusCode();

        //Act
        var response = await HttpClient.GetAsync("/api/score?studentId=1&page=1&pageSize=1");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ScoreTransactionDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, Assert.Single(result.Data).Delta);
    }
}