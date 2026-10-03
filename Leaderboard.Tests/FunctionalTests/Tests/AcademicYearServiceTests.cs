using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.DAL;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class AcademicYearServiceTests : SequentialFunctionalTest
{
    private const string Url = "/api/academicyear";

    public AcademicYearServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    private static string NewYearTitle => $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Year + 1}";

    [Fact]
    public async Task GetCurrent_Anonymous_ReturnsOk()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        //Act
        var response = await HttpClient.GetAsync($"{Url}/current");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AcademicYearDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(TestConstants.CurrentAcademicYearTitle, result.Data.Title);
    }

    [Fact]
    public async Task EnsureCurrent_NoYears_CreatesYearByCurrentDate()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Set<AcademicYear>().ExecuteDeleteAsync();
        var academicYearInitializer = scope.ServiceProvider.GetRequiredService<IAcademicYearInitializer>();
        var now = DateTime.UtcNow;
        var startYear = now.Month >= 9 ? now.Year : now.Year - 1;

        //Act
        await academicYearInitializer.EnsureCurrentAsync();
        var result = await HttpClient.GetFromJsonAsync<BaseResult<AcademicYearDto>>($"{Url}/current");

        //Assert
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal($"{startYear}/{startYear + 1}", result.Data.Title);
    }

    [Fact]
    public async Task EnsureCurrent_YearExists_KeepsCurrentYear()
    {
        //Arrange
        await using var scope = ServiceProvider.CreateAsyncScope();
        var academicYearInitializer = scope.ServiceProvider.GetRequiredService<IAcademicYearInitializer>();

        //Act
        await academicYearInitializer.EnsureCurrentAsync();
        var result = await HttpClient.GetFromJsonAsync<BaseResult<AcademicYearDto>>($"{Url}/current");

        //Assert
        Assert.Equal(TestConstants.CurrentAcademicYearTitle, result!.Data!.Title);
    }

    [Fact]
    public async Task StartNew_CurrentYearOpen_ReturnsOkAndPromotesClasses()
    {
        //Act
        var response = await HttpClient.PostAsync(Url, null);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AcademicYearDto>>();
        var current = await HttpClient.GetFromJsonAsync<BaseResult<AcademicYearDto>>($"{Url}/current");
        var classes = await HttpClient.GetFromJsonAsync<CollectionResult<ClassDto>>("/api/class");
        var graduateResponse = await HttpClient.GetAsync("/api/student/1");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(NewYearTitle, result.Data.Title);
        Assert.Equal(result.Data.Id, current!.Data!.Id);
        // 11А graduated, 10А became the new 11А
        Assert.Equal([(6L, "6В"), (3L, "8А"), (4L, "8Б"), (2L, "11А")],
            classes!.Data!.Select(x => (x.Id, x.DisplayName)));
        Assert.Equal(HttpStatusCode.NotFound, graduateResponse.StatusCode);
    }

    [Fact]
    public async Task StartNew_CurrentYearOpen_ReturnsOkAndResetsScores()
    {
        //Act
        var response = await HttpClient.PostAsync(Url, null);
        var history = await HttpClient.GetFromJsonAsync<PagedResult<ScoreTransactionDto>>("/api/score");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await HttpClient.GetScoreAsync(3));
        Assert.Equal(0, history!.TotalCount);
    }

    [Fact]
    public async Task StartNew_AlreadyStartedThisCalendarYear_ReturnsConflict()
    {
        //Arrange
        (await HttpClient.PostAsync(Url, null)).EnsureSuccessStatusCode();

        //Act
        var response = await HttpClient.PostAsync(Url, null);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AcademicYearDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.AcademicYearAlreadyChanged, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task StartNew_ParallelRequests_ReturnsOkOnceAndConflictForOthers()
    {
        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => HttpClient.PostAsync(Url, null)));
        var classes = await HttpClient.GetFromJsonAsync<CollectionResult<ClassDto>>("/api/class");

        //Assert
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.OK);
        Assert.Equal(4, responses.Count(x => x.StatusCode == HttpStatusCode.Conflict));
        // Promoted once, not once per request
        Assert.Equal(["6В", "8А", "8Б", "11А"], classes!.Data!.Select(x => x.DisplayName));
    }
}