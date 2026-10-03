using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class ClassServiceTests : SequentialFunctionalTest
{
    public ClassServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task GetAll_Anonymous_ReturnsOkWithActiveClasses()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        //Act
        var response = await HttpClient.GetAsync("/api/class");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(["5В", "7А", "7Б", "10А", "11А"], result.Data.Select(x => x.DisplayName));
    }

    [Fact]
    public async Task GetById_Class_ReturnsOk()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;
        const long classId = 3;

        //Act
        var response = await HttpClient.GetAsync($"/api/class/{classId}");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(classId, result.Data.Id);
        Assert.Equal("7А", result.Data.DisplayName);
    }

    [Fact]
    public async Task GetById_GraduatedClass_ReturnsNotFound()
    {
        //Arrange
        const long classId = 5;

        //Act
        var response = await HttpClient.GetAsync($"/api/class/{classId}");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateClass_NewClass_ReturnsCreated()
    {
        //Arrange
        var dto = new CreateClassDto(8, 'а');

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/class", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("8А", result.Data.DisplayName);
    }

    [Fact]
    public async Task CreateClass_ExistingClass_ReturnsConflict()
    {
        //Arrange
        var dto = new CreateClassDto(7, 'А');

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/class", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateClass_InvalidGrade_ReturnsBadRequest()
    {
        //Arrange
        var dto = new CreateClassDto(EntityConstraints.MaxGrade + 1, 'А');

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/class", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateClass_ExistingId_ReturnsOk()
    {
        //Arrange
        const long classId = 3;
        var dto = new UpdateClassDto(8, 'А');

        //Act
        var response = await HttpClient.PutAsJsonAsync($"/api/class/{classId}", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("8А", result.Data.DisplayName);
    }

    [Fact]
    public async Task UpdateClass_NameOfAnotherClass_ReturnsConflict()
    {
        //Arrange
        const long classId = 3;
        var dto = new UpdateClassDto(7, 'Б');

        //Act
        var response = await HttpClient.PutAsJsonAsync($"/api/class/{classId}", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<ClassDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}