using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class OwnerServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        const long ownerId = 1;

        //Act
        var result = await ownerService.GetByIdAsync(ownerId);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("testowner1", result.Data.Login);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsOwnerNotFound()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        const long ownerId = 0;

        //Act
        var result = await ownerService.GetByIdAsync(ownerId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_ValidOwner_ReturnsSuccess()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto("newowner", TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var result = await ownerService.CreateAsync(1, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("newowner", result.Data.Login);
    }

    [Fact]
    public async Task CreateAsync_WrongCreatorPassword_ReturnsWrongCurrentPassword()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto("newowner", TestConstants.TestPassword, "New", "Owner",
            TestConstants.WrongPassword);

        //Act
        var result = await ownerService.CreateAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.WrongCurrentPassword, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_ExistingLogin_ReturnsOwnerAlreadyExists()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto("testowner2", TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var result = await ownerService.CreateAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerAlreadyExists, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("new owner")]
    [InlineData("учитель")]
    public async Task CreateAsync_InvalidLogin_ReturnsInvalidLogin(string login)
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto(login, TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var result = await ownerService.CreateAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            string.Format(ErrorMessage.InvalidLogin, EntityConstraints.LoginMinLength,
                EntityConstraints.LoginMaxLength), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_ShortPassword_ReturnsInvalidPassword()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto("newowner", "short", "New", "Owner", TestConstants.TestPassword + "1");

        //Act
        var result = await ownerService.CreateAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            string.Format(ErrorMessage.InvalidPassword, EntityConstraints.PasswordMinLength,
                EntityConstraints.PasswordMaxLength), result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_NonExistentCreator_ReturnsOwnerNotFound()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new CreateOwnerDto("newowner", TestConstants.TestPassword, "New", "Owner",
            TestConstants.TestPassword + "1");

        //Act
        var result = await ownerService.CreateAsync(0, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsSuccess()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new UpdateOwnerDto("Updated", "Owner");

        //Act
        var result = await ownerService.UpdateAsync(1, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated", result.Data.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_EmptyLastName_ReturnsInvalidLastName()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new UpdateOwnerDto("Updated", "");

        //Act
        var result = await ownerService.UpdateAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Format(ErrorMessage.InvalidLastName, EntityConstraints.NameMaxLength),
            result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ReturnsOwnerNotFound()
    {
        //Arrange
        var ownerService = new OwnerServiceSut().GetService();
        var dto = new UpdateOwnerDto("Updated", "Owner");

        //Act
        var result = await ownerService.UpdateAsync(0, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerNotFound, result.ErrorMessage);
        Assert.Null(result.Data);
    }
}