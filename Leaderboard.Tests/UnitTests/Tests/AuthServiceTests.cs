using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Auth;
using Leaderboard.Domain.Settings;
using Leaderboard.Tests.Constants;
using Leaderboard.Tests.Traits;
using Leaderboard.Tests.UnitTests.Sut;
using Xunit;

namespace Leaderboard.Tests.UnitTests.Tests;

[UnitTest]
public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new LoginDto("testowner1", TestConstants.TestPassword + "1");

        //Act
        var result = await authService.LoginAsync(dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_NonExistentLogin_ReturnsInvalidCredentials()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new LoginDto("notexistingowner", TestConstants.TestPassword + "1");

        //Act
        var result = await authService.LoginAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidCredentials, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new LoginDto("testowner1", TestConstants.WrongPassword);

        //Act
        var result = await authService.LoginAsync(dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidCredentials, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    // Only this scenario is tested because ExecuteUpdateAsync requires a real database context,
    // and the mock repository does not support it.
    [Theory]
    [InlineData(TestConstants.WrongRefreshToken)]
    [InlineData(TestConstants.RevokedRefreshToken)]
    [InlineData(TestConstants.ExpiredRefreshToken)]
    public async Task RefreshAsync_UnusableToken_ReturnsInvalidRefreshToken(string refreshToken)
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();

        //Act
        var result = await authService.RefreshAsync(refreshToken);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.InvalidRefreshToken, result.ErrorMessage);
        Assert.Null(result.Data);
    }

    // Logout is not tested because it requires a real database context,
    // and the mock repository does not support it.

    // Only these scenarios are tested because ExecuteUpdateAsync requires a real database context,
    // and the mock repository does not support it.
    [Fact]
    public async Task ChangePasswordAsync_ShortNewPassword_ReturnsInvalidPassword()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new ChangePasswordDto(TestConstants.TestPassword + "1", "short");

        //Act
        var result = await authService.ChangePasswordAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            string.Format(ErrorMessage.InvalidPassword, EntityConstraints.PasswordMinLength,
                EntityConstraints.PasswordMaxLength), result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsWrongCurrentPassword()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new ChangePasswordDto(TestConstants.WrongPassword, TestConstants.NewPassword);

        //Act
        var result = await authService.ChangePasswordAsync(1, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.WrongCurrentPassword, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePasswordAsync_NonExistentOwner_ReturnsOwnerNotFound()
    {
        //Arrange
        var authService = new AuthServiceSut().GetService();
        var dto = new ChangePasswordDto(TestConstants.TestPassword + "1", TestConstants.NewPassword);

        //Act
        var result = await authService.ChangePasswordAsync(0, dto);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.OwnerNotFound, result.ErrorMessage);
    }
}