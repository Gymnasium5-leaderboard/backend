using Leaderboard.Application.Resources;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class LeaderboardServiceTests
{
    [Fact]
    public async Task GetSchoolClassesAsync_Default_ReturnsClassesRankedByAverage()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetSchoolClassesAsync();

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Graduated 11Б and 5В without students are not ranked; the student who left 7А is not counted
        Assert.Equal([(1, 3L, 10.0), (2, 4L, 6.0), (3, 1L, 5.0), (4, 2L, 3.0)],
            result.Data.Select(x => (x.Rank, x.ClassId, x.Score)));
    }

    [Fact]
    public async Task GetSchoolClassesAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var leaderboardService =
            new LeaderboardServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await leaderboardService.GetSchoolClassesAsync();

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetGradeClassesAsync_ExistingGrade_ReturnsClassesOfGrade()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetGradeClassesAsync(7);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1, 3L), (2, 4L)], result.Data.Select(x => (x.Rank, x.ClassId)));
    }

    [Theory]
    [InlineData(EntityConstraints.MinGrade - 1)]
    [InlineData(EntityConstraints.MaxGrade + 1)]
    public async Task GetGradeClassesAsync_InvalidGrade_ReturnsInvalidGrade(int grade)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetGradeClassesAsync(grade);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetGradeClassesAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var leaderboardService =
            new LeaderboardServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await leaderboardService.GetGradeClassesAsync(7);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetClassStudentsAsync_EqualScores_EarlierReachedRanksHigher()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();
        const long classId = 3;

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(classId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1, 3L, 10), (2, 4L, 10)], result.Data.Select(x => (x.Rank, x.StudentId, x.Score)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task GetClassStudentsAsync_NonExistentOrGraduatedClass_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(classId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetClassStudentsAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var leaderboardService =
            new LeaderboardServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(3);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetStudentRankAsync_ActiveStudent_ReturnsRankAmongClassmates()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();
        const long studentId = 4;

        //Act
        var result = await leaderboardService.GetStudentRankAsync(studentId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Rank);
        Assert.Equal(10, result.Data.Score);
        Assert.Equal("7А", result.Data.ClassName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // left the school
    [InlineData(7)] // graduated
    public async Task GetStudentRankAsync_NonExistentOrInactiveStudent_ReturnsStudentNotFound(long studentId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetStudentRankAsync(studentId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetStudentRankAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var leaderboardService =
            new LeaderboardServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await leaderboardService.GetStudentRankAsync(4);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}