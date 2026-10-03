using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class ClassServiceTests
{
    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsActiveClassesSorted()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();

        //Act
        var result = await classService.GetAllAsync(null);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([6, 3, 4, 2, 1], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAllAsync_GradeFilter_ReturnsClassesOfGrade()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();

        //Act
        var result = await classService.GetAllAsync(7);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([3, 4], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;

        //Act
        var result = await classService.GetByIdAsync(classId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("7А", result.Data.DisplayName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task GetByIdAsync_NonExistentOrGraduatedId_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();

        //Act
        var result = await classService.GetByIdAsync(classId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_NewClass_ReturnsSuccess()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(8, 'А');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("8А", result.Data.DisplayName);
    }

    [Fact]
    public async Task CreateAsync_LowercaseLetter_ReturnsUppercaseLetter()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(9, 'б');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal('Б', result.Data.Letter);
    }

    [Fact]
    public async Task CreateAsync_ExistingActiveClass_ReturnsClassAlreadyExists()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(7, 'а');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_GraduatedClassName_ReturnsSuccess()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(11, 'Б');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Theory]
    [InlineData(EntityConstraints.MinGrade - 1)]
    [InlineData(EntityConstraints.MaxGrade + 1)]
    public async Task CreateAsync_InvalidGrade_ReturnsInvalidGrade(int grade)
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(grade, 'А');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_InvalidLetter_ReturnsInvalidClassLetter()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new CreateClassDto(7, '1');

        //Act
        var result = await classService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidClassLetter, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;
        var dto = new UpdateClassDto(8, 'А');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("8А", result.Data.DisplayName);
    }

    [Fact]
    public async Task UpdateAsync_SameName_ReturnsSuccess()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;
        var dto = new UpdateClassDto(7, 'а');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_NameOfAnotherClass_ReturnsClassAlreadyExists()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;
        var dto = new UpdateClassDto(7, 'Б');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // graduated
    public async Task UpdateAsync_NonExistentOrGraduatedId_ReturnsClassNotFound(long classId)
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        var dto = new UpdateClassDto(10, 'Б');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.ClassNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(EntityConstraints.MinGrade - 1)]
    [InlineData(EntityConstraints.MaxGrade + 1)]
    public async Task UpdateAsync_InvalidGrade_ReturnsInvalidGrade(int grade)
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;
        var dto = new UpdateClassDto(grade, 'А');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidGrade, EntityConstraints.MinGrade, EntityConstraints.MaxGrade),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_InvalidLetter_ReturnsInvalidClassLetter()
    {
        //Arrange
        var classService = new ClassServiceSut().GetService();
        const long classId = 3;
        var dto = new UpdateClassDto(7, '1');

        //Act
        var result = await classService.UpdateAsync(classId, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidClassLetter, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}