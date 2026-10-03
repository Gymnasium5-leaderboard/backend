using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.FunctionalTests.Base.Exception;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

// Every commit fails, so no test here changes the data and the tests share it
[FunctionalTest]
public class ExceptionTests : ExceptionBaseFunctionalTest
{
    public ExceptionTests(ExceptionFunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task ChangeScore_SimulatedDbFailure_ReturnsInternalServerError()
    {
        //Arrange
        var dto = new ChangeScoreDto(1, 5, null);

        //Act
        var response = await HttpClient.ChangeScoreAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.StartsWith(ErrorMessage.InternalServerError, result.ErrorMessage);
        Assert.Null(result.Data);
        Assert.Equal(5, await HttpClient.GetScoreAsync(1));
    }

    [Fact]
    public async Task ChangeScoreBatch_SimulatedDbFailure_ReturnsInternalServerError()
    {
        //Arrange
        var dto = new ChangeScoreBatchDto(null, 3, 5, null);

        //Act
        var response = await HttpClient.ChangeScoreBatchAsync(dto);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ScoreChangedDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.StartsWith(ErrorMessage.InternalServerError, result.ErrorMessage);
        Assert.Null(result.Data);
        Assert.Equal(10, await HttpClient.GetScoreAsync(3));
    }

    [Fact]
    public async Task ChangePassword_SimulatedDbFailure_ReturnsInternalServerError()
    {
        //Arrange
        var dto = new ChangePasswordDto(TestConstants.TestPassword + "1", TestConstants.NewPassword);

        //Act
        var response = await HttpClient.PutAsJsonAsync("/api/owner/me/password", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult>();
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login",
            new LoginDto("testowner1", TestConstants.TestPassword + "1"));

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.StartsWith(ErrorMessage.InternalServerError, result.ErrorMessage);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task StartNewAcademicYear_SimulatedDbFailure_ReturnsInternalServerError()
    {
        //Act
        var response = await HttpClient.PostAsync("/api/academicyear", null);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<AcademicYearDto>>();
        var current = await HttpClient.GetFromJsonAsync<BaseResult<AcademicYearDto>>("/api/academicyear/current");
        var classes = await HttpClient.GetFromJsonAsync<CollectionResult<ClassDto>>("/api/class");

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.StartsWith(ErrorMessage.InternalServerError, result.ErrorMessage);
        Assert.Null(result.Data);
        // Nothing was promoted or graduated
        Assert.Equal(TestConstants.CurrentAcademicYearTitle, current!.Data!.Title);
        Assert.Equal(["5В", "7А", "7Б", "10А", "11А"], classes!.Data!.Select(x => x.DisplayName));
    }
}