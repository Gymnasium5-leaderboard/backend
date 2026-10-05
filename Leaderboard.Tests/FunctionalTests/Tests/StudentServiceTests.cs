using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Dtos.Student;
using Leaderboard.Domain.Results;
using Leaderboard.Tests.FunctionalTests.Base;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Leaderboard.Tests.Traits;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Tests;

[FunctionalTest]
public class StudentServiceTests : SequentialFunctionalTest
{
    public StudentServiceTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenHelper.GetToken());
    }

    [Fact]
    public async Task GetAll_ClassFilter_ReturnsOkWithActiveStudentsOfClass()
    {
        //Arrange
        const long classId = 3;

        //Act
        var response = await HttpClient.GetAsync($"/api/student?classId={classId}");
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3L, 4L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task CreateStudent_ActiveClass_ReturnsCreated()
    {
        //Arrange
        var dto = new CreateStudentDto("Nikita", "Sokolov", 3);

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/student", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("7А", result.Data.ClassName);
    }

    [Fact]
    public async Task CreateStudent_GraduatedClass_ReturnsNotFound()
    {
        //Arrange
        var dto = new CreateStudentDto("Nikita", "Sokolov", 5);

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/student", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateStudents_ValidList_ReturnsCreated()
    {
        //Arrange
        CreateStudentDto[] dtos = [new("Nikita", "Sokolov", 3), new("Olga", "Morozova", 4)];

        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/student/batch", dtos);
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(["7А", "7Б"], result.Data.Select(x => x.ClassName));
    }

    [Fact]
    public async Task CreateStudents_EmptyList_ReturnsBadRequest()
    {
        //Act
        var response = await HttpClient.PostAsJsonAsync("/api/student/batch", Array.Empty<CreateStudentDto>());
        var result = await response.Content.ReadFromJsonAsync<CollectionResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.StudentListEmpty, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateStudent_ExistingId_ReturnsOk()
    {
        //Arrange
        const long studentId = 1;
        var dto = new UpdateStudentDto("Ivan", "Petrovsky");

        //Act
        var response = await HttpClient.PutAsJsonAsync($"/api/student/{studentId}", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Petrovsky", result.Data.LastName);
    }

    [Fact]
    public async Task TransferStudent_ActiveClass_ReturnsOkAndKeepsScore()
    {
        //Arrange
        const long studentId = 3;
        var dto = new TransferStudentDto(4);

        //Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/student/{studentId}/transfer", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("7Б", result.Data.ClassName);
        Assert.Equal(10, await HttpClient.GetScoreAsync(studentId));
    }

    [Fact]
    public async Task TransferStudent_CachedLeaderboard_ReturnsStudentInNewClassOnNextGet()
    {
        //Arrange
        const long studentId = 3;
        const long classId = 4;
        var dto = new TransferStudentDto(classId);
        await HttpClient.GetAsync($"/api/leaderboard/classes/{classId}/students");
        await HttpClient.GetAsync($"/api/leaderboard/students/{studentId}");

        //Act
        await HttpClient.PatchAsJsonAsync($"/api/student/{studentId}/transfer", dto);
        var students = await HttpClient.GetFromJsonAsync<CollectionResult<StudentLeaderboardEntryDto>>(
            $"/api/leaderboard/classes/{classId}/students");
        var rank = await HttpClient.GetFromJsonAsync<BaseResult<StudentRankDto>>(
            $"/api/leaderboard/students/{studentId}");

        //Assert
        Assert.Equal([(1, studentId, 10), (2, 6L, 6)], students!.Data!.Select(x => (x.Rank, x.StudentId, x.Score)));
        Assert.Equal("7Б", rank!.Data!.ClassName);
    }

    [Fact]
    public async Task TransferStudent_GraduatedClass_ReturnsNotFound()
    {
        //Arrange
        const long studentId = 3;
        var dto = new TransferStudentDto(5);

        //Act
        var response = await HttpClient.PatchAsJsonAsync($"/api/student/{studentId}/transfer", dto);
        var result = await response.Content.ReadFromJsonAsync<BaseResult<StudentDto>>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task DeactivateStudent_ActiveStudent_ReturnsNoContent()
    {
        //Arrange
        const long studentId = 1;

        //Act
        var response = await HttpClient.PatchAsync($"/api/student/{studentId}/deactivate", null);
        var getResponse = await HttpClient.GetAsync($"/api/student/{studentId}");

        //Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateStudent_InactiveStudent_ReturnsNotFound()
    {
        //Arrange
        const long studentId = 5;

        //Act
        var response = await HttpClient.PatchAsync($"/api/student/{studentId}/deactivate", null);
        var result = await response.Content.ReadFromJsonAsync<BaseResult>();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(result!.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
    }
}