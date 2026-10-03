using Leaderboard.Application.Resources;
using Leaderboard.Domain.Entities;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Moq;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class AcademicYearServiceTests
{
    [Fact]
    public async Task GetCurrentAsync_CurrentYearExists_ReturnsSuccess()
    {
        //Arrange
        var academicYearService = new AcademicYearServiceSut().GetService();

        //Act
        var result = await academicYearService.GetCurrentAsync();

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(TestConstants.CurrentAcademicYearTitle, result.Data.Title);
    }

    [Fact]
    public async Task GetCurrentAsync_NoYears_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var academicYearService =
            new AcademicYearServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await academicYearService.GetCurrentAsync();

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    // Only this scenario is tested because ExecuteUpdateAsync requires a real database context,
    // and the mock repository does not support it.
    [Fact]
    public async Task StartNewAsync_NoYears_ReturnsCurrentAcademicYearNotFound()
    {
        //Arrange
        var academicYearService =
            new AcademicYearServiceSut(RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object).GetService();

        //Act
        var result = await academicYearService.StartNewAsync();

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CurrentAcademicYearNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task EnsureCurrentAsync_YearExists_DoesNotCreateYear()
    {
        //Arrange
        var academicYearRepository = RepositoryMocks.GetMockAcademicYearRepository().Object;
        var academicYearInitializer = new AcademicYearServiceSut(academicYearRepository).GetInitializer();

        //Act
        await academicYearInitializer.EnsureCurrentAsync();

        //Assert
        Mock.Get(academicYearRepository).Verify(
            x => x.CreateAsync(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureCurrentAsync_NoYears_CreatesYearByCurrentDate()
    {
        //Arrange
        var academicYearRepository = RepositoryMocks.GetEmptyMockRepository<AcademicYear>().Object;
        var academicYearInitializer = new AcademicYearServiceSut(academicYearRepository).GetInitializer();
        var now = DateTime.UtcNow;
        var startYear = now.Month >= 9 ? now.Year : now.Year - 1;

        //Act
        await academicYearInitializer.EnsureCurrentAsync();

        //Assert
        Mock.Get(academicYearRepository).Verify(x => x.CreateAsync(
            It.Is<AcademicYear>(y => y.Title == $"{startYear}/{startYear + 1}"
                                     && y.StartedAt >= now
                                     && y.FinishedAt == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}