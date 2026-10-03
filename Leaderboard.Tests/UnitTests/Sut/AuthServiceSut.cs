using FluentValidation;
using Leaderboard.Application.Providers;
using Leaderboard.Application.Services;
using Leaderboard.Application.Validators;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Provider;
using Leaderboard.Domain.Interfaces.Repository;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Tests.Mocks;
using Leaderboard.Tests.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace Leaderboard.Tests.UnitTests.Sut;

internal class AuthServiceSut
{
    public readonly IPasswordHasher<LeaderboardOwner> PasswordHasher = new PasswordHasher<LeaderboardOwner>();

    public readonly IValidator<IValidatableOwnerPassword> PasswordValidator =
        ValidatorFixture<IValidatableOwnerPassword>.GetValidator(new PasswordValidator());

    public readonly IBaseRepository<RefreshToken> RefreshTokenRepository =
        RepositoryMocks.GetMockRefreshTokenRepository().Object;

    public readonly ITokenProvider TokenProvider = new JwtTokenProvider(SettingsFixture.GetJwtSettings());

    public readonly IUnitOfWork UnitOfWork = RepositoryMocks.GetMockUnitOfWork().Object;
    private readonly IAuthService _authService;

    public AuthServiceSut()
    {
        _authService = new AuthService(RefreshTokenRepository, UnitOfWork, TokenProvider, PasswordHasher,
            PasswordValidator, SettingsFixture.GetJwtSettings());
    }

    public IAuthService GetService()
    {
        return _authService;
    }
}