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
        var dto = new ChangeScoreDto(1, 5, "Olympiad");

        //Act
        var result = await scoreService.ChangeScoreAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.StudentId);
        Assert.Equal(10, result.Data.Score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(SettingsFixture.MaxScoreDelta + 1)]
    [InlineData(-SettingsFixture.MaxScoreDelta - 1)]
    public async Task ChangeScoreAsync_InvalidDelta_ReturnsInvalidScoreDelta(int delta)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreDto(1, delta, null);

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
        var dto = new ChangeScoreDto(1, 5, new string('a', EntityConstraints.ScoreDescriptionMaxLength + 1));

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
        var dto = new ChangeScoreDto(studentId, 5, null);

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
        var dto = new ChangeScoreDto(1, -6, null);

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
        var dto = new ChangeScoreDto(1, -6, null);

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
        var dto = new ChangeScoreDto(3, 10, null);

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
        var dto = new ChangeScoreDto(1, 5, null);

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
        var dto = new ChangeScoreBatchDto([2, 1, 1], null, 2, null);

        //Act
        var result = await scoreService.ChangeScoreBatchAsync(1, dto, null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([(1L, 7), (2L, 5)], result.Data.Select(x => (x.StudentId, x.Score)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(SettingsFixture.MaxScoreDelta + 1)]
    [InlineData(-SettingsFixture.MaxScoreDelta - 1)]
    public async Task ChangeScoreBatchAsync_InvalidDelta_ReturnsInvalidScoreDelta(int delta)
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var dto = new ChangeScoreBatchDto([2, 1, 1], null, delta, null);

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
        var dto = new ChangeScoreBatchDto(null, 3, 1, null);

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
        var dto = new ChangeScoreBatchDto(null, classId, 1, null);

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
        var dto = new ChangeScoreBatchDto(null, 6, 1, null);

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
        var dto = new ChangeScoreBatchDto(studentIds, classId, 1, null);

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
        var dto = new ChangeScoreBatchDto([1, 5, 7], null, 1, null);

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
        var dto = new ChangeScoreBatchDto([1, 2], null, -4, null);

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
        var query = new ScoreHistoryQueryDto(null, null, 1, 2);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(6, result.TotalCount);
        Assert.Equal([6L, 5L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetHistoryAsync_ClassFilter_IncludesStudentsWhoLeft()
    {
        //Arrange
        var scoreService = new ScoreServiceSut().GetService();
        var query = new ScoreHistoryQueryDto(null, 3);

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
        var query = new ScoreHistoryQueryDto(1, null);

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
        var query = new ScoreHistoryQueryDto(null, null, page, pageSize);

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
        var query = new ScoreHistoryQueryDto(null, null, 0);

        //Act
        var result = await scoreService.GetHistoryAsync(query);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidPage, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}