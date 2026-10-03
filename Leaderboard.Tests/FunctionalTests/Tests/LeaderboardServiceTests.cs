using System.Net;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Results;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

// Read-only and anonymous: all tests share the seeded data
[FunctionalTest]
public class LeaderboardServiceTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetSchoolClasses_Default_ReturnsOkRankedByAverage()
    {
        //Act
        var response = await HttpClient.GetAsync("/api/leaderboard/classes");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ClassLeaderboardEntryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1, "7А", 10.0), (2, "7Б", 6.0), (3, "11А", 5.0), (4, "10А", 3.0)],
            result.Data.Select(x => (x.Rank, x.ClassName, x.Score)));
    }

    [Fact]
    public async Task GetGradeClasses_ExistingGrade_ReturnsOkWithClassesOfGrade()
    {
        //Arrange
        const int grade = 7;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/grades/{grade}/classes");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ClassLeaderboardEntryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(["7А", "7Б"], result.Data.Select(x => x.ClassName));
    }

    [Fact]
    public async Task GetGradeClasses_InvalidGrade_ReturnsBadRequest()
    {
        //Arrange
        const int grade = EntityConstraints.MinGrade - 1;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/grades/{grade}/classes");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<ClassLeaderboardEntryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetClassStudents_EqualScores_ReturnsOkWithEarlierReachedHigher()
    {
        //Arrange
        const long classId = 3;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/classes/{classId}/students");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<StudentLeaderboardEntryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1, 3L, 10), (2, 4L, 10)], result.Data.Select(x => (x.Rank, x.StudentId, x.Score)));
    }

    [Fact]
    public async Task GetClassStudents_GraduatedClass_ReturnsNotFound()
    {
        //Arrange
        const long classId = 5;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/classes/{classId}/students");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<StudentLeaderboardEntryDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetStudentRank_ActiveStudent_ReturnsOkWithRankAmongClassmates()
    {
        //Arrange
        const long studentId = 4;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/students/{studentId}");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentRankDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Rank);
        Assert.Equal("7А", result.Data.ClassName);
    }

    [Fact]
    public async Task GetStudentRank_InactiveStudent_ReturnsNotFound()
    {
        //Arrange
        const long studentId = 5;

        //Act
        var response = await HttpClient.GetAsync($"/api/leaderboard/students/{studentId}");
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentRankDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}