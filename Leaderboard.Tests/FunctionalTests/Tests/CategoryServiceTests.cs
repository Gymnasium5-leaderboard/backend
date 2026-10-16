using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class CategoryServiceTests : SequentialFunctionalTest
{
    private const string Url = "/api/category";

    public CategoryServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task GetAll_Anonymous_ReturnsOkWithActiveCategories()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        //Act
        var response = await HttpClient.GetAsync(Url);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<CategoryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result!.Data);
        Assert.Equal([1L, 2L, 3L, 4L, 5L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAll_IncludeArchived_ReturnsOkWithAllCategories()
    {
        //Act
        var response = await HttpClient.GetAsync($"{Url}?includeArchived=true");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<CategoryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result!.Data);
        Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task Create_NewName_ReturnsCreatedAndListsIt()
    {
        //Arrange
        await HttpClient.GetAsync(Url); // cached before the change

        //Act
        var response = await HttpClient.PostAsJsonAsync(Url, new CreateCategoryDto("Музыка"));
        var result = await response.Content.ReadFromJsonAsync<BaseResult<CategoryDto>>();
        var categories = await HttpClient.GetFromJsonAsync<CollectionResult<CategoryDto>>(Url);

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result!.Data);
        Assert.Equal(7, result.Data.Id); // identity starts after the seed and the archived category
        Assert.Contains(categories!.Data!, x => x.Name == "Музыка");
    }

    [Fact]
    public async Task Create_ExistingNameOtherCase_ReturnsConflict()
    {
        //Act
        var response = await HttpClient.PostAsJsonAsync(Url, new CreateCategoryDto("СПОРТ"));
        var result = await response.Content.ReadFromJsonAsync<BaseResult<CategoryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorMessage.CategoryAlreadyExists, result!.ErrorMessage);
    }

    [Fact]
    public async Task Create_ParallelSameName_CreatesOne()
    {
        //Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(i =>
            HttpClient.PostAsJsonAsync(Url, new CreateCategoryDto(i % 2 == 0 ? "Музыка" : "МУЗЫКА"))));

        var categories = await HttpClient.GetFromJsonAsync<CollectionResult<CategoryDto>>(Url);

        //Assert
        // A request that passed the check at the same time fails on the unique index
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(categories!.Data!, x => x.Name.Equals("музыка", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Update_NewName_ReturnsOkAndRenamesInLeaderboard()
    {
        //Arrange
        await HttpClient.GetAsync("/api/leaderboard/students/3");

        //Act
        var response = await HttpClient.PutAsJsonAsync($"{Url}/1", new UpdateCategoryDto("Учёба и наука"));
        var rank = await HttpClient.GetFromJsonAsync<BaseResult<StudentRankDto>>("/api/leaderboard/students/3");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Учёба и наука", rank!.Data!.CategoryScores.First().CategoryName);
    }

    [Fact]
    public async Task Archive_CategoryWithScores_RemovesThemFromLeaderboards()
    {
        //Arrange
        Assert.Equal(10, await HttpClient.GetScoreAsync(3)); // cached before the change

        //Act
        var response = await HttpClient.PatchAsync($"{Url}/1/archive", null);
        var score = await HttpClient.GetScoreAsync(3);
        var rank = await HttpClient.GetFromJsonAsync<BaseResult<StudentRankDto>>("/api/leaderboard/students/3");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, score);
        Assert.DoesNotContain(rank!.Data!.CategoryScores, x => x.CategoryId == 1);
    }

    [Fact]
    public async Task Restore_ArchivedCategory_CountsItsScoresAgain()
    {
        //Arrange
        Assert.Equal(6, await HttpClient.GetScoreAsync(6));

        //Act
        var response = await HttpClient.PatchAsync($"{Url}/6/restore", null);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(56, await HttpClient.GetScoreAsync(6));
    }

    [Fact]
    public async Task ChangeScore_ArchivedCategory_ReturnsConflict()
    {
        //Act
        var response = await HttpClient.ChangeScoreAsync(new ChangeScoreDto(1, 6, 1, null));
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorMessage.CategoryArchived, result!.ErrorMessage);
    }

    [Fact]
    public async Task Delete_CategoryWithTransactions_ReturnsConflict()
    {
        //Act
        var response = await HttpClient.DeleteAsync($"{Url}/1");
        var result = await response.Content.ReadFromJsonAsync<BaseResult>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorMessage.CategoryInUse, result!.ErrorMessage);
    }

    [Fact]
    public async Task Delete_CategoryWithoutTransactions_ReturnsNoContent()
    {
        //Act
        var response = await HttpClient.DeleteAsync($"{Url}/3");
        var getResponse = await HttpClient.GetAsync($"{Url}/3");

        //Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Archive_Subscribed_SendsChangedEvent()
    {
        //Arrange
        await using var events = await LeaderboardEventStream.ConnectAsync(HttpClient);

        //Act
        await HttpClient.PatchAsync($"{Url}/2/archive", null);

        //Assert
        Assert.Equal(LeaderboardEventStream.ChangedEventType, await events.ReadEventTypeAsync());
    }
}
