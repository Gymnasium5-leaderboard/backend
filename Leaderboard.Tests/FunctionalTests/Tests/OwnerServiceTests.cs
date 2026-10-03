using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class OwnerServiceTests : SequentialFunctionalTest
{
    public OwnerServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task GetMe_ExistingOwner_ReturnsOk()
    {
        //Act
        var response = await HttpClient.GetAsync("/api/owner/me");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("testowner1", result.Data.Login);
    }

    [Fact]
    public async Task GetMe_DeletedOwnerToken_ReturnsNotFound()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken(0));

        //Act
        var response = await HttpClient.GetAsync("/api/owner/me");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ErrorMessage.OwnerNotFound, result!.ErrorMessage);
    }

    [Fact]
    public async Task CreateOwner_NewLogin_ReturnsCreated()
    {
        //Arrange
        var dto = new CreateOwnerDto("newowner", TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/owner", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Id);
    }

    [Fact]
    public async Task CreateOwner_ExistingLogin_ReturnsConflict()
    {
        //Arrange
        var dto = new CreateOwnerDto("testowner2", TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/owner", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateOwner_WrongCreatorPassword_ReturnsUnauthorized()
    {
        //Arrange
        var dto = new CreateOwnerDto("newowner", TestConstants.TestPassword, "New", "Owner",
            TestConstants.WrongPassword);

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/owner", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.WrongCurrentPassword, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateMe_ValidNames_ReturnsOk()
    {
        //Arrange
        var dto = new UpdateOwnerDto("Updated", "Owner");

        //Act
        var response = await HttpClient.PutAsJsonAsync("/api/owner/me", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated", result.Data.FirstName);
    }

    [Fact]
    public async Task UpdateMe_EmptyFirstName_ReturnsBadRequest()
    {
        //Arrange
        var dto = new UpdateOwnerDto("", "Owner");

        //Act
        var response = await HttpClient.PutAsJsonAsync("/api/owner/me", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<OwnerDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidFirstName, EntityConstraints.NameMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }
}