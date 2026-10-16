using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Fixtures;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class ScoreServiceTests
{
    public static TheoryData<long[]?, long?> InvalidTargets => new()
    {
        { [1], 3 },
        { null, null },
        { [], null }
    };

    [Fact]
    public async Task ChangeScoreAsync_ActiveStudent_ReturnsNewScore()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, 1, 5, "Olympiad");

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.StudentId);
        Assert.Equal(10, result.Data.Score);
    }

    [Theory]
    [InlineData(99, nameof(ErrorMessage.CategoryNotFound))]
    [InlineData(6, nameof(ErrorMessage.CategoryArchived))]
    public async Task ChangeScoreAsync_UnknownOrArchivedCategory_ReturnsError(long categoryId, string errorName)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, categoryId, 5, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ResourceManager.GetString(errorName), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreAsync_CategoryWouldBeNegative_ReturnsScoreWouldBeNegative()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        // Student 6 has 6 in total, but only 2 in category 1
        var dto = new ChangeScoreDto(6, 1, -3, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.ScoreWouldBeNegative, 6), result.ErrorMessage);
    }

    [Fact]
    public async Task ChangeScoreAsync_OtherCategory_ReturnsScoreAndCategoryScore()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(6, 2, -3, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.CategoryId);
        // The archived category is not counted in the total
        Assert.Equal(3, result.Data.Score);
        Assert.Equal(1, result.Data.CategoryScore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(SettingsFixture.MaxScoreDelta + 1)]
    [InlineData(-SettingsFixture.MaxScoreDelta - 1)]
    public async Task ChangeScoreAsync_InvalidDelta_ReturnsInvalidScoreDelta(int delta)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, 1, delta, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidScoreDelta, SettingsFixture.MaxScoreDelta),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreAsync_TooLongDescription_ReturnsInvalidScoreDescription()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, 1, 5, new string('a', EntityConstraints.ScoreDescriptionMaxLength + 1));

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            string.Format(ErrorMessage.InvalidScoreDescription, EntityConstraints.ScoreDescriptionMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // left the school
    public async Task ChangeScoreAsync_NonExistentOrInactiveStudent_ReturnsStudentsNotFound(long studentId)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(studentId, 1, 5, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.StudentsNotFound, studentId), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreAsync_ScoreBelowZero_ReturnsScoreWouldBeNegative()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, 1, -6, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.ScoreWouldBeNegative, 1), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreAsync_ScoreBelowZeroAllowed_ReturnsNegativeScore()
    {
        //Arrange
        var scoreService = new ScoreServiceSut(allowNegativeScore: true).GetService();
        var dto = new ChangeScoreDto(1, 1, -6, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(-1, result.Data.Score);
    }

    [Fact]
    public async Task ChangeScoreAsync_UsedIdempotencyKey_ReturnsExistingTransaction()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(3, 1, 10, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, TestConstants.ExistingIdempotencyKey);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TransactionId);
        Assert.Equal(10, result.Data.Score);
    }

    [Fact]
    public async Task ChangeScoreAsync_NoCurrentYear_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var scoreService =
            new ScoreServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();
        var dto = new ChangeScoreDto(1, 1, 5, null);

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_StudentIds_ReturnsNewScores()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto([2, 1, 1], null, 1, 2, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1L, 7), (2L, 5)], result.Data.Select(x => (x.StudentId, x.Score)));
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_ArchivedCategory_ReturnsCategoryArchived()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto(null, 3, 6, 2, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryArchived, result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(SettingsFixture.MaxScoreDelta + 1)]
    [InlineData(-SettingsFixture.MaxScoreDelta - 1)]
    public async Task ChangeScoreBatchAsync_InvalidDelta_ReturnsInvalidScoreDelta(int delta)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto([2, 1, 1], null, 1, delta, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidScoreDelta, SettingsFixture.MaxScoreDelta),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_ClassId_ChangesActiveStudentsOnly()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto(null, 3, 1, 1, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3L, 4L], result.Data.Select(x => x.StudentId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task ChangeScoreBatchAsync_NonExistentOrGraduatedClass_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto(null, classId, 1, 1, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_ClassWithoutStudents_ReturnsStudentListEmpty()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto(null, 6, 1, 1, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.StudentListEmpty, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [MemberData(nameof(InvalidTargets))]
    public async Task ChangeScoreBatchAsync_NotExactlyOneTarget_ReturnsInvalidScoreTarget(long[]? studentIds,
        long? classId)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto(studentIds, classId, 1, 1, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidScoreTarget, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_OneStudentInactive_ReturnsStudentsNotFound()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto([1, 5, 7], null, 1, 1, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.StudentsNotFound, "5, 7"), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ChangeScoreBatchAsync_OneScoreBelowZero_ReturnsScoreWouldBeNegative()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto([1, 2], null, 1, -4, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.ScoreWouldBeNegative, 2), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetHistoryAsync_NoFilter_ReturnsNewestFirst()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, null, null, 1, 2);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(9, result.TotalCount);
        Assert.Equal([9L, 8L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetHistoryAsync_CategoryFilter_ReturnsTransactionsOfCategory()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, null, 2);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var transaction = Assert.Single(result.Data);
        Assert.Equal("Спорт", transaction.CategoryName);
    }

    [Fact]
    public async Task GetHistoryAsync_ArchivedCategory_IsIncluded()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(6, null, null);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(9L, true), (8L, false), (7L, false), (4L, false)],
            result.Data.Select(x => (x.Id, x.CategoryIsArchived)));
    }

    [Fact]
    public async Task GetHistoryAsync_ClassFilter_IncludesStudentsWhoLeft()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, 3, null);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3L, 2L, 1L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetHistoryAsync_StudentFilter_ReturnsStudentAndOwnerNames()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(1, null, null);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var transaction = Assert.Single(result.Data);
        Assert.Equal("Petrov", transaction.StudentLastName);
        Assert.Equal("Owner1", transaction.OwnerFirstName);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, EntityConstraints.MaxPageSize + 1)]
    public async Task GetHistoryAsync_InvalidPageSize_ReturnsInvalidPageSize(int page, int pageSize)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, null, null, page, pageSize);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidPageSize, EntityConstraints.MaxPageSize),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetHistoryAsync_InvalidPage_ReturnsInvalidPage()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, null, null, 0);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidPage, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}