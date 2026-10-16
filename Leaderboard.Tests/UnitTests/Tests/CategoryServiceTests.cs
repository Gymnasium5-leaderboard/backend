using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class CategoryServiceTests
{
    [Fact]
    public async Task GetAllAsync_WithoutArchived_ReturnsActiveCategoriesById()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.GetAllAsync(false);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([1L, 2L, 3L, 4L, 5L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAllAsync_IncludeArchived_ReturnsAllCategories()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.GetAllAsync(true);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L], result.Data.Select(x => x.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ArchivedCategory_ReturnsSuccess()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.GetByIdAsync(6);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsArchived);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsCategoryNotFound()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.GetByIdAsync(99);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_NewName_ReturnsTrimmedCategory()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new CreateCategoryDto("  Музыка ");

        //Act
        var result = await categoryService.CreateAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Музыка", result.Data.Name);
        Assert.False(result.Data.IsArchived);
    }

    [Theory]
    [InlineData("спорт")]
    [InlineData("ОЛИМПИАДЫ")] // archived
    public async Task CreateAsync_ExistingNameIgnoringCase_ReturnsCategoryAlreadyExists(string name)
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new CreateCategoryDto(name);

        //Act
        var result = await categoryService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAsync_EmptyName_ReturnsInvalidCategoryName(string? name)
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new CreateCategoryDto(name!);

        //Act
        var result = await categoryService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidCategoryName, EntityConstraints.CategoryNameMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_TooLongName_ReturnsInvalidCategoryName()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new CreateCategoryDto(new string('a', EntityConstraints.CategoryNameMaxLength + 1));

        //Act
        var result = await categoryService.CreateAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidCategoryName, EntityConstraints.CategoryNameMaxLength),
            result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ArchivedCategory_ReturnsRenamedCategory()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new UpdateCategoryDto("Олимпиады 2025");

        //Act
        var result = await categoryService.UpdateAsync(6, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Олимпиады 2025", result.Data.Name);
        Assert.True(result.Data.IsArchived);
    }

    [Fact]
    public async Task UpdateAsync_SameNameOtherCase_ReturnsSuccess()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new UpdateCategoryDto("СПОРТ");

        //Act
        var result = await categoryService.UpdateAsync(2, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("СПОРТ", result.Data.Name);
    }

    [Fact]
    public async Task UpdateAsync_NameOfOtherCategory_ReturnsCategoryAlreadyExists()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new UpdateCategoryDto("учёба");

        //Act
        var result = await categoryService.UpdateAsync(2, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryAlreadyExists, result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsCategoryNotFound()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();
        var dto = new UpdateCategoryDto("Музыка");

        //Act
        var result = await categoryService.UpdateAsync(99, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)] // already archived
    public async Task ArchiveAsync_ExistingCategory_ReturnsArchived(long id)
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.ArchiveAsync(id);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsArchived);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(2)] // not archived
    public async Task RestoreAsync_ExistingCategory_ReturnsNotArchived(long id)
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.RestoreAsync(id);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsArchived);
    }

    [Fact]
    public async Task ArchiveAsync_NonExistingId_ReturnsCategoryNotFound()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.ArchiveAsync(99);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithoutTransactions_ReturnsSuccess()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.DeleteAsync(3);

        //Assert
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)] // archived
    public async Task DeleteAsync_CategoryWithTransactions_ReturnsCategoryInUse(long id)
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.DeleteAsync(id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryInUse, result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsCategoryNotFound()
    {
        //Arrange
        var categoryService = new CategoryServiceSut().GetService();

        //Act
        var result = await categoryService.DeleteAsync(99);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.CategoryNotFound, result.ErrorMessage);
    }
}
