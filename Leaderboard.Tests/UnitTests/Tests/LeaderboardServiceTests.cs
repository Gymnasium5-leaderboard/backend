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
        var result = await leaderboardService.GetSchoolClassesAsync(null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Graduated 11Б and 5В without students are not ranked; the student who left 7А is not counted
        Assert.Equal([(1, 3L, 10.0), (2, 4L, 6.0), (3, 1L, 5.0), (4, 2L, 3.0)],
            result.Data.Select(x => (x.Rank, x.ClassId, x.Score)));

        // Two active students of 7А with 10 each in category 1; the archived category is not listed
        var schoolClass = result.Data.First(x => x.ClassId == 3);
        Assert.Equal([(1L, 10.0), (2L, 0.0), (3L, 0.0), (4L, 0.0), (5L, 0.0)],
            schoolClass.CategoryScores.Select(x => (x.CategoryId, x.Score)));
    }

    [Fact]
    public async Task GetSchoolClassesAsync_CategoryFilter_RanksByCategory()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetSchoolClassesAsync(2);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Only 7Б has points in category 2, the others are ordered by grade and letter
        Assert.Equal([(4L, 4.0), (3L, 0.0), (2L, 0.0), (1L, 0.0)], result.Data.Select(x => (x.ClassId, x.Score)));
    }

    [Theory]
    [InlineData(99)]
    [InlineData(6)] // archived
    public async Task GetSchoolClassesAsync_UnknownOrArchivedCategory_ReturnsCategoryNotFound(long categoryId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetSchoolClassesAsync(categoryId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetSchoolClassesAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var leaderboardService =
            new LeaderboardServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await leaderboardService.GetSchoolClassesAsync(null);

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
        var result = await leaderboardService.GetGradeClassesAsync(7, null);

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
        var result = await leaderboardService.GetGradeClassesAsync(grade, null);

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
        var result = await leaderboardService.GetGradeClassesAsync(7, null);

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
        var result = await leaderboardService.GetClassStudentsAsync(classId, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1, 3L, 10), (2, 4L, 10)], result.Data.Select(x => (x.Rank, x.StudentId, x.Score)));
    }

    [Fact]
    public async Task GetClassStudentsAsync_Default_ReturnsScoreOfEachCategory()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(4, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var student = Assert.Single(result.Data);
        // 50 in the archived category is not counted
        Assert.Equal(6, student.Score);
        Assert.Equal([
                (1L, "Учёба", 2), (2L, "Спорт", 4), (3L, "Творчество", 0),
                (4L, "Общественная деятельность", 0), (5L, "Военно-патриотическая деятельность", 0)
            ],
            student.CategoryScores.Select(x => (x.CategoryId, x.CategoryName, x.Score)));
    }

    [Fact]
    public async Task GetClassStudentsAsync_CategoryFilter_ReturnsScoreOfCategory()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(4, 2);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var student = Assert.Single(result.Data);
        Assert.Equal(4, student.Score);
        Assert.Equal(5, student.CategoryScores.Count);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(6)] // archived
    public async Task GetClassStudentsAsync_UnknownOrArchivedCategory_ReturnsCategoryNotFound(long categoryId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(3, categoryId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task GetClassStudentsAsync_NonExistentOrGraduatedClass_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetClassStudentsAsync(classId, null);

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
        var result = await leaderboardService.GetClassStudentsAsync(3, null);

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
        var result = await leaderboardService.GetStudentRankAsync(studentId, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Rank);
        Assert.Equal(10, result.Data.Score);
        Assert.Equal("7А", result.Data.ClassName);
    }
    
    [Fact]
    public async Task GetStudentRankAsync_CategoryFilter_ReturnsScoreOfCategory()
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetStudentRankAsync(3, 2);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Students 3 and 4 have nothing in category 2, so they are ordered by name: Ivanov before Smirnova
        Assert.Equal((1, 0), (result.Data.Rank, result.Data.Score));
        Assert.Equal(10, result.Data.CategoryScores.Single(x => x.CategoryId == 1).Score);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(6)] // archived
    public async Task GetStudentRankAsync_UnknownOrArchivedCategory_ReturnsCategoryNotFound(long categoryId)
    {
        //Arrange
        var leaderboardService = new LeaderboardServiceSut().GetService();

        //Act
        var result = await leaderboardService.GetStudentRankAsync(3, categoryId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
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
        var result = await leaderboardService.GetStudentRankAsync(studentId, null);

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
        var result = await leaderboardService.GetStudentRankAsync(4, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}